# Concierge invent kill (requirement rows)

Written: 2026-10-07 11:55 CT (America/Chicago).
Machine: PAYTON-LEGION2
Branch: cursor/capture-operator-reqs-b19f (PR #26)
Accuracy rule: removed Concierge invent only; no invented replacement FR/UC/TR/TEST. FR-RIDE-011 kept.

## Payton order
Concierge was never approved / not in scope. Do not put removal on him. Kill leftover invent requirement rows. Code already removed at 2e0670e.

## MCP SoT kills (deleted / not_found)
| Id | Result |
|----|--------|
| FR-RIDE-004 | deleted |
| FR-RIDE-012 | deleted |
| FR-RIDE-204 | deleted (prior phase) |
| FR-RIDE-206 | deleted (prior phase) |
| TR-RIDE-INGEST-004 (Concierge OAuth and status poller) | deleted (prior phase) |
| TEST-RIDE-004 (Concierge optional poll and partnership gate) | deleted (prior phase) |
| TEST-RIDE-030 (Concierge resilience and accuracy labeling) | deleted (prior phase) |

## MCP strip (not delete)
| Id | Change |
|----|--------|
| TR-RIDE-INGEST-006 | Removed `lyft_concierge_api` from provenance enum. Remains: lyft_privacy_export \| in_app_manual \| third_party_telematics \| unverified. |

## Disk UC kills (usecase MCP not routed)
| Id | Action |
|----|--------|
| UC-RIDE-003 | Removed from Use-Cases-Batch.yaml; deleted docs/ux/use-cases/UC-RIDE-003.md (Concierge-only) |
| UC-RIDE-020 | Removed from Use-Cases-Batch.yaml; deleted docs/ux/use-cases/UC-RIDE-020.md (partnership/Concierge-only after prior RBAC strip) |

## Mappings
- Dropped mapping rows for FR-004 / FR-012 / FR-204 / FR-206.
- FR-RIDE-011 remapped: testIds [TEST-RIDE-001, TEST-RIDE-002]; useCaseLocalIds [] (was TEST-004 + UC-003/020 Concierge cites). FR-011 itself kept (no undocumented Lyft private APIs).

## Mixed-content strips (Concierge phrase only; remainder kept)
- UC-RIDE-005.md: removed Concierge status poll wording from signal-categories note.
- UC-RIDE-008.md: removed "Optional Concierge poll remains out of this DSAR use case" line.
- overview.md / use-cases README: Concierge / killed-id cites scrubbed.

## Additive / TEST-052 C# lab wording (finished)
MCP updateBatch records + disk Additive-Operator-Capture-20260929-Batch.yaml:
- FR-RIDE-072 / TEST-RIDE-052 / TR-RIDE-LAB-004: committed in-repo lab toolchain under artifacts/hardware; Python ban scoped there (not physical LAB-OMARCHY host languages); go-by-default PAYTON-DESKTOP + PAYTON-LEGION2.

## Wiki scrub
Functional / Technical / Testing / Requirements-Matrix / TR-per-FR-Mapping (github + azure): Concierge sections and killed-id rows/cites removed. generateDocument format=wiki succeeded (zip returned).

## Not in this commit
- No merge. No Camera2/H.264.
- Code path already gone at 2e0670e.

## Residual
Primary Project YAML + wiki + ux use-cases: 0 Concierge / killed-id hits after scrub.