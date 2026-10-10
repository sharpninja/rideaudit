# Remediation receipt: PR #29 DSAR export omits retained attestation tokens (Codex comment 4235889113)

- **When (UTC):** 20261010T190900Z
- **When (operator):** 2026-10-10 14:09:00 CT
- **Branch:** claude/eager-pascal-wsb5at (PR #29).
- **Operator:** Claude Code (Anthropic) cloud session.
- **Classification:** code defect in this PR's DSAR export work. Payton asked for it to be pushed alone rather than held for the pending owner decisions.
- **No force-push. No rebase. No amend.**

| Finding | Verified | Change |
| --- | --- | --- |
| P1 code: `AdmissionCoordinator` retains the raw Play Integrity token in `AttestationArchive`, keyed by sealed record id, but the FR-RIDE-010 access export carries only the receipt's attestation hash, so the retained evidence is missing from the subject's export | Yes. `PrivacyDesk` reads submissions through `ISubmittedRecordSource`, and `JournalRecordSource` had no access to the archive. | `AttestationArchive.Find(sealedRecordId)`. `JournalRecordSource` takes the archive and fills `CustodyMetadata.AttestationToken` for each record. `AdmissionHost` wires the archive in. The export filters by the subject's own submissions, so another driver's token is never read. |

Test: the DSAR export test in `IngestAnalysisTests.cs` asserts that `Custody.AttestationToken` in `data/receipts.json` equals the subject's submitted token, and that the neighbor's token appears nowhere in the file.

Not changed (pending owner decisions, listed in HANDOFF.md): operator alerts, escrow records, log lines and abuse counters in the DSAR export.

## Validation (one full pass, no fixes needed in it)

- .NET: Chain 17, Client 111, Escrow 4, Protos 5, Seal 8, Server.Admission 32, Workflow 29, all pass. Host.Windows does not build on Linux without `EnableWindowsTargeting` and was not run.
- Mutation: with `AdmissionHost` building `JournalRecordSource` without the archive, Workflow fails 1 of 29 (the DSAR test). Restored, and the diff is byte-identical to the validated one.
- ngrok scripts unchanged; their harnesses were not rerun.
