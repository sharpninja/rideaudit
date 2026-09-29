// SPDX-License-Identifier: GPL-2.0-only

namespace RideAudit.Server.Admission.Tests;

/// <summary>TEST-RIDE-040. FR-RIDE-064 / TR-RIDE-EDGE-001.</summary>
public class NgrokDeploySecretsTests
{
    [Fact]
    [Trait("TEST", "TEST-RIDE-040")]
    [Trait("FR", "FR-RIDE-064")]
    [Trait("TR", "TR-RIDE-EDGE-001")]
    [Trait("AC", "AC-TEST-040-001")]
    [Trait("AC", "AC-TEST-040-002")]
    [Trait("AC", "AC-RIDE-064-002")]
    [Trait("AC", "AC-RIDE-EDGE-001-001")]
    public void Ngrok_example_and_scripts_use_placeholders_not_tokens()
    {
        var root = FindRepoRoot();
        var ngrokDir = Path.Combine(root, "deploy", "omarchy", "ngrok");
        var example = Path.Combine(ngrokDir, "ngrok.yml.example");
        Assert.True(File.Exists(example), example);

        var exampleText = File.ReadAllText(example);
        Assert.Contains("YOUR_NGROK_AUTHTOKEN", exampleText, StringComparison.Ordinal);
        Assert.Contains("127.0.0.1:18080", exampleText, StringComparison.Ordinal);

        foreach (var path in Directory.GetFiles(ngrokDir, "*", SearchOption.TopDirectoryOnly))
        {
            var name = Path.GetFileName(path);
            if (name.Equals("ngrok.yml", StringComparison.OrdinalIgnoreCase))
            {
                Assert.Fail("committed ngrok.yml is forbidden; only ngrok.yml.example may exist.");
            }

            var text = File.ReadAllText(path);
            Assert.DoesNotMatch(
                @"(?im)^\s*authtoken\s*:\s*(?!YOUR_NGROK_AUTHTOKEN\s*$)(?!.*placeholder)\S+",
                text);
        }

        var start = File.ReadAllText(Path.Combine(ngrokDir, "Start-Ngrok.ps1"));
        Assert.Contains(".creds\\ngrok.yml", start, StringComparison.Ordinal);
        Assert.Contains("OmarchySsh.ps1", start, StringComparison.Ordinal);
        Assert.Contains("Copy-OmarchyStdinFile", start, StringComparison.Ordinal);
        Assert.Contains("fail-closed", start, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ghcr.io", start, StringComparison.OrdinalIgnoreCase);

        var readme = File.ReadAllText(Path.Combine(ngrokDir, "README.md"));
        Assert.Contains("PAYTON-OMARCHY", readme, StringComparison.Ordinal);
        Assert.Contains("PAYTON-DESKTOP", readme, StringComparison.Ordinal);
        Assert.Contains("127.0.0.1:18080", readme, StringComparison.Ordinal);
        Assert.Contains("KeepConflictingLocal", readme, StringComparison.Ordinal);
        Assert.DoesNotContain("ghcr.io", readme, StringComparison.OrdinalIgnoreCase);
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "RideAudit.sln")) &&
                Directory.Exists(Path.Combine(dir.FullName, "deploy", "omarchy", "ngrok")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }
}
