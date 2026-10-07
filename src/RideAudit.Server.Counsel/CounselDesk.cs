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
    AnchorProofEnvelope? Anchor);

public interface ISubmittedRecordSource
{
    SubmittedRecordView? Find(string submissionId);
    IReadOnlyList<SubmittedRecordView> ForDriver(string driverId);
}

public sealed record BundleRecord(
    string SubmissionId,
    string DriverId,
    string VehicleId,
    string CollectorId,
    bool IndependentCustody,
    string ContentHashHex,
    string CustodyState,
    bool HashMatches,
    string AnchorStatus);

public sealed record MultiDriverBundle(string BundleId, string CaseId, IReadOnlyList<BundleRecord> Records, bool AggregationReplacesRecords);

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

    public MultiDriverBundle Build(string caseId, IReadOnlyList<string> submissionIds)
    {
        if (string.IsNullOrWhiteSpace(caseId))
            throw new RideAuditException(ErrorCodes.ValidationFailed, "Case id is required.");
        if (submissionIds.Count == 0)
            throw new RideAuditException(ErrorCodes.ValidationFailed, "At least one submission id is required.");
        if (submissionIds.Distinct(StringComparer.Ordinal).Count() != submissionIds.Count)
            throw new RideAuditException(ErrorCodes.ValidationFailed, "Bundle submission ids must be unique.");

        var records = new List<BundleRecord>();
        foreach (var submissionId in submissionIds)
        {
            var record = _records.Find(submissionId) ?? throw new RideAuditException(ErrorCodes.SubmissionNotFound, "Submission was not found.");
            var report = VerifyRecord(record);
            records.Add(new BundleRecord(
                record.SubmissionId,
                record.DriverId,
                record.VehicleId,
                record.CollectorId,
                true,
                Ids.Hex(ReceiptCore.Parser.ParseFrom(record.ReceiptCoreBytes).ContentHash.ToByteArray()),
                record.CustodyState,
                report.HashMatches,
                report.AnchorStatus));
        }
        return new MultiDriverBundle(Ids.New("bnd-"), caseId, records, false);
    }

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
