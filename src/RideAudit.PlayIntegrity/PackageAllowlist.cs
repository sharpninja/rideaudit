// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

namespace RideAudit.PlayIntegrity;

public sealed record AllowlistEntry(string PackageIdentity, string SigningCertDigest);

public sealed record AllowlistRotation(
    int Version,
    string Action,
    AllowlistEntry Entry,
    string Reason,
    string Actor,
    DateTimeOffset When);

/// <summary>
/// Versioned package identity and signing-certificate allowlist (FR-RIDE-215).
/// </summary>
public sealed class PackageAllowlist
{
    private readonly List<AllowlistEntry> _entries;
    private readonly List<AllowlistRotation> _rotations;

    public PackageAllowlist(
        int version,
        IEnumerable<AllowlistEntry> entries,
        IEnumerable<AllowlistRotation>? rotations = null)
    {
        if (version < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(version));
        }

        Version = version;
        _entries = entries.ToList();
        _rotations = rotations?.ToList() ?? [];
        if (_entries.Count == 0)
        {
            throw new ArgumentException("Allowlist must contain at least one entry.", nameof(entries));
        }
    }

    public int Version { get; }

    public IReadOnlyList<AllowlistEntry> Entries => _entries;

    public IReadOnlyList<AllowlistRotation> Rotations => _rotations;

    public bool IsAllowed(string packageIdentity, string signingCertDigest) =>
        _entries.Any(entry =>
            string.Equals(entry.PackageIdentity, packageIdentity, StringComparison.Ordinal) &&
            string.Equals(entry.SigningCertDigest, signingCertDigest, StringComparison.OrdinalIgnoreCase));

    public PackageAllowlist RotateAdd(AllowlistEntry entry, string reason, string actor, DateTimeOffset when)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        ArgumentException.ThrowIfNullOrWhiteSpace(actor);
        var next = new List<AllowlistEntry>(_entries) { entry };
        var rotations = new List<AllowlistRotation>(_rotations)
        {
            new(Version + 1, "add", entry, reason, actor, when),
        };
        return new PackageAllowlist(Version + 1, next, rotations);
    }

    public PackageAllowlist RotateRemove(AllowlistEntry entry, string reason, string actor, DateTimeOffset when)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        var next = _entries.Where(existing =>
            !(string.Equals(existing.PackageIdentity, entry.PackageIdentity, StringComparison.Ordinal) &&
              string.Equals(existing.SigningCertDigest, entry.SigningCertDigest, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        if (next.Count == 0)
        {
            throw new InvalidOperationException("Refusing to empty the authenticity allowlist.");
        }

        var rotations = new List<AllowlistRotation>(_rotations)
        {
            new(Version + 1, "remove", entry, reason, actor, when),
        };
        return new PackageAllowlist(Version + 1, next, rotations);
    }

    public static PackageAllowlist CreateDevelopmentDefault() =>
        new(
            1,
            [
                new AllowlistEntry(ApprovedPackage.PackageName, ApprovedPackage.CertDigest),
            ]);
}

public static class ApprovedPackage
{
    public const string PackageName = "org.rideaudit.app";
    public const string CertDigest = "sha256:c0ffee00rideaudit-dev-cert";
}
