// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
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

    /// <summary>
    /// Rejects plaintext admission listeners and ngrok before any socket is opened.
    /// Counsel is not an address this client calls.
    /// </summary>
    public static void RequireEdgeTlsAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address)
            || !address.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            throw new RideAuditFailClosedException(
                ErrorCodes.AdmissionUnavailable,
                "FR-RIDE-035",
                "EDGE_TLS refused a non-https admission address.");
        }

        if (address.Contains(":28080", StringComparison.OrdinalIgnoreCase)
            || address.Contains(":28081", StringComparison.OrdinalIgnoreCase))
        {
            throw new RideAuditFailClosedException(
                ErrorCodes.AdmissionUnavailable,
                "FR-RIDE-035",
                "EDGE_TLS refused plaintext port 28080 or 28081.");
        }

        if (address.Contains("ngrok", StringComparison.OrdinalIgnoreCase))
        {
            throw new RideAuditFailClosedException(
                ErrorCodes.AdmissionUnavailable,
                "FR-RIDE-035",
                "EDGE_TLS refused an ngrok admission address.");
        }
    }

    /// <summary>
    /// Opens the admission gRPC channel and calls Health.
    /// Lab trust accepts a certificate only when the chain builds to the supplied
    /// Caddy local root. An empty root uses platform trust. Neither path accepts
    /// every certificate. A failed handshake stays a failure.
    /// </summary>
    public static EdgeTlsProbeResult ProbeEdgeTls(
        CaptureAdmissionOptions options,
        ReadOnlyMemory<byte> labRootPem,
        TimeSpan timeout)
    {
        options.EnsureReady();
        var address = options.AdmissionAddress!;
        RequireEdgeTlsAddress(address);
        var trust = labRootPem.Length == 0 ? "platform" : "caddy-local-root";
        X509Certificate2? root = null;
        try
        {
            string? peer = null;
            SocketsHttpHandler handler;
            if (labRootPem.Length == 0)
            {
                handler = new SocketsHttpHandler { ConnectTimeout = timeout };
            }
            else
            {
                root = X509Certificate2.CreateFromPem(Encoding.ASCII.GetString(labRootPem.Span));
                var trusted = root;
                handler = new SocketsHttpHandler
                {
                    ConnectTimeout = timeout,
                    SslOptions = new SslClientAuthenticationOptions
                    {
                        TargetHost = new Uri(address).Host,
                        RemoteCertificateValidationCallback = (sender, certificate, chain, errors) =>
                        {
                            peer = certificate?.Subject;
                            return LabRootTrust.Accepts(certificate, chain, errors, trusted);
                        }
                    }
                };
            }

            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            using var channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions
            {
                HttpHandler = handler,
                DisposeHttpClient = true
            });
            var client = new Admission.AdmissionClient(channel);
            var headers = new Metadata { { "authorization", "Bearer " + options.BearerToken } };
            var response = client.Health(
                new HealthRequest(),
                headers,
                deadline: DateTime.UtcNow.Add(timeout));
            return EdgeTlsProbeResult.Connected(
                address,
                trust,
                "OK",
                "Health status=" + response.Status + " service=" + response.Service
                    + " peer=" + (peer ?? ""));
        }
        catch (Exception ex) when (IsTlsFailure(ex))
        {
            return EdgeTlsProbeResult.Failed(address, trust, "TLS handshake failed: " + Describe(ex));
        }
        catch (RpcException ex)
        {
            return EdgeTlsProbeResult.Connected(
                address,
                trust,
                ex.StatusCode.ToString(),
                "gRPC " + ex.StatusCode + " " + (ex.Status.Detail ?? ""));
        }
        catch (Exception ex)
        {
            return EdgeTlsProbeResult.Failed(
                address,
                trust,
                "connect failed: " + ex.GetType().Name + ": " + ex.Message);
        }
        finally
        {
            root?.Dispose();
        }
    }

    private static bool IsTlsFailure(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is AuthenticationException)
            {
                return true;
            }

            var message = current.Message ?? "";
            if (message.Contains("SSL", StringComparison.OrdinalIgnoreCase)
                || message.Contains("certificate", StringComparison.OrdinalIgnoreCase)
                || message.Contains("TLS", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string Describe(Exception exception)
    {
        var parts = new List<string>();
        for (var current = exception; current is not null && parts.Count < 4; current = current.InnerException)
        {
            parts.Add(current.GetType().Name + ": " + current.Message);
        }

        return string.Join(" | ", parts);
    }
}

public sealed class EdgeTlsProbeResult
{
    public required bool HandshakeCompleted { get; init; }
    public required string Address { get; init; }
    public required string Trust { get; init; }
    public required string Outcome { get; init; }
    public string? GrpcStatus { get; init; }

    public string Display =>
        HandshakeCompleted
            ? "EDGE_TLS CONNECTED " + Address + " trust=" + Trust + " grpc=" + GrpcStatus + " " + Outcome
            : "EDGE_TLS FAIL " + Address + " trust=" + Trust + " " + Outcome;

    public static EdgeTlsProbeResult Connected(string address, string trust, string grpcStatus, string outcome) =>
        new()
        {
            HandshakeCompleted = true,
            Address = address,
            Trust = trust,
            GrpcStatus = grpcStatus,
            Outcome = outcome
        };

    public static EdgeTlsProbeResult Failed(string address, string trust, string outcome) =>
        new()
        {
            HandshakeCompleted = false,
            Address = address,
            Trust = trust,
            Outcome = outcome
        };
}

/// <summary>
/// Trusts the live Caddy local root only. Platform-valid certificates still pass.
/// A chain that does not build to the supplied root is rejected. Name mismatch is rejected.
/// </summary>
public static class LabRootTrust
{
    public static bool Accepts(
        X509Certificate? certificate,
        X509Chain? presented,
        SslPolicyErrors errors,
        X509Certificate2 root)
    {
        if (certificate is null)
        {
            return false;
        }

        if ((errors & SslPolicyErrors.RemoteCertificateNameMismatch) != 0
            || (errors & SslPolicyErrors.RemoteCertificateNotAvailable) != 0)
        {
            return false;
        }

        if (errors == SslPolicyErrors.None)
        {
            return true;
        }

        var ownsLeaf = certificate is not X509Certificate2;
        var leaf = certificate as X509Certificate2 ?? new X509Certificate2(certificate);
        try
        {
            using var chain = new X509Chain();
            chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
            chain.ChainPolicy.CustomTrustStore.Add(root);
            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            if (presented is not null)
            {
                foreach (var element in presented.ChainElements)
                {
                    chain.ChainPolicy.ExtraStore.Add(element.Certificate);
                }
            }

            return chain.Build(leaf);
        }
        catch (Exception)
        {
            return false;
        }
        finally
        {
            if (ownsLeaf)
            {
                leaf.Dispose();
            }
        }
    }
}
