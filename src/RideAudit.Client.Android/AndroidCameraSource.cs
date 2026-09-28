// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using Android.Content;
using RideAudit.Client.Core;
using RideAudit.Contracts;
using RideAudit.Video;

namespace RideAudit.Client.Android;

/// <summary>
/// Android camera probe. CameraAvailable is true only when CameraManager reports a device.
/// Capture refuses fixture frames until a Camera2 encode pipeline exists.
/// </summary>
public sealed class AndroidCameraSource : ICameraSource
{
    private AndroidCameraSource(bool available, string detail)
    {
        CameraAvailable = available;
        ProbeDetail = detail;
    }

    public string SourceKind => "android-camera2";

    public bool CameraAvailable { get; }

    public string ProbeDetail { get; }

    public static AndroidCameraSource Create(Context context)
    {
        try
        {
            AndroidCaptureHardware.EnsureCameraOrThrow(context);
            return new(true, "CameraManager reported at least one camera.");
        }
        catch (RideAuditFailClosedException ex)
        {
            return new(false, ex.Message);
        }
    }

    public SourceStream Capture(CameraCaptureRequest request)
    {
        if (!CameraAvailable)
        {
            throw new RideAuditFailClosedException(
                ErrorCodes.CameraUnavailable,
                "FR-RIDE-041",
                "Android camera is unavailable: " + ProbeDetail);
        }

        throw new RideAuditFailClosedException(
            ErrorCodes.CameraUnavailable,
            "FR-RIDE-041",
            "Android Camera2 frame pipeline is not implemented. Refusing fixture frames.");
    }
}
