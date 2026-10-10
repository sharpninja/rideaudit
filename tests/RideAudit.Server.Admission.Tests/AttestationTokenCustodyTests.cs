using RideAudit.TestSupport;

namespace RideAudit.Server.Admission.Tests;

/// <summary>
/// The retained attestation token lives on the custody record, so each submission maps to its own token
/// even when two share a client-supplied sealed record id, and the token survives a journal export and
/// import (Codex reviews 5480479481 and 5480543403).
/// </summary>
public class AttestationTokenCustodyTests
{
    [Fact]
    [Trait("FR", "FR-RIDE-010")]
    [Trait("AC", "AC-RIDE-010-001")]
    public void Submissions_sharing_a_sealed_record_id_keep_their_own_attestation_tokens()
    {
        var journal = new CustodyJournal();
        var first = journal.CreatePending("tenant-a", "driver-a", "idem-a", [1], [1], [1], [], [1], "nonce-a", "aa", "rec-shared");
        first.AttestationToken = "token-a";
        var second = journal.CreatePending("tenant-b", "driver-b", "idem-b", [2], [2], [2], [], [2], "nonce-b", "bb", "rec-shared");
        second.AttestationToken = "token-b";

        var source = new JournalRecordSource(journal);

        Assert.Equal("token-a", source.ForDriver("driver-a").Single().Custody!.AttestationToken);
        Assert.Equal("token-b", source.ForDriver("driver-b").Single().Custody!.AttestationToken);
        Assert.Equal("token-a", source.Find(first.SubmissionId)!.Custody!.AttestationToken);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-010")]
    [Trait("AC", "AC-RIDE-010-001")]
    public void Admitted_attestation_token_survives_journal_export_and_import()
    {
        var world = ServerWorld.Create();
        var enrolled = world.Enroll();
        var ready = world.SealReady(enrolled.Driver, enrolled.Session);
        var outcome = world.Submit(enrolled.Driver, ready.Package, ready.Token, ready.Nonce);

        var restored = new CustodyJournal();
        restored.Import(world.App.Journal.Export());

        var view = new JournalRecordSource(restored).ForDriver(enrolled.Driver.DriverId).Single();
        Assert.Equal(outcome.SubmissionId, view.SubmissionId);
        Assert.Equal(ready.Token, view.Custody!.AttestationToken);
    }
}
