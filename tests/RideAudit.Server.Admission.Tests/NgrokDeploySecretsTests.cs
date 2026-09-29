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
    [Trait("AC", "AC-RIDE-064-003")]
    [Trait("AC", "AC-UC-033-001")]
    [Trait("AC", "AC-TEST-039-002")]
    [Trait("AC", "AC-RIDE-EDGE-001-001")]
    [Trait("AC", "AC-RIDE-EDGE-001-002")]
    public void Ngrok_example_and_scripts_use_placeholders_not_tokens()
    {
        var root = FindRepoRoot();
        var ngrokDir = Path.Combine(root, "deploy", "omarchy", "ngrok");
        var example = Path.Combine(ngrokDir, "ngrok.yml.example");
        Assert.True(File.Exists(example), example);

        var exampleText = File.ReadAllText(example);
        Assert.Contains("YOUR_NGROK_AUTHTOKEN", exampleText, StringComparison.Ordinal);
        Assert.Contains("192.168.0.149:28080", exampleText, StringComparison.Ordinal);
        Assert.DoesNotContain("127.0.0.1:18080", exampleText, StringComparison.Ordinal);

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

        Assert.Contains("[string]$Addr = \"192.168.0.149:28080\"", start, StringComparison.Ordinal);
        Assert.Contains("[string]$SshHost = \"PAYTON-DESKTOP\"", start, StringComparison.Ordinal);
        Assert.Contains("No public URL is advertised", start, StringComparison.Ordinal);

        var unit = File.ReadAllText(Path.Combine(ngrokDir, "rideaudit-ngrok.service"));
        Assert.Contains("192.168.0.149:28080", unit, StringComparison.Ordinal);
        Assert.Contains("Restart=on-failure", unit, StringComparison.Ordinal);

        var remote = File.ReadAllText(Path.Combine(ngrokDir, "remote-start.sh"));
        Assert.Contains("192.168.0.149:28080", remote, StringComparison.Ordinal);
        Assert.DoesNotContain("python3", remote, StringComparison.Ordinal);
        Assert.DoesNotContain("python ", remote, StringComparison.Ordinal);

        var readme = File.ReadAllText(Path.Combine(ngrokDir, "README.md"));
        Assert.Contains("PAYTON-OMARCHY", readme, StringComparison.Ordinal);
        Assert.Contains("PAYTON-DESKTOP", readme, StringComparison.Ordinal);
        Assert.Contains("192.168.0.149:28080", readme, StringComparison.Ordinal);
        Assert.Contains("127.0.0.1:18080", readme, StringComparison.Ordinal);
        Assert.Contains("prior interim", readme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("KeepConflictingLocal", readme, StringComparison.Ordinal);
        Assert.DoesNotContain("ghcr.io", readme, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("TEST", "TEST-RIDE-038")]
    [Trait("FR", "FR-RIDE-063")]
    [Trait("UC", "UC-RIDE-032")]
    [Trait("AC", "AC-TEST-038-001")]
    [Trait("AC", "AC-TEST-038-002")]
    [Trait("AC", "AC-RIDE-063-001")]
    [Trait("AC", "AC-RIDE-063-003")]
    [Trait("AC", "AC-UC-032-001")]
    [Trait("AC", "AC-RIDE-063-002")]
    [Trait("AC", "AC-RIDE-DEPLOY-001-001")]
    [Trait("AC", "AC-RIDE-DEPLOY-001-002")]
    [Trait("AC", "AC-RIDE-DEPLOY-002-001")]
    [Trait("AC", "AC-RIDE-DEPLOY-002-002")]
    public void Octopus_desktop_receipt_names_instance_and_does_not_claim_ghcr()
    {
        var root = FindRepoRoot();
        var receipt = File.ReadAllText(Path.Combine(root, "docs", "receipts", "distribution", "20260929T015822Z-octopus-payton-desktop.md"));
        Assert.Contains("octopus-rideaudit", receipt, StringComparison.Ordinal);
        Assert.Contains("PAYTON-DESKTOP", receipt, StringComparison.Ordinal);
        Assert.Contains("28080", receipt, StringComparison.Ordinal);
        Assert.Contains("28081", receipt, StringComparison.Ordinal);
        Assert.Contains("deploy/containers/admission/Dockerfile", receipt, StringComparison.Ordinal);
        Assert.Contains("New container created | yes", receipt, StringComparison.Ordinal);
        Assert.Contains("free license", receipt, StringComparison.Ordinal);
        Assert.Contains("TentacleActive", receipt, StringComparison.Ordinal);
        Assert.Contains("No GHCR push or pull", receipt, StringComparison.Ordinal);
        Assert.Contains("Not a GHCR green", receipt, StringComparison.Ordinal);
        Assert.DoesNotContain("ghcr.io", receipt, StringComparison.OrdinalIgnoreCase);

        var compose = File.ReadAllText(Path.Combine(root, "deploy", "octopus", "compose.yaml"));
        Assert.Contains("192.168.0.149:28080:8080", compose, StringComparison.Ordinal);
        Assert.Contains("192.168.0.149:28081:8080", compose, StringComparison.Ordinal);
        Assert.DoesNotContain("ghcr.io", compose, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("TEST", "TEST-RIDE-039")]
    [Trait("FR", "FR-RIDE-064")]
    [Trait("UC", "UC-RIDE-033")]
    [Trait("AC", "AC-RIDE-064-001")]
    [Trait("AC", "AC-TEST-039-001")]
    public void Desktop_ngrok_receipt_records_http_200_on_canonical_admission()
    {
        var root = FindRepoRoot();
        var receipt = File.ReadAllText(Path.Combine(
            root, "docs", "receipts", "distribution", "20260929T030643Z-ngrok-desktop-28080.md"));
        Assert.Contains("192.168.0.149:28080", receipt, StringComparison.Ordinal);
        Assert.Contains("PROBE_HTTP=200", receipt, StringComparison.Ordinal);
        Assert.Contains("PROBE_BODY_OK=1", receipt, StringComparison.Ordinal);
        Assert.Contains("https://zeugmatically-unindicative-calista.ngrok-free.dev", receipt, StringComparison.Ordinal);
        Assert.Contains("127.0.0.1:18080", receipt, StringComparison.Ordinal);
        Assert.Contains("prior interim", receipt, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("authtoken:", receipt, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ghcr.io/", receipt, StringComparison.OrdinalIgnoreCase);
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
