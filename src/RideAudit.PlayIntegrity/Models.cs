// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

namespace RideAudit.PlayIntegrity;

public enum AttestationFailure
{
    None = 0,
    Missing,
    Failed,
    Compromised,
    Sideloaded,
    UnapprovedPackage,
    UnapprovedCertificate,
    Stale,
    NonceMismatch,
    Unverifiable,
    NotAllowlisted,
}

public enum StubPlayMode
{
    /// <summary>Simulated success. Receipt provider stays stub-play-integrity.</summary>
    SimulatedSuccess,
    Missing,
    Failed,
    Compromised,
    Sideloaded,
    Stale,
    NonceMismatch,
    Unverifiable,
    NotAllowlisted,
}

/// <summary>
/// Attestation evidence retained for later verification. Raw tokens are not logged.
/// </summary>
public sealed record AttestationEvidence(
    string Provider,
    string TokenHash,
    string Nonce,
    DateTimeOffset ObtainedAt,
    string PackageIdentity,
    string SigningCertDigest,
    string Verdict,
    bool MeetsDeviceIntegrity,
    bool RecognizedApp,
    string? StubNotice)
{
    public bool IsSimulated =>
        string.Equals(Provider, PlayIntegrityProviders.Stub, StringComparison.Ordinal);
}

public sealed class PlayAuthorization
{
    private PlayAuthorization(
        bool accepted,
        AttestationEvidence? evidence,
        AttestationFailure failure,
        string? detail)
    {
        Accepted = accepted;
        Evidence = evidence;
        Failure = failure;
        Detail = detail;
    }

    public bool Accepted { get; }

    public AttestationEvidence? Evidence { get; }

    public AttestationFailure Failure { get; }

    public string? Detail { get; }

    public static PlayAuthorization Accept(AttestationEvidence evidence) =>
        new(true, evidence, AttestationFailure.None, null);

    public static PlayAuthorization Reject(AttestationFailure failure, string detail) =>
        new(false, null, failure, detail);
}

public static class PlayIntegrityProviders
{
    public const string Real = "play_integrity";
    public const string Stub = "stub-play-integrity";
}
