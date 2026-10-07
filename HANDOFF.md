# RideAudit handoff update (PAYTON-LEGION2)

Written: 2026-10-07 (America/Chicago).
Author: Grok Bot.

## PR #26 remedia resumed (own-submissions + safe accuracy batch)

- Payton: own-submissions only / no roles applied to CounselDesk.Build + Analysis negatives. SetPartnership STOP (global gate).
- Safe threads: ledger 569, mappings dedupe, README UC approvals, plan unassigned=10, Android visual scope, receipt pointer, orphan linkTypes, LAB-OMARCHY 182 deploy retarget, storyboards layout, About attributions, privacy.proto 0.3.0, PrivacyDesk dead branch, Precise=sample.Precise, BOM/nits.
- STOP for Payton: SetPartnership product path; TEST-052 FR whole-tree vs TEST change-scope ambiguity.
- Receipt: docs\receipts\remediation\20261007T161645Z-pr26-safe-remedia-own-submissions.md
- Do not merge. Remedia continues after reviewer re-check.

---# RideAudit handoff update (PAYTON-LEGION2)

Written: 2026-10-07 10:36 CT (America/Chicago).
Author: Grok Bot.

## PRIORITY 1 done: About UI (FR-RIDE-074 / TR-VIEW-007 / TEST-055)

- Shared Avalonia `MainView`: removed top `LicenseNotice`; bottom panel **About** opens dedicated `AboutView` with copyright **and** third-party attributions (Avalonia MIT from NOTICE). Framework label stays on title bar.
- Tests: `TestRide055AboutTests` + `TestRide035ShellTests` **16/16 passed**.
- MCP ACs satisfied (updateBatch req-20261007T153617Z-1d20): FR-074 5/5, VIEW-007 4/4, TEST-055 4/4. status pending. FR-029/030 untouched.
- How to open: bottom panel **About** on capture or review host.

Receipt: `docs/receipts/ui/20261007T153631Z-about-ui-fr074.md` (also requirements/)

---
# RideAudit handoff update (PAYTON-LEGION2)

Written: 2026-10-07 10:30 CT (America/Chicago).
Author: Grok Bot.

## PRIORITY 2 done: PrivacyDesk GeoMask/MaskedLocation cleanup

- Census: `GeoMask`/`MaskedLocation` were **name-only** — already returned full G17 + Precise=true (FR-077).
- Renamed → `GeoPresent` / `PresentedLocation`. Retention unchanged (365/730; precise-geo exempt; no CA 30/180) — matches FR-078.
- PrivacySlice checklist → FR-077/078. Tests: 3/3 passed.
- No About UI. No Camera2/H.264 SEI. PRIORITY 1 (About UI) still queued.

Receipt: `docs/receipts/privacy/*-privacydesk-geomask-cleanup.md` (also under requirements/)

---
# RideAudit handoff update (PAYTON-LEGION2)

Written: 2026-10-07 10:25 CT (America/Chicago).
Author: Grok Bot.

## PRIORITY 3 done: PLAN-RIDEAUDIT-001* obsolete cite scrub

- Touched: `docs/plans/PLAN-RIDEAUDIT-001-implementation.md`, `docs/plans/PLAN-RIDEAUDIT-001-SERVER.md`.
- Kill-list invent cites marked OBSOLETE / struck; pointers to FR-077/078 (+ TR-VIDEO-007 / TR-PRIV-004) where AGREEd.
- ANDROID/BRACKET: no invent rows to scrub (counsel product cites left).
- No About UI / PrivacyDesk / Camera2 / H.264 / telematics code. PRIORITY 2→1 still queued.

Receipt: `docs/receipts/requirements/*-plan-obsolete-cites-scrub.md`

---
# RideAudit handoff update (PAYTON-LEGION2)

Written: 2026-10-07 10:20 CT (America/Chicago).
Author: Grok Bot.

## Payton AGREE FR-RIDE-074 About cluster

- MCP notes: `approval: Payton AGREE 2026-10-07` on FR-RIDE-074, TR-RIDE-VIEW-007, TEST-RIDE-055.
- status remains pending; descriptions/ACs unchanged; isSatisfied false.
- Disk Additive batch + FR-030 pointer synced.
- No About UI feature code.

Receipt: `docs/receipts/requirements/*-payton-agree-fr074-about.md`

---
# RideAudit handoff update (PAYTON-LEGION2)

Written: 2026-10-07 10:18 CT (America/Chicago).
Author: Grok Bot.

## FR-RIDE-074 + TEST-RIDE-010 accuracy

- **FR-RIDE-074:** still Draft pending separate Payton AGREE. No AGREE invented. MCP↔Additive batch consistent. Exact Draft text in receipt for Payton to AGREE/reject. No About UI code.
- **FR-RIDE-010:** sole AC is AC-RIDE-010-001 access export (AC-002 legal-hold already killed). Disk notes synced.
- **TEST-RIDE-010:** option (a) access-export only. Title `DSAR access deletion` → `DSAR access export`. Description unchanged (`Access export works.`). No deletion test invented.

Receipt: `docs/receipts/requirements/*-fr074-test010-accuracy.md`

---
# RideAudit handoff update (PAYTON-LEGION2)

Written: 2026-10-07 10:14 CT (America/Chicago).
Author: Grok Bot.

## BDPv4 AGREE written to MCP SoT

Payton AGREED Candidates A+B; priority high on all four.

- MCP: FR-RIDE-077, TR-RIDE-VIDEO-007, FR-RIDE-078, TR-RIDE-PRIV-004 created (status pending; notes `approval: Payton AGREE 2026-10-07`; acceptanceCriteria empty).
- Mappings: FR-077→TR-VIDEO-007; FR-078→TR-PRIV-004.
- Disk: Functional / Technical / Mappings batch YAML appended.
- Descriptions verbatim from AGREE draft (Exact on FRs; Role body on TRs). No invented AC/mask/RBAC/legal-hold. No feature code.

Receipt: `docs/receipts/requirements/20261007T151421Z-bdpv4-agree-mcp-write.md`

---
# RideAudit handoff update (PAYTON-LEGION2)

Written: 2026-10-07 10:12 CT (America/Chicago).
Author: Grok Bot.

## BDPv4 AGREE → MCP write STOPPED (Accuracy)

Payton AGREED BDPv4 Candidates A+B. Accuracy addendum forbids inventing forced fields.

- `workflow.requirements.createFr` rejected probe without `priority`: `payload.params.priority is required` (req-20261007T151140Z-2739).
- Priority is not in `docs/receipts/requirements/20261007-bdpv4-candidates-for-agree.md`.
- No MCP create. FR-RIDE-077 / 078 / TR-RIDE-VIDEO-007 / TR-RIDE-PRIV-004 still `not_found`.
- No disk YAML sync. No feature code. No invented AC/mask/RBAC/legal-hold.

Receipt: `docs/receipts/requirements/20261007T151201Z-bdpv4-agree-schema-gap-stop.md`

Unblock: supply `priority` (critical|high|medium) for the four ids; confirm Role→title and TR description=Role text; re-dispatch write.

---
# RideAudit handoff update (PAYTON-LEGION2)

Written: 2026-10-07 10:11 CT (America/Chicago).
Author: Grok Bot.

## Accuracy finish

- Legal-hold invent removed from MCP+disk (FR-210 deleted; FR-010 AC-002 removed; TR/TEST legal-hold ACs/phrases removed; FR-030/219 legal-hold phrases removed).
- FR-030 description briefly corrupted by YAML `>-` fold in updateFr; restored to full quoted text without legal holds.
- wiki.yaml child paths = titles verbatim; generateDocument success; wiki legal-hold hit count 0 after restore regen.
- TEST-RIDE-010 description is access-export only (deletion clause was legal-hold-only; not replaced).
- UC-008 legal-hold stripped; Realizes FR-010; Driver only.
- No feature code. No invented replacement ACs.

Receipt: `docs/receipts/requirements/*-legalhold-kill-complete.md`

---
# RideAudit handoff update (PAYTON-LEGION2)

Written: 2026-10-07 10:09 CT (America/Chicago).
Author: Grok Bot (legal-hold kill surgical finish + wiki paths).

## Accuracy

Removed only clear legal-hold invent text. No invented replacement AC/UC stories.

## Done

- MCP: FR-210 deleted; FR-010 AC-002 removed; TR-PRIV-003 / TR-STORE-003 legal-hold ACs+phrases removed; TEST-010 legal-hold title/description clauses removed.
- UC-008: legal-hold stripped; Realizes FR-010; Driver only.
- wiki.yaml: child path = title verbatim (Mobile Dual-Phone / Review App). generateDocument success.
- Receipts under docs/receipts/requirements/*-legalhold-kill-complete.md and *-wiki-yaml-child-paths.md

## Observe for Payton

- TEST-RIDE-010 description is now access-export only (deletion clause was legal-hold-only; not replaced).

## Freeze

No feature code. No commit unless asked.

---
# RideAudit handoff update (PAYTON-LEGION2)

Written: 2026-10-07 10:04 CT (America/Chicago).
Author: Grok Bot executor (legal-hold kill + wiki child paths).

## Freeze

No feature code (Camera2 / H.264 / telematics / PrivacyDesk / About UI). No commit unless Payton asks.

## Done this pass

- MCP: deleted FR-RIDE-210. FR-RIDE-010 AC-RIDE-010-002 (legal hold) removed; AC-001 remains.
- Disk: FR-210 removed from batch YAML; UC-RIDE-008 legal-hold stripped; Realizes FR-RIDE-010 only; Driver-only actors.
- wiki.yaml: child paths set to existing titles verbatim (Mobile Dual-Phone, Review App). Parent Storyboards unchanged.
- generateDocument wiki: success=true (see receipts).

## Receipts

- `docs/receipts/requirements/*-wiki-yaml-child-paths.md`
- `docs/receipts/requirements/*-legalhold-kill-fr210-fr010-uc008.md`
- prior: `*-wiki-yaml-storyboards-dup.md`

## Still open

- FR-RIDE-074 AGREE (About) - out of this pass.
- PLAN docs may still cite killed IDs (stale maps).

---
# RideAudit handoff update (PAYTON-LEGION2)

Written: 2026-10-07 10:00 CT (America/Chicago).
Author: Grok Bot executor (P1 wiki.yaml + P2 UC strip).

## Stop / freeze

No feature code (Camera2 / H.264 / telematics / PrivacyDesk / About UI). Do not mark requirements done. Do not commit unless Payton asks.

## P1 wiki.yaml Storyboards (STOP after new error)

- Path: `F:\GitHub\rideaudit\docs\wiki.yaml`
- Before: navigation[6] path=Storyboards; children[0] Mobile Dual-Phone path=Storyboards; children[1] Review App path=Storyboards (duplicate of parent path).
- Attempted fix: removed child-level `path: Storyboards` keys only; kept both children + parent path.
- generateDocument after that: NEW error `navigation[6].children[0].path is required` (and children[1]). Per orders: STOP, do not invent unique child paths.
- Receipt: `docs/receipts/requirements/*-wiki-yaml-storyboards-dup.md`
- Needs Payton choice: (A) flatten document children under Storyboards with no nested path nodes, (B) assign unique child paths from existing titles, or (C) other.

## P2 UC leftovers

- UC-RIDE-008: STOP not edited. Still Realizes FR-RIDE-010 + FR-RIDE-210. MCP still has FR-RIDE-210 (Legal hold suspends deletion) and FR-RIDE-010 AC-002 honors legal holds. Stripping UC legal-hold text would contradict those FRs.
- UC-RIDE-020: RBAC stripped on disk md + Use-Cases-Batch.yaml. Title/goal now Admin partnership gates aligned to FR-RIDE-012; Realizes remain FR-RIDE-011, FR-RIDE-012. Admin actor kept (FR-012 Admin UI).
- Receipt: `docs/receipts/requirements/*-uc-008-020-strip.md`
- workflow.usecase.* not routed on this MCP dispatcher.

## Explicit next for Payton

1. Decide wiki.yaml nested Storyboards path strategy (A/B/C above).
2. Decide fate of FR-RIDE-210 and FR-RIDE-010 AC legal-hold wording before UC-008 can be stripped.
3. FR-RIDE-074 AGREE still open (out of this pass).

---
# RideAudit handoff update (PAYTON-LEGION2)

Written: 2026-10-07 09:53 CT (America/Chicago).
Author: Grok Bot executor. Current handoff. Older sections below remain historical unless re-proved.

## Stop

No more RideAudit application or feature code until requirements are correct and Payton accepts them. Do not implement Camera2, an H.264 encoder, or telematics. Do not mark requirements done. Do not commit unless he asks.

## Resume state (2026-10-07)

- MCP SoT (via mcpserver-grok-plugin on LEGION2): kill-list rows are already absent (getFr/getTr/getTest not_found): FR-RIDE-014, FR-RIDE-202, FR-RIDE-203, FR-RIDE-208, TR-RIDE-PRIV-002, TR-RIDE-SEC-002, TR-RIDE-SEC-003, TEST-RIDE-012, TEST-RIDE-032.
- Disk docs/Project batch YAML: synced this pass to remove stale kill-list id blocks. Receipt: docs/receipts/requirements/ (latest *-invalid-rows-disk-sync.md).
- Wiki regenerate from MCP: see same session notes / receipt.
- FR-RIDE-074 still pending Draft in MCP - needs Payton AGREE before About UI code.
- PrivacyDesk still has GeoMask/MaskedLocation type names and RetentionPolicy 365/730 day timers (precise-geo exempt per prior HV). App code freeze: do not edit until requirements accepted.
- Plans (PLAN-RIDEAUDIT-001-*) and some UC markdown may still cite kill-list IDs. Treat as stale maps, not SoT.
- Do not invent replacement FR/TR/TEST text without an approved BDPv4 sentence from Payton.

## Explicit next step

1. Payton: AGREE or reject FR-RIDE-074 Draft (About).
2. Payton: approve BDPv4 sentences for any replacements that certifiable-legal-data needs (precise unmasked location embed in H.264; retention that does not treat the driver as a third party). Do not write those rows until approved.
3. Optional hygiene: scrub stale kill-list citations from plans/UC markdown, or regenerate docs from MCP only.
4. Keep PR #24 / #25 / Camera2 / encode frozen.

## Rules that still bind

1. Approved wireframes are source of truth unless Payton names a defect.
2. Assumption kill loop: observation vs inference; if inference would change approved work, stop and report.
3. Requirements live in MCP. GitHub PRs are custody, not SoT.
4. No Python in the lab. No em or en dashes in authored lab text.
5. Cursor agents only on PAYTON-LEGION2 with full MCP through mcpserver-grok-plugin.

---
# RideAudit handoff update (PAYTON-LEGION2)

Written: 2026-10-03 ~11:50 CT (America/Chicago).
Author: AnnoyingOrange. This section is the current handoff. The 2026-10-01 body below is historical and was not re-proved in this pass.

## Stop

No more RideAudit code until the requirements are correct. Do not implement Camera2, an H.264 encoder, or telematics until Payton accepts the corrected requirements. Do not mark requirements done. Do not commit unless he asks.

The 2026-10-01 hold (no RideAudit work until the next weekly reset) was lifted when work resumed on 2026-10-03. The code freeze above replaces it.

## What Payton corrected on 2026-10-03

- RideAudit is a new app. There is no prior code to preserve. Legal hold was never requested. Do not describe invented code as already in the app.
- Do not mask anything. The app exists to create certifiable legal data. Precise location and other capture data stay unmasked.
- He never asked for RBAC. Roles subject, auditor, admin, and counsel are out of scope. Do not put them back, including for legal hold.
- Ride data is evidentiary and collected by the driver in their own vehicle. The driver is not a third party and is not subject to third-party retention limits. The 30-day California and 180-day location deletion in PrivacyDesk contradicts that. Those rows were not edited in the turn that named the contradiction. Verify the current PrivacyDesk text before treating them as still present or as already removed.
- Accelerometer and precise location are to be embedded in the H.264 stream as real-time per-picture data, matched to each picture. That embed is not implemented.
- Cursor agents run only on PAYTON-LEGION2, and only with full MCP Server usage through mcpserver-grok-plugin. Not a partial or skipped MCP path.

## Edits already made (not committed)

Checked in the session that made them. Not re-run in this handoff pass except where noted.

- Legal hold code (LegalHoldRegistry.Place) was removed from src and tests in F:\GitHub\rideaudit, rideaudit-ui-font, and rideaudit-rc080. A search after that found no LegalHold. RideAudit.Workflow.Tests on F:\GitHub\rideaudit: Failed 0, Passed 26, Skipped 0. Docs were not touched.
- RBAC role names subject, auditor, admin, and counsel were removed from source and tests. Lowercase counsel remains only as the court-review product (rideaudit.counsel.v1, CounselDesk). The DSAR zip still says Subject plus the driver id.
- Deleted requirement rows: FR-RIDE-202, FR-RIDE-014, FR-RIDE-203, FR-RIDE-208, TR-RIDE-PRIV-002, TR-RIDE-SEC-002, TR-RIDE-SEC-003, TEST-RIDE-012, TEST-RIDE-032. NFR-2 was removed from the source markdown and batch YAML in rideaudit and rideaudit-ui-font. Stale copies remain in receipts.
- All 30 video requirement descriptions now say H.264. Status is still pending. FR-RIDE-219 says use H.264 and that accelerometer and location are embedded in the H.264 video and matched to each picture.

## Still open

- Requirements audit is not finished. Do not write a replacement row until Payton approves a BDPv4 sentence.
- PR #24 visual and storyboard work, About UI, and PR #25 Caddy path were still open as of 2026-10-01. Do not resume them under the code freeze.
- Fold camera: on 2026-10-03 the Fold showed a hard Camera2 refusal (pipeline unimplemented, not a permission prompt) plus a missing attestation token. Do not start Camera2 encode unless he orders it after the requirements are accepted. Play attestation and HSM escrow stay untouched.
- About view still needs Payton AGREE on FR-RIDE-074 Draft.

## Transcript export (not RideAudit)

Payton ordered the entire AnnoyingOrange session stored at C:\Users\kingd\annoyingorange.jsonl on this machine, with every later message appended. OBS this pass: C:\Users\kingd\annoyingorange.jsonl does not exist. The full tool transcript is not a file on the Grok Bot computer. The service copy is an off-box row index written by CommitGrokBotTranscriptEntries. Large bodies are blobs/<sha256> in the per-tenant box store. The local journal path is /home/box/sand-data/agent-transcripts/<conversation id>/<conversation id>.jsonl, and this chat's id 7dc610bb-aa5a-40cf-85da-1803046495a1 was not in that folder. The Windows UI replica is under C:\Users\kingd\AppData\Roaming\Grok Bot\sand-client-persistence and is not the full tool transcript. Do not invent missing lines. The 5-minute append routine (folder annoyingorange-transcript-append) must be checked before anyone relies on it. As of the start of this chat it was paused. A later status line said the 11:18 AM CT run succeeded.

## Rules that still bind

1. Approved wireframes are source of truth unless Payton names a defect. Do not assume they are wrong from an app or aiUnit failure.
2. Visual QA is controls, layout, and style fidelity, not pixel match. Phone path uses WF-01 through WF-08. WF-R is out of scope.
3. When something is really not correct, stop, present the problem, and wait. Do not assume the fix.
4. No Python in the lab. No em or en dashes in authored lab text. ASCII hyphen only.
5. No done mark without receipts and hostile validation at or above 98.

---

# RideAudit handoff (PAYTON-LEGION2)

Written: 2026-10-01 ~06:30 CT (America/Chicago).
Author: Grok Bot executor handoff pass. Overwrites none prior (no existing HANDOFF.md at write time).

**Observation vs inference:** Items marked OBS are checked on disk, git, or GitHub in this pass. Items marked OPS are operator-stated facts carried forward (profile / prior session); not re-proved here unless noted. Do not treat OPS as newly discovered.

---

## Status summary

- Primary dual-phone app work lives on draft **PR #24** branch `cursor/dual-phone-fold-moto-8aa2`.
- **OBS:** After merge of PR #28, remote tip of that branch is `081e02e4e28cdf1a07054d407fd504e4806be750` (merge commit: parents `88c8714397a7ecf560f43c51f86d105a4f1e905e` + `581ad1ac1b96dd6fd2271d2fa6948da00e8154a7`).
- RemoteControl 0.8.0 port-0 bind is **merged** into the dual-phone branch. Fold + Edge Debug APK install and AvaloniaRemote GetCapabilities were recorded in the PR #28 device receipt. That is **not** a visual/storyboard green and **not** merge-ready for PR #24.
- PLAN-PR24FOLD-001 remains open. Do **not** mark FR/AC greens. Do **not** edit approved wireframes. **WF-R** (desktop review chrome under `docs/ux/review-app/`) is **OUT OF SCOPE** for current dual-phone phone-path work (Payton killed WF-R Fold re-score of tip `88c8714` on 2026-09-30).
- Preferred Cursor worker checkout for this lab: `F:\GitHub\rideaudit` on PAYTON-LEGION2. That tree is populated; this file is at its root.

---

## Environment

| Item | Value | Kind |
| --- | --- | --- |
| Operator | Payton Byrd | OPS |
| Machine | PAYTON-LEGION2 (machineId `e0e72849-1155-4885-be2b-708c9977a19e`) | OBS connected |
| Preferred workspace | `F:\GitHub\rideaudit` | OBS exists, non-empty |
| Current checked-out branch in `rideaudit` | `cursor/capture-operator-reqs-b19f` @ `9f9c5f0cf0c2d9bd92297aff7fc4c776375a6bbc` | OBS (not dual-phone tip) |
| Coding preference | Cursor cloud agents on PAYTON-LEGION2 private worker | OPS |
| MCP plugin | `mcpserver-grok-plugin` at `F:\GitHub\mcpserver-grok-plugin` (also under `C:\Users\kingd\.grok`); agent GrokCode; invoke via pwsh; `MCP_PLUGIN_ROOT` / `GROK_PLUGIN_ROOT` | OPS path OBS exists |
| Fold 4 | serial `RFCW7078MVZ` | OPS + OBS in PR #28 receipt |
| Motorola edge 2024 | serial `ZD222QH58Q` (adb-tls form may appear, e.g. `adb-ZD222QH58Q-DiWqEr._adb-tls-connect._tcp`) | OPS + OBS in receipt |
| Moto lab status | RELEASED for lab use (as of 2026-09-30 evening) | OPS |
| Drive UI | AvaloniaRemote only (no ADB clicking) | OPS / hard rule |
| Sibling checkouts (OBS) | `rideaudit-rc080` on `cursor/remotecontrol-080-dynamic-port-178e` @ `581ad1ac1b96dd6fd2271d2fa6948da00e8154a7`; `rideaudit-ui-font` still on pre-merge dual-phone @ `88c8714397a7ecf560f43c51f86d105a4f1e905e` (stale vs origin); also `rideaudit-caddy`, `rideaudit-caddy-tls`, `rideaudit-font` | OBS |
| Lab Caddy endpoints | `https://192.168.1.182:28443` (admission), `:28444` (counsel) on LAB-OMARCHY | OPS / PR #25 |

**Worktree note (OBS):** PR #28 original work used `F:\GitHub\rideaudit-rc080`. Base dual-phone tip after merge is on `origin/cursor/dual-phone-fold-moto-8aa2` @ `081e02e4e28cdf1a07054d407fd504e4806be750`. Local `rideaudit` is on a different branch; local `rideaudit-ui-font` dual-phone ref is behind origin until fetched/reset.

---

## Hard rules

(Already in operator profile. State once. Do not re-discover.)

1. Approved SVG wireframes = source of truth; do not edit or HV wireframes unless Payton names a defect.
2. Visual gate: controls / layout / style fidelity (FR-RIDE-076); pixels advisory.
3. When wrong: stop, present, wait - no invent / false-forward.
4. Assumption kill loop: observation vs inference; if inference changes approved work / path / done-state, stop and wait.
5. Requirements live in MCP, not GitHub PR #26 (custody only).
6. No FR/AC greens / PLAN done without receipts + HV (>=98% accuracy+completeness AGREE).
7. No Python in lab; no em/en dashes in authored text (ASCII hyphen `-` only).
8. Section 9 Astra/Payton boxes are not open Class C.
9. Be precise about which ART owns which chrome before changing layout.

---

## ART / chrome SoT

**Critical:** Prior agents scored/reshaped Fold phone UI against desktop WF-R SVGs and forced 2-col desktop chrome onto phone. Wrong SoT / mixed ARTs. Keep vertical packing for phone. Do not force desktop 2-col chrome onto phone.

| Surface | Artifact | Path | Chrome IDs |
| --- | --- | --- | --- |
| Mobile capture (phone path) | **ART-RIDE-UX-001** | `docs/ux/` | WF-01..08 (phone chrome) |
| Desktop reviewer | **ART-RIDE-UX-REVIEW-001** | `docs/ux/review-app/` | WF-R-01..08 |

OBS: `docs/ux/ARTIFACT.yaml` id `ART-RIDE-UX-001`; `docs/ux/review-app/ARTIFACT.yaml` id `ART-RIDE-UX-REVIEW-001`. Review ART notes it does not replace the mobile ART.

**WF-R is OUT OF SCOPE** for current dual-phone app work. Do not resume WF-R scoring on the phone path.

---

## Done

### PR #28 - Bind Android RemoteControl on port 0 with 0.8.0

- **OBS GitHub:** MERGED into `cursor/dual-phone-fold-moto-8aa2`. `merged_at` 2026-10-01T01:21:28Z. Merge commit `081e02e4e28cdf1a07054d407fd504e4806be750`. PR head tip before merge `581ad1ac1b96dd6fd2271d2fa6948da00e8154a7`. Base at merge was `88c8714397a7ecf560f43c51f86d105a4f1e905e`.
- **OBS receipts on dual-phone tip:**  
  - `docs/receipts/android/20261001T010611Z-remotecontrol-080-devices.md`  
  - `docs/receipts/android/20260930T201737Z-remotecontrol-080.md` (earlier host/no-phone restore)
- Fold + Edge Debug APK installed; AvaloniaRemote GetCapabilities OK both (per receipt / PR body). Host tests `RemoteBridgeMarkerTests|PhaseBGateTests|DeviceSerialChoiceTests`: 18 passed / 0 failed (per receipt / PR body).
- **Agent id note:** OPS handoff brief cited agent `bc-1abeee97-a36b-51a9-91fa-8954d2b680d3`. OBS PR #28 footer links agent `bc-3dea82cf-ef6d-5b9b-84db-35dd05e4178e`. Do not invent which was "the" agent; cite the PR footer for GitHub custody.
- No FR/AC marked satisfied by that work (stated in PR body and receipt).

### PR #27 - Fold cradle

- **OBS:** Squash-merged; merge commit `e7dd19b6ab8bc0d6b7eac37da74290a21a748428` (matches short `e7dd19b`). Still relevant as hardware CAD on master line; not phone UI chrome.

### OTS / PR #23

- **OBS GitHub:** PR #23 still **open** (not draft), head `cursor/live-ots-smoke-e5ce` @ `8550105b1355f719e5f0f2ce8980153bcd8d1dc9`, base master. Remains custody for live OTS smoke.
- **OBS disk:** Bitcoin height attestation path recorded in `docs/receipts/chain/20260930T161255Z-live-ots-bitcoin-recheck/result.json` with `BitcoinHeights: [969230]`, `BitcoinAttestationSeen: true`, `TransactionId: null`. OPS said recheck routine deleted; this pass did not re-verify script deletion beyond the receipt remaining on disk.

### Requirements (MCP, not re-greened here)

- OPS: FR-RIDE-075 / FR-RIDE-076 AGREEd in MCP; ACs unsatisfied.
- OPS: About FR-RIDE-074 (+ related) still Draft pending AGREE.
- This handoff does **not** mutate TODO done or FR greens.

### WF-R Fold re-score

- OPS: WF-R Fold re-score of tip `88c8714` was **KILLED** as out of scope (Payton 2026-09-30). Do not resume.

---

## Open work

1. **PR #24** draft - `cursor/dual-phone-fold-moto-8aa2` (now includes RemoteControl via #28). Dual-phone Fold+Edge visual + storyboard gate still open. PLAN-PR24FOLD-001 not done. Align to **mobile** WF-01..08 (ART-RIDE-UX-001), not WF-R.
   - Earlier score receipts (pre-#28 tip era) recorded local controls/layout/style often passing on WF-01..07 with WF-08 controls fail (live CAMERA_UNAVAILABLE sentence), frontier disagree, storyboards red, Edge similar. **Do not assert greens** from those notes without a fresh receipt against current tip `081e02e...`. HV on evidence report is not a visual agree.
2. **About view UI** not built (copyright + third-party attributions + bottom-panel button); FR-RIDE-074 Draft.
3. **Caddy live from app** on Fold + Edge to lab endpoints `https://192.168.1.182:28443` / `:28444`. **PR #25** remains draft until phone-app path is credible (host Caddy TLS work is separate custody).
4. Do **not** claim merge-ready on dual-phone until Avalonia on Fold/Edge clears visual/storyboard gates with receipts.

---

## Receipts index

| Receipt | Role |
| --- | --- |
| `docs/receipts/android/20261001T010611Z-remotecontrol-080-devices.md` | PR #28 both-phone RemoteControl 0.8.0 port 0 (on dual-phone tip after merge) |
| `docs/receipts/android/20260930T201737Z-remotecontrol-080.md` | Earlier host restore / no-phone package receipt |
| `docs/receipts/android/20260930T180300Z-fold-scored.md` | Fold scored catalog (pre-#28 era; do not treat as current tip green) |
| `docs/receipts/android/20260930T184947Z-edge-scored.md` | Edge scored catalog (pre-#28 era) |
| `docs/receipts/android/20260930T202334Z-review-hosted.md` | Hosted WF-R on capture client (WF-R path; killed for phone SoT work) |
| `docs/receipts/hostile-validator-20260930T191709Z.md` | HV AGREE on evidence report for PR #24 phase work (not visual green) |
| `docs/receipts/chain/20260930T161255Z-live-ots-bitcoin-recheck/result.json` | OTS Bitcoin height 969230 attestation seen |
| `docs/receipts/distribution/20260929T145508Z-caddy-edge-tls.md` | Lab Caddy TLS (PR #25) |

Paths for RemoteControl receipts: present on `origin/cursor/dual-phone-fold-moto-8aa2` @ `081e02e...`. May be absent from the currently checked-out `capture-operator-reqs` tree until that branch is checked out or the file is shown from origin.

---

## Branch / PR table

| PR | Title (short) | State (OBS) | Head / merge | Notes |
| --- | --- | --- | --- | --- |
| #28 | RemoteControl 0.8.0 port 0 | **merged** | merge `081e02e4e28cdf1a07054d407fd504e4806be750` into dual-phone | Device receipt on tip |
| #24 | Dual-phone Fold+Edge visuals | **open draft** | head `cursor/dual-phone-fold-moto-8aa2` @ `081e02e4e28cdf1a07054d407fd504e4806be750` | Visual/storyboard gates open; PLAN-PR24FOLD-001 open |
| #25 | Lab Caddy TLS | **open draft** | `cursor/caddy-edge-tls-f1c0` @ `a2e0d70bd0dba5868ef3cb637723bf608709eea5` | Host TLS; phone-app path still open |
| #23 | Live OTS smoke | **open** | `cursor/live-ots-smoke-e5ce` @ `8550105b1355f719e5f0f2ce8980153bcd8d1dc9` | Custody; height 969230 on disk receipt |
| #27 | Fold 4 cradle CAD | **merged** | `e7dd19b6ab8bc0d6b7eac37da74290a21a748428` | Hardware ART; not phone UI |

Repo: `https://github.com/sharpninja/rideaudit`

---

## Next steps for incoming agent

Do **not** start these unless the next operator explicitly assigns one:

1. Align capture UI to **mobile WF-01..08** (ART-RIDE-UX-001, not WF-R). Device receipts on Fold + Edge at tip `081e02e...`. Storyboards. HV only if asked / DoD.
2. Or Caddy from app on both phones to `:28443` / `:28444`.
3. Or About UI + FR-RIDE-074 AGREE path.

**Before layout changes:** name which ART owns the chrome. Phone = vertical packing under ART-RIDE-UX-001. Desktop review = ART-RIDE-UX-REVIEW-001 / WF-R (out of scope for phone path).

**Checkout hygiene:** For dual-phone work, use a tree on `cursor/dual-phone-fold-moto-8aa2` at `081e02e4e28cdf1a07054d407fd504e4806be750` (fetch first). Do not assume `F:\GitHub\rideaudit` HEAD is that tip today.

---

## MCP session note

Optional one-line session note via mcpserver-grok-plugin was **skipped**. Reason: this executor session's connected MCP catalog does not expose the grok-plugin session/note tools (plugin tree exists on disk at `F:\GitHub\mcpserver-grok-plugin`; transport config points at `http://localhost:7147/mcp-transport`). No TODO done / FR green mutations attempted.

---

## Verification appendix (this pass)

- Workspace root used: `F:\GitHub\rideaudit` (preferred path present and populated).
- No prior `HANDOFF.md` at root before write.
- `git fetch origin cursor/dual-phone-fold-moto-8aa2` then `origin/cursor/dual-phone-fold-moto-8aa2` = `081e02e4e28cdf1a07054d407fd504e4806be750`.
- Parents of tip: `88c8714397a7ecf560f43c51f86d105a4f1e905e` (pre-merge dual-phone) and `581ad1ac1b96dd6fd2271d2fa6948da00e8154a7` (PR #28 tip).
- PR states via GitHub API as listed in Branch/PR table.











