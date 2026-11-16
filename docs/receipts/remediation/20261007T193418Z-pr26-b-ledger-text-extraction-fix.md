# Remedia receipt: PR #26 step B, fix AC ledger text extraction (own text only)

- **When (UTC):** 20261007T193418Z
- **When (operator):** 2026-10-07 14:34:18 CT
- **Branch:** cursor/capture-operator-reqs-b19f
- **Base HEAD before:** 31dccd2f2ca09d05d2a4a8d4e44f4b96110c71ae (step A)
- **Operator:** Grok Bot remedia on PAYTON-LEGION2 (pwsh 7)
- **Authority:** Payton approved step B 2026-10-07.
- **No merge. No force-push. No amend.**

## Bug

`AcCoverageLedgerTests.ExtractAcText` took a 500-character slice after each `id: AC-...` and preferred the first quoted `text: "..."` in that slice. When an AC's own text was a plain (unquoted) scalar, a later AC's quoted text won. AC-UC-009-002 (plain text "Unverified caveats from source requirements remain visible where applicable.") picked up AC-UC-009-003's quoted OTS/txid text and was keyword-deferred as "Deferred: public OTS remains pending-labeled; no invented txid."

## Fix

`tests/RideAudit.Client.Tests/AcCoverageLedgerTests.cs`:
- `ExtractAcText` (now `internal static`) reads only the AC's own YAML list item. It scans lines after the `- id:` line and stops at the first non-blank line indented at or left of the item's dash. Within that block it takes the `text:` value, quoted (closing quote, backslash escapes honored) or plain (trimmed). Handles LF and CRLF. If the item has no `text:`, it returns empty and never borrows a neighbor's text.
- New regression tests (both FR-RIDE-060):
  - `Ac_text_extraction_takes_only_the_acs_own_text`: synthetic YAML (ids AC-XLEDGER-*, not in any batch) with a plain AC followed by a quoted neighbor containing "txid", LF and CRLF variants, and an AC with no `text:` followed by a neighbor and a parent-level `text:`.
  - `Ac_uc_009_002_reads_its_own_text_not_the_next_acs_ots_text`: reads the real `docs/Project/Use-Cases-Batch.yaml` and asserts AC-UC-009-002 extracts exactly its own sentence with no "txid" or "OpenTimestamps".
  - With the old implementation both would fail: the old slice for AC-UC-009-002 returns AC-UC-009-003's quoted text, and for AC-XLEDGER-001-001 returns the neighbor's quoted text.

## Classification changes (ledger diff before vs after)

Pre-check: for all 549 ACs I compared the old-window text with the own-item text. 3 ACs differed:
- AC-UC-009-002: old window = AC-UC-009-003 text (OTS/txid keywords). Own text has no deferral keyword. It is already named in `tests/RideAudit.Workflow.Tests/ClassAMissingAcTests.cs:69` (`Seal_receipt_chain_profile_and_idempotent_submit_stay_labeled`, which asserts fixture-labeled anchors). **Flip: deferred -> covered.**
- AC-UC-017-002: old window = AC-UC-017-003 text, which matched no keyword rule. It was covered and stays covered (named at `TestRide025And034Tests.cs:138`). No flip.
- AC-UC-025-001: has an explicit deferral, which wins before any keyword. Stays deferred. No flip.

The regenerated ledger diff confirms exactly one row changed: AC-UC-009-002 deferred -> covered.

**Flips to missing: none.**

AC-RIDE-201-001 did not change: its own text is quoted "TLS 1.2+ enforced for network traffic." and still matches the "TLS 1.2" keyword rule ("Deferred: Omarchy loopback is not an edge TLS receipt"). It is named in `AdmissionTests.cs`, but deferred still wins. That predates this bug and is reported, not changed.

## Docs

- `docs/plans/PLAN-RIDEAUDIT-001-implementation.md` lines 8 and 15: counts updated to 549 (387 covered / 162 deferred).
- `docs/receipts/ac-coverage/20260928-ledger.md`: regenerated.

## Tests (Release, PAYTON-LEGION2)

- `dotnet test tests/RideAudit.Client.Tests -c Release -m:1 --filter FullyQualifiedName~AcCoverageLedgerTests`: Passed 3, Failed 0.

Ledger after: total 549, covered 387, deferred 162, missing 0.
