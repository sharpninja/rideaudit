// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using RideAudit.Bt;
using RideAudit.Capture;
using RideAudit.Client.Contracts;
using RideAudit.Client.Core;
using RideAudit.Client.Seal;
using RideAudit.Contracts;
using RideAudit.PlayIntegrity;
using RideAudit.Protos.Admission.V1;
using RideAudit.Shared.Ui.Views;
using RideAudit.Video;

namespace RideAudit.Shared.Ui;

public sealed class CaptureSessionIdentity
{
    public string TenantId { get; init; } = "";
    public string DriverId { get; init; } = "";
    public string VehicleId { get; init; } = "";
    public string SessionId { get; init; } = "";
    public string CollectorId { get; init; } = "";
    public string PolicyVersion { get; init; } = RideAuditPolicy.Version;

    public static CaptureSessionIdentity FromEnvironment() =>
        new()
        {
            TenantId = Environment.GetEnvironmentVariable("RIDEAUDIT_TENANT_ID") ?? "",
            DriverId = Environment.GetEnvironmentVariable("RIDEAUDIT_DRIVER_ID") ?? "",
            VehicleId = Environment.GetEnvironmentVariable("RIDEAUDIT_VEHICLE_ID") ?? "",
            SessionId = Environment.GetEnvironmentVariable("RIDEAUDIT_SESSION_ID") ?? "",
            CollectorId = Environment.GetEnvironmentVariable("RIDEAUDIT_COLLECTOR_ID")
                ?? Environment.GetEnvironmentVariable("RIDEAUDIT_DRIVER_ID")
                ?? "",
            PolicyVersion = Environment.GetEnvironmentVariable("RIDEAUDIT_POLICY_VERSION") ?? RideAuditPolicy.Version
        };
}

public sealed class CapturePathResult
{
    public required bool Ok { get; init; }
    public required string Phase { get; init; }
    public required IReadOnlyList<string> Attempted { get; init; }
    public string? Code { get; init; }
    public string? Message { get; init; }
    public SubmitSealedRequest? Request { get; init; }
    public AdmissionDecision? Decision { get; init; }

    public string Display =>
        (Code is null ? "" : Code + ": ") + (Message ?? (Ok ? "ok" : "fail-closed"));
}

/// <summary>
/// Production capture entry. UI start/stop invoke camera, Play, seal, escrow,
/// and authenticated admission. Missing hardware or Play is Unavailable*,
/// never a silent UnavailableDiscoveryBus success.
/// </summary>
public sealed class CaptureRuntime
{
    private readonly List<string> _attempted = new();
    private SourceStream? _captured;
    private PlayAuthorization? _authorization;
    private string? _role;

    public required ProductionCaptureGraph Graph { get; init; }

    public RideAudit.Client.Core.IClock Clock { get; init; } = new RideAudit.Client.Core.SystemClock();

    public PackageAllowlist Allowlist { get; init; } = PackageAllowlist.CreateDevelopmentDefault();

    public EscrowPublicKey? EscrowPublicKey { get; init; }

    public CaptureSessionIdentity Identity { get; init; } = CaptureSessionIdentity.FromEnvironment();

    public CapturePathResult? LastResult { get; private set; }

    public IReadOnlyList<string> Attempted => _attempted;

    public CaptureShellView CreateShell()
    {
        var view = new CaptureShellView(this);
        if (!Graph.ProductionReady)
        {
            view.ShowUnavailableBanner(
                "PRODUCTION_UNAVAILABLE: " + string.Join(" | ", Graph.UnavailableSeams));
        }

        return view;
    }

    public CapturePathResult Start(string role)
    {
        _attempted.Clear();
        _captured = null;
        _authorization = null;
        _role = role;
        var failures = new List<string>();

        if (!string.Equals(role, "driver", StringComparison.Ordinal))
        {
            return Remember(Fail("start", ErrorCodes.AuthForbidden, "FR-RIDE-035", "Only the driver phone may start the session."));
        }

        try
        {
            _attempted.Add("camera");
            _captured = Graph.Camera.Capture(new CameraCaptureRequest(
                "driver-stream",
                "device-driver",
                "attest-ref",
                "rear"));
        }
        catch (RideAuditFailClosedException ex)
        {
            failures.Add(ex.Code + ": " + ex.Message);
        }

        try
        {
            _attempted.Add("play");
            var gate = new PlayIntegrityGate(Graph.Play, Allowlist, Clock);
            var collector = string.IsNullOrWhiteSpace(Identity.CollectorId) ? "unconfigured-collector" : Identity.CollectorId;
            _authorization = gate.AuthorizeKeyGeneration(AttestationRequest.Create(collector));
            if (!_authorization.Accepted || _authorization.Evidence is null)
            {
                failures.Add(ErrorCodes.AttestationFailed + ": " + (_authorization.Detail ?? "Play Integrity failed closed."));
            }
        }
        catch (RideAuditFailClosedException ex)
        {
            failures.Add(ex.Code + ": " + ex.Message);
        }

        if (failures.Count > 0)
        {
            _captured = null;
            _authorization = null;
            return Remember(new CapturePathResult
            {
                Ok = false,
                Phase = "start",
                Attempted = _attempted.ToArray(),
                Code = ExtractCode(failures[0]),
                Message = string.Join(" | ", failures)
            });
        }

        return Remember(new CapturePathResult
        {
            Ok = true,
            Phase = "start",
            Attempted = _attempted.ToArray(),
            Message = "Camera and Play gate armed. Seal waits for stop."
        });
    }

    public CapturePathResult StopAndSubmit()
    {
        if (!string.Equals(_role, "driver", StringComparison.Ordinal))
        {
            return Remember(Fail("stop", ErrorCodes.AuthForbidden, "FR-RIDE-035", "Only the driver phone may stop the session."));
        }

        if (_captured is null || _authorization is null || !_authorization.Accepted || _authorization.Evidence is null)
        {
            return Remember(Fail("stop", ErrorCodes.ValidationFailed, "FR-RIDE-015", "Start did not arm camera and Play. Seal is refused."));
        }

        try
        {
            _attempted.Add("escrow-check");
            if (Graph.EscrowDeposit is null or UnavailableDeviceEscrowDeposit || EscrowPublicKey is null)
            {
                throw new RideAuditFailClosedException(
                    ErrorCodes.EscrowUnavailable,
                    "FR-RIDE-033",
                    "Hardware HSM escrow is not configured. In-process fixtures are test-only.");
            }

            _attempted.Add("admission-check");
            if (Graph.Admission is null)
            {
                throw new RideAuditFailClosedException(
                    ErrorCodes.AdmissionUnavailable,
                    "FR-RIDE-035",
                    "Authenticated admission is not configured. RIDEAUDIT_ADMISSION_ADDRESS/BEARER are required.");
            }

            if (string.IsNullOrWhiteSpace(Identity.TenantId)
                || string.IsNullOrWhiteSpace(Identity.DriverId)
                || string.IsNullOrWhiteSpace(Identity.VehicleId)
                || string.IsNullOrWhiteSpace(Identity.SessionId))
            {
                throw new RideAuditFailClosedException(
                    ErrorCodes.AuthRequired,
                    "FR-RIDE-035",
                    "Canonical admission requires tenant, driver, vehicle, and session identity.");
            }

            _attempted.Add("seal");
            var sealer = new CollectionSealer(Clock);
            var sealedRecord = sealer.Seal(new SealRequest
            {
                Plaintext = _captured.Payload.ToArray(),
                SessionId = Identity.SessionId,
                RecordId = "capture-" + Identity.SessionId,
                Scope = KeyScope.Session,
                ScopeId = Identity.SessionId,
                Authorization = _authorization,
                EscrowKey = EscrowPublicKey,
                CollectorIdentity = string.IsNullOrWhiteSpace(Identity.CollectorId) ? Identity.DriverId : Identity.CollectorId,
                ProvenanceTag = RideAuditPolicy.ProvenanceTag,
                Kind = EvidenceKind.SensorSample,
                DeviceIds = [_captured.DeviceId],
                SourceCommitNotice = "capture-runtime"
            });
            sealer.DiscardSessionKeys();

            _attempted.Add("escrow");
            Graph.EscrowDeposit.Deposit(sealedRecord);

            _attempted.Add("request");
            var request = Graph.Requests.Create(sealedRecord, FactoryRequest(), _authorization.Evidence);

            _attempted.Add("admission");
            var decision = Graph.Admission.SubmitSealed(request);
            if (!string.IsNullOrEmpty(decision.RejectCode))
            {
                throw new RideAuditFailClosedException(decision.RejectCode, "FR-RIDE-035", decision.Message);
            }

            return Remember(new CapturePathResult
            {
                Ok = true,
                Phase = "stop",
                Attempted = _attempted.ToArray(),
                Request = request,
                Decision = decision,
                Message = decision.Admitted
                    ? "Sealed submission admitted."
                    : decision.Message
            });
        }
        catch (RideAuditFailClosedException ex)
        {
            return Remember(new CapturePathResult
            {
                Ok = false,
                Phase = "stop",
                Attempted = _attempted.ToArray(),
                Code = ex.Code,
                Message = ex.Message
            });
        }
    }

    private CaptureRequest FactoryRequest()
    {
        var driver = new PhoneNode("00:00:00:00:00:01", "driver-phone", PhoneRole.Driver);
        var passenger = new PhoneNode("00:00:00:00:00:02", "passenger-phone", PhoneRole.Passenger);
        driver.Confirm(PhoneRole.Driver, Identity.SessionId);
        passenger.Confirm(PhoneRole.Passenger, Identity.SessionId);
        return new CaptureRequest
        {
            Driver = driver,
            Passenger = passenger,
            SessionId = Identity.SessionId,
            VehicleId = Identity.VehicleId,
            CollectorIdentity = string.IsNullOrWhiteSpace(Identity.CollectorId) ? Identity.DriverId : Identity.CollectorId,
            DriverPlay = Graph.Play,
            PassengerPlay = Graph.Play,
            Allowlist = Allowlist,
            Clock = Clock,
            Probe = new ExplicitDeviceProbe(12, 80_000_000, 77, 36),
            Escrow = EscrowPublicKey!,
            DriverStream = _captured!,
            PassengerStream = _captured!,
            Samples = [],
            SealRaw = false,
            RawConsent = false,
            TenantId = Identity.TenantId,
            DriverId = Identity.DriverId,
            PolicyVersion = Identity.PolicyVersion,
            RequestFactory = Graph.Requests,
            EscrowDeposit = Graph.EscrowDeposit
        };
    }

    private CapturePathResult Fail(string phase, string code, string requirement, string message)
    {
        _ = requirement;
        return new CapturePathResult
        {
            Ok = false,
            Phase = phase,
            Attempted = _attempted.ToArray(),
            Code = code,
            Message = message
        };
    }

    private CapturePathResult Remember(CapturePathResult result)
    {
        LastResult = result;
        return result;
    }

    private static string ExtractCode(string failure)
    {
        var colon = failure.IndexOf(':');
        return colon <= 0 ? ErrorCodes.ValidationFailed : failure[..colon];
    }
}
