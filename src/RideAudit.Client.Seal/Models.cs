// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using RideAudit.Client.Core;

namespace RideAudit.Client.Seal;

public enum KeyScope
{
    Session = 1,
    Sample = 2,
}

public enum EvidenceKind
{
    SensorSample = 1,
    Composite = 2,
    RawStream = 3,
}

public enum HandoffStage
{
    Collection = 0,
    DurableStore = 1,
    Queue = 2,
    Normalization = 3,
    Analysis = 4,
}

public static class AdmissionPolicy
{
    public const string PolicyId = "rideaudit-admission-v1";
    public const string ChainId = "btc-ots";
    public const string Algorithm = "AES-256-GCM";
    public const string AlgorithmVersion = "1";
    public const string KeySchemeSession = "session-aead-v1";
    public const string KeySchemeSample = "sample-aead-v1";
}

public sealed record CompositeMetadata(
    string Codec,
    string Compression,
    string OverlayManifestVersion,
    string SyncClockOffset,
    string Drift,
    string Uncertainty,
    int DroppedFrameCount,
    IReadOnlyList<string> SourceStreamIds,
    IReadOnlyList<string> SourceContentHashes,
    string TimelineManifestVersion,
    IReadOnlyList<string> DeviceIds);

public sealed class EscrowPublicKey
{
    public EscrowPublicKey(string keyId, string publicKeyPem)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(keyId);
        ArgumentException.ThrowIfNullOrWhiteSpace(publicKeyPem);
        KeyId = keyId;
        PublicKeyPem = publicKeyPem;
    }

    public string KeyId { get; }

    public string PublicKeyPem { get; }
}

public sealed class WrappedDek
{
    public WrappedDek(string keyId, KeyScope scope, string scopeId, string bindingDigest, byte[] wrappedKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(keyId);
        ArgumentException.ThrowIfNullOrWhiteSpace(scopeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(bindingDigest);
        if (wrappedKey.Length == 0)
        {
            throw new ArgumentException("Wrapped key is empty.", nameof(wrappedKey));
        }

        KeyId = keyId;
        Scope = scope;
        ScopeId = scopeId;
        BindingDigest = bindingDigest;
        WrappedKey = wrappedKey;
    }

    public string KeyId { get; }

    public KeyScope Scope { get; }

    public string ScopeId { get; }

    public string BindingDigest { get; }

    public byte[] WrappedKey { get; }
}

public sealed class CustodyReceipt
{
    public required string SessionId { get; init; }
    public required string SealedRecordId { get; init; }
    public required string ContentHash { get; init; }
    public required string PlaintextContentHash { get; init; }
    public required DateTimeOffset SealedAt { get; init; }
    public required string AttestationTokenHash { get; init; }
    public required string KeyScheme { get; init; }
    public required bool Composite { get; init; }
    public required IReadOnlyList<string> DeviceIds { get; init; }
    public required string ChainId { get; init; }
    public required string BlockchainTxHint { get; init; }
    public required string ChainWriteStatus { get; init; }
    public required string PackageIdentity { get; init; }
    public required string SigningCertDigest { get; init; }
    public required string AttestationReference { get; init; }
    public required string AttestationProvider { get; init; }
    public required string KeyId { get; init; }
    public required string PublicKeyPem { get; init; }
    public required string CollectorIdentity { get; init; }
    public required string ProvenanceTag { get; init; }
    public required string Algorithm { get; init; }
    public required string AlgorithmVersion { get; init; }
    public required string KeyScope { get; init; }
    public required string ScopeId { get; init; }
    public required string KeyBindingDigest { get; init; }
    public required string Nonce { get; init; }
    public required string AdmissionPolicyId { get; init; }
    public CompositeMetadata? CompositeMetadata { get; init; }
    public string? LinkedCompositeId { get; init; }
    public required LicenseMetadata License { get; init; }
    public string? StubNotice { get; init; }

    public string CanonicalForm()
    {
        var devices = string.Join(",", DeviceIds);
        return string.Join('\n',
            "rideaudit-receipt-v1",
            "sessionId=" + SessionId,
            "sealedRecordId=" + SealedRecordId,
            "contentHash=" + ContentHash,
            "plaintextContentHash=" + PlaintextContentHash,
            "sealedAt=" + SealedAt.ToUniversalTime().ToString("O"),
            "attestationTokenHash=" + AttestationTokenHash,
            "keyScheme=" + KeyScheme,
            "composite=" + Composite,
            "deviceIds=" + devices,
            "chainId=" + ChainId,
            "packageIdentity=" + PackageIdentity,
            "signingCertDigest=" + SigningCertDigest,
            "attestationReference=" + AttestationReference,
            "keyId=" + KeyId,
            "collectorIdentity=" + CollectorIdentity,
            "provenanceTag=" + ProvenanceTag,
            "algorithm=" + Algorithm,
            "algorithmVersion=" + AlgorithmVersion,
            "keyScope=" + KeyScope,
            "scopeId=" + ScopeId,
            "keyBindingDigest=" + KeyBindingDigest,
            "nonce=" + Nonce,
            "policy=" + AdmissionPolicyId);
    }
}

public sealed class SealedRecord
{
    public required string Id { get; init; }
    public required byte[] Envelope { get; init; }
    public required CustodyReceipt Receipt { get; init; }
    public required WrappedDek WrappedKey { get; init; }
    public required EvidenceKind Kind { get; init; }
    public required HandoffStage SealedBefore { get; init; }
    public bool PlaintextRetained => false;
    public int Version { get; init; } = 1;
    public string? SupersedesId { get; init; }
}

public sealed class OtsProof
{
    public required string CommittedDigestHex { get; init; }
    public required string BitcoinBlockHash { get; init; }
    public required IReadOnlyList<string> MerkleSiblingsHex { get; init; }
}

public sealed class BitcoinHeader
{
    public required string BlockHash { get; init; }
    public required string MerkleRoot { get; init; }
}
