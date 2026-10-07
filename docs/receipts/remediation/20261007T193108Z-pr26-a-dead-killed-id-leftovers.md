# Remedia receipt: PR #26 step A, remove dead killed-ID leftovers

- **When (UTC):** 20261007T193108Z
- **When (operator):** 2026-10-07 14:31:08 CT
- **Branch:** cursor/capture-operator-reqs-b19f
- **Base HEAD before:** cfd3173dbe110eaa945a9a8cd972e06c1ea10cc2 (step E)
- **Operator:** Grok Bot remedia on PAYTON-LEGION2 (pwsh 7)
- **Authority:** Payton approved step A 2026-10-07.
- **No merge. No force-push. No amend.**

## Changes

1. `docs/receipts/ac-coverage/explicit-deferrals.txt`: removed 11 lines whose AC IDs exist in none of the 9 `docs/Project/*Batch.yaml` files (checked with `id:\s*<AC>` per ID: 0 hits each). Line numbers are as of cfd3173 (after step E):
   - L8 AC-RIDE-004-002, L11 AC-UC-003-001, L12 AC-UC-003-002, L13 AC-RIDE-206-001
   - L16 AC-RIDE-004-001, L17 AC-RIDE-004-003, L18 AC-RIDE-012-001, L19 AC-RIDE-012-002
   - L76 AC-RIDE-INGEST-004-001, L77 AC-RIDE-INGEST-004-002, L150 AC-UC-020-002
   After: 160 explicit entries, every one present in the ledger.
2. `tests/RideAudit.Client.Tests/AcCoverageLedgerTests.cs`:
   - removed the Concierge/partnership keyword rule ("Deferred: Lyft partnership / Concierge live path is not enabled.")
   - removed the AC-RIDE-INGEST-004 prefix rule ("Deferred: Concierge OAuth live partnership is not enabled.")
   - ledger header text: dropped "partnership/" from the list of live-work deferral kinds
   Before removal, no ledger row carried either reason, and no batch YAML AC text contains "Concierge" or "partnership", so no classification changed.
3. Stale AC-RIDE-STORE-003-002 PLAN rows: none left. Step E already marked PLAN 251/428/850 (receipt 20261007T192812Z).
4. `docs/receipts/ac-coverage/20260928-ledger.md`: regenerated. Only the header line changed.

## Tests (Release, PAYTON-LEGION2)

- `dotnet test tests/RideAudit.Client.Tests -c Release -m:1 --filter FullyQualifiedName~AcCoverageLedgerTests`: Passed 1, Failed 0. (The first attempt without `-m:1` hit MSBuild MSB4166 "child node exited prematurely" before any test ran. The rerun passed.)

Ledger after: total 549, covered 386, deferred 163, missing 0 (unchanged from step E).
