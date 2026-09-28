// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Text;
using System.Text.RegularExpressions;
using RideAudit.Client.Tests.Support;
using Xunit;

namespace RideAudit.Client.Tests;

public class AcCoverageLedgerTests
{
    private static readonly Regex AcId = new(@"id:\s*(AC-[A-Z0-9-]+)", RegexOptions.Compiled);

    [Fact]
    [Trait("FR", "FR-RIDE-060")]
    public void Writes_honest_ledger_for_all_requirement_ac_ids()
    {
        var root = Repo.Root();
        var yamlFiles = Directory.GetFiles(Path.Combine(root, "docs", "Project"), "*Batch.yaml");
        Assert.Equal(7, yamlFiles.Length);

        var records = new List<AcRecord>();
        foreach (var file in yamlFiles.OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            var text = File.ReadAllText(file);
            foreach (Match match in AcId.Matches(text))
            {
                var id = match.Groups[1].Value;
                var window = text.Substring(match.Index, Math.Min(400, text.Length - match.Index));
                records.Add(new AcRecord(id, Path.GetFileName(file), window));
            }
        }

        var unique = records
            .GroupBy(record => record.Id, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(record => record.Id, StringComparer.Ordinal)
            .ToList();
        Assert.Equal(404, unique.Count);

        var testText = string.Join('\n', Directory.GetFiles(Path.Combine(root, "tests"), "*.cs", SearchOption.AllDirectories)
            .Select(File.ReadAllText));
        var named = unique.Where(record => testText.Contains(record.Id, StringComparison.Ordinal)).Select(record => record.Id).ToHashSet(StringComparer.Ordinal);

        var rows = unique.Select(record =>
        {
            if (named.Contains(record.Id))
                return (record.Id, record.Source, "covered", "Named in executable test source.");
            var deferred = ClassifyDeferred(record);
            return deferred is null
                ? (record.Id, record.Source, "missing", "No test source reference and no deferred live-integration reason.")
                : (record.Id, record.Source, "deferred", deferred);
        }).ToList();

        var covered = rows.Count(row => row.Item3 == "covered");
        var deferred = rows.Count(row => row.Item3 == "deferred");
        var missing = rows.Count(row => row.Item3 == "missing");
        Assert.True(covered >= 111, "Coverage ledger regressed below the rem-r1 named-AC count.");

        var output = new StringBuilder();
        output.AppendLine("# RideAudit AC coverage ledger");
        output.AppendLine();
        output.AppendLine("Generated: 2026-09-28. Workspace: PAYTON-LEGION2. Not a claim that all ACs are satisfied.");
        output.AppendLine();
        output.AppendLine("Statuses: `covered` = AC ID appears in `tests/**/*.cs`; `deferred` = live hardware/Play/HSM/CD/partnership proof is honestly out of scope for this tree; `missing` = no test reference yet.");
        output.AppendLine();
        output.AppendLine("| Status | Count |");
        output.AppendLine("| --- | ---: |");
        output.AppendLine("| covered | " + covered + " |");
        output.AppendLine("| deferred | " + deferred + " |");
        output.AppendLine("| missing | " + missing + " |");
        output.AppendLine("| total | " + rows.Count + " |");
        output.AppendLine();
        output.AppendLine("| AC ID | Source YAML | Status | Reason |");
        output.AppendLine("| --- | --- | --- | --- |");
        foreach (var row in rows)
            output.AppendLine("| " + row.Item1 + " | " + row.Item2 + " | " + row.Item3 + " | " + row.Item4.Replace("|", "/") + " |");

        var destDir = Path.Combine(root, "docs", "receipts", "ac-coverage");
        Directory.CreateDirectory(destDir);
        var dest = Path.Combine(destDir, "20260928-ledger.md");
        File.WriteAllText(dest, output.ToString());
        Assert.True(File.Exists(dest));
        Assert.Equal(404, rows.Count);
        Assert.Equal(0, rows.Count(row => string.IsNullOrWhiteSpace(row.Item4)));
    }

    private static string? ClassifyDeferred(AcRecord record)
    {
        var blob = record.Id + " " + record.Window;
        if (ContainsAny(blob, "Play Store", "Play publication", "published on Google Play"))
            return "Deferred: Play Store publication is not claimed in this tree.";
        if (ContainsAny(blob, "hardware HSM", "live HSM", "HSM hardware"))
            return "Deferred: hardware HSM is not integrated; in-process Shamir remains labeled.";
        if (ContainsAny(blob, "GHCR", "CD green", "signed release", "Development/Staging/Production"))
            return "Deferred: lab loopback is not production CD or GHCR.";
        if (ContainsAny(blob, "physical dual-phone", "two physical", "Bluetooth media", "H.264"))
            return "Deferred: physical dual-phone media transport / production H.264 is not implemented.";
        if (ContainsAny(blob, "live Bitcoin", "txid", "OpenTimestamps confirmation", "confirmed Bitcoin"))
            return "Deferred: public OTS remains pending-labeled; no invented txid.";
        if (ContainsAny(blob, "L2 signer", "live L2", "unsigned commit"))
            return "Deferred: public L2 refuses commit without a signer.";
        if (ContainsAny(blob, "Caddy", "edge TLS", "TLS 1.2"))
            return "Deferred: Omarchy loopback is not an edge TLS receipt.";
        if (ContainsAny(blob, "Concierge", "Lyft private", "partnership"))
            return "Deferred: Lyft partnership / Concierge live path is not enabled.";
        if (ContainsAny(blob, "vehicle-fit", "on-vehicle print", "road release"))
            return "Deferred: hardware mount road-fit is operator checklist work.";
        if (record.Id.StartsWith("AC-RIDE-INGEST-004", StringComparison.Ordinal))
            return "Deferred: Concierge OAuth live partnership is not enabled.";
        if (record.Id is "AC-RIDE-CHAIN-001-002")
            return "Deferred B07: public OTS confirmation / live txid is not claimed; fixture anchors stay labeled.";
        if (record.Id.StartsWith("AC-RIDE-STORE-003", StringComparison.Ordinal))
            return "Deferred B08: jurisdiction retention timers and legal-hold deletion are not a lab loopback receipt.";
        return null;
    }

    private static bool ContainsAny(string blob, params string[] needles) =>
        needles.Any(needle => blob.Contains(needle, StringComparison.OrdinalIgnoreCase));

    private sealed record AcRecord(string Id, string Source, string Window);
}
