// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Security.Cryptography;
using RideAudit.Client.Core;
using RideAudit.PlayIntegrity;
using RideAudit.Client.Seal;

namespace RideAudit.Viewer;

public static class ViewerLogic
{
    public const string Version = "0.1.0";
    public const string License = LicenseMetadata.GplId;
    public static readonly string[] Components =
    [
        "decryption",
        "verification",
        "synchronization",
        "overlay",
        "rendering",
    ];

    public static readonly string[] PortableTargets = ["Windows", "Linux", "macOS"];
}

public sealed class CustodianAttestation
{
    public required string CustodianId { get; init; }
    public required string Statement { get; init; }
}

public sealed class CourtReleaseAuthorization
{
    public required string CaseId { get; init; }
    public required string Authorizer { get; init; }
    public required int M { get; init; }
    public required int N { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public required IReadOnlyList<CustodianAttestation> Attestations { get; init; }
    public required string WorkingCopyRef { get; init; }
}

public interface IEscrowKeyRelease
{
    bool TryReleaseDek(CourtReleaseAuthorization authorization, WrappedDek wrapped, string caseId, out byte[] dek);
}

/// <summary>
/// Off-device escrow. The private key never ships inside the viewer binary.
/// </summary>
public sealed class QuorumEscrow : IEscrowKeyRelease
{
    private readonly RSA _privateKey;
    private readonly IClock _clock;

    public QuorumEscrow(RSA privateKey, IClock clock)
    {
        _privateKey = privateKey;
        _clock = clock;
    }

    public int ReleaseAttempts { get; private set; }

    public bool TryReleaseDek(CourtReleaseAuthorization authorization, WrappedDek wrapped, string caseId, out byte[] dek)
    {
        ReleaseAttempts++;
        dek = [];
        if (!string.Equals(authorization.CaseId, caseId, StringComparison.Ordinal))
        {
            return false;
        }

        if (authorization.Attestations.Count < authorization.M || authorization.M < 1 || authorization.Attestations.Count > authorization.N)
        {
            return false;
        }

        if (authorization.ExpiresAt <= _clock.UtcNow)
        {
            return false;
        }

        try
        {
            dek = EscrowKeyFactory.Unwrap(_privateKey, wrapped.WrappedKey);
            return true;
        }
        catch (CryptographicException)
        {
            dek = [];
            return false;
        }
    }
}

public sealed class ReviewRecord
{
    public required SealedRecord Sealed { get; init; }
    public required OtsProof Ots { get; init; }
    public required bool Admitted { get; init; }
    public bool GpsPresent { get; init; }
    public bool Obd2Present { get; init; }
    public IReadOnlyList<TelematicsCue> Telematics { get; init; } = [];
}

public sealed record TelematicsCue(TimeSpan SessionTime, string Kind, string Label);

public sealed class RideBundle
{
    public required string BundleId { get; init; }
    public required string CaseId { get; init; }
    public required string VehicleId { get; init; }
    public required IReadOnlyList<ReviewRecord> Records { get; init; }
}

public sealed class ViewerSession
{
    public required string SessionId { get; init; }
    public required string ViewerBuild { get; init; }
    public required string ReviewerRole { get; init; }
    public required string CaseId { get; init; }
    public required string BundleId { get; init; }
    public required DateTimeOffset StartedAt { get; init; }
    public required string LogicVersion { get; init; }
    public required string License { get; init; }
    public List<string> AuditEvents { get; } = [];
    public VerificationReport? Report { get; set; }
}

public sealed class VerificationCheck
{
    public required string RecordId { get; init; }
    public required string Name { get; init; }
    public required string Status { get; init; }
    public required string Detail { get; init; }

    public bool Passed => Status == "pass";
}

public sealed class VerificationReport
{
    public required string ReportId { get; init; }
    public required string ViewerSessionId { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required string LogicVersion { get; init; }
    public required IReadOnlyList<VerificationCheck> Checks { get; init; }
    public required bool FailClosed { get; init; }

    public bool RecordCustodyPassed(string recordId) =>
        Checks.Where(check => check.RecordId == recordId && check.Name != "escrow_authorization").All(check => check.Passed);

    public bool AllCustodyPassed =>
        Checks.Where(check => check.Name != "escrow_authorization").All(check => check.Passed);
}

public sealed class ExpiringWorkingCopy
{
    public required string RecordId { get; init; }
    public required byte[] Plaintext { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public required string CaseId { get; init; }

    public bool IsExpired(DateTimeOffset now) => now >= ExpiresAt;
}

public sealed class TimelineTrack
{
    public required string Kind { get; init; }
    public required bool Present { get; init; }
    public required int SampleCount { get; init; }
    public required string AbsenceReason { get; init; }
}

public sealed class TimelineCue
{
    public required TimeSpan SessionTime { get; init; }
    public required string CompositeLabel { get; init; }
    public required string SpiderLabel { get; init; }
}

public sealed class SynchronizedTimeline
{
    public required string RecordId { get; init; }
    public required string SyncClockOffset { get; init; }
    public required TimelineTrack Composite { get; init; }
    public required TimelineTrack Spider { get; init; }
    public required TimelineTrack Telematics { get; init; }
    public required TimelineTrack Gps { get; init; }
    public required TimelineTrack Obd2 { get; init; }
    public required IReadOnlyList<TimelineCue> Cues { get; init; }
    public bool RequiresCollectionDevice => false;

    public TimelineCue CueAt(TimeSpan sessionTime)
    {
        TimelineCue? best = null;
        foreach (var cue in Cues)
        {
            if (cue.SessionTime <= sessionTime)
            {
                best = cue;
            }
        }

        return best ?? Cues[0];
    }
}

public sealed class DisclosurePack
{
    public required string BundleId { get; init; }
    public required byte[] SealedEnvelope { get; init; }
    public required string OtsCommittedDigest { get; init; }
    public required string AttestationTokenHash { get; init; }
    public required VerificationReport Report { get; init; }
    public bool ContainsPlaintext { get; init; }
}

public enum ReviewPhase
{
    FailClosed,
    EscrowRequired,
    Playback,
}

public sealed class ReviewOutcome
{
    public required ViewerSession Session { get; init; }
    public required VerificationReport Report { get; init; }
    public required ReviewPhase Phase { get; init; }
    public required bool DecryptAllowed { get; init; }
    public required bool DisplayAllowed { get; init; }
    public IReadOnlyList<ExpiringWorkingCopy> WorkingCopies { get; init; } = [];
    public IReadOnlyList<SynchronizedTimeline> Timelines { get; init; } = [];
    public string? BlockReason { get; init; }
}
