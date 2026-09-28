// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Text.Json;
using RideAudit.Client.Core;

namespace RideAudit.Licensing;

public sealed class DistributionManifest
{
    public required string Schema { get; init; }
    public required string License { get; init; }
    public required SourceRepositoryClaim SourceRepository { get; init; }
    public required PlayStoreClaim PlayStore { get; init; }
    public required DesktopBuildClaim DesktopBuilds { get; init; }
    public required bool RoadReady { get; init; }
}

public sealed class SourceRepositoryClaim
{
    public required string Url { get; init; }
    public required bool PublicSourcePresent { get; init; }
    public required string Note { get; init; }
}

public sealed class PlayStoreClaim
{
    public required bool Published { get; init; }
    public string? ReceiptPath { get; init; }
    public string? ListingUrl { get; init; }
    public required string Note { get; init; }
}

public sealed class DesktopBuildClaim
{
    public required string Windows { get; init; }
    public required string Linux { get; init; }
    public required string Macos { get; init; }
    public required bool ReproducibleSignedClaim { get; init; }
    public required string Note { get; init; }
}

public static class PublicationClaimGuard
{
    public static DistributionManifest LoadEmbedded()
    {
        var assembly = typeof(PublicationClaimGuard).Assembly;
        var name = assembly.GetManifestResourceNames().Single(resource => resource.EndsWith("client-distribution-manifest.json", StringComparison.Ordinal));
        using var stream = assembly.GetManifestResourceStream(name) ?? throw new InvalidOperationException("Manifest missing.");
        var manifest = JsonSerializer.Deserialize<DistributionManifest>(stream, JsonOptions())
            ?? throw new InvalidOperationException("Manifest empty.");
        AssertHonest(manifest, static _ => false);
        return manifest;
    }

    public static void AssertHonest(DistributionManifest manifest, Func<string, bool> receiptExists)
    {
        if (!string.Equals(manifest.License, LicenseMetadata.GplId, StringComparison.Ordinal))
        {
            throw new RideAuditFailClosedException(
                "VALIDATION_FAILED",
                "FR-RIDE-029",
                "Client distribution license must be GPL-2.0-or-later.");
        }

        if (manifest.License.Contains("Apache-2.0", StringComparison.Ordinal) ||
            string.Equals(manifest.License, "MIT", StringComparison.Ordinal))
        {
            throw new RideAuditFailClosedException(
                "VALIDATION_FAILED",
                "FR-RIDE-029",
                "Apache-2.0 or MIT must not replace GPL-2.0 for the client.");
        }

        if (manifest.PlayStore.Published)
        {
            if (string.IsNullOrWhiteSpace(manifest.PlayStore.ReceiptPath) || !receiptExists(manifest.PlayStore.ReceiptPath))
            {
                throw new RideAuditFailClosedException(
                    "VALIDATION_FAILED",
                    "FR-RIDE-031",
                    "Refusing a Play publication claim without a real receipt file.");
            }
        }

        if (manifest.RoadReady)
        {
            throw new RideAuditFailClosedException(
                "VALIDATION_FAILED",
                "FR-RIDE-031",
                "This build must not claim road-ready.");
        }

        if (manifest.DesktopBuilds.ReproducibleSignedClaim)
        {
            throw new RideAuditFailClosedException(
                "VALIDATION_FAILED",
                "FR-RIDE-222",
                "Signed reproducible desktop builds were not produced.");
        }
    }

    private static JsonSerializerOptions JsonOptions() => new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
}
