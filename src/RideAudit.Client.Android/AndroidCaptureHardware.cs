// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using Android;
using Android.Bluetooth;
using Android.Content;
using Android.Hardware.Camera2;
using RideAudit.Client.Core;
using RideAudit.Contracts;
using Permission = Android.Content.PM.Permission;

namespace RideAudit.Client.Android;

/// <summary>
/// Android radio and camera probes. Missing adapter, radio-off, or missing camera fail-closes.
/// This does not invent a paired peer or a camera frame.
/// </summary>
public static class AndroidCaptureHardware
{
    public static void EnsureBluetoothOrThrow(Context context)
    {
        var manager = context.GetSystemService(Context.BluetoothService) as BluetoothManager;
        var adapter = manager?.Adapter;
        if (adapter is null)
        {
            throw new RideAuditFailClosedException(
                ErrorCodes.BluetoothDisabled,
                "FR-RIDE-053",
                "Android BluetoothManager.Adapter is null.");
        }

        if (adapter.State != State.On)
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

    /// <summary>
    /// Runtime permission probe. Missing CAMERA/BT permissions fail-closed.
    /// This does not invent a grant and does not start a Play/HSM path.
    /// </summary>
    public static IReadOnlyList<string> MissingRuntimePermissions(Context context)
    {
        var required = new List<string> { Manifest.Permission.Camera };
        if (OperatingSystem.IsAndroidVersionAtLeast(31))
        {
            required.Add(Manifest.Permission.BluetoothConnect);
            required.Add(Manifest.Permission.BluetoothScan);
            required.Add(Manifest.Permission.BluetoothAdvertise);
        }
        else
        {
            required.Add(Manifest.Permission.Bluetooth);
            required.Add(Manifest.Permission.BluetoothAdmin);
        }

        return required
            .Where(permission => context.CheckSelfPermission(permission) != Permission.Granted)
            .ToArray();
    }
}
