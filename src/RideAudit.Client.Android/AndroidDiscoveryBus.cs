// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;
using RideAudit.Bt;
using RideAudit.Client.Core;
using RideAudit.Contracts;

namespace RideAudit.Client.Android;

/// <summary>
/// Android BLE advertise/scan. RadioAvailable is true only after a real adapter probe.
/// Advertise and Scan never invent peers. Missing adapter, radio-off, or BLE API failure fail-closes.
/// </summary>
public sealed class AndroidDiscoveryBus : IDiscoveryBus
{
    private readonly Context _context;
    private readonly List<Advertisement> _seen = [];

    private AndroidDiscoveryBus(Context context, bool radioAvailable, string probeDetail)
    {
        _context = context;
        RadioAvailable = radioAvailable;
        ProbeDetail = probeDetail;
    }

    public string TransportKind => "android-ble";

    public bool RadioAvailable { get; }

    public string ProbeDetail { get; }

    public static AndroidDiscoveryBus Create(Context context)
    {
        try
        {
            AndroidCaptureHardware.EnsureBluetoothOrThrow(context);
            return new(context, true, "BluetoothManager.Adapter is on.");
        }
        catch (RideAuditFailClosedException ex)
        {
            return new(context, false, ex.Message);
        }
    }

    public void Advertise(Advertisement advertisement)
    {
        EnsureRadio();
        var adapter = RequireAdapter();
        var advertiser = adapter.BluetoothLeAdvertiser
            ?? throw new RideAuditFailClosedException(
                ErrorCodes.BluetoothDisabled,
                "FR-RIDE-053",
                "Android BluetoothLeAdvertiser is null.");
        try
        {
            var settings = new AdvertiseSettings.Builder()!
                .SetAdvertiseMode(AdvertiseMode.LowLatency)!
                .SetConnectable(false)!
                .SetTimeout(0)!
                .Build()
                ?? throw new RideAuditFailClosedException(
                    ErrorCodes.BluetoothDisabled,
                    "FR-RIDE-053",
                    "Android AdvertiseSettings.Builder returned null.");
            var data = new AdvertiseData.Builder()!
                .SetIncludeDeviceName(true)!
                .AddServiceUuid(ParcelUuid.FromString(ApiBoundary.RideAuditBluetoothService.ToString()))!
                .Build()
                ?? throw new RideAuditFailClosedException(
                    ErrorCodes.BluetoothDisabled,
                    "FR-RIDE-053",
                    "Android AdvertiseData.Builder returned null.");
            advertiser.StartAdvertising(settings, data, new SilentAdvertiseCallback());
        }
        catch (Exception ex) when (ex is not RideAuditFailClosedException)
        {
            throw new RideAuditFailClosedException(
                ErrorCodes.BluetoothDisabled,
                "FR-RIDE-053",
                "Android BLE advertise failed: " + ex.GetType().Name + ".");
        }
    }

    public IReadOnlyList<Advertisement> Scan(string exceptAddress)
    {
        EnsureRadio();
        _seen.Clear();
        var adapter = RequireAdapter();
        var scanner = adapter.BluetoothLeScanner
            ?? throw new RideAuditFailClosedException(
                ErrorCodes.BluetoothDisabled,
                "FR-RIDE-053",
                "Android BluetoothLeScanner is null.");
        var callback = new CollectingScanCallback(exceptAddress, _seen);
        try
        {
            var filters = new List<ScanFilter>
            {
                new ScanFilter.Builder()!
                    .SetServiceUuid(ParcelUuid.FromString(ApiBoundary.RideAuditBluetoothService.ToString()))!
                    .Build()!
            };
            var settings = new ScanSettings.Builder()!
                .SetScanMode(global::Android.Bluetooth.LE.ScanMode.LowLatency)!
                .Build()
                ?? throw new RideAuditFailClosedException(
                    ErrorCodes.BluetoothDisabled,
                    "FR-RIDE-053",
                    "Android ScanSettings.Builder returned null.");
            scanner.StartScan(filters, settings, callback);
            Thread.Sleep(1500);
            scanner.StopScan(callback);
        }
        catch (Exception ex) when (ex is not RideAuditFailClosedException)
        {
            throw new RideAuditFailClosedException(
                ErrorCodes.BluetoothDisabled,
                "FR-RIDE-053",
                "Android BLE scan failed: " + ex.GetType().Name + ".");
        }

        lock (_seen)
            return _seen.ToList();
    }

    public void Clear()
    {
        lock (_seen)
            _seen.Clear();
    }

    private BluetoothAdapter RequireAdapter()
    {
        var manager = _context.GetSystemService(Context.BluetoothService) as BluetoothManager;
        return manager?.Adapter
            ?? throw new RideAuditFailClosedException(
                ErrorCodes.BluetoothDisabled,
                "FR-RIDE-053",
                "Android BluetoothManager.Adapter is null.");
    }

    private void EnsureRadio()
    {
        if (!RadioAvailable)
        {
            throw new RideAuditFailClosedException(
                ErrorCodes.BluetoothDisabled,
                "FR-RIDE-053",
                "Android BLE radio is unavailable: " + ProbeDetail);
        }
    }

    private sealed class SilentAdvertiseCallback : AdvertiseCallback
    {
    }

    private sealed class CollectingScanCallback : ScanCallback
    {
        private readonly string _except;
        private readonly List<Advertisement> _seen;

        public CollectingScanCallback(string except, List<Advertisement> seen)
        {
            _except = except;
            _seen = seen;
        }

        public override void OnScanResult(ScanCallbackType callbackType, ScanResult? result)
        {
            var address = result?.Device?.Address;
            if (string.IsNullOrWhiteSpace(address)
                || string.Equals(address, _except, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            lock (_seen)
            {
                if (_seen.Any(item => string.Equals(item.Address, address, StringComparison.OrdinalIgnoreCase)))
                    return;
                _seen.Add(new Advertisement
                {
                    Address = address,
                    DisplayName = result?.Device?.Name ?? address,
                    Service = ApiBoundary.RideAuditBluetooth
                });
            }
        }
    }
}
