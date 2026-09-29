# About view (Draft)

Date: 2026-09-29T16:27:00Z. Revised: 2026-09-29T16:32:00Z. Host: PAYTON-LEGION2. Workspace: `F:\GitHub\rideaudit`.
SPDX: GPL-2.0-only.

Operator delta (Payton, 2026-09-29): move the UI copyright to a dedicated About view, and include third-party attributions (licenses and credits) on that view, not only copyright. Add an About control in the bottom panel that opens About. Copyright must not remain on the previous chrome location, the top title bar.

This row set is Draft until a separate Payton AGREE. It is not part of the 2026-09-29 capture AGREE in `docs/receipts/requirements/20260929T161600Z-payton-agree-capture.md`. Acceptance criteria stay unsatisfied. Section 9 is not open Class C work.

## IDs

- FR-RIDE-074
- UC-RIDE-044 (MCP useCaseId 171, approvalStatus Draft)
- TR-RIDE-VIEW-007
- TEST-RIDE-055

Cross-links: FR-RIDE-029 (GPL-2.0 licensing) and FR-RIDE-030 (GPL notices on artifacts). FR-RIDE-030 keeps Payton AGREE 2026-09-29. A pointer note on that row names FR-RIDE-074 and does not withdraw the AGREE. Source-file copyright headers and artifact GPL notices stay those requirements.

`workflow.requirements.createBatch` created FR-RIDE-074, TR-RIDE-VIEW-007, and TEST-RIDE-055 (`total: 3`). `client.UseCases.CreateAsync` created useCaseId 171 for UC-RIDE-044 and left it Draft. `createMapping` stored FR-RIDE-074 to TR-RIDE-VIEW-007 and TEST-RIDE-055. `getFr` FR-RIDE-074 has no satisfied acceptance criterion. `updateBatch` kept FR-RIDE-030 on Payton AGREE 2026-09-29 and added the FR-RIDE-074 pointer.

Addendum 2026-09-29T16:32:00Z: Payton added that the About view must include third-party attributions (licenses and credits), not only copyright. An About view that shows copyright only does not satisfy the Draft ACs. Updated FR-RIDE-074 AC-005, UC-RIDE-044 AC-002, TR-RIDE-VIEW-007 AC-004, and TEST-RIDE-055 AC-004. Still Draft until a separate AGREE. The 2026-09-29 capture AGREE is unchanged.

Addendum 2026-09-29T16:34:27Z: `workflow.requirements.updateBatch` updated FR-RIDE-074, TR-RIDE-VIEW-007, and TEST-RIDE-055 (`total: 3`, status pending, no satisfied acceptance criterion). `client.UseCases.UpdateAsync` updated useCaseId 171 title and brief and left `approvalStatus` Draft. Probe: satisfied-true count 0, third-party attributions present, `approvalStatus` Approved count 0.

## Non-claims

Showing copyright on About does not satisfy FR-RIDE-029, FR-RIDE-030, AC-UC-025-001, Public Trust, Caddy, live OpenTimestamps txid, or feature completion.
