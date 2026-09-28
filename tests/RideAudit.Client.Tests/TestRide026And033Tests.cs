// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using RideAudit.Client.Core;
using RideAudit.Client.Tests.Support;
using RideAudit.Seal;
using RideAudit.Video;
using Xunit;

namespace RideAudit.Client.Tests;

[Trait("Partition", "TEST-RIDE-026")]
public class TestRide026CompositeSealTests
{
    [Fact]
    [Trait("FR", "FR-RIDE-045")]
    [Trait("AC", "AC-RIDE-045-001")]
    public void Composite_is_a_sealed_record()
    {
        var record = Fixtures.CaptureHappy().Capture.SealedComposite;
        Assert.Equal(EvidenceKind.Composite, record.Kind);
        Assert.True(record.Receipt.Composite);
        Assert.True(CollectionSealer.LooksSealed(record.Envelope));
    }

    [Fact]
    [Trait("FR", "FR-RIDE-045")]
    [Trait("AC", "AC-RIDE-045-002")]
    public void Composite_and_sensor_receipts_share_admission_policy()
    {
        var composite = Fixtures.CaptureHappy().Capture.SealedComposite.Receipt;
        var clock = new FixedClock(DateTimeOffset.Parse("2026-09-28T12:00:00Z"));
        var sealer = new CollectionSealer(clock);
        var (publicKey, _) = EscrowKeyFactory.CreateEphemeral("escrow");
        var gateAuth = new PlayIntegrity.PlayIntegrityGate(
            new PlayIntegrity.FixturePlayIntegrityClient(),
            PlayIntegrity.PackageAllowlist.CreateDevelopmentDefault(),
            clock).AuthorizeKeyGeneration(PlayIntegrity.AttestationRequest.Create("collector"));
        var sensor = sealer.Seal(new SealRequest
        {
            Plaintext = [9, 9, 9],
            SessionId = "session-sensor",
            RecordId = "sensor-1",
            Scope = KeyScope.Sample,
            ScopeId = "sensor-1",
            Authorization = gateAuth,
            EscrowKey = publicKey,
            CollectorIdentity = "collector",
            ProvenanceTag = "sensor",
            Kind = EvidenceKind.SensorSample,
            DeviceIds = ["device-1"],
            SourceCommitNotice = "test-commit",
        });
        Assert.Equal(AdmissionPolicy.PolicyId, composite.AdmissionPolicyId);
        Assert.Equal(sensor.Receipt.AdmissionPolicyId, composite.AdmissionPolicyId);
        Assert.Equal(AdmissionPolicy.ChainId, composite.ChainId);
        Assert.Equal("pending-server-write", composite.ChainWriteStatus);
        Assert.Equal("", composite.BlockchainTxHint);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-046")]
    [Trait("AC", "AC-RIDE-046-001")]
    public void Optional_raw_requires_consent_and_links_to_composite()
    {
        var denied = Assert.Throws<RideAuditFailClosedException>(() =>
            Fixtures.CaptureHappy(sealRaw: true, rawConsent: false, raw: [1, 2, 3, 4]));
        Assert.Equal("FR-RIDE-046", denied.RequirementId);

        var ok = Fixtures.CaptureHappy(sealRaw: true, rawConsent: true, raw: System.Text.Encoding.UTF8.GetBytes("raw-stream"));
        Assert.NotNull(ok.Capture.SealedRaw);
        Assert.Equal(ok.Capture.SealedComposite.Id, ok.Capture.SealedRaw!.Receipt.LinkedCompositeId);
        Assert.Equal(EvidenceKind.RawStream, ok.Capture.SealedRaw.Kind);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-046")]
    [Trait("AC", "AC-RIDE-046-002")]
    public void Submission_does_not_ask_the_server_to_reencode_plaintext()
    {
        var request = Fixtures.CaptureHappy().Capture.Submission.Request;
        Assert.Equal("driver", request.SubmitterRole);
        Assert.DoesNotContain("reencode", request.ContentType);
        Assert.True(request.Ciphertext.Length > 33);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-048")]
    [Trait("AC", "AC-RIDE-048-001")]
    public void Custody_package_includes_composite_metadata()
    {
        var meta = Fixtures.CaptureHappy().Capture.SealedComposite.Receipt.CompositeMetadata!;
        Assert.False(string.IsNullOrWhiteSpace(meta.Codec));
        Assert.False(string.IsNullOrWhiteSpace(meta.Compression));
        Assert.False(string.IsNullOrWhiteSpace(meta.SyncClockOffset));
        Assert.False(string.IsNullOrWhiteSpace(meta.OverlayManifestVersion));
        Assert.Equal(2, meta.SourceStreamIds.Count);
        Assert.Equal(2, meta.SourceContentHashes.Count);
        Assert.Equal(2, meta.DeviceIds.Count);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-048")]
    [Trait("AC", "AC-RIDE-048-002")]
    public void Prepared_submission_has_no_plaintext_marker()
    {
        var bytes = Fixtures.CaptureHappy().Capture.Submission.Request.Ciphertext.ToByteArray();
        Assert.False(Bytes.Contains(bytes, System.Text.Encoding.ASCII.GetBytes(Fixtures.Marker)));
    }
}

[Trait("Partition", "TEST-RIDE-033")]
public class TestRide033QuotaTests
{
    [Fact]
    [Trait("FR", "FR-RIDE-219")]
    [Trait("AC", "AC-RIDE-219-001")]
    public void Quotas_and_max_sizes_are_defined()
    {
        var policy = VideoQuotaPolicy.Default;
        Assert.True(policy.MaxCompositeBytes > 0);
        Assert.True(policy.MaxRawStreamBytes > 0);
        Assert.True(policy.ChunkSizeBytes > 0);
        var over = VideoQuota.CheckComposite(policy.MaxCompositeBytes + 1, policy);
        Assert.False(over.Ok);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-219")]
    [Trait("AC", "AC-RIDE-219-002")]
    public void Chunks_round_trip_and_reject_tamper_or_gaps()
    {
        var payload = Enumerable.Range(0, 1000).Select(index => (byte)(index % 251)).ToArray();
        var chunks = ChunkedSealedTransfer.Chunk(payload, 128);
        var rebuilt = ChunkedSealedTransfer.Reassemble(chunks);
        Assert.Equal(payload, rebuilt);

        var tampered = chunks.ToList();
        tampered[1] = tampered[1] with { Payload = [0, 1, 2] };
        Assert.Throws<RideAuditFailClosedException>(() => ChunkedSealedTransfer.Reassemble(tampered));

        var gap = chunks.Where(chunk => chunk.Index != 1).ToList();
        var gapEx = Assert.Throws<RideAuditFailClosedException>(() => ChunkedSealedTransfer.Reassemble(gap));
        Assert.Equal("CHUNK_OUT_OF_ORDER", gapEx.Code);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-219")]
    [Trait("AC", "AC-RIDE-219-003")]
    public void Raw_budget_is_separate_from_composite()
    {
        var policy = new VideoQuotaPolicy
        {
            MaxCompositeBytes = 100,
            SessionCompositeBudget = 100,
            MaxRawStreamBytes = 10,
            SessionRawBudget = 10,
        };
        Assert.True(VideoQuota.CheckComposite(80, policy).Ok);
        Assert.False(VideoQuota.CheckRaw(11, consent: true, policy).Ok);
        Assert.False(VideoQuota.CheckRaw(4, consent: false, policy).Ok);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-220")]
    [Trait("AC", "AC-RIDE-220-001")]
    public void Performance_metrics_are_populated()
    {
        var metrics = Fixtures.CaptureHappy().Capture.Performance.Metrics;
        Assert.True(metrics.BatteryMeasured);
        Assert.True(metrics.ThermalMeasured);
        Assert.True(metrics.MemoryBytes > 0);
        Assert.True(metrics.IngressBytes > 0);
        Assert.True(metrics.EncodingLatency >= TimeSpan.Zero);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-220")]
    [Trait("AC", "AC-RIDE-220-002")]
    public void Below_threshold_metrics_are_not_admitted()
    {
        var badSync = new VideoPerformanceMetrics(10, 1000, 50, 30, TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(80), 0, 10, true, true);
        var decision = PerformanceGate.Evaluate(badSync, 10, new PerformanceThresholds());
        Assert.False(decision.Admitted);
        Assert.Contains("Clock sync", decision.Detail);

        var missing = new VideoPerformanceMetrics(10, 1000, null, null, TimeSpan.Zero, TimeSpan.Zero, 0, 10, false, false);
        Assert.False(PerformanceGate.Evaluate(missing, 10, new PerformanceThresholds()).Admitted);
    }
}

internal static class Bytes
{
    public static bool Contains(byte[] haystack, byte[] needle)
    {
        if (needle.Length == 0 || haystack.Length < needle.Length)
        {
            return false;
        }

        for (var i = 0; i <= haystack.Length - needle.Length; i++)
        {
            if (haystack.AsSpan(i, needle.Length).SequenceEqual(needle))
            {
                return true;
            }
        }

        return false;
    }
}
