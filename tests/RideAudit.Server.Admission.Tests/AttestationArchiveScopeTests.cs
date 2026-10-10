namespace RideAudit.Server.Admission.Tests;

/// <summary>
/// The sealed record id comes from the client's envelope and is not unique. Two submissions that share
/// one must each map to their own retained attestation token, so an access export never carries
/// another submission's token (Codex review 5480479481).
/// </summary>
public class AttestationArchiveScopeTests
{
    [Fact]
    [Trait("FR", "FR-RIDE-010")]
    [Trait("AC", "AC-RIDE-010-001")]
    public void Submissions_sharing_a_sealed_record_id_keep_their_own_attestation_tokens()
    {
        var journal = new CustodyJournal();
        var archive = new AttestationArchive();
        var first = journal.CreatePending("tenant-a", "driver-a", "idem-a", [1], [1], [1], [], [1], "nonce-a", "aa", "rec-shared");
        archive.Put(first.SubmissionId, "token-a");
        var second = journal.CreatePending("tenant-b", "driver-b", "idem-b", [2], [2], [2], [], [2], "nonce-b", "bb", "rec-shared");
        archive.Put(second.SubmissionId, "token-b");

        var source = new JournalRecordSource(journal, archive);

        Assert.Equal("token-a", source.ForDriver("driver-a").Single().Custody!.AttestationToken);
        Assert.Equal("token-b", source.ForDriver("driver-b").Single().Custody!.AttestationToken);
        Assert.Equal("token-a", source.Find(first.SubmissionId)!.Custody!.AttestationToken);
    }
}
