# Remedia receipt: PR #26 small fixes from the review-thread triage

- **When (UTC):** 20261008T123447Z
- **When (operator):** 2026-10-08 07:34:47 CT
- **Branch:** cursor/capture-operator-reqs-b19f
- **Operator:** Claude Code (Anthropic) cloud session.
- **Authority:** Payton 2026-10-08 takeover of PR #26. Each change below answers an open review thread and was checked against the code before it was made.
- **No force-push. No rebase. No amend.**

## Changes

| Thread | Path | Change |
| --- | --- | --- |
| Plan totals / FR-011 rows | `docs/plans/PLAN-RIDEAUDIT-001-SERVER.md`, `PLAN-RIDEAUDIT-001-implementation.md` | SERVER linked-set counts now count only ids that still exist in `docs/Project/*.yaml`: FR 58 -> 48, UC 25 -> 22, TR 47 -> 42, TEST 32 -> 28 (and the SERVER child row 58 -> 48). Ids missing from the batches are struck as `~~...~~`. The FR-RIDE-011 rows now cite TEST-RIDE-001 and TEST-RIDE-002 with no UC (the mapping), not the killed TEST-RIDE-004 and UC-RIDE-020. |
| Plan ledger counts | `PLAN-RIDEAUDIT-001-implementation.md` | Added revision r3.11: FR-RIDE-038, TR-RIDE-SERVER-007 and UC-RIDE-016 retired; ledger 543 (385 covered / 158 deferred / 0 missing). |
| README counts | `docs/Project/README.md` | 66 FRs (FR-RIDE-038 retired); 28 live use cases (UC-RIDE-016 removed). |
| UC-RIDE-021 legal hold | `docs/ux/use-cases/UC-RIDE-021.md` | Include 4 now matches basic flow step 4 in `Use-Cases-Batch.yaml` (own-submissions privacy deletion, no legal-hold controls). The Counsel edge to it is removed, and the related link no longer says "under legal hold". |
| Octopus receipt attribution | `deploy/containers/counsel/README.md`, `deploy/omarchy/ngrok/README.md` | The `octopus-payton-desktop` receipt records the PAYTON-DESKTOP Docker host (`192.168.0.149`), which is what the receipt itself says. The READMEs no longer call it a LAB-OMARCHY deployment and state that no LAB-OMARCHY Octopus receipt exists yet. |
| ngrok conflict check | `deploy/omarchy/ngrok/Start-Ngrok.ps1` | A local tunnel is a conflict unless its full backend address equals `$Addr` after normalization (scheme and trailing slash stripped, lower case). The old `-notmatch '28080'` accepted any address containing `28080`, such as `localhost:28080`. The function-local variable no longer reuses the name `$addr`, which PowerShell treats as the same name as the `$Addr` parameter. |
| Killed AC traits | `tests/RideAudit.Server.Admission.Tests/AdmissionTests.cs`, `tests/RideAudit.Workflow.Tests/ClassAMissingAcTests.cs` | Removed traits for killed ACs AC-RIDE-SEC-003-001, AC-RIDE-204-001 and AC-RIDE-204-002. |
| Azure manifest | `docs/Project/wiki/azure/.mcp-requirements-manifest.json` | The 12 storyboard entries use the nested `Storyboards/Mobile Dual-Phone/` and `Storyboards/Review App/` paths. Every listed document exists. This is a hand edit to generated output: the MCP wiki generator on LEGION2 must emit nested paths, or the next regeneration reverts this. |

## Evidence (this session)

- `Start-Ngrok.ps1` parses under PowerShell 7.6.6 with 0 errors. The conflict function was run against mocked `/api/tunnels` replies. New behavior: `192.168.1.182:28080` and `http://192.168.1.182:28080/` are not conflicts, while `localhost:28080`, `192.168.1.182:280801` and `localhost:7147` are. Old behavior: only `localhost:7147` was a conflict.
- Non-Android test projects: Chain 17/17, Client 111/111, Escrow 4/4, Protos 5/5, Seal 8/8, Server.Admission 32/32, Workflow 27/27. Ledger unchanged at 543 (the removed traits named ACs that no longer exist).

## Not changed (needs Payton or is larger)

- **TEST-RIDE-032:** it is on the kill list, but `Requirements-Mappings-Batch.yaml` still maps FR-RIDE-211 and FR-RIDE-213 to it, and `SealTests.cs` / `ChainTests.cs` still carry its trait. The options are to restore a TEST-RIDE-032 scoped to FR-211/213 (without the killed FR-208), or to map FR-211/213 to no TEST. This needs Payton's choice.
- **Unassigned operator-capture FRs (FR-065..067, 069..074):** these have no implementation phase. That is plan work.
- **Generic deferral reasons:** the explicit deferrals with generic "not yet named in test source" reasons need specific reasons, or the ledger needs to report a non-zero missing count.
- **Role actors:** UC-RIDE-005, 006, 007 and 021 still list Auditor, Counsel or Admin as actors, against the no-roles decision.
- **Other leftovers:** traits for AC-TEST-035/036/037-00x name ACs that were never in the batches. Four `.axaml` files have no BOM.
- No HV run. No FR or AC marked satisfied.
