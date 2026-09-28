// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using RideAudit.Client.Core;

namespace RideAudit.PlayIntegrity;

public sealed record PlayTokenResult(
    string Provider,
    string TokenMaterial,
    string Nonce,
    DateTimeOffset ObtainedAt,
    string PackageIdentity,
    string SigningCertDigest,
    string Verdict,
    bool MeetsDeviceIntegrity,
    bool RecognizedApp,
    string? StubNotice);

public interface IPlayIntegrityClient
{
    /// <summary>
    /// Request an attestation token for <paramref name="nonce"/>.
    /// Implementations must not invent a success when Play is unavailable.
    /// </summary>
    PlayTokenResult RequestToken(string nonce, IClock clock);
}

/// <summary>
/// Test and cloud stand-in. Never claims to be a Google Play Integrity token.
/// </summary>
public sealed class StubPlayIntegrityClient : IPlayIntegrityClient
{
    public StubPlayIntegrityClient(StubPlayMode mode, IClock? clock = null)
    {
        Mode = mode;
        Clock = clock;
    }

    public StubPlayMode Mode { get; set; }

    public IClock? Clock { get; }

    public const string SimulatedTokenPrefix = "stub-token-not-a-play-integrity-jwt:";

    public PlayTokenResult RequestToken(string nonce, IClock clock)
    {
        var now = (Clock ?? clock).UtcNow;
        var package = ApprovedPackage.PackageName;
        var cert = ApprovedPackage.CertDigest;
        var verdict = "MEETS_DEVICE_INTEGRITY";
        var deviceOk = true;
        var appOk = true;
        var tokenNonce = nonce;
        var obtained = now;
        var token = SimulatedTokenPrefix + Guid.NewGuid().ToString("N");

        switch (Mode)
        {
            case StubPlayMode.SimulatedSuccess:
                break;
            case StubPlayMode.Missing:
                token = "";
                verdict = "MISSING";
                deviceOk = false;
                appOk = false;
                break;
            case StubPlayMode.Failed:
                verdict = "FAILED";
                deviceOk = false;
                appOk = false;
                break;
            case StubPlayMode.Compromised:
                verdict = "DEVICE_COMPROMISED";
                deviceOk = false;
                break;
            case StubPlayMode.Sideloaded:
                package = "org.rideaudit.app.sideload";
                cert = "sha256:unofficial";
                verdict = "UNRECOGNIZED_VERSION";
                appOk = false;
                break;
            case StubPlayMode.Stale:
                obtained = now.AddMinutes(-30);
                verdict = "STALE";
                break;
            case StubPlayMode.NonceMismatch:
                tokenNonce = "nonce-mismatch";
                verdict = "NONCE_MISMATCH";
                break;
            case StubPlayMode.Unverifiable:
                token = "not-verifiable";
                verdict = "UNVERIFIABLE";
                deviceOk = false;
                appOk = false;
                break;
            case StubPlayMode.NotAllowlisted:
                package = "com.example.other";
                cert = "sha256:other";
                verdict = "NOT_ALLOWLISTED";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(Mode));
        }

        return new PlayTokenResult(
            PlayIntegrityProviders.Stub,
            token,
            tokenNonce,
            obtained,
            package,
            cert,
            verdict,
            deviceOk,
            appOk,
            "Simulated attestation. This is not a Google Play Integrity token and is not a Play Store receipt.");
    }
}

/// <summary>
/// Deterministic fixture for tests of court-ready field layout. It does not contact Google
/// and its token prefix is not a Play Integrity JWT.
/// </summary>
public sealed class FixturePlayIntegrityClient : IPlayIntegrityClient
{
    public const string TokenPrefix = "fixture-not-live-play:";

    public PlayTokenResult RequestToken(string nonce, IClock clock) =>
        new(
            PlayIntegrityProviders.Real,
            TokenPrefix + Guid.NewGuid().ToString("N"),
            nonce,
            clock.UtcNow,
            ApprovedPackage.PackageName,
            ApprovedPackage.CertDigest,
            "MEETS_DEVICE_INTEGRITY",
            MeetsDeviceIntegrity: true,
            RecognizedApp: true,
            StubNotice: null);
}

/// <summary>
/// Production default until a real Play Integrity token provider is injected.
/// Always fail closed. There is no user-reachable disable flag.
/// </summary>
public sealed class UnavailablePlayIntegrityClient : IPlayIntegrityClient
{
    public PlayTokenResult RequestToken(string nonce, IClock clock) =>
        new(
            PlayIntegrityProviders.Real,
            TokenMaterial: "",
            nonce,
            clock.UtcNow,
            ApprovedPackage.PackageName,
            ApprovedPackage.CertDigest,
            "UNAVAILABLE",
            MeetsDeviceIntegrity: false,
            RecognizedApp: false,
            StubNotice: "Play Integrity API was not called. No token is available in this build.");
}
