using Grpc.Core;
using RideAudit.Contracts;
using RideAudit.Ingest;
using RideAudit.Protos.Counsel.V1;
using RideAudit.Protos.Ingest.V1;
using RideAudit.Protos.Privacy.V1;
using RideAudit.Sec;
using RideAudit.Server.Identity;
using ProtoCounsel = RideAudit.Protos.Counsel.V1.Counsel;
using ProtoIngest = RideAudit.Protos.Ingest.V1.Ingest;
using ProtoPrivacy = RideAudit.Protos.Privacy.V1.Privacy;

namespace RideAudit.Server.Admission;

public sealed class CounselGrpcService : ProtoCounsel.CounselBase
{
    private readonly AdmissionComposition _app;

    public CounselGrpcService(AdmissionComposition app) => _app = app;

    public override Task<MultiDriverBundle> BuildMultiDriverBundle(BuildMultiDriverBundleRequest request, ServerCallContext context)
    {
        var (caller, role) = PlatformAuth.Require(_app, context);
        _ = caller;
        var bundle = _app.Counsel.Build(role, request.CaseId, request.SubmissionIds);
        var response = new MultiDriverBundle
        {
            BundleId = bundle.BundleId,
            CaseId = bundle.CaseId,
            AggregationReplacesRecords = bundle.AggregationReplacesRecords
        };
        response.Records.AddRange(bundle.Records.Select(row => new PerRecordVerification
        {
            SubmissionId = row.SubmissionId,
            DriverId = row.DriverId,
            VehicleId = row.VehicleId,
            CollectorId = row.CollectorId,
            IndependentCustody = row.IndependentCustody,
            ContentHashHex = row.ContentHashHex,
            CustodyState = row.CustodyState,
            HashMatches = row.HashMatches,
            AnchorStatus = row.AnchorStatus
        }));
        return Task.FromResult(response);
    }

    public override Task<CoverageMatrix> AnalyzeCoverage(AnalyzeCoverageRequest request, ServerCallContext context)
    {
        var (caller, role) = PlatformAuth.Require(_app, context);
        var matrix = _app.Analysis.Coverage(caller.DriverId, role, request.DriverId, request.ImportId);
        var response = new CoverageMatrix { DriverId = matrix.DriverId };
        response.Cells.AddRange(matrix.Cells.Select(cell => new CoverageCell
        {
            Signal = cell.Signal,
            CollectedBySource = cell.CollectedBySource,
            AvailableToAuditor = cell.AvailableToAuditor,
            Missing = cell.Missing,
            GapNotice = cell.GapNotice
        }));
        return Task.FromResult(response);
    }

    public override Task<OnlineHoursAudit> AnalyzeOnlineHours(AnalyzeOnlineHoursRequest request, ServerCallContext context)
    {
        var (caller, role) = PlatformAuth.Require(_app, context);
        var audit = _app.Analysis.OnlineHours(caller.DriverId, role, request.DriverId, request.Jurisdiction, request.MaxHours, request.BreakHours);
        var response = new OnlineHoursAudit { HoursDataPresent = audit.HoursDataPresent, Policy = audit.Policy };
        response.Violations.AddRange(audit.Violations);
        return Task.FromResult(response);
    }

    public override Task<IncidentReport> AnalyzeIncidentWindow(AnalyzeIncidentWindowRequest request, ServerCallContext context)
    {
        var (caller, role) = PlatformAuth.Require(_app, context);
        var report = _app.Analysis.Incident(caller.DriverId, role, request.DriverId, request.StartUnixMillis, request.EndUnixMillis);
        var response = new IncidentReport { TripCount = report.TripCount, ScoreCount = report.ScoreCount, GapNotice = report.GapNotice };
        response.Lines.AddRange(report.Lines);
        return Task.FromResult(response);
    }

    public override Task<VerificationReport> BuildVerificationReport(VerificationReportRequest request, ServerCallContext context)
    {
        var (caller, role) = PlatformAuth.Require(_app, context);
        var report = _app.Counsel.Verify(caller.DriverId, role, request.CaseId, request.SubmissionId);
        return Task.FromResult(new VerificationReport
        {
            SubmissionId = report.SubmissionId,
            HashMatches = report.HashMatches,
            AnchorConfirmed = report.AnchorConfirmed,
            DecryptionPerformed = report.DecryptionPerformed,
            Proves = report.Proves,
            DoesNotProve = report.DoesNotProve,
            AnchorStatus = report.AnchorStatus,
            TransactionReference = report.TransactionReference
        });
    }
}

public sealed class IngestGrpcService : ProtoIngest.IngestBase
{
    private readonly AdmissionComposition _app;

    public IngestGrpcService(AdmissionComposition app) => _app = app;

    public override Task<IngestPrivacyExportResponse> IngestPrivacyExport(IngestPrivacyExportRequest request, ServerCallContext context)
    {
        var (caller, _) = PlatformAuth.Require(_app, context);
        var result = _app.Ingest.IngestPrivacyExport(Command(caller, request.ConsentGranted, request.ConsentStatement, request.Jurisdiction, request.Purpose, request.ProvenanceTag), request.ZipBytes.ToByteArray(), request.SourceLabel);
        return Task.FromResult(Map(result));
    }

    public override Task<IngestPrivacyExportResponse> IngestThirdParty(IngestThirdPartyRequest request, ServerCallContext context)
    {
        var (caller, _) = PlatformAuth.Require(_app, context);
        var result = _app.Ingest.IngestThirdParty(Command(caller, request.ConsentGranted, request.ConsentStatement, request.Jurisdiction, request.Purpose, ProvenanceTags.ThirdParty), request.CsvBytes.ToByteArray(), request.SourceName);
        return Task.FromResult(Map(result));
    }

    public override Task<IngestPrivacyExportResponse> RecordManualScore(RecordManualScoreRequest request, ServerCallContext context)
    {
        var (caller, _) = PlatformAuth.Require(_app, context);
        var result = _app.Ingest.RecordManualScore(Command(caller, request.ConsentGranted, request.ConsentStatement, request.Jurisdiction, request.Purpose, ProvenanceTags.Manual), request.ObservedUnixMillis, request.Overall, request.GentleBraking, request.SmoothSteering, request.PhoneMount, request.SpeedVsArea);
        return Task.FromResult(Map(result));
    }

    public override Task<PartnershipStatus> SetPartnership(SetPartnershipRequest request, ServerCallContext context)
    {
        var (_, role) = PlatformAuth.Require(_app, context);
        var view = _app.Ingest.SetPartnership(role, request.Approved);
        return Task.FromResult(new PartnershipStatus { Approved = view.Approved, Notice = view.Notice });
    }

    public override Task<IngestPrivacyExportResponse> IngestConciergeStatus(IngestConciergeStatusRequest request, ServerCallContext context)
    {
        var (caller, _) = PlatformAuth.Require(_app, context);
        var result = _app.Ingest.IngestConcierge(Command(caller, request.ConsentGranted, request.ConsentStatement, request.Jurisdiction, request.Purpose, ProvenanceTags.Concierge), request.RideId);
        return Task.FromResult(Map(result));
    }

    private static IngestCommand Command(DriverPrincipal caller, bool consent, string statement, string jurisdiction, string purpose, string provenance) =>
        new(caller.DriverId, jurisdiction, purpose, statement, consent, provenance);

    private static IngestPrivacyExportResponse Map(ImportResult result) => new()
    {
        ImportId = result.ImportId,
        Status = result.Status,
        ContentHashHex = result.ContentHashHex,
        Version = result.Version,
        GapNotice = result.GapNotice,
        ProvenanceTag = result.Provenance
    };
}

public sealed class PrivacyGrpcService : ProtoPrivacy.PrivacyBase
{
    private readonly AdmissionComposition _app;

    public PrivacyGrpcService(AdmissionComposition app) => _app = app;

    public override Task<AccessExportResponse> RequestAccessExport(AccessExportRequest request, ServerCallContext context)
    {
        var (caller, role) = PlatformAuth.Require(_app, context);
        var export = _app.Privacy.Export(caller.DriverId, role, request.SubjectDriverId);
        return Task.FromResult(new AccessExportResponse
        {
            ExportId = export.ExportId,
            Status = export.Status,
            ZipBytes = Google.Protobuf.ByteString.CopyFrom(export.ZipBytes)
        });
    }

    public override Task<DeletionResponse> RequestDeletion(DeletionRequest request, ServerCallContext context)
    {
        var (caller, role) = PlatformAuth.Require(_app, context);
        var result = _app.Privacy.Delete(caller.DriverId, role, request.SubjectDriverId, request.CaseId);
        return Task.FromResult(new DeletionResponse
        {
            Status = result.Status,
            Deleted = result.Deleted,
            CustodyCiphertextRetained = result.CustodyCiphertextRetained
        });
    }

    public override Task<ViewLocationsResponse> ViewLocations(ViewLocationsRequest request, ServerCallContext context)
    {
        var (caller, role) = PlatformAuth.Require(_app, context);
        var samples = _app.Privacy.ViewLocations(caller.DriverId, role, request.DriverId);
        var response = new ViewLocationsResponse();
        response.Samples.AddRange(samples.Select(sample => new LocationSample
        {
            SampleId = sample.SampleId,
            Latitude = sample.Latitude,
            Longitude = sample.Longitude,
            Precise = sample.Precise
        }));
        return Task.FromResult(response);
    }

    public override Task<LegalHoldResponse> PlaceLegalHold(LegalHoldRequest request, ServerCallContext context)
    {
        var (caller, role) = PlatformAuth.Require(_app, context);
        _ = caller;
        _app.Privacy.Holds.Place(role, request.SubjectDriverId, request.CaseId);
        return Task.FromResult(new LegalHoldResponse { Active = true, Status = "held" });
    }
}

internal static class PlatformAuth
{
    public static (DriverPrincipal Principal, string Role) Require(AdmissionComposition app, ServerCallContext context)
    {
        var principal = app.Identity.Authenticate(CallerContext.Bearer(context))
            ?? throw new RideAuditException(ErrorCodes.AuthRequired, "Driver authentication is required.");
        var requested = context.RequestHeaders.GetValue("x-rideaudit-role") ?? Roles.Subject;
        if (!app.Roles.Is(principal.DriverId, requested))
            throw new RideAuditException(ErrorCodes.AuthForbidden, "Role is not granted.");
        return (principal, requested);
    }
}
