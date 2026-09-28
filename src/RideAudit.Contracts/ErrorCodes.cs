namespace RideAudit.Contracts;

public static class ErrorCodes
{
    public const string AuthRequired = "AUTH_REQUIRED";
    public const string AuthForbidden = "AUTH_FORBIDDEN";
    public const string ValidationFailed = "VALIDATION_FAILED";
    public const string ReceiptMissing = "RECEIPT_MISSING";
    public const string ReceiptInvalid = "RECEIPT_INVALID";
    public const string AttestationFailed = "ATTESTATION_FAILED";
    public const string VehicleUnregistered = "VEHICLE_UNREGISTERED";
    public const string PlaintextRejected = "PLAINTEXT_REJECTED";
    public const string SessionInvalid = "SESSION_INVALID";
    public const string SubmissionNotFound = "SUBMISSION_NOT_FOUND";
    public const string ChunkOutOfOrder = "CHUNK_OUT_OF_ORDER";
    public const string RateLimited = "RATE_LIMITED";
    public const string TenantIsolation = "TENANT_ISOLATION";
    public const string InternalError = "INTERNAL_ERROR";
    public const string NotImplemented = "NOT_IMPLEMENTED";
    public const string ConfigProfileInvalid = "CONFIG_PROFILE_INVALID";
    public const string ChainUnconfirmed = "CHAIN_UNCONFIRMED";
    public const string ChainFailed = "CHAIN_FAILED";
    public const string ChainProfileUnsupported = "CHAIN_PROFILE_UNSUPPORTED";
    public const string EscrowUnavailable = "ESCROW_UNAVAILABLE";
    public const string EscrowQuorum = "ESCROW_QUORUM";
    public const string EscrowIntegrity = "ESCROW_INTEGRITY";
    public const string DuplicateReplay = "DUPLICATE_REPLAY";
    public const string IdempotencyConflict = "IDEMPOTENCY_CONFLICT";
    public const string SizeLimit = "SIZE_LIMIT";
    public const string QuotaExceeded = "QUOTA_EXCEEDED";
    public const string PolicyMismatch = "POLICY_MISMATCH";
    public const string KeyScopeRejected = "KEY_SCOPE_REJECTED";
    public const string LatencyBudgetExceeded = "LATENCY_BUDGET_EXCEEDED";
    public const string WorkingCopyExpired = "WORKING_COPY_EXPIRED";
    public const string ConsentRequired = "CONSENT_REQUIRED";
    public const string PartnershipDisabled = "PARTNERSHIP_DISABLED";
    public const string LegalHoldActive = "LEGAL_HOLD_ACTIVE";
    public const string ImportRejected = "IMPORT_REJECTED";
}

public class RideAuditException : Exception
{
    public RideAuditException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    public string Code { get; }
    public string? SubmissionId { get; init; }
}
