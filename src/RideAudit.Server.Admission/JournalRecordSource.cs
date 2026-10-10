using RideAudit.Protos.Custody.V1;
using RideAudit.Server.Counsel;

namespace RideAudit.Server.Admission;

public sealed class JournalRecordSource : ISubmittedRecordSource
{
    private readonly CustodyJournal _journal;
    private readonly AttestationArchive? _attestations;

    public JournalRecordSource(CustodyJournal journal, AttestationArchive? attestations = null)
    {
        _journal = journal;
        _attestations = attestations;
    }

    public SubmittedRecordView? Find(string submissionId)
    {
        var record = _journal.Find(submissionId);
        return record is null ? null : Map(record);
    }

    public IReadOnlyList<SubmittedRecordView> ForDriver(string driverId) =>
        _journal.Snapshot().Where(record => record.DriverId == driverId).Select(Map).ToArray();

    private SubmittedRecordView Map(CustodyRecord record)
    {
        var core = ReceiptCore.Parser.ParseFrom(record.ReceiptCoreBytes);
        return new SubmittedRecordView(
            record.SubmissionId,
            record.TenantId,
            record.DriverId,
            core.VehicleId,
            core.CollectorId,
            record.ReceiptCoreBytes,
            record.Ciphertext,
            Contracts.CustodyStateNames.ToWire(record.State),
            record.Anchor,
            new CustodyMetadata(
                record.SealedRecordId,
                record.IdempotencyKey,
                Contracts.Ids.Hex(record.BodyHash),
                record.Nonce,
                record.CollectionComplete,
                record.Audit.ToArray(),
                _attestations?.Find(record.SealedRecordId)));
    }
}
