// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

namespace RideAudit.Client.Core;

/// <summary>
/// GPL-2.0 notice attached to shared software artifacts. Does not carry evidence payloads.
/// </summary>
public sealed record LicenseMetadata(
    string LicenseId,
    string LicenseVersion,
    string SourceCommitNotice)
{
    public const string GplId = "GPL-2.0-or-later";
    public const string GplVersion = "2";

    public static LicenseMetadata ForArtifact(string sourceCommitNotice)
    {
        if (string.IsNullOrWhiteSpace(sourceCommitNotice))
        {
            throw new ArgumentException("Source commit notice is required.", nameof(sourceCommitNotice));
        }

        if (sourceCommitNotice.Contains("PLAINTEXT", StringComparison.OrdinalIgnoreCase))
        {
            throw new RideAuditFailClosedException(
                "LICENSE_PAYLOAD",
                "FR-RIDE-030",
                "License metadata must not embed evidence payloads.");
        }

        return new LicenseMetadata(GplId, GplVersion, sourceCommitNotice);
    }
}

public static class ApiBoundary
{
    public const string RideAuditBluetooth = "bluetooth-rideaudit-device-pairing";
    public static readonly Guid RideAuditBluetoothService = Guid.Parse("f0a1d17e-6c2b-4e3a-9b11-81c3a4d5e6f7");

    /// <summary>
    /// FR-RIDE-053 / AC-RIDE-053-003. A scanned peer is a RideAudit device only when its
    /// advertisement carries the RideAudit service UUID. Adapters apply this per advertisement,
    /// so a platform that cannot install a scan filter still never surfaces a foreign peer.
    /// </summary>
    public static bool AdvertisesRideAuditService(IEnumerable<Guid>? serviceUuids) =>
        serviceUuids is not null && serviceUuids.Contains(RideAuditBluetoothService);

    public const string SealedContentType = "application/vnd.rideaudit.sealed+octet-stream";

    /// <summary>
    /// RideAudit does not call Lyft private APIs. This constant exists so tests can
    /// assert the boundary stays named and unused.
    /// </summary>
    public const string LyftPrivateApiStatus = "not-used";
}
