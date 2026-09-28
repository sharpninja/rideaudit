using Grpc.Core;
using RideAudit.Contracts;
using RideAudit.Protos.Counsel.V1;
using RideAudit.Protos.Ingest.V1;
using RideAudit.Protos.Privacy.V1;

namespace RideAudit.Server.Admission;

public sealed class DeferredGrpcService : Counsel.CounselBase
{
    public override Task<MultiDriverBundle> BuildMultiDriverBundle(BuildMultiDriverBundleRequest request, ServerCallContext context) =>
        throw new RideAuditException(ErrorCodes.NotImplemented, "S5 counsel multi-driver bundle is deferred. Per-record custody is not aggregated in this build.");
}

public sealed class IngestGrpcService : Ingest.IngestBase
{
    public override Task<IngestPrivacyExportResponse> IngestPrivacyExport(IngestPrivacyExportRequest request, ServerCallContext context) =>
        throw new RideAuditException(
            ErrorCodes.NotImplemented,
            "S6 ingest is deferred. This method does not call Lyft private APIs. Analysis must use an authorized working copy, not public ingest.");
}

public sealed class PrivacyGrpcService : Privacy.PrivacyBase
{
    public override Task<AccessExportResponse> RequestAccessExport(AccessExportRequest request, ServerCallContext context) =>
        throw new RideAuditException(ErrorCodes.NotImplemented, "S7 DSAR export is deferred.");

    public override Task<DeletionResponse> RequestDeletion(DeletionRequest request, ServerCallContext context) =>
        throw new RideAuditException(ErrorCodes.NotImplemented, "S7 deletion is deferred and is not performed. Legal hold behavior is not yet enforced.");
}
