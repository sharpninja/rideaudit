using RideAudit.Contracts;
using RideAudit.TestSupport;

namespace RideAudit.Server.Admission.Tests;

public class ChunkUploadBoundsTests
{
    [Fact]
    [Trait("FR", "FR-RIDE-219")]
    [Trait("TR", "TR-RIDE-VIDEO-006")]
    [Trait("AC", "AC-RIDE-VIDEO-006-001")]
    [Trait("AC", "AC-RIDE-039-002")]
    public void Incomplete_uploads_are_bounded_by_count_bytes_and_age()
    {
        var world = ServerWorld.Create();
        var enrolled = world.Enroll();
        var principal = world.Require(enrolled.Driver);
        world.App.Admission.MaxConcurrentChunkUploads = 2;
        world.App.Admission.MaxChunksPerUpload = 4;
        world.App.Admission.MaxAggregateChunkBytes = 32;
        world.App.Admission.ChunkUploadMaxAge = TimeSpan.FromMinutes(5);

        var tooManyParts = Assert.Throws<RideAuditException>(() =>
            world.App.Admission.UploadChunk(
                principal, enrolled.Driver.Token, "127.0.0.1", "over-parts", 0, 8, [1], Ids.Sha256([1]),
                RideAuditPolicy.SealedContentType, enrolled.Session.SessionId, enrolled.Vehicle.VehicleId, "idem-over",
                null, null, null, null));
        Assert.Equal(ErrorCodes.SizeLimit, tooManyParts.Code);

        var first = world.App.Admission.UploadChunk(
            principal, enrolled.Driver.Token, "127.0.0.1", "u1", 0, 2, [1, 2], Ids.Sha256([1, 2]),
            RideAuditPolicy.SealedContentType, enrolled.Session.SessionId, enrolled.Vehicle.VehicleId, "idem-u1",
            null, null, null, null);
        Assert.False(first.Complete);
        var second = world.App.Admission.UploadChunk(
            principal, enrolled.Driver.Token, "127.0.0.1", "u2", 0, 2, [3, 4], Ids.Sha256([3, 4]),
            RideAuditPolicy.SealedContentType, enrolled.Session.SessionId, enrolled.Vehicle.VehicleId, "idem-u2",
            null, null, null, null);
        Assert.False(second.Complete);

        var concurrent = Assert.Throws<RideAuditException>(() =>
            world.App.Admission.UploadChunk(
                principal, enrolled.Driver.Token, "127.0.0.1", "u3", 0, 2, [5], Ids.Sha256([5]),
                RideAuditPolicy.SealedContentType, enrolled.Session.SessionId, enrolled.Vehicle.VehicleId, "idem-u3",
                null, null, null, null));
        Assert.Equal(ErrorCodes.QuotaExceeded, concurrent.Code);

        var aggregate = Assert.Throws<RideAuditException>(() =>
            world.App.Admission.UploadChunk(
                principal, enrolled.Driver.Token, "127.0.0.1", "u1", 1, 2, new byte[32], Ids.Sha256(new byte[32]),
                RideAuditPolicy.SealedContentType, enrolled.Session.SessionId, enrolled.Vehicle.VehicleId, "idem-u1",
                null, null, null, null));
        Assert.Equal(ErrorCodes.SizeLimit, aggregate.Code);

        world.Clock.Advance(TimeSpan.FromMinutes(6));
        var expired = Assert.Throws<RideAuditException>(() =>
            world.App.Admission.UploadChunk(
                principal, enrolled.Driver.Token, "127.0.0.1", "u1", 1, 2, [9], Ids.Sha256([9]),
                RideAuditPolicy.SealedContentType, enrolled.Session.SessionId, enrolled.Vehicle.VehicleId, "idem-u1",
                null, null, null, null));
        Assert.Equal(ErrorCodes.WorkingCopyExpired, expired.Code);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-219")]
    [Trait("AC", "AC-RIDE-VIDEO-006-002")]
    public void Completed_chunk_upload_still_assembles_and_admits()
    {
        var world = ServerWorld.Create();
        var enrolled = world.Enroll();
        var ready = world.SealReady(enrolled.Driver, enrolled.Session);
        var envelope = ready.Package.EnvelopeBytes;
        var split = envelope.Length / 2;
        var first = envelope[..split];
        var second = envelope[split..];
        var partial = world.App.Admission.UploadChunk(
            world.Require(enrolled.Driver), enrolled.Driver.Token, "127.0.0.1", "bound-ok", 0, 2, first, Ids.Sha256(first),
            ready.Package.ContentType, enrolled.Session.SessionId, enrolled.Vehicle.VehicleId, "idem-bound",
            null, null, null, null);
        Assert.False(partial.Complete);
        var done = world.App.Admission.UploadChunk(
            world.Require(enrolled.Driver), enrolled.Driver.Token, "127.0.0.1", "bound-ok", 1, 2, second, Ids.Sha256(second),
            ready.Package.ContentType, enrolled.Session.SessionId, enrolled.Vehicle.VehicleId, "idem-bound",
            ready.Package.ReceiptCoreBytes, ready.Token, ready.Nonce, ready.Package.KeyId);
        Assert.True(done.Complete);
        Assert.True(done.Decision!.Admitted);
    }
}
