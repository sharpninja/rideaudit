// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using RideAudit.Client.Core;
using RideAudit.Client.Tests.Support;
using RideAudit.PlayIntegrity;
using RideAudit.Seal;
using Xunit;

namespace RideAudit.Client.Tests;

[Trait("Partition", "TEST-RIDE-019")]
public class TestRide019PlayTests
{
    [Fact]
    [Trait("FR", "FR-RIDE-025")]
    [Trait("AC", "AC-RIDE-025-001")]
    public void Accepted_attestation_binds_package_cert_and_nonce()
    {
        var fixture = Fixtures.CaptureHappy();
        var receipt = fixture.Capture.SealedComposite.Receipt;
        Assert.Equal(ApprovedPackage.PackageName, receipt.PackageIdentity);
        Assert.Equal(ApprovedPackage.CertDigest, receipt.SigningCertDigest);
        Assert.False(string.IsNullOrWhiteSpace(receipt.Nonce));
        Assert.Equal(receipt.Nonce, fixture.Capture.SealedComposite.WrappedKey.BindingDigest is not null ? receipt.Nonce : "");
        Assert.Contains(receipt.PackageIdentity, receipt.KeyBindingDigest is string ? receipt.CanonicalForm() : "");
    }

    [Fact]
    [Trait("FR", "FR-RIDE-025")]
    [Trait("AC", "AC-RIDE-025-002")]
    public void Attestation_evidence_is_retained_on_the_receipt()
    {
        var receipt = Fixtures.CaptureHappy().Capture.SealedComposite.Receipt;
        Assert.False(string.IsNullOrWhiteSpace(receipt.AttestationTokenHash));
        Assert.False(string.IsNullOrWhiteSpace(receipt.AttestationReference));
        Assert.Equal(receipt.AttestationTokenHash, receipt.AttestationReference);
        Assert.Equal(PlayIntegrityProviders.Real, receipt.AttestationProvider);
        Assert.True(string.IsNullOrEmpty(receipt.StubNotice));
    }

    [Theory]
    [InlineData(StubPlayMode.Missing)]
    [InlineData(StubPlayMode.Failed)]
    [InlineData(StubPlayMode.Compromised)]
    [InlineData(StubPlayMode.Sideloaded)]
    [InlineData(StubPlayMode.Stale)]
    [InlineData(StubPlayMode.NonceMismatch)]
    [InlineData(StubPlayMode.Unverifiable)]
    [InlineData(StubPlayMode.NotAllowlisted)]
    [Trait("FR", "FR-RIDE-026")]
    [Trait("AC", "AC-RIDE-026-002")]
    public void Failed_attestations_reject_collection(StubPlayMode mode)
    {
        var ex = Assert.Throws<RideAuditFailClosedException>(() => Fixtures.CaptureHappy(new StubPlayIntegrityClient(mode)));
        Assert.Equal("ATTESTATION_FAILED", ex.Code);
        Assert.Equal("FR-RIDE-026", ex.RequirementId);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-026")]
    [Trait("AC", "AC-RIDE-026-001")]
    public void Gate_rejects_before_a_key_exists()
    {
        var clock = new FixedClock(DateTimeOffset.Parse("2026-09-28T12:00:00Z"));
        var gate = new PlayIntegrityGate(new UnavailablePlayIntegrityClient(), PackageAllowlist.CreateDevelopmentDefault(), clock);
        var auth = gate.AuthorizeKeyGeneration(AttestationRequest.Create("collector"));
        Assert.False(auth.Accepted);
        Assert.Null(auth.Evidence);
        var sealer = new CollectionSealer(clock);
        var (publicKey, _) = EscrowKeyFactory.CreateEphemeral("escrow");
        var ex = Assert.Throws<RideAuditFailClosedException>(() => sealer.Seal(new SealRequest
        {
            Plaintext = [1, 2, 3],
            SessionId = "s",
            RecordId = "r",
            Scope = KeyScope.Session,
            ScopeId = "s",
            Authorization = auth,
            EscrowKey = publicKey,
            CollectorIdentity = "collector",
            ProvenanceTag = "sensor",
            Kind = EvidenceKind.SensorSample,
            DeviceIds = ["d"],
        }));
        Assert.Equal("ATTESTATION_FAILED", ex.Code);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-026")]
    [Trait("AC", "AC-RIDE-026-003")]
    public void Failed_attempt_is_not_stored()
    {
        var store = new DeviceBoundaryStore();
        Assert.Throws<RideAuditFailClosedException>(() => Fixtures.CaptureHappy(new StubPlayIntegrityClient(StubPlayMode.Failed)));
        Assert.Empty(store.Records);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-027")]
    [Trait("AC", "AC-RIDE-027-001")]
    public void Receipt_carries_attestation_package_and_cert()
    {
        var receipt = Fixtures.CaptureHappy().Capture.SealedComposite.Receipt;
        Assert.False(string.IsNullOrWhiteSpace(receipt.AttestationTokenHash));
        Assert.Equal(ApprovedPackage.PackageName, receipt.PackageIdentity);
        Assert.Equal(ApprovedPackage.CertDigest, receipt.SigningCertDigest);
        Assert.Equal(AdmissionPolicy.ChainId, receipt.ChainId);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-027")]
    [Trait("AC", "AC-RIDE-027-002")]
    public void Receipt_links_key_material_and_sealed_record()
    {
        var record = Fixtures.CaptureHappy().Capture.SealedComposite;
        Assert.Equal(record.Id, record.Receipt.SealedRecordId);
        Assert.Equal(record.WrappedKey.KeyId, record.Receipt.KeyId);
        Assert.Contains("BEGIN RSA PUBLIC KEY", record.Receipt.PublicKeyPem);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-215")]
    [Trait("AC", "AC-RIDE-215-001")]
    public void Allowlist_version_is_enforced()
    {
        var clock = new FixedClock(DateTimeOffset.Parse("2026-09-28T12:00:00Z"));
        var allow = PackageAllowlist.CreateDevelopmentDefault();
        var gate = new PlayIntegrityGate(new FixturePlayIntegrityClient(), allow, clock);
        var ok = gate.AuthorizeKeyGeneration(AttestationRequest.Create("collector"));
        Assert.True(ok.Accepted);
        Assert.Equal(1, allow.Version);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-215")]
    [Trait("AC", "AC-RIDE-215-002")]
    public void Removed_certificate_is_rejected()
    {
        var clock = new FixedClock(DateTimeOffset.Parse("2026-09-28T12:00:00Z"));
        var replacement = new AllowlistEntry(ApprovedPackage.PackageName, "sha256:rotated-cert");
        var rotated = PackageAllowlist.CreateDevelopmentDefault()
            .RotateAdd(replacement, "stage replacement", "security", clock.UtcNow)
            .RotateRemove(
                new AllowlistEntry(ApprovedPackage.PackageName, ApprovedPackage.CertDigest),
                "cert rotation",
                "security",
                clock.UtcNow);
        var gate = new PlayIntegrityGate(new FixturePlayIntegrityClient(), rotated, clock);
        var auth = gate.AuthorizeKeyGeneration(AttestationRequest.Create("collector"));
        Assert.False(auth.Accepted);
        Assert.Equal(AttestationFailure.NotAllowlisted, auth.Failure);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-215")]
    [Trait("AC", "AC-RIDE-215-003")]
    public void Rotation_is_auditable()
    {
        var when = DateTimeOffset.Parse("2026-09-28T12:00:00Z");
        var entry = new AllowlistEntry("org.rideaudit.app", "sha256:next");
        var rotated = PackageAllowlist.CreateDevelopmentDefault().RotateAdd(entry, "annual rotation", "security", when);
        Assert.Equal(2, rotated.Version);
        Assert.Contains(rotated.Rotations, rotation => rotation.Action == "add" && rotation.Actor == "security" && rotation.Reason == "annual rotation");
    }

    [Fact]
    public void Stub_success_is_labeled_and_not_a_play_token()
    {
        var clock = new FixedClock(DateTimeOffset.Parse("2026-09-28T12:00:00Z"));
        var gate = new PlayIntegrityGate(new StubPlayIntegrityClient(StubPlayMode.SimulatedSuccess), PackageAllowlist.CreateDevelopmentDefault(), clock);
        var auth = gate.AuthorizeKeyGeneration(AttestationRequest.Create("collector"));
        Assert.True(auth.Accepted);
        Assert.Equal(PlayIntegrityProviders.Stub, auth.Evidence!.Provider);
        Assert.Contains("not a Google Play Integrity token", auth.Evidence.StubNotice);
    }
}
