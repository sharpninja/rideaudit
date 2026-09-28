using System.Security.Cryptography;
using RideAudit.Attest;
using RideAudit.Contracts;

namespace RideAudit.Seal;

public sealed class SealRequest
{
    public required byte[] Plaintext { get; init; }
    public required string KeyScope { get; init; }
    public required string ScopeBinding { get; init; }
    public required string TenantId { get; init; }
    public required string DriverId { get; init; }
    public required string CollectorId { get; init; }
    public required string VehicleId { get; init; }
    public required string SessionId { get; init; }
    public required string ProvenanceTag { get; init; }
    public required string PolicyVersion { get; init; }
    public required string AttestationToken { get; init; }
    public required string ExpectedNonce { get; init; }
    public required IReadOnlyList<string> CustodianIds { get; init; }
    public int ThresholdM { get; init; } = 2;
    public int TotalN { get; init; } = 3;
}

public sealed class SealedPackage
{
    public required string SealedRecordId { get; init; }
    public required byte[] EnvelopeBytes { get; init; }
    public required byte[] Ciphertext { get; init; }
    public required byte[] ReceiptCoreBytes { get; init; }
    public required byte[] ReceiptCoreDigest { get; init; }
    public required string KeyId { get; init; }
    public required string AlgorithmId { get; init; }
    public required string ContentType { get; init; }
    public required ReceiptCoreBuild Receipt { get; init; }
}

public interface ISealStore
{
    void Put(string tenantId, string sealedRecordId, byte[] ciphertext, byte[] receiptCoreBytes);
    int PutCount { get; }
}

public sealed class InMemorySealStore : ISealStore
{
    private readonly Dictionary<string, (byte[] Cipher, byte[] Receipt)> _items = new(StringComparer.Ordinal);

    public int PutCount { get; private set; }

    public void Put(string tenantId, string sealedRecordId, byte[] ciphertext, byte[] receiptCoreBytes)
    {
        var key = tenantId + "|" + sealedRecordId;
        if (_items.TryGetValue(key, out var existing))
        {
            if (!existing.Cipher.AsSpan().SequenceEqual(ciphertext))
                throw new RideAuditException(ErrorCodes.ReceiptInvalid, "Sealed ciphertext cannot be rewritten.");
            return;
        }
        _items[key] = (ciphertext.ToArray(), receiptCoreBytes.ToArray());
        PutCount++;
    }

    public byte[]? Ciphertext(string tenantId, string sealedRecordId)
    {
        var key = tenantId + "|" + sealedRecordId;
        return _items.TryGetValue(key, out var existing) ? existing.Cipher : null;
    }
}

/// <summary>
/// FR-RIDE-015 / FR-RIDE-016 / FR-RIDE-026. Integrity is verified before key generation.
/// The returned package holds ciphertext only.
/// </summary>
public sealed class CollectionBoundarySealer
{
    private readonly PlayIntegrityVerifier _play;
    private readonly ICollectionKeySource _keys;
    private readonly IEscrowSink _escrow;
    private readonly IClock _clock;
    private readonly AlgorithmRegistry _algorithms;

    public CollectionBoundarySealer(
        PlayIntegrityVerifier play,
        ICollectionKeySource keys,
        IEscrowSink escrow,
        IClock clock,
        AlgorithmRegistry algorithms)
    {
        _play = play;
        _keys = keys;
        _escrow = escrow;
        _clock = clock;
        _algorithms = algorithms;
    }

    public SealedPackage Seal(SealRequest request)
    {
        if (!EnvelopeFormat.IsAcceptedScope(request.KeyScope))
            throw new RideAuditException(ErrorCodes.KeyScopeRejected, "Key scope must be session or sample. Shared all-record keys are rejected.");
        if (request.Plaintext is null || request.Plaintext.Length == 0)
            throw new RideAuditException(ErrorCodes.ValidationFailed, "Nothing to seal.");

        var verdict = _play.Verify(request.AttestationToken, request.ExpectedNonce, request.SessionId, boundKeyId: null);
        if (!verdict.Success || verdict.Evidence is null)
            throw new RideAuditException(verdict.Code, verdict.Message);

        var generatesBefore = _keys.GenerateCount;
        if (generatesBefore < 0)
            throw new InvalidOperationException("Key source counter is invalid.");

        using var material = _keys.Create(request.KeyScope, request.ScopeBinding);
        var evidenceHash = PlayIntegrityVerifier.BindEvidence(
            request.AttestationToken, material.KeyId, request.SessionId, request.ExpectedNonce);

        var nonce = RandomNumberGenerator.GetBytes(12);
        var cipher = new byte[request.Plaintext.Length];
        var tag = new byte[16];
        using (var aes = new AesGcm(material.Dek, 16))
        {
            aes.Encrypt(nonce, request.Plaintext, cipher, tag);
        }
        var ciphertext = new byte[cipher.Length + tag.Length];
        cipher.CopyTo(ciphertext, 0);
        tag.CopyTo(ciphertext, cipher.Length);

        var recordId = Ids.New("rec-");
        var header = new EnvelopeHeader
        {
            AlgorithmId = _algorithms.CurrentAlgorithmId,
            CollectionUnixMillis = _clock.UtcNow.ToUnixTimeMilliseconds(),
            KeyId = material.KeyId,
            KeyScope = request.KeyScope,
            ScopeBinding = request.ScopeBinding,
            AesNonceB64 = Convert.ToBase64String(nonce),
            PublicKeyB64 = Convert.ToBase64String(material.PublicKeySpki),
            SealedRecordId = recordId,
            ProvenanceTag = request.ProvenanceTag,
            PolicyVersion = request.PolicyVersion,
            CollectorId = request.CollectorId,
            SessionId = request.SessionId,
            VehicleId = request.VehicleId,
            TenantId = request.TenantId
        };
        var envelope = EnvelopeFormat.Write(header, ciphertext);
        var receipt = ReceiptCoreCodec.Build(new ReceiptCoreInput
        {
            PolicyVersion = request.PolicyVersion,
            ContentHash = Ids.Sha256(ciphertext),
            AlgorithmId = header.AlgorithmId,
            KeyId = material.KeyId,
            PublicKeyMaterial = material.PublicKeySpki,
            KeyScope = request.KeyScope,
            ScopeBinding = request.ScopeBinding,
            CollectorId = request.CollectorId,
            DriverId = request.DriverId,
            VehicleId = request.VehicleId,
            SessionId = request.SessionId,
            TenantId = request.TenantId,
            CollectionUnixMillis = header.CollectionUnixMillis,
            ProvenanceTag = request.ProvenanceTag,
            AttestationEvidenceHash = evidenceHash,
            PackageIdentity = verdict.Evidence.PackageName,
            SigningCertDigest = verdict.Evidence.CertDigest,
            Nonce = request.ExpectedNonce,
            SealedRecordId = recordId
        });

        _escrow.EscrowCollectionSecret(new EscrowSecret
        {
            KeyId = material.KeyId,
            TenantId = request.TenantId,
            SealedRecordId = recordId,
            PrivateKeyPkcs8 = material.PrivateKeyPkcs8.ToArray(),
            Dek = material.Dek.ToArray(),
            CustodianIds = request.CustodianIds,
            ThresholdM = request.ThresholdM,
            TotalN = request.TotalN
        });

        return new SealedPackage
        {
            SealedRecordId = recordId,
            EnvelopeBytes = envelope,
            Ciphertext = ciphertext,
            ReceiptCoreBytes = receipt.Bytes,
            ReceiptCoreDigest = receipt.Digest,
            KeyId = material.KeyId,
            AlgorithmId = header.AlgorithmId,
            ContentType = RideAuditPolicy.SealedContentType,
            Receipt = receipt
        };
    }
}

public sealed class SealPipeline
{
    private readonly CollectionBoundarySealer _sealer;
    private readonly ISealStore _store;

    public SealPipeline(CollectionBoundarySealer sealer, ISealStore store)
    {
        _sealer = sealer;
        _store = store;
    }

    public int LastSealSequence { get; private set; }
    public int LastStoreSequence { get; private set; }
    private int _sequence;

    public SealedPackage Collect(SealRequest request)
    {
        var package = _sealer.Seal(request);
        LastSealSequence = ++_sequence;
        if (package.EnvelopeBytes.AsSpan().IndexOf(request.Plaintext) >= 0 && request.Plaintext.Length > 8)
            throw new RideAuditException(ErrorCodes.PlaintextRejected, "Plaintext leaked into the sealed envelope.");
        _store.Put(request.TenantId, package.SealedRecordId, package.Ciphertext, package.ReceiptCoreBytes);
        LastStoreSequence = ++_sequence;
        return package;
    }
}
