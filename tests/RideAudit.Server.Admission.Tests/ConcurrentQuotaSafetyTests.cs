using System.Collections.Concurrent;
using System.Text;
using RideAudit.Contracts;
using RideAudit.TestSupport;

namespace RideAudit.Server.Admission.Tests;

/// <summary>
/// D01: concurrent gRPC-shaped admission/chunk/abuse mutations must stay atomic.
/// These assertions fail on the unsynchronized rem-r1 dictionaries and the
/// rem-r1 aggregate that ignored retained receipt/token metadata.
/// </summary>
public class ConcurrentQuotaSafetyTests
{
    [Fact]
    [Trait("FR", "FR-RIDE-039")]
    [Trait("FR", "FR-RIDE-218")]
    [Trait("TR", "TR-RIDE-SERVER-005")]
    [Trait("TR", "TR-RIDE-VIDEO-006")]
    [Trait("AC", "AC-RIDE-039-002")]
    [Trait("AC", "AC-RIDE-SERVER-005-001")]
    [Trait("AC", "AC-RIDE-VIDEO-006-001")]
    public void Concurrent_new_uploads_never_exceed_max_concurrent()
    {
        var world = ServerWorld.Create();
        var enrolled = world.Enroll();
        var principal = world.Require(enrolled.Driver);
        world.App.Admission.MaxConcurrentChunkUploads = 2;
        world.App.Admission.MaxAggregateChunkBytes = 1_000_000;
        world.App.Abuse.LimitPerMinute = 10_000;

        var accepted = 0;
        var quota = 0;
        var other = new ConcurrentBag<string>();
        Parallel.For(0, 48, index =>
        {
            try
            {
                var chunk = new byte[] { (byte)(index + 1) };
                var result = world.App.Admission.UploadChunk(
                    principal,
                    enrolled.Driver.Token,
                    "127.0.0.1",
                    "concurrent-" + index,
                    0,
                    2,
                    chunk,
                    Ids.Sha256(chunk),
                    RideAuditPolicy.SealedContentType,
                    enrolled.Session.SessionId,
                    enrolled.Vehicle.VehicleId,
                    "idem-concurrent-" + index,
                    null,
                    null,
                    null,
                    null);
                Assert.False(result.Complete);
                Interlocked.Increment(ref accepted);
            }
            catch (RideAuditException ex) when (ex.Code == ErrorCodes.QuotaExceeded)
            {
                Interlocked.Increment(ref quota);
            }
            catch (Exception ex)
            {
                other.Add(ex.GetType().Name + ": " + ex.Message);
            }
        });

        Assert.Empty(other);
        Assert.Equal(2, accepted);
        Assert.Equal(46, quota);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-219")]
    [Trait("AC", "AC-RIDE-VIDEO-006-002")]
    [Trait("AC", "AC-RIDE-SERVER-005-001")]
    public void Concurrent_chunks_on_one_upload_do_not_corrupt_the_dictionary()
    {
        var world = ServerWorld.Create();
        var enrolled = world.Enroll();
        var principal = world.Require(enrolled.Driver);
        world.App.Admission.MaxChunksPerUpload = 32;
        world.App.Admission.MaxAggregateChunkBytes = 1_000_000;
        world.App.Abuse.LimitPerMinute = 10_000;

        var errors = new ConcurrentBag<string>();
        Parallel.For(0, 15, index =>
        {
            try
            {
                var chunk = new byte[] { (byte)(index + 3) };
                var result = world.App.Admission.UploadChunk(
                    principal,
                    enrolled.Driver.Token,
                    "127.0.0.1",
                    "same-upload",
                    index,
                    16,
                    chunk,
                    Ids.Sha256(chunk),
                    RideAuditPolicy.SealedContentType,
                    enrolled.Session.SessionId,
                    enrolled.Vehicle.VehicleId,
                    "idem-same-upload",
                    null,
                    null,
                    null,
                    null);
                Assert.False(result.Complete);
            }
            catch (Exception ex)
            {
                errors.Add(ex.GetType().Name + ": " + ex.Message);
            }
        });

        Assert.Empty(errors);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-039")]
    [Trait("AC", "AC-RIDE-039-002")]
    [Trait("AC", "AC-RIDE-SERVER-005-001")]
    public void Concurrent_abuse_checks_never_exceed_the_per_minute_limit()
    {
        var guard = new AbuseGuard(new FakeClock(ServerWorld.Start))
        {
            LimitPerMinute = 8,
            MaxPayloadBytes = 1024
        };

        var accepted = 0;
        var limited = 0;
        var other = new ConcurrentBag<string>();
        Parallel.For(0, 64, _ =>
        {
            try
            {
                guard.Check("tenant|127.0.0.1", 4);
                Interlocked.Increment(ref accepted);
            }
            catch (RideAuditException ex) when (ex.Code == ErrorCodes.RateLimited)
            {
                Interlocked.Increment(ref limited);
            }
            catch (Exception ex)
            {
                other.Add(ex.GetType().Name + ": " + ex.Message);
            }
        });

        Assert.Empty(other);
        Assert.Equal(8, accepted);
        Assert.Equal(56, limited);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-219")]
    [Trait("AC", "AC-RIDE-VIDEO-006-001")]
    [Trait("AC", "AC-RIDE-SERVER-005-001")]
    public void Retained_receipt_and_token_metadata_count_toward_aggregate_bytes()
    {
        var world = ServerWorld.Create();
        var enrolled = world.Enroll();
        var principal = world.Require(enrolled.Driver);
        world.App.Admission.MaxAggregateChunkBytes = 16;
        world.App.Abuse.LimitPerMinute = 10_000;

        var first = new byte[] { 1, 2, 3, 4 };
        var partial = world.App.Admission.UploadChunk(
            principal,
            enrolled.Driver.Token,
            "127.0.0.1",
            "meta-bytes",
            0,
            3,
            first,
            Ids.Sha256(first),
            RideAuditPolicy.SealedContentType,
            enrolled.Session.SessionId,
            enrolled.Vehicle.VehicleId,
            "idem-meta-bytes",
            null,
            null,
            null,
            null);
        Assert.False(partial.Complete);

        var second = new byte[] { 5 };
        var receipt = Encoding.UTF8.GetBytes("retained-receipt-metadata-xx");
        var token = "retained-attestation-token";
        var overflow = Assert.Throws<RideAuditException>(() =>
            world.App.Admission.UploadChunk(
                principal,
                enrolled.Driver.Token,
                "127.0.0.1",
                "meta-bytes",
                1,
                3,
                second,
                Ids.Sha256(second),
                RideAuditPolicy.SealedContentType,
                enrolled.Session.SessionId,
                enrolled.Vehicle.VehicleId,
                "idem-meta-bytes",
                receipt,
                token,
                "nonce-meta",
                "bound-key"));
        Assert.Equal(ErrorCodes.SizeLimit, overflow.Code);
    }
}
