using RideAudit.Chain;
using RideAudit.Contracts;
using RideAudit.Escrow;
using RideAudit.Server.Admission;
using RideAudit.TestSupport;

namespace RideAudit.Escrow.Tests;

public class ShamirTests
{
    [Fact]
    [Trait("TEST", "TEST-RIDE-017")]
    public void Two_of_three_shares_reconstruct_and_one_share_does_not()
    {
        var shamir = new ShamirSecretSharing();
        var secret = new byte[] { 1, 2, 3, 9, 255, 0, 7, 8 };
        var shares = shamir.Split(secret, 2, 3);
        var restored = shamir.Combine(new[] { shares[0], shares[2] }, 2);
        Assert.Equal(secret, restored);
        Assert.Throws<InvalidOperationException>(() => shamir.Combine(new[] { shares[1] }, 2));
    }
}

/// <summary>
/// TEST-RIDE-017. FR-RIDE-022, FR-RIDE-023, FR-RIDE-216, FR-RIDE-214.
/// </summary>
public class TestRide017Escrow
{
    [Fact]
    [Trait("TEST", "TEST-RIDE-017")]
    [Trait("FR", "FR-RIDE-022")]
    [Trait("FR", "FR-RIDE-023")]
    [Trait("AC", "AC-RIDE-022-001")]
    [Trait("AC", "AC-RIDE-023-001")]
        [Trait("AC", "AC-RIDE-023-003")]
        [Trait("AC", "AC-RIDE-ESCROW-001-001")]
        [Trait("AC", "AC-RIDE-ESCROW-001-002")]
        [Trait("AC", "AC-RIDE-ESCROW-003-001")]
    public void Escrow_is_off_the_application_database_and_does_not_change_the_sealed_blob()
    {
        var world = ServerWorld.Create();
        var enrolled = world.Enroll();
        var ready = world.SealReady(enrolled.Driver, enrolled.Session);
        var before = ready.Package.ReceiptCoreBytes.ToArray();
        var cipher = ready.Package.Ciphertext.ToArray();
        Assert.True(world.App.Hsm.IsEscrowed(ready.Package.KeyId, enrolled.Driver.TenantId));
        Assert.Equal(before, ready.Package.ReceiptCoreBytes);
        Assert.Equal(cipher, ready.Package.Ciphertext);

        world.Submit(enrolled.Driver, ready.Package, ready.Token, ready.Nonce);
        var dump = world.App.Identity.Database.DumpForAudit();
        foreach (var share in world.App.Hsm.CopySharePayloads(ready.Package.KeyId))
            Assert.True(dump.AsSpan().IndexOf(share) < 0);
        Assert.False(world.App.Hsm.ReleaseLogHasRemovalApi());
        Assert.DoesNotContain(world.App.Hsm.AuditLog, entry => entry.Detail.Contains("PRIVATE", StringComparison.Ordinal));
    }
}

/// <summary>
/// TEST-RIDE-018 and the TEST-RIDE-016 court-release path partition (not the verification UI).
/// FR-RIDE-024, FR-RIDE-028, FR-RIDE-020 documentation.
/// </summary>
public class TestRide018CourtRelease
{
    [Fact]
    [Trait("TEST", "TEST-RIDE-018")]
    [Trait("FR", "FR-RIDE-024")]
    [Trait("AC", "AC-RIDE-024-001")]
        [Trait("AC", "AC-RIDE-024-003")]
        [Trait("AC", "AC-RIDE-ESCROW-002-001")]
        [Trait("AC", "AC-RIDE-ESCROW-002-002")]
        [Trait("AC", "AC-RIDE-ESCROW-003-002")]
    public void Quorum_release_opens_one_expiring_working_copy_and_leaves_ciphertext_unchanged()
    {
        var world = ServerWorld.Create();
        var enrolled = world.Enroll();
        var ready = world.SealReady(enrolled.Driver, enrolled.Session);
        var other = world.SealReady(enrolled.Driver, enrolled.Session);
        world.Submit(enrolled.Driver, ready.Package, ready.Token, ready.Nonce);
        var cipher = ready.Package.Ciphertext.ToArray();
        var receipt = ready.Package.ReceiptCoreBytes.ToArray();

        var release = world.App.Hsm.RequestRelease(ready.Package.KeyId, enrolled.Driver.TenantId, "CASE-9", "subpoena-2026-9", "counsel review", "counsel-1");
        world.App.Hsm.Approve(release.ReleaseId, "custodian-a", "I approve this case.");
        Assert.Throws<RideAuditException>(() => world.App.Hsm.OpenWorkingCopy(release.ReleaseId, ready.Package.EnvelopeBytes, TimeSpan.FromMinutes(15)));
        world.App.Hsm.Approve(release.ReleaseId, "custodian-a", "duplicate approval");
        Assert.Single(release.Approvers);
        world.App.Hsm.Approve(release.ReleaseId, "custodian-b", "second custodian");

        var copy = world.App.Hsm.OpenWorkingCopy(release.ReleaseId, ready.Package.EnvelopeBytes, TimeSpan.FromMinutes(15));
        Assert.Equal(ready.Plaintext, copy.ReadPlaintext());
        Assert.True(copy.MinimumScope);
        Assert.Equal(cipher, ready.Package.Ciphertext);
        Assert.Equal(receipt, ready.Package.ReceiptCoreBytes);
        Assert.True(world.App.Hsm.IsEscrowed(other.Package.KeyId, enrolled.Driver.TenantId));
        Assert.Contains(world.App.Hsm.ReleaseLog, entry => entry.Action == "open-working-copy" && entry.KeyId == ready.Package.KeyId);

        world.Clock.Advance(TimeSpan.FromMinutes(16));
        Assert.Throws<RideAuditException>(() => copy.ReadPlaintext());
    }

    [Fact]
    [Trait("TEST", "TEST-RIDE-016")]
    [Trait("FR", "FR-RIDE-020")]
    [Trait("FR", "FR-RIDE-028")]
    [Trait("AC", "AC-RIDE-020-001")]
    [Trait("AC", "AC-RIDE-028-001")]
    public void Court_path_doc_and_verifier_report_hash_mismatches()
    {
        var doc = File.ReadAllText(Path.Combine(ServerWorld.RepoRoot(), "docs/architecture/court-review-decryption-path.md"));
        Assert.Contains("Custodians", doc, StringComparison.Ordinal);
        Assert.Contains("Legal process", doc, StringComparison.Ordinal);
        Assert.Contains("dual control", doc, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("expiring", doc, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("does not prove", doc, StringComparison.OrdinalIgnoreCase);

        var world = ServerWorld.Create();
        var enrolled = world.Enroll();
        var ready = world.SealReady(enrolled.Driver, enrolled.Session);
        var outcome = world.Submit(enrolled.Driver, ready.Package, ready.Token, ready.Nonce);
        var good = AnchorProofVerifier.Verify(ready.Package.ReceiptCoreBytes, ready.Package.Ciphertext, outcome.Anchor!);
        Assert.True(good.PayloadHashMatch);
        Assert.True(good.Confirmed);

        var tampered = ready.Package.Ciphertext.ToArray();
        tampered[0] ^= 0xFF;
        var bad = AnchorProofVerifier.Verify(ready.Package.ReceiptCoreBytes, tampered, outcome.Anchor!);
        Assert.False(bad.PayloadHashMatch);
        Assert.Contains(bad.Mismatches, mismatch => mismatch.Contains("hash", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("does not prove", CourtReviewStatements.DoesNotProve, StringComparison.OrdinalIgnoreCase);
    }
}
