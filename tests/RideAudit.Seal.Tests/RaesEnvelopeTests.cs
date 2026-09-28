using System.Text;
using RideAudit.Contracts;
using RideAudit.Seal;

namespace RideAudit.Seal.Tests;

public class RaesEnvelopeTests
{
    [Fact]
    public void Bind_commits_the_untouched_raes_envelope()
    {
        var envelope = Envelope("raes-ciphertext-bytes");
        var receipt = ReceiptFor(envelope);
        var parsed = RaesEnvelopeFormat.Bind(
            envelope,
            receipt.Bytes,
            RideAuditPolicy.Version,
            "tenant",
            "session",
            "vehicle");

        Assert.True(parsed.Ciphertext.AsSpan().SequenceEqual(envelope));
        Assert.True(RaesEnvelopeFormat.HasMagic(parsed.Ciphertext));
        Assert.False(parsed.Ciphertext.AsSpan().StartsWith(EnvelopeFormat.Magic));
        Assert.Equal(Ids.Hex(Ids.Sha256(envelope)), Ids.Hex(parsed.ContentHash));
        Assert.Equal("key-raes", parsed.Header.KeyId);
        Assert.DoesNotContain("raes-ciphertext-bytes", parsed.Header.PublicKeyB64, StringComparison.Ordinal);
    }

    [Fact]
    public void Bind_rejects_tamper_truncation_version_scope_and_policy()
    {
        var envelope = Envelope("raes-ciphertext-bytes");
        var receipt = ReceiptFor(envelope);
        var tampered = envelope.ToArray();
        tampered[^1] ^= 0x5A;
        var hash = Assert.Throws<RideAuditException>(() => Bind(tampered, receipt.Bytes));
        Assert.Equal(ErrorCodes.ReceiptInvalid, hash.Code);

        var truncated = Assert.Throws<RideAuditException>(() => Bind(envelope.AsSpan(0, 20).ToArray(), receipt.Bytes));
        Assert.Equal(ErrorCodes.ReceiptInvalid, truncated.Code);

        var wrongVersion = Envelope("raes-ciphertext-bytes", version: 9);
        var version = Assert.Throws<RideAuditException>(() => Bind(wrongVersion, ReceiptFor(wrongVersion).Bytes));
        Assert.Equal(ErrorCodes.ReceiptInvalid, version.Code);

        var shared = Assert.Throws<RideAuditException>(() => Bind(envelope, ReceiptFor(envelope, scope: "all-records").Bytes));
        Assert.Equal(ErrorCodes.KeyScopeRejected, shared.Code);

        var policy = Assert.Throws<RideAuditException>(() => Bind(envelope, ReceiptFor(envelope, policy: "other-policy").Bytes));
        Assert.Equal(ErrorCodes.PolicyMismatch, policy.Code);
    }

    [Fact]
    public void Sealed_ingest_still_parses_rideseal1()
    {
        var world = TestSupport.ServerWorld.Create();
        var enrolled = world.Enroll();
        var ready = world.SealReady(enrolled.Driver, enrolled.Session);
        var parsed = SealedIngest.Parse(
            ready.Package.EnvelopeBytes,
            ready.Package.ReceiptCoreBytes,
            world.Options.PolicyVersion,
            enrolled.Driver.TenantId,
            enrolled.Session.SessionId,
            enrolled.Vehicle.VehicleId);

        Assert.True(ready.Package.EnvelopeBytes.AsSpan().StartsWith(EnvelopeFormat.Magic));
        Assert.Equal(ready.Package.KeyId, parsed.Header.KeyId);
        Assert.False(RaesEnvelopeFormat.HasMagic(ready.Package.EnvelopeBytes));
    }

    private static ParsedEnvelope Bind(byte[] envelope, byte[] receipt) =>
        RaesEnvelopeFormat.Bind(envelope, receipt, RideAuditPolicy.Version, "tenant", "session", "vehicle");

    private static byte[] Envelope(string body, byte version = RaesEnvelopeFormat.Version)
    {
        var payload = Encoding.UTF8.GetBytes(body);
        var envelope = new byte[RaesEnvelopeFormat.PrefixLength + payload.Length];
        RaesEnvelopeFormat.Magic.CopyTo(envelope);
        envelope[4] = version;
        payload.CopyTo(envelope.AsSpan(RaesEnvelopeFormat.PrefixLength));
        return envelope;
    }

    private static ReceiptCoreBuild ReceiptFor(byte[] envelope, string scope = "session", string policy = RideAuditPolicy.Version) =>
        ReceiptCoreCodec.Build(new ReceiptCoreInput
        {
            PolicyVersion = policy,
            ContentHash = Ids.Sha256(envelope),
            AlgorithmId = RideAuditPolicy.AlgorithmId,
            KeyId = "key-raes",
            PublicKeyMaterial = Encoding.UTF8.GetBytes("pem-public-key"),
            KeyScope = scope,
            ScopeBinding = "scope-1",
            CollectorId = "collector",
            DriverId = "driver",
            VehicleId = "vehicle",
            SessionId = "session",
            TenantId = "tenant",
            CollectionUnixMillis = 1_700_000_000_000,
            ProvenanceTag = RideAuditPolicy.ProvenanceTag,
            AttestationEvidenceHash = new byte[32],
            PackageIdentity = "app.rideaudit.capture",
            SigningCertDigest = "sha256:rideaudit-play-cert-fixture",
            Nonce = "nonce",
            SealedRecordId = "rec-raes"
        });
}
