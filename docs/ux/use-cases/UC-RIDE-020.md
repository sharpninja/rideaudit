# UC-RIDE-020: Admin RBAC and partnership gates

**Author:** Sharp Ninja
**License:** GPL-2.0
**Source:** `docs/Project/Use-Cases-Batch.yaml`
**Actors field:** Admin
**Realizes:** FR-RIDE-011, FR-RIDE-012, FR-RIDE-014, FR-RIDE-202

**Goal:** Admin configures roles and Business API partnership status.

UML use case diagram. Notation is defined in [README.md](README.md). Session sequence diagrams under `docs/ux/flows/` are not a substitute for this diagram.

- Primary actors: Admin
- Secondary actors: None.

```mermaid
%% UC-RIDE-020 Admin RBAC and partnership gates
flowchart LR
  subgraph primaries["Primary actors"]
    direction TB
    A_Admin((Admin))
  end
  subgraph system["RideAudit"]
    direction TB
    UC(["UC-RIDE-020<br/>Admin RBAC and partnership gates"])
    UC003(["UC-RIDE-003<br/>Optional Concierge ride location poll"])
    UC003 -.->|"«extend»<br/>partnership approved"| UC
  end
  A_Admin --- UC
```

## Relationships

- «extend» UC-RIDE-003: basic flow step 3 enables Concierge features when the partnership is approved. The same arrow is on the UC-RIDE-003 diagram.
- Denied partnership leaves Concierge disabled. Role assignment with least privilege, including precise location views, is this use case (steps 1, 2, and 4).

## Constraints

- Setting partnership status is an admin gate in RideAudit. This use case does not poll Concierge and does not add Lyft endpoints.

## Unverified gaps

None specific to this use case beyond the project rule: do not invent Lyft private APIs, and keep source Unverified caveats visible where they apply.

## Related

- Optional poll: [UC-RIDE-003](UC-RIDE-003.md).
