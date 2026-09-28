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

    public bool ProductionReady => UnavailableSeams.Count == 0;

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
            UnavailableSeams = unavailable
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
                UnavailableSeams = graph.UnavailableSeams.Append("CanonicalAdmission: " + ex.Message).ToList()
            };
        }
    }
}
