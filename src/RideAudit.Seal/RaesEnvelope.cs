using System.Security.Cryptography;
using Google.Protobuf;
using RideAudit.Contracts;
using RideAudit.Protos.Custody.V1;

namespace RideAudit.Seal;

/// <summary>
/// Device RAES envelopes (magic RAES, version, 12-byte nonce, 16-byte tag, ciphertext).
/// Admission commits the untouched envelope bytes. It does not re-encode them as RIDESEAL1.
/// Court working-copy decrypt may open an already-escrowed RAES envelope after M-of-N release.
/// </summary>
public static class RaesEnvelopeFormat
{
    public static ReadOnlySpan<byte> Magic => "RAES"u8;
    public const byte Version = 1;
    public const int PrefixLength = 33;

    public static bool HasMagic(ReadOnlySpan<byte> bytes) =>
        bytes.Length >= Magic.Length && bytes.StartsWith(Magic);

    public static ParsedEnvelope Bind(
        byte[] envelope,
        byte[] submittedReceiptBytes,
        string policyVersion,
        string tenantId,
        string sessionId,
        string vehicleId)
    {
        var span = envelope.AsSpan();
        if (!HasMagic(span))
            throw new RideAuditException(ErrorCodes.PlaintextRejected, "Payload is not a device RAES envelope.");
        if (span.Length < PrefixLength + 1)
            throw new RideAuditException(ErrorCodes.ReceiptInvalid, "Device RAES envelope is truncated.");
        if (span[4] != Version)
            throw new RideAuditException(ErrorCodes.ReceiptInvalid, "Unsupported device RAES envelope version.");

        ReceiptCore submitted;
        try
        {
            submitted = ReceiptCore.Parser.ParseFrom(submittedReceiptBytes);
        }
        catch (InvalidProtocolBufferException)
        {
            throw new RideAuditException(ErrorCodes.ReceiptInvalid, "Custody receipt core is not parseable.");
        }

        var contentHash = Ids.Sha256(envelope);
        if (submitted.ContentHash.IsEmpty || !submitted.ContentHash.Span.SequenceEqual(contentHash))
            throw new RideAuditException(ErrorCodes.ReceiptInvalid, "RAES content hash does not match the sealed envelope.");

        if (!string.Equals(submitted.PolicyVersion, policyVersion, StringComparison.Ordinal))
            throw new RideAuditException(ErrorCodes.PolicyMismatch, "Receipt policy version is not the active admission policy.");
        if (!string.Equals(submitted.TenantId, tenantId, StringComparison.Ordinal))
            throw new RideAuditException(ErrorCodes.TenantIsolation, "Sealed envelope tenant does not match the caller.");
        if (!string.Equals(submitted.SessionId, sessionId, StringComparison.Ordinal)
            || !string.Equals(submitted.VehicleId, vehicleId, StringComparison.Ordinal))
            throw new RideAuditException(ErrorCodes.ReceiptInvalid, "Envelope session or vehicle does not match the submission.");
        if (!EnvelopeFormat.IsAcceptedScope(submitted.KeyScope))
            throw new RideAuditException(ErrorCodes.KeyScopeRejected, "Receipt key scope is not session or sample.");

        new AlgorithmRegistry(RideAuditPolicy.AlgorithmId).EnsureKnown(submitted.AlgorithmId);
        if (submitted.PublicKeyMaterial.IsEmpty)
            throw new RideAuditException(ErrorCodes.ReceiptInvalid, "Receipt public key is missing.");
        Require(submitted.KeyId, "key");
        Require(submitted.ScopeBinding, "scope");
        Require(submitted.CollectorId, "collector");
        Require(submitted.ProvenanceTag, "provenance");
        Require(submitted.SealedRecordId, "record");
        if (submitted.CollectionUnixMillis <= 0)
            throw new RideAuditException(ErrorCodes.ReceiptInvalid, "Receipt collection time is missing.");

        return new ParsedEnvelope
        {
            Header = new EnvelopeHeader
            {
                V = Version,
                AlgorithmId = submitted.AlgorithmId,
                CollectionUnixMillis = submitted.CollectionUnixMillis,
                KeyId = submitted.KeyId,
                KeyScope = submitted.KeyScope,
                ScopeBinding = submitted.ScopeBinding,
                PublicKeyB64 = Convert.ToBase64String(submitted.PublicKeyMaterial.ToByteArray()),
                SealedRecordId = submitted.SealedRecordId,
                ProvenanceTag = submitted.ProvenanceTag,
                PolicyVersion = submitted.PolicyVersion,
                CollectorId = submitted.CollectorId,
                SessionId = submitted.SessionId,
                VehicleId = submitted.VehicleId,
                TenantId = submitted.TenantId
            },
            Ciphertext = envelope.ToArray(),
            ContentHash = contentHash
        };
    }

    public static byte[] Open(byte[] envelope, byte[] dek)
    {
        var span = envelope.AsSpan();
        if (!HasMagic(span))
            throw new RideAuditException(ErrorCodes.PlaintextRejected, "Payload is not a device RAES envelope.");
        if (span.Length < PrefixLength + 1)
            throw new RideAuditException(ErrorCodes.ReceiptInvalid, "Device RAES envelope is truncated.");
        if (span[4] != Version)
            throw new RideAuditException(ErrorCodes.ReceiptInvalid, "Unsupported device RAES envelope version.");

        var nonce = span.Slice(5, 12);
        var tag = span.Slice(17, 16);
        var data = span[PrefixLength..];
        var plain = new byte[data.Length];
        using var aes = new AesGcm(dek, 16);
        aes.Decrypt(nonce, data, tag, plain);
        return plain;
    }

    private static void Require(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new RideAuditException(ErrorCodes.ReceiptInvalid, "Receipt field is missing: " + name);
    }
}

public static class SealedIngest
{
    public static bool IsRecognizedSeal(ReadOnlySpan<byte> bytes) =>
        bytes.StartsWith(EnvelopeFormat.Magic) || RaesEnvelopeFormat.HasMagic(bytes);

    public static ParsedEnvelope Parse(
        byte[] envelope,
        byte[] submittedReceiptBytes,
        string policyVersion,
        string tenantId,
        string sessionId,
        string vehicleId)
    {
        if (envelope.AsSpan().StartsWith(EnvelopeFormat.Magic))
            return EnvelopeFormat.Parse(envelope);
        if (RaesEnvelopeFormat.HasMagic(envelope))
            return RaesEnvelopeFormat.Bind(envelope, submittedReceiptBytes, policyVersion, tenantId, sessionId, vehicleId);
        throw new RideAuditException(ErrorCodes.PlaintextRejected, "Payload is not a RideAudit sealed envelope.");
    }
}
