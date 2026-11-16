// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.IO.Compression;
using RideAudit.Ingest;
using RideAudit.TestSupport;

namespace RideAudit.Workflow.Tests;

/// <summary>UC-RIDE-021 (FR-RIDE-207 portable audit ZIP): import caveats stay visible in the export.</summary>
public class PortableAuditCaveatTests
{
    [Fact]
    [Trait("FR", "FR-RIDE-207")]
    [Trait("UC", "UC-RIDE-021")]
    [Trait("AC", "AC-UC-021-002")]
    public void Portable_audit_zip_keeps_gap_and_unverified_import_status_visible()
    {
        var world = ServerWorld.Create();
        var driver = world.Register();
        var consent = TestRide001Through006And011And030.Consent(driver.DriverId);

        // An export with an unknown file imports with a gap notice instead of a clean status.
        var gapped = world.App.Ingest.IngestPrivacyExport(
            consent, TestRide001Through006And011And030.ExportZip(includeScores: false, includeUnknown: true), "driver-upload");
        Assert.Equal("imported-with-gap", gapped.Status);
        Assert.Contains(ApiGapNotice.Text, gapped.GapNotice, StringComparison.Ordinal);

        // An export without its data dictionary stays an unverified package.
        var bare = world.App.Ingest.IngestPrivacyExport(
            consent,
            TestRide001Through006And011And030.Zip(("trips.csv", "trip_id,started_unix_millis,ended_unix_millis,status\ntrip-021,1700000000000,1700003600000,droppedOff\n")),
            "driver-upload");
        Assert.Equal("unverified-package", bare.Status);

        var export = world.App.Privacy.Export(driver.DriverId, driver.DriverId);
        using var zip = new ZipArchive(new MemoryStream(export.ZipBytes), ZipArchiveMode.Read);
        var csvLines = new StreamReader(zip.GetEntry("provenance.csv")!.Open()).ReadToEnd()
            .Split('\n')
            .Select(line => line.TrimEnd('\r'))
            .ToList();
        Assert.Contains(csvLines, line => line.StartsWith(gapped.ImportId + ",", StringComparison.Ordinal) && line.EndsWith(",imported-with-gap", StringComparison.Ordinal));
        Assert.Contains(csvLines, line => line.StartsWith(bare.ImportId + ",", StringComparison.Ordinal) && line.EndsWith(",unverified-package", StringComparison.Ordinal));

        var manifest = new StreamReader(zip.GetEntry("manifest.json")!.Open()).ReadToEnd();
        Assert.Contains("imported-with-gap", manifest, StringComparison.Ordinal);
        Assert.Contains("unverified-package", manifest, StringComparison.Ordinal);
    }
}
