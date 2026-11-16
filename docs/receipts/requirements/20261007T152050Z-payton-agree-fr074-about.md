# Payton AGREE FR-RIDE-074 cluster (About)

Written: 2026-10-07 10:20 CT (America/Chicago) / 20261007T152050Z UTC.
Machine: PAYTON-LEGION2
Author: Grok Bot executor

## Authorization

Payton AGREE on FR-RIDE-074 + TR-RIDE-VIEW-007 + TEST-RIDE-055.
Convention: status stays pending; notes get `approval: Payton AGREE 2026-10-07`; descriptions/ACs unchanged; isSatisfied stays false. No About UI feature code.

## MCP

`workflow.requirements.updateBatch` success (req-20261007T151954Z-bb40), total 3.

## Get-back quotes

### FR-RIDE-074 (getFr req-20261007T151956Z-da42)
- title: About view holds copyright and third-party attributions
- description: The RideAudit UI shows its copyright and its third-party attributions (licenses and credits) on a dedicated About view. Copyright alone does not satisfy this requirement. The bottom panel includes an About control that opens that view. Copyright does not remain on the previous chrome location, the top title bar. GPL licensing of the code stays FR-RIDE-029. GPL notices on shared artifacts stay FR-RIDE-030. Source-file copyright headers stay in place.
- status: pending | priority: medium
- notes: Operator delta 2026-09-29, plus the same-day addon that About includes third-party attributions. approval: Payton AGREE 2026-10-07. Not part of the 2026-09-29 capture AGREE. Cross-links FR-RIDE-029 and FR-RIDE-030. Does not satisfy those FRs. Does not satisfy AC-UC-025-001. The Avalonia UI 12 framework label is not this copyright notice. Does not mark any AC satisfied.
- ACs 074-001..005: unchanged, all isSatisfied false

### TR-RIDE-VIEW-007 (getTr req-20261007T151957Z-db4d)
- title: About view with copyright and third-party attributions
- description: Add a dedicated About view to the shared Avalonia UI. Put an About control in the bottom panel that opens it. Show the UI copyright and the third-party attributions (licenses and credits) on that view. Copyright alone is not enough. Remove the copyright notice from the top title bar. Leave source-file copyright headers and artifact GPL notices in place.
- status: pending | priority: medium | subarea: VIEW
- notes: Implements FR-RIDE-074. Operator delta 2026-09-29. approval: Payton AGREE 2026-10-07. The current top-bar LicenseNotice is the previous chrome location. The Avalonia UI 12 framework label is not the copyright notice. Third-party attributions are licenses and credits shown on About. Does not mark any AC satisfied.
- ACs VIEW-007-001..004: unchanged, all isSatisfied false

### TEST-RIDE-055 (getTest req-20261007T151959Z-5b36)
- title: About shows copyright and attributions, not top-bar copyright
- description: Verify the bottom panel About control opens the About view, the About view shows the UI copyright and third-party attributions (licenses and credits), and the top title bar does not show that copyright. Copyright alone fails.
- status: pending | priority: medium
- notes: Covers FR-RIDE-074. Operator delta 2026-09-29. approval: Payton AGREE 2026-10-07. Does not satisfy FR-RIDE-029 or FR-RIDE-030. Does not mark any AC satisfied.
- ACs TEST-055-001..004: unchanged, all isSatisfied false

## Disk synced
- docs/Project/Additive-Operator-Capture-20260929-Batch.yaml — notes AGREE on FR-074 / TR-VIEW-007 / TEST-055
- docs/Project/Functional-Requirements-Batch.yaml — FR-030 pointer no longer says 074 stays Draft

## Not done
- No About UI feature code
- No commit
- ACs remain unsatisfied
