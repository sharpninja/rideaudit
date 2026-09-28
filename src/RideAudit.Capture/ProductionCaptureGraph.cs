// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using RideAudit.Bt;
using RideAudit.Client.Contracts;
using RideAudit.Client.Core;
using RideAudit.Client.Seal;
using RideAudit.Contracts;
using RideAudit.PlayIntegrity;
using RideAudit.Video;

namespace RideAudit.Capture;

public sealed class UnavailableDeviceEscrowDeposit : IDeviceEscrowDeposit
{
    public void Deposit(SealedRecord record) =>
        throw new RideAuditFailClosedException(
            ErrorCodes.EscrowUnavailable,
            "FR-RIDE-033",
            "Hardware HSM escrow is not configured. In-process fixtures are test-only.");
}

/// <summary>
/// Production composition of capture seams. Missing hardware, Play, or admission
/// is recorded as an honest Unavailable* reason. This is not a silent success.
/// </summary>
public sealed class ProductionCaptureGraph
{
    public required IDiscoveryBus Discovery { get; init; }
    public required ICameraSource Camera { get; init; }
    public required IPlayIntegrityClient Play { get; init; }
    public required IAdmissionRequestFactory Requests { get; init; }
    public required ISealedAdmissionClient? Admission { get; init; }
    public required IDeviceEscrowDeposit? EscrowDeposit { get; init; }
    public required string DiscoveryDetail { get; init; }
    public required string CameraDetail { get; init; }
    public required IReadOnlyList<string> UnavailableSeams { get; init; }

    public required IReadOnlyList<string> FixtureSeams { get; init; }

    /// <summary>
    /// True only when no Unavailable* hardware/config gap and no fixture/in-memory
    /// seam is present. Fixture-executable graphs are not production-ready.
    /// </summary>
    public bool ProductionReady => UnavailableSeams.Count == 0 && FixtureSeams.Count == 0;

    public static ProductionCaptureGraph Wire(
        IDiscoveryBus discovery,
        ICameraSource camera,
        IPlayIntegrityClient play,
        IAdmissionRequestFactory requests,
        ISealedAdmissionClient? admission,
        IDeviceEscrowDeposit? escrow,
        string discoveryDetail,
        string cameraDetail)
    {
        var unavailable = new List<string>();
        if (!discovery.RadioAvailable)
            unavailable.Add("UnavailableDiscoveryBus/radio: " + discoveryDetail);
        if (!camera.CameraAvailable)
            unavailable.Add("UnavailableCameraSource: " + cameraDetail);
        if (play is UnavailablePlayIntegrityClient)
            unavailable.Add("UnavailablePlayIntegrityClient: Play Integrity API was not called.");
        if (admission is null)
            unavailable.Add("AdmissionUnavailable: RIDEAUDIT_ADMISSION_ADDRESS/BEARER are not set.");
        if (escrow is null or UnavailableDeviceEscrowDeposit)
            unavailable.Add("UnavailableHsmEscrow: hardware HSM is not configured.");

        return new ProductionCaptureGraph
        {
            Discovery = discovery,
            Camera = camera,
            Play = play,
            Requests = requests,
            Admission = admission,
            EscrowDeposit = escrow,
            DiscoveryDetail = discoveryDetail,
            CameraDetail = cameraDetail,
            UnavailableSeams = unavailable,
            FixtureSeams = LabelFixtureSeams(discovery, camera, play, admission, escrow)
        };
    }

    public static ProductionCaptureGraph FromEnvironment(IDiscoveryBus discovery, ICameraSource camera, string discoveryDetail, string cameraDetail)
    {
        ISealedAdmissionClient? admission = null;
        try
        {
            admission = CaptureAdmissionChannel.Connect(CaptureAdmissionOptions.FromEnvironment());
        }
        catch (RideAuditFailClosedException)
        {
            admission = null;
        }

        var identity = CanonicalAdmissionIdentity.FromEnvironment();
        var graph = Wire(
            discovery,
            camera,
            new UnavailablePlayIntegrityClient(),
            new CanonicalAdmissionRequestFactory(identity),
            admission,
            null,
            discoveryDetail,
            cameraDetail);
        try
        {
            identity.EnsureReady();
            return graph;
        }
        catch (RideAuditFailClosedException ex)
        {
            return new ProductionCaptureGraph
            {
                Discovery = graph.Discovery,
                Camera = graph.Camera,
                Play = graph.Play,
                Requests = graph.Requests,
                Admission = graph.Admission,
                EscrowDeposit = graph.EscrowDeposit,
                DiscoveryDetail = graph.DiscoveryDetail,
                CameraDetail = graph.CameraDetail,
                UnavailableSeams = graph.UnavailableSeams.Append("CanonicalAdmission: " + ex.Message).ToList(),
                FixtureSeams = graph.FixtureSeams
            };
        }
    }

    public ProductionCaptureGraph WithExtraUnavailable(params string[] extras)
    {
        if (extras.Length == 0)
            return this;

        return new ProductionCaptureGraph
        {
            Discovery = Discovery,
            Camera = Camera,
            Play = Play,
            Requests = Requests,
            Admission = Admission,
            EscrowDeposit = EscrowDeposit,
            DiscoveryDetail = DiscoveryDetail,
            CameraDetail = CameraDetail,
            UnavailableSeams = UnavailableSeams.Concat(extras).ToList(),
            FixtureSeams = FixtureSeams
        };
    }

    private static IReadOnlyList<string> LabelFixtureSeams(
        IDiscoveryBus discovery,
        ICameraSource camera,
        IPlayIntegrityClient play,
        ISealedAdmissionClient? admission,
        IDeviceEscrowDeposit? escrow)
    {
        var fixtures = new List<string>();
        if (discovery is InMemoryDiscoveryBus || LooksLikeFixtureType(discovery))
            fixtures.Add("FIXTURE: InMemoryDiscoveryBus / in-memory discovery is not a live Bluetooth radio.");
        if (camera is FixtureCameraSource || LooksLikeFixtureType(camera))
            fixtures.Add("FIXTURE: FixtureCameraSource is not a live camera.");
        if (play is FixturePlayIntegrityClient || LooksLikeFixtureType(play))
            fixtures.Add("FIXTURE: Play Integrity client is a labeled fixture, not live Play.");
        if (admission is InterimInProcessAdmissionClient)
            fixtures.Add("FIXTURE: InterimInProcessAdmissionClient is a client preflight, not server admission.");
        else if (admission is not null && LooksLikeFixtureType(admission))
            fixtures.Add("FIXTURE: " + admission.GetType().Name + " is a labeled test admission seam, not production readiness.");
        if (escrow is not null && escrow is not UnavailableDeviceEscrowDeposit)
            fixtures.Add("FIXTURE: in-process escrow deposit is not hardware HSM.");
        return fixtures;
    }

    private static bool LooksLikeFixtureType(object? seam)
    {
        if (seam is null)
            return false;
        var name = seam.GetType().Name;
        return name.Contains("Fixture", StringComparison.Ordinal)
            || name.Contains("InMemory", StringComparison.Ordinal)
            || name.Contains("Recording", StringComparison.Ordinal)
            || name.Contains("Interim", StringComparison.Ordinal);
    }
}
