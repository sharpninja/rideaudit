// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Security.Cryptography;
using RideAudit.Client.Core;

namespace RideAudit.PlayIntegrity;

public sealed class AttestationRequest
{
    public AttestationRequest(string nonce, string collectorIdentity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nonce);
        ArgumentException.ThrowIfNullOrWhiteSpace(collectorIdentity);
        Nonce = nonce;
        CollectorIdentity = collectorIdentity;
    }

    public string Nonce { get; }

    public string CollectorIdentity { get; }

    public static AttestationRequest Create(string collectorIdentity)
    {
        var bytes = RandomNumberGenerator.GetBytes(16);
        return new AttestationRequest(Convert.ToHexString(bytes).ToLowerInvariant(), collectorIdentity);
    }
}

/// <summary>
/// Verifies Play Integrity (or the explicit stub) before any key generation or sealing.
/// </summary>
public sealed class PlayIntegrityGate
{
    public static readonly TimeSpan MaxAttestationAge = TimeSpan.FromMinutes(2);

    private readonly IPlayIntegrityClient _client;
    private readonly IClock _clock;
    private readonly PackageAllowlist _allowlist;

    public PlayIntegrityGate(IPlayIntegrityClient client, PackageAllowlist allowlist, IClock clock)
    {
        _client = client;
        _allowlist = allowlist;
        _clock = clock;
    }

    public PackageAllowlist Allowlist => _allowlist;

    public PlayAuthorization AuthorizeKeyGeneration(AttestationRequest request)
    {
        PlayTokenResult token;
        try
        {
            token = _client.RequestToken(request.Nonce, _clock);
        }
        catch (Exception ex)
        {
            return PlayAuthorization.Reject(
                AttestationFailure.Unverifiable,
                "Attestation request failed: " + ex.GetType().Name);
        }

        if (string.IsNullOrWhiteSpace(token.TokenMaterial))
        {
            return PlayAuthorization.Reject(AttestationFailure.Missing, "Attestation token is missing.");
        }

        if (!string.Equals(token.Nonce, request.Nonce, StringComparison.Ordinal))
        {
            return PlayAuthorization.Reject(AttestationFailure.NonceMismatch, "Attestation nonce does not match the request.");
        }

        var age = _clock.UtcNow - token.ObtainedAt;
        if (age < TimeSpan.Zero || age > MaxAttestationAge)
        {
            return PlayAuthorization.Reject(AttestationFailure.Stale, "Attestation is stale or from the future.");
        }

        if (!token.MeetsDeviceIntegrity || token.Verdict.Contains("COMPROMISED", StringComparison.OrdinalIgnoreCase))
        {
            return PlayAuthorization.Reject(AttestationFailure.Compromised, "Device integrity failed.");
        }

        if (!token.RecognizedApp || token.Verdict.Contains("UNRECOGNIZED", StringComparison.OrdinalIgnoreCase)
            || token.PackageIdentity.Contains("sideload", StringComparison.OrdinalIgnoreCase)
            || token.SigningCertDigest.Contains("unofficial", StringComparison.OrdinalIgnoreCase))
        {
            return PlayAuthorization.Reject(AttestationFailure.Sideloaded, "App is unrecognized, modified, or sideloaded.");
        }

        if (token.Verdict is "FAILED" or "UNVERIFIABLE" or "MISSING" or "UNAVAILABLE")
        {
            var failure = token.Verdict == "UNVERIFIABLE"
                ? AttestationFailure.Unverifiable
                : AttestationFailure.Failed;
            return PlayAuthorization.Reject(failure, "Attestation verdict is " + token.Verdict + ".");
        }

        if (!_allowlist.IsAllowed(token.PackageIdentity, token.SigningCertDigest))
        {
            return PlayAuthorization.Reject(
                AttestationFailure.NotAllowlisted,
                "Package identity or signing certificate is not on allowlist version " + _allowlist.Version + ".");
        }

        var evidence = new AttestationEvidence(
            token.Provider,
            Hashes.Sha256Hex(token.TokenMaterial),
            token.Nonce,
            token.ObtainedAt,
            token.PackageIdentity,
            token.SigningCertDigest,
            token.Verdict,
            token.MeetsDeviceIntegrity,
            token.RecognizedApp,
            token.StubNotice);

        return PlayAuthorization.Accept(evidence);
    }
}
