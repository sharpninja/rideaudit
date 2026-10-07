// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

namespace RideAudit.Server.Admission.Tests;

/// <summary>FR-RIDE-030 headrest mount notices. Checks committed text only; it is not a vehicle-fit or print receipt.</summary>
public class HeadrestGplNoticeTests
{
    [Fact]
    [Trait("FR", "FR-RIDE-030")]
    [Trait("AC", "AC-RIDE-030-003")]
    public void Headrest_scad_readme_bom_and_artifact_carry_gpl_notices_and_cad_is_not_a_road_release()
    {
        var mount = Path.Combine(FindRepoRoot(), "artifacts", "hardware", "headrest-phone-mount");
        Assert.True(File.Exists(Path.Combine(mount, "LICENSE")), "headrest LICENSE missing");

        var scad = File.ReadAllText(Path.Combine(mount, "headrest-phone-mount.scad"));
        Assert.Contains("SPDX-License-Identifier: GPL-2.0", scad, StringComparison.Ordinal);
        Assert.Contains("GNU General Public License", scad, StringComparison.Ordinal);

        var readme = File.ReadAllText(Path.Combine(mount, "README.md"));
        Assert.Contains("**License:** GPL-2.0", readme, StringComparison.Ordinal);
        Assert.Contains("GNU General Public License", readme, StringComparison.Ordinal);
        Assert.Contains("not a road release", readme, StringComparison.Ordinal);

        var bom = File.ReadAllText(Path.Combine(mount, "bom.md"));
        Assert.Contains("License: GPL-2.0", bom, StringComparison.Ordinal);

        var artifact = File.ReadAllText(Path.Combine(mount, "ARTIFACT.yaml"));
        Assert.Contains("SPDX-License-Identifier: GPL-2.0", artifact, StringComparison.Ordinal);
        Assert.Contains("license: GPL-2.0", artifact, StringComparison.Ordinal);
        Assert.Contains("Not an on-vehicle", artifact, StringComparison.Ordinal);

        foreach (var text in new[] { scad, readme, bom, artifact })
        {
            Assert.DoesNotContain("road release approved", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("SPDX-License-Identifier: MIT", text, StringComparison.Ordinal);
            Assert.DoesNotContain("SPDX-License-Identifier: Apache", text, StringComparison.Ordinal);
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
