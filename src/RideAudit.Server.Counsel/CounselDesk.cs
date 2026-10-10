using RideAudit.Anal;
using RideAudit.Chain;
using RideAudit.Contracts;
using RideAudit.Protos.Custody.V1;

namespace RideAudit.Server.Counsel;

public sealed record SubmittedRecordView(
    string SubmissionId,
    string TenantId,
    string DriverId,
    string VehicleId,
    string CollectorId,
    byte[] ReceiptCoreBytes,
    byte[] Ciphertext,
    string CustodyState,
    AnchorProofEnvelope? Anchor,
    CustodyMetadata? Custody = null);

// Retained custody fields about a submission other than its sealed bytes (ciphertext and envelope),
// for the subject's own access export.
public sealed record CustodyMetadata(
    string SealedRecordId,
    string IdempotencyKey,
    string BodyHashHex,
    string Nonce,
    bool CollectionComplete,
    IReadOnlyList<string> Audit);

public interface ISubmittedRecordSource
{
    SubmittedRecordView? Find(string submissionId);
    IReadOnlyList<SubmittedRecordView> ForDriver(string driverId);
}

public sealed record VerificationReport(
    string SubmissionId,
    bool HashMatches,
    bool AnchorConfirmed,
    bool DecryptionPerformed,
    string Proves,
    string DoesNotProve,
    string AnchorStatus,
    string TransactionReference);

public sealed class CounselDesk
{
    private readonly ISubmittedRecordSource _records;

    public CounselDesk(ISubmittedRecordSource records) => _records = records;

    public VerificationReport Verify(string actorId, string caseId, string submissionId)
    {
        if (string.IsNullOrWhiteSpace(caseId))
            throw new RideAuditException(ErrorCodes.ValidationFailed, "Case id is required.");
        var record = _records.Find(submissionId) ?? throw new RideAuditException(ErrorCodes.SubmissionNotFound, "Submission was not found.");
        if (!string.Equals(actorId, record.DriverId, StringComparison.Ordinal))
            throw new RideAuditException(ErrorCodes.TenantIsolation, "Caller cannot verify another driver.");
        return VerifyRecord(record);
    }

    private static VerificationReport VerifyRecord(SubmittedRecordView record)
    {
        var hashMatch = false;
        var confirmed = false;
        var status = record.Anchor?.Status ?? "missing";
        var tx = record.Anchor?.TransactionReference ?? "";
        if (record.Anchor is not null)
        {
            var verification = AnchorProofVerifier.Verify(record.ReceiptCoreBytes, record.Ciphertext, record.Anchor);
            hashMatch = verification.PayloadHashMatch;
            confirmed = verification.Confirmed;
        }
        return new VerificationReport(
            record.SubmissionId,
            hashMatch,
            confirmed,
            false,
            CourtReviewStatements.Proves,
            CourtReviewStatements.DoesNotProve,
            status,
            tx);
    }
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
