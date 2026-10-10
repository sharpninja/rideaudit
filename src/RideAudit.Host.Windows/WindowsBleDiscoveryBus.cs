// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using RideAudit.Bt;
using RideAudit.Client.Core;
using RideAudit.Contracts;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;

namespace RideAudit.Host.Windows;

/// <summary>
/// WinRT BLE advertisement/scan. RadioAvailable is true only after a real adapter probe.
/// Advertise and Scan never invent peers. Missing adapter or permission fail-closes.
/// </summary>
public sealed class WindowsBleDiscoveryBus : IDiscoveryBus, IDisposable
{
    private readonly List<Advertisement> _seen = [];
    private BluetoothLEAdvertisementPublisher? _publisher;
    private BluetoothLEAdvertisementWatcher? _watcher;

    private WindowsBleDiscoveryBus(bool radioAvailable, string probeDetail)
    {
        RadioAvailable = radioAvailable;
        ProbeDetail = probeDetail;
    }

    public string TransportKind => "windows-ble";

    public bool RadioAvailable { get; }

    public string ProbeDetail { get; }

    public static WindowsBleDiscoveryBus Create()
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
            return new(false, "Windows 10 1809 or later is required for WinRT BLE.");

        try
        {
            var adapter = BluetoothAdapter.GetDefaultAsync().AsTask().GetAwaiter().GetResult();
            if (adapter is null)
                return new(false, "BluetoothAdapter.GetDefaultAsync returned null.");
            if (!adapter.IsLowEnergySupported)
                return new(false, "Default adapter does not report BLE support.");
            return new(true, "adapter-id=" + adapter.BluetoothAddress.ToString("X", System.Globalization.CultureInfo.InvariantCulture));
        }
        catch (Exception ex)
        {
            return new(false, ex.GetType().Name + ": " + ex.Message);
        }
    }

    public void Advertise(Advertisement advertisement)
    {
        EnsureRadio();
        try
        {
            _publisher?.Stop();
            var publisher = new BluetoothLEAdvertisementPublisher();
            publisher.Advertisement.LocalName = Truncate(advertisement.DisplayName, 8);
            try
            {
                publisher.Advertisement.ServiceUuids.Add(ApiBoundary.RideAuditBluetoothService);
            }
            catch (ArgumentException)
            {
                // Unpackaged hosts may reject a custom service UUID. Scanners accept only advertisements
                // that carry it, so a name-only advertise would never be discovered: fail closed instead.
                throw new RideAuditFailClosedException(
                    ErrorCodes.BluetoothDisabled,
                    "FR-RIDE-053",
                    "Windows BLE advertise cannot carry the RideAudit service UUID on this host.");
            }
            publisher.Start();
            _publisher = publisher;
        }
        catch (Exception ex) when (ex is not RideAuditFailClosedException)
        {
            throw new RideAuditFailClosedException(
                ErrorCodes.BluetoothDisabled,
                "FR-RIDE-053",
                "Windows BLE advertise failed: " + ex.GetType().Name + ".");
        }
    }

    public IReadOnlyList<Advertisement> Scan(string exceptAddress)
    {
        EnsureRadio();
        _seen.Clear();
        var watcher = new BluetoothLEAdvertisementWatcher
        {
            ScanningMode = BluetoothLEScanningMode.Active
        };
        try
        {
            watcher.AdvertisementFilter.Advertisement.ServiceUuids.Add(ApiBoundary.RideAuditBluetoothService);
        }
        catch (ArgumentException)
        {
            // Unpackaged test hosts may reject a custom service UUID. The watcher then runs unfiltered,
            // so every advertisement is checked for the RideAudit service UUID below (fail closed).
        }

        watcher.Received += (_, args) =>
        {
            if (!ApiBoundary.AdvertisesRideAuditService(args.Advertisement.ServiceUuids))
                return;
            var name = args.Advertisement.LocalName;
            if (string.IsNullOrWhiteSpace(name))
                return;
            var address = args.BluetoothAddress.ToString("X12", System.Globalization.CultureInfo.InvariantCulture);
            if (string.Equals(address, exceptAddress, StringComparison.OrdinalIgnoreCase))
                return;
            lock (_seen)
            {
                if (_seen.Any(item => string.Equals(item.Address, address, StringComparison.OrdinalIgnoreCase)))
                    return;
                _seen.Add(new Advertisement
                {
                    Address = address,
                    DisplayName = name,
                    Service = ApiBoundary.RideAuditBluetooth
                });
            }
        };

        try
        {
            watcher.Start();
            _watcher = watcher;
            Thread.Sleep(1500);
            watcher.Stop();
        }
        catch (Exception ex)
        {
            throw new RideAuditFailClosedException(
                ErrorCodes.BluetoothDisabled,
                "FR-RIDE-053",
                "Windows BLE scan failed: " + ex.GetType().Name + ".");
        }

        lock (_seen)
            return _seen.ToList();
    }

    public void Clear()
    {
        lock (_seen)
            _seen.Clear();
    }

    public void Dispose()
    {
        try
        {
            _publisher?.Stop();
        }
        catch (Exception)
        {
        }

        try
        {
            _watcher?.Stop();
        }
        catch (Exception)
        {
        }
    }

    private void EnsureRadio()
    {
        if (!RadioAvailable)
        {
            throw new RideAuditFailClosedException(
                ErrorCodes.BluetoothDisabled,
                "FR-RIDE-053",
                "Windows BLE radio is unavailable: " + ProbeDetail);
        }
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max];
}
