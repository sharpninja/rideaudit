// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using Android.Bluetooth;
using Android.Content;
using Android.Hardware.Camera2;
using RideAudit.Client.Core;
using RideAudit.Contracts;

namespace RideAudit.Client.Android;

/// <summary>
/// Android radio and camera probes. Missing adapter, radio-off, or missing camera fail-closes.
/// This does not invent a paired peer or a camera frame.
/// </summary>
public static class AndroidCaptureHardware
{
    public static void EnsureBluetoothOrThrow(Context context)
    {
        var adapter = BluetoothAdapter.DefaultAdapter;
        if (adapter is null)
        {
            throw new RideAuditFailClosedException(
                ErrorCodes.BluetoothDisabled,
                "FR-RIDE-053",
                "Android BluetoothAdapter is null.");
        }

        if (!adapter.IsEnabled)
        {
            throw new RideAuditFailClosedException(
                ErrorCodes.BluetoothDisabled,
                "FR-RIDE-053",
                "Android Bluetooth radio is off or permission was denied.");
        }
    }

    public static void EnsureCameraOrThrow(Context context)
    {
        var manager = (CameraManager?)context.GetSystemService(Context.CameraService);
        if (manager is null || manager.GetCameraIdList().Length == 0)
        {
            throw new RideAuditFailClosedException(
                ErrorCodes.CameraUnavailable,
                "FR-RIDE-041",
                "Android CameraManager reported no cameras.");
        }
    }
}
