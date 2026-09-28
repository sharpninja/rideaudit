// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using RideAudit.Bt;
using RideAudit.Client.Seal;
using RideAudit.Client.Tests.Support;
using RideAudit.PlayIntegrity;
using RideAudit.Video;
using Xunit;

namespace RideAudit.Client.Tests;

public class HighRiskAcCoverageTests
{
    [Fact]
    [Trait("FR", "FR-RIDE-041")]
    [Trait("FR", "FR-RIDE-042")]
    [Trait("FR", "FR-RIDE-043")]
    [Trait("FR", "FR-RIDE-044")]
    [Trait("FR", "FR-RIDE-045")]
    [Trait("TR", "TR-RIDE-VIDEO-001")]
    [Trait("TR", "TR-RIDE-VIDEO-002")]
    [Trait("TR", "TR-RIDE-VIDEO-003")]
    [Trait("TR", "TR-RIDE-VIDEO-004")]
    [Trait("TR", "TR-RIDE-VIDEO-005")]
    [Trait("AC", "AC-RIDE-VIDEO-001-001")]
    [Trait("AC", "AC-RIDE-VIDEO-001-002")]
    [Trait("AC", "AC-RIDE-VIDEO-002-001")]
    [Trait("AC", "AC-RIDE-VIDEO-002-002")]
    [Trait("AC", "AC-RIDE-VIDEO-003-001")]
    [Trait("AC", "AC-RIDE-VIDEO-003-002")]
    [Trait("AC", "AC-RIDE-VIDEO-004-001")]
    [Trait("AC", "AC-RIDE-VIDEO-004-002")]
    [Trait("AC", "AC-RIDE-VIDEO-005-001")]
    [Trait("AC", "AC-RIDE-VIDEO-005-002")]
    public void In_process_dual_phone_capture_records_sync_overlay_and_sealed_composite()
    {
        var fixture = Fixtures.CaptureHappy();
        var capture = fixture.Capture;
        Assert.Equal(PhoneRole.Driver, capture.Pairing.Driver.IntendedRole);
        Assert.Equal(PhoneRole.Passenger, capture.Pairing.Passenger.IntendedRole);
        Assert.Equal(2, capture.Composite.Sources.Count);
        Assert.Contains(capture.Composite.Sources, source => source.DeviceId == "device-driver");
        Assert.Contains(capture.Composite.Sources, source => source.DeviceId == "device-passenger");
        Assert.True(capture.Sync.Uncertainty >= TimeSpan.Zero);
        Assert.NotNull(capture.Composite.Overlay);
        Assert.False(string.IsNullOrWhiteSpace(capture.Composite.Overlay.Version));
        Assert.False(string.IsNullOrWhiteSpace(capture.Composite.Overlay.TimelineManifestVersion));
        Assert.True(capture.Composite.ProducedOnDevice);
        Assert.False(capture.Composite.ServerPlaintextComposite);
        Assert.Equal(EvidenceKind.Composite, capture.SealedComposite.Kind);
        Assert.False(string.IsNullOrWhiteSpace(capture.SealedComposite.Receipt.ContentHash));
        Assert.False(string.IsNullOrWhiteSpace(capture.SealedComposite.Receipt.AdmissionPolicyId));
        Assert.Contains("not-h264", System.Text.Encoding.UTF8.GetString(capture.Composite.CanonicalBytes));
    }

    [Fact]
    [Trait("FR", "FR-RIDE-027")]
    [Trait("FR", "FR-RIDE-215")]
    [Trait("AC", "AC-RIDE-PLAY-002-001")]
    [Trait("AC", "AC-RIDE-PLAY-002-002")]
    [Trait("AC", "AC-RIDE-PLAY-003-002")]
    public void Receipt_carries_attestation_fields_and_allowlist_rotation_is_audited()
    {
        var fixture = Fixtures.CaptureHappy();
        var receipt = fixture.Capture.SealedComposite.Receipt;
        Assert.False(string.IsNullOrWhiteSpace(receipt.AttestationTokenHash));
        Assert.Equal(ApprovedPackage.PackageName, receipt.PackageIdentity);
        Assert.Equal(ApprovedPackage.CertDigest, receipt.SigningCertDigest);
        Assert.Equal(fixture.Capture.SealedComposite.Receipt.KeyId, fixture.Capture.SealedComposite.WrappedKey.KeyId);
        Assert.Equal(fixture.Capture.SealedComposite.Id, receipt.SealedRecordId);

        var rotated = fixture.Allowlist.RotateAdd(
            new AllowlistEntry("org.rideaudit.app.next", "sha256:next-cert"),
            "rotate signing cert",
            "operator",
            fixture.Clock.UtcNow);
        Assert.Equal(2, rotated.Version);
        Assert.Contains(rotated.Rotations, item => item.Action == "add" && item.Reason == "rotate signing cert");
        Assert.True(rotated.IsAllowed("org.rideaudit.app.next", "sha256:next-cert"));
    }
}
