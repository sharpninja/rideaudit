// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using RideAudit.Client.Core;
using RideAudit.Contracts;
using RideAudit.Protos.Admission.V1;
using RideAudit.Protos.Custody.V1;

namespace RideAudit.Client.Contracts;

public static class ContractProvenance
{
    public const string Source = "src/RideAudit.Protos";
    public const string ContractVersion = ContractAuthority.ContractVersion;
    public const string SwapTarget = ContractAuthority.AuthoritativePath;
    public const string OpenApiAuthority = "non-authoritative";
}

public interface ISealedAdmissionClient
{
    AdmissionDecision SubmitSealed(SubmitSealedRequest request);

    AdmissionDecision GetAdmissionStatus(GetAdmissionStatusRequest request);
}

/// <summary>
/// In-process client preflight over authoritative admission messages.
/// This is not the server admission service and it does not confirm an anchor.
/// </summary>
public sealed class InterimInProcessAdmissionClient : ISealedAdmissionClient
{
    private readonly Dictionary<string, AdmissionDecision> _status = new(StringComparer.Ordinal);

    public AdmissionDecision SubmitSealed(SubmitSealedRequest request)
    {
        if (request.ReceiptCore is null || request.ReceiptCore.ContentHash.IsEmpty)
        {
            return Reject(ErrorCodes.ReceiptMissing, "Custody receipt core is missing.");
        }

        if (!string.Equals(request.ContentType, RideAuditPolicy.SealedContentType, StringComparison.Ordinal))
        {
            return Reject(ErrorCodes.PlaintextRejected, "Content type is not a sealed envelope.");
        }

        if (!LooksLikeDeviceEnvelope(request.SealedEnvelope))
        {
            return Reject(ErrorCodes.PlaintextRejected, "Sealed envelope is missing the device RAES magic.");
        }

        if (string.IsNullOrWhiteSpace(request.VehicleId))
        {
            return Reject(ErrorCodes.VehicleUnregistered, "Vehicle id is required.");
        }

        if (request.Attestation is null || string.IsNullOrWhiteSpace(request.Attestation.Token))
        {
            return Reject(ErrorCodes.AttestationFailed, "Attestation is missing.");
        }

        if (string.IsNullOrWhiteSpace(request.SessionId))
        {
            return Reject(ErrorCodes.SessionInvalid, "Session id is required.");
        }

        var id = "sub-" + Guid.NewGuid().ToString("N");
        var decision = new AdmissionDecision
        {
            SubmissionId = id,
            CustodyState = CustodyStateNames.ToWire(CustodyState.LocalSealedPending),
            Admitted = false,
            CollectionComplete = false,
            RejectCode = "",
            Message = "Client preflight accepted a sealed envelope. Server admission is not performed here.",
            CiphertextStored = false,
            Anchor = PendingAnchor(),
        };
        decision.Checks.Add("client-preflight");
        decision.Checks.Add("not-server-admission");
        _status[id] = decision;
        return decision;
    }

    public AdmissionDecision GetAdmissionStatus(GetAdmissionStatusRequest request)
    {
        if (_status.TryGetValue(request.SubmissionId, out var status))
        {
            return status;
        }

        return new AdmissionDecision
        {
            SubmissionId = request.SubmissionId,
            CustodyState = CustodyStateNames.ToWire(CustodyState.Rejected),
            Admitted = false,
            CollectionComplete = false,
            RejectCode = ErrorCodes.SubmissionNotFound,
            Message = "Submission was not found.",
            CiphertextStored = false,
            Anchor = PendingAnchor(),
        };
    }

    private static AdmissionDecision Reject(string code, string message) =>
        new()
        {
            SubmissionId = "",
            CustodyState = CustodyStateNames.ToWire(CustodyState.Rejected),
            Admitted = false,
            CollectionComplete = false,
            RejectCode = code,
            Message = message,
            CiphertextStored = false,
            Anchor = PendingAnchor(),
        };

    private static bool LooksLikeDeviceEnvelope(Google.Protobuf.ByteString bytes) =>
        bytes.Length > 33 &&
        bytes[0] == (byte)'R' &&
        bytes[1] == (byte)'A' &&
        bytes[2] == (byte)'E' &&
        bytes[3] == (byte)'S';

    /// <summary>
    /// Pending is not confirmation. No Bitcoin transaction metadata is invented here.
    /// </summary>
    private static AnchorProofEnvelope PendingAnchor() =>
        new()
        {
            ProfileId = ChainProfileIds.BtcOts,
            ProofSource = "client-preflight",
            Status = "pending",
            Disclaimer = "Pending anchor is not confirmation. This client did not verify a Bitcoin header or an OpenTimestamps upgrade.",
            LiveBitcoinMetadata = false,
            FailureCode = "",
        };
}
