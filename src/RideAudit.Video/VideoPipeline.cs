// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Text;
using RideAudit.Bt;
using RideAudit.Client.Core;
using RideAudit.Contracts;

namespace RideAudit.Video;

public sealed record CameraMetadata(string CameraId, string Facing, int Width, int Height);

public sealed record SourceStream(
    string StreamId,
    string DeviceId,
    string AttestationReference,
    CameraMetadata Camera,
    IReadOnlyList<TimeSpan> FrameTimestamps,
    byte[] Payload);

public sealed record CameraCaptureRequest(
    string StreamId,
    string DeviceId,
    string AttestationReference,
    string Facing);

public interface ICameraSource
{
    string SourceKind { get; }

    bool CameraAvailable { get; }

    SourceStream Capture(CameraCaptureRequest request);
}

/// <summary>
/// Production default when no platform camera adapter is injected. Never returns fixture frames.
/// </summary>
public sealed class UnavailableCameraSource : ICameraSource
{
    public string SourceKind => "unavailable";

    public bool CameraAvailable => false;

    public SourceStream Capture(CameraCaptureRequest request) =>
        throw new RideAuditFailClosedException(
            ErrorCodes.CameraUnavailable,
            "FR-RIDE-041",
            "Camera hardware is unavailable. Capture refused.");
}

/// <summary>
/// Test double. Not a live camera. Payload is caller-supplied fixture bytes.
/// </summary>
public sealed class FixtureCameraSource : ICameraSource
{
    private readonly SourceStream _stream;

    public FixtureCameraSource(SourceStream stream) => _stream = stream;

    public string SourceKind => "fixture";

    public bool CameraAvailable => true;

    public SourceStream Capture(CameraCaptureRequest request) => _stream;
}

public static class CaptureMedia
{
    public static SourceStream Require(ICameraSource source, CameraCaptureRequest request)
    {
        if (!source.CameraAvailable)
        {
            throw new RideAuditFailClosedException(
                ErrorCodes.CameraUnavailable,
                "FR-RIDE-041",
                "Camera hardware is unavailable. Capture refused.");
        }

        return source.Capture(request);
    }
}

public sealed record TelematicsSample(
    TimeSpan SessionTime,
    double AccelX,
    double AccelY,
    double AccelZ,
    double SpeedMps,
    double? GpsLatitude,
    double? GpsLongitude,
    double? Obd2SpeedKph);

public sealed record SpiderPoint(TimeSpan SessionTime, double AccelX, double AccelY, double AccelZ, double SpeedMps);

public sealed record OverlayManifest(
    string Version,
    string TimelineManifestVersion,
    IReadOnlyList<SpiderPoint> Points);

public sealed record VideoPerformanceMetrics(
    double CpuPercent,
    long MemoryBytes,
    double? BatteryPercent,
    double? ThermalCelsius,
    TimeSpan EncodingLatency,
    TimeSpan ClockSyncError,
    int FrameDrops,
    long IngressBytes,
    bool BatteryMeasured,
    bool ThermalMeasured);

public sealed class PerformanceThresholds
{
    public TimeSpan MaxClockSyncError { get; init; } = TimeSpan.FromMilliseconds(50);
    public double MaxDroppedFrameRatio { get; init; } = 0.05;
    public TimeSpan MaxEncodingLatency { get; init; } = TimeSpan.FromMilliseconds(500);
}

public sealed class PerformanceDecision
{
    public required bool Admitted { get; init; }
    public required string Detail { get; init; }
    public required VideoPerformanceMetrics Metrics { get; init; }
}

public static class PerformanceGate
{
    public static PerformanceDecision Evaluate(VideoPerformanceMetrics metrics, int frameCount, PerformanceThresholds thresholds)
    {
        if (!metrics.BatteryMeasured || !metrics.ThermalMeasured)
        {
            return new PerformanceDecision
            {
                Admitted = false,
                Detail = "Battery or thermal metrics were not measured.",
                Metrics = metrics,
            };
        }

        if (metrics.ClockSyncError > thresholds.MaxClockSyncError)
        {
            return new PerformanceDecision
            {
                Admitted = false,
                Detail = "Clock sync error exceeds threshold.",
                Metrics = metrics,
            };
        }

        var ratio = frameCount == 0 ? 1 : (double)metrics.FrameDrops / frameCount;
        if (ratio > thresholds.MaxDroppedFrameRatio)
        {
            return new PerformanceDecision
            {
                Admitted = false,
                Detail = "Dropped-frame ratio exceeds threshold.",
                Metrics = metrics,
            };
        }

        if (metrics.EncodingLatency > thresholds.MaxEncodingLatency)
        {
            return new PerformanceDecision
            {
                Admitted = false,
                Detail = "Encoding latency exceeds threshold.",
                Metrics = metrics,
            };
        }

        return new PerformanceDecision
        {
            Admitted = true,
            Detail = "Performance thresholds met.",
            Metrics = metrics,
        };
    }
}

public sealed class VideoQuotaPolicy
{
    public long MaxCompositeBytes { get; init; } = 8 * 1024 * 1024;
    public long MaxRawStreamBytes { get; init; } = 4 * 1024 * 1024;
    public int ChunkSizeBytes { get; init; } = 64 * 1024;
    public long SessionCompositeBudget { get; init; } = 8 * 1024 * 1024;
    public long SessionRawBudget { get; init; } = 4 * 1024 * 1024;

    public static VideoQuotaPolicy Default { get; } = new();
}

public sealed class QuotaDecision
{
    public required bool Ok { get; init; }
    public required string Detail { get; init; }
}

public static class VideoQuota
{
    public static QuotaDecision CheckComposite(long bytes, VideoQuotaPolicy policy)
    {
        if (bytes > policy.MaxCompositeBytes || bytes > policy.SessionCompositeBudget)
        {
            return new QuotaDecision { Ok = false, Detail = "Composite exceeds per-session quota." };
        }

        return new QuotaDecision { Ok = true, Detail = "Composite within quota." };
    }

    public static QuotaDecision CheckRaw(long bytes, bool consent, VideoQuotaPolicy policy)
    {
        if (!consent)
        {
            return new QuotaDecision { Ok = false, Detail = "Raw stream sealing requires driver consent." };
        }

        if (bytes > policy.MaxRawStreamBytes || bytes > policy.SessionRawBudget)
        {
            return new QuotaDecision { Ok = false, Detail = "Raw stream exceeds its separate budget." };
        }

        return new QuotaDecision { Ok = true, Detail = "Raw stream within separate budget." };
    }
}

public sealed record SealedChunk(int Index, int Total, string Sha256Hex, byte[] Payload);

public static class ChunkedSealedTransfer
{
    public static IReadOnlyList<SealedChunk> Chunk(byte[] ciphertext, int chunkSize)
    {
        if (chunkSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(chunkSize));
        }

        var total = Math.Max(1, (int)Math.Ceiling(ciphertext.Length / (double)chunkSize));
        var chunks = new List<SealedChunk>(total);
        for (var i = 0; i < total; i++)
        {
            var offset = i * chunkSize;
            var length = Math.Min(chunkSize, ciphertext.Length - offset);
            var payload = ciphertext.AsSpan(offset, length).ToArray();
            chunks.Add(new SealedChunk(i, total, Hashes.Sha256Hex(payload), payload));
        }

        if (ciphertext.Length == 0)
        {
            return [new SealedChunk(0, 1, Hashes.Sha256Hex(ReadOnlySpan<byte>.Empty), [])];
        }

        return chunks;
    }

    public static byte[] Reassemble(IReadOnlyList<SealedChunk> chunks)
    {
        if (chunks.Count == 0)
        {
            throw new RideAuditFailClosedException("CHUNK_OUT_OF_ORDER", "FR-RIDE-219", "No chunks.");
        }

        var total = chunks[0].Total;
        if (chunks.Any(chunk => chunk.Total != total))
        {
            throw new RideAuditFailClosedException("CHUNK_OUT_OF_ORDER", "FR-RIDE-219", "Chunk total mismatch.");
        }

        var ordered = chunks.OrderBy(chunk => chunk.Index).ToList();
        if (ordered.Count != total || ordered.Select(chunk => chunk.Index).Distinct().Count() != total)
        {
            throw new RideAuditFailClosedException("CHUNK_OUT_OF_ORDER", "FR-RIDE-219", "Missing or duplicate chunk index.");
        }

        for (var i = 0; i < ordered.Count; i++)
        {
            if (ordered[i].Index != i)
            {
                throw new RideAuditFailClosedException("CHUNK_OUT_OF_ORDER", "FR-RIDE-219", "Chunk gap.");
            }

            if (!string.Equals(ordered[i].Sha256Hex, Hashes.Sha256Hex(ordered[i].Payload), StringComparison.OrdinalIgnoreCase))
            {
                throw new RideAuditFailClosedException("RECEIPT_INVALID", "FR-RIDE-219", "Chunk integrity check failed.");
            }
        }

        return ordered.SelectMany(chunk => chunk.Payload).ToArray();
    }
}

public sealed class CompositePackage
{
    public required string CompositeId { get; init; }
    public required byte[] CanonicalBytes { get; init; }
    public required OverlayManifest Overlay { get; init; }
    public required SyncClockOffset Sync { get; init; }
    public required IReadOnlyList<SourceStream> Sources { get; init; }
    public required VideoPerformanceMetrics Metrics { get; init; }
    public required string Codec { get; init; }
    public required string Compression { get; init; }
    public bool ProducedOnDevice { get; init; } = true;
    public bool ServerPlaintextComposite { get; init; }
}

public interface IDeviceProbe
{
    VideoPerformanceMetrics Measure(TimeSpan encodingLatency, TimeSpan clockSyncError, int frameDrops, long ingressBytes);
}

public sealed class ExplicitDeviceProbe : IDeviceProbe
{
    private readonly double _cpu;
    private readonly long _memory;
    private readonly double? _battery;
    private readonly double? _thermal;

    public ExplicitDeviceProbe(double cpuPercent, long memoryBytes, double? batteryPercent, double? thermalCelsius)
    {
        _cpu = cpuPercent;
        _memory = memoryBytes;
        _battery = batteryPercent;
        _thermal = thermalCelsius;
    }

    public VideoPerformanceMetrics Measure(TimeSpan encodingLatency, TimeSpan clockSyncError, int frameDrops, long ingressBytes) =>
        new(
            _cpu,
            _memory,
            _battery,
            _thermal,
            encodingLatency,
            clockSyncError,
            frameDrops,
            ingressBytes,
            BatteryMeasured: _battery.HasValue,
            ThermalMeasured: _thermal.HasValue);
}

/// <summary>
/// Passenger compositor. Produces a canonical on-device source-payload container.
/// This is not a production H.264 encoder and is not a Bluetooth media transport.
/// </summary>
public sealed class PassengerCompositor
{
    public const string CodecId = CompositeSourceContainer.CodecId;
    public const string CompressionId = "none-canonical";
    public const string OverlayVersion = "spider-graph-overlay-v1";
    public const string TimelineVersion = "timeline-manifest-v1";

    private readonly IDeviceProbe _probe;

    public PassengerCompositor(IDeviceProbe probe) => _probe = probe;

    public CompositePackage Compose(
        string compositeId,
        SyncClockOffset sync,
        SourceStream driver,
        SourceStream passenger,
        IReadOnlyList<TelematicsSample> samples)
    {
        if (driver.FrameTimestamps.Count == 0 || passenger.FrameTimestamps.Count == 0)
        {
            throw new RideAuditFailClosedException(
                "VALIDATION_FAILED",
                "FR-RIDE-041",
                "Both phones must supply frames.");
        }

        var started = DateTimeOffset.UtcNow;
        var frameCount = Math.Min(driver.FrameTimestamps.Count, passenger.FrameTimestamps.Count);
        var points = new List<SpiderPoint>(frameCount);
        for (var i = 0; i < frameCount; i++)
        {
            var stamp = driver.FrameTimestamps[i];
            var sample = samples.LastOrDefault(item => item.SessionTime <= stamp) ?? samples.FirstOrDefault();
            points.Add(new SpiderPoint(
                stamp,
                sample?.AccelX ?? 0,
                sample?.AccelY ?? 0,
                sample?.AccelZ ?? 0,
                sample?.SpeedMps ?? 0));
        }

        var manifest = new OverlayManifest(OverlayVersion, TimelineVersion, points);
        var canonical = CompositeSourceContainer.Encode(compositeId, driver, passenger, sync, manifest);
        var latency = DateTimeOffset.UtcNow - started;
        if (latency < TimeSpan.Zero)
        {
            latency = TimeSpan.Zero;
        }

        var metrics = _probe.Measure(latency, sync.Offset.Duration(), sync.UnsyncedIntervals.Count, canonical.Length);
        return new CompositePackage
        {
            CompositeId = compositeId,
            CanonicalBytes = canonical,
            Overlay = manifest,
            Sync = sync,
            Sources = [driver, passenger],
            Metrics = metrics,
            Codec = CodecId,
            Compression = CompressionId,
            ProducedOnDevice = true,
            ServerPlaintextComposite = false,
        };
    }

}

public sealed record ParsedCompositeContainer(
    string CompositeId,
    string HeaderText,
    string DriverStreamId,
    string PassengerStreamId,
    byte[] DriverPayload,
    byte[] PassengerPayload);

/// <summary>
/// Binary container that includes both source payloads and their SHA-256 bindings.
/// Not H.264, not a Play Store asset, and not a dual-phone Bluetooth media path.
/// </summary>
public static class CompositeSourceContainer
{
    public const string Magic = "RIDEAUDIT-COMPOSITE-v2\n";
    public const string CodecId = "rideaudit-composite-source-container-v2";

    public static byte[] Encode(
        string compositeId,
        SourceStream driver,
        SourceStream passenger,
        SyncClockOffset sync,
        OverlayManifest manifest)
    {
        var header = new StringBuilder();
        header.AppendLine("id=" + compositeId);
        header.AppendLine("codec=" + CodecId);
        header.AppendLine("driver=" + driver.StreamId + " device=" + driver.DeviceId + " attest=" + driver.AttestationReference);
        header.AppendLine("passenger=" + passenger.StreamId + " device=" + passenger.DeviceId + " attest=" + passenger.AttestationReference);
        header.AppendLine("driverHash=" + Hashes.Sha256Hex(driver.Payload));
        header.AppendLine("passengerHash=" + Hashes.Sha256Hex(passenger.Payload));
        header.AppendLine("camera-driver=" + driver.Camera.CameraId + " " + driver.Camera.Width + "x" + driver.Camera.Height);
        header.AppendLine("camera-passenger=" + passenger.Camera.CameraId + " " + passenger.Camera.Width + "x" + passenger.Camera.Height);
        header.AppendLine("syncOffsetMs=" + sync.Offset.TotalMilliseconds.ToString("0.###"));
        header.AppendLine("driftMs=" + sync.Drift.TotalMilliseconds.ToString("0.###"));
        header.AppendLine("uncertaintyMs=" + sync.Uncertainty.TotalMilliseconds.ToString("0.###"));
        header.AppendLine("overlay=" + manifest.Version);
        header.AppendLine("timeline=" + manifest.TimelineManifestVersion);
        header.AppendLine("media=source-payload-container;not-h264;not-bt-transport");
        foreach (var point in manifest.Points)
        {
            header.AppendLine("spider " + point.SessionTime.TotalMilliseconds.ToString("0.###")
                + " " + point.AccelX + " " + point.AccelY + " " + point.AccelZ + " " + point.SpeedMps);
        }

        foreach (var gap in sync.UnsyncedIntervals)
        {
            header.AppendLine("unsynced " + gap.Start.TotalMilliseconds + " " + gap.End.TotalMilliseconds + " " + gap.Reason);
        }

        var headerBytes = Encoding.UTF8.GetBytes(header.ToString());
        using var buffer = new MemoryStream();
        var magic = Encoding.ASCII.GetBytes(Magic);
        buffer.Write(magic);
        WriteInt(buffer, headerBytes.Length);
        buffer.Write(headerBytes);
        WriteInt(buffer, driver.Payload.Length);
        buffer.Write(driver.Payload);
        WriteInt(buffer, passenger.Payload.Length);
        buffer.Write(passenger.Payload);
        return buffer.ToArray();
    }

    public static bool TryParse(ReadOnlySpan<byte> data, out ParsedCompositeContainer parsed)
    {
        parsed = null!;
        var magic = Encoding.ASCII.GetBytes(Magic);
        if (data.Length < magic.Length + 12 || !data[..magic.Length].SequenceEqual(magic))
            return false;

        var cursor = magic.Length;
        if (!TryReadInt(data, ref cursor, out var headerLen) || headerLen < 1 || cursor + headerLen > data.Length)
            return false;
        var headerText = Encoding.UTF8.GetString(data.Slice(cursor, headerLen));
        cursor += headerLen;
        if (!TryReadInt(data, ref cursor, out var driverLen) || driverLen < 0 || cursor + driverLen > data.Length)
            return false;
        var driver = data.Slice(cursor, driverLen).ToArray();
        cursor += driverLen;
        if (!TryReadInt(data, ref cursor, out var passengerLen) || passengerLen < 0 || cursor + passengerLen > data.Length)
            return false;
        var passenger = data.Slice(cursor, passengerLen).ToArray();
        cursor += passengerLen;
        if (cursor != data.Length)
            return false;

        parsed = new ParsedCompositeContainer(
            ReadField(headerText, "id="),
            headerText,
            ReadToken(headerText, "driver="),
            ReadToken(headerText, "passenger="),
            driver,
            passenger);
        return parsed.CompositeId.Length > 0 && parsed.DriverStreamId.Length > 0 && parsed.PassengerStreamId.Length > 0;
    }

    public static (bool Ok, string Detail) VerifyBindings(
        ReadOnlySpan<byte> plaintext,
        IReadOnlyList<string> sourceIds,
        IReadOnlyList<string> sourceHashes)
    {
        if (!TryParse(plaintext, out var parsed))
        {
            return (false, "Composite plaintext is not a source-payload container. Court-ready media is blocked.");
        }

        if (sourceIds.Count < 2 || sourceHashes.Count < 2)
            return (false, "Receipt source bindings are missing.");
        if (!string.Equals(parsed.DriverStreamId, sourceIds[0], StringComparison.Ordinal)
            || !string.Equals(parsed.PassengerStreamId, sourceIds[1], StringComparison.Ordinal))
        {
            return (false, "Embedded stream ids do not match the receipt.");
        }

        var driverHash = Hashes.Sha256Hex(parsed.DriverPayload);
        var passengerHash = Hashes.Sha256Hex(parsed.PassengerPayload);
        if (!string.Equals(driverHash, sourceHashes[0], StringComparison.OrdinalIgnoreCase)
            || !string.Equals(passengerHash, sourceHashes[1], StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Embedded source payloads do not match receipt source hashes.");
        }

        return (true, "Source payloads are bound to the receipt hashes.");
    }

    private static void WriteInt(Stream stream, int value)
    {
        Span<byte> bytes = stackalloc byte[4];
        BitConverter.TryWriteBytes(bytes, value);
        stream.Write(bytes);
    }

    private static bool TryReadInt(ReadOnlySpan<byte> data, ref int cursor, out int value)
    {
        value = 0;
        if (cursor + 4 > data.Length)
            return false;
        value = BitConverter.ToInt32(data.Slice(cursor, 4));
        cursor += 4;
        return true;
    }

    private static string ReadField(string header, string prefix)
    {
        foreach (var line in header.Split('\n'))
        {
            var trimmed = line.TrimEnd('\r');
            if (trimmed.StartsWith(prefix, StringComparison.Ordinal))
                return trimmed[prefix.Length..].Trim();
        }

        return "";
    }

    private static string ReadToken(string header, string prefix)
    {
        var line = ReadField(header, prefix);
        var space = line.IndexOf(' ');
        return space < 0 ? line : line[..space];
    }
}
