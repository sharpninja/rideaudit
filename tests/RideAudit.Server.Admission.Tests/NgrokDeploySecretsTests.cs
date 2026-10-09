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
        Assert.Contains("192.168.1.182:28080", exampleText, StringComparison.Ordinal);
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

        Assert.Contains("[string]$Addr = \"192.168.1.182:28080\"", start, StringComparison.Ordinal);
        Assert.Contains("[string]$SshHost = \"LAB-OMARCHY\"", start, StringComparison.Ordinal);
        Assert.Contains("No public URL is advertised", start, StringComparison.Ordinal);

        var unit = File.ReadAllText(Path.Combine(ngrokDir, "rideaudit-ngrok.service"));
        Assert.Contains("192.168.1.182:28080", unit, StringComparison.Ordinal);
        Assert.Contains("Restart=on-failure", unit, StringComparison.Ordinal);

        var remote = File.ReadAllText(Path.Combine(ngrokDir, "remote-start.sh"));
        Assert.Contains("192.168.1.182:28080", remote, StringComparison.Ordinal);
        Assert.DoesNotContain("python3", remote, StringComparison.Ordinal);
        Assert.DoesNotContain("python ", remote, StringComparison.Ordinal);

        var readme = File.ReadAllText(Path.Combine(ngrokDir, "README.md"));
        Assert.Contains("LAB-OMARCHY", readme, StringComparison.Ordinal);
        Assert.Contains("192.168.1.182:28080", readme, StringComparison.Ordinal);
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
        Assert.Contains("192.168.1.182:28080:8080", compose, StringComparison.Ordinal);
        Assert.Contains("192.168.1.182:28081:8080", compose, StringComparison.Ordinal);
        Assert.DoesNotContain("ghcr.io", compose, StringComparison.OrdinalIgnoreCase);

        var octopusRelease = File.ReadAllText(Path.Combine(root, "deploy", "octopus", "Invoke-RideAuditOctopusRelease.ps1"));
        Assert.Contains("[string]$TargetRole = \"rideaudit-host-lab-omarchy\"", octopusRelease, StringComparison.Ordinal);
        Assert.DoesNotContain("[string]$TargetRole = \"rideaudit-host\"", octopusRelease, StringComparison.Ordinal);
        Assert.Contains("ambiguous Octopus role", octopusRelease, StringComparison.Ordinal);
        Assert.Contains("refuse role-first fallback to an old host", octopusRelease, StringComparison.Ordinal);
        Assert.Contains("$deployBody[\"SpecificMachineIds\"] = @($MachineId)", octopusRelease, StringComparison.Ordinal);
        Assert.Contains("-MachineId $machineIdForDeploy", octopusRelease, StringComparison.Ordinal);
        Assert.Contains("refuse role-wide deploy", octopusRelease, StringComparison.Ordinal);
        var tentacleCompose = File.ReadAllText(Path.Combine(root, "deploy", "octopus", "new-instance", "compose.yaml"));
        Assert.Contains("TargetRole: rideaudit-host-lab-omarchy", tentacleCompose, StringComparison.Ordinal);

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

    [Fact]
    [Trait("TEST", "TEST-RIDE-038")]
    [Trait("FR", "FR-RIDE-063")]
    [Trait("UC", "UC-RIDE-032")]
    [Trait("AC", "AC-RIDE-063-004")]
    [Trait("AC", "AC-RIDE-DEPLOY-002-003")]
    [Trait("AC", "AC-TEST-038-003")]
    [Trait("AC", "AC-UC-032-002")]
    public void Omarchy_compose_cutover_is_prior_interim_and_not_the_octopus_cd_green()
    {
        var root = FindRepoRoot();

        // The compose cutover is recorded as loopback-only interim evidence and says it is not a CD green.
        var cutoverReceipt = File.ReadAllText(Path.Combine(root, "docs", "receipts", "distribution", "legion2-omarchy-20260928.md"));
        Assert.Contains("compose up on loopback only", cutoverReceipt, StringComparison.Ordinal);
        Assert.Contains("rideaudit-omarchy-admission-1", cutoverReceipt, StringComparison.Ordinal);
        Assert.Contains("Still not a CD green", cutoverReceipt, StringComparison.Ordinal);
        Assert.DoesNotContain("octopus-rideaudit", cutoverReceipt, StringComparison.Ordinal);

        var omarchyReadme = File.ReadAllText(Path.Combine(root, "deploy", "omarchy", "README.md"));
        Assert.Contains("It is not a continuous-delivery receipt and not a production cutover.", omarchyReadme, StringComparison.Ordinal);
        Assert.Contains("Omarchy loopback plus ngrok is interim admission hosting only.", omarchyReadme, StringComparison.Ordinal);
        Assert.Contains("That bind is the prior interim.", omarchyReadme, StringComparison.Ordinal);

        var confirmCutover = File.ReadAllText(Path.Combine(root, "deploy", "omarchy", "Confirm-Cutover.ps1"));
        Assert.Contains("Not a CD green.", confirmCutover, StringComparison.Ordinal);
        Assert.Contains("This is still not a CD green until the coordinator records a receipt.", confirmCutover, StringComparison.Ordinal);

        // The Octopus receipt that TEST-RIDE-038 scores is a different compose project and excludes the interim loopback.
        var octopusReceipt = File.ReadAllText(Path.Combine(root, "docs", "receipts", "distribution", "20260929T015822Z-octopus-payton-desktop.md"));
        Assert.Contains("Compose project `rideaudit-octopus` (not `rideaudit-omarchy`)", octopusReceipt, StringComparison.Ordinal);
        Assert.Contains("Omarchy interim loopback", octopusReceipt, StringComparison.Ordinal);
        Assert.Contains("It is not this receipt", octopusReceipt, StringComparison.Ordinal);
        Assert.DoesNotContain("Confirm-Cutover", octopusReceipt, StringComparison.Ordinal);
        Assert.DoesNotContain("legion2-omarchy-20260928", octopusReceipt, StringComparison.Ordinal);
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
