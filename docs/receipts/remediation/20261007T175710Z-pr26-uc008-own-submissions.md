# PR #26: UC-RIDE-008 own-submissions alignment (no merge)

Written: 2026-10-07 12:57:10 CT (America/Chicago) / 20261007T175710Z UTC.
Author: Grok Bot on PAYTON-LEGION2.
Parent head: `399ef35`.
Payton correction: own-submissions only / no roles already ordered; do not re-ask.

## Change

- `docs/Project/Use-Cases-Batch.yaml` UC-RIDE-008 `basicFlow` step 1:
  - Was: `Subject or authorized agent requests access or deletion.`
  - Now: `Authenticated driver (data subject) requests access or deletion of their own audit-held data.`
- Actors remain `Driver`. No RBAC / delegated-agent invent.
- `docs/ux/use-cases/UC-RIDE-008.md` Constraints aligned to caller==subject own-submissions.
- FR-RIDE-010 batch/wiki already subject self-service; no FR invent rewrite.
- Left `docs/source/lyft-telematics-audit-requirements.md` CCPA research Unverified note alone (not product SoT).

## Codex reply

Reply on discussion_r4209383140: UC text now matches caller==subject; no delegated auth invent per Payton order.

## Do not

Merge. Camera2/H.264 SEI invent. Re-ask Payton.