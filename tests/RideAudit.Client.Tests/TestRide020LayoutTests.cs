// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using RideAudit.Client.Core;
using RideAudit.Client.Tests.Support;
using RideAudit.Licensing;
using Xunit;

namespace RideAudit.Client.Tests;

[Trait("Partition", "TEST-RIDE-020")]
public class TestRide020LayoutTests
{
    [Fact]
    [Trait("FR", "FR-RIDE-029")]
    [Trait("AC", "AC-RIDE-029-001")]
    public void Repository_license_is_gpl_2()
    {
        var license = File.ReadAllText(Path.Combine(Repo.Root(), "LICENSE"));
        Assert.Contains("GNU GENERAL PUBLIC LICENSE", license);
        Assert.Contains("Version 2, June 1991", license);
        var props = File.ReadAllText(Path.Combine(Repo.Root(), "Directory.Build.props"));
        Assert.Contains("GPL-2.0-only", props);
        Assert.DoesNotContain("Apache-2.0", props);
        Assert.DoesNotContain(">MIT<", props);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-029")]
    [Trait("AC", "AC-RIDE-029-002")]
    public void Source_and_license_notices_are_published()
    {
        var root = Repo.Root();
        Assert.True(File.Exists(Path.Combine(root, "NOTICE")));
        Assert.Contains("GPL-2.0-or-later", File.ReadAllText(Path.Combine(root, "NOTICE")));
        Assert.Contains("SPDX-License-Identifier: GPL-2.0-or-later", File.ReadAllText(Path.Combine(root, "src", "RideAudit.Client.Seal", "CollectionSealer.cs")));
    }

    [Fact]
    [Trait("FR", "FR-RIDE-029")]
    [Trait("AC", "AC-RIDE-029-003")]
    public void Mit_is_not_accepted_as_the_client_license()
    {
        var forged = new DistributionManifest
        {
            Schema = "x",
            License = "MIT",
            SourceRepository = new SourceRepositoryClaim { Url = "https://example.invalid", PublicSourcePresent = true, Note = "n" },
            PlayStore = new PlayStoreClaim { Published = false, Note = "n" },
            DesktopBuilds = new DesktopBuildClaim { Windows = "not-produced", Linux = "not-produced", Macos = "not-produced", ReproducibleSignedClaim = false, Note = "n" },
            RoadReady = false,
        };
        var ex = Assert.Throws<RideAuditFailClosedException>(() => PublicationClaimGuard.AssertHonest(forged, static _ => false));
        Assert.Equal("FR-RIDE-029", ex.RequirementId);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-030")]
    [Trait("AC", "AC-RIDE-030-001")]
    public void Artifact_metadata_carries_gpl_and_commit_notice()
    {
        var receipt = Fixtures.CaptureHappy().Capture.SealedComposite.Receipt;
        Assert.Equal(LicenseMetadata.GplId, receipt.License.LicenseId);
        Assert.Equal("2", receipt.License.LicenseVersion);
        Assert.Equal("test-commit", receipt.License.SourceCommitNotice);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-030")]
    [Trait("AC", "AC-RIDE-030-002")]
    public void License_metadata_does_not_carry_sealed_payloads()
    {
        var ex = Assert.Throws<RideAuditFailClosedException>(() => LicenseMetadata.ForArtifact("PLAINTEXT payload"));
        Assert.Equal("FR-RIDE-030", ex.RequirementId);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-031")]
    [Trait("AC", "AC-RIDE-031-002")]
    public void Distribution_scaffold_is_present_and_does_not_invent_a_play_receipt()
    {
        var manifest = PublicationClaimGuard.LoadEmbedded();
        Assert.Equal("https://github.com/sharpninja/rideaudit", manifest.SourceRepository.Url);
        Assert.True(manifest.SourceRepository.PublicSourcePresent);
        Assert.False(manifest.PlayStore.Published);
        Assert.Null(manifest.PlayStore.ReceiptPath);
        Assert.False(manifest.RoadReady);
        Assert.False(manifest.DesktopBuilds.ReproducibleSignedClaim);
        var path = Path.Combine(Repo.Root(), "src", "RideAudit.Licensing", "Distribution", "client-distribution-manifest.json");
        var json = File.ReadAllText(path);
        Assert.Contains("\"published\": false", json);
        Assert.DoesNotContain("play.google.com/store/apps/details", json);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-031")]
    [Trait("AC", "AC-RIDE-031-001")]
    public void Play_published_claim_without_receipt_is_rejected()
    {
        var manifest = PublicationClaimGuard.LoadEmbedded();
        var forged = new DistributionManifest
        {
            Schema = manifest.Schema,
            License = manifest.License,
            SourceRepository = manifest.SourceRepository,
            PlayStore = new PlayStoreClaim
            {
                Published = true,
                ReceiptPath = "missing-play-receipt.json",
                ListingUrl = "https://play.google.com/store/apps/details?id=org.rideaudit.app",
                Note = "forged",
            },
            DesktopBuilds = manifest.DesktopBuilds,
            RoadReady = false,
        };
        var ex = Assert.Throws<RideAuditFailClosedException>(() => PublicationClaimGuard.AssertHonest(forged, static _ => false));
        Assert.Equal("FR-RIDE-031", ex.RequirementId);
        Assert.Contains("receipt", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
