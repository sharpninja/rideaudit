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
        Assert.Equal(9, yamlFiles.Length);

        var records = new List<AcRecord>();
        foreach (var file in yamlFiles.OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            var text = File.ReadAllText(file);
            foreach (Match match in AcId.Matches(text))
            {
                var id = match.Groups[1].Value;
                var acText = ExtractAcText(text, match.Index);
                records.Add(new AcRecord(id, Path.GetFileName(file), acText));
            }
        }

        var unique = records
            .GroupBy(record => record.Id, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(record => record.Id, StringComparer.Ordinal)
            .ToList();
        Assert.Equal(549, unique.Count);

        var testText = string.Join('\n', Directory.GetFiles(Path.Combine(root, "tests"), "*.cs", SearchOption.AllDirectories)
            .Select(File.ReadAllText));
        var named = unique.Where(record => testText.Contains(record.Id, StringComparison.Ordinal)).Select(record => record.Id).ToHashSet(StringComparer.Ordinal);

        var rows = unique.Select(record =>
        {
            var deferred = ClassifyDeferred(record);
            if (deferred is not null)
                return (record.Id, record.Source, "deferred", deferred);
            return named.Contains(record.Id)
                ? (record.Id, record.Source, "covered", "Named in executable test source. Not semantic closure.")
                : (record.Id, record.Source, "missing", "No test source reference and no deferred live-integration reason.");
        }).ToList();

        var covered = rows.Count(row => row.Item3 == "covered");
        var deferred = rows.Count(row => row.Item3 == "deferred");
        var missing = rows.Count(row => row.Item3 == "missing");
        Assert.True(covered >= 170, "Coverage ledger regressed below the rem-r2 named-AC count.");
        var missingIds = rows.Where(row => row.Item3 == "missing").Select(row => row.Item1).ToArray();
        Assert.True(missing == 0, "Uncovered AC ids remain: " + string.Join(", ", missingIds));
        Assert.Equal(rows.Count, covered + deferred);

        var output = new StringBuilder();
        output.AppendLine("# RideAudit AC coverage ledger");
        output.AppendLine();
        output.AppendLine("Generated: 2026-10-07 (recount). Workspace: PAYTON-LEGION2. Not a claim that all ACs are satisfied.");
        output.AppendLine();
        output.AppendLine("Statuses: `deferred` wins when the AC's own text, an id prefix, or `explicit-deferrals.txt` marks live hardware/Play/HSM/partnership/Caddy-TLS work. A neighboring requirement in the YAML file does not defer this AC. `covered` = the id appears in `tests/**/*.cs` and is not deferred. `missing` = neither. A covered row is a test-source name, not semantic closure. Octopus CD evidence is the receipt `20260929T015822Z-octopus-payton-desktop.md` (not GHCR). Canonical ngrok target is LAB-OMARCHY admission 192.168.1.182:28080; Omarchy 127.0.0.1:18080 is the prior interim.");
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
        Assert.Equal(549, rows.Count);
        Assert.Equal(0, rows.Count(row => string.IsNullOrWhiteSpace(row.Item4)));
    }

    private static Dictionary<string, string> LoadExplicitDeferrals()
    {
        var path = Path.Combine(Repo.Root(), "docs", "receipts", "ac-coverage", "explicit-deferrals.txt");
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        if (!File.Exists(path))
            return map;
        foreach (var line in File.ReadAllLines(path))
        {
            if (line.Length == 0 || line[0] == '#')
                continue;
            var tab = line.IndexOf('\t');
            if (tab <= 0)
                continue;
            map[line[..tab]] = line[(tab + 1)..];
        }
        return map;
    }

    private static string ExtractAcText(string text, int idIndex)
    {
        var slice = text.Substring(idIndex, Math.Min(500, text.Length - idIndex));
        var quoted = Regex.Match(slice, "text:\\s*\"([^\"]*)\"");
        if (quoted.Success)
            return quoted.Groups[1].Value;
        var plain = Regex.Match(slice, "text:\\s*([^\\r\\n]+)");
        return plain.Success ? plain.Groups[1].Value.Trim() : string.Empty;
    }

    private static string? ClassifyDeferred(AcRecord record)
    {
        var explicitMap = LoadExplicitDeferrals();
        if (explicitMap.TryGetValue(record.Id, out var explicitReason))
            return explicitReason;
        var blob = record.Id + " " + record.Window;
        if (ContainsAny(blob, "Play Store", "Play publication", "published on Google Play"))
            return "Deferred: Play Store publication is not claimed in this tree.";
        if (ContainsAny(blob, "hardware HSM", "live HSM", "HSM hardware"))
            return "Deferred: hardware HSM is not integrated; in-process Shamir remains labeled.";
        if (ContainsAny(blob, "CD green", "signed release", "Development/Staging/Production"))
            return "Deferred: lab loopback is not production CD.";
        if (ContainsAny(blob, "physical dual-phone", "two physical", "Bluetooth media", "H.264"))
            return "Deferred: physical dual-phone media transport / production H.264 is not implemented.";
        if (ContainsAny(blob, "live Bitcoin", "txid", "OpenTimestamps confirmation", "confirmed Bitcoin"))
            return "Deferred: public OTS remains pending-labeled; no invented txid.";
        if (ContainsAny(blob, "L2 signer", "live L2", "unsigned commit"))
            return "Deferred: public L2 refuses commit without a signer.";
        if (ContainsAny(blob, "Caddy", "edge TLS", "TLS 1.2"))
            return "Deferred: Omarchy loopback is not an edge TLS receipt.";
        if (ContainsAny(blob, "Concierge", "partnership"))
            return "Deferred: Lyft partnership / Concierge live path is not enabled.";
        if (ContainsAny(blob, "vehicle-fit", "on-vehicle print", "road release"))
            return "Deferred: hardware mount road-fit is operator checklist work.";
        if (record.Id.StartsWith("AC-RIDE-INGEST-004", StringComparison.Ordinal))
            return "Deferred: Concierge OAuth live partnership is not enabled.";
        if (record.Id is "AC-RIDE-CHAIN-001-002")
            return "Deferred B07: public OTS confirmation / live txid is not claimed; fixture anchors stay labeled.";
        return null;
    }

    private static bool ContainsAny(string blob, params string[] needles) =>
        needles.Any(needle => blob.Contains(needle, StringComparison.OrdinalIgnoreCase));

    private sealed record AcRecord(string Id, string Source, string Window);
}
