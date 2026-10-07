# RideAudit UML use case diagrams

**Author:** Sharp Ninja
**License:** GPL-2.0

One UML use case diagram for every `UC-RIDE-*` id defined in `docs/Project/Use-Cases-Batch.yaml`.

## Counts

| Item | Count |
| --- | --- |
| Use cases defined in Project YAML | 31 (UC-RIDE-001 through UC-RIDE-031) |
| Proper UML use case diagrams already present before this set | 0 |
| Diagrams in this directory | 31, plus this index and the [overview](overview.md) |

## What was not counted as a use case diagram

These existing docs are session or review workflows. They do not show per-UC actors, a system boundary, and «include» / «extend» for a named use case:

| Path | Kind |
| --- | --- |
| [docs/ux/flows/mermaid-session-flow.md](../flows/mermaid-session-flow.md) | Dual-phone session sequence |
| [docs/ux/review-app/flows/mermaid-review-workflow.md](../review-app/flows/mermaid-review-workflow.md) | Desktop review sequence |
| [docs/ux/review-app/flows/review-workflow.md](../review-app/flows/review-workflow.md) | Desktop review workflow prose |

## Inventory source

All use case ids come from `docs/Project/Use-Cases-Batch.yaml`, merged across its three top-level `records:` sections:

| Section | Ids |
| --- | --- |
| Base use cases | UC-RIDE-001 through UC-RIDE-021 |
| Bluetooth driver-rider pairing | UC-RIDE-022 through UC-RIDE-024 |
| Avalonia UI 12 and gRPC .NET 10 | UC-RIDE-025 through UC-RIDE-031 |

`Additive-Bluetooth-Pairing-Batch.yaml` and `Additive-Avalonia-Grpc-Stack-Batch.yaml` add functional, technical, and test records. They do not define extra `UC-*` ids. The matching use cases are the records above.

## Notation

Mermaid has no native use-case element. These diagrams use flowchart shapes in UML use case notation:

- Circle: actor. Primary actors sit on the left. Secondary actors and external systems sit on the right.
- Stadium node: use case, inside the RideAudit boundary.
- Solid line: association between an actor and a use case.
- Dashed arrow labeled «include»: the source use case always includes the target.
- Dashed arrow labeled «extend»: the source adds behavior to the target only under the condition on the arrow.
- Thick arrow labeled generalizes: the source is a specialization of the target.

Include and extend are drawn only where that use case text states the reused or conditional behavior. A use case with only associations is still a complete use case diagram.

Do not read these as C4 context diagrams.

## Overview

Package view of every actor and every use case: [overview.md](overview.md).

## Index

| UC | Name | Actors field | Diagram | Relationships on the diagram |
| --- | --- | --- | --- | --- |
| UC-RIDE-001 | Ingest privacy-export ZIP | Driver, Auditor | [UC-RIDE-001.md](UC-RIDE-001.md) | 1 include |
| UC-RIDE-002 | Record Smooth Cruiser evidence | Driver, Auditor | [UC-RIDE-002.md](UC-RIDE-002.md) | 1 include, 1 extend |
| UC-RIDE-004 | Import third-party telematics | Driver, Auditor | [UC-RIDE-004.md](UC-RIDE-004.md) | 1 include |
| UC-RIDE-005 | Generate coverage matrix | Auditor, Counsel | [UC-RIDE-005.md](UC-RIDE-005.md) | associations only |
| UC-RIDE-006 | Online-hours policy check | Auditor | [UC-RIDE-006.md](UC-RIDE-006.md) | 1 extend |
| UC-RIDE-007 | Build incident time-window package | Auditor, Counsel | [UC-RIDE-007.md](UC-RIDE-007.md) | associations only |
| UC-RIDE-008 | Data subject access or deletion | Driver, Admin, Counsel | [UC-RIDE-008.md](UC-RIDE-008.md) | 1 include, 2 extend |
| UC-RIDE-009 | Seal-at-collect with chain receipt | Driver, PublicServer | [UC-RIDE-009.md](UC-RIDE-009.md) | 1 include, 1 extend |
| UC-RIDE-010 | Counsel verification and decrypt path | Counsel, Admin | [UC-RIDE-010.md](UC-RIDE-010.md) | 1 include |
| UC-RIDE-011 | Escrow key and court release | Admin, Counsel | [UC-RIDE-011.md](UC-RIDE-011.md) | 1 include |
| UC-RIDE-012 | Play Integrity gated collection | Driver | [UC-RIDE-012.md](UC-RIDE-012.md) | 1 extend |
| UC-RIDE-013 | GPL-2.0 publish and notice | Admin | [UC-RIDE-013.md](UC-RIDE-013.md) | associations only |
| UC-RIDE-014 | Driver self-registers on public server | Driver, PublicServer | [UC-RIDE-014.md](UC-RIDE-014.md) | associations only |
| UC-RIDE-015 | Submit sealed package to public server | Driver, PublicServer | [UC-RIDE-015.md](UC-RIDE-015.md) | 1 extend |
| UC-RIDE-016 | Counsel multi-driver bundle | Counsel, Admin | [UC-RIDE-016.md](UC-RIDE-016.md) | associations only |
| UC-RIDE-017 | Dual-phone composite evidence | Driver | [UC-RIDE-017.md](UC-RIDE-017.md) | 5 include, 1 extend |
| UC-RIDE-018 | Counsel composite playback | Counsel | [UC-RIDE-018.md](UC-RIDE-018.md) | 2 include, 1 extend |
| UC-RIDE-019 | Desktop court viewer review | Counsel, Auditor | [UC-RIDE-019.md](UC-RIDE-019.md) | 2 include, 1 extend |
| UC-RIDE-021 | Cross-cutting compliance and quality gates | Admin, Auditor, Counsel | [UC-RIDE-021.md](UC-RIDE-021.md) | 4 include |
| UC-RIDE-022 | Pair driver and passenger phones over Bluetooth | DriverPhone, PassengerPhone | [UC-RIDE-022.md](UC-RIDE-022.md) | 1 extend |
| UC-RIDE-023 | Driver coordinates dual-phone session | DriverPhone, PassengerPhone | [UC-RIDE-023.md](UC-RIDE-023.md) | 2 include |
| UC-RIDE-024 | Passenger syncs joins and overlays telematics | PassengerPhone | [UC-RIDE-024.md](UC-RIDE-024.md) | 1 include |
| UC-RIDE-025 | Capture ride evidence with Avalonia Android client | DriverPhone, PassengerPhone | [UC-RIDE-025.md](UC-RIDE-025.md) | 1 include, 1 generalization |
| UC-RIDE-026 | Review sealed bundle with Avalonia desktop viewer | Counsel, Auditor | [UC-RIDE-026.md](UC-RIDE-026.md) | 1 include, 1 generalization |
| UC-RIDE-027 | Reuse shared Avalonia UI under GPL-2.0 | Developer, Auditor | [UC-RIDE-027.md](UC-RIDE-027.md) | associations only |
| UC-RIDE-028 | Submit sealed package via gRPC .NET 10 | DriverPhone, PublicServer | [UC-RIDE-028.md](UC-RIDE-028.md) | 1 include, 1 generalization |
| UC-RIDE-029 | Consume published GPL-2.0 gRPC protos | Developer | [UC-RIDE-029.md](UC-RIDE-029.md) | associations only |
| UC-RIDE-030 | Fail-closed gRPC admission | DriverPhone, PublicServer | [UC-RIDE-030.md](UC-RIDE-030.md) | 1 extend |
| UC-RIDE-031 | Prefer gRPC over interim OpenAPI companion | Developer, Auditor | [UC-RIDE-031.md](UC-RIDE-031.md) | 1 include |

## House rules

- License for the in-scope application, schemas, and evidence-network components: GPL-2.0.
- Bluetooth pairing is RideAudit device pairing, not a Lyft Bluetooth API.
- Unverified gaps are labeled on the use case page when that use case depends on them.
- Custody receipt anchor default in architecture is Bitcoin OpenTimestamps (`btc-ots`). Diagrams say configured public chain, matching the use case text.
