// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using Grpc.Core;
using RideAudit.Contracts;
using RideAudit.Protos.Admission.V1;

namespace RideAudit.Client.Contracts;

/// <summary>
/// Sends authoritative admission protos over a generated gRPC client.
/// This type does not seal, decrypt, or invent Play Integrity or Bitcoin receipts.
/// The caller supplies the request, including whatever attestation token it already holds.
/// </summary>
public sealed class GrpcSealedAdmissionClient : ISealedAdmissionClient
{
    private readonly Admission.AdmissionClient _inner;
    private readonly Metadata _headers;

    public GrpcSealedAdmissionClient(Admission.AdmissionClient inner, Metadata headers)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _headers = headers ?? throw new ArgumentNullException(nameof(headers));
    }

    public AdmissionDecision SubmitSealed(SubmitSealedRequest request)
    {
        try
        {
            return _inner.SubmitSealed(request, _headers);
        }
        catch (RpcException ex)
        {
            return Rejected(ex);
        }
    }

    public AdmissionDecision GetAdmissionStatus(GetAdmissionStatusRequest request)
    {
        try
        {
            return _inner.GetAdmissionStatus(request, _headers);
        }
        catch (RpcException ex)
        {
            return Rejected(ex);
        }
    }

    private static AdmissionDecision Rejected(RpcException ex)
    {
        var detail = ex.Status.Detail ?? "";
        var code = detail;
        var message = detail;
        var split = detail.Split(':', 2);
        if (split.Length == 2 && !string.IsNullOrWhiteSpace(split[0]))
        {
            code = split[0].Trim();
            message = split[1].Trim();
        }

        var submissionId = ex.Trailers.GetValue("x-rideaudit-submission-id") ?? "";
        return new AdmissionDecision
        {
            SubmissionId = submissionId,
            CustodyState = CustodyStateNames.ToWire(CustodyState.Rejected),
            Admitted = false,
            CollectionComplete = false,
            RejectCode = code,
            Message = message,
            CiphertextStored = false
        };
    }
}
