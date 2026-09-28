// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using RideAudit.Client.Core;
using RideAudit.V1;

namespace RideAudit.Client.Contracts;

public static class ContractProvenance
{
    public const string Source = "interim-companion";
    public const string SwapTarget = "src/RideAudit.Protos";
    public const string OpenApiAuthority = "non-authoritative";
}

public interface ISealedAdmissionClient
{
    SubmitSealedResponse SubmitSealed(SubmitSealedRequest request);

    AdmissionStatusResponse GetAdmissionStatus(AdmissionStatusRequest request);
}

/// <summary>
/// In-process client preflight. This is not the server admission service.
/// </summary>
public sealed class InterimInProcessAdmissionClient : ISealedAdmissionClient
{
    private readonly Dictionary<string, AdmissionStatusResponse> _status = new(StringComparer.Ordinal);

    public SubmitSealedResponse SubmitSealed(SubmitSealedRequest request)
    {
        if (request.Receipt is null || string.IsNullOrWhiteSpace(request.Receipt.ContentHash))
        {
            return Reject("RECEIPT_MISSING", "Custody receipt is missing.");
        }

        if (!string.Equals(request.ContentType, ApiBoundary.SealedContentType, StringComparison.Ordinal))
        {
            return Reject("PLAINTEXT_REJECTED", "Content type is not a sealed envelope.");
        }

        if (request.Ciphertext.IsEmpty || request.Ciphertext[0] != (byte)'R')
        {
            return Reject("PLAINTEXT_REJECTED", "Ciphertext is missing the sealed envelope magic.");
        }

        if (!string.Equals(request.SubmitterRole, "driver", StringComparison.OrdinalIgnoreCase))
        {
            return Reject("AUTH_FORBIDDEN", "Only the driver phone may submit.");
        }

        if (string.IsNullOrWhiteSpace(request.VehicleId))
        {
            return Reject("VEHICLE_UNREGISTERED", "Vehicle id is required.");
        }

        if (request.Attestation is null || string.IsNullOrWhiteSpace(request.Attestation.TokenHash))
        {
            return Reject("ATTESTATION_FAILED", "Attestation is missing.");
        }

        if (string.IsNullOrWhiteSpace(request.SessionId))
        {
            return Reject("SESSION_INVALID", "Session id is required.");
        }

        var id = "sub-" + Guid.NewGuid().ToString("N");
        var status = new AdmissionStatusResponse
        {
            SubmissionId = id,
            Decision = "pending_server_admission",
            ReceiptPresent = true,
            PlayIntegrityOk = request.Attestation.ClientClaimedVerdictOk,
            VehicleRegistered = true,
            PlaintextRejected = true,
            RejectCode = "",
        };
        _status[id] = status;
        return new SubmitSealedResponse
        {
            SubmissionId = id,
            Status = "pending_server_admission",
            ErrorCode = "",
            Message = "Client preflight accepted a sealed envelope. Server admission is not performed here.",
        };
    }

    public AdmissionStatusResponse GetAdmissionStatus(AdmissionStatusRequest request)
    {
        if (_status.TryGetValue(request.SubmissionId, out var status))
        {
            return status;
        }

        return new AdmissionStatusResponse
        {
            SubmissionId = request.SubmissionId,
            Decision = "rejected",
            ReceiptPresent = false,
            PlayIntegrityOk = false,
            VehicleRegistered = false,
            PlaintextRejected = true,
            RejectCode = "SUBMISSION_NOT_FOUND",
        };
    }

    private static SubmitSealedResponse Reject(string code, string message) =>
        new()
        {
            SubmissionId = "",
            Status = "rejected",
            ErrorCode = code,
            Message = message,
        };
}
