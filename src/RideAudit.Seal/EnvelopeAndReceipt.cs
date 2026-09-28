using Google.Protobuf;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using RideAudit.Attest;
using RideAudit.Contracts;
using RideAudit.Protos.Custody.V1;

namespace RideAudit.Seal;

public sealed class EnvelopeHeader
{
    public int V { get; set; } = 1;
    public string AlgorithmId { get; set; } = "";
    public long CollectionUnixMillis { get; set; }
    public string KeyId { get; set; } = "";
    public string KeyScope { get; set; } = "";
    public string ScopeBinding { get; set; } = "";
    public string AesNonceB64 { get; set; } = "";
    public string PublicKeyB64 { get; set; } = "";
    public string SealedRecordId { get; set; } = "";
    public string ProvenanceTag { get; set; } = "";
    public string PolicyVersion { get; set; } = "";
    public string CollectorId { get; set; } = "";
    public string SessionId { get; set; } = "";
    public string VehicleId { get; set; } = "";
    public string TenantId { get; set; } = "";
}

public sealed class ParsedEnvelope
{
    public required EnvelopeHeader Header { get; init; }
    public required byte[] Ciphertext { get; init; }
    public required byte[] ContentHash { get; init; }
}

public static class PlaintextDetector
{
    public static bool ContentTypeIsPlain(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return false;
        if (string.Equals(contentType, RideAuditPolicy.SealedContentType, StringComparison.OrdinalIgnoreCase))
            return false;
        return contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase)
            || contentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase)
            || contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
            || contentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase)
            || string.Equals(contentType, "application/json", StringComparison.OrdinalIgnoreCase);
    }

    public static bool BodyLooksLikeMedia(ReadOnlySpan<byte> body)
    {
        if (body.Length >= 3 && body[0] == 0xFF && body[1] == 0xD8 && body[2] == 0xFF)
            return true;
        if (body.Length >= 8 && body[0] == 0x89 && body[1] == 0x50 && body[2] == 0x4E && body[3] == 0x47)
            return true;
        if (body.Length >= 12 && body[4] == (byte)'f' && body[5] == (byte)'t' && body[6] == (byte)'y' && body[7] == (byte)'p')
            return true;
        if (body.Length >= 4 && body[0] == 0x1A && body[1] == 0x45 && body[2] == 0xDF && body[3] == 0xA3)
            return true;
        if (body.Length >= 4 && body[0] == (byte)'R' && body[1] == (byte)'I' && body[2] == (byte)'F' && body[3] == (byte)'F')
            return true;
        if (body.Length >= 1 && (body[0] == (byte)'{' || body[0] == (byte)'<'))
            return true;
        return false;
    }
}

public static class EnvelopeFormat
{
    public static ReadOnlySpan<byte> Magic => "RIDESEAL1"u8;

    public static bool IsAcceptedScope(string? scope) =>
        scope is "session" or "sample";

    public static byte[] Write(EnvelopeHeader header, byte[] ciphertext)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(header);
        var buffer = new byte[Magic.Length + 1 + 4 + json.Length + ciphertext.Length];
        Magic.CopyTo(buffer);
        buffer[Magic.Length] = 1;
        System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(Magic.Length + 1, 4), json.Length);
        json.CopyTo(buffer.AsSpan(Magic.Length + 5));
        ciphertext.CopyTo(buffer.AsSpan(Magic.Length + 5 + json.Length));
        return buffer;
    }

    public static ParsedEnvelope Parse(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < Magic.Length + 5 || !bytes.StartsWith(Magic))
            throw new RideAuditException(ErrorCodes.PlaintextRejected, "Payload is not a RideAudit sealed envelope.");

        var version = bytes[Magic.Length];
        if (version != 1)
            throw new RideAuditException(ErrorCodes.ReceiptInvalid, "Unsupported seal envelope version.");

        var headerLength = System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(bytes.Slice(Magic.Length + 1, 4));
        if (headerLength <= 0 || Magic.Length + 5 + headerLength > bytes.Length)
            throw new RideAuditException(ErrorCodes.ReceiptInvalid, "Seal envelope header is truncated.");

        var json = bytes.Slice(Magic.Length + 5, headerLength);
        var header = JsonSerializer.Deserialize<EnvelopeHeader>(json)
            ?? throw new RideAuditException(ErrorCodes.ReceiptInvalid, "Seal envelope header is empty.");
        var ciphertext = bytes[(Magic.Length + 5 + headerLength)..].ToArray();
        if (ciphertext.Length < 16)
            throw new RideAuditException(ErrorCodes.ReceiptInvalid, "Sealed ciphertext is missing an authentication tag.");

        return new ParsedEnvelope
        {
            Header = header,
            Ciphertext = ciphertext,
            ContentHash = Ids.Sha256(ciphertext)
        };
    }
}

public sealed class ReceiptCoreInput
{
    public required string PolicyVersion { get; init; }
    public required byte[] ContentHash { get; init; }
    public required string AlgorithmId { get; init; }
    public required string KeyId { get; init; }
    public required byte[] PublicKeyMaterial { get; init; }
    public required string KeyScope { get; init; }
    public required string ScopeBinding { get; init; }
    public required string CollectorId { get; init; }
    public required string DriverId { get; init; }
    public required string VehicleId { get; init; }
    public required string SessionId { get; init; }
    public required string TenantId { get; init; }
    public required long CollectionUnixMillis { get; init; }
    public required string ProvenanceTag { get; init; }
    public required byte[] AttestationEvidenceHash { get; init; }
    public required string PackageIdentity { get; init; }
    public required string SigningCertDigest { get; init; }
    public required string Nonce { get; init; }
    public required string SealedRecordId { get; init; }
}

public sealed record ReceiptCoreBuild(ReceiptCore Core, byte[] Bytes, byte[] Digest, string ReceiptId);

public static class ReceiptCoreCodec
{
    public static ReceiptCoreBuild Build(ReceiptCoreInput input)
    {
        Require(input.PolicyVersion, "policy");
        Require(input.AlgorithmId, "algorithm");
        Require(input.KeyId, "key");
        Require(input.KeyScope, "scope");
        Require(input.CollectorId, "collector");
        Require(input.DriverId, "driver");
        Require(input.VehicleId, "vehicle");
        Require(input.SessionId, "session");
        Require(input.TenantId, "tenant");
        Require(input.ProvenanceTag, "provenance");
        Require(input.PackageIdentity, "package");
        Require(input.SigningCertDigest, "cert");
        Require(input.Nonce, "nonce");
        Require(input.SealedRecordId, "record");
        if (input.ContentHash.Length != 32 || input.AttestationEvidenceHash.Length != 32 || input.PublicKeyMaterial.Length == 0)
            throw new RideAuditException(ErrorCodes.ReceiptInvalid, "Receipt core hashes or public key are incomplete.");

        var receiptId = Ids.Hex(Ids.Sha256(Encoding.UTF8.GetBytes(
            Ids.Hex(input.ContentHash) + "|" + input.CollectorId + "|" + input.SessionId + "|" + input.KeyId + "|" + input.CollectionUnixMillis)));

        var core = new ReceiptCore
        {
            SchemaVersion = RideAuditPolicy.SchemaVersion,
            ReceiptId = receiptId,
            PolicyVersion = input.PolicyVersion,
            ContentHash = ByteString.CopyFrom(input.ContentHash),
            AlgorithmId = input.AlgorithmId,
            KeyId = input.KeyId,
            PublicKeyMaterial = ByteString.CopyFrom(input.PublicKeyMaterial),
            KeyScope = input.KeyScope,
            ScopeBinding = input.ScopeBinding,
            CollectorId = input.CollectorId,
            DriverId = input.DriverId,
            VehicleId = input.VehicleId,
            SessionId = input.SessionId,
            TenantId = input.TenantId,
            CollectionUnixMillis = input.CollectionUnixMillis,
            ProvenanceTag = input.ProvenanceTag,
            AttestationEvidenceHash = ByteString.CopyFrom(input.AttestationEvidenceHash),
            PackageIdentity = input.PackageIdentity,
            SigningCertDigest = input.SigningCertDigest,
            Nonce = input.Nonce,
            SealedRecordId = input.SealedRecordId
        };
        var bytes = core.ToByteArray();
        return new ReceiptCoreBuild(core, bytes, Ids.Sha256(bytes), receiptId);
    }

    private static void Require(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new RideAuditException(ErrorCodes.ReceiptInvalid, "Receipt field is missing: " + name);
    }
}

public static class AuthorizedDecryptor
{
    public static byte[] Open(byte[] envelope, byte[] dek)
    {
        if (envelope.AsSpan().StartsWith(EnvelopeFormat.Magic))
            return OpenRideSeal1(envelope, dek);
        if (RaesEnvelopeFormat.HasMagic(envelope))
            return RaesEnvelopeFormat.Open(envelope, dek);
        throw new RideAuditException(ErrorCodes.PlaintextRejected, "Payload is not a RideAudit sealed envelope.");
    }

    private static byte[] OpenRideSeal1(byte[] envelope, byte[] dek)
    {
        var parsed = EnvelopeFormat.Parse(envelope);
        var nonce = Convert.FromBase64String(parsed.Header.AesNonceB64);
        if (parsed.Ciphertext.Length < 16)
            throw new RideAuditException(ErrorCodes.ReceiptInvalid, "Ciphertext is truncated.");
        var tag = parsed.Ciphertext[^16..];
        var data = parsed.Ciphertext[..^16];
        var plain = new byte[data.Length];
        using var aes = new AesGcm(dek, 16);
        aes.Decrypt(nonce, data, tag, plain);
        return plain;
    }
}

public sealed class AlgorithmRegistry
{
    public AlgorithmRegistry(string current)
    {
        if (string.IsNullOrWhiteSpace(current))
            throw new ArgumentException("Algorithm id is required.", nameof(current));
        CurrentAlgorithmId = current;
    }

    public string CurrentAlgorithmId { get; private set; }

    public void Rotate(string newAlgorithmId)
    {
        if (string.IsNullOrWhiteSpace(newAlgorithmId) || newAlgorithmId == CurrentAlgorithmId)
            throw new RideAuditException(ErrorCodes.ValidationFailed, "Algorithm rotation requires a new identifier.");
        CurrentAlgorithmId = newAlgorithmId;
    }

    public void EnsureKnown(string algorithmId)
    {
        if (string.IsNullOrWhiteSpace(algorithmId)
            || !algorithmId.StartsWith("aes-256-gcm-sha256-v", StringComparison.Ordinal))
            throw new RideAuditException(ErrorCodes.ReceiptInvalid, "Unknown sealing algorithm.");
    }
}
