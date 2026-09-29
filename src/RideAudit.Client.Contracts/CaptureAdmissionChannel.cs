// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Formats.Asn1;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
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
        TimeSpan timeout,
        ReadOnlyMemory<byte> labIntermediatePem = default)
    {
        options.EnsureReady();
        var address = options.AdmissionAddress!;
        RequireEdgeTlsAddress(address);
        var trust = labRootPem.Length == 0 ? "platform" : "caddy-local-root";
        X509Certificate2? root = null;
        X509Certificate2? intermediate = null;
        string? peer = null;
        string? validation = null;
        try
        {
            SocketsHttpHandler handler;
            if (labRootPem.Length == 0)
            {
                handler = new SocketsHttpHandler { ConnectTimeout = timeout };
            }
            else
            {
                root = X509Certificate2.CreateFromPem(Encoding.ASCII.GetString(labRootPem.Span));
                if (labIntermediatePem.Length > 0)
                {
                    intermediate = X509Certificate2.CreateFromPem(Encoding.ASCII.GetString(labIntermediatePem.Span));
                }

                var trusted = root;
                var extra = intermediate is null ? null : new X509Certificate2[] { intermediate };
                handler = new SocketsHttpHandler
                {
                    ConnectTimeout = timeout,
                    SslOptions = new SslClientAuthenticationOptions
                    {
                        TargetHost = new Uri(address).Host,
                        ApplicationProtocols = new List<SslApplicationProtocol> { SslApplicationProtocol.Http2 },
                        RemoteCertificateValidationCallback = (sender, certificate, chain, errors) =>
                        {
                            peer = certificate?.Subject;
                            var accepted = LabRootTrust.Accepts(certificate, chain, errors, trusted, extra, out var detail);
                            validation = "accepted=" + accepted + " " + detail + " peer=" + (peer ?? "");
                            return accepted;
                        }
                    }
                };
                handler.Properties["__GrpcLoadBalancingDisabled"] = true;
            }

            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            using var channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions
            {
                HttpHandler = handler,
                DisposeHttpClient = true,
                HttpVersion = HttpVersion.Version20,
                HttpVersionPolicy = HttpVersionPolicy.RequestVersionExact
            });
            var client = new Admission.AdmissionClient(channel);
            var headers = new Metadata { { "authorization", "Bearer " + options.BearerToken } };
            var response = client.Health(
                new HealthRequest(),
                headers,
                deadline: DateTime.UtcNow.Add(timeout));
            return EdgeTlsProbeResult.HealthOk(
                address,
                trust,
                "Health status=" + response.Status + " service=" + response.Service
                    + " peer=" + (peer ?? "")
                    + (validation is null ? "" : " " + validation));
        }
        catch (Exception ex) when (IsTlsFailure(ex))
        {
            return EdgeTlsProbeResult.Failed(
                address,
                trust,
                "TLS handshake failed: " + Describe(ex) + (validation is null ? "" : " " + validation));
        }
        catch (RpcException ex)
        {
            return EdgeTlsProbeResult.TlsOnly(
                address,
                trust,
                ex.StatusCode.ToString(),
                "gRPC " + ex.StatusCode + " " + (ex.Status.Detail ?? "")
                    + (validation is null ? "" : " " + validation));
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
            intermediate?.Dispose();
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
        GrpcStatus == "OK"
            ? "EDGE_TLS HEALTH_OK " + Address + " trust=" + Trust + " " + Outcome
            : HandshakeCompleted
                ? "EDGE_TLS TLS_OK " + Address + " trust=" + Trust + " grpc=" + GrpcStatus + " " + Outcome
                : "EDGE_TLS FAIL " + Address + " trust=" + Trust + " " + Outcome;

    public static EdgeTlsProbeResult HealthOk(string address, string trust, string outcome) =>
        new()
        {
            HandshakeCompleted = true,
            Address = address,
            Trust = trust,
            GrpcStatus = "OK",
            Outcome = outcome
        };

    public static EdgeTlsProbeResult TlsOnly(string address, string trust, string grpcStatus, string outcome) =>
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
        X509Certificate2 root) =>
        Accepts(certificate, presented, errors, root, out _);

    public static bool Accepts(
        X509Certificate? certificate,
        X509Chain? presented,
        SslPolicyErrors errors,
        X509Certificate2 root,
        out string detail) =>
        Accepts(certificate, presented, errors, root, null, out detail);

    public static bool Accepts(
        X509Certificate? certificate,
        X509Chain? presented,
        SslPolicyErrors errors,
        X509Certificate2 root,
        IReadOnlyList<X509Certificate2>? extras,
        out string detail)
    {
        if (certificate is null)
        {
            detail = "no-certificate";
            return false;
        }

        if ((errors & SslPolicyErrors.RemoteCertificateNameMismatch) != 0
            || (errors & SslPolicyErrors.RemoteCertificateNotAvailable) != 0)
        {
            detail = "name-or-cert-unavailable errors=" + errors;
            return false;
        }

        if (errors == SslPolicyErrors.None)
        {
            detail = "platform-trust";
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

            if (extras is not null)
            {
                foreach (var extra in extras)
                {
                    chain.ChainPolicy.ExtraStore.Add(extra);
                }
            }

            if (chain.Build(leaf))
            {
                detail = "custom-root-built";
                return true;
            }

            var rejected = string.Join(",", chain.ChainStatus.Select(item => item.Status.ToString()));
            if (ReachesKnownRoot(leaf, presented, root, extras))
            {
                detail = "signature-walk-root after " + rejected;
                return true;
            }

            detail = "custom-root-rejected " + rejected;
            return false;
        }
        catch (Exception ex)
        {
            if (ReachesKnownRoot(leaf, presented, root, extras))
            {
                detail = "signature-walk-root after " + ex.GetType().Name;
                return true;
            }

            detail = "custom-root-threw " + ex.GetType().Name;
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

    private static bool ReachesKnownRoot(
        X509Certificate2 leaf,
        X509Chain? presented,
        X509Certificate2 root,
        IReadOnlyList<X509Certificate2>? extras)
    {
        var pool = new List<X509Certificate2> { root };
        if (extras is not null)
        {
            pool.AddRange(extras);
        }

        if (presented is not null)
        {
            foreach (var element in presented.ChainElements)
            {
                pool.Add(element.Certificate);
            }
        }

        var current = leaf;
        for (var hop = 0; hop < 6; hop++)
        {
            if (string.Equals(current.Thumbprint, root.Thumbprint, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            X509Certificate2? signer = null;
            foreach (var candidate in pool)
            {
                if (!current.IssuerName.RawData.AsSpan().SequenceEqual(candidate.SubjectName.RawData))
                {
                    continue;
                }

                if (VerifyEcdsaSha256(current, candidate))
                {
                    signer = candidate;
                    break;
                }
            }

            if (signer is null)
            {
                return false;
            }

            if (string.Equals(signer.Thumbprint, root.Thumbprint, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            current = signer;
        }

        return false;
    }

    private static bool VerifyEcdsaSha256(X509Certificate2 child, X509Certificate2 issuer)
    {
        try
        {
            using var key = issuer.GetECDsaPublicKey();
            if (key is null)
            {
                return false;
            }

            var reader = new AsnReader(child.RawData, AsnEncodingRules.DER);
            var certificate = reader.ReadSequence();
            var tbs = certificate.ReadEncodedValue();
            var algorithm = certificate.ReadSequence();
            var oid = algorithm.ReadObjectIdentifier();
            if (oid != "1.2.840.10045.4.3.2")
            {
                return false;
            }

            var signature = certificate.ReadBitString(out _);
            return key.VerifyData(tbs.Span, signature, HashAlgorithmName.SHA256, DSASignatureFormat.Rfc3279DerSequence);
        }
        catch (Exception)
        {
            return false;
        }
    }
}
