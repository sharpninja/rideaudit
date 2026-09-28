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
/// Builds a server-admissible RAES request using a documented fixture.v1. token,
/// ReceiptCoreCodec, and the enrolled tenant/driver/session. Not a live Play Integrity JWT.
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
