# RideAudit Project requirements (MCP Server YAML entities)

**Author:** Sharp Ninja

BDPv4 batch files for Functional, Technical, Testing, Use-Case, and Mapping entities.

## Source of truth

- `docs/source/lyft-telematics-audit-requirements.md`
- Stack decision: `docs/architecture/stack.md` (Avalonia UI 12 clients; gRPC on .NET 10 containers)

## MCP status

Repo root currently has `MCP_TRUSTED.yaml` for workspace `F:\GitHub\rideaudit`. The post-planning Octopus + ngrok batch was ingested via `mcpserver-grok-plugin` on 2026-09-28 (see `docs/receipts/requirements/20260928T234854Z-post-planning-octopus-ngrok.md`). If marker signature or health nonce fails, log `MCP_UNTRUSTED` and keep YAML as the reviewable source of truth.

## How to ingest (when MCP is healthy)

1. Confirm McpServer health and clear or update `MCP_UNTRUSTED` per plugin guidance.
2. Ensure the RideAudit workspace is registered (workspacePath `F:\GitHub\rideaudit`).
3. Use `mcpserver-grok-plugin` `createBatch` (or equivalent batch create) against each file under `docs/Project/`:
   - `Functional-Requirements-Batch.yaml`
   - `Technical-Requirements-Batch.yaml`
   - `Testing-Requirements-Batch.yaml`
   - `Use-Cases-Batch.yaml`
   - `Additive-Bluetooth-Pairing-Batch.yaml`
   - `Additive-Avalonia-Grpc-Stack-Batch.yaml`
   - `Additive-PostPlanning-Deploy-Ngrok-Batch.yaml`
   - `Additive-Operator-Capture-20260929-Batch.yaml`
4. Apply `Requirements-Mappings-Batch.yaml` after FR/TR/TEST/UC records exist (use-case localIds map to numeric MCP IDs at runtime). Apply `Additive-PostPlanning-Deploy-Ngrok-Mappings.yaml` after the post-planning batch records exist. Apply `Additive-Operator-Capture-20260929-Mappings.yaml` after the 2026-09-29 capture records exist. That capture mappings file lists **new FR-RIDE-065..074 rows only**; updated mappings for existing FRs (018/041/056/063/222 and peers) already live in `Requirements-Mappings-Batch.yaml` / the post-planning mappings file — do not re-add those FR rows or `createMapping` will collide. Capture FR/TR/TEST text is Payton AGREE 2026-09-29 (About FR-RIDE-074 / TR-VIEW-007 / TEST-055 separately Payton AGREE 2026-10-07). Acceptance criteria stay unsatisfied until proven. Plan section 9 Astra/Payton acceptance boxes are not open work (Payton 2026-09-29).
5. Restore MCP use-case approvals after createBatch: call `client.UseCases.SetApprovalAsync` (or equivalent) for UC-RIDE-034..043 and the six updated use cases UC-RIDE-009, 010, 017, 022, 025, 032 so they match Payton AGREE. Leave UC-RIDE-044 Draft until a separate use-case AGREE. YAML has no `approvalStatus` field; notes alone do not flip MCP approval.
6. Verify counts: Functional-Requirements-Batch.yaml holds 66 FRs after Concierge/RBAC invent removals and the FR-RIDE-038 retirement (Payton 2026-10-07) (was 74: 52 functional + 22 NFR-as-FR; FR-004/012/014 and NFR invent rows gone; FR-077/078 added). Plus additive FR-RIDE-053..055 (Bluetooth), FR-RIDE-056..062 (Avalonia/gRPC stack), FR-RIDE-063..064 (Octopus CD + ngrok), and FR-RIDE-065..067 and FR-RIDE-069..073 (2026-09-29 operator capture, Payton AGREE 2026-09-29, acceptance criteria unsatisfied). FR-RIDE-074, TR-RIDE-VIEW-007, and TEST-RIDE-055 are Payton AGREE 2026-10-07 (About view: copyright plus third-party attributions; ACs satisfied only with proof). UC-RIDE-044 remains Draft until a separate use-case AGREE. FR-RIDE-068 was withdrawn as a duplicate of cradle criteria on FR-RIDE-041. Use-Cases-Batch.yaml must contain a single top-level records key with 28 live use cases (UC-RIDE-001, 002, 004..015, 017..019, 021..031). UC-RIDE-003 and UC-RIDE-020 were removed with Concierge invent (2026-10-07). UC-RIDE-016 was removed with the counsel multi-driver bundle (2026-10-08). Duplicate records keys are invalid for strict parsers. UC-RIDE-032..033 live in Additive-PostPlanning-Deploy-Ngrok-Batch.yaml. UC-RIDE-034..043 live in the 2026-09-29 capture batch. UC-RIDE-044 is the Draft About use case. UC-RIDE-038 realizes FR-RIDE-041.

## ID conventions

- FR: `FR-RIDE-001`..`FR-RIDE-052`, NFRs as `FR-RIDE-201`..`FR-RIDE-222`, additive `FR-RIDE-053`+
- TR: `TR-RIDE-<SUBAREA>-NNN` with SUBAREA in INGEST, STORE, ANAL, SEAL, CHAIN, ESCROW, PLAY, GPL, SERVER, VIDEO, VIEW, PRIV, SEC, PERF, DEPLOY, EDGE, HW, LAB
- TEST: `TEST-RIDE-NNN`
- Use cases: local `UC-RIDE-NNN` (MCP assigns numeric IDs at ingest)

## Use case diagrams

UML use case diagrams for the 28 live Use-Cases-Batch.yaml records (UC-RIDE-001, 002, 004..015, 017..019, 021..031; UC-003/016/020 removed) under the single records key: [docs/ux/use-cases/README.md](../ux/use-cases/README.md). UC-RIDE-032..033 live in the post-planning additive batch and do not have UML diagrams yet.

Session and review sequence diagrams under `docs/ux/flows/` and `docs/ux/review-app/flows/` are workflows. They are not per-use-case UML diagrams.

## Additive batches

| Batch | FR | TR | TEST | UC |
| --- | --- | --- | --- | --- |
| Bluetooth pairing | FR-RIDE-053..055 | TR-RIDE-VIDEO-010..011 | TEST-RIDE-034 | UC-RIDE-022..024 |
| Avalonia/gRPC stack | FR-RIDE-056..062 | TR-RIDE-VIDEO-012, VIEW-005, GPL-004..005, SERVER-008..010 | TEST-RIDE-035..037 | UC-RIDE-025..031 |
| Post-planning Octopus + ngrok | FR-RIDE-063..064 | TR-RIDE-DEPLOY-001..002, EDGE-001 | TEST-RIDE-038..040 | UC-RIDE-032..033 |
| Operator capture 2026-09-29 (Payton AGREE 2026-09-29, acceptance criteria unsatisfied) | FR-RIDE-065..067, 069..073. FR-RIDE-068 withdrawn. Updates to FR-RIDE-018, 020, 030, 041, 053, 056, 057, 063, 064, 201, 222 | TR-RIDE-VIEW-006, EDGE-002, VIDEO-013..017, HW-001, LAB-001..004 plus CHAIN-002, GPL-002, DEPLOY-002 AC updates | TEST-RIDE-041..054 plus AC updates on TEST-RIDE-016, 034, 038 | UC-RIDE-034..043 plus AC updates on UC-RIDE-009, 010, 017, 022, 025, 032 |
| About view (FR/TR/TEST Payton AGREE 2026-10-07; UC Draft) | FR-RIDE-074 copyright plus third-party attributions. Cross-links FR-RIDE-029 and FR-RIDE-030 | TR-RIDE-VIEW-007 | TEST-RIDE-055 | UC-RIDE-044 (Draft) |

## Notes

- No em dashes in YAML text.
- Do not invent Lyft APIs; Unverified caveats from the source doc are preserved in FR notes where relevant.
- Seal-at-collect, blockchain receipts, escrow, Play Integrity, GPL-2.0, public multi-driver server, dual-phone video composite, and desktop court viewer are covered.
- Stack: Avalonia UI 12 for Android capture and desktop court viewer; backend gRPC on .NET 10 containers; protos GPL-2.0; OpenAPI companion non-authoritative. See `docs/architecture/stack.md`.
- Container CD: Use Octopus Deploy. Build containers and deploy to LAB-OMARCHY. If you are out of licenses on the default container, create a new Octopus container on LAB-OMARCHY. Do not use GHCR. FR-RIDE-063. Plans must cite it and must not weaken it. Omarchy plus ngrok is interim admission hosting (FR-RIDE-064).

