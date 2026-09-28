// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using RideAudit.Client.Core;
using RideAudit.Client.Tests.Support;
using RideAudit.Client.Seal;
using RideAudit.Viewer;
using Xunit;

namespace RideAudit.Client.Tests;

[Trait("Partition", "TEST-RIDE-027")]
public class TestRide027PlaybackTests
{
    [Fact]
    [Trait("FR", "FR-RIDE-047")]
    [Trait("AC", "AC-RIDE-047-001")]
    public void Playback_requires_hash_receipt_links_clock_overlay_and_attestation()
    {
        var fixture = Fixtures.CaptureSimulated();
        var outcome = Fixtures.ViewerForSimulated(fixture).Review(fixture.Bundle, "counsel", Fixtures.Release(fixture.Clock));
        Assert.Equal(ReviewPhase.Playback, outcome.Phase);
        Assert.Contains(outcome.Report.Checks, check => check.Name == "payload_hash" && check.Passed);
        Assert.Contains(outcome.Report.Checks, check => check.Name == "ots_receipt" && check.Passed);
        Assert.Contains(outcome.Report.Checks, check => check.Name == "source_stream_links" && check.Passed);
        Assert.Contains(outcome.Report.Checks, check => check.Name == "clock_offsets" && check.Passed);
        Assert.Contains(outcome.Report.Checks, check => check.Name == "overlay_timeline" && check.Passed);
        Assert.Contains(outcome.Report.Checks, check => check.Name == "play_attestation" && check.Passed);
        Assert.Contains(outcome.Report.Checks, check => check.Name == "source_payload_binding" && check.Passed);
        Assert.NotEmpty(outcome.WorkingCopies);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-047")]
    [Trait("AC", "AC-RIDE-047-002")]
    public void Inconsistent_components_are_reported_and_working_copy_expires()
    {
        var fixture = Fixtures.CaptureSimulated();
        var record = fixture.Bundle.Records[0];
        var brokenMeta = record.Sealed.Receipt.CompositeMetadata! with { OverlayManifestVersion = "" };
        var brokenReceipt = CopyReceipt(record.Sealed.Receipt, brokenMeta);
        var broken = Replace(record, brokenReceipt, record.Sealed.Envelope, record.Ots);
        var bundle = CopyBundle(fixture.Bundle, broken);
        var outcome = Fixtures.ViewerForSimulated(fixture).Review(bundle, "counsel", Fixtures.Release(fixture.Clock));
        Assert.False(outcome.DisplayAllowed);
        Assert.Contains(outcome.Report.Checks, check => check.Name == "overlay_timeline" && !check.Passed);
        Assert.Empty(outcome.WorkingCopies);

        var good = Fixtures.ViewerForSimulated(fixture).Review(fixture.Bundle, "counsel", Fixtures.Release(fixture.Clock));
        fixture.Clock.UtcNow = good.WorkingCopies[0].ExpiresAt;
        Assert.False(Fixtures.ViewerForSimulated(fixture).CanPlay(good.WorkingCopies[0]));
    }

    [Fact]
    [Trait("FR", "FR-RIDE-221")]
    [Trait("AC", "AC-RIDE-221-001")]
    public void Integrity_fields_are_preserved()
    {
        var receipt = Fixtures.CaptureHappy().Capture.SealedComposite.Receipt;
        var meta = receipt.CompositeMetadata!;
        Assert.False(string.IsNullOrWhiteSpace(meta.Codec));
        Assert.False(string.IsNullOrWhiteSpace(meta.OverlayManifestVersion));
        Assert.NotEmpty(meta.SourceContentHashes);
        Assert.False(string.IsNullOrWhiteSpace(meta.SyncClockOffset));
        Assert.False(string.IsNullOrWhiteSpace(receipt.ContentHash));
    }

    [Fact]
    [Trait("FR", "FR-RIDE-221")]
    [Trait("AC", "AC-RIDE-221-002")]
    public void Counsel_can_reproduce_the_hash_check()
    {
        var fixture = Fixtures.CaptureSimulated();
        var envelope = fixture.Bundle.Records[0].Sealed.Envelope;
        var recomputed = Hashes.Sha256Hex(envelope);
        Assert.Equal(fixture.Bundle.Records[0].Sealed.Receipt.ContentHash, recomputed);
        var outcome = Fixtures.ViewerForSimulated(fixture).Review(fixture.Bundle, "counsel", Fixtures.Release(fixture.Clock));
        Assert.True(outcome.Report.Checks.Single(check => check.Name == "payload_hash").Passed);
    }

    private static CustodyReceipt CopyReceipt(CustodyReceipt source, CompositeMetadata meta) =>
        new()
        {
            SessionId = source.SessionId,
            SealedRecordId = source.SealedRecordId,
            ContentHash = source.ContentHash,
            PlaintextContentHash = source.PlaintextContentHash,
            SealedAt = source.SealedAt,
            AttestationTokenHash = source.AttestationTokenHash,
            KeyScheme = source.KeyScheme,
            Composite = source.Composite,
            DeviceIds = source.DeviceIds,
            ChainId = source.ChainId,
            BlockchainTxHint = source.BlockchainTxHint,
            ChainWriteStatus = source.ChainWriteStatus,
            PackageIdentity = source.PackageIdentity,
            SigningCertDigest = source.SigningCertDigest,
            AttestationReference = source.AttestationReference,
            AttestationProvider = source.AttestationProvider,
            KeyId = source.KeyId,
            PublicKeyPem = source.PublicKeyPem,
            CollectorIdentity = source.CollectorIdentity,
            ProvenanceTag = source.ProvenanceTag,
            Algorithm = source.Algorithm,
            AlgorithmVersion = source.AlgorithmVersion,
            KeyScope = source.KeyScope,
            ScopeId = source.ScopeId,
            KeyBindingDigest = source.KeyBindingDigest,
            Nonce = source.Nonce,
            AdmissionPolicyId = source.AdmissionPolicyId,
            CompositeMetadata = meta,
            LinkedCompositeId = source.LinkedCompositeId,
            License = source.License,
            StubNotice = source.StubNotice,
        };

    private static ReviewRecord Replace(ReviewRecord record, CustodyReceipt receipt, byte[] envelope, OtsProof ots) =>
        new()
        {
            Sealed = new SealedRecord
            {
                Id = record.Sealed.Id,
                Envelope = envelope,
                Receipt = receipt,
                WrappedKey = record.Sealed.WrappedKey,
                Kind = record.Sealed.Kind,
                SealedBefore = record.Sealed.SealedBefore,
            },
            Ots = ots,
            Admitted = record.Admitted,
            GpsPresent = record.GpsPresent,
            Obd2Present = record.Obd2Present,
            Telematics = record.Telematics,
        };

    private static RideBundle CopyBundle(RideBundle bundle, ReviewRecord record) =>
        new()
        {
            BundleId = bundle.BundleId,
            CaseId = bundle.CaseId,
            VehicleId = bundle.VehicleId,
            Records = [record],
        };
}

[Trait("Partition", "TEST-RIDE-028")]
public class TestRide028ViewerTests
{
    [Fact]
    [Trait("FR", "FR-RIDE-049")]
    [Trait("AC", "AC-RIDE-049-001")]
    public void Desktop_project_targets_portable_net10_without_claiming_untested_hosts()
    {
        var csproj = File.ReadAllText(Path.Combine(Repo.Root(), "src", "RideAudit.Client.Desktop", "RideAudit.Client.Desktop.csproj"));
        Assert.Contains("net10.0", csproj);
        Assert.Contains("Avalonia.Desktop", csproj);
        Assert.DoesNotContain("win-x64", csproj);
        Assert.Contains(DesktopPortability.CurrentOs(), ViewerLogic.PortableTargets);
        Assert.Contains("Windows", ViewerLogic.PortableTargets);
        Assert.Contains("macOS", ViewerLogic.PortableTargets);
        var manifest = RideAudit.Licensing.PublicationClaimGuard.LoadEmbedded();
        Assert.Equal("not-produced", manifest.DesktopBuilds.Windows);
        Assert.Equal("not-produced", manifest.DesktopBuilds.Macos);
        Assert.False(manifest.DesktopBuilds.ReproducibleSignedClaim);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-049")]
    [Trait("AC", "AC-RIDE-049-002")]
    public void Decrypt_uses_escrow_release()
    {
        var fixture = Fixtures.CaptureSimulated();
        var escrow = new QuorumEscrow(fixture.EscrowPrivate, fixture.Clock);
        var viewer = new CourtViewer(new VerificationGate(fixture.Allowlist, fixture.Headers, allowSimulatedAttestation: true), escrow, fixture.Clock);
        var blocked = viewer.Review(fixture.Bundle, "counsel", null);
        Assert.False(blocked.DecryptAllowed);
        Assert.Equal(0, escrow.ReleaseAttempts);
        var opened = viewer.Review(fixture.Bundle, "counsel", Fixtures.Release(fixture.Clock));
        Assert.True(opened.DecryptAllowed);
        Assert.True(escrow.ReleaseAttempts >= 1);
        Assert.Equal(fixture.Bundle.CaseId, opened.WorkingCopies[0].CaseId);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-050")]
    [Trait("AC", "AC-RIDE-050-001")]
    public void Checks_run_before_decrypt()
    {
        var fixture = Fixtures.CaptureHappy();
        var escrow = new QuorumEscrow(fixture.EscrowPrivate, fixture.Clock);
        var viewer = new CourtViewer(new VerificationGate(fixture.Allowlist, fixture.Headers), escrow, fixture.Clock);
        var tampered = TamperEnvelope(fixture);
        var outcome = viewer.Review(tampered, "counsel", Fixtures.Release(fixture.Clock));
        Assert.Equal(0, escrow.ReleaseAttempts);
        Assert.False(outcome.DecryptAllowed);
        Assert.Contains(outcome.Report.Checks, check => check.Name == "payload_hash" && !check.Passed);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-050")]
    [Trait("AC", "AC-RIDE-050-002")]
    public void Failure_is_auditable_and_blocks_display()
    {
        var fixture = Fixtures.CaptureHappy(new RideAudit.PlayIntegrity.StubPlayIntegrityClient(RideAudit.PlayIntegrity.StubPlayMode.SimulatedSuccess));
        var outcome = Fixtures.ViewerFor(fixture).Review(fixture.Bundle, "counsel", Fixtures.Release(fixture.Clock));
        Assert.False(outcome.DisplayAllowed);
        Assert.NotNull(outcome.Session.Report);
        Assert.True(outcome.Report.FailClosed);
        Assert.Contains(outcome.Session.AuditEvents, evt => evt.Length > 0);
        Assert.Contains(outcome.Report.Checks, check => check.Name == "play_attestation" && !check.Passed);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-051")]
    [Trait("AC", "AC-RIDE-051-001")]
    public void Timeline_shows_available_tracks_and_labels_gaps()
    {
        var fixture = Fixtures.CaptureSimulated();
        var outcome = Fixtures.ViewerForSimulated(fixture).Review(fixture.Bundle, "counsel", Fixtures.Release(fixture.Clock));
        var timeline = outcome.Timelines[0];
        Assert.True(timeline.Composite.Present);
        Assert.True(timeline.Spider.Present);
        Assert.True(timeline.Telematics.Present);
        Assert.False(timeline.Gps.Present);
        Assert.Contains("absent", timeline.Gps.AbsenceReason, StringComparison.OrdinalIgnoreCase);
        Assert.False(timeline.Obd2.Present);
        Assert.Contains("absent", timeline.Obd2.AbsenceReason, StringComparison.OrdinalIgnoreCase);
        var later = timeline.CueAt(TimeSpan.FromMilliseconds(66));
        Assert.Contains("66", later.CompositeLabel);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-051")]
    [Trait("AC", "AC-RIDE-051-002")]
    public void Timeline_does_not_require_the_collection_device()
    {
        var fixture = Fixtures.CaptureSimulated();
        var outcome = Fixtures.ViewerForSimulated(fixture).Review(fixture.Bundle, "counsel", Fixtures.Release(fixture.Clock));
        Assert.False(outcome.Timelines[0].RequiresCollectionDevice);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-052")]
    [Trait("AC", "AC-RIDE-052-001")]
    public void Every_review_creates_a_session_and_report()
    {
        var fixture = Fixtures.CaptureHappy();
        var viewer = Fixtures.ViewerFor(fixture);
        var failed = viewer.Review(new RideBundle
        {
            BundleId = fixture.Bundle.BundleId,
            CaseId = fixture.Bundle.CaseId,
            VehicleId = fixture.Bundle.VehicleId,
            Records = [new ReviewRecord
            {
                Sealed = fixture.Bundle.Records[0].Sealed,
                Ots = fixture.Bundle.Records[0].Ots,
                Admitted = false,
            }],
        }, "counsel", null);
        Assert.False(string.IsNullOrWhiteSpace(failed.Session.SessionId));
        Assert.NotNull(failed.Report);
        var passed = viewer.Review(fixture.Bundle, "counsel", Fixtures.Release(fixture.Clock));
        Assert.NotEqual(failed.Session.SessionId, passed.Session.SessionId);
        Assert.Equal(passed.Session.SessionId, passed.Report.ViewerSessionId);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-052")]
    [Trait("AC", "AC-RIDE-052-002")]
    public void Viewer_logic_is_versioned_gpl()
    {
        Assert.Equal(LicenseMetadata.GplId, ViewerLogic.License);
        Assert.Equal("0.1.0", ViewerLogic.Version);
        Assert.Contains("verification", ViewerLogic.Components);
        Assert.Contains("decryption", ViewerLogic.Components);
        var outcome = ReviewHappy();
        Assert.Equal(ViewerLogic.Version, outcome.Report.LogicVersion);
        Assert.Equal(ViewerLogic.License, outcome.Session.License);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-222")]
    [Trait("AC", "AC-RIDE-222-002")]
    public void Viewer_does_not_decrypt_when_verification_fails()
    {
        var fixture = Fixtures.CaptureHappy();
        var outcome = Fixtures.ViewerFor(fixture).Review(TamperEnvelope(fixture), "counsel", Fixtures.Release(fixture.Clock));
        Assert.False(outcome.DecryptAllowed);
        Assert.False(outcome.DisplayAllowed);
        Assert.Empty(outcome.WorkingCopies);
        Assert.Equal(ReviewPhase.FailClosed, outcome.Phase);
    }

    [Fact]
    public void Sibling_failure_does_not_mark_the_good_record_passed_as_custody()
    {
        var good = Fixtures.CaptureSimulated();
        var badBundle = TamperEnvelope(good);
        var mixed = new RideBundle
        {
            BundleId = "mixed",
            CaseId = good.Bundle.CaseId,
            VehicleId = good.Bundle.VehicleId,
            Records = [good.Bundle.Records[0], badBundle.Records[0]],
        };
        var outcome = Fixtures.ViewerForSimulated(good).Review(mixed, "counsel", Fixtures.Release(good.Clock));
        Assert.False(outcome.DecryptAllowed);
        Assert.Contains(outcome.Report.Checks, check => check.RecordId == good.Bundle.Records[0].Sealed.Id && check.Name == "payload_hash" && check.Passed);
        Assert.Contains(outcome.Report.Checks, check => check.RecordId == badBundle.Records[0].Sealed.Id && check.Name == "payload_hash" && !check.Passed);
    }

    [Fact]
    public void Disclosure_pack_excludes_plaintext()
    {
        var fixture = Fixtures.CaptureSimulated();
        var viewer = Fixtures.ViewerForSimulated(fixture);
        var outcome = viewer.Review(fixture.Bundle, "counsel", Fixtures.Release(fixture.Clock));
        var pack = viewer.Export(fixture.Bundle, outcome);
        Assert.False(pack.ContainsPlaintext);
        Assert.False(Bytes.Contains(pack.SealedEnvelope, System.Text.Encoding.ASCII.GetBytes(Fixtures.Marker)));
        Assert.Equal(outcome.Report.ReportId, pack.Report.ReportId);
    }

    private static ReviewOutcome ReviewHappy()
    {
        var fixture = Fixtures.CaptureSimulated();
        return Fixtures.ViewerForSimulated(fixture).Review(fixture.Bundle, "counsel", Fixtures.Release(fixture.Clock));
    }

    private static RideBundle TamperEnvelope(SealedFixture fixture)
    {
        var record = fixture.Bundle.Records[0];
        var envelope = record.Sealed.Envelope.ToArray();
        envelope[^1] ^= 0xFF;
        return new RideBundle
        {
            BundleId = fixture.Bundle.BundleId,
            CaseId = fixture.Bundle.CaseId,
            VehicleId = fixture.Bundle.VehicleId,
            Records =
            [
                new ReviewRecord
                {
                    Sealed = new SealedRecord
                    {
                        Id = record.Sealed.Id,
                        Envelope = envelope,
                        Receipt = record.Sealed.Receipt,
                        WrappedKey = record.Sealed.WrappedKey,
                        Kind = record.Sealed.Kind,
                        SealedBefore = record.Sealed.SealedBefore,
                    },
                    Ots = record.Ots,
                    Admitted = true,
                    Telematics = record.Telematics,
                },
            ],
        };
    }
}
