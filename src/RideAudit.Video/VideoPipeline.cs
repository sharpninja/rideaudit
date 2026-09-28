// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Text;
using RideAudit.Bt;
using RideAudit.Client.Core;

namespace RideAudit.Video;

public sealed record CameraMetadata(string CameraId, string Facing, int Width, int Height);

public sealed record SourceStream(
    string StreamId,
    string DeviceId,
    string AttestationReference,
    CameraMetadata Camera,
    IReadOnlyList<TimeSpan> FrameTimestamps,
    byte[] Payload);

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
/// Passenger compositor. Produces a canonical on-device composite. This is not a production H.264 encoder.
/// </summary>
public sealed class PassengerCompositor
{
    public const string CodecId = "rideaudit-composite-canonical-v1";
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
        var canonical = CanonicalEncode(compositeId, driver, passenger, sync, manifest);
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

    private static byte[] CanonicalEncode(
        string compositeId,
        SourceStream driver,
        SourceStream passenger,
        SyncClockOffset sync,
        OverlayManifest manifest)
    {
        var builder = new StringBuilder();
        builder.AppendLine("RIDEAUDIT-COMPOSITE-v1");
        builder.AppendLine("id=" + compositeId);
        builder.AppendLine("codec=" + CodecId);
        builder.AppendLine("driver=" + driver.StreamId + " device=" + driver.DeviceId + " attest=" + driver.AttestationReference);
        builder.AppendLine("passenger=" + passenger.StreamId + " device=" + passenger.DeviceId + " attest=" + passenger.AttestationReference);
        builder.AppendLine("camera-driver=" + driver.Camera.CameraId + " " + driver.Camera.Width + "x" + driver.Camera.Height);
        builder.AppendLine("camera-passenger=" + passenger.Camera.CameraId + " " + passenger.Camera.Width + "x" + passenger.Camera.Height);
        builder.AppendLine("syncOffsetMs=" + sync.Offset.TotalMilliseconds.ToString("0.###"));
        builder.AppendLine("driftMs=" + sync.Drift.TotalMilliseconds.ToString("0.###"));
        builder.AppendLine("uncertaintyMs=" + sync.Uncertainty.TotalMilliseconds.ToString("0.###"));
        builder.AppendLine("overlay=" + manifest.Version);
        builder.AppendLine("timeline=" + manifest.TimelineManifestVersion);
        foreach (var point in manifest.Points)
        {
            builder.AppendLine("spider " + point.SessionTime.TotalMilliseconds.ToString("0.###")
                + " " + point.AccelX + " " + point.AccelY + " " + point.AccelZ + " " + point.SpeedMps);
        }

        foreach (var gap in sync.UnsyncedIntervals)
        {
            builder.AppendLine("unsynced " + gap.Start.TotalMilliseconds + " " + gap.End.TotalMilliseconds + " " + gap.Reason);
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }
}
