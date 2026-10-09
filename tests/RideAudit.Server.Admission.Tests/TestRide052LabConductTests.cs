// SPDX-License-Identifier: GPL-2.0-only
// Copyright (C) 2026 RideAudit contributors

namespace RideAudit.Server.Admission.Tests;

/// <summary>
/// TEST-RIDE-052 / FR-RIDE-072.
/// Scope is committed in-repo lab toolchain under artifacts/hardware
/// (e.g. headrest-phone-mount), not the physical LAB-OMARCHY host machine.
/// </summary>
public class TestRide052LabConductTests
{
    private static readonly HashSet<string> TextExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".md", ".txt", ".yaml", ".yml", ".json", ".sh", ".ps1", ".scad", ".cs",
        ".csproj", ".props", ".targets", ".sln", ".gitignore", ".editorconfig",
        ".csv", ".tsv", ".xml", ".html", ".css", ".js", ".ts", ".toml", ".ini",
        ".cfg", ".conf", ".service", ".example", ".notice", ".license",
    };

    [Fact]
    [Trait("TEST", "TEST-RIDE-052")]
    [Trait("FR", "FR-RIDE-072")]
    [Trait("TR", "TR-RIDE-LAB-004")]
    [Trait("AC", "AC-TEST-052-001")]
    [Trait("AC", "AC-RIDE-072-001")]
    [Trait("AC", "AC-RIDE-072-004")]
    public void Committed_lab_text_has_receipt_reference_and_no_em_or_en_dashes()
    {
        var root = FindRepoRoot();
        var labRoot = Path.Combine(root, "artifacts", "hardware");
        Assert.True(Directory.Exists(labRoot), labRoot);

        var receiptHits = 0;
        var dashHits = new List<string>();

        foreach (var path in EnumerateLabTextFiles(labRoot))
        {
            var text = File.ReadAllText(path);
            if (text.Contains("receipt", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("docs/receipts", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("docs\\receipts", StringComparison.OrdinalIgnoreCase))
            {
                receiptHits++;
            }

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (c is '\u2013' or '\u2014')
                {
                    var rel = Path.GetRelativePath(root, path);
                    dashHits.Add($"{rel}:{i + 1}: U+{(int)c:X4}");
                    if (dashHits.Count >= 20)
                    {
                        break;
                    }
                }
            }

            if (dashHits.Count >= 20)
            {
                break;
            }
        }

        Assert.True(receiptHits > 0, "lab tree should reference receipts somewhere under artifacts/hardware");
        Assert.True(dashHits.Count == 0, "em/en dashes in committed lab text:\n" + string.Join("\n", dashHits));
    }

    [Fact]
    [Trait("TEST", "TEST-RIDE-052")]
    [Trait("FR", "FR-RIDE-072")]
    [Trait("TR", "TR-RIDE-LAB-004")]
    [Trait("AC", "AC-TEST-052-002")]
    [Trait("AC", "AC-RIDE-072-003")]
    [Trait("AC", "AC-RIDE-072-005")]
    public void Committed_lab_toolchain_has_no_python_and_csharp_geometry_entrypoint_exists()
    {
        var root = FindRepoRoot();
        var labRoot = Path.Combine(root, "artifacts", "hardware");
        Assert.True(Directory.Exists(labRoot), labRoot);

        var pyFiles = Directory.EnumerateFiles(labRoot, "*.py", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(labRoot, "*.pyw", SearchOption.AllDirectories))
            .Select(p => Path.GetRelativePath(root, p))
            .ToList();
        Assert.True(pyFiles.Count == 0, "Python files under artifacts/hardware:\n" + string.Join("\n", pyFiles));

        var pythonInvokes = new List<string>();
        foreach (var path in EnumerateLabTextFiles(labRoot))
        {
            var lines = File.ReadAllLines(path);
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.Contains("python3", StringComparison.OrdinalIgnoreCase) ||
                    System.Text.RegularExpressions.Regex.IsMatch(line, @"(?i)(?<![\w.])python(\s|$)") ||
                    line.Contains("verify-geometry.py", StringComparison.OrdinalIgnoreCase))
                {
                    pythonInvokes.Add($"{Path.GetRelativePath(root, path)}:{i + 1}:{line.Trim()}");
                }
            }
        }

        Assert.True(pythonInvokes.Count == 0, "Python lab toolchain refs under artifacts/hardware:\n" + string.Join("\n", pythonInvokes));

        var csproj = Path.Combine(root, "tools", "RideAudit.HeadrestGeometry", "RideAudit.HeadrestGeometry.csproj");
        var program = Path.Combine(root, "tools", "RideAudit.HeadrestGeometry", "Program.cs");
        Assert.True(File.Exists(csproj), csproj);
        Assert.True(File.Exists(program), program);

        var programText = File.ReadAllText(program);
        Assert.Contains("RideAudit.HeadrestGeometry", programText, StringComparison.Ordinal);
        Assert.DoesNotContain("python", programText, StringComparison.OrdinalIgnoreCase);

        // Go-by-default host names on FR-072 / TEST-052 wiki sections (disk SoT mirror).
        var frWiki = File.ReadAllText(Path.Combine(root, "docs", "Project", "wiki", "github", "Functional-Requirements.md"));
        var testWiki = File.ReadAllText(Path.Combine(root, "docs", "Project", "wiki", "github", "Testing-Requirements.md"));
        var fr072Idx = frWiki.IndexOf("## FR-RIDE-072", StringComparison.Ordinal);
        Assert.True(fr072Idx >= 0, "FR-RIDE-072 missing from Functional-Requirements wiki");
        var fr073Idx = frWiki.IndexOf("## FR-RIDE-073", fr072Idx, StringComparison.Ordinal);
        var fr072 = fr073Idx > fr072Idx ? frWiki.Substring(fr072Idx, fr073Idx - fr072Idx) : frWiki.Substring(fr072Idx);
        var test052Idx = testWiki.IndexOf("### TEST-RIDE-052", StringComparison.Ordinal);
        Assert.True(test052Idx >= 0, "TEST-RIDE-052 missing from Testing-Requirements wiki");
        var test053Idx = testWiki.IndexOf("### TEST-RIDE-053", test052Idx, StringComparison.Ordinal);
        var test052 = test053Idx > test052Idx ? testWiki.Substring(test052Idx, test053Idx - test052Idx) : testWiki.Substring(test052Idx);
        Assert.Contains("PAYTON-DESKTOP", fr072, StringComparison.Ordinal);
        Assert.Contains("PAYTON-LEGION2", fr072, StringComparison.Ordinal);
        Assert.Contains("PAYTON-DESKTOP", test052, StringComparison.Ordinal);
        Assert.Contains("PAYTON-LEGION2", test052, StringComparison.Ordinal);
        Assert.Contains("artifacts/hardware", test052, StringComparison.Ordinal);
    }

    private static IEnumerable<string> EnumerateLabTextFiles(string labRoot)
    {
        foreach (var path in Directory.EnumerateFiles(labRoot, "*", SearchOption.AllDirectories))
        {
            var name = Path.GetFileName(path);
            if (name.StartsWith(".", StringComparison.Ordinal))
            {
                continue;
            }

            var ext = Path.GetExtension(path);
            if (ext.Length == 0)
            {
                if (name.Equals("LICENSE", StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("NOTICE", StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("README", StringComparison.OrdinalIgnoreCase))
                {
                    yield return path;
                }

                continue;
            }

            if (TextExtensions.Contains(ext) || name.EndsWith(".sh", StringComparison.OrdinalIgnoreCase))
            {
                yield return path;
            }
        }
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "RideAudit.sln")) &&
                Directory.Exists(Path.Combine(dir.FullName, "artifacts", "hardware")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }
}
