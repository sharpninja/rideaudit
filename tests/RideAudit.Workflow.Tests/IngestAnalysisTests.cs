using System.Globalization;
using System.IO.Compression;
using System.Text;
using RideAudit.Anal;
using RideAudit.Chain.EthL2;
using RideAudit.Contracts;
using RideAudit.Ingest;
using RideAudit.Privacy;
using RideAudit.Sec;
using RideAudit.Server.Admission;
using RideAudit.Server.Counsel;
using RideAudit.TestSupport;

namespace RideAudit.Workflow.Tests;

public class TestRide001Through006And011And030
{
    private const string Marker = "RAWZIP-MARKER-9f3a";

    [Fact]
    [Trait("TEST", "TEST-RIDE-001")]
    [Trait("FR", "FR-RIDE-001")]
        [Trait("AC", "AC-RIDE-001-001")]
        [Trait("AC", "AC-RIDE-001-002")]
        [Trait("AC", "AC-RIDE-001-003")]
        [Trait("AC", "AC-UC-001-001")]
        [Trait("AC", "AC-UC-001-002")]
        [Trait("AC", "AC-TEST-001-001")]
        [Trait("AC", "AC-RIDE-INGEST-001-001")]
        [Trait("AC", "AC-RIDE-INGEST-001-002")]
        [Trait("AC", "AC-RIDE-INGEST-006-001")]
        [Trait("AC", "AC-RIDE-STORE-001-001")]
    public void Privacy_export_parses_dictionary_and_marks_unknown_files_unverified()
    {
        var world = ServerWorld.Create();
        var driver = world.Register();
        var zip = ExportZip(includeScores: false, includeUnknown: true);
        var result = world.App.Ingest.IngestPrivacyExport(Consent(driver.DriverId), zip, "driver-upload");
        Assert.Equal("imported-with-gap", result.Status);
        Assert.Equal(ProvenanceTags.PrivacyExport, result.Provenance);
        Assert.Contains(world.App.Imports.Unverified, row => row.Name.EndsWith("note.txt", StringComparison.Ordinal));
        Assert.Single(world.App.Imports.Trips);
        Assert.Empty(world.App.Imports.Scores);
        Assert.DoesNotContain(Marker, world.App.Imports.AuditText(), StringComparison.Ordinal);
        Assert.True(world.App.Imports.Imports[0].Ciphertext.AsSpan().IndexOf(Encoding.UTF8.GetBytes(Marker)) < 0);
    }

    [Fact]
    [Trait("TEST", "TEST-RIDE-002")]
    [Trait("FR", "FR-RIDE-002")]
        [Trait("AC", "AC-RIDE-002-001")]
        [Trait("AC", "AC-RIDE-002-003")]
        [Trait("AC", "AC-UC-002-001")]
        [Trait("AC", "AC-UC-002-002")]
        [Trait("AC", "AC-TEST-002-001")]
        [Trait("AC", "AC-TEST-002-002")]
        [Trait("AC", "AC-RIDE-INGEST-002-001")]
        [Trait("AC", "AC-RIDE-INGEST-002-002")]
    public void Smooth_cruiser_scores_come_only_from_export_fields_or_manual_entry()
    {
        var world = ServerWorld.Create();
        var driver = world.Register();
        var scored = world.App.Ingest.IngestPrivacyExport(Consent(driver.DriverId), ExportZip(includeScores: true, includeUnknown: false), "driver-upload");
        Assert.Equal("imported", scored.Status);
        var score = Assert.Single(world.App.Imports.Scores);
        Assert.Equal(80, score.Overall);
        Assert.Equal(81, score.GentleBraking);
        Assert.Equal(82, score.SmoothSteering);
        Assert.Equal(83, score.PhoneMount);
        Assert.Equal(84, score.SpeedVsArea);
        Assert.Equal(ProvenanceTags.PrivacyExport, score.Provenance);

        var manual = world.App.Ingest.RecordManualScore(Consent(driver.DriverId), 1_700_000_000_000, 70, 71, 72, 73, 74);
        Assert.Equal(ProvenanceTags.Manual, manual.Provenance);
        Assert.Equal(2, world.App.Imports.Scores.Count);
        var mislabeled = Zip(("DataDictionary.csv", "file,signal,availability\nscores.csv,smooth_cruiser,collected\n"),
            ("scores.csv", "observed_unix_millis,overall,gentle_braking,smooth_steering,phone_mount,speed_vs_area,source\n1700000001000,1,1,1,1,1,lyft_concierge_api\n"));
        var rejected = world.App.Ingest.IngestPrivacyExport(Consent(driver.DriverId), mislabeled, "driver-upload");
        Assert.Equal("imported-with-gap", rejected.Status);
        Assert.Equal(2, world.App.Imports.Scores.Count);
        Assert.DoesNotContain(world.App.Imports.Scores, row => row.Provenance == "lyft_concierge_api");
    }

    [Fact]
    [Trait("TEST", "TEST-RIDE-003")]
    [Trait("FR", "FR-RIDE-003")]
    [Trait("AC", "AC-RIDE-003-001")]
    [Trait("AC", "AC-RIDE-INGEST-003-001")]
    [Trait("AC", "AC-TEST-003-001")]
    public void Trip_rows_keep_ids_and_provenance()
    {
        var world = ServerWorld.Create();
        var driver = world.Register();
        world.App.Ingest.IngestPrivacyExport(Consent(driver.DriverId), ExportZip(includeScores: false, includeUnknown: false), "driver-upload");
        var trip = Assert.Single(world.App.Imports.Trips);
        Assert.Equal("trip-1", trip.TripId);
        Assert.Equal(ProvenanceTags.PrivacyExport, trip.Provenance);
        Assert.True(trip.EndedUnixMillis > trip.StartedUnixMillis);
    }


    [Fact]
    [Trait("TEST", "TEST-RIDE-001")]
    [Trait("FR", "FR-RIDE-011")]
    [Trait("AC", "AC-RIDE-011-001")]
    [Trait("AC", "AC-RIDE-011-002")]
    public void RideAudit_Ingest_path_has_no_HttpClient_and_no_lyft_com_private_api_scrape()
    {
        // Natural replacement for the Concierge partnership half deleted in 2e0670e.
        // FR-RIDE-011: product must not call undocumented Lyft private APIs.
        var root = ServerWorld.RepoRoot();
        var ingestDir = Path.Combine(root, "src/RideAudit.Ingest");
        var sources = string.Join('\n', Directory.GetFiles(ingestDir, "*.cs").Select(File.ReadAllText));
        Assert.DoesNotContain("HttpClient", sources, StringComparison.Ordinal);
        Assert.DoesNotContain("lyft.com", sources, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("undocumented Lyft private APIs", IngestSlice.Constraint, StringComparison.Ordinal);
    }


    [Fact]
    [Trait("TEST", "TEST-RIDE-005")]
    [Trait("FR", "FR-RIDE-005")]
    [Trait("AC", "AC-RIDE-005-001")]
    [Trait("AC", "AC-RIDE-005-002")]
    [Trait("AC", "AC-RIDE-005-003")]
    [Trait("AC", "AC-RIDE-INGEST-005-001")]
    [Trait("AC", "AC-RIDE-INGEST-005-002")]
    [Trait("AC", "AC-UC-004-001")]
    [Trait("AC", "AC-UC-004-002")]
    [Trait("AC", "AC-TEST-005-001")]
    [Trait("AC", "AC-TEST-005-002")]
    public void Third_party_samples_keep_their_own_provenance()
    {
        var world = ServerWorld.Create();
        var driver = world.Register();
        var csv = "observed_unix_millis,latitude,longitude,speed_mps\n1700000000000,37.7749,-122.4194,1.5\n";
        var result = world.App.Ingest.IngestThirdParty(Consent(driver.DriverId), Encoding.UTF8.GetBytes(csv), "mileage-export");
        Assert.Equal(ProvenanceTags.ThirdParty, result.Provenance);
        var sample = Assert.Single(world.App.Imports.Locations);
        Assert.Equal("third-party-sample", sample.MetricKind);
        Assert.NotEqual(ProvenanceTags.PrivacyExport, sample.Provenance);
        Assert.Empty(world.App.Imports.Scores);
    }

    [Fact]
    [Trait("TEST", "TEST-RIDE-006")]
    [Trait("FR", "FR-RIDE-006")]
    [Trait("AC", "AC-RIDE-006-001")]
    [Trait("AC", "AC-RIDE-006-002")]
    [Trait("AC", "AC-TEST-006-001")]
    [Trait("AC", "AC-TEST-006-002")]
    public void Import_without_consent_is_rejected_and_a_granted_consent_is_ledgered()
    {
        var world = ServerWorld.Create();
        var driver = world.Register();
        var denied = Consent(driver.DriverId) with { ConsentGranted = false, ConsentStatement = "" };
        var ex = Assert.Throws<RideAuditException>(() => world.App.Ingest.IngestPrivacyExport(denied, ExportZip(false, false), "driver-upload"));
        Assert.Equal(ErrorCodes.ConsentRequired, ex.Code);
        Assert.Empty(world.App.Imports.Imports);

        world.App.Ingest.IngestPrivacyExport(Consent(driver.DriverId), ExportZip(false, false), "driver-upload");
        var consent = Assert.Single(world.App.Imports.Consents);
        Assert.Equal("US-CA", consent.Jurisdiction);
        Assert.Equal("court-audit", consent.Purpose);
        Assert.True(consent.Granted);
        Assert.Contains("I consent", consent.Statement, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("TEST", "TEST-RIDE-011")]
    [Trait("FR", "FR-RIDE-013")]
    [Trait("AC", "AC-RIDE-013-001")]
    [Trait("AC", "AC-RIDE-013-002")]
    [Trait("AC", "AC-TEST-011-001")]
    [Trait("AC", "AC-TEST-011-002")]
    public void Raw_import_hash_and_version_are_stable_across_a_retry()
    {
        var world = ServerWorld.Create();
        var driver = world.Register();
        var zip = ExportZip(true, false);
        var first = world.App.Ingest.IngestPrivacyExport(Consent(driver.DriverId), zip, "driver-upload");
        var again = world.App.Ingest.IngestPrivacyExport(Consent(driver.DriverId), zip, "driver-upload");
        Assert.Equal(Ids.Hex(Ids.Sha256(zip)), first.ContentHashHex);
        Assert.Equal(1, first.Version);
        Assert.Equal(PrivacyExportParser.ParserVersion, world.App.Imports.Imports[0].ParserVersion);
        Assert.Equal(first.ImportId, again.ImportId);
        Assert.Equal("duplicate-hash", again.Status);
        Assert.Equal(1, again.Version);
        Assert.Single(world.App.Imports.Imports);

        var other = world.App.Ingest.IngestPrivacyExport(Consent(driver.DriverId), ExportZip(false, false), "driver-upload");
        Assert.Equal(2, other.Version);
        Assert.NotEqual(first.ContentHashHex, other.ContentHashHex);
    }


    internal static IngestCommand Consent(string driverId) =>
        new(driverId, "US-CA", "court-audit", "I consent to sealed custody of this import.", true, ProvenanceTags.PrivacyExport);

    internal static byte[] ExportZip(bool includeScores, bool includeUnknown)
    {
        var files = new List<(string Name, string Body)>
        {
            ("DataDictionary.csv", "file,signal,availability\ntrips.csv,trip_records,collected\nscores.csv,smooth_cruiser,collected\nonline_hours.csv,online_hours,not_collected\nimu.bin,imu,unknown\n"),
            ("trips.csv", "trip_id,started_unix_millis,ended_unix_millis,status\ntrip-1,1700000000000,1700003600000,droppedOff\n")
        };
        if (includeScores)
            files.Add(("scores.csv", "observed_unix_millis,overall,gentle_braking,smooth_steering,phone_mount,speed_vs_area,source\n1700000000000,80,81,82,83,84,lyft_privacy_export\n"));
        if (includeUnknown)
            files.Add(("note.txt", Marker + "\n"));
        return Zip(files.ToArray());
    }

    internal static byte[] Zip(params (string Name, string Body)[] files)
    {
        using var stream = new MemoryStream();
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            foreach (var (name, body) in files)
            {
                var entry = zip.CreateEntry(name);
                using var writer = new StreamWriter(entry.Open());
                writer.Write(body);
            }
        }
        return stream.ToArray();
    }
}


public class TestRide007Through009And016And023
{
    [Fact]
    [Trait("TEST", "TEST-RIDE-007")]
    [Trait("FR", "FR-RIDE-007")]
    [Trait("FR", "FR-RIDE-209")]
    [Trait("AC", "AC-RIDE-007-001")]
    [Trait("AC", "AC-RIDE-007-002")]
    [Trait("AC", "AC-RIDE-209-001")]
    [Trait("AC", "AC-RIDE-ANAL-002-001")]
    [Trait("AC", "AC-RIDE-ANAL-002-002")]
    [Trait("AC", "AC-UC-005-001")]
    [Trait("AC", "AC-UC-005-002")]
    [Trait("AC", "AC-TEST-007-001")]
    [Trait("AC", "AC-TEST-007-002")]
    public void Coverage_matrix_separates_collected_available_and_missing_signals()
    {
        var world = ServerWorld.Create();
        var driver = world.Register();
        var imported = world.App.Ingest.IngestPrivacyExport(TestRide001Through006And011And030.Consent(driver.DriverId), TestRide001Through006And011And030.ExportZip(false, true), "driver-upload");
        var matrix = world.App.Analysis.Coverage(driver.DriverId, driver.DriverId, imported.ImportId);
        var trip = Assert.Single(matrix.Cells, cell => cell.Signal == "trip_records");
        Assert.True(trip.CollectedBySource);
        Assert.True(trip.AvailableToAuditor);
        Assert.False(trip.Missing);
        var score = Assert.Single(matrix.Cells, cell => cell.Signal == "smooth_cruiser");
        Assert.True(score.CollectedBySource);
        Assert.False(score.AvailableToAuditor);
        Assert.True(score.Missing);
        Assert.Contains("not a Smooth Cruiser", score.GapNotice, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("2026-09-27", score.GapNotice, StringComparison.Ordinal);
        var imu = Assert.Single(matrix.Cells, cell => cell.Signal == "imu");
        Assert.True(imu.Missing);
        Assert.Equal(AnalSlice.State, "implemented");
    }

    [Fact]
    [Trait("TEST", "TEST-RIDE-008")]
    [Trait("FR", "FR-RIDE-008")]
    [Trait("AC", "AC-RIDE-008-001")]
    [Trait("AC", "AC-RIDE-008-002")]
    [Trait("AC", "AC-RIDE-008-003")]
    [Trait("AC", "AC-RIDE-ANAL-003-001")]
    [Trait("AC", "AC-RIDE-ANAL-003-002")]
    [Trait("AC", "AC-UC-006-001")]
    [Trait("AC", "AC-UC-006-002")]
    [Trait("AC", "AC-TEST-008-001")]
    [Trait("AC", "AC-TEST-008-002")]
    public void Online_hours_flags_only_when_hours_data_exists()
    {
        var world = ServerWorld.Create();
        var driver = world.Register();
        world.App.Imports.AddTrip(new TripRow("imp", driver.DriverId, "US-CA", "long", 0, 20 * 3_600_000, "droppedOff", ProvenanceTags.PrivacyExport));
        var absent = world.App.Analysis.OnlineHours(driver.DriverId, driver.DriverId, "US-CA", 0, 0);
        Assert.False(absent.HoursDataPresent);
        Assert.Empty(absent.Violations);

        world.App.Imports.Hours.Add(new OnlineHoursRow("imp", driver.DriverId, "US-CA", 0, 13 * 3_600_000));
        var flagged = world.App.Analysis.OnlineHours(driver.DriverId, driver.DriverId, "US-CA", 0, 0);
        Assert.True(flagged.HoursDataPresent);
        Assert.Contains(flagged.Violations, line => line.Contains("exceeds 12h", StringComparison.Ordinal));

        world.App.Imports.Hours.Clear();
        world.App.Imports.Hours.Add(new OnlineHoursRow("imp", driver.DriverId, "US-NY", 0, 9 * 3_600_000));
        var regional = world.App.Analysis.OnlineHours(driver.DriverId, driver.DriverId, "US-NY", 8, 6);
        Assert.Contains(regional.Violations, line => line.Contains("exceeds 8h", StringComparison.Ordinal));
        Assert.Contains("regional override", regional.Policy, StringComparison.Ordinal);
        var analysis = File.ReadAllText(Path.Combine(ServerWorld.RepoRoot(), "src/RideAudit.Anal/AnalysisService.cs"));
        Assert.DoesNotContain("FMCSA", analysis, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("TEST", "TEST-RIDE-009")]
    [Trait("FR", "FR-RIDE-009")]
    [Trait("AC", "AC-RIDE-009-001")]
    [Trait("AC", "AC-RIDE-009-002")]
    [Trait("AC", "AC-RIDE-ANAL-004-001")]
    [Trait("AC", "AC-RIDE-ANAL-004-002")]
    [Trait("AC", "AC-UC-007-001")]
    [Trait("AC", "AC-UC-007-002")]
    [Trait("AC", "AC-TEST-009-001")]
    [Trait("AC", "AC-TEST-009-002")]
    public void Incident_window_returns_only_rows_inside_the_range()
    {
        var world = ServerWorld.Create();
        var driver = world.Register();
        world.App.Imports.AddTrip(new TripRow("imp", driver.DriverId, "US-CA", "in", 1_000, 2_000, "droppedOff", ProvenanceTags.PrivacyExport));
        world.App.Imports.AddTrip(new TripRow("imp", driver.DriverId, "US-CA", "out", 9_000, 9_500, "droppedOff", ProvenanceTags.PrivacyExport));
        world.App.Imports.Scores.Add(new ScoreRow("imp", driver.DriverId, 1_500, 80, 80, 80, 80, 80, ProvenanceTags.Manual));
        var report = world.App.Analysis.Incident(driver.DriverId, driver.DriverId, 1_000, 3_000);
        Assert.Equal(1, report.TripCount);
        Assert.Equal(1, report.ScoreCount);
        Assert.Contains(report.Lines, line => line.Contains("trip in", StringComparison.Ordinal));
        Assert.DoesNotContain(report.Lines, line => line.Contains("trip out", StringComparison.Ordinal));

        var empty = world.App.Analysis.Incident(driver.DriverId, driver.DriverId, 50_000, 60_000);
        Assert.Equal(0, empty.TripCount);
        Assert.Contains("Missing signals stay missing", empty.GapNotice, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("TEST", "TEST-RIDE-016")]
    [Trait("FR", "FR-RIDE-020")]
    [Trait("FR", "FR-RIDE-021")]
    [Trait("AC", "AC-RIDE-021-001")]
    [Trait("AC", "AC-RIDE-021-002")]
    [Trait("AC", "AC-RIDE-017-002")]
    [Trait("AC", "AC-RIDE-CHAIN-002-002")]
    [Trait("AC", "AC-UC-010-001")]
    [Trait("AC", "AC-UC-010-002")]
    [Trait("AC", "AC-TEST-016-001")]
    public void Verification_report_checks_the_hash_and_does_not_decrypt()
    {
        var world = ServerWorld.Create();
        var enrolled = world.Enroll();
        var ready = world.SealReady(enrolled.Driver, enrolled.Session);
        var outcome = world.Submit(enrolled.Driver, ready.Package, ready.Token, ready.Nonce);
        var opens = world.App.Hsm.WorkingCopyOpens;
        var report = world.App.Counsel.Verify(enrolled.Driver.DriverId, "case-1", outcome.SubmissionId);
        Assert.True(report.HashMatches);
        Assert.True(report.AnchorConfirmed);
        Assert.False(report.DecryptionPerformed);
        Assert.Contains("does not prove", report.DoesNotProve, StringComparison.OrdinalIgnoreCase);
        Assert.StartsWith("fixture:", report.TransactionReference, StringComparison.Ordinal);
        Assert.Equal(opens, world.App.Hsm.WorkingCopyOpens);
        Assert.Contains("court-review", File.ReadAllText(Path.Combine(ServerWorld.RepoRoot(), "docs/architecture/court-review-decryption-path.md")), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("TEST", "TEST-RIDE-023")]
    [Trait("FR", "FR-RIDE-037")]
    [Trait("FR", "FR-RIDE-038")]
    [Trait("AC", "AC-RIDE-037-001")]
    [Trait("AC", "AC-RIDE-037-002")]
    [Trait("AC", "AC-RIDE-038-001")]
    [Trait("AC", "AC-RIDE-038-002")]
    [Trait("AC", "AC-RIDE-SERVER-006-002")]
    [Trait("AC", "AC-RIDE-SERVER-007-001")]
    [Trait("AC", "AC-RIDE-SERVER-007-002")]
    [Trait("AC", "AC-UC-016-001")]
    [Trait("AC", "AC-UC-016-002")]
    [Trait("AC", "AC-TEST-023-001")]
    [Trait("AC", "AC-TEST-023-002")]
    public void Own_submissions_bundle_keeps_each_record_independent()
    {
        var world = ServerWorld.Create();
        var first = world.Enroll();
        var left = world.SealReady(first.Driver, first.Session);
        var right = world.SealReady(first.Driver, first.Session);
        var a = world.Submit(first.Driver, left.Package, left.Token, left.Nonce);
        var b = world.Submit(first.Driver, right.Package, right.Token, right.Nonce);
        var beforeA = world.App.Journal.Find(a.SubmissionId)!.Ciphertext.ToArray();
        var beforeB = world.App.Journal.Find(b.SubmissionId)!.Ciphertext.ToArray();

        var bundle = world.App.Counsel.Build(first.Driver.DriverId, "case-9", new[] { a.SubmissionId, b.SubmissionId });
        Assert.False(bundle.AggregationReplacesRecords);
        Assert.Equal(2, bundle.Records.Count);
        Assert.All(bundle.Records, row =>
        {
            Assert.True(row.IndependentCustody);
            Assert.True(row.HashMatches);
            Assert.False(string.IsNullOrWhiteSpace(row.AnchorStatus));
            Assert.Equal(first.Driver.DriverId, row.DriverId);
        });
        Assert.NotEqual(bundle.Records[0].ContentHashHex, bundle.Records[1].ContentHashHex);
        Assert.Equal(beforeA, world.App.Journal.Find(a.SubmissionId)!.Ciphertext);
        Assert.Equal(beforeB, world.App.Journal.Find(b.SubmissionId)!.Ciphertext);
        Assert.Equal(0, world.App.Hsm.WorkingCopyOpens);
        Assert.Equal("implemented", CounselSlice.State);
    }

    [Fact]
    [Trait("TEST", "TEST-RIDE-023")]
    [Trait("NOTE", "own-submissions-only-no-roles")]
    public void Build_rejects_non_own_submissions()
    {
        var world = ServerWorld.Create();
        var first = world.Enroll();
        var second = world.Enroll();
        var left = world.SealReady(first.Driver, first.Session);
        var right = world.SealReady(second.Driver, second.Session);
        var a = world.Submit(first.Driver, left.Package, left.Token, left.Nonce);
        var b = world.Submit(second.Driver, right.Package, right.Token, right.Nonce);
        var ex = Assert.Throws<RideAuditException>(() =>
            world.App.Counsel.Build(first.Driver.DriverId, "case-x", new[] { a.SubmissionId, b.SubmissionId }));
        Assert.Equal(ErrorCodes.TenantIsolation, ex.Code);
    }

    [Fact]
    [Trait("NOTE", "own-submissions-only-no-roles")]
    public void Analysis_rejects_cross_driver_subject()
    {
        var world = ServerWorld.Create();
        var driver = world.Register();
        var other = world.Register();
        var cov = Assert.Throws<RideAuditException>(() => world.App.Analysis.Coverage(driver.DriverId, other.DriverId, null));
        Assert.Equal(ErrorCodes.TenantIsolation, cov.Code);
        var hours = Assert.Throws<RideAuditException>(() => world.App.Analysis.OnlineHours(driver.DriverId, other.DriverId, "US-CA", 0, 0));
        Assert.Equal(ErrorCodes.TenantIsolation, hours.Code);
        var incident = Assert.Throws<RideAuditException>(() => world.App.Analysis.Incident(driver.DriverId, other.DriverId, 1_000, 2_000));
        Assert.Equal(ErrorCodes.TenantIsolation, incident.Code);
    }
}

public class TestRide010012029031032
{
    [Fact]
    [Trait("TEST", "TEST-RIDE-010")]
    [Trait("FR", "FR-RIDE-010")]
    [Trait("AC", "AC-RIDE-010-002")]
    [Trait("AC", "AC-UC-008-001")]
    [Trait("AC", "AC-UC-008-002")]
    [Trait("AC", "AC-TEST-010-001")]
    [Trait("AC", "AC-TEST-010-002")]
    public void Dsar_deletes_personal_imports_and_retains_custody_ciphertext()
    {
        var world = ServerWorld.Create();
        var enrolled = world.Enroll();
        var ready = world.SealReady(enrolled.Driver, enrolled.Session);
        var outcome = world.Submit(enrolled.Driver, ready.Package, ready.Token, ready.Nonce);
        var cipher = world.App.Journal.Find(outcome.SubmissionId)!.Ciphertext.ToArray();
        world.App.Ingest.IngestPrivacyExport(TestRide001Through006And011And030.Consent(enrolled.Driver.DriverId), TestRide001Through006And011And030.ExportZip(true, true), "driver-upload");
        var enrolledDeleted = world.App.Privacy.Delete(enrolled.Driver.DriverId, enrolled.Driver.DriverId, "");
        Assert.True(enrolledDeleted.Deleted);
        Assert.True(enrolledDeleted.CustodyCiphertextRetained);
        Assert.DoesNotContain(world.App.Imports.Scores, row => row.DriverId == enrolled.Driver.DriverId);

        var other = world.Register();
        world.App.Ingest.IngestPrivacyExport(TestRide001Through006And011And030.Consent(other.DriverId), TestRide001Through006And011And030.ExportZip(true, false), "driver-upload");
        var deleted = world.App.Privacy.Delete(other.DriverId, other.DriverId, "");
        Assert.True(deleted.Deleted);
        Assert.True(deleted.CustodyCiphertextRetained);
        Assert.DoesNotContain(world.App.Imports.Scores, row => row.DriverId == other.DriverId);
        Assert.Contains(world.App.Imports.Consents, row => row.DriverId == other.DriverId && row.Statement.Length == 0);
        Assert.Equal(cipher, world.App.Journal.Find(outcome.SubmissionId)!.Ciphertext);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-077")]
    [Trait("NOTE", "killed-TEST-012-FR-014-202-replaced-by-FR-077")]
    public void Precise_location_stays_visible_and_blocks_cross_driver_reads()
    {
        var world = ServerWorld.Create();
        var driver = world.Register();
        var other = world.Register();
        world.App.Imports.Locations.Add(new LocationRow("loc-1", "imp", driver.DriverId, "US-CA", 10, 37.7749, -122.4194, ProvenanceTags.ThirdParty, "third-party-sample"));
        var ownView = Assert.Single(world.App.Privacy.ViewLocations(driver.DriverId, driver.DriverId));
        Assert.True(ownView.Precise);
        Assert.Equal(37.7749.ToString("G17", CultureInfo.InvariantCulture), ownView.Latitude);
        Assert.Equal((-122.4194).ToString("G17", CultureInfo.InvariantCulture), ownView.Longitude);
        Assert.Throws<RideAuditException>(() => world.App.Privacy.ViewLocations(driver.DriverId, other.DriverId));
        Assert.Equal(0, world.App.Privacy.SweepRetention());
        Assert.Contains(world.App.Imports.Locations, row => row.SampleId == "loc-1");
    }

    [Fact]
    [Trait("TEST", "TEST-RIDE-029")]
    [Trait("NOTE", "access-log-helper-retained-not-RBAC-mask; prior FR-203/AC-203 removed from live traits")]
    [Trait("AC", "AC-TEST-029-001")]
    [Trait("AC", "AC-TEST-029-002")]
    public void Access_log_is_append_only()
    {
        var world = ServerWorld.Create();
        var driver = world.Register();
        world.App.Imports.Locations.Add(new LocationRow("loc-1", "imp", driver.DriverId, "US-CA", 10, 1, 2, ProvenanceTags.ThirdParty, "third-party-sample"));
        var before = world.App.AccessLog.Entries.Count;
        world.App.Privacy.ViewLocations(driver.DriverId, driver.DriverId);
        var snapshot = world.App.AccessLog.Entries;
        world.App.Privacy.ViewLocations(driver.DriverId, driver.DriverId);
        Assert.Equal(before + 1, snapshot.Count);
        Assert.Equal(before + 2, world.App.AccessLog.Entries.Count);
        var names = typeof(AppendOnlyAccessLog).GetMethods().Select(method => method.Name).ToArray();
        Assert.DoesNotContain("Clear", names);
        Assert.DoesNotContain("Remove", names);
    }

    [Fact]
    [Trait("TEST", "TEST-RIDE-031")]
    [Trait("FR", "FR-RIDE-205")]
    [Trait("FR", "FR-RIDE-207")]
    [Trait("AC", "AC-RIDE-207-001")]
    [Trait("AC", "AC-RIDE-010-001")]
    [Trait("AC", "AC-RIDE-PRIV-003-001")]
    [Trait("AC", "AC-RIDE-PERF-002-001")]
    [Trait("AC", "AC-UC-021-001")]
    [Trait("AC", "AC-TEST-031-001")]
    [Trait("AC", "AC-TEST-031-002")]
    public void Multi_year_window_uses_the_index_and_the_audit_zip_omits_plaintext()
    {
        var world = ServerWorld.Create();
        var driver = world.Register();
        var origin = DateTimeOffset.Parse("2020-01-01T00:00:00Z").ToUnixTimeMilliseconds();
        for (var i = 0; i < 4000; i++)
            world.App.Imports.AddTrip(new TripRow("imp", driver.DriverId, "US-CA", "t-" + i, origin + i * 86_400_000L, origin + i * 86_400_000L + 1000, "droppedOff", ProvenanceTags.PrivacyExport));
        var windowStart = origin + 100 * 86_400_000L;
        var rows = world.App.Imports.TripIndex.Window(windowStart, windowStart + 86_400_000L);
        Assert.Single(rows);
        Assert.True(world.App.Imports.TripIndex.LastComparisons < world.App.Imports.TripIndex.Count);

        var imported = world.App.Ingest.IngestPrivacyExport(TestRide001Through006And011And030.Consent(driver.DriverId), TestRide001Through006And011And030.ExportZip(true, true), "driver-upload");
        var export = world.App.Privacy.Export(driver.DriverId, driver.DriverId);
        Assert.Equal("exported", export.Status);
        using var zip = new ZipArchive(new MemoryStream(export.ZipBytes), ZipArchiveMode.Read);
        var manifest = new StreamReader(zip.GetEntry("manifest.json")!.Open()).ReadToEnd();
        var notice = new StreamReader(zip.GetEntry("NOTICE.txt")!.Open()).ReadToEnd();
        Assert.Contains(imported.ContentHashHex, manifest, StringComparison.Ordinal);
        Assert.Contains("sealedPlaintextIncluded\":false", manifest.Replace(" ", ""), StringComparison.Ordinal);
        Assert.DoesNotContain("RAWZIP-MARKER-9f3a", manifest, StringComparison.Ordinal);
        Assert.Contains("GPL-2.0-only", notice, StringComparison.Ordinal);
        Assert.Contains("does not contain sealed plaintext", notice, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(zip.GetEntry("provenance.csv"));
        Assert.NotNull(zip.GetEntry("provenance.json"));
        var pdf = zip.GetEntry("summary.pdf") ?? throw new InvalidOperationException("summary.pdf missing");
        using var pdfStream = pdf.Open();
        using var pdfReader = new StreamReader(pdfStream);
        var pdfText = pdfReader.ReadToEnd();
        Assert.StartsWith("%PDF-", pdfText, StringComparison.Ordinal);
        var csv = new StreamReader(zip.GetEntry("provenance.csv")!.Open()).ReadToEnd();
        Assert.Contains("import_id,content_hash", csv, StringComparison.Ordinal);
        Assert.Contains(imported.ContentHashHex, csv, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-078")]
    [Trait("NOTE", "killed-TEST-032-FR-208-replaced-by-FR-078")]
    public void Retention_does_not_remove_location_rows()
    {
        var world = ServerWorld.Create();
        var now = world.Clock.UtcNow;
        var old = now.AddDays(-31).ToUnixTimeMilliseconds();
        var recent = now.AddDays(-10).ToUnixTimeMilliseconds();
        var driver = world.Register();
        world.App.Imports.Locations.Add(new LocationRow("ca-old", "imp", driver.DriverId, "US-CA", old, 1, 2, ProvenanceTags.ThirdParty, "third-party-sample"));
        world.App.Imports.Locations.Add(new LocationRow("ca-new", "imp", driver.DriverId, "US-CA", recent, 1, 2, ProvenanceTags.ThirdParty, "third-party-sample"));
        world.App.Imports.Locations.Add(new LocationRow("ny-old", "imp", driver.DriverId, "US-NY", old, 1, 2, ProvenanceTags.ThirdParty, "third-party-sample"));

        var removed = world.App.Privacy.SweepRetention();
        Assert.Equal(0, removed);
        Assert.Contains(world.App.Imports.Locations, row => row.SampleId == "ca-old");
        Assert.Contains(world.App.Imports.Locations, row => row.SampleId == "ca-new");
        Assert.Contains(world.App.Imports.Locations, row => row.SampleId == "ny-old");

        var viewed = world.App.Privacy.ViewLocations(driver.DriverId, driver.DriverId);
        Assert.Equal(3, viewed.Count);
        Assert.All(viewed, sample =>
        {
            Assert.True(sample.Precise);
            Assert.Equal(1d.ToString("G17", CultureInfo.InvariantCulture), sample.Latitude);
            Assert.Equal(2d.ToString("G17", CultureInfo.InvariantCulture), sample.Longitude);
        });
    }
}

public class TestRide020And036ServerPortions
{
    [Fact]
    [Trait("TEST", "TEST-RIDE-020")]
    [Trait("FR", "FR-RIDE-217")]
    public void Distribution_receipts_do_not_claim_a_deployment_or_store_publication()
    {
        var receipts = DistributionReceipts.ServerPortions;
        Assert.Equal(new[] { "Dev", "Staging", "Prod" }, receipts.Select(row => row.Environment).ToArray());
        Assert.All(receipts, row => Assert.Equal(DistributionReceipts.NotRun, row.Status));
        Assert.All(receipts, row => Assert.Equal(DistributionReceipts.PlayNotClaimed, row.PlayPublication));
        Assert.All(receipts, row => Assert.Equal("GPL-2.0-only", row.Spdx));
        var doc = File.ReadAllText(Path.Combine(ServerWorld.RepoRoot(), "docs/receipts/distribution/cd-receipts.md"));
        Assert.Contains("not-run", doc, StringComparison.Ordinal);
        Assert.Contains("not-claimed", doc, StringComparison.Ordinal);
        Assert.Contains("GPL-2.0-only", doc, StringComparison.Ordinal);
        Assert.DoesNotContain("published to Google Play", doc, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("TEST", "TEST-RIDE-038")]
    [Trait("FR", "FR-RIDE-063")]
    [Trait("AC", "AC-RIDE-063-003")]
    [Trait("AC", "AC-TEST-038-001")]
    [Trait("AC", "AC-TEST-038-002")]
    public void Octopus_desktop_pointer_is_a_receipt_not_a_live_probe_or_ghcr_row()
    {
        var onFile = DistributionReceipts.OctopusDesktopOnFile;
        Assert.Equal("Octopus-PAYTON-DESKTOP", onFile.Environment);
        Assert.Equal("receipt-on-file", onFile.Status);
        Assert.Equal(DistributionReceipts.PlayNotClaimed, onFile.PlayPublication);
        Assert.Contains("20260929T015822Z-octopus-payton-desktop.md", onFile.Detail, StringComparison.Ordinal);
        Assert.Contains("not a live probe", onFile.Detail, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ghcr.io", onFile.Detail, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(onFile.Environment, DistributionReceipts.ServerPortions.Select(row => row.Environment));
    }

    [Fact]
    [Trait("TEST", "TEST-RIDE-036")]
    [Trait("FR", "FR-RIDE-059")]
    [Trait("AC", "AC-UC-028-001")]
    [Trait("AC", "AC-TEST-036-001")]
    [Trait("AC", "AC-TEST-036-002")]
    public void Counsel_container_sketch_targets_dotnet_10()
    {
        var docker = File.ReadAllText(Path.Combine(ServerWorld.RepoRoot(), "deploy/containers/counsel/Dockerfile"));
        Assert.Contains("mcr.microsoft.com/dotnet/aspnet:10.0", docker, StringComparison.Ordinal);
        Assert.Contains("mcr.microsoft.com/dotnet/sdk:10.0", docker, StringComparison.Ordinal);
        Assert.Contains("GPL-2.0-only", docker, StringComparison.Ordinal);
        Assert.DoesNotContain("lyft.com", docker, StringComparison.OrdinalIgnoreCase);
        var admission = File.ReadAllText(Path.Combine(ServerWorld.RepoRoot(), "src/RideAudit.Server.Admission/AdmissionCoordinator.cs"));
        Assert.Contains("Public ingest accepts only sealed ciphertext.", admission, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("TEST", "TEST-RIDE-014")]
    [Trait("FR", "FR-RIDE-212")]
    public void L2_fixture_catalog_matches_the_documented_json()
    {
        var json = File.ReadAllText(Path.Combine(ServerWorld.RepoRoot(), "src/RideAudit.Chain.EthL2/Fixtures/eth-l2-fixture.json"));
        var described = DocumentedFixtureL2Calendar.Describe(ChainProfileIds.EthL2Base);
        Assert.Contains(described.TransactionReference, json, StringComparison.Ordinal);
        Assert.Contains("\"live_chain_metadata\": false", json, StringComparison.Ordinal);
        Assert.Contains(DualFixture.TransactionReference, json, StringComparison.Ordinal);
        Assert.DoesNotMatch("[0-9a-fA-F]{64}", described.TransactionReference);
    }
}
