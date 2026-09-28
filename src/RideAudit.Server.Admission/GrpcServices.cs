using Google.Protobuf;
using Grpc.Core;
using Grpc.Core.Interceptors;
using RideAudit.Contracts;
using RideAudit.Escrow;
using RideAudit.Protos.Admission.V1;
using RideAudit.Protos.Escrow.V1;
using RideAudit.Protos.Identity.V1;
using RideAudit.Sec;
using RideAudit.Server.Identity;
using ProtoAdmission = RideAudit.Protos.Admission.V1.Admission;
using ProtoEscrow = RideAudit.Protos.Escrow.V1.Escrow;
using ProtoIdentity = RideAudit.Protos.Identity.V1.Identity;

namespace RideAudit.Server.Admission;

public sealed class RideAuditExceptionInterceptor : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
        where TRequest : class
        where TResponse : class
    {
        try
        {
            return await continuation(request, context);
        }
        catch (RideAuditException ex)
        {
            if (!string.IsNullOrEmpty(ex.SubmissionId))
                context.ResponseTrailers.Add("x-rideaudit-submission-id", ex.SubmissionId);
            throw new RpcException(new Status(GrpcStatusMapper.Map(ex.Code), ex.Code + ": " + ex.Message));
        }
    }
}

public static class GrpcStatusMapper
{
    public static StatusCode Map(string code) => code switch
    {
        ErrorCodes.AuthRequired => StatusCode.Unauthenticated,
        ErrorCodes.AuthForbidden or ErrorCodes.TenantIsolation => StatusCode.PermissionDenied,
        ErrorCodes.RateLimited or ErrorCodes.QuotaExceeded or ErrorCodes.SizeLimit => StatusCode.ResourceExhausted,
        ErrorCodes.SubmissionNotFound => StatusCode.NotFound,
        ErrorCodes.IdempotencyConflict or ErrorCodes.ChunkOutOfOrder => StatusCode.AlreadyExists,
        ErrorCodes.ValidationFailed => StatusCode.InvalidArgument,
        ErrorCodes.NotImplemented => StatusCode.Unimplemented,
        _ => StatusCode.FailedPrecondition
    };
}

public static class CallerContext
{
    public static string? Bearer(ServerCallContext context)
    {
        var value = context.RequestHeaders.GetValue("authorization");
        if (string.IsNullOrWhiteSpace(value) || !value.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return null;
        return value["Bearer ".Length..].Trim();
    }

    public static string ClientIp(ServerCallContext context) =>
        context.RequestHeaders.GetValue("x-rideaudit-client-ip") ?? context.Peer ?? "unknown";
}

public sealed class IdentityGrpcService : ProtoIdentity.IdentityBase
{
    private readonly DriverDirectory _directory;
    private readonly SecretRedactor _redactor;
    private readonly InMemoryLogSink _logs;

    public IdentityGrpcService(DriverDirectory directory, SecretRedactor redactor, InMemoryLogSink logs)
    {
        _directory = directory;
        _redactor = redactor;
        _logs = logs;
    }

    public override Task<RegisterDriverResponse> RegisterDriver(RegisterDriverRequest request, ServerCallContext context)
    {
        var result = _directory.Register(request.Email, request.DisplayName, request.Jurisdiction, request.Purpose, request.ConsentAccepted, request.ConsentStatement);
        _redactor.Track(result.AccessToken);
        _redactor.Track(result.RecoveryCode);
        _logs.Write("registered driver " + result.DriverId);
        return Task.FromResult(new RegisterDriverResponse
        {
            DriverId = result.DriverId,
            TenantId = result.TenantId,
            Email = result.Email,
            CreatedUnixMillis = result.CreatedUnixMillis,
            AccessToken = result.AccessToken,
            RecoveryCode = result.RecoveryCode
        });
    }

    public override Task<RecoverAccountResponse> RecoverAccount(RecoverAccountRequest request, ServerCallContext context)
    {
        _redactor.Track(request.RecoveryCode);
        var result = _directory.Recover(request.Email, request.RecoveryCode);
        _redactor.Track(result.AccessToken);
        _logs.Write("recovered driver " + result.DriverId);
        return Task.FromResult(new RecoverAccountResponse
        {
            DriverId = result.DriverId,
            TenantId = result.TenantId,
            AccessToken = result.AccessToken
        });
    }

    public override Task<Vehicle> RegisterVehicle(RegisterVehicleRequest request, ServerCallContext context)
    {
        var caller = Require(context);
        var vehicle = _directory.RegisterVehicle(caller, request.VinOrPlateKey, request.Label, request.Make, request.Model, request.Year, request.ConsentAccepted);
        return Task.FromResult(Map(vehicle));
    }

    public override Task<Vehicle> UpdateVehicle(UpdateVehicleRequest request, ServerCallContext context)
    {
        var caller = Require(context);
        var vehicle = _directory.UpdateVehicle(caller, request.VehicleId, request.Label, request.Make, request.Model, request.Year, request.ChangeReason);
        return Task.FromResult(Map(vehicle));
    }

    public override Task<ListVehiclesResponse> ListVehicles(ListVehiclesRequest request, ServerCallContext context)
    {
        var caller = Require(context);
        var response = new ListVehiclesResponse();
        response.Vehicles.AddRange(_directory.ListVehicles(caller).Select(Map));
        return Task.FromResult(response);
    }

    public override Task<ConfigurationProfile> PutConfigurationProfile(PutConfigurationProfileRequest request, ServerCallContext context)
    {
        var caller = Require(context);
        var profile = _directory.PutProfile(caller, request.VehicleId, request.Jurisdiction, request.ChainProfile, request.Valid, request.Notes);
        return Task.FromResult(Map(profile));
    }

    public override Task<ConfigurationProfile> GetConfigurationProfile(GetConfigurationProfileRequest request, ServerCallContext context)
    {
        var caller = Require(context);
        var profile = _directory.GetProfile(caller, request.VehicleId)
            ?? throw new RideAuditException(ErrorCodes.ConfigProfileInvalid, "Configuration profile is missing.");
        return Task.FromResult(Map(profile));
    }

    private DriverPrincipal Require(ServerCallContext context)
    {
        var token = CallerContext.Bearer(context);
        _redactor.Track(token);
        return _directory.Authenticate(token) ?? throw new RideAuditException(ErrorCodes.AuthRequired, "Driver authentication is required.");
    }

    private static Vehicle Map(VehicleRecord vehicle)
    {
        var mapped = new Vehicle
        {
            VehicleId = vehicle.VehicleId,
            DriverId = vehicle.DriverId,
            TenantId = vehicle.TenantId,
            VinOrPlateKey = vehicle.VinOrPlateKey,
            Label = vehicle.Label,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = vehicle.Year,
            RegisteredUnixMillis = vehicle.RegisteredUnixMillis
        };
        mapped.Changes.AddRange(vehicle.Changes.Select(change => new VehicleChange
        {
            AtUnixMillis = change.AtUnixMillis,
            ActorDriverId = change.ActorDriverId,
            Summary = change.Summary
        }));
        return mapped;
    }

    private static ConfigurationProfile Map(ConfigurationProfileRecord profile) => new()
    {
        ProfileId = profile.ProfileId,
        VehicleId = profile.VehicleId,
        TenantId = profile.TenantId,
        Jurisdiction = profile.Jurisdiction,
        ChainProfile = profile.ChainProfile,
        Valid = profile.Valid,
        PolicyVersion = profile.PolicyVersion
    };
}

public sealed class AdmissionGrpcService : ProtoAdmission.AdmissionBase
{
    private readonly AdmissionCoordinator _admission;
    private readonly DriverDirectory _directory;
    private readonly SecretRedactor _redactor;

    public AdmissionGrpcService(AdmissionCoordinator admission, DriverDirectory directory, SecretRedactor redactor)
    {
        _admission = admission;
        _directory = directory;
        _redactor = redactor;
    }

    public override Task<AuditSession> OpenSession(OpenSessionRequest request, ServerCallContext context)
    {
        var caller = Require(context);
        var session = _directory.OpenSession(caller, request.VehicleId, request.DualPhone);
        return Task.FromResult(new AuditSession
        {
            SessionId = session.SessionId,
            DriverId = session.DriverId,
            TenantId = session.TenantId,
            VehicleId = session.VehicleId,
            Status = session.Status,
            CreatedUnixMillis = session.CreatedUnixMillis,
            DualPhone = session.DualPhone
        });
    }

    public override Task<AdmissionDecision> SubmitSealed(SubmitSealedRequest request, ServerCallContext context)
    {
        var token = CallerContext.Bearer(context);
        _redactor.Track(token);
        _redactor.Track(request.Attestation?.Token);
        var caller = _directory.Authenticate(token);
        var outcome = _admission.Submit(new SubmitSealedCommand
        {
            Principal = caller,
            RawBearerToken = token,
            ClientIp = CallerContext.ClientIp(context),
            IdempotencyKey = request.IdempotencyKey,
            ContentType = request.ContentType,
            EnvelopeBytes = request.SealedEnvelope.ToByteArray(),
            SubmittedReceiptBytes = request.ReceiptCore?.ToByteArray() ?? Array.Empty<byte>(),
            AttestationToken = request.Attestation?.Token ?? "",
            AttestationNonce = request.Attestation?.Nonce ?? "",
            BoundKeyId = request.Attestation?.BoundKeyId ?? "",
            SessionId = request.SessionId,
            VehicleId = request.VehicleId
        });
        return Task.FromResult(Map(outcome));
    }

    public override Task<UploadSealedChunkResponse> UploadSealedChunk(UploadSealedChunkRequest request, ServerCallContext context)
    {
        var token = CallerContext.Bearer(context);
        var caller = _directory.Authenticate(token);
        var (complete, decision) = _admission.UploadChunk(
            caller,
            token,
            CallerContext.ClientIp(context),
            request.UploadId,
            request.ChunkIndex,
            request.TotalChunks,
            request.SealedChunk.ToByteArray(),
            request.ChunkContentHash.ToByteArray(),
            request.ContentType,
            request.SessionId,
            request.VehicleId,
            request.IdempotencyKey,
            request.ReceiptCore is null || request.ReceiptCore.CalculateSize() == 0 ? null : request.ReceiptCore.ToByteArray(),
            request.Attestation?.Token,
            request.Attestation?.Nonce,
            request.Attestation?.BoundKeyId);
        return Task.FromResult(new UploadSealedChunkResponse
        {
            Complete = complete,
            Decision = decision is null ? null : Map(decision),
            RejectCode = ""
        });
    }

    public override Task<AdmissionDecision> GetAdmissionStatus(GetAdmissionStatusRequest request, ServerCallContext context)
    {
        var caller = Require(context);
        return Task.FromResult(Map(_admission.GetStatus(caller, request.SubmissionId)));
    }

    public override Task<HealthResponse> Health(HealthRequest request, ServerCallContext context) =>
        Task.FromResult(new HealthResponse
        {
            Status = "ok",
            ContractAuthority = ContractAuthority.Authoritative,
            OpenapiRole = ContractAuthority.OpenApiRole,
            Framework = "net10.0",
            ContractVersion = ContractAuthority.ContractVersion,
            Service = "rideaudit.admission.v1"
        });

    private DriverPrincipal Require(ServerCallContext context)
    {
        var token = CallerContext.Bearer(context);
        _redactor.Track(token);
        return _directory.Authenticate(token) ?? throw new RideAuditException(ErrorCodes.AuthRequired, "Driver authentication is required.");
    }

    private static AdmissionDecision Map(AdmissionOutcome outcome)
    {
        var decision = new AdmissionDecision
        {
            SubmissionId = outcome.SubmissionId,
            CustodyState = CustodyStateNames.ToWire(outcome.State),
            Admitted = outcome.Admitted,
            CollectionComplete = outcome.CollectionComplete,
            RejectCode = outcome.RejectCode ?? "",
            Message = outcome.Message,
            CiphertextStored = outcome.CiphertextStored,
            ReceiptCoreDigest = Google.Protobuf.ByteString.CopyFrom(outcome.ReceiptCoreDigest)
        };
        decision.Checks.AddRange(outcome.Checks);
        if (outcome.Anchor is not null)
            decision.Anchor = outcome.Anchor;
        return decision;
    }
}

public sealed class EscrowGrpcService : ProtoEscrow.EscrowBase
{
    private readonly HsmKeyCustody _hsm;
    private readonly CustodyJournal _journal;
    private readonly DriverDirectory _directory;

    public EscrowGrpcService(HsmKeyCustody hsm, CustodyJournal journal, DriverDirectory directory)
    {
        _hsm = hsm;
        _journal = journal;
        _directory = directory;
    }

    public override Task<EscrowPackageInfo> GetEscrowStatus(GetEscrowStatusRequest request, ServerCallContext context)
    {
        var caller = Require(context);
        if (!_hsm.IsEscrowed(request.KeyId, caller.TenantId))
            throw new RideAuditException(ErrorCodes.EscrowUnavailable, "Key is not escrowed for this tenant.");
        var info = _hsm.Describe(request.KeyId);
        var mapped = new EscrowPackageInfo
        {
            KeyId = info.KeyId,
            TenantId = info.TenantId,
            SealedRecordId = info.SealedRecordId,
            ThresholdM = info.ThresholdM,
            TotalN = info.TotalN,
            Escrowed = true,
            SecretKind = info.SecretKind
        };
        mapped.CustodianIds.AddRange(info.CustodianIds);
        return Task.FromResult(mapped);
    }

    public override Task<CourtReleaseState> RequestCourtRelease(RequestCourtReleaseRequest request, ServerCallContext context)
    {
        var caller = Require(context);
        var release = _hsm.RequestRelease(request.KeyId, caller.TenantId, request.CaseId, request.LegalProcessReference, request.Purpose, request.RequesterId);
        return Task.FromResult(Map(release));
    }

    public override Task<CourtReleaseState> ApproveCourtRelease(ApproveCourtReleaseRequest request, ServerCallContext context)
    {
        Require(context);
        return Task.FromResult(Map(_hsm.Approve(request.ReleaseId, request.CustodianId, request.Statement)));
    }

    public override Task<WorkingCopyDescriptor> OpenExpiringWorkingCopy(OpenWorkingCopyRequest request, ServerCallContext context)
    {
        Require(context);
        var release = _hsm.ReleaseLog.LastOrDefault(entry => entry.ReleaseId == request.ReleaseId)
            ?? throw new RideAuditException(ErrorCodes.EscrowQuorum, "Release was not found.");
        var record = _journal.FindBySealedRecord(request.SealedRecordId)
            ?? throw new RideAuditException(ErrorCodes.SubmissionNotFound, "Sealed record was not found.");
        var copy = _hsm.OpenWorkingCopy(request.ReleaseId, record.EnvelopeBytes, TimeSpan.FromMinutes(15));
        try
        {
            return Task.FromResult(new WorkingCopyDescriptor
            {
                ReleaseId = copy.ReleaseId,
                SealedRecordId = request.SealedRecordId,
                ExpiresUnixMillis = copy.Expires.ToUnixTimeMilliseconds(),
                Plaintext = Google.Protobuf.ByteString.CopyFrom(copy.ReadPlaintext()),
                MinimumScope = copy.MinimumScope
            });
        }
        finally
        {
            copy.Wipe();
        }
    }

    public override Task<CounselVerificationReport> VerifyForCounsel(VerifyForCounselRequest request, ServerCallContext context)
    {
        Require(context);
        var record = _journal.Find(request.SubmissionId)
            ?? throw new RideAuditException(ErrorCodes.SubmissionNotFound, "Submission was not found.");
        var mismatches = new List<string>();
        var hashMatch = false;
        var chainConfirmed = false;
        if (record.Anchor is null)
            mismatches.Add("Anchor envelope is missing.");
        else
        {
            var verification = RideAudit.Chain.AnchorProofVerifier.Verify(record.ReceiptCoreBytes, record.Ciphertext, record.Anchor);
            hashMatch = verification.PayloadHashMatch;
            chainConfirmed = verification.Confirmed;
            mismatches.AddRange(verification.Mismatches);
        }
        var quorum = _hsm.ReleaseLog.Any(entry => entry.ReleaseId == request.ReleaseId && entry.Action == "open-working-copy");
        if (!quorum)
            mismatches.Add("Escrow quorum release is not recorded.");
        var report = new CounselVerificationReport
        {
            PayloadHashMatch = hashMatch,
            ChainConfirmed = chainConfirmed,
            AttestationOk = record.State is CustodyState.Admitted,
            EscrowQuorumMet = quorum,
            WorkingCopyAuthorized = quorum && hashMatch && chainConfirmed,
            Proves = CourtReviewStatements.Proves,
            DoesNotProve = CourtReviewStatements.DoesNotProve,
            WorkingCopyExpiresUnixMillis = quorum ? DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeMilliseconds() : 0,
            LiveBitcoinMetadata = record.Anchor?.LiveBitcoinMetadata ?? false
        };
        report.Mismatches.AddRange(mismatches);
        return Task.FromResult(report);
    }

    private DriverPrincipal Require(ServerCallContext context) =>
        _directory.Authenticate(CallerContext.Bearer(context))
        ?? throw new RideAuditException(ErrorCodes.AuthRequired, "Driver authentication is required.");

    private static CourtReleaseState Map(CourtRelease release) => new()
    {
        ReleaseId = release.ReleaseId,
        KeyId = release.KeyId,
        CaseId = release.CaseId,
        Approvals = release.Approvers.Count,
        ThresholdM = release.ThresholdM,
        QuorumMet = release.Approvers.Count >= release.ThresholdM,
        State = release.State
    };
}

public static class CourtReviewStatements
{
    public const string Proves =
        "An upgraded custody anchor proves that the receipt-core digest was committed under the stated proof source. " +
        "It binds the sealed-payload content hash, public key id, collector, time, provenance, and attestation hash that were inside the receipt core.";

    public const string DoesNotProve =
        "The receipt does not prove the plaintext contents, the truth of sensor readings, identity of persons depicted, " +
        "or that a fixture calendar is a live Bitcoin transaction. Decryption still requires a court-authorized M-of-N release into an expiring working copy.";
}
