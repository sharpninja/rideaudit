# PLAN-RIDEAUDIT-001-BRACKET — Headrest phone-mount (hardware)

**Plan ID:** PLAN-RIDEAUDIT-001-BRACKET  
**Revision:** r1 (scoped extract from PLAN-RIDEAUDIT-001 r3.2)  
**Kind:** Mechanical / OpenSCAD / STL physical mount  
**Artifact:** [ART-RIDE-MOUNT-001](../../artifacts/hardware/headrest-phone-mount/) (`artifacts/hardware/headrest-phone-mount/`)  
**Parent portfolio:** [PLAN-RIDEAUDIT-001-implementation.md](./PLAN-RIDEAUDIT-001-implementation.md)  
**Workspace:** `F:\GitHub\rideaudit` → https://github.com/sharpninja/rideaudit  
**Branch track:** `origin/master`  
**Author (git):** Sharp Ninja `<ninja@thesharp.ninja>`  
**Process:** Byrd Dev Process v4 (BDPv4)  
**Generator for this plan:** Grok (executor) — plan/docs split only  
**Created:** 2026-09-27 (America/Chicago)

> **Astra AGREE context:** Parent plan PLAN-RIDEAUDIT-001 holds Astra AGREE R7 (confidence/accuracy/completeness 98). This child plan is a **scoped extract** for portfolio division. It does **not** claim a separate Astra AGREE until reviewed (if process requires child-plan HV). No Avalonia/gRPC application implementation in this docs-only commit.

> **HARD GATE:** No Avalonia/gRPC application implementation, skeletons, application test projects, or generated application bindings until parent section 8 gate (P0 complete + Astra AGREE on reviewed revision + Payton AGREE). No waivers.


---

## 1. Scope

### 1.1 Problem / V²

RideAudit dual-phone capture needs a **physical dual-cradle headrest mount** that clamps to two vertical headrest posts and holds one or two phones in landscape. This plan owns the mechanical OpenSCAD/STL deliverable only — not Avalonia UI or gRPC services.

### 1.2 Goals

1. Parametric OpenSCAD source of truth under GPL-2.0 with exported STL.
2. Dimensions/BOM/print guidance sufficient for DIY fabrication.
3. BDPv4 FR → UC → AC → TEST links for the hardware slice (no orphan rows in this plan).
4. Explicit safety disclaimer: not crash-rated; airbag/clearance constraints documented.

### 1.3 Non-goals

- Android/desktop Avalonia implementation (see [PLAN-RIDEAUDIT-001-ANDROID](./PLAN-RIDEAUDIT-001-ANDROID.md)).
- Server/custody/API implementation (see [PLAN-RIDEAUDIT-001-SERVER](./PLAN-RIDEAUDIT-001-SERVER.md)).
- Claiming FR-RIDE-041 **software** ownership (Android plan is primary for capture software).
- Crash certification or OEM accessory claims.

### 1.4 Baseline

| Item | State |
| --- | --- |
| `headrest-phone-mount.scad` | Present (parametric) |
| `exports/headrest-phone-mount.stl` | Present |
| `bom.md` / `dimensions.md` / `README.md` / `ARTIFACT.yaml` | Present |
| Crash testing | Out of scope |

---

## 2. FR / UC / AC / TEST coverage (hardware slice)

**Primary software ownership note:** FR-RIDE-041 software remains with the Android plan. This plan owns **physical fit** acceptance that enables dual-phone landscape cradles for that FR, plus GPL notices on the mount package (FR-RIDE-029 / FR-RIDE-030).

| FR | Priority | Title | TR | TEST | UC | FR-owned ACs | Parent phase |
| --- | --- | --- | --- | --- | --- | --- | --- |
| FR-RIDE-029 | critical | GPL-2.0 licensing | TR-RIDE-GPL-001 | TEST-RIDE-020 | UC-RIDE-013 | AC-RIDE-029-001, AC-RIDE-029-002, AC-RIDE-029-003 | P1 |
| FR-RIDE-030 | high | GPL2 notices on artifacts | TR-RIDE-GPL-002 | TEST-RIDE-020 | UC-RIDE-013 | AC-RIDE-030-001, AC-RIDE-030-002 | P1 |
| FR-RIDE-041 | high | Dual-phone video capture | TR-RIDE-VIDEO-001 | TEST-RIDE-025 | UC-RIDE-017 | AC-RIDE-041-001, AC-RIDE-041-002 | P6 |

### 2.1 Linked records (closure)

| Kind | IDs |
| --- | --- |
| FR | FR-RIDE-029, FR-RIDE-030, FR-RIDE-041 |
| UC | UC-RIDE-013, UC-RIDE-017 |
| TR | TR-RIDE-GPL-001, TR-RIDE-VIDEO-001, TR-RIDE-GPL-002 |
| TEST | TEST-RIDE-020, TEST-RIDE-025 |
| FR-owned ACs | AC-RIDE-029-001, AC-RIDE-029-002, AC-RIDE-029-003, AC-RIDE-030-001, AC-RIDE-030-002, AC-RIDE-041-001, AC-RIDE-041-002 |

### 2.2 Hardware acceptance criteria (plan-local, linked)

These HW-ACs do not invent new FR IDs; each closes against the linked FR/TEST above.

| HW-AC | Links | Acceptance |
| --- | --- | --- |
| HW-AC-MOUNT-001 | FR-RIDE-041, UC-RIDE-017, TEST-RIDE-025 | Dual cradle holds two landscape phones within documented width/thickness params without blocking camera lenses (`camera_clearance`) |
| HW-AC-MOUNT-002 | FR-RIDE-041, TR-RIDE-VIDEO-001 | Post clamp spacing covers `post_spacing_min`..`post_spacing_max`; post diameter parametric |
| HW-AC-MOUNT-003 | FR-RIDE-029, FR-RIDE-030, TEST-RIDE-020 | LICENSE/NOTICE + GPL-2.0 headers present on `.scad`, README, BOM, ARTIFACT.yaml |
| HW-AC-MOUNT-004 | FR-RIDE-030 | STL export regenerable from `.scad`; `.scad` remains source of truth |
| HW-AC-MOUNT-005 | FR-RIDE-041 | README safety disclaimer: not crash-tested; airbag/head-restraint interference warnings |

No orphan HW-AC: each row links FR + (UC or TR) + TEST or explicit documentation TEST-RIDE-020 for licensing.

---

## 3. Phased hardware slices

| Phase | Goal | Depends | Exit |
| --- | --- | --- | --- |
| HW0 | Design freeze: params, dimensions.md, BOM, safety text | none | ARTIFACT.yaml relatedFrIds match this plan; disclaimer present |
| HW1 | Print validation on at least one vehicle + two phone sizes | HW0 | HW-AC-MOUNT-001/002 evidence photos or measurement log retained under `docs/receipts/` (or artifact notes) |
| HW2 | Release package: tagged STL + NOTICE audit | HW1 | HW-AC-MOUNT-003/004/005; opposing-model or operator check of notices |

**BDPv4 notes:** Hardware phases are documentation + fabrication validation. No application `src/` code. Product HV for software phases remains on Android/Server plans.

---

## 4. Artifact evolution

| Artifact | Today | Evolves into |
| --- | --- | --- |
| ART-RIDE-MOUNT-001 | Parametric OpenSCAD + STL | Remains independent hardware artifact; path and ID authoritative (parent §5) |

---

## 5. Risks

| ID | Risk | Mitigation |
| --- | --- | --- |
| B1 | Insecure fit / heat warping | PETG/ABS guidance; HW1 vehicle fit gate |
| B2 | Airbag interference | Clearance notes; user risk disclaimer |
| B3 | FR-041 ownership confusion | Software primary = ANDROID; Bracket = physical support only |

---

## 6. Acceptance of this child plan

- [ ] HW0 design freeze complete
- [ ] Child plan FR/UC/AC/TEST links complete (no orphans in slice)
- [ ] Optional: Astra/child HV if process requires (parent R7 AGREE does not automatically cover this extract)
- [ ] HW1/HW2 fabrication evidence before claiming mount “release”

**Sibling plans:** [ANDROID](./PLAN-RIDEAUDIT-001-ANDROID.md) · [SERVER](./PLAN-RIDEAUDIT-001-SERVER.md) · [Portfolio index](./PLAN-RIDEAUDIT-001-implementation.md)
