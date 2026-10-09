using System.Text;
using RideAudit.Contracts;
using RideAudit.Seal;
using RideAudit.TestSupport;

namespace RideAudit.Server.Admission.Tests;

/// <summary>
/// Escrowed device RAES opens a court working copy after the same 2-of-3 threshold as RIDESEAL1.
/// In-process HSM only. No live hardware HSM.
/// </summary>
public class RaesWorkingCopyTests
{
    [Fact]
    [Trait("TEST", "TEST-RIDE-018")]
    [Trait("FR", "FR-RIDE-024")]
    public void Quorum_release_opens_escrowed_raes_and_leaves_envelope_unchanged()
    {
        var world = ServerWorld.Create();
        var enrolled = world.Enroll();
        var plaintext = Encoding.UTF8.GetBytes("raes-working-copy-plaintext");
        var raes = RaesHelpers.SealRaes(enrolled.Driver.DriverId, enrolled.Session.SessionId, plaintext);
        RaesHelpers.Escrow(world, enrolled.Driver.TenantId, raes);
        var envelope = raes.Record.Envelope.ToArray();

        var release = world.App.Hsm.RequestRelease(
            raes.Record.Receipt.KeyId,
            enrolled.Driver.TenantId,
            "CASE-RAES",
            "subpoena-raes-1",
            "court review",
            "reviewer-1");
        world.App.Hsm.Approve(release.ReleaseId, "custodian-a", "I approve this case.");
        var oneShare = Assert.Throws<RideAuditException>(() =>
            world.App.Hsm.OpenWorkingCopy(release.ReleaseId, raes.Record.Envelope, TimeSpan.FromMinutes(15)));
        Assert.Equal(ErrorCodes.EscrowQuorum, oneShare.Code);

        world.App.Hsm.Approve(release.ReleaseId, "custodian-b", "second custodian");
        var copy = world.App.Hsm.OpenWorkingCopy(release.ReleaseId, raes.Record.Envelope, TimeSpan.FromMinutes(15));
        Assert.Equal(plaintext, copy.ReadPlaintext());
        Assert.True(RaesEnvelopeFormat.HasMagic(raes.Record.Envelope));
        Assert.True(envelope.AsSpan().SequenceEqual(raes.Record.Envelope));
        Assert.Equal(1, world.App.Hsm.WorkingCopyOpens);

        world.Clock.Advance(TimeSpan.FromMinutes(16));
        var expired = Assert.Throws<RideAuditException>(() => copy.ReadPlaintext());
        Assert.Equal(ErrorCodes.WorkingCopyExpired, expired.Code);
    }

    [Fact]
    [Trait("TEST", "TEST-RIDE-018")]
    public void Tampered_raes_envelope_fails_closed_and_does_not_rewrite_ciphertext()
    {
        var world = ServerWorld.Create();
        var enrolled = world.Enroll();
        var raes = RaesHelpers.SealRaes(enrolled.Driver.DriverId, enrolled.Session.SessionId);
        RaesHelpers.Escrow(world, enrolled.Driver.TenantId, raes);
        var original = raes.Record.Envelope.ToArray();
        var release = world.App.Hsm.RequestRelease(
            raes.Record.Receipt.KeyId,
            enrolled.Driver.TenantId,
            "CASE-RAES-TAMPER",
            "subpoena-raes-2",
            "court review",
            "reviewer-1");
        world.App.Hsm.Approve(release.ReleaseId, "custodian-a", "approve");
        world.App.Hsm.Approve(release.ReleaseId, "custodian-b", "approve");

        var tampered = original.ToArray();
        tampered[^1] ^= 0x5A;
        Assert.ThrowsAny<Exception>(() => world.App.Hsm.OpenWorkingCopy(release.ReleaseId, tampered, TimeSpan.FromMinutes(15)));
        Assert.True(original.AsSpan().SequenceEqual(raes.Record.Envelope));
    }
}
