using RideAudit.Chain;
using RideAudit.Chain.EthL2;
using RideAudit.Chain.OpenTimestamps;
using RideAudit.Contracts;
using RideAudit.Server.Admission;
using RideAudit.TestSupport;

namespace RideAudit.Chain.Tests;

/// <summary>
/// TEST-RIDE-014. FR-RIDE-017, FR-RIDE-018, FR-RIDE-212, TR-RIDE-CHAIN-001, TR-RIDE-CHAIN-002.
/// Confirmation metadata comes from the documented fixture, not a live Bitcoin transaction.
/// </summary>
public class TestRide014CustodyReceipt
{
    [Fact]
    [Trait("TEST", "TEST-RIDE-014")]
    [Trait("FR", "FR-RIDE-017")]
    [Trait("FR", "FR-RIDE-018")]
    [Trait("AC", "AC-RIDE-017-001")]
        [Trait("AC", "AC-RIDE-018-002")]
        [Trait("AC", "AC-RIDE-CHAIN-001-001")]
        [Trait("AC", "AC-RIDE-CHAIN-002-001")]
        [Trait("AC", "AC-RIDE-CHAIN-004-001")]
    public void Confirmed_fixture_receipt_records_hash_key_collector_and_anchor_metadata()
    {
        var world = ServerWorld.Create();
        var enrolled = world.Enroll();
        var ready = world.SealReady(enrolled.Driver, enrolled.Session);
        var outcome = world.Submit(enrolled.Driver, ready.Package, ready.Token, ready.Nonce);

        Assert.True(outcome.Admitted);
        Assert.Equal(CustodyState.Admitted, outcome.State);
        var core = ready.Package.Receipt.Core;
        Assert.Equal(32, core.ContentHash.Length);
        Assert.True(core.PublicKeyMaterial.Length > 0);
        Assert.Equal(enrolled.Driver.DriverId, core.CollectorId);
        Assert.True(core.CollectionUnixMillis > 0);
        Assert.Equal(RideAuditPolicy.ProvenanceTag, core.ProvenanceTag);
        Assert.NotNull(outcome.Anchor);
        Assert.Equal("upgraded", outcome.Anchor!.Status);
        Assert.Equal(DocumentedFixtureOtsCalendar.ChainId, outcome.Anchor.ChainId);
        Assert.Equal(DocumentedFixtureOtsCalendar.TransactionReference, outcome.Anchor.TransactionReference);
        Assert.Equal(DocumentedFixtureOtsCalendar.BlockHeight, outcome.Anchor.BlockHeight);
        Assert.Equal(DocumentedFixtureOtsCalendar.WriteTimeUnixMillis, outcome.Anchor.WriteTimeUnixMillis);
        Assert.False(outcome.Anchor.LiveBitcoinMetadata);
        Assert.StartsWith("fixture:", outcome.Anchor.TransactionReference, StringComparison.Ordinal);
        Assert.DoesNotMatch("^[0-9a-fA-F]{64}$", outcome.Anchor.TransactionReference);
    }

    [Fact]
    [Trait("TEST", "TEST-RIDE-014")]
    [Trait("FR", "FR-RIDE-212")]
    [Trait("AC", "AC-RIDE-212-001")]
    [Trait("AC", "AC-RIDE-212-002")]
    public void Eth_l2_profile_is_selectable_and_fails_closed_without_fabricated_transactions()
    {
        var world = ServerWorld.Create();
        var driver = world.Register();
        var vehicle = world.AddVehicle(driver);
        world.PutProfile(driver, vehicle.VehicleId, ChainProfileIds.EthL2Base, valid: true);
        var session = world.OpenSession(driver, vehicle.VehicleId);
        var ready = world.SealReady(driver, session);
        var ex = Assert.Throws<RideAuditException>(() => world.Submit(driver, ready.Package, ready.Token, ready.Nonce));
        Assert.Equal(ErrorCodes.ChainProfileUnsupported, ex.Code);
        var record = world.App.Journal.Find(ex.SubmissionId!);
        Assert.NotNull(record);
        Assert.NotEqual(CustodyState.Admitted, record!.State);
        Assert.False(record.CollectionComplete);
        Assert.False(record.Anchor!.HasTransactionReference);
        Assert.Equal(ready.Package.Ciphertext, record.Ciphertext);
    }

    [Fact]
    [Trait("TEST", "TEST-RIDE-014")]
    public void Pending_proof_and_substituted_digest_do_not_confirm()
    {
        var world = ServerWorld.Create();
        world.App.Calendar!.Mode = FixtureCalendarMode.PendingForever;
        var enrolled = world.Enroll();
        var ready = world.SealReady(enrolled.Driver, enrolled.Session);
        var ex = Assert.Throws<RideAuditException>(() => world.Submit(enrolled.Driver, ready.Package, ready.Token, ready.Nonce));
        Assert.Equal(ErrorCodes.ChainUnconfirmed, ex.Code);
        var record = world.App.Journal.Find(ex.SubmissionId!);
        Assert.Equal("pending", record!.Anchor!.Status);
        Assert.False(record.Anchor.HasTransactionReference);

        var upgraded = ServerWorld.Create();
        var ok = upgraded.Enroll();
        var sealedOk = upgraded.SealReady(ok.Driver, ok.Session);
        var admitted = upgraded.Submit(ok.Driver, sealedOk.Package, sealedOk.Token, sealedOk.Nonce);
        var swapped = admitted.Anchor!.Clone();
        swapped.ReceiptCoreDigest = Google.Protobuf.ByteString.CopyFrom(new byte[32]);
        var verification = AnchorProofVerifier.Verify(sealedOk.Package.ReceiptCoreBytes, sealedOk.Package.Ciphertext, swapped);
        Assert.False(verification.Confirmed);
        Assert.Contains(verification.Mismatches, mismatch => mismatch.Contains("digest", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    [Trait("TEST", "TEST-RIDE-014")]
    [Trait("FR", "FR-RIDE-212")]
    [Trait("AC", "AC-RIDE-212-003")]
    public void Documented_l2_and_dual_fixtures_admit_without_live_transaction_ids()
    {
        foreach (var profile in new[] { ChainProfileIds.EthL2Base, ChainProfileIds.EthL2Polygon, ChainProfileIds.DualBtcOtsL2 })
        {
            var world = ServerWorld.Create(fixtureL2: true);
            var driver = world.Register();
            var vehicle = world.AddVehicle(driver);
            world.PutProfile(driver, vehicle.VehicleId, profile, valid: true);
            var session = world.OpenSession(driver, vehicle.VehicleId);
            var ready = world.SealReady(driver, session);
            var outcome = world.Submit(driver, ready.Package, ready.Token, ready.Nonce);
            Assert.True(outcome.Admitted);
            Assert.NotNull(outcome.Anchor);
            Assert.StartsWith("fixture:", outcome.Anchor!.TransactionReference, StringComparison.Ordinal);
            Assert.False(outcome.Anchor.LiveBitcoinMetadata);
            Assert.DoesNotMatch("^(0x)?[0-9a-fA-F]{64}$", outcome.Anchor.TransactionReference);
            Assert.DoesNotMatch("^[0-9a-fA-F]{64}$", outcome.Anchor.TransactionReference);
        }
    }

    [Fact]
    [Trait("TEST", "TEST-RIDE-014")]
    [Trait("FR", "FR-RIDE-212")]
    public void Unconfigured_polygon_and_dual_profiles_write_no_transaction_reference()
    {
        foreach (var profile in new[] { ChainProfileIds.EthL2Polygon, ChainProfileIds.DualBtcOtsL2 })
        {
            var world = ServerWorld.Create();
            var driver = world.Register();
            var vehicle = world.AddVehicle(driver);
            world.PutProfile(driver, vehicle.VehicleId, profile, valid: true);
            var session = world.OpenSession(driver, vehicle.VehicleId);
            var ready = world.SealReady(driver, session);
            var ex = Assert.Throws<RideAuditException>(() => world.Submit(driver, ready.Package, ready.Token, ready.Nonce));
            var record = world.App.Journal.Find(ex.SubmissionId!);
            Assert.NotEqual(CustodyState.Admitted, record!.State);
            Assert.False(record.Anchor!.HasTransactionReference);
            Assert.False(record.Anchor.LiveBitcoinMetadata);
            Assert.Equal(ready.Package.Ciphertext, record.Ciphertext);
        }
    }

    [Fact]
    [Trait("TEST", "TEST-RIDE-014")]
    [Trait("FR", "FR-RIDE-212")]
    public void Dual_anchor_omits_transaction_metadata_when_the_l2_leg_fails()
    {
        var world = ServerWorld.Create(fixtureL2: true);
        world.App.L2!.Mode = L2FixtureMode.FailSubmit;
        var driver = world.Register();
        var vehicle = world.AddVehicle(driver);
        world.PutProfile(driver, vehicle.VehicleId, ChainProfileIds.DualBtcOtsL2, valid: true);
        var session = world.OpenSession(driver, vehicle.VehicleId);
        var ready = world.SealReady(driver, session);
        var ex = Assert.Throws<RideAuditException>(() => world.Submit(driver, ready.Package, ready.Token, ready.Nonce));
        Assert.Equal(ErrorCodes.ChainFailed, ex.Code);
        var anchor = world.App.Journal.Find(ex.SubmissionId!)!.Anchor!;
        Assert.False(anchor.HasTransactionReference);
        Assert.False(anchor.HasChainId);
        Assert.False(anchor.LiveBitcoinMetadata);
    }
}

/// <summary>
/// TEST-RIDE-015. FR-RIDE-019. Non-admission, unchanged ciphertext, operator alert.
/// </summary>
public class TestRide015ChainFailure
{
    [Fact]
    [Trait("TEST", "TEST-RIDE-015")]
    [Trait("FR", "FR-RIDE-019")]
    [Trait("AC", "AC-RIDE-019-001")]
    [Trait("AC", "AC-RIDE-019-002")]
        [Trait("AC", "AC-RIDE-019-003")]
        [Trait("AC", "AC-RIDE-CHAIN-003-001")]
        [Trait("AC", "AC-RIDE-CHAIN-003-002")]
    public void Chain_failure_quarantines_without_rewriting_ciphertext_and_retry_can_confirm()
    {
        var world = ServerWorld.Create();
        world.App.Calendar!.Mode = FixtureCalendarMode.FailSubmit;
        var enrolled = world.Enroll();
        var ready = world.SealReady(enrolled.Driver, enrolled.Session);
        var idem = "idem-chain-retry";
        var ex = Assert.Throws<RideAuditException>(() => world.Submit(enrolled.Driver, ready.Package, ready.Token, ready.Nonce, idem));
        Assert.Equal(ErrorCodes.ChainFailed, ex.Code);
        var record = world.App.Journal.Find(ex.SubmissionId!);
        Assert.Equal(CustodyState.Quarantined, record!.State);
        Assert.False(record.CollectionComplete);
        Assert.Equal(ready.Package.Ciphertext, record.Ciphertext);
        Assert.False(record.Anchor!.HasTransactionReference);
        Assert.NotEmpty(world.App.Alerts.Alerts);
        Assert.Contains(world.App.Journal.FailureAudit, line => line.Contains(ErrorCodes.ChainFailed, StringComparison.Ordinal));

        var snapshot = record.Ciphertext.ToArray();
        world.App.Calendar.Mode = FixtureCalendarMode.UpgradeImmediately;
        var outcome = world.Submit(enrolled.Driver, ready.Package, ready.Token, ready.Nonce, idem);
        Assert.True(outcome.Admitted);
        Assert.Equal(snapshot, world.App.Journal.Find(outcome.SubmissionId)!.Ciphertext);
        Assert.Equal(ex.SubmissionId, outcome.SubmissionId);
    }

    [Fact]
    [Trait("TEST", "TEST-RIDE-015")]
    public void Restart_preserves_quarantine_ciphertext_and_receipt_core()
    {
        var world = ServerWorld.Create();
        world.App.Calendar!.Mode = FixtureCalendarMode.FailSubmit;
        var enrolled = world.Enroll();
        var ready = world.SealReady(enrolled.Driver, enrolled.Session);
        var ex = Assert.Throws<RideAuditException>(() => world.Submit(enrolled.Driver, ready.Package, ready.Token, ready.Nonce));
        var exported = world.App.Journal.Export();
        var restarted = new CustodyJournal();
        restarted.Import(exported);
        var record = restarted.Find(ex.SubmissionId!);
        Assert.Equal(CustodyState.Quarantined, record!.State);
        Assert.False(record.CollectionComplete);
        Assert.Equal(ready.Package.Ciphertext, record.Ciphertext);
        Assert.Equal(ready.Package.ReceiptCoreBytes, record.ReceiptCoreBytes);
    }

    [Fact]
    [Trait("TEST", "TEST-RIDE-014")]
    public void Unconfigured_calendar_does_not_invent_transaction_metadata()
    {
        var world = ServerWorld.Create(fixtureCalendar: false);
        var enrolled = world.Enroll();
        var ready = world.SealReady(enrolled.Driver, enrolled.Session);
        var ex = Assert.Throws<RideAuditException>(() => world.Submit(enrolled.Driver, ready.Package, ready.Token, ready.Nonce));
        Assert.Equal(ErrorCodes.ChainFailed, ex.Code);
        var anchor = world.App.Journal.Find(ex.SubmissionId!)!.Anchor!;
        Assert.Equal("unconfigured", anchor.ProofSource);
        Assert.False(anchor.HasTransactionReference);
        Assert.False(anchor.LiveBitcoinMetadata);
    }
}

/// <summary>
/// FR-RIDE-213 seal/receipt latency budget. TEST-RIDE-032 is killed and FR-RIDE-213 maps to no TEST (Payton 2026-10-08).
/// </summary>
public class TestRide032Latency
{
    [Fact]
    [Trait("FR", "FR-RIDE-213")]
    [Trait("AC", "AC-RIDE-213-001")]
    [Trait("AC", "AC-RIDE-213-002")]
    public void Latency_budget_blocks_admission_until_a_later_attempt_is_inside_budget()
    {
        var timer = new ScriptedTimer { Next = TimeSpan.FromHours(1) };
        var world = ServerWorld.Create(timer: timer);
        var enrolled = world.Enroll();
        var ready = world.SealReady(enrolled.Driver, enrolled.Session);
        var idem = "idem-latency";
        var ex = Assert.Throws<RideAuditException>(() => world.Submit(enrolled.Driver, ready.Package, ready.Token, ready.Nonce, idem));
        Assert.Equal(ErrorCodes.LatencyBudgetExceeded, ex.Code);
        Assert.Contains(world.App.Latency.Samples, sample => sample.Breached);
        Assert.False(world.App.Journal.Find(ex.SubmissionId!)!.CollectionComplete);

        timer.Next = TimeSpan.FromMilliseconds(5);
        var outcome = world.Submit(enrolled.Driver, ready.Package, ready.Token, ready.Nonce, idem);
        Assert.True(outcome.Admitted);
        Assert.True(outcome.CollectionComplete);
    }
}
