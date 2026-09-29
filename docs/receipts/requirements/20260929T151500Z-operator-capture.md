# Operator requirement capture after plan approval

Date: 2026-09-29T15:15:00Z. Revised: 2026-09-29T15:52:00Z. Host: PAYTON-LEGION2. Workspace: `F:\GitHub\rideaudit`.
SPDX: GPL-2.0-only.

This receipt lists draft BDPv4 entities captured from operator direction after PLAN-RIDEAUDIT-001 approval (Astra AGREE R7 on r3.3). Every new or updated row is pending Payton AGREE. Astra plan AGREE is not Payton section 8 AGREE. None of these rows close plan section 9 Class C boxes. None mark AC-RIDE-222-001 or AC-UC-025-001 satisfied. Live OpenTimestamps confirmation and Caddy edge TLS stay open.

The 15:15Z text of this file was a partial Class C inventory. This revision replaces that list. The partial list is not frozen and is not complete.

## Sources

- conversation-history (full), from PLAN-RIDEAUDIT-001 approval through the full-history correction, plus the 2026-09-29 follow-up to add SharpNinja.aiUnit device visual regression. This is not limited to the Class C morning stretch.
- Seed file: `C:\Users\kingd\.cursor\projects\F-GitHub-rideaudit\uploads\operator-directions-seed_8c95.md` (operator-directions-seed).
- Repo evidence: `docs/Project/*.yaml`, plan revisions r3.4 through r3.8 in `docs/plans/PLAN-RIDEAUDIT-001-implementation.md`, `docs/plans/PLAN-RIDEAUDIT-001-BRACKET.md` (HW-AC-MOUNT-001, no second FR for the cradle), and receipts under `docs/receipts/` (distribution Octopus and ngrok, chain live OTS pending, android Fold 4 and edge, requirements 20260928T234854Z).

## MCP

Plugin: `F:\github\mcpserver-grok-plugin`. Agent: GrokCode. Wrapper: `lib\repl-invoke.ps1`.
`skills/*/scripts/invoke.ps1` is not present in that plugin.

The capture prompt named `MCP_WORKSPACE_PATH=F:\github\mcpserver`. RideAudit `MCP_TRUSTED.yaml` binds this product to `F:\GitHub\rideaudit`. Ingest used `F:\GitHub\rideaudit`. `getFr` returned `workspaceId: F:\GitHub\rideaudit`. RideAudit entities were not written into the mcpserver product workspace.

First ingest (partial list, still on record): `createBatch` 22 records, `errors: []`. `updateBatch` 10 records, `errors: []`.

This revision, 2026-09-29T15:34:14Z: `workflow.requirements.createBatch` created 9 records (FR-RIDE-070..072, TR-RIDE-LAB-002..004, TEST-RIDE-050..052), `success: true`, `errors: []`.
`workflow.requirements.updateBatch` updated 19 records, `total: 19`, `errors: []`.
`workflow.requirements.deleteFr` removed FR-RIDE-068. A later `getFr` returned `FR 'FR-RIDE-068' not found`.
`deleteMapping` then `createMapping` for FR-RIDE-041. `listMappings` shows TR-RIDE-VIDEO-001, TR-RIDE-VIDEO-016, TR-RIDE-HW-001, TEST-RIDE-025, TEST-RIDE-044, TEST-RIDE-048 (`totalCount: 6`).
`createMapping` stored FR-RIDE-070, FR-RIDE-071, and FR-RIDE-072.
`getFr` FR-RIDE-222 shows AC-RIDE-222-004 as individual Payton Byrd using IV plus eSigner, not an organization OV certificate, and not to be purchased yet. `isSatisfied` remains false.
`getFr` FR-RIDE-070 is pending with AC-RIDE-070-001..004 unsatisfied.

Addendum 2026-09-29T15:41:19Z: `createBatch` created FR-RIDE-073, TR-RIDE-VIDEO-017, and TEST-RIDE-053 (`total: 3`, `errors: []`). `createMapping` stored FR-RIDE-073 to TR-RIDE-VIDEO-017 and TEST-RIDE-053. `client.UseCases.CreateAsync` created useCaseId 170 for UC-RIDE-043, left Draft. `getFr` FR-RIDE-073 is pending. No AC is satisfied. This does not close plan section 9.

Addendum 2026-09-29T15:46:19Z: storyboard tests must walk each step sequence through SharpNinja.Avalonia.RemoteControl (AvaloniaRemote) and compare a screenshot at each frame. A static single-shot screenshot does not satisfy a storyboard AC. Wireframe tests stay a single-screen compare unless that wireframe says otherwise. `updateBatch` updated FR-RIDE-073, TR-RIDE-VIDEO-017, and TEST-RIDE-053 (`total: 3`, `errors: []`). UC-RIDE-043 brief updated. `LinkFrAsync` links use case 170 to FR-RIDE-067. `getFr` AC-RIDE-073-003 matches that storyboard rule and stays unsatisfied. Plan section 9 is not closed.

Addendum 2026-09-29T15:51:33Z: screenshot validation includes usability validation in addition to baseline comparison. Fail closed on cut-off, truncated, or clipped text, missing icons, overlapping controls, text overflow, and other layout defects from the screenshot or the AvaloniaRemote visual tree. A pixel match alone does not satisfy the ACs when a usability defect is present. `updateBatch` updated FR-RIDE-073 and TR-RIDE-VIDEO-017 (`total: 2`, `errors: []`). `createBatch` created TEST-RIDE-054 (`errors: []`). Mapping for FR-RIDE-073 is TR-RIDE-VIDEO-017, TEST-RIDE-053, and TEST-RIDE-054. UC-RIDE-043 (170) brief includes AC-UC-043-007 and AC-UC-043-008. `getFr` AC-RIDE-073-009 stays unsatisfied. Plan section 9 is not closed.

New MCP use cases (approvalStatus left Draft, not Approved):

| YAML localId | MCP useCaseId | FR |
| --- | --- | --- |
| UC-RIDE-034 | 161 | FR-RIDE-222 |
| UC-RIDE-035 | 162 | FR-RIDE-065 |
| UC-RIDE-036 | 163 | FR-RIDE-066 |
| UC-RIDE-037 | 164 | FR-RIDE-067 |
| UC-RIDE-038 | 165 | FR-RIDE-041 (was FR-RIDE-068; link updated) |
| UC-RIDE-039 | 166 | FR-RIDE-069 |
| UC-RIDE-040 | 167 | FR-RIDE-070 |
| UC-RIDE-041 | 168 | FR-RIDE-071 |
| UC-RIDE-042 | 169 | FR-RIDE-072 |
| UC-RIDE-043 | 170 | FR-RIDE-073 |

Brief updates this revision: 137 (UC-RIDE-010), 149 (UC-RIDE-022), 159 (UC-RIDE-032), 161, 165, 166. MCP use case objects do not store acceptance-criteria arrays. AC text is in repo YAML.

`workflow.sessionlog.bootstrap` returned `deprecated: true` and `initialized: true`. That flag is the plugin success shape, not a failed call.

## Direction inventory

| Seed direction | Disposition | IDs | Approval |
| --- | --- | --- | --- |
| Octopus to PAYTON-DESKTOP, new container if licenses exhausted, no GHCR | reconciled, already FR-RIDE-063 AC-001..003 | FR-RIDE-063, UC-RIDE-032 (159), TR-RIDE-DEPLOY-001, TR-RIDE-DEPLOY-002, TEST-RIDE-038, TEST-RIDE-040 | pending Payton AGREE |
| PAYTON-OMARCHY compose then DESKTOP Octopus. Compose is not the CD green | update | FR-RIDE-063 AC-004, UC-RIDE-032 AC-002, TR-RIDE-DEPLOY-002 AC-003, TEST-RIDE-038 AC-003 | pending Payton AGREE |
| Canonical ngrok PAYTON-DESKTOP 192.168.0.149:28080. Omarchy 127.0.0.1:18080 is prior interim | reconciled, already FR-RIDE-064 | FR-RIDE-064, UC-RIDE-033, TR-RIDE-EDGE-001, TEST-RIDE-039 | pending Payton AGREE |
| Avalonia UI 12 Android and desktop. gRPC .NET 10 containers | reconciled, no duplicate | FR-RIDE-056..062, UC-RIDE-025..031, TR-RIDE-VIDEO-012, TR-RIDE-VIEW-005, TR-RIDE-SERVER-008..010, TEST-RIDE-035..037 | pending Payton AGREE |
| BT roles: driver coordinates, passenger video sync and telematics. RideAudit pairing only, not a Lyft BT API | update on existing FR | FR-RIDE-053 AC-003, FR-RIDE-054, FR-RIDE-055, UC-RIDE-022 (149) AC-001, TR-RIDE-VIDEO-010, TEST-RIDE-034 AC-001..002 | pending Payton AGREE |
| RAES working-copy decrypt via counsel and HSM. Fail-closed public admission | update | FR-RIDE-020 AC-004, UC-RIDE-010 (137) AC-003, TEST-RIDE-016 AC-003. Fail-closed admission remains FR-RIDE-036 and FR-RIDE-061 | pending Payton AGREE |
| HV opposing agent and model. Retain request and response JSONL. Commit immediately. AGREE needs accuracy and completeness at or above 98 | new | FR-RIDE-070, UC-RIDE-040 (167), TR-RIDE-LAB-002, TEST-RIDE-050 | pending Payton AGREE |
| Class A name-or-defer. Deferred wins for live third-party ACs. A covered row is not whole-AC closure. 401/23/0/424 is not P11b done | new | FR-RIDE-071, UC-RIDE-041 (168), TR-RIDE-LAB-003, TEST-RIDE-051 | pending Payton AGREE |
| P11b signed reproducible desktop. Lab self-sign interim allowed. Public Trust not claimed | update | FR-RIDE-222, UC-RIDE-034 (161), TR-RIDE-VIEW-006, TEST-RIDE-041. AC-RIDE-222-001 stays unsatisfied | pending Payton AGREE |
| Ignore macOS builds. Windows-only for now | update | FR-RIDE-222 AC-005, FR-RIDE-057 notes, TR-RIDE-VIEW-001 notes | pending Payton AGREE |
| Publisher is individual Payton Byrd. Commercial path is IV plus eSigner. Not an organization OV certificate | update. Corrects the earlier "IV or OV" wording | FR-RIDE-222 AC-004, TR-RIDE-VIEW-006 AC-003, UC-RIDE-034, TEST-RIDE-041 AC-002 | pending Payton AGREE |
| Do not buy a signing certificate yet. Self-sign now. Real certs later | update | FR-RIDE-222 AC-004 | pending Payton AGREE |
| Install Cursor Desktop on LEGION2. Agents as ninja@thesharp.ninja | update | FR-RIDE-069 AC-003..004, UC-RIDE-039 (166), TR-RIDE-LAB-001 AC-003, TEST-RIDE-049 AC-003 | pending Payton AGREE |
| Cursor agents on the PAYTON-LEGION2 private worker so mcpserver-grok-plugin stays available | new from the partial pass, still pending | FR-RIDE-069, UC-RIDE-039 (166), TR-RIDE-LAB-001, TEST-RIDE-049 | pending Payton AGREE |
| Caddy edge TLS distinct from ngrok | new from the partial pass, still pending. Class C | FR-RIDE-065, UC-RIDE-035 (162), TR-RIDE-EDGE-002, TEST-RIDE-045. FR-RIDE-064 and FR-RIDE-201 notes say ngrok HTTPS is not Caddy | pending Payton AGREE |
| Live OTS public calendar submit. Pending until txid | update | FR-RIDE-018, TR-RIDE-CHAIN-002, UC-RIDE-009, TEST-RIDE-042 | pending Payton AGREE |
| Android validation on attached Samsung Fold 4, not emulator-only | update | FR-RIDE-056, UC-RIDE-025, TR-RIDE-VIDEO-015, TEST-RIDE-043. AC-UC-025-001 stays unsatisfied | pending Payton AGREE |
| Larger durable Android UI font in app styles | new from the partial pass, still pending | FR-RIDE-066, UC-RIDE-036 (163), TR-RIDE-VIDEO-013, TEST-RIDE-046 | pending Payton AGREE |
| Motorola edge 2024 secondary after wireless adb | update | FR-RIDE-041 AC-003, UC-RIDE-017 AC-003, TR-RIDE-VIDEO-016, TEST-RIDE-044 | pending Payton AGREE |
| Cradle tray for Fold 4 CLOSED, landscape, primary cameras forward. Dual post-blocks, slotted arms, thumbscrews, cradle grid | update on FR-RIDE-041. FR-RIDE-068 withdrawn as a duplicate of HW-AC-MOUNT-001 | FR-RIDE-041 AC-004..006, UC-RIDE-038 (165), TR-RIDE-HW-001, TEST-RIDE-048 | pending Payton AGREE |
| SharpNinja.Avalonia.RemoteControl instead of ADB taps | new from the partial pass, still pending | FR-RIDE-067, UC-RIDE-037 (164), TR-RIDE-VIDEO-014, TEST-RIDE-047 | pending Payton AGREE |
| GPL notices, including headrest mount scad, README, BOM, and ARTIFACT | update | FR-RIDE-030 AC-003, TR-RIDE-GPL-002 AC-003. FR-RIDE-029 stays the GPL-2.0 code FR | pending Payton AGREE |
| Lab conduct: accuracy, receipts, no silent path substitution, no Python, no em or en dashes, approve-before-execute except go-by-default on DESKTOP and LEGION2 | new | FR-RIDE-072, UC-RIDE-042 (169), TR-RIDE-LAB-004, TEST-RIDE-052 | pending Payton AGREE |
| Android Avalonia client references SharpNinja.aiUnit. Wireframes are a single-screen device compare unless the wireframe says otherwise. Storyboards are a RemoteControl step sequence with a compare at each frame. Screenshot validation also checks usability and fails closed on layout defects. A pixel match does not pass when a usability defect is present. A static single-shot screenshot does not satisfy a storyboard. Threshold documented. Run on LEGION2. Receipt required. No silent skip | update | FR-RIDE-073 AC-008 and AC-009, UC-RIDE-043 AC-007 and AC-008, TR-RIDE-VIDEO-017 AC-008 and AC-009, TEST-RIDE-053, TEST-RIDE-054 | pending Payton AGREE |
| Play, HSM escrow, Concierge | reconciled, no duplicate | FR-RIDE-031, FR-RIDE-214, FR-RIDE-004 | pending Payton AGREE |

## Withdrawn

FR-RIDE-068 (Fold 4 tray as its own FR) is withdrawn. Bracket plan HW-AC-MOUNT criteria stay on FR-RIDE-041. UC-RIDE-038 remains and realizes FR-RIDE-041. MCP no longer has FR-RIDE-068.

## Not closed

Plan section 9 stays unchecked. Public Trust is not claimed. Caddy TLS is not receipted. Live OTS has no Bitcoin txid. AC-RIDE-222-001 and AC-UC-025-001 stay unsatisfied. HW1 on-vehicle print stays open. A lab signature, an Octopus receipt-on-file, and an ngrok HTTP 200 are evidence, not AC satisfaction.
