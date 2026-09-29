// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Reflection;
using System.Text;
using RideAudit.Chain;
using RideAudit.Chain.OpenTimestamps;
using RideAudit.Contracts;
using RideAudit.Ingest;
using RideAudit.TestSupport;

namespace RideAudit.Workflow.Tests;

public class ClassAMissingAcTests
{
    [Fact]
    [Trait("AC", "AC-TEST-001-002")]
    [Trait("AC", "AC-RIDE-002-002")]
    public void Unreadable_exports_and_unconsented_manual_scores_fail_closed()
    {
        var world = ServerWorld.Create();
        var driver = world.Register();
        var broken = Assert.Throws<RideAuditException>(() => world.App.Ingest.IngestPrivacyExport(
            TestRide001Through006And011And030.Consent(driver.DriverId),
            new byte[] { 1, 2, 3, 4 },
            "driver-upload"));
        Assert.Equal(ErrorCodes.ImportRejected, broken.Code);
        Assert.Empty(world.App.Imports.Imports);

        var denied = TestRide001Through006And011And030.Consent(driver.DriverId) with { ConsentGranted = false, ConsentStatement = "" };
        var manual = Assert.Throws<RideAuditException>(() => world.App.Ingest.RecordManualScore(denied, 1_700_000_000_000, 70, 71, 72, 73, 74));
        Assert.Equal(ErrorCodes.ConsentRequired, manual.Code);
        var stored = world.App.Ingest.RecordManualScore(
            TestRide001Through006And011And030.Consent(driver.DriverId),
            1_700_000_000_000,
            70, 71, 72, 73, 74);
        Assert.Equal(ProvenanceTags.Manual, stored.Provenance);
        var score = Assert.Single(world.App.Imports.Scores);
        Assert.Equal(1_700_000_000_000, score.ObservedUnixMillis);
        Assert.Equal(71, score.GentleBraking);
        Assert.Contains(world.App.Imports.Consents, row => row.ImportId == stored.ImportId && row.Granted);
    }

    [Fact]
    [Trait("AC", "AC-RIDE-003-002")]
    [Trait("AC", "AC-RIDE-INGEST-003-002")]
    [Trait("AC", "AC-TEST-003-002")]
    public void Incomplete_trip_rows_are_flagged_and_not_invented()
    {
        var world = ServerWorld.Create();
        var driver = world.Register();
        var zip = TestRide001Through006And011And030.Zip(
            ("DataDictionary.csv", "file,signal,availability\ntrips.csv,trip_records,collected\n"),
            ("trips.csv", "trip_id,started_unix_millis,ended_unix_millis,status\ntrip-1,1700000000000,1700003600000,droppedOff\n,,,\n"));
        var result = world.App.Ingest.IngestPrivacyExport(TestRide001Through006And011And030.Consent(driver.DriverId), zip, "driver-upload");
        Assert.Equal("imported-with-gap", result.Status);
        Assert.Contains("not invented", result.GapNotice, StringComparison.OrdinalIgnoreCase);
        var trip = Assert.Single(world.App.Imports.Trips);
        Assert.Equal("trip-1", trip.TripId);
        Assert.Equal(1700000000000, trip.StartedUnixMillis);
        Assert.DoesNotContain(world.App.Imports.Trips, row => row.TripId.Length == 0);
    }

    [Fact]
    [Trait("AC", "AC-RIDE-018-001")]
    [Trait("AC", "AC-RIDE-018-003")]
    [Trait("AC", "AC-RIDE-023-002")]
    [Trait("AC", "AC-UC-009-001")]
    [Trait("AC", "AC-UC-009-002")]
    [Trait("AC", "AC-UC-015-001")]
    [Trait("AC", "AC-TEST-013-001")]
    [Trait("AC", "AC-TEST-013-002")]
    [Trait("AC", "AC-TEST-014-001")]
    [Trait("AC", "AC-TEST-014-002")]
    [Trait("AC", "AC-TEST-015-001")]
    [Trait("AC", "AC-TEST-015-002")]
    [Trait("AC", "AC-TEST-022-001")]
    [Trait("AC", "AC-RIDE-204-002")]
    public void Seal_receipt_chain_profile_and_idempotent_submit_stay_labeled()
    {
        var world = ServerWorld.Create(fixtureL2: true);
        var enrolled = world.Enroll();
        var ready = world.SealReady(enrolled.Driver, enrolled.Session);
        Assert.NotEmpty(ready.Package.ReceiptCoreBytes);
        Assert.NotEmpty(ready.Package.Ciphertext);
        Assert.False(ready.Package.Ciphertext.AsSpan().SequenceEqual(ready.Plaintext));
        Assert.StartsWith("key-", ready.Package.KeyId, StringComparison.Ordinal);
        Assert.DoesNotContain(ready.Package.GetType().GetProperties().Select(property => property.Name), name => name.Contains("Private", StringComparison.Ordinal));
        var dumpBefore = Encoding.UTF8.GetString(world.App.Identity.Database.DumpForAudit());
        Assert.DoesNotContain("PRIVATE KEY", dumpBefore, StringComparison.Ordinal);

        var first = world.Submit(enrolled.Driver, ready.Package, ready.Token, ready.Nonce, "idem-same");
        var again = world.Submit(enrolled.Driver, ready.Package, ready.Token, ready.Nonce, "idem-same");
        Assert.True(first.Admitted);
        Assert.Equal(first.SubmissionId, again.SubmissionId);
        var anchor = first.Anchor ?? throw new InvalidOperationException("Admitted submit has no anchor.");
        var confirmed = AnchorProofVerifier.Verify(ready.Package.ReceiptCoreBytes, ready.Package.Ciphertext, anchor);
        Assert.True(confirmed.Confirmed);
        Assert.StartsWith("fixture:", anchor.TransactionReference, StringComparison.Ordinal);

        world.PutProfile(enrolled.Driver, enrolled.Vehicle.VehicleId, ChainProfileIds.EthL2Base);
        var l2Ready = world.SealReady(enrolled.Driver, enrolled.Session);
        var l2 = world.Submit(enrolled.Driver, l2Ready.Package, l2Ready.Token, l2Ready.Nonce, "idem-l2");
        Assert.True(l2.Admitted);
        Assert.Contains("fixture:", l2.Anchor!.TransactionReference, StringComparison.Ordinal);
        Assert.NotEqual(ChainProfileIds.BtcOts, world.App.Identity.GetProfile(world.Require(enrolled.Driver), enrolled.Vehicle.VehicleId)!.ChainProfile);

        var unknown = Assert.Throws<RideAuditException>(() => world.PutProfile(enrolled.Driver, enrolled.Vehicle.VehicleId, "solana-mainnet"));
        Assert.Equal(ErrorCodes.ValidationFailed, unknown.Code);

        var unconfigured = ServerWorld.Create(fixtureL2: false);
        var person = unconfigured.Enroll();
        unconfigured.PutProfile(person.Driver, person.Vehicle.VehicleId, ChainProfileIds.EthL2Base);
        var blockedReady = unconfigured.SealReady(person.Driver, person.Session);
        var blocked = Assert.Throws<RideAuditException>(() => unconfigured.Submit(person.Driver, blockedReady.Package, blockedReady.Token, blockedReady.Nonce, "idem-nol2"));
        Assert.False(string.IsNullOrWhiteSpace(blocked.Code));
        Assert.DoesNotContain("lyft.com", blocked.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("AC", "AC-RIDE-PERF-001-001")]
    [Trait("AC", "AC-RIDE-PERF-001-002")]
    public void Chain_confirmation_budget_is_monitored_and_blocks_admission()
    {
        var world = ServerWorld.Create();
        var enrolled = world.Enroll();
        var ready = world.SealReady(enrolled.Driver, enrolled.Session);
        var monitor = new LatencyMonitor();
        var slow = new AnchoringPolicy(monitor, new FixedElapsed(TimeSpan.FromMinutes(5)), TimeSpan.FromMinutes(2));
        var anchor = new BtcOtsAnchor(world.App.Calendar);
        var request = new AnchorRequest(ready.Package.ReceiptCoreBytes, ready.Package.ReceiptCoreDigest, RideAuditPolicy.Version, ChainProfileIds.BtcOts);
        var breached = slow.Anchor(anchor, request);
        Assert.False(breached.Confirmed);
        Assert.Equal(ErrorCodes.LatencyBudgetExceeded, breached.FailureCode);
        Assert.Contains(monitor.Samples, sample => sample.Breached && sample.Budget == TimeSpan.FromMinutes(2));

        var fastMonitor = new LatencyMonitor();
        var fast = new AnchoringPolicy(fastMonitor, new FixedElapsed(TimeSpan.FromSeconds(1)), TimeSpan.FromMinutes(2));
        var admitted = fast.Anchor(anchor, request);
        Assert.True(admitted.Confirmed);
        Assert.Contains(fastMonitor.Samples, sample => !sample.Breached);
    }

    private sealed class FixedElapsed : IElapsedTimer
    {
        private readonly TimeSpan _elapsed;
        public FixedElapsed(TimeSpan elapsed) => _elapsed = elapsed;
        public TimeSpan Measure(string phase, Action action)
        {
            action();
            return _elapsed;
        }
    }
}
