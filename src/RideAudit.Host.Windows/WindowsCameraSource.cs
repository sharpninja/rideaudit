// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using RideAudit.Client.Core;
using RideAudit.Contracts;
using RideAudit.Video;
using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;

namespace RideAudit.Host.Windows;

/// <summary>
/// WinRT MediaCapture seam. CameraAvailable is true only after a real device enumeration.
/// Capture never returns fixture bytes. Permission or device failure fail-closes.
/// </summary>
public sealed class WindowsCameraSource : ICameraSource
{
    private WindowsCameraSource(bool cameraAvailable, int deviceCount, string probeDetail)
    {
        CameraAvailable = cameraAvailable;
        DeviceCount = deviceCount;
        ProbeDetail = probeDetail;
    }

    public string SourceKind => "windows-media-capture";

    public bool CameraAvailable { get; }

    public int DeviceCount { get; }

    public string ProbeDetail { get; }

    public static WindowsCameraSource Create()
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
            return new(false, 0, "Windows 10 1809 or later is required for WinRT camera.");

        try
        {
            var devices = DeviceInformation.FindAllAsync(DeviceClass.VideoCapture).AsTask().GetAwaiter().GetResult();
            var count = (int)devices.Count;
            if (count == 0)
                return new(false, 0, "DeviceInformation found no VideoCapture devices.");
            return new(true, count, "video-capture-devices=" + count);
        }
        catch (Exception ex)
        {
            return new(false, 0, ex.GetType().Name + ": " + ex.Message);
        }
    }

    public SourceStream Capture(CameraCaptureRequest request)
    {
        if (!CameraAvailable)
        {
            throw new RideAuditFailClosedException(
                ErrorCodes.CameraUnavailable,
                "FR-RIDE-041",
                "Windows camera is unavailable: " + ProbeDetail);
        }

        var capture = new MediaCapture();
        try
        {
            capture.InitializeAsync(new MediaCaptureInitializationSettings
            {
                StreamingCaptureMode = StreamingCaptureMode.Video
            }).AsTask().GetAwaiter().GetResult();

            var encoding = ImageEncodingProperties.CreateJpeg();
            var stream = new InMemoryRandomAccessStream();
            capture.CapturePhotoToStreamAsync(encoding, stream).AsTask().GetAwaiter().GetResult();
            stream.Seek(0);
            var bytes = new byte[stream.Size];
            var reader = new DataReader(stream);
            reader.LoadAsync((uint)stream.Size).AsTask().GetAwaiter().GetResult();
            reader.ReadBytes(bytes);
            if (bytes.Length == 0)
            {
                throw new RideAuditFailClosedException(
                    ErrorCodes.CameraUnavailable,
                    "FR-RIDE-041",
                    "Windows camera returned an empty frame.");
            }

            return new SourceStream(
                request.StreamId,
                request.DeviceId,
                request.AttestationReference,
                new CameraMetadata(request.DeviceId + "-cam", request.Facing, 0, 0),
                [TimeSpan.Zero],
                bytes);
        }
        catch (RideAuditFailClosedException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new RideAuditFailClosedException(
                ErrorCodes.CameraUnavailable,
                "FR-RIDE-041",
                "Windows camera capture failed: " + ex.GetType().Name + ".");
        }
        finally
        {
            capture.Dispose();
        }
    }
}
