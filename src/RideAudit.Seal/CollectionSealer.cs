// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Security.Cryptography;
using RideAudit.Client.Core;
using RideAudit.PlayIntegrity;

namespace RideAudit.Seal;

public sealed class SealRequest
{
    public required byte[] Plaintext { get; init; }
    public required string SessionId { get; init; }
    public required string RecordId { get; init; }
    public required KeyScope Scope { get; init; }
    public required string ScopeId { get; init; }
    public required PlayAuthorization Authorization { get; init; }
    public required EscrowPublicKey EscrowKey { get; init; }
    public required string CollectorIdentity { get; init; }
    public required string ProvenanceTag { get; init; }
    public required EvidenceKind Kind { get; init; }
    public required IReadOnlyList<string> DeviceIds { get; init; }
    public CompositeMetadata? Composite { get; init; }
    public string? LinkedCompositeId { get; init; }
    public string SourceCommitNotice { get; init; } = "workspace";
}

public sealed class CollectionSealer
{
    public const byte EnvelopeVersion = 1;

    private readonly IClock _clock;
    private readonly ScopedKeyGenerator _keys;

    public CollectionSealer(IClock clock, ScopedKeyGenerator? keys = null)
    {
        _clock = clock;
        _keys = keys ?? new ScopedKeyGenerator();
    }

    public void DiscardSessionKeys() => _keys.DiscardSessionKeys();

    public SealedRecord Seal(SealRequest request)
    {
        if (request.Plaintext.Length == 0)
        {
            throw new RideAuditFailClosedException(
                "VALIDATION_FAILED",
                "FR-RIDE-015",
                "Refusing to seal an empty payload.");
        }

        if (!request.Authorization.Accepted || request.Authorization.Evidence is null)
        {
            throw new RideAuditFailClosedException(
                "ATTESTATION_FAILED",
                "FR-RIDE-026",
                "Integrity check must pass before sealing.");
        }

        if (request.Kind == EvidenceKind.Composite && request.Composite is null)
        {
            throw new RideAuditFailClosedException(
                "VALIDATION_FAILED",
                "FR-RIDE-048",
                "Composite seal requires composite metadata.");
        }

        var evidence = request.Authorization.Evidence;
        var (keyId, dek, binding) = _keys.Create(request.Scope, request.ScopeId, request.Authorization);
        try
        {
            var plaintextHash = Hashes.Sha256Hex(request.Plaintext);
            var nonce = RandomNumberGenerator.GetBytes(12);
            var ciphertext = new byte[request.Plaintext.Length];
            var tag = new byte[16];
            using (var aes = new AesGcm(dek, 16))
            {
                aes.Encrypt(nonce, request.Plaintext, ciphertext, tag);
            }

            var envelope = BuildEnvelope(nonce, tag, ciphertext);
            var contentHash = Hashes.Sha256Hex(envelope);
            var wrapped = WrapDek(request.EscrowKey.PublicKeyPem, dek);
            var scheme = request.Scope == KeyScope.Session
                ? AdmissionPolicy.KeySchemeSession
                : AdmissionPolicy.KeySchemeSample;

            var receipt = new CustodyReceipt
            {
                SessionId = request.SessionId,
                SealedRecordId = request.RecordId,
                ContentHash = contentHash,
                PlaintextContentHash = plaintextHash,
                SealedAt = _clock.UtcNow,
                AttestationTokenHash = evidence.TokenHash,
                KeyScheme = scheme,
                Composite = request.Kind == EvidenceKind.Composite,
                DeviceIds = request.DeviceIds,
                ChainId = AdmissionPolicy.ChainId,
                BlockchainTxHint = "",
                ChainWriteStatus = "pending-server-write",
                PackageIdentity = evidence.PackageIdentity,
                SigningCertDigest = evidence.SigningCertDigest,
                AttestationReference = evidence.TokenHash,
                AttestationProvider = evidence.Provider,
                KeyId = keyId,
                PublicKeyPem = request.EscrowKey.PublicKeyPem,
                CollectorIdentity = request.CollectorIdentity,
                ProvenanceTag = request.ProvenanceTag,
                Algorithm = AdmissionPolicy.Algorithm,
                AlgorithmVersion = AdmissionPolicy.AlgorithmVersion,
                KeyScope = request.Scope.ToString().ToLowerInvariant(),
                ScopeId = request.ScopeId,
                KeyBindingDigest = binding,
                Nonce = evidence.Nonce,
                AdmissionPolicyId = AdmissionPolicy.PolicyId,
                CompositeMetadata = request.Composite,
                LinkedCompositeId = request.LinkedCompositeId,
                License = LicenseMetadata.ForArtifact(request.SourceCommitNotice),
                StubNotice = evidence.StubNotice,
            };

            return new SealedRecord
            {
                Id = request.RecordId,
                Envelope = envelope,
                Receipt = receipt,
                WrappedKey = new WrappedDek(keyId, request.Scope, request.ScopeId, binding, wrapped),
                Kind = request.Kind,
                SealedBefore = HandoffStage.DurableStore,
            };
        }
        finally
        {
            Hashes.Zero(dek);
            Hashes.Zero(request.Plaintext);
        }
    }

    public static byte[] BuildEnvelope(byte[] nonce, byte[] tag, byte[] ciphertext)
    {
        var envelope = new byte[4 + 1 + nonce.Length + tag.Length + ciphertext.Length];
        envelope[0] = (byte)'R';
        envelope[1] = (byte)'A';
        envelope[2] = (byte)'E';
        envelope[3] = (byte)'S';
        envelope[4] = EnvelopeVersion;
        nonce.CopyTo(envelope.AsSpan(5));
        tag.CopyTo(envelope.AsSpan(17));
        ciphertext.CopyTo(envelope.AsSpan(33));
        return envelope;
    }

    public static bool LooksSealed(ReadOnlySpan<byte> bytes) =>
        bytes.Length > 33 &&
        bytes[0] == (byte)'R' &&
        bytes[1] == (byte)'A' &&
        bytes[2] == (byte)'E' &&
        bytes[3] == (byte)'S' &&
        bytes[4] == EnvelopeVersion;

    public static byte[] DecryptEnvelope(byte[] envelope, byte[] dek)
    {
        if (!LooksSealed(envelope))
        {
            throw new RideAuditFailClosedException(
                "PLAINTEXT_REJECTED",
                "FR-RIDE-015",
                "Envelope is not a RideAudit sealed payload.");
        }

        var nonce = envelope.AsSpan(5, 12);
        var tag = envelope.AsSpan(17, 16);
        var ciphertext = envelope.AsSpan(33);
        var plaintext = new byte[ciphertext.Length];
        using var aes = new AesGcm(dek, 16);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);
        return plaintext;
    }

    private static byte[] WrapDek(string publicKeyPem, byte[] dek)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(publicKeyPem);
        return rsa.Encrypt(dek, RSAEncryptionPadding.OaepSHA256);
    }
}

public static class EscrowKeyFactory
{
    public static (EscrowPublicKey PublicKey, RSA Private) CreateEphemeral(string keyId)
    {
        var rsa = RSA.Create(2048);
        var pem = rsa.ExportRSAPublicKeyPem();
        return (new EscrowPublicKey(keyId, pem), rsa);
    }

    public static byte[] Unwrap(RSA privateKey, byte[] wrapped) =>
        privateKey.Decrypt(wrapped, RSAEncryptionPadding.OaepSHA256);
}
