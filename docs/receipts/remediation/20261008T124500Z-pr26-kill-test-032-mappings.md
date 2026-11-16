# Remedia receipt: PR #26 kill TEST-RIDE-032 mappings for FR-RIDE-211 and FR-RIDE-213

- **When (UTC):** 20261008T124500Z
- **When (operator):** 2026-10-08 07:45:00 CT
- **Branch:** cursor/capture-operator-reqs-b19f
- **Operator:** Claude Code (Anthropic) cloud session.
- **Authority:** Payton 2026-10-08: "Kill TEST-RIDE-032 mappings for FR-211 and FR-213." TEST-RIDE-032 itself was already on the kill list.
- **No force-push. No rebase. No amend.**

## Changes

| Path | Change |
| --- | --- |
| `docs/Project/Requirements-Mappings-Batch.yaml` | FR-RIDE-211 and FR-RIDE-213: `testIds: [TEST-RIDE-032]` -> `testIds: []`. TR and UC links unchanged (TR-RIDE-SEAL-003 / TR-RIDE-PERF-001, UC-RIDE-009). |
| `tests/RideAudit.Seal.Tests/SealTests.cs`, `tests/RideAudit.Chain.Tests/ChainTests.cs` | Removed the `[Trait("TEST", "TEST-RIDE-032")]` traits and reworded the class summaries. The tests themselves and their FR, AC-RIDE-211-00x, AC-RIDE-213-00x and AC-RIDE-SEAL-003-002 traits are unchanged, so the ledger still counts those ACs as covered. Class names `TestRide032Agility` / `TestRide032Latency` are kept so existing filters keep working. |
| `PLAN-RIDEAUDIT-001-implementation.md`, `PLAN-RIDEAUDIT-001-SERVER.md` | Struck every TEST-RIDE-032 cite as `~~...~~`. The "FR-211/213 remapping TBD" notes now say FR-211/213 map to no TEST (Payton 2026-10-08). |

## MCP sync list addendum (apply on PAYTON-LEGION2)

1. FR-RIDE-211 mapping: testIds `[]` (trIds `[TR-RIDE-SEAL-003]`, useCaseLocalIds `[UC-RIDE-009]`).
2. FR-RIDE-213 mapping: testIds `[]` (trIds `[TR-RIDE-PERF-001]`, useCaseLocalIds `[UC-RIDE-009]`).

## Evidence (this session)

Batch YAML passes a strict duplicate-key parse. Non-Android test projects: Chain 17/17, Client 111/111, Escrow 4/4, Protos 5/5, Seal 8/8, Server.Admission 32/32, Workflow 27/27. Ledger unchanged at 543 (385 covered / 158 deferred / 0 missing). No HV run.
