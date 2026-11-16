# PR #26: Concierge / partnership gate removed (out of scope)

Date: 20261007T164200Z. Operator host: PAYTON-LEGION2. Branch: cursor/capture-operator-reqs-b19f.
SPDX: GPL-2.0-only. Not a merge. Not reviewer AGREE.

## Payton

Concierge / partnership gate is **not in scope at all**. Do not invent replacement RBAC. Do not rewrite AGREEd FR-004 / UC-003 / FR-012 YAML or MCP rows until Payton kills them explicitly.

## Code deleted (not stubbed)

- `IngestPipeline`: `PartnershipGate`, `SetPartnership`, `IngestConcierge`, `IConciergeStatusSource`, `NoNetworkConciergeSource`, `ConciergePoll`, `ConciergeUnavailable`, `PartnershipView`
- `ProvenanceTags.Concierge` (`lyft_concierge_api`)
- `ingest.proto` RPCs `SetPartnership` / `IngestConciergeStatus` and their messages; Contract-Version **0.3.0**
- `IngestGrpcService` overrides for those RPCs
- `AdmissionHost` `NoNetworkConciergeSource` wiring; `IngestPipeline` ctor is store/keys/clock only
- `AnalysisService` `concierge_location` signal
- Tests: `Concierge_stays_behind_the_partnership_gate_...`, `Ride_status_outage_...`, `ScriptedConcierge`
- `IngestSlice.Constraint` no longer names a partnership-gated connector

## Kept (accuracy)

- AcCoverageLedger deferral heuristics that mark Concierge/partnership ACs deferred while those requirement rows still exist
- Smooth-cruiser rejection fixture that uses the string `lyft_concierge_api` as a false source label (not a live Concierge path)
- `ApiGapNotice` still says coarse location is not a Smooth Cruiser score (FR-206 accuracy), without a Concierge product path

## Tests

- Workflow.Tests excluding pre-existing `Octopus_desktop_pointer_...`: **25 passed**
- Pre-existing fail (unchanged this turn): Octopus env name expects `Octopus-LAB-OMARCHY` vs receipt `Octopus-PAYTON-DESKTOP`

## STOP — requirement rows still name Concierge (not rewritten)

Payton must explicitly kill these before MCP/YAML AGREE scrub:

- **FR-RIDE-004** Optional Concierge/Business API integration (Functional-Requirements-Batch + wiki)
- **FR-RIDE-012** Admin partnership gates (batch + wiki)
- **FR-RIDE-204** Concierge ingestion resilience (batch + wiki)
- **FR-RIDE-206** Concierge lat/lng accuracy labeling (batch + wiki; Concierge-named text)
- **UC-RIDE-003** Optional Concierge ride location poll (Use-Cases-Batch + docs/ux/use-cases/UC-RIDE-003.md)
- **UC-RIDE-020** Admin partnership gates (Use-Cases-Batch; UX overview / UC-003 extend)
- **TR** Concierge OAuth and status poller + provenance enum `lyft_concierge_api` (Technical-Requirements-Batch)
- **TEST** Concierge optional poll / resilience titles (Testing-Requirements-Batch + wiki Testing)

## Not done

- No AGREEd YAML/MCP rewrite for the rows above
- No RBAC / admin role invent
- No Camera2 / H.264 SEI / telematics
- No merge