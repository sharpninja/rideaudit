// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using RideAudit.Client.Core;
using RideAudit.PlayIntegrity;

namespace RideAudit.Bt;

public enum PhoneRole
{
    Driver = 1,
    Passenger = 2,
}

public sealed record UnsyncedInterval(TimeSpan Start, TimeSpan End, string Reason);

public sealed record SessionClock(
    DateTimeOffset MasterUtc,
    TimeSpan Drift,
    TimeSpan Uncertainty);

public sealed record SyncClockOffset(
    TimeSpan Offset,
    TimeSpan Drift,
    TimeSpan Uncertainty,
    IReadOnlyList<UnsyncedInterval> UnsyncedIntervals);

public sealed class PhoneNode
{
    public PhoneNode(string address, string displayName, PhoneRole intendedRole)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(address);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        Address = address;
        DisplayName = displayName;
        IntendedRole = intendedRole;
    }

    public string Address { get; }

    public string DisplayName { get; }

    public PhoneRole IntendedRole { get; }

    public bool RoleConfirmed { get; private set; }

    public bool SessionIntentConfirmed { get; private set; }

    public string? SessionIntentId { get; private set; }

    public void Confirm(PhoneRole role, string sessionIntentId)
    {
        if (role != IntendedRole)
        {
            throw new RideAuditFailClosedException(
                "VALIDATION_FAILED",
                "FR-RIDE-053",
                "Role confirmation does not match the intended role.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(sessionIntentId);
        RoleConfirmed = true;
        SessionIntentConfirmed = true;
        SessionIntentId = sessionIntentId;
    }
}

public sealed class Advertisement
{
    public required string Address { get; init; }
    public required string DisplayName { get; init; }
    public required string Service { get; init; }
}

/// <summary>
/// In-memory stand-in for Bluetooth discovery. Real radio access is platform-specific.
/// </summary>
public sealed class InMemoryDiscoveryBus
{
    private readonly List<Advertisement> _ads = [];

    public void Advertise(Advertisement advertisement) => _ads.Add(advertisement);

    public IReadOnlyList<Advertisement> Scan(string exceptAddress) =>
        _ads.Where(ad => !string.Equals(ad.Address, exceptAddress, StringComparison.OrdinalIgnoreCase)).ToList();

    public void Clear() => _ads.Clear();
}

public sealed class PairedSession
{
    public required string SessionIntentId { get; init; }
    public required PhoneNode Driver { get; init; }
    public required PhoneNode Passenger { get; init; }
    public required string Transport { get; init; }
}

public sealed class PairingResult
{
    private PairingResult(bool ok, PairedSession? session, string? code, string? detail)
    {
        Ok = ok;
        Session = session;
        Code = code;
        Detail = detail;
    }

    public bool Ok { get; }
    public PairedSession? Session { get; }
    public string? Code { get; }
    public string? Detail { get; }

    public static PairingResult Success(PairedSession session) => new(true, session, null, null);

    public static PairingResult Fail(string code, string detail) => new(false, null, code, detail);
}

public sealed class BluetoothPairingService
{
    private readonly InMemoryDiscoveryBus _bus;

    public BluetoothPairingService(InMemoryDiscoveryBus bus) => _bus = bus;

    public IReadOnlyList<Advertisement> Discover(PhoneNode local)
    {
        _bus.Advertise(new Advertisement
        {
            Address = local.Address,
            DisplayName = local.DisplayName,
            Service = ApiBoundary.RideAuditBluetooth,
        });
        return _bus.Scan(local.Address)
            .Where(ad => ad.Service == ApiBoundary.RideAuditBluetooth)
            .ToList();
    }

    public PairingResult Pair(PhoneNode local, PhoneNode remote)
    {
        var seen = Discover(local);
        if (seen.Count == 0 || seen.All(ad => !string.Equals(ad.Address, remote.Address, StringComparison.OrdinalIgnoreCase)))
        {
            return PairingResult.Fail("BT_DISCOVERY_FAILED", "Bluetooth discovery did not find the peer.");
        }

        if (!local.RoleConfirmed || !remote.RoleConfirmed)
        {
            return PairingResult.Fail("BT_ROLE_UNCONFIRMED", "Both phones must confirm driver vs passenger role.");
        }

        if (!local.SessionIntentConfirmed || !remote.SessionIntentConfirmed ||
            string.IsNullOrWhiteSpace(local.SessionIntentId) ||
            !string.Equals(local.SessionIntentId, remote.SessionIntentId, StringComparison.Ordinal))
        {
            return PairingResult.Fail("BT_SESSION_BINDING_FAILED", "Session intent binding failed.");
        }

        if (local.IntendedRole == remote.IntendedRole)
        {
            return PairingResult.Fail("BT_ROLE_CONFLICT", "Pairing requires one driver and one passenger.");
        }

        var driver = local.IntendedRole == PhoneRole.Driver ? local : remote;
        var passenger = local.IntendedRole == PhoneRole.Passenger ? local : remote;
        return PairingResult.Success(new PairedSession
        {
            SessionIntentId = local.SessionIntentId!,
            Driver = driver,
            Passenger = passenger,
            Transport = ApiBoundary.RideAuditBluetooth,
        });
    }
}

public enum CoordinationCommandKind
{
    Start,
    Stop,
    Clock,
}

public sealed record CoordinationCommand(
    CoordinationCommandKind Kind,
    string SessionId,
    SessionClock? Clock,
    DateTimeOffset IssuedAt);

public sealed class SessionCoordinator
{
    private readonly List<CoordinationCommand> _log = [];

    public bool Running { get; private set; }

    public SessionClock? Clock { get; private set; }

    public IReadOnlyList<CoordinationCommand> Commands => _log;

    public bool IsClockMaster(PhoneRole role) => role == PhoneRole.Driver;

    public void Start(PairedSession session, PlayAuthorization authorization, IClock clock, string sessionId)
    {
        if (!authorization.Accepted)
        {
            throw new RideAuditFailClosedException(
                "ATTESTATION_FAILED",
                "FR-RIDE-026",
                "Driver cannot start a session without Play Integrity authorization.");
        }

        if (session.Driver.IntendedRole != PhoneRole.Driver)
        {
            throw new RideAuditFailClosedException(
                "VALIDATION_FAILED",
                "FR-RIDE-054",
                "Only the driver-role phone may start an admitted dual-phone session.");
        }

        var sessionClock = new SessionClock(clock.UtcNow, TimeSpan.FromMilliseconds(1), TimeSpan.FromMilliseconds(5));
        Clock = sessionClock;
        Running = true;
        _log.Add(new CoordinationCommand(CoordinationCommandKind.Clock, sessionId, sessionClock, clock.UtcNow));
        _log.Add(new CoordinationCommand(CoordinationCommandKind.Start, sessionId, sessionClock, clock.UtcNow));
    }

    public void Stop(PhoneRole actor, string sessionId, IClock clock)
    {
        if (actor != PhoneRole.Driver)
        {
            throw new RideAuditFailClosedException(
                "VALIDATION_FAILED",
                "FR-RIDE-054",
                "Only the driver-role phone may stop the session.");
        }

        Running = false;
        _log.Add(new CoordinationCommand(CoordinationCommandKind.Stop, sessionId, Clock, clock.UtcNow));
    }

    public void PassengerAttemptStart() =>
        throw new RideAuditFailClosedException(
            "VALIDATION_FAILED",
            "FR-RIDE-054",
            "Passenger phone cannot start the session.");
}

public sealed class VideoSyncJoiner
{
    public SyncClockOffset Join(SessionClock master, DateTimeOffset passengerLocal, TimeSpan frameInterval, IReadOnlyList<TimeSpan> frameTimestamps)
    {
        var offset = passengerLocal - master.MasterUtc;
        var unsynced = new List<UnsyncedInterval>();
        for (var i = 1; i < frameTimestamps.Count; i++)
        {
            var gap = frameTimestamps[i] - frameTimestamps[i - 1];
            if (gap > frameInterval * 1.5)
            {
                unsynced.Add(new UnsyncedInterval(frameTimestamps[i - 1], frameTimestamps[i], "dropped-or-unsynced"));
            }
        }

        return new SyncClockOffset(offset, master.Drift, master.Uncertainty, unsynced);
    }
}
