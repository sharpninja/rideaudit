// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using Google.Protobuf;
using RideAudit.Bt;
using RideAudit.Client.Contracts;
using RideAudit.Client.Core;
using RideAudit.Client.Seal;
using RideAudit.PlayIntegrity;
using RideAudit.Protos.Admission.V1;
using RideAudit.Protos.Custody.V1;
using RideAudit.Video;

namespace RideAudit.Capture;

public sealed class CaptureRequest
{
    public required PhoneNode Driver { get; init; }
    public required PhoneNode Passenger { get; init; }
    public required string SessionId { get; init; }
    public required string VehicleId { get; init; }
    public required string CollectorIdentity { get; init; }
    public required IPlayIntegrityClient DriverPlay { get; init; }
    public required IPlayIntegrityClient PassengerPlay { get; init; }
    public required PackageAllowlist Allowlist { get; init; }
    public required IClock Clock { get; init; }
    public required IDeviceProbe Probe { get; init; }
    public required EscrowPublicKey Escrow { get; init; }
    public required SourceStream DriverStream { get; init; }
    public required SourceStream PassengerStream { get; init; }
    public required IReadOnlyList<TelematicsSample> Samples { get; init; }
    public required bool SealRaw { get; init; }
    public required bool RawConsent { get; init; }
    public byte[]? RawPayload { get; init; }
    public KeyScope Scope { get; init; } = KeyScope.Session;
    public VideoQuotaPolicy Quotas { get; init; } = VideoQuotaPolicy.Default;
    public PerformanceThresholds Thresholds { get; init; } = new();
    public string SourceCommitNotice { get; init; } = "workspace";
    public TimeSpan FrameInterval { get; init; } = TimeSpan.FromMilliseconds(33);
    public IAdmissionRequestFactory? RequestFactory { get; init; }
    public IDeviceEscrowDeposit? EscrowDeposit { get; init; }
}

public sealed class CaptureResult
{
    public required PairedSession Pairing { get; init; }
    public required SyncClockOffset Sync { get; init; }
    public required CompositePackage Composite { get; init; }
    public required SealedRecord SealedComposite { get; init; }
    public SealedRecord? SealedRaw { get; init; }
    public required PreparedSubmission Submission { get; init; }
    public required PerformanceDecision Performance { get; init; }
    public required IReadOnlyList<CoordinationCommand> Commands { get; init; }
    public bool PlaintextQueued { get; init; }
}

public sealed class PreparedSubmission
{
    public required SubmitSealedRequest Request { get; init; }
    public required AdmissionDecision Response { get; init; }
}

public sealed class DualPhoneCaptureSession
{
    private readonly IDiscoveryBus _bus;
    private readonly ISealedAdmissionClient _admission;
    private readonly CollectionSealer _sealer;
    private readonly DeviceBoundaryStore _store;
    private readonly IAdmissionRequestFactory _defaultRequests;

    public DualPhoneCaptureSession(
        IDiscoveryBus bus,
        ISealedAdmissionClient admission,
        IClock clock,
        IAdmissionRequestFactory? requests = null)
    {
        _bus = bus;
        _admission = admission;
        _sealer = new CollectionSealer(clock);
        _store = new DeviceBoundaryStore();
        _defaultRequests = requests ?? PreflightAdmissionRequestFactory.Instance;
    }

    public DeviceBoundaryStore Store => _store;

    public SessionCoordinator Coordinator { get; } = new();

    public CaptureResult Run(CaptureRequest request)
    {
        var pairingService = new BluetoothPairingService(_bus);
        _bus.Advertise(new Advertisement
        {
            Address = request.Passenger.Address,
            DisplayName = request.Passenger.DisplayName,
            Service = ApiBoundary.RideAuditBluetooth,
        });
        var pairing = pairingService.Pair(request.Driver, request.Passenger);
        if (!pairing.Ok || pairing.Session is null)
        {
            throw new RideAuditFailClosedException(
                pairing.Code ?? "BT_DISCOVERY_FAILED",
                "FR-RIDE-053",
                pairing.Detail ?? "Pairing failed.");
        }

        var driverGate = new PlayIntegrityGate(request.DriverPlay, request.Allowlist, request.Clock);
        var passengerGate = new PlayIntegrityGate(request.PassengerPlay, request.Allowlist, request.Clock);
        var driverAuth = driverGate.AuthorizeKeyGeneration(AttestationRequest.Create(request.CollectorIdentity));
        var passengerAuth = passengerGate.AuthorizeKeyGeneration(AttestationRequest.Create(request.Passenger.DisplayName));
        if (!driverAuth.Accepted || !passengerAuth.Accepted)
        {
            throw new RideAuditFailClosedException(
                "ATTESTATION_FAILED",
                "FR-RIDE-026",
                "Play Integrity failed closed before key generation.");
        }

        Coordinator.Start(pairing.Session, driverAuth, request.Clock, request.SessionId);
        var sync = new VideoSyncJoiner().Join(
            Coordinator.Clock!,
            request.Clock.UtcNow,
            request.FrameInterval,
            request.DriverStream.FrameTimestamps);

        var composite = new PassengerCompositor(request.Probe).Compose(
            "composite-" + request.SessionId,
            sync,
            request.DriverStream,
            request.PassengerStream,
            request.Samples);

        var performance = PerformanceGate.Evaluate(
            composite.Metrics,
            request.DriverStream.FrameTimestamps.Count,
            request.Thresholds);
        if (!performance.Admitted)
        {
            throw new RideAuditFailClosedException(
                "VALIDATION_FAILED",
                "FR-RIDE-220",
                performance.Detail);
        }

        var quota = VideoQuota.CheckComposite(composite.CanonicalBytes.Length, request.Quotas);
        if (!quota.Ok)
        {
            throw new RideAuditFailClosedException("VALIDATION_FAILED", "FR-RIDE-219", quota.Detail);
        }

        var metadata = new CompositeMetadata(
            composite.Codec,
            composite.Compression,
            composite.Overlay.Version,
            sync.Offset.ToString(),
            sync.Drift.ToString(),
            sync.Uncertainty.ToString(),
            sync.UnsyncedIntervals.Count,
            composite.Sources.Select(source => source.StreamId).ToList(),
            composite.Sources.Select(source => Hashes.Sha256Hex(source.Payload)).ToList(),
            composite.Overlay.TimelineManifestVersion,
            composite.Sources.Select(source => source.DeviceId).ToList());

        var scopeId = request.Scope == KeyScope.Session ? request.SessionId : "composite-" + request.SessionId;
        var sealedComposite = _sealer.Seal(new SealRequest
        {
            Plaintext = composite.CanonicalBytes.ToArray(),
            SessionId = request.SessionId,
            RecordId = composite.CompositeId,
            Scope = request.Scope,
            ScopeId = scopeId,
            Authorization = driverAuth,
            EscrowKey = request.Escrow,
            CollectorIdentity = request.CollectorIdentity,
            ProvenanceTag = "dual-phone-composite",
            Kind = EvidenceKind.Composite,
            DeviceIds = metadata.DeviceIds,
            Composite = metadata,
            SourceCommitNotice = request.SourceCommitNotice,
        });
        _store.Commit(sealedComposite);

        SealedRecord? sealedRaw = null;
        if (request.SealRaw)
        {
            var rawBytes = request.RawPayload ?? request.DriverStream.Payload;
            var rawQuota = VideoQuota.CheckRaw(rawBytes.Length, request.RawConsent, request.Quotas);
            if (!rawQuota.Ok)
            {
                throw new RideAuditFailClosedException("VALIDATION_FAILED", "FR-RIDE-046", rawQuota.Detail);
            }

            sealedRaw = _sealer.Seal(new SealRequest
            {
                Plaintext = rawBytes.ToArray(),
                SessionId = request.SessionId,
                RecordId = "raw-" + request.SessionId,
                Scope = KeyScope.Sample,
                ScopeId = "raw-" + request.SessionId,
                Authorization = passengerAuth,
                EscrowKey = request.Escrow,
                CollectorIdentity = request.CollectorIdentity,
                ProvenanceTag = "optional-raw-stream",
                Kind = EvidenceKind.RawStream,
                DeviceIds = [request.DriverStream.DeviceId],
                LinkedCompositeId = sealedComposite.Id,
                SourceCommitNotice = request.SourceCommitNotice,
            });
            _store.Commit(sealedRaw);
        }

        _sealer.DiscardSessionKeys();
        request.EscrowDeposit?.Deposit(sealedComposite);
        if (sealedRaw is not null)
            request.EscrowDeposit?.Deposit(sealedRaw);
        var submission = Submit(sealedComposite, request, driverAuth);
        Coordinator.Stop(PhoneRole.Driver, request.SessionId, request.Clock);
        return new CaptureResult
        {
            Pairing = pairing.Session,
            Sync = sync,
            Composite = composite,
            SealedComposite = sealedComposite,
            SealedRaw = sealedRaw,
            Submission = submission,
            Performance = performance,
            Commands = Coordinator.Commands.ToList(),
            PlaintextQueued = false,
        };
    }

    public PreparedSubmission Submit(SealedRecord record, CaptureRequest request, PlayAuthorization authorization)
    {
        if (!authorization.Accepted || authorization.Evidence is null)
        {
            throw new RideAuditFailClosedException("ATTESTATION_FAILED", "FR-RIDE-026", "Cannot submit without attestation.");
        }

        if (request.Driver.IntendedRole != PhoneRole.Driver)
        {
            throw new RideAuditFailClosedException("AUTH_FORBIDDEN", "FR-RIDE-035", "Only the driver phone may submit.");
        }

        var factory = request.RequestFactory ?? _defaultRequests;
        var proto = factory.Create(record, request, authorization.Evidence);
        var response = _admission.SubmitSealed(proto);
        if (!string.IsNullOrEmpty(response.RejectCode))
        {
            throw new RideAuditFailClosedException(response.RejectCode, "FR-RIDE-035", response.Message);
        }

        return new PreparedSubmission { Request = proto, Response = response };
    }
}

public static class SubmissionMapper
{
    public static SubmitSealedRequest ToRequest(
        SealedRecord record,
        string vehicleId,
        AttestationEvidence evidence)
    {
        var receipt = record.Receipt;
        var core = new ReceiptCore
        {
            SchemaVersion = RideAudit.Contracts.RideAuditPolicy.SchemaVersion,
            ReceiptId = receipt.SealedRecordId,
            PolicyVersion = receipt.AdmissionPolicyId,
            ContentHash = HexToBytes(receipt.ContentHash),
            AlgorithmId = receipt.Algorithm,
            KeyId = receipt.KeyId,
            PublicKeyMaterial = ByteString.CopyFromUtf8(receipt.PublicKeyPem),
            KeyScope = receipt.KeyScope,
            ScopeBinding = receipt.ScopeId,
            CollectorId = receipt.CollectorIdentity,
            DriverId = receipt.CollectorIdentity,
            VehicleId = vehicleId,
            SessionId = receipt.SessionId,
            TenantId = "",
            CollectionUnixMillis = receipt.SealedAt.ToUnixTimeMilliseconds(),
            ProvenanceTag = receipt.ProvenanceTag,
            AttestationEvidenceHash = HexToBytes(receipt.AttestationTokenHash),
            PackageIdentity = receipt.PackageIdentity,
            SigningCertDigest = receipt.SigningCertDigest,
            Nonce = receipt.Nonce,
            SealedRecordId = receipt.SealedRecordId,
        };

        return new SubmitSealedRequest
        {
            SessionId = receipt.SessionId,
            VehicleId = vehicleId,
            IdempotencyKey = receipt.SealedRecordId,
            ContentType = ApiBoundary.SealedContentType,
            SealedEnvelope = ByteString.CopyFrom(record.Envelope),
            ReceiptCore = core,
            Attestation = new AttestationSubmission
            {
                Provider = evidence.Provider,
                // SHA-256 hex retained on the device. Not a raw Play JWT and not a server fixture.v1 token.
                Token = evidence.TokenHash,
                Nonce = evidence.Nonce,
                ObtainedUnixMillis = evidence.ObtainedAt.ToUnixTimeMilliseconds(),
                PackageName = evidence.PackageIdentity,
                CertDigest = evidence.SigningCertDigest,
                BoundKeyId = receipt.KeyId,
                BoundSessionId = receipt.SessionId,
            },
        };
    }

    private static ByteString HexToBytes(string hex)
    {
        if (string.IsNullOrEmpty(hex) || hex.Length % 2 != 0)
        {
            throw new RideAuditFailClosedException(
                "RECEIPT_INVALID",
                "FR-RIDE-017",
                "Receipt hash is not hex.");
        }

        var bytes = new byte[hex.Length / 2];
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }

        return ByteString.CopyFrom(bytes);
    }
}
