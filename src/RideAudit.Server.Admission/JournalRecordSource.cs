using RideAudit.Protos.Custody.V1;
using RideAudit.Server.Counsel;

namespace RideAudit.Server.Admission;

public sealed class JournalRecordSource : ISubmittedRecordSource
{
    private readonly CustodyJournal _journal;

    public JournalRecordSource(CustodyJournal journal) => _journal = journal;

    public SubmittedRecordView? Find(string submissionId)
    {
        var record = _journal.Find(submissionId);
        return record is null ? null : Map(record);
    }

    public IReadOnlyList<SubmittedRecordView> ForDriver(string driverId) =>
        _journal.Snapshot().Where(record => record.DriverId == driverId).Select(Map).ToArray();

    private static SubmittedRecordView Map(CustodyRecord record)
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
            record.Anchor);
    }
}
