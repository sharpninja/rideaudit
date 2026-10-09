// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using RideAudit.Bt;
using RideAudit.Client.Core;
using RideAudit.Client.Tests.Support;
using Xunit;

namespace RideAudit.Client.Tests;

/// <summary>FR-RIDE-053 / UC-RIDE-022: pairing runs over the RideAudit Bluetooth service only.</summary>
[Trait("Partition", "TEST-RIDE-034")]
public class BluetoothRideAuditOnlyTests
{
    [Fact]
    [Trait("FR", "FR-RIDE-053")]
    [Trait("UC", "UC-RIDE-022")]
    [Trait("AC", "AC-RIDE-053-003")]
    [Trait("AC", "AC-UC-022-001")]
    public void Pairing_uses_only_the_rideaudit_bluetooth_service_and_no_lyft_api()
    {
        var driver = new PhoneNode("11:11:11:11:11:11", "driver", PhoneRole.Driver);
        var passenger = new PhoneNode("22:22:22:22:22:22", "passenger", PhoneRole.Passenger);
        driver.Confirm(PhoneRole.Driver, "intent");
        passenger.Confirm(PhoneRole.Passenger, "intent");

        // A peer advertised under any other Bluetooth service is not discovered and cannot pair.
        var foreignBus = new InMemoryDiscoveryBus();
        foreignBus.Advertise(new Advertisement { Address = passenger.Address, DisplayName = passenger.DisplayName, Service = "lyft-bluetooth" });
        var foreignService = new BluetoothPairingService(foreignBus);
        Assert.Empty(foreignService.Discover(driver));
        var foreign = foreignService.Pair(driver, passenger);
        Assert.False(foreign.Ok);
        Assert.Equal("BT_DISCOVERY_FAILED", foreign.Code);

        // The same peer on the RideAudit service pairs, and the session transport is the RideAudit Bluetooth session.
        var bus = new InMemoryDiscoveryBus();
        bus.Advertise(new Advertisement { Address = passenger.Address, DisplayName = passenger.DisplayName, Service = ApiBoundary.RideAuditBluetooth });
        var paired = new BluetoothPairingService(bus).Pair(driver, passenger);
        Assert.True(paired.Ok);
        Assert.Equal(ApiBoundary.RideAuditBluetooth, paired.Session!.Transport);

        // Pairing and the platform radio adapters reference no Lyft API.
        var root = Repo.Root();
        var sources = Directory.GetFiles(Path.Combine(root, "src", "RideAudit.Bt"), "*.cs", SearchOption.AllDirectories)
            .Concat(new[]
            {
                Path.Combine(root, "src", "RideAudit.Client.Android", "AndroidDiscoveryBus.cs"),
                Path.Combine(root, "src", "RideAudit.Host.Windows", "WindowsBleDiscoveryBus.cs"),
            })
            .Where(path => !path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                && !path.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            .ToList();
        Assert.Equal(3, sources.Count(path => path.EndsWith("DualPhone.cs", StringComparison.Ordinal)
            || path.EndsWith("AndroidDiscoveryBus.cs", StringComparison.Ordinal)
            || path.EndsWith("WindowsBleDiscoveryBus.cs", StringComparison.Ordinal)));
        foreach (var path in sources)
            Assert.DoesNotContain("lyft", File.ReadAllText(path), StringComparison.OrdinalIgnoreCase);

        Assert.Contains("ApiBoundary.RideAuditBluetoothService", File.ReadAllText(sources.Single(path => path.EndsWith("AndroidDiscoveryBus.cs", StringComparison.Ordinal))), StringComparison.Ordinal);
        Assert.Contains("ApiBoundary.RideAuditBluetoothService", File.ReadAllText(sources.Single(path => path.EndsWith("WindowsBleDiscoveryBus.cs", StringComparison.Ordinal))), StringComparison.Ordinal);

        // The Windows watcher can fall back to an unfiltered scan on unpackaged hosts, so each
        // advertisement must carry the RideAudit service UUID before it is surfaced.
        Assert.True(ApiBoundary.AdvertisesRideAuditService(new[] { Guid.NewGuid(), ApiBoundary.RideAuditBluetoothService }));
        Assert.False(ApiBoundary.AdvertisesRideAuditService(new[] { Guid.NewGuid() }));
        Assert.False(ApiBoundary.AdvertisesRideAuditService(Array.Empty<Guid>()));
        Assert.False(ApiBoundary.AdvertisesRideAuditService(null));
        var windows = File.ReadAllText(sources.Single(path => path.EndsWith("WindowsBleDiscoveryBus.cs", StringComparison.Ordinal)));
        var received = windows.IndexOf("watcher.Received +=", StringComparison.Ordinal);
        Assert.True(received >= 0, "Windows scan handler not found");
        var handler = windows.Substring(received, Math.Min(400, windows.Length - received));
        Assert.Contains("if (!ApiBoundary.AdvertisesRideAuditService(args.Advertisement.ServiceUuids))", handler, StringComparison.Ordinal);
    }
}
