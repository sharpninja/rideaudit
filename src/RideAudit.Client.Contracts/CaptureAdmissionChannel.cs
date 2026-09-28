// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using Grpc.Core;
using Grpc.Net.Client;
using RideAudit.Client.Core;
using RideAudit.Contracts;
using RideAudit.Protos.Admission.V1;

namespace RideAudit.Client.Contracts;

public sealed class CaptureAdmissionOptions
{
    public string? AdmissionAddress { get; init; }
    public string? BearerToken { get; init; }
    public string? TenantId { get; init; }
    public string? DriverId { get; init; }
    public string? PolicyVersion { get; init; }

    public static CaptureAdmissionOptions FromEnvironment() =>
        new()
        {
            AdmissionAddress = Environment.GetEnvironmentVariable("RIDEAUDIT_ADMISSION_ADDRESS"),
            BearerToken = Environment.GetEnvironmentVariable("RIDEAUDIT_ADMISSION_BEARER"),
            TenantId = Environment.GetEnvironmentVariable("RIDEAUDIT_TENANT_ID"),
            DriverId = Environment.GetEnvironmentVariable("RIDEAUDIT_DRIVER_ID"),
            PolicyVersion = Environment.GetEnvironmentVariable("RIDEAUDIT_POLICY_VERSION") ?? RideAuditPolicy.Version
        };

    public void EnsureReady()
    {
        if (string.IsNullOrWhiteSpace(AdmissionAddress))
        {
            throw new RideAuditFailClosedException(
                ErrorCodes.AdmissionUnavailable,
                "FR-RIDE-035",
                "No admission endpoint is configured. Capture will not invent a local admit.");
        }

        if (string.IsNullOrWhiteSpace(BearerToken))
        {
            throw new RideAuditFailClosedException(
                ErrorCodes.AuthRequired,
                "FR-RIDE-035",
                "Admission requires a bearer token.");
        }
    }
}

/// <summary>
/// Production wiring point from capture to generated admission gRPC.
/// Missing address or bearer fail-closes. This does not mint fixture.v1. tokens or chain receipts.
/// </summary>
public static class CaptureAdmissionChannel
{
    public static ISealedAdmissionClient Connect(CaptureAdmissionOptions options)
    {
        options.EnsureReady();
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        var channel = GrpcChannel.ForAddress(options.AdmissionAddress!);
        var headers = new Metadata { { "authorization", "Bearer " + options.BearerToken } };
        return new GrpcSealedAdmissionClient(new Admission.AdmissionClient(channel), headers);
    }
}
