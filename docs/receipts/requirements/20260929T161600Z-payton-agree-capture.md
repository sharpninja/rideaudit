# Payton AGREE on the 2026-09-29 operator capture

Date: 2026-09-29T16:16:00Z. Host: PAYTON-LEGION2. Workspace: `F:\GitHub\rideaudit`.
SPDX: GPL-2.0-only.

Operator Payton AGREE (2026-09-29): the drafted BDPv4 rows in the capture on PR #26 and `docs/receipts/requirements/20260929T151500Z-operator-capture.md` correctly capture post-plan operator directions and are approved as requirements.

This AGREE approves the requirement, use case, acceptance-criterion, technical-requirement, and test text as written. It does not claim Public Trust, Caddy done, an OpenTimestamps Bitcoin txid, phone acceptance criteria satisfied, or features complete. Section 9 Astra/Payton plan-acceptance boxes are not reopened and are not tracked as open Class C.

Acceptance criteria stay `isSatisfied: false`. Functional, technical, and test `status` stays `pending`. AC-RIDE-222-001 and AC-UC-025-001 stay unsatisfied.

## AGREEd IDs

New capture rows:

- FR-RIDE-065, FR-RIDE-066, FR-RIDE-067, FR-RIDE-069, FR-RIDE-070, FR-RIDE-071, FR-RIDE-072, FR-RIDE-073
- UC-RIDE-034 through UC-RIDE-043 (MCP 161 through 170), approvalStatus Approved
- TR-RIDE-VIEW-006, TR-RIDE-EDGE-002, TR-RIDE-VIDEO-013, TR-RIDE-VIDEO-014, TR-RIDE-VIDEO-015, TR-RIDE-VIDEO-016, TR-RIDE-VIDEO-017, TR-RIDE-HW-001, TR-RIDE-LAB-001, TR-RIDE-LAB-002, TR-RIDE-LAB-003, TR-RIDE-LAB-004
- TEST-RIDE-041 through TEST-RIDE-054

Drafted updates to existing rows:

- FR-RIDE-018, FR-RIDE-020, FR-RIDE-030, FR-RIDE-041, FR-RIDE-053, FR-RIDE-056, FR-RIDE-057, FR-RIDE-063, FR-RIDE-064, FR-RIDE-201, FR-RIDE-222
- TR-RIDE-CHAIN-002, TR-RIDE-GPL-002, TR-RIDE-VIEW-001, TR-RIDE-VIDEO-010, TR-RIDE-DEPLOY-002
- TEST-RIDE-016, TEST-RIDE-034, TEST-RIDE-038
- UC-RIDE-009 (136), UC-RIDE-010 (137), UC-RIDE-017 (144), UC-RIDE-022 (149), UC-RIDE-025 (152), UC-RIDE-032 (159), approvalStatus Approved

FR-RIDE-068 stays withdrawn.

## Not restamped

These IDs were cited as already covering a direction and this capture did not draft new text on them. This AGREE does not rewrite them:

- FR-RIDE-004, FR-RIDE-031, FR-RIDE-214
- FR-RIDE-054, FR-RIDE-055
- FR-RIDE-058 through FR-RIDE-062
- UC-RIDE-033

## Full-history scan

Checked the seed `operator-directions-seed_8c95.md` against the 15:15Z capture inventory, plus the later directions already in that receipt: SharpNinja.aiUnit device visual tests, storyboard frames through SharpNinja.Avalonia.RemoteControl, usability fail-closed, the `codex-subscription` profile, and Section 9 not open work.

No remaining direction needed a new ID. The asset-review wireframe manifest already on this branch is covered by FR-RIDE-073 per-wireframe coverage. A later new ID still needs its own AGREE unless it is only a note reconciliation of text already in this AGREEd set.

## MCP

Workspace `F:\GitHub\rideaudit`. Plugin `mcpserver-grok-plugin` `lib\repl-invoke.ps1`, agent GrokCode.

`workflow.requirements.updateBatch` updated 53 FR/TR/TEST records in seven batches (counts 8, 8, 8, 8, 8, 8, 5), `success: true`. Notes now say `approval: Payton AGREE 2026-09-29`. `status` stays `pending`. `isSatisfied` stays false.

Use-case briefs were updated for the 16 AGREEd use cases. `client.UseCases.SetApprovalAsync` set `approvalStatus` to Approved for MCP ids 136, 137, 144, 149, 152, 159, and 161 through 170. `getFr` FR-RIDE-065 and FR-RIDE-222 contain the AGREE note and no satisfied acceptance criterion. `GetAsync` for use cases 170 and 159 reports Approved.

## Non-claims

- Public Trust is not claimed.
- Caddy edge TLS is not done.
- Live OpenTimestamps has no Bitcoin txid.
- Phone acceptance criteria are not satisfied. AC-UC-025-001 stays unsatisfied. HW1 on-vehicle print stays open.
- Features are not complete. P11b stays open.
- AC-RIDE-222-001 stays unsatisfied.
- Section 9 is not open Class C work.
