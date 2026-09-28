// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Text;
using Google.Protobuf;
using RideAudit.Attest;
using RideAudit.Client.Contracts;
using RideAudit.Client.Core;
using RideAudit.Client.Seal;
using RideAudit.Contracts;
using RideAudit.PlayIntegrity;
using RideAudit.Protos.Admission.V1;
using RideAudit.Protos.Custody.V1;
using RideAudit.Seal;
using AttestationEvidence = RideAudit.PlayIntegrity.AttestationEvidence;

namespace RideAudit.Capture;

public interface IAdmissionRequestFactory
{
    SubmitSealedRequest Create(SealedRecord record, CaptureRequest request, AttestationEvidence evidence);
}

public interface IDeviceEscrowDeposit
{
    void Deposit(SealedRecord record);
}

public sealed class PreflightAdmissionRequestFactory : IAdmissionRequestFactory
{
    public static PreflightAdmissionRequestFactory Instance { get; } = new();

    public SubmitSealedRequest Create(SealedRecord record, CaptureRequest request, AttestationEvidence evidence) =>
        SubmissionMapper.ToRequest(record, request.VehicleId, evidence);
}

/// <summary>
/// Production client→server admission contract. Tenant, driver, policy, and the raw
/// Play token material must be present. This factory does not mint fixture.v1. tokens
/// and does not send a device-retained token hash as Attestation.Token.
/// </summary>
public sealed class CanonicalAdmissionIdentity
{
    public required string TenantId { get; init; }
    public required string DriverId { get; init; }
    public required string PolicyVersion { get; init; }

    public static CanonicalAdmissionIdentity FromEnvironment() =>
        new()
        {
            TenantId = Environment.GetEnvironmentVariable("RIDEAUDIT_TENANT_ID") ?? "",
            DriverId = Environment.GetEnvironmentVariable("RIDEAUDIT_DRIVER_ID") ?? "",
            PolicyVersion = Environment.GetEnvironmentVariable("RIDEAUDIT_POLICY_VERSION") ?? RideAuditPolicy.Version
        };

    public void EnsureReady()
    {
        if (string.IsNullOrWhiteSpace(TenantId) || string.IsNullOrWhiteSpace(DriverId))
        {
            throw new RideAuditFailClosedException(
                ErrorCodes.AuthRequired,
                "FR-RIDE-035",
                "Canonical admission requires tenant and driver identity. Preflight hash-only mapping is not the production contract.");
        }

        if (string.IsNullOrWhiteSpace(PolicyVersion))
        {
            throw new RideAuditFailClosedException(
                ErrorCodes.PolicyMismatch,
                "FR-RIDE-035",
                "Canonical admission requires the server policy version.");
        }
    }
}

public sealed class CanonicalAdmissionRequestFactory : IAdmissionRequestFactory
{
    private readonly CanonicalAdmissionIdentity _identity;

    public CanonicalAdmissionRequestFactory(CanonicalAdmissionIdentity identity) => _identity = identity;

    public static CanonicalAdmissionRequestFactory FromEnvironment() =>
        new(CanonicalAdmissionIdentity.FromEnvironment());

    public SubmitSealedRequest Create(SealedRecord record, CaptureRequest request, AttestationEvidence evidence)
    {
        var identity = new CanonicalAdmissionIdentity
        {
            TenantId = FirstNonEmpty(request.TenantId, _identity.TenantId),
            DriverId = FirstNonEmpty(request.DriverId, _identity.DriverId),
            PolicyVersion = FirstNonEmpty(request.PolicyVersion, _identity.PolicyVersion)
        };
        identity.EnsureReady();
        if (string.IsNullOrWhiteSpace(evidence.RawTokenMaterial))
        {
            throw new RideAuditFailClosedException(
                ErrorCodes.AttestationFailed,
                "FR-RIDE-026",
                "Canonical admission requires raw attestation token material. A token hash is not sufficient.");
        }

        var nonce = evidence.Nonce;
        var token = evidence.RawTokenMaterial;
        var evidenceHash = PlayIntegrityVerifier.BindEvidence(token, record.Receipt.KeyId, record.Receipt.SessionId, nonce);
        var built = ReceiptCoreCodec.Build(new ReceiptCoreInput
        {
            PolicyVersion = identity.PolicyVersion,
            ContentHash = HexToBytes(record.Receipt.ContentHash),
            AlgorithmId = RideAuditPolicy.AlgorithmId,
            KeyId = record.Receipt.KeyId,
            PublicKeyMaterial = Encoding.UTF8.GetBytes(record.Receipt.PublicKeyPem),
            KeyScope = record.Receipt.KeyScope,
            ScopeBinding = record.Receipt.ScopeId,
            CollectorId = record.Receipt.CollectorIdentity,
            DriverId = identity.DriverId,
            VehicleId = request.VehicleId,
            SessionId = record.Receipt.SessionId,
            TenantId = identity.TenantId,
            CollectionUnixMillis = record.Receipt.SealedAt.ToUnixTimeMilliseconds(),
            ProvenanceTag = record.Receipt.ProvenanceTag,
            AttestationEvidenceHash = evidenceHash,
            PackageIdentity = evidence.PackageIdentity,
            SigningCertDigest = evidence.SigningCertDigest,
            Nonce = nonce,
            SealedRecordId = record.Id
        });

        return new SubmitSealedRequest
        {
            SessionId = record.Receipt.SessionId,
            VehicleId = request.VehicleId,
            IdempotencyKey = "idem-" + record.Id,
            ContentType = RideAuditPolicy.SealedContentType,
            SealedEnvelope = ByteString.CopyFrom(record.Envelope),
            ReceiptCore = built.Core,
            Attestation = new AttestationSubmission
            {
                Provider = evidence.Provider,
                Token = token,
                Nonce = nonce,
                ObtainedUnixMillis = evidence.ObtainedAt.ToUnixTimeMilliseconds(),
                PackageName = evidence.PackageIdentity,
                CertDigest = evidence.SigningCertDigest,
                BoundKeyId = record.Receipt.KeyId,
                BoundSessionId = record.Receipt.SessionId
            }
        };
    }

    private static string FirstNonEmpty(string? preferred, string fallback) =>
        string.IsNullOrWhiteSpace(preferred) ? fallback : preferred;

    private static byte[] HexToBytes(string hex)
    {
        if (string.IsNullOrEmpty(hex) || hex.Length % 2 != 0)
        {
            throw new RideAuditFailClosedException(
                "RECEIPT_INVALID",
                "FR-RIDE-017",
                "Receipt hash is not hex.");
        }

        var bytes = new byte[hex.Length / 2];
        for (var i = 0; i < bytes.Length; i++)
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        return bytes;
    }
}

/// <summary>
/// Builds a server-admissible RAES request using a documented fixture.v1. token,
/// ReceiptCoreCodec, and the enrolled tenant/driver/session. Not a live Play Integrity JWT.
/// Explicit test-only inject — not the production composition default.
/// </summary>
public sealed class ServerAdmissionBinding
{
    public required string TenantId { get; init; }
    public required string DriverId { get; init; }
    public required string PolicyVersion { get; init; }
    public required string PackageIdentity { get; init; }
    public required string SigningCertDigest { get; init; }
    public required Func<string, string, string> IssueToken { get; init; }
    public required long ObtainedUnixMillis { get; init; }
}

public sealed class ServerAdmissionRequestFactory : IAdmissionRequestFactory
{
    private readonly ServerAdmissionBinding _binding;

    public ServerAdmissionRequestFactory(ServerAdmissionBinding binding) => _binding = binding;

    public SubmitSealedRequest Create(SealedRecord record, CaptureRequest request, AttestationEvidence evidence)
    {
        var nonce = evidence.Nonce;
        var token = _binding.IssueToken(record.Receipt.SessionId, nonce);
        var evidenceHash = PlayIntegrityVerifier.BindEvidence(token, record.Receipt.KeyId, record.Receipt.SessionId, nonce);
        var built = ReceiptCoreCodec.Build(new ReceiptCoreInput
        {
            PolicyVersion = _binding.PolicyVersion,
            ContentHash = HexToBytes(record.Receipt.ContentHash),
            AlgorithmId = RideAuditPolicy.AlgorithmId,
            KeyId = record.Receipt.KeyId,
            PublicKeyMaterial = Encoding.UTF8.GetBytes(record.Receipt.PublicKeyPem),
            KeyScope = record.Receipt.KeyScope,
            ScopeBinding = record.Receipt.ScopeId,
            CollectorId = record.Receipt.CollectorIdentity,
            DriverId = _binding.DriverId,
            VehicleId = request.VehicleId,
            SessionId = record.Receipt.SessionId,
            TenantId = _binding.TenantId,
            CollectionUnixMillis = record.Receipt.SealedAt.ToUnixTimeMilliseconds(),
            ProvenanceTag = record.Receipt.ProvenanceTag,
            AttestationEvidenceHash = evidenceHash,
            PackageIdentity = _binding.PackageIdentity,
            SigningCertDigest = _binding.SigningCertDigest,
            Nonce = nonce,
            SealedRecordId = record.Id
        });

        return new SubmitSealedRequest
        {
            SessionId = record.Receipt.SessionId,
            VehicleId = request.VehicleId,
            IdempotencyKey = "idem-" + record.Id,
            ContentType = RideAuditPolicy.SealedContentType,
            SealedEnvelope = ByteString.CopyFrom(record.Envelope),
            ReceiptCore = built.Core,
            Attestation = new AttestationSubmission
            {
                Provider = RideAuditPolicy.PlayProvider,
                Token = token,
                Nonce = nonce,
                ObtainedUnixMillis = _binding.ObtainedUnixMillis,
                PackageName = _binding.PackageIdentity,
                CertDigest = _binding.SigningCertDigest,
                BoundKeyId = record.Receipt.KeyId,
                BoundSessionId = record.Receipt.SessionId
            }
        };
    }

    private static byte[] HexToBytes(string hex)
    {
        if (string.IsNullOrEmpty(hex) || hex.Length % 2 != 0)
        {
            throw new RideAuditFailClosedException(
                "RECEIPT_INVALID",
                "FR-RIDE-017",
                "Receipt hash is not hex.");
        }

        var bytes = new byte[hex.Length / 2];
        for (var i = 0; i < bytes.Length; i++)
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        return bytes;
    }
}
