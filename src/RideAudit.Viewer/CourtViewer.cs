// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using RideAudit.Client.Core;
using RideAudit.Client.Seal;

namespace RideAudit.Viewer;

public sealed class TimelineBuilder
{
    public SynchronizedTimeline Build(ReviewRecord record, ExpiringWorkingCopy copy)
    {
        var receipt = record.Sealed.Receipt;
        var meta = receipt.CompositeMetadata;
        var spiderPresent = meta is not null && !string.IsNullOrWhiteSpace(meta.OverlayManifestVersion);
        var telematics = record.Telematics;
        var cues = new List<TimelineCue>();
        if (telematics.Count == 0 && spiderPresent)
        {
            cues.Add(new TimelineCue
            {
                SessionTime = TimeSpan.Zero,
                CompositeLabel = "composite",
                SpiderLabel = meta!.OverlayManifestVersion,
            });
        }

        foreach (var sample in telematics)
        {
            cues.Add(new TimelineCue
            {
                SessionTime = sample.SessionTime,
                CompositeLabel = "composite@" + sample.SessionTime.TotalMilliseconds.ToString("0.###"),
                SpiderLabel = sample.Kind == "spider" ? sample.Label : meta?.OverlayManifestVersion ?? "absent",
            });
        }

        if (cues.Count == 0)
        {
            cues.Add(new TimelineCue
            {
                SessionTime = TimeSpan.Zero,
                CompositeLabel = copy.RecordId,
                SpiderLabel = "absent",
            });
        }

        return new SynchronizedTimeline
        {
            RecordId = record.Sealed.Id,
            SyncClockOffset = meta?.SyncClockOffset ?? "n/a",
            Composite = Track("composite", true, cues.Count, ""),
            Spider = Track("spider", spiderPresent, spiderPresent ? cues.Count : 0, spiderPresent ? "" : "Spider overlay absent."),
            Telematics = Track("telematics", telematics.Count > 0, telematics.Count, telematics.Count == 0 ? "Telematics track absent." : ""),
            Gps = Track("gps", record.GpsPresent, record.GpsPresent ? 1 : 0, record.GpsPresent ? "" : "GPS absent. Not fabricated."),
            Obd2 = Track("obd2", record.Obd2Present, record.Obd2Present ? 1 : 0, record.Obd2Present ? "" : "OBD2 absent. Not fabricated."),
            Cues = cues,
        };
    }

    private static TimelineTrack Track(string kind, bool present, int count, string absence) =>
        new()
        {
            Kind = kind,
            Present = present,
            SampleCount = count,
            AbsenceReason = absence,
        };
}

public sealed class CourtViewer
{
    private readonly VerificationGate _gate;
    private readonly IEscrowKeyRelease _escrow;
    private readonly IClock _clock;
    private readonly TimelineBuilder _timelines = new();

    public CourtViewer(VerificationGate gate, IEscrowKeyRelease escrow, IClock clock)
    {
        _gate = gate;
        _escrow = escrow;
        _clock = clock;
    }

    public ViewerSession OpenSession(RideBundle bundle, string reviewerRole)
    {
        if (bundle.Records.Count == 0 || bundle.Records.Any(record => !record.Admitted))
        {
            var session = NewSession(bundle, reviewerRole);
            session.AuditEvents.Add("rejected-non-admitted-or-empty");
            return session;
        }

        var opened = NewSession(bundle, reviewerRole);
        opened.AuditEvents.Add("opened-sealed-bundle");
        return opened;
    }

    public ReviewOutcome Review(RideBundle bundle, string reviewerRole, CourtReleaseAuthorization? release)
    {
        var session = OpenSession(bundle, reviewerRole);
        if (session.AuditEvents.Contains("rejected-non-admitted-or-empty"))
        {
            var rejected = Finish(session, [], failClosed: true, "Bundle is empty or not admitted.");
            return Block(session, rejected, ReviewPhase.FailClosed, "Bundle is empty or not admitted.");
        }

        var checks = new List<VerificationCheck>();
        foreach (var record in bundle.Records)
        {
            checks.AddRange(_gate.VerifyRecord(record));
        }

        var custodyPassed = checks.All(check => check.Passed);
        if (!custodyPassed)
        {
            checks.Add(new VerificationCheck
            {
                RecordId = "*",
                Name = "escrow_authorization",
                Status = "fail",
                Detail = "Escrow was not attempted because custody verification failed.",
            });
            var report = Finish(session, checks, failClosed: true, "Custody verification failed.");
            session.AuditEvents.Add("fail-closed");
            return Block(session, report, ReviewPhase.FailClosed, "Custody verification failed.");
        }

        if (release is null || release.Attestations.Count < release.M)
        {
            checks.Add(new VerificationCheck
            {
                RecordId = "*",
                Name = "escrow_authorization",
                Status = "fail",
                Detail = "Escrow release is not authorized. Decrypt remains blocked.",
            });
            var report = Finish(session, checks, failClosed: true, "Escrow authorization missing.");
            session.AuditEvents.Add("escrow-required");
            return Block(session, report, ReviewPhase.EscrowRequired, "Escrow authorization missing.");
        }

        checks.Add(new VerificationCheck
        {
            RecordId = "*",
            Name = "escrow_authorization",
            Status = "pass",
            Detail = "M-of-N quorum presented. Sealed bytes are unchanged by this check.",
        });

        var copies = new List<ExpiringWorkingCopy>();
        var timelines = new List<SynchronizedTimeline>();
        foreach (var record in bundle.Records)
        {
            if (!_escrow.TryReleaseDek(release, record.Sealed.WrappedKey, bundle.CaseId, out var dek) || dek.Length == 0)
            {
                Hashes.Zero(dek);
                var failed = Finish(session, checks, failClosed: true, "Escrow did not release a key.");
                session.AuditEvents.Add("escrow-release-failed");
                return Block(session, failed, ReviewPhase.FailClosed, "Escrow did not release a key.");
            }

            byte[] plaintext;
            try
            {
                plaintext = CollectionSealer.DecryptEnvelope(record.Sealed.Envelope, dek);
            }
            finally
            {
                Hashes.Zero(dek);
            }

            var plaintextHash = Hashes.Sha256Hex(plaintext);
            if (!string.Equals(plaintextHash, record.Sealed.Receipt.PlaintextContentHash, StringComparison.OrdinalIgnoreCase))
            {
                Hashes.Zero(plaintext);
                var failed = Finish(session, checks, failClosed: true, "Working copy hash mismatch.");
                return Block(session, failed, ReviewPhase.FailClosed, "Working copy hash mismatch.");
            }

            var copy = new ExpiringWorkingCopy
            {
                RecordId = record.Sealed.Id,
                Plaintext = plaintext,
                ExpiresAt = release.ExpiresAt,
                CaseId = bundle.CaseId,
            };
            if (copy.IsExpired(_clock.UtcNow))
            {
                Hashes.Zero(plaintext);
                var failed = Finish(session, checks, failClosed: true, "Working copy is expired.");
                return Block(session, failed, ReviewPhase.FailClosed, "Working copy is expired.");
            }

            copies.Add(copy);
            timelines.Add(_timelines.Build(record, copy));
        }

        var ok = Finish(session, checks, failClosed: false, null);
        session.AuditEvents.Add("working-copy-issued");
        return new ReviewOutcome
        {
            Session = session,
            Report = ok,
            Phase = ReviewPhase.Playback,
            DecryptAllowed = true,
            DisplayAllowed = true,
            WorkingCopies = copies,
            Timelines = timelines,
        };
    }

    public bool CanPlay(ExpiringWorkingCopy copy) => !copy.IsExpired(_clock.UtcNow);

    public DisclosurePack Export(RideBundle bundle, ReviewOutcome outcome)
    {
        if (outcome.Report.FailClosed && outcome.Phase == ReviewPhase.FailClosed && !outcome.Report.AllCustodyPassed)
        {
            throw new RideAuditFailClosedException(
                "VALIDATION_FAILED",
                "FR-RIDE-050",
                "Refusing disclosure export that would imply a court-ready plaintext package.");
        }

        var first = bundle.Records[0];
        return new DisclosurePack
        {
            BundleId = bundle.BundleId,
            SealedEnvelope = first.Sealed.Envelope.ToArray(),
            OtsCommittedDigest = first.Ots.CommittedDigestHex,
            AttestationTokenHash = first.Sealed.Receipt.AttestationTokenHash,
            Report = outcome.Report,
            ContainsPlaintext = false,
        };
    }

    private ViewerSession NewSession(RideBundle bundle, string reviewerRole) =>
        new()
        {
            SessionId = "vs-" + Guid.NewGuid().ToString("N"),
            ViewerBuild = "RideAudit.Client.Desktop/" + ViewerLogic.Version,
            ReviewerRole = reviewerRole,
            CaseId = bundle.CaseId,
            BundleId = bundle.BundleId,
            StartedAt = _clock.UtcNow,
            LogicVersion = ViewerLogic.Version,
            License = ViewerLogic.License,
        };

    private VerificationReport Finish(ViewerSession session, List<VerificationCheck> checks, bool failClosed, string? reason)
    {
        var report = new VerificationReport
        {
            ReportId = "vr-" + Guid.NewGuid().ToString("N"),
            ViewerSessionId = session.SessionId,
            CreatedAt = _clock.UtcNow,
            LogicVersion = ViewerLogic.Version,
            Checks = checks,
            FailClosed = failClosed,
        };
        session.Report = report;
        if (reason is not null)
        {
            session.AuditEvents.Add(reason);
        }

        return report;
    }

    private static ReviewOutcome Block(ViewerSession session, VerificationReport report, ReviewPhase phase, string reason) =>
        new()
        {
            Session = session,
            Report = report,
            Phase = phase,
            DecryptAllowed = false,
            DisplayAllowed = false,
            BlockReason = reason,
        };
}

public static class DesktopPortability
{
    public static string CurrentOs()
    {
        if (OperatingSystem.IsWindows())
        {
            return "Windows";
        }

        if (OperatingSystem.IsLinux())
        {
            return "Linux";
        }

        if (OperatingSystem.IsMacOS())
        {
            return "macOS";
        }

        return "other";
    }
}
