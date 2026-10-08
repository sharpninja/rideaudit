# Remedia receipt: PR #26 retire FR-RIDE-038 (counsel multi-driver bundle)

- **When (UTC):** 20261008T122143Z
- **When (operator):** 2026-10-08 07:21:43 CT
- **Branch:** cursor/capture-operator-reqs-b19f
- **Operator:** Claude Code (Anthropic) cloud session, taking over from Grok Bot.
- **Authority:** Payton 2026-10-07: "Multidriver is unnecessary." Payton 2026-10-08 scope choice: retire FR-RIDE-038 only. Keep FR-RIDE-037 and drop "multi-driver" from its wording. Leave the approved review-app UX files untouched and list them. Edit the disk batch YAML and list the MCP mutations for LEGION2.
- **No force-push. No rebase. No amend.**

## Why

Codex P1 (thread on `CounselDesk.cs:70`) found that own-submissions-only `Build` could not satisfy FR-RIDE-038 as written. The owner retired the feature instead of restoring cross-driver aggregation.

## Code and contract

| Path | Change |
| --- | --- |
| `src/RideAudit.Protos/Protos/rideaudit/counsel/v1/counsel.proto` | Removed `rpc BuildMultiDriverBundle`, `BuildMultiDriverBundleRequest`, `PerRecordVerification`, `MultiDriverBundle`. Header no longer cites FR-RIDE-038. |
| `src/RideAudit.Server.Admission/PlatformGrpcServices.cs` | Removed the `BuildMultiDriverBundle` override. |
| `src/RideAudit.Server.Counsel/CounselDesk.cs` | Removed `Build`, `MultiDriverBundle` and `BundleRecord`. `Verify` is unchanged. |
| `src/RideAudit.Server.Counsel/CounselSlice.cs` | Removed the "FR-RIDE-038 counsel multi-driver bundle" checklist line. |
| Contract version | 0.3.0 -> **0.4.0**, because removing an RPC is a breaking contract change: `ContractAuthority.ContractVersion`, all eight `.proto` headers, `artifacts/server-api/openapi.yaml`, `artifacts/server-api/ARTIFACT.yaml`, `artifacts/server-api/README.md`, `src/RideAudit.Client.Contracts/SWAP.md`, and `TestRide035ShellTests`. |

## Tests

- Removed `Own_submissions_bundle_keeps_each_record_independent` and `Build_rejects_non_own_submissions` (they exercised the removed `Build`).
- Added `Each_submission_is_verified_on_its_own_record` (Workflow.Tests), TEST-RIDE-023 / FR-RIDE-037. Two submissions by one driver are each verified by `CounselDesk.Verify` on their own record: hash matches, anchor confirmed, no decryption. Each record view keeps its own driver, vehicle and collector ids and its own receipt. Ciphertext is unchanged and no working copy is opened. Failure cases: another driver's submission is `TENANT_ISOLATION`, and an unknown id is `SUBMISSION_NOT_FOUND`. The slice checklist no longer names FR-RIDE-038.
- Traits moved to that test: AC-RIDE-037-001, AC-RIDE-037-002, AC-TEST-023-001, AC-TEST-023-002.
- Not moved, because no remaining test exercises what they say (explicit deferrals added with reasons): AC-RIDE-SERVER-006-002 (aggregate index authorization; no aggregate index exists now), AC-RIDE-SERVER-007-001/002 (bundle builder), AC-UC-016-001/002 (bundle use case).
- `AcCoverageLedgerTests`: unique AC count 549 -> 547 (AC-RIDE-038-001/002 removed). Regenerated `docs/receipts/ac-coverage/20260928-ledger.md`: covered 392 -> 385, deferred 157 -> 162, total 549 -> 547, missing 0.

## Requirement rows on disk (docs/Project)

| File | Change |
| --- | --- |
| `Functional-Requirements-Batch.yaml` | Deleted FR-RIDE-038 with AC-RIDE-038-001/002. FR-RIDE-037 title "Multi-driver per-record provenance" -> "Per-record provenance"; description "Preserve court multi-driver provenance:" -> "Preserve court provenance:". ACs unchanged. |
| `Requirements-Mappings-Batch.yaml` | Deleted the FR-RIDE-038 mapping row. FR-RIDE-037 row unchanged. |
| `Use-Cases-Batch.yaml` | Removed the FR-RIDE-038 Realizes link from UC-RIDE-007 and UC-RIDE-016. |
| `Technical-Requirements-Batch.yaml` | TR-RIDE-SERVER-007 notes: "FR-RIDE-037, FR-RIDE-038" -> "FR-RIDE-037". Title and description untouched (STOP below). |
| `Testing-Requirements-Batch.yaml` | TEST-RIDE-023 title -> "Per-record provenance"; description -> "Per-record verification; aggregation does not weaken custody."; notes -> "Covers FR-RIDE-037; method: system". |

Also: `docs/ux/use-cases/UC-RIDE-007.md` Realizes line drops FR-RIDE-038. `PLAN-RIDEAUDIT-001-implementation.md` and `PLAN-RIDEAUDIT-001-SERVER.md` strike FR-RIDE-038 and AC-RIDE-038-00x as `~~...~~` (same convention as earlier kill-list scrubs).

All nine batch YAML files parse with a strict duplicate-key loader.

## MCP sync list (apply on PAYTON-LEGION2 via mcpserver-grok-plugin, then regenerate the wiki)

This session cannot reach the MCP SoT. Disk is ahead of MCP until these are applied:

1. Delete FR-RIDE-038 (and its ACs AC-RIDE-038-001, AC-RIDE-038-002).
2. Delete the FR-RIDE-038 mapping (trIds TR-RIDE-SERVER-007, testIds TEST-RIDE-023, useCaseLocalIds UC-RIDE-007, UC-RIDE-016).
3. Update FR-RIDE-037: title "Per-record provenance"; description "Preserve court provenance: each submission must be independently verifiable through device attestation, collector_id, driver_id, vehicle_id, and its on-chain receipt; aggregation must not weaken per-record custody."
4. Update TR-RIDE-SERVER-007 notes to "Implements/supports: FR-RIDE-037".
5. Update TEST-RIDE-023: title "Per-record provenance"; description "Per-record verification; aggregation does not weaken custody."; notes "Covers FR-RIDE-037; method: system".
6. Remove the FR-RIDE-038 Realizes links from UC-RIDE-007 and UC-RIDE-016 (use-case tools were not routed on that dispatcher before; disk is the fallback).
7. Regenerate `docs/Project/wiki/azure` and `docs/Project/wiki/github`. Both still show FR-RIDE-038 until then.

## STOP for Payton (not changed, needs a decision)

- **TR-RIDE-SERVER-007** "Counsel multi-driver bundle builder" and **UC-RIDE-016** "Counsel multi-driver bundle" (actors Counsel, Admin) describe the retired bundle. Kill them, or approve BDPv4 rewording. Their ACs are deferred meanwhile. FR-RIDE-037 still maps to both.
- **Approved review-app UX files** that still describe multi-driver bundles (left untouched by scope choice): `docs/ux/review-app/ARTIFACT.yaml`, `README.md`, `flows/review-workflow.md`, `flows/mermaid-review-workflow.md`, `storyboards/SB-R-01-open-bundle.md`, `storyboards/SB-R-05-multi-driver-counsel-bundle.md`, `storyboards/SB-R-06-export-disclosure.md`, `wireframes/WF-R-02-bundle-contents.md`, `wireframes/WF-R-04-fail-closed-blocking.md`, `wireframes/WF-R-08-export-opposing-counsel.md`; also `docs/ux/assets/wireframes/README.md`, `docs/ux/assets/wireframes/_manifest.md`, `docs/wiki.yaml` (sb-r-05 entry), `docs/ux/use-cases/UC-RIDE-016.md`, `docs/ux/use-cases/README.md`, `docs/ux/use-cases/overview.md`.

## Evidence (this session)

- Non-Android test projects: Chain 17/17, Client 111/111, Escrow 4/4, Protos 5/5, Seal 8/8, Server.Admission 32/32, Workflow 27/27.
- `RideAudit.Host.Windows.Tests` compiles with `-p:EnableWindowsTargeting=true` (0 warnings, 0 errors). Not run (Windows only).
- Android projects not built (no Android workload here). No Android source referenced the removed RPC or types.

## Not claimed

No HV run (the 2026-10-08 amendment names `gpt-6.1-sol` at `high`; that run is not available from this session). No FR or AC marked satisfied.
