# PLAN-RIDEAUDIT-001-ANDROID — Avalonia clients (Android + desktop review)

**Plan ID:** PLAN-RIDEAUDIT-001-ANDROID  
**Revision:** r1 (scoped extract from PLAN-RIDEAUDIT-001 r3.2)  
**Kind:** Clients — Avalonia UI 12 Android dual-phone + desktop court/counsel review  
**Artifacts:** [ART-RIDE-ANDROID-001](../../artifacts/android/), [ART-RIDE-UX-001](../ux/), [ART-RIDE-UX-REVIEW-001](../ux/review-app/)  
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

RideAudit clients: (1) **Android dual-phone** capture (driver coordinator + passenger compositor over Bluetooth), (2) **desktop court/counsel** review app with fail-closed verification before decrypt. Stack: **Avalonia UI 12**. Desktop review lives in this Clients plan as a sibling Avalonia client (not under Bracket hardware; not under Server containers).

### 1.2 Goals

1. BDPv4-complete client slices with FR → UC → AC → TR → TEST for every owned FR.
2. Seal-at-collect and Play Integrity on device; BT roles per architecture docs.
3. Desktop fail-closed viewer consuming Server custody/escrow APIs.
4. No app code until parent section 8 gate.

### 1.3 Non-goals

- Headrest mount fabrication (see [BRACKET](./PLAN-RIDEAUDIT-001-BRACKET.md)).
- gRPC admission containers, chain writers, ingest pipelines (see [SERVER](./PLAN-RIDEAUDIT-001-SERVER.md)).
- Lyft private APIs; claiming Play publication complete without receipts.

### 1.4 Baseline

| Area | State |
| --- | --- |
| Avalonia Android / Desktop `src/` | Absent (gate) |
| `artifacts/android/` | Historical Kotlin scaffold only |
| UX mobile / review-app | Present under `docs/ux/` |

---

## 2. FR coverage matrix (client-owned)

| FR | Priority | Title | TR | TEST | UC | FR-owned ACs | Parent phase |
| --- | --- | --- | --- | --- | --- | --- | --- |
| FR-RIDE-015 | critical | Seal and encrypt at collection | TR-RIDE-STORE-001, TR-RIDE-STORE-002, TR-RIDE-SEAL-001 | TEST-RIDE-013 | UC-RIDE-009 | AC-RIDE-015-001, AC-RIDE-015-002 | P3 |
| FR-RIDE-016 | critical | Per-session or per-sample keys | TR-RIDE-SEAL-002 | TEST-RIDE-013 | UC-RIDE-009 | AC-RIDE-016-001, AC-RIDE-016-002, AC-RIDE-016-003 | P3 |
| FR-RIDE-025 | critical | Play Integrity key binding | TR-RIDE-PLAY-001 | TEST-RIDE-019 | UC-RIDE-012 | AC-RIDE-025-001, AC-RIDE-025-002 | P4 |
| FR-RIDE-026 | critical | Reject failed Play Integrity | TR-RIDE-PLAY-001, TR-RIDE-PLAY-003 | TEST-RIDE-019 | UC-RIDE-012 | AC-RIDE-026-001, AC-RIDE-026-002, AC-RIDE-026-003 | P4 |
| FR-RIDE-027 | critical | Attestation on custody receipt | TR-RIDE-CHAIN-001, TR-RIDE-PLAY-002 | TEST-RIDE-019 | UC-RIDE-012 | AC-RIDE-027-001, AC-RIDE-027-002 | P4 |
| FR-RIDE-029 | critical | GPL-2.0 licensing | TR-RIDE-GPL-001 | TEST-RIDE-020 | UC-RIDE-013 | AC-RIDE-029-001, AC-RIDE-029-002, AC-RIDE-029-003 | P1 |
| FR-RIDE-030 | high | GPL2 notices on artifacts | TR-RIDE-GPL-002 | TEST-RIDE-020 | UC-RIDE-013 | AC-RIDE-030-001, AC-RIDE-030-002 | P1 |
| FR-RIDE-031 | high | Publish client via Play and source repo | TR-RIDE-GPL-003 | TEST-RIDE-020 | UC-RIDE-013, UC-RIDE-014 | AC-RIDE-031-001, AC-RIDE-031-002 | P11b |
| FR-RIDE-041 | high | Dual-phone video capture | TR-RIDE-VIDEO-001 | TEST-RIDE-025 | UC-RIDE-017 | AC-RIDE-041-001, AC-RIDE-041-002 | P6 |
| FR-RIDE-042 | high | Shared session clock sync | TR-RIDE-VIDEO-002 | TEST-RIDE-025 | UC-RIDE-017 | AC-RIDE-042-001, AC-RIDE-042-002 | P6 |
| FR-RIDE-043 | high | On-device/edge compositing | TR-RIDE-VIDEO-003 | TEST-RIDE-025 | UC-RIDE-017 | AC-RIDE-043-001, AC-RIDE-043-002 | P6 |
| FR-RIDE-044 | high | Spider-graph overlay | TR-RIDE-VIDEO-004 | TEST-RIDE-025 | UC-RIDE-017 | AC-RIDE-044-001, AC-RIDE-044-002 | P6 |
| FR-RIDE-045 | critical | Seal composite as first-class evidence | TR-RIDE-VIDEO-005 | TEST-RIDE-026 | UC-RIDE-017 | AC-RIDE-045-001, AC-RIDE-045-002 | P6 |
| FR-RIDE-046 | medium | Optional raw stream sealing | TR-RIDE-VIDEO-003 | TEST-RIDE-026 | UC-RIDE-017 | AC-RIDE-046-001, AC-RIDE-046-002 | P6 |
| FR-RIDE-047 | critical | Counsel composite playback | TR-RIDE-VIEW-002 | TEST-RIDE-027 | UC-RIDE-018 | AC-RIDE-047-001, AC-RIDE-047-002 | P7 |
| FR-RIDE-048 | high | Composite metadata in custody package | TR-RIDE-VIDEO-004 | TEST-RIDE-026 | UC-RIDE-017, UC-RIDE-018 | AC-RIDE-048-001, AC-RIDE-048-002 | P6 |
| FR-RIDE-049 | critical | GPL-2.0 desktop court viewer | TR-RIDE-VIEW-001 | TEST-RIDE-028 | UC-RIDE-019 | AC-RIDE-049-001, AC-RIDE-049-002 | P7 |
| FR-RIDE-050 | critical | Viewer fail-closed verification | TR-RIDE-VIEW-002 | TEST-RIDE-028 | UC-RIDE-018, UC-RIDE-019 | AC-RIDE-050-001, AC-RIDE-050-002 | P7 |
| FR-RIDE-051 | high | Synchronized timeline display | TR-RIDE-VIEW-003 | TEST-RIDE-028 | UC-RIDE-019 | AC-RIDE-051-001, AC-RIDE-051-002 | P7 |
| FR-RIDE-052 | critical | ViewerSession and VerificationReport | TR-RIDE-VIEW-004 | TEST-RIDE-028 | UC-RIDE-019 | AC-RIDE-052-001, AC-RIDE-052-002 | P7 |
| FR-RIDE-053 | high | Bluetooth driver-rider phone pairing | TR-RIDE-VIDEO-010 | TEST-RIDE-034 | UC-RIDE-022 | AC-RIDE-053-001, AC-RIDE-053-002 | P6 |
| FR-RIDE-054 | high | Driver phone session coordination | TR-RIDE-VIDEO-011 | TEST-RIDE-034 | UC-RIDE-023 | AC-RIDE-054-001, AC-RIDE-054-002 | P6 |
| FR-RIDE-055 | high | Passenger phone video sync join and telematics overlay | TR-RIDE-VIDEO-011 | TEST-RIDE-034 | UC-RIDE-024 | AC-RIDE-055-001, AC-RIDE-055-002 | P6 |
| FR-RIDE-056 | high | Avalonia UI 12 Android dual-phone capture client | TR-RIDE-VIDEO-012 | TEST-RIDE-035 | UC-RIDE-025 | AC-RIDE-056-001, AC-RIDE-056-002 | P6 |
| FR-RIDE-057 | high | Avalonia UI 12 desktop court viewer | TR-RIDE-VIEW-005 | TEST-RIDE-035 | UC-RIDE-026 | AC-RIDE-057-001, AC-RIDE-057-002 | P7 |
| FR-RIDE-058 | high | Shared Avalonia UI 12 constraints under GPL-2.0 | TR-RIDE-GPL-004 | TEST-RIDE-035 | UC-RIDE-027 | AC-RIDE-058-001, AC-RIDE-058-002 | P7 |
| FR-RIDE-215 | critical | Play authenticity allowlist | TR-RIDE-PLAY-001, TR-RIDE-PLAY-003 | TEST-RIDE-019 | UC-RIDE-012 | AC-RIDE-215-001, AC-RIDE-215-002, AC-RIDE-215-003 | P4 |
| FR-RIDE-219 | high | Video storage and bandwidth quotas | TR-RIDE-VIDEO-006 | TEST-RIDE-033 | UC-RIDE-017 | AC-RIDE-219-001, AC-RIDE-219-002, AC-RIDE-219-003 | P6 |
| FR-RIDE-220 | high | Video performance thresholds | TR-RIDE-VIDEO-006, TR-RIDE-PERF-003 | TEST-RIDE-033 | UC-RIDE-017 | AC-RIDE-220-001, AC-RIDE-220-002 | P6 |
| FR-RIDE-221 | high | Composite integrity for playback | TR-RIDE-VIDEO-001, TR-RIDE-VIDEO-002 | TEST-RIDE-027 | UC-RIDE-018 | AC-RIDE-221-001, AC-RIDE-221-002 | P7 |
| FR-RIDE-222 | critical | Desktop portability fail-closed | TR-RIDE-VIEW-001 | TEST-RIDE-028 | UC-RIDE-019 | AC-RIDE-222-001, AC-RIDE-222-002 | P7 |

### 2.1 Linked record sets

| Kind | Count | IDs |
| --- | ---: | --- |
| FR | 31 | FR-RIDE-015, FR-RIDE-016, FR-RIDE-025, FR-RIDE-026, FR-RIDE-027, FR-RIDE-029, FR-RIDE-030, FR-RIDE-031, FR-RIDE-041, FR-RIDE-042, FR-RIDE-043, FR-RIDE-044, FR-RIDE-045, FR-RIDE-046, FR-RIDE-047, FR-RIDE-048, FR-RIDE-049, FR-RIDE-050, FR-RIDE-051, FR-RIDE-052, FR-RIDE-053, FR-RIDE-054, FR-RIDE-055, FR-RIDE-056, FR-RIDE-057, FR-RIDE-058, FR-RIDE-215, FR-RIDE-219, FR-RIDE-220, FR-RIDE-221, FR-RIDE-222 |
| UC | 13 | UC-RIDE-009, UC-RIDE-012, UC-RIDE-013, UC-RIDE-014, UC-RIDE-017, UC-RIDE-018, UC-RIDE-019, UC-RIDE-022, UC-RIDE-023, UC-RIDE-024, UC-RIDE-025, UC-RIDE-026, UC-RIDE-027 |
| TR | 27 | TR-RIDE-CHAIN-001, TR-RIDE-GPL-001, TR-RIDE-PLAY-001, TR-RIDE-SEAL-001, TR-RIDE-STORE-001, TR-RIDE-VIDEO-001, TR-RIDE-VIEW-001, TR-RIDE-GPL-002, TR-RIDE-PLAY-002, TR-RIDE-SEAL-002, TR-RIDE-STORE-002, TR-RIDE-VIDEO-002, TR-RIDE-VIEW-002, TR-RIDE-GPL-003, TR-RIDE-PERF-003, TR-RIDE-PLAY-003, TR-RIDE-VIDEO-003, TR-RIDE-VIEW-003, TR-RIDE-GPL-004, TR-RIDE-VIDEO-004, TR-RIDE-VIEW-004, TR-RIDE-VIDEO-005, TR-RIDE-VIEW-005, TR-RIDE-VIDEO-006, TR-RIDE-VIDEO-010, TR-RIDE-VIDEO-011, TR-RIDE-VIDEO-012 |
| TEST | 10 | TEST-RIDE-013, TEST-RIDE-019, TEST-RIDE-020, TEST-RIDE-025, TEST-RIDE-026, TEST-RIDE-027, TEST-RIDE-028, TEST-RIDE-033, TEST-RIDE-034, TEST-RIDE-035 |
| FR-owned ACs | 67 | (enumerated in matrix column; full ledger parent §2.7) |

**Shared / cross-plan:** FR-RIDE-029/030 also apply to Server and Bracket artifacts. FR-RIDE-026 server reject path is enforced in [SERVER](./PLAN-RIDEAUDIT-001-SERVER.md) admission; client owns device-side fail-closed. FR-RIDE-041 physical mount support is [BRACKET](./PLAN-RIDEAUDIT-001-BRACKET.md). Full AC ledger remains in parent §2.7; this plan owns implementation of the FR rows above.

**No orphan FRs in this slice:** every FR row carries TR, TEST, UC, and FR-owned AC IDs from the parent matrix.

---

## 3. Phased client slices

Parent phase mapping: A2↔P3/P4 client · A3↔P6 · A4↔P7 · A5↔P11b client.

### A0 — Client docs / UX freeze (docs-only)

**Goal:** Confirm mobile + review-app UX artifacts and stack pointers; no app code.

| Field | Value |
| --- | --- |
| FR IDs | (no implementation ownership; references Android slice FRs) |
| Files | `docs/ux/`, `docs/ux/review-app/`, `artifacts/android/ARTIFACT.yaml`, `docs/architecture/stack.md`, `docs/architecture/dual-phone-bluetooth-roles.md` |
| Exit | UX IDs ART-RIDE-UX-001 / ART-RIDE-UX-REVIEW-001 stable; historical Kotlin marked non-target |

### A1 — Shared Avalonia / client project layout (after parent section 8 gate)

**Goal:** Tests-first client solution layout for Android + desktop + shared UI; archive historical Kotlin scaffold.

| Field | Value |
| --- | --- |
| FR IDs | FR-RIDE-029, FR-RIDE-030, FR-RIDE-058 |
| UC / TR / TEST | UC-RIDE-013, UC-RIDE-027; TR-RIDE-GPL-001/002/004; TEST-RIDE-020, TEST-RIDE-035 |
| Files | `src/RideAudit.Client.Android/`, `src/RideAudit.Client.Desktop/`, `src/RideAudit.Shared.Ui/`; `artifacts/android/legacy-kotlin/` archive |
| Depends | Parent P0 + section 8 gate; Server P1 protos available for client stubs |
| Exit | Contract/UI shell tests Failed 0 Skipped 0 for partition; no Lyft private APIs |

### A2 — Seal-at-collect + Play Integrity on device (maps parent P3/P4 client scope)

**Goal:** Device-boundary seal + Play Integrity binding before key/seal; attestation fields for custody receipt.

| Field | Value |
| --- | --- |
| FR IDs | FR-RIDE-015, FR-RIDE-016, FR-RIDE-025, FR-RIDE-026, FR-RIDE-027, FR-RIDE-215 |
| UC / TR / TEST | UC-RIDE-009, UC-RIDE-012; TR-RIDE-SEAL-001/002, TR-RIDE-PLAY-001/002/003; TEST-RIDE-013, TEST-RIDE-019 |
| Depends | A1; Server seal/store + admission contracts |
| Exit | TEST-RIDE-013/019 client partitions Failed 0 Skipped 0; fail closed on Play failure |

### A3 — Bluetooth dual-phone + Avalonia Android capture (parent P6)

**Goal:** BT pairing; driver coordinator; passenger compositor; Avalonia UI 12 Android.

| Field | Value |
| --- | --- |
| FR IDs | FR-RIDE-041, FR-RIDE-042, FR-RIDE-043, FR-RIDE-044, FR-RIDE-045, FR-RIDE-046, FR-RIDE-048, FR-RIDE-053, FR-RIDE-054, FR-RIDE-055, FR-RIDE-056, FR-RIDE-219, FR-RIDE-220 |
| UC IDs | UC-RIDE-017, UC-RIDE-018, UC-RIDE-022, UC-RIDE-023, UC-RIDE-024, UC-RIDE-025 |
| TR IDs | TR-RIDE-VIDEO-001..006, TR-RIDE-VIDEO-010..012, TR-RIDE-PERF-003 |
| TEST IDs | TEST-RIDE-025, TEST-RIDE-026, TEST-RIDE-033, TEST-RIDE-034, TEST-RIDE-035 |
| FR-owned ACs | AC-RIDE-041-001, AC-RIDE-041-002, AC-RIDE-042-001, AC-RIDE-042-002, AC-RIDE-043-001, AC-RIDE-043-002, AC-RIDE-044-001, AC-RIDE-044-002, AC-RIDE-045-001, AC-RIDE-045-002, AC-RIDE-046-001, AC-RIDE-046-002, AC-RIDE-048-001, AC-RIDE-048-002, AC-RIDE-053-001, AC-RIDE-053-002, AC-RIDE-054-001, AC-RIDE-054-002, AC-RIDE-055-001, AC-RIDE-055-002, AC-RIDE-056-001, AC-RIDE-056-002, AC-RIDE-219-001, AC-RIDE-219-002, AC-RIDE-219-003, AC-RIDE-220-001, AC-RIDE-220-002 |
| Files | `src/RideAudit.Client.Android/`, `src/RideAudit.Bt/`, `src/RideAudit.Video/`, `artifacts/android/` |
| Depends | A2; Server admission (parent P2); Bracket mount available for field capture (HW1 recommended) |
| Exit | TEST-RIDE-025/026/033/034 Failed 0 Skipped 0; TEST-RIDE-035 Android+shared partition; HV AGREE |

### A4 — Avalonia desktop court/counsel viewer (parent P7)

**Goal:** Fail-closed verification, escrow working-copy decrypt UX, synchronized timeline, ViewerSession/VerificationReport.

| Field | Value |
| --- | --- |
| FR IDs | FR-RIDE-047, FR-RIDE-049, FR-RIDE-050, FR-RIDE-051, FR-RIDE-052, FR-RIDE-057, FR-RIDE-058, FR-RIDE-221, FR-RIDE-222 |
| UC IDs | UC-RIDE-018, UC-RIDE-019, UC-RIDE-026, UC-RIDE-027 |
| TR IDs | TR-RIDE-VIEW-001..005, TR-RIDE-GPL-004 |
| TEST IDs | TEST-RIDE-027, TEST-RIDE-028, TEST-RIDE-035 |
| FR-owned ACs | AC-RIDE-047-001, AC-RIDE-047-002, AC-RIDE-049-001, AC-RIDE-049-002, AC-RIDE-050-001, AC-RIDE-050-002, AC-RIDE-051-001, AC-RIDE-051-002, AC-RIDE-052-001, AC-RIDE-052-002, AC-RIDE-057-001, AC-RIDE-057-002, AC-RIDE-058-001, AC-RIDE-058-002, AC-RIDE-221-001, AC-RIDE-221-002, AC-RIDE-222-001, AC-RIDE-222-002 |
| Files | `src/RideAudit.Client.Desktop/`, `src/RideAudit.Shared.Ui/`, `docs/ux/review-app/` |
| Depends | A3; Server escrow + custody verify adapters (parent P3–P5) |
| Exit | TEST-RIDE-027/028 Failed 0 Skipped 0; TEST-RIDE-035 desktop partition; fail-closed before decrypt; HV AGREE |

### A5 — Client distribution (parent P11b client portion)

**Goal:** Play + source publication receipts for client; signed desktop builds Win/Linux/macOS contribution.

| Field | Value |
| --- | --- |
| FR IDs | FR-RIDE-031 |
| UC / TR / TEST | UC-RIDE-013, UC-RIDE-014; TR-RIDE-GPL-003; TEST-RIDE-020 |
| Depends | A3, A4; portfolio P11b integrated gate |
| Exit | Play+source receipts retained; desktop signed/reproducible build evidence |

---

## 4. Architecture (clients)

| Surface | Project | FR | Notes |
| --- | --- | --- | --- |
| Android dual-phone | `RideAudit.Client.Android` | FR-RIDE-056, 053–055, 041–048 | Driver + passenger |
| Desktop court/counsel | `RideAudit.Client.Desktop` | FR-RIDE-057, 049–052 | Fail-closed before decrypt |
| Shared UI | `RideAudit.Shared.Ui` | FR-RIDE-058 | GPL-2.0 shared Avalonia |

BT roles: `docs/architecture/dual-phone-bluetooth-roles.md`. Stack: `docs/architecture/stack.md`.

---

## 5. Artifact evolution

| Artifact | Today | Evolves into |
| --- | --- | --- |
| ART-RIDE-ANDROID-001 | Kotlin/Gradle historical | P6/A3 → `src/RideAudit.Client.Android/`; archive legacy-kotlin |
| ART-RIDE-UX-001 | Mobile UX | Acceptance refs for A3 |
| ART-RIDE-UX-REVIEW-001 | Desktop review UX | Acceptance refs for A4 |

---

## 6. HV / risks / gate

- Product HV per `docs/process/hostile-validation.md` (opposing model) after each construction phase.
- Risks: Kotlin scaffold mistaken for target; BT/Lyft confusion; Play attest unavailable — record truthfully.
- Parent section 8 gate binds this plan.

## 7. Acceptance

- [ ] Parent P0 + Astra R7 context acknowledged; child Astra only if required
- [ ] Payton AGREE on portfolio before A1 app code
- [ ] A3/A4 HV AGREE + suites Failed 0 Skipped 0 for partitions

**Sibling plans:** [BRACKET](./PLAN-RIDEAUDIT-001-BRACKET.md) · [SERVER](./PLAN-RIDEAUDIT-001-SERVER.md) · [Portfolio index](./PLAN-RIDEAUDIT-001-implementation.md)
