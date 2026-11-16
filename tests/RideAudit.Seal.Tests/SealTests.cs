using System.Text;
using RideAudit.Contracts;
using RideAudit.Seal;
using RideAudit.TestSupport;

namespace RideAudit.Seal.Tests;

/// <summary>
/// TEST-RIDE-013. FR-RIDE-015, FR-RIDE-016, TR-RIDE-SEAL-001, TR-RIDE-SEAL-002.
/// </summary>
public class TestRide013Seal
{
    [Fact]
    [Trait("TEST", "TEST-RIDE-013")]
    [Trait("FR", "FR-RIDE-015")]
        [Trait("AC", "AC-RIDE-015-001")]
        [Trait("AC", "AC-RIDE-SEAL-001-001")]
        [Trait("AC", "AC-RIDE-SEAL-001-002")]
        [Trait("AC", "AC-RIDE-SEAL-003-001")]
    public void Seal_completes_before_durable_store_and_records_algorithm_and_hash()
    {
        var world = ServerWorld.Create();
        var enrolled = world.Enroll();
        var plaintext = Encoding.UTF8.GetBytes("rideaudit-sample-plaintext-seal-order");
        var ready = world.SealReady(enrolled.Driver, enrolled.Session, plaintext);
        var store = new InMemorySealStore();
        var pipeline = new SealPipeline(world.App.Sealer, store);
        var nonce = "nonce-pipeline";
        var token = world.IssueToken(enrolled.Session.SessionId, nonce);
        var package = pipeline.Collect(new SealRequest
        {
            Plaintext = plaintext,
            KeyScope = "session",
            ScopeBinding = enrolled.Session.SessionId,
            TenantId = enrolled.Driver.TenantId,
            DriverId = enrolled.Driver.DriverId,
            CollectorId = enrolled.Driver.DriverId,
            VehicleId = enrolled.Session.VehicleId,
            SessionId = enrolled.Session.SessionId,
            ProvenanceTag = RideAuditPolicy.ProvenanceTag,
            PolicyVersion = world.Options.PolicyVersion,
            AttestationToken = token,
            ExpectedNonce = nonce,
            CustodianIds = ["custodian-a", "custodian-b", "custodian-c"]
        });

        Assert.True(pipeline.LastSealSequence < pipeline.LastStoreSequence);
        Assert.Equal(1, store.PutCount);
        Assert.Equal(RideAuditPolicy.AlgorithmId, package.AlgorithmId);
        Assert.Equal(32, package.Receipt.Core.ContentHash.Length);
        Assert.DoesNotContain("PrivateKey", typeof(SealedPackage).GetProperties().Select(p => p.Name));
        Assert.NotEqual(plaintext, package.Ciphertext);
        _ = ready;
    }

    [Fact]
    [Trait("TEST", "TEST-RIDE-013")]
    [Trait("FR", "FR-RIDE-016")]
    [Trait("AC", "AC-RIDE-016-001")]
        [Trait("AC", "AC-RIDE-016-002")]
        [Trait("AC", "AC-RIDE-SEAL-002-001")]
        [Trait("AC", "AC-RIDE-SEAL-002-002")]
    public void Keys_are_session_scoped_and_shared_all_record_keys_are_rejected()
    {
        var world = ServerWorld.Create();
        var enrolled = world.Enroll();
        var first = world.SealReady(enrolled.Driver, enrolled.Session);
        var second = world.SealReady(enrolled.Driver, enrolled.Session);
        Assert.NotEqual(first.Package.KeyId, second.Package.KeyId);
        Assert.Equal("session", first.Package.Receipt.Core.KeyScope);

        var before = world.App.Hsm.GenerateCount;
        var ex = Assert.Throws<RideAuditException>(() => world.Seal(enrolled.Driver, enrolled.Session, keyScope: "all-records"));
        Assert.Equal(ErrorCodes.KeyScopeRejected, ex.Code);
        Assert.Equal(before, world.App.Hsm.GenerateCount);
    }
}

/// <summary>
/// TEST-RIDE-019 sealer partition. FR-RIDE-026. Integrity runs before key generation.
/// </summary>
public class TestRide019SealGate
{
    [Fact]
    [Trait("TEST", "TEST-RIDE-019")]
    [Trait("FR", "FR-RIDE-026")]
    [Trait("AC", "AC-RIDE-026-001")]
        [Trait("AC", "AC-RIDE-026-002")]
        [Trait("AC", "AC-RIDE-PLAY-001-001")]
        [Trait("AC", "AC-RIDE-PLAY-001-002")]
        [Trait("AC", "AC-RIDE-PLAY-003-001")]
    public void Failed_sideloaded_or_unlisted_attestation_rejects_before_key_generation()
    {
        var world = ServerWorld.Create();
        var enrolled = world.Enroll();
        var nonce = "nonce-fail";
        var token = world.IssueToken(enrolled.Session.SessionId, nonce, payload => payload.Sideloaded = true);
        var before = world.App.Hsm.GenerateCount;
        var ex = Assert.Throws<RideAuditException>(() => world.Seal(enrolled.Driver, enrolled.Session, token: token, nonce: nonce));
        Assert.Equal(ErrorCodes.AttestationFailed, ex.Code);
        Assert.Equal(before, world.App.Hsm.GenerateCount);

        var unlisted = world.IssueToken(enrolled.Session.SessionId, nonce, payload => payload.PackageName = "app.unofficial");
        ex = Assert.Throws<RideAuditException>(() => world.Seal(enrolled.Driver, enrolled.Session, token: unlisted, nonce: nonce));
        Assert.Equal(ErrorCodes.AttestationFailed, ex.Code);
        Assert.Equal(before, world.App.Hsm.GenerateCount);
    }
}

/// <summary>
/// FR-RIDE-211 cryptographic agility. TEST-RIDE-032 is killed and FR-RIDE-211 maps to no TEST (Payton 2026-10-08).
/// </summary>
public class TestRide032Agility
{
    [Fact]
    [Trait("FR", "FR-RIDE-211")]
    [Trait("AC", "AC-RIDE-211-001")]
        [Trait("AC", "AC-RIDE-211-002")]
        [Trait("AC", "AC-RIDE-SEAL-003-002")]
    public void Rotation_stamps_new_records_and_does_not_rewrite_ciphertext()
    {
        var world = ServerWorld.Create();
        var enrolled = world.Enroll();
        var first = world.SealReady(enrolled.Driver, enrolled.Session);
        var original = first.Package.Ciphertext.ToArray();
        world.App.Algorithms.Rotate("aes-256-gcm-sha256-v2");
        var second = world.SealReady(enrolled.Driver, enrolled.Session);
        Assert.Equal(RideAuditPolicy.AlgorithmId, first.Package.AlgorithmId);
        Assert.Equal("aes-256-gcm-sha256-v2", second.Package.AlgorithmId);
        Assert.Equal(original, first.Package.Ciphertext);
    }
}
