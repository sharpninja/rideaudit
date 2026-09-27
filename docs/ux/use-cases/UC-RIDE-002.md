# UC-RIDE-002: Record Smooth Cruiser evidence

**Author:** Sharp Ninja
**License:** GPL-2.0
**Source:** `docs/Project/Use-Cases-Batch.yaml`
**Actors field:** Driver, Auditor
**Realizes:** FR-RIDE-002, FR-RIDE-006

**Goal:** Capture Smooth Cruiser scores from export fields or consented manual/screenshot entry with provenance.

UML use case diagram. Notation is defined in [README.md](README.md). Session sequence diagrams under `docs/ux/flows/` are not a substitute for this diagram.

- Primary actors: Driver, Auditor
- Secondary actors: None.

```mermaid
%% UC-RIDE-002 Record Smooth Cruiser evidence
flowchart LR
  subgraph primaries["Primary actors"]
    direction TB
    A_Driver((Driver))
    A_Auditor((Auditor))
  end
  subgraph system["RideAudit"]
    direction TB
    UC(["UC-RIDE-002<br/>Record Smooth Cruiser evidence"])
    UC009(["UC-RIDE-009<br/>Seal-at-collect with chain receipt"])
    EXT(["Enter consented screenshot<br/>or manual values"])
    UC -.->|"«include»"| UC009
    EXT -.->|"«extend»<br/>structured fields absent"| UC
  end
  A_Driver --- UC
  A_Auditor --- UC
  A_Driver --- EXT
  A_Auditor --- EXT
```

## Relationships

- «include» UC-RIDE-009: basic flow step 4 seals and receipts the capture.
- «extend» manual or screenshot entry: basic flow step 3 applies only when structured Smooth Cruiser fields are absent. The actor supplies timestamp and source tag.

## Constraints

- Manual and screenshot values stay source-tagged. They are not relabeled as a Lyft API response.

## Unverified gaps

Presence of structured Smooth Cruiser fields in a privacy export is Unverified. The manual path exists so the audit can record consented values without inventing an export schema.

## Related

- Seal: [UC-RIDE-009](UC-RIDE-009.md).
