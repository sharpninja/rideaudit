// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using RideAudit.Client.Core;
using RideAudit.Client.Tests.Support;
using RideAudit.PlayIntegrity;
using RideAudit.Client.Seal;
using Xunit;

namespace RideAudit.Client.Tests;

[Trait("Partition", "TEST-RIDE-013")]
public class TestRide013SealTests
{
    [Fact]
    [Trait("FR", "FR-RIDE-015")]
    [Trait("AC", "AC-RIDE-015-001")]
    public void Seal_happens_before_durable_store()
    {
        var fixture = Fixtures.CaptureHappy();
        var record = fixture.Capture.SealedComposite;
        Assert.Equal(HandoffStage.DurableStore, record.SealedBefore);
        Assert.False(record.PlaintextRetained);
        Assert.True(CollectionSealer.LooksSealed(record.Envelope));
        Assert.False(Bytes.Contains(record.Envelope, System.Text.Encoding.ASCII.GetBytes(Fixtures.Marker)));
    }

    [Fact]
    [Trait("FR", "FR-RIDE-015")]
    [Trait("AC", "AC-RIDE-015-002")]
    public void Algorithm_version_and_hashes_are_recorded()
    {
        var record = Fixtures.CaptureHappy().Capture.SealedComposite;
        Assert.Equal(AdmissionPolicy.Algorithm, record.Receipt.Algorithm);
        Assert.Equal(AdmissionPolicy.AlgorithmVersion, record.Receipt.AlgorithmVersion);
        Assert.Equal(Hashes.Sha256Hex(record.Envelope), record.Receipt.ContentHash);
        Assert.False(string.IsNullOrWhiteSpace(record.Receipt.PlaintextContentHash));
    }

    [Fact]
    [Trait("FR", "FR-RIDE-016")]
    [Trait("AC", "AC-RIDE-016-001")]
    public void Session_scope_reuses_key_id_and_sample_scope_does_not()
    {
        var clock = new FixedClock(DateTimeOffset.Parse("2026-09-28T12:00:00Z"));
        var sealer = new CollectionSealer(clock);
        var (publicKey, _) = EscrowKeyFactory.CreateEphemeral("escrow");
        var auth = Authorize();
        var first = Seal(sealer, publicKey, auth, KeyScope.Session, "session-a", "rec-1");
        var second = Seal(sealer, publicKey, auth, KeyScope.Session, "session-a", "rec-2");
        var sampleA = Seal(sealer, publicKey, auth, KeyScope.Sample, "sample-a", "rec-3");
        var sampleB = Seal(sealer, publicKey, auth, KeyScope.Sample, "sample-b", "rec-4");
        Assert.Equal(first.Receipt.KeyId, second.Receipt.KeyId);
        Assert.Equal("session", first.Receipt.KeyScope);
        Assert.NotEqual(sampleA.Receipt.KeyId, sampleB.Receipt.KeyId);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-016")]
    [Trait("AC", "AC-RIDE-016-002")]
    public void Long_lived_shared_key_is_rejected()
    {
        var sealer = new CollectionSealer(new FixedClock(DateTimeOffset.UnixEpoch));
        var (publicKey, _) = EscrowKeyFactory.CreateEphemeral("escrow");
        var ex = Assert.Throws<RideAuditFailClosedException>(() =>
            Seal(sealer, publicKey, Authorize(), KeyScope.Session, ScopedKeyGenerator.ForbiddenScopeId, "rec"));
        Assert.Equal("KEY_SCOPE_REJECTED", ex.Code);
        Assert.Equal("FR-RIDE-016", ex.RequirementId);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-016")]
    [Trait("AC", "AC-RIDE-016-003")]
    public void Key_binding_includes_declared_scope()
    {
        var record = Fixtures.CaptureHappy().Capture.SealedComposite;
        var expected = ScopedKeyGenerator.BindingDigest(
            record.Receipt.KeyId,
            record.Receipt.Nonce,
            record.Receipt.PackageIdentity,
            record.Receipt.SigningCertDigest,
            record.Receipt.ScopeId);
        Assert.Equal(expected, record.Receipt.KeyBindingDigest);
        Assert.Equal(record.Receipt.KeyId, record.WrappedKey.KeyId);
        Assert.Equal(record.Receipt.ScopeId, record.WrappedKey.ScopeId);
    }

    [Fact]
        [Trait("TR", "TR-RIDE-STORE-001")]
        [Trait("AC", "AC-RIDE-STORE-001-002")]
    public void Store_rejects_plaintext_and_unsealed_handoff()
    {
        var store = new DeviceBoundaryStore();
        var record = Fixtures.CaptureHappy().Capture.SealedComposite;
        var unsealed = new SealedRecord
        {
            Id = "bad",
            Envelope = System.Text.Encoding.UTF8.GetBytes("video/mp4 plaintext"),
            Receipt = record.Receipt,
            WrappedKey = record.WrappedKey,
            Kind = EvidenceKind.SensorSample,
            SealedBefore = HandoffStage.DurableStore,
        };
        var ex = Assert.Throws<RideAuditFailClosedException>(() => store.Commit(unsealed));
        Assert.Equal("PLAINTEXT_REJECTED", ex.Code);
    }

    [Fact]
        [Trait("TR", "TR-RIDE-STORE-002")]
        [Trait("AC", "AC-RIDE-STORE-002-001")]
        [Trait("AC", "AC-RIDE-STORE-002-002")]
    public void Corrections_create_a_new_version_and_keep_the_original()
    {
        var store = new DeviceBoundaryStore();
        var record = Fixtures.CaptureHappy().Capture.SealedComposite;
        store.Commit(record);
        var replacement = new SealedRecord
        {
            Id = record.Id + "-v2",
            Envelope = record.Envelope.ToArray(),
            Receipt = record.Receipt,
            WrappedKey = record.WrappedKey,
            Kind = record.Kind,
            SealedBefore = HandoffStage.DurableStore,
        };
        var corrected = store.Correct(record.Id, replacement);
        Assert.Equal(2, corrected.Version);
        Assert.Equal(record.Id, corrected.SupersedesId);
        Assert.True(store.TryGet(record.Id, out var original));
        Assert.Equal(record.Receipt.ContentHash, original.Receipt.ContentHash);
        Assert.Throws<RideAuditFailClosedException>(() => store.Commit(record));
    }

    private static PlayAuthorization Authorize()
    {
        var clock = new FixedClock(DateTimeOffset.Parse("2026-09-28T12:00:00Z"));
        var gate = new PlayIntegrityGate(new FixturePlayIntegrityClient(), PackageAllowlist.CreateDevelopmentDefault(), clock);
        return gate.AuthorizeKeyGeneration(AttestationRequest.Create("collector"));
    }

    private static SealedRecord Seal(
        CollectionSealer sealer,
        EscrowPublicKey key,
        PlayAuthorization auth,
        KeyScope scope,
        string scopeId,
        string recordId) =>
        sealer.Seal(new SealRequest
        {
            Plaintext = System.Text.Encoding.UTF8.GetBytes("sensor-" + recordId),
            SessionId = "session-a",
            RecordId = recordId,
            Scope = scope,
            ScopeId = scopeId,
            Authorization = auth,
            EscrowKey = key,
            CollectorIdentity = "collector",
            ProvenanceTag = "sensor",
            Kind = EvidenceKind.SensorSample,
            DeviceIds = ["device-1"],
            SourceCommitNotice = "test-commit",
        });
}
