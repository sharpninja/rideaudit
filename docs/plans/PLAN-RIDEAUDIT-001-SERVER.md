# PLAN-RIDEAUDIT-001-SERVER — gRPC .NET 10 backend, custody, APIs

> **Concierge invent KILLED 2026-10-07 (Payton):** FR-004/012/204/206, UC-003/020, TR-INGEST-004, TEST-004/030 and their ACs are deleted from SoT. Rows below that still name those IDs are historical scrub targets marked KILLED; do not implement.
> **OBSOLETE-CITES-SCRUB 2026-10-07:** Kill-list invent cites (mask/RBAC/legal-hold/FR-014/202/203/208/210, TR-PRIV-002/SEC-002/SEC-003, TEST-012/032) marked obsolete. Replacements where AGREEd: **FR-RIDE-077** / **TR-RIDE-VIDEO-007** (unmasked + H.264 SEI); **FR-RIDE-078** / **TR-RIDE-PRIV-004** (driver not third-party retention). Do not implement killed invent. Legitimate court/counsel product FRs (e.g. FR-047) unchanged.


**Plan ID:** PLAN-RIDEAUDIT-001-SERVER  
**Revision:** r1.1 — cite FR-RIDE-063 Octopus CD to LAB-OMARCHY; no GHCR  
**Kind:** Backend — gRPC on .NET 10 containers, custody anchoring, sealed admission, ingest, escrow, counsel APIs  
**Artifact:** [ART-RIDE-API-001](../../artifacts/server-api/)  
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

Public sealed-only admission, custody receipts (Bitcoin OTS primary), escrow M-of-N, counsel multi-driver APIs, ingest pipelines, privacy/retention — all on **gRPC .NET 10 containers**. Clients are out of scope here (see Android plan).

### 1.2 Goals

1. Authoritative `.proto` contracts (OpenAPI companion non-authoritative).
2. Fail-closed sealed admission; no decrypt at public ingest.
3. OTS custody anchoring (+ optional L2 dual-anchor profiles).
4. BDPv4 FR → UC → AC → TR → TEST for every server-owned FR.
5. Container CD via Octopus Deploy to LAB-OMARCHY (FR-RIDE-063). No GHCR.

### 1.3 Non-goals

- Avalonia Android/desktop UI (Android plan).
- Headrest mount (Bracket plan).
- Treating interim OpenAPI as wire truth.
- Private/permissioned chain as sole custody ledger.
- GHCR or GitHub Actions container registry as the image distribution path.
- Treating Octopus license exhaustion as deferral or out of scope.
- Inventing a live Octopus green without a named instance/container and LAB-OMARCHY receipt.

---

## 2. FR coverage matrix (server-owned)

| FR | Priority | Title | TR | TEST | UC | FR-owned ACs | Parent phase |
| --- | --- | --- | --- | --- | --- | --- | --- |
| FR-RIDE-001 | high | Ingest Lyft privacy-export ZIP | TR-RIDE-INGEST-001, TR-RIDE-INGEST-006 | TEST-RIDE-001 | UC-RIDE-001 | AC-RIDE-001-001, AC-RIDE-001-002, AC-RIDE-001-003 | P9 |
| FR-RIDE-002 | high | Record Smooth Cruiser scores | TR-RIDE-INGEST-002 | TEST-RIDE-002 | UC-RIDE-002 | AC-RIDE-002-001, AC-RIDE-002-002, AC-RIDE-002-003 | P9 |
| FR-RIDE-003 | high | Ingest trip-level records | TR-RIDE-INGEST-003 | TEST-RIDE-003 | UC-RIDE-001 | AC-RIDE-003-001, AC-RIDE-003-002 | P9 |
| FR-RIDE-004 | KILLED | Concierge invent killed 2026-10-07 | - | - | - |
| FR-RIDE-005 | high | Import third-party telematics | TR-RIDE-INGEST-005, TR-RIDE-INGEST-006 | TEST-RIDE-005 | UC-RIDE-004 | AC-RIDE-005-001, AC-RIDE-005-002, AC-RIDE-005-003 | P9 |
| FR-RIDE-006 | critical | Provenance and consent ledger | TR-RIDE-INGEST-006, TR-RIDE-PRIV-001 | TEST-RIDE-006 | UC-RIDE-001, UC-RIDE-002, UC-RIDE-004 | AC-RIDE-006-001, AC-RIDE-006-002 | P9 |
| FR-RIDE-007 | high | Coverage matrix | TR-RIDE-ANAL-002 | TEST-RIDE-007 | UC-RIDE-005 | AC-RIDE-007-001, AC-RIDE-007-002 | P8 |
| FR-RIDE-008 | high | Online-hours policy audit | TR-RIDE-ANAL-003 | TEST-RIDE-008 | UC-RIDE-006 | AC-RIDE-008-001, AC-RIDE-008-002, AC-RIDE-008-003 | P8 |
| FR-RIDE-009 | high | Time-window incident report | TR-RIDE-ANAL-004 | TEST-RIDE-009 | UC-RIDE-007 | AC-RIDE-009-001, AC-RIDE-009-002 | P8 |
| FR-RIDE-010 | critical | Data subject access and deletion | TR-RIDE-STORE-003, TR-RIDE-PRIV-001, TR-RIDE-PRIV-003 | TEST-RIDE-010 | UC-RIDE-008 | AC-RIDE-010-001 (AC-002 legal-hold killed) | P10 |
| FR-RIDE-011 | critical | No undocumented Lyft private APIs | TR-RIDE-INGEST-001, TR-RIDE-INGEST-002 | TEST-RIDE-004 | UC-RIDE-020 | AC-RIDE-011-001, AC-RIDE-011-002 | P9 |
| FR-RIDE-012 | KILLED | Concierge invent killed 2026-10-07 | - | - | - |
| FR-RIDE-013 | high | Hash and version raw imports | TR-RIDE-STORE-001, TR-RIDE-STORE-002 | TEST-RIDE-011 | UC-RIDE-001 | AC-RIDE-013-001, AC-RIDE-013-002 | P9 |
| FR-RIDE-014 | ~~critical~~ | **OBSOLETE (killed invent)** Role-based access / RBAC — see **FR-RIDE-077** | ~~TR-PRIV-002/SEC-002~~ | ~~TEST-012~~ | UC-RIDE-020 | OBSOLETE | P10 |
| FR-RIDE-017 | critical | Custody receipt content | TR-RIDE-CHAIN-001 | TEST-RIDE-014 | UC-RIDE-009 | AC-RIDE-017-001, AC-RIDE-017-002 | P3 |
| FR-RIDE-018 | critical | Blockchain receipt write | TR-RIDE-CHAIN-002 | TEST-RIDE-014 | UC-RIDE-009 | AC-RIDE-018-001, AC-RIDE-018-002, AC-RIDE-018-003 | P3 |
| FR-RIDE-019 | critical | Blockchain write failure policy | TR-RIDE-CHAIN-003 | TEST-RIDE-015 | UC-RIDE-009 | AC-RIDE-019-001, AC-RIDE-019-002, AC-RIDE-019-003 | P3 |
| FR-RIDE-020 | critical | Court-review decryption path docs | TR-RIDE-ANAL-001 | TEST-RIDE-016 | UC-RIDE-010 | AC-RIDE-020-001, AC-RIDE-020-002, AC-RIDE-020-003 | P8 |
| FR-RIDE-021 | critical | Verification UI/report | TR-RIDE-ANAL-001, TR-RIDE-CHAIN-004 | TEST-RIDE-016 | UC-RIDE-010 | AC-RIDE-021-001, AC-RIDE-021-002, AC-RIDE-021-003 | P8 |
| FR-RIDE-022 | critical | M-of-N key escrow | TR-RIDE-ESCROW-001 | TEST-RIDE-017 | UC-RIDE-011 | AC-RIDE-022-001, AC-RIDE-022-002 | P5 |
| FR-RIDE-023 | critical | Escrow separation from device | TR-RIDE-ESCROW-001, TR-RIDE-ESCROW-003 | TEST-RIDE-017 | UC-RIDE-011 | AC-RIDE-023-001, AC-RIDE-023-002, AC-RIDE-023-003 | P5 |
| FR-RIDE-024 | critical | Court-authorized escrow release | TR-RIDE-ESCROW-002 | TEST-RIDE-018 | UC-RIDE-011 | AC-RIDE-024-001, AC-RIDE-024-002, AC-RIDE-024-003 | P5 |
| FR-RIDE-026 | critical | Reject failed Play Integrity | TR-RIDE-PLAY-001, TR-RIDE-PLAY-003 | TEST-RIDE-019 | UC-RIDE-012 | AC-RIDE-026-001, AC-RIDE-026-002, AC-RIDE-026-003 | P4 |
| FR-RIDE-028 | critical | Counsel verification steps | TR-RIDE-CHAIN-004, TR-RIDE-ESCROW-002 | TEST-RIDE-018 | UC-RIDE-010, UC-RIDE-019 | AC-RIDE-028-001, AC-RIDE-028-002 | P5 |
| FR-RIDE-029 | critical | GPL-2.0 licensing | TR-RIDE-GPL-001 | TEST-RIDE-020 | UC-RIDE-013 | AC-RIDE-029-001, AC-RIDE-029-002, AC-RIDE-029-003 | P1 |
| FR-RIDE-030 | high | GPL2 notices on artifacts | TR-RIDE-GPL-002 | TEST-RIDE-020 | UC-RIDE-013 | AC-RIDE-030-001, AC-RIDE-030-002 | P1 |
| FR-RIDE-032 | critical | Public-server driver account | TR-RIDE-SERVER-001 | TEST-RIDE-021 | UC-RIDE-014 | AC-RIDE-032-001, AC-RIDE-032-002, AC-RIDE-032-003 | P2 |
| FR-RIDE-033 | high | Vehicle registration | TR-RIDE-SERVER-002 | TEST-RIDE-021 | UC-RIDE-014 | AC-RIDE-033-001, AC-RIDE-033-002 | P2 |
| FR-RIDE-034 | high | Configuration profile gate | TR-RIDE-SERVER-002 | TEST-RIDE-021 | UC-RIDE-014 | AC-RIDE-034-001, AC-RIDE-034-002 | P2 |
| FR-RIDE-035 | critical | Sealed-only submission API | TR-RIDE-SERVER-003 | TEST-RIDE-022 | UC-RIDE-015 | AC-RIDE-035-001, AC-RIDE-035-002 | P2 |
| FR-RIDE-036 | critical | Admission verification chain | TR-RIDE-SERVER-004 | TEST-RIDE-022 | UC-RIDE-015 | AC-RIDE-036-001, AC-RIDE-036-002 | P2 |
| FR-RIDE-037 | critical | Multi-driver per-record provenance | TR-RIDE-SERVER-007 | TEST-RIDE-023 | UC-RIDE-015, UC-RIDE-016 | AC-RIDE-037-001, AC-RIDE-037-002 | P8 |
| FR-RIDE-038 | high | Counsel multi-driver bundle | TR-RIDE-SERVER-007 | TEST-RIDE-023 | UC-RIDE-007, UC-RIDE-016 | AC-RIDE-038-001, AC-RIDE-038-002 | P8 |
| FR-RIDE-039 | critical | Public-server abuse controls | TR-RIDE-SERVER-004, TR-RIDE-SERVER-005 | TEST-RIDE-024 | UC-RIDE-015 | AC-RIDE-039-001, AC-RIDE-039-002, AC-RIDE-039-003 | P2 |
| FR-RIDE-040 | critical | Multi-tenant isolation | TR-RIDE-SERVER-006 | TEST-RIDE-024 | UC-RIDE-015 | AC-RIDE-040-001, AC-RIDE-040-002 | P2 |
| FR-RIDE-059 | critical | Backend gRPC on .NET 10 containers | TR-RIDE-SERVER-008 | TEST-RIDE-036 | UC-RIDE-028 | AC-RIDE-059-001, AC-RIDE-059-002 | P2 |
| FR-RIDE-060 | high | Proto and schema publication under GPL-2.0 | TR-RIDE-GPL-005 | TEST-RIDE-037 | UC-RIDE-029 | AC-RIDE-060-001, AC-RIDE-060-002 | P1 |
| FR-RIDE-061 | critical | Fail-closed admission over gRPC | TR-RIDE-SERVER-009 | TEST-RIDE-036 | UC-RIDE-030 | AC-RIDE-061-001, AC-RIDE-061-002 | P2 |
| FR-RIDE-062 | high | Interim OpenAPI companion non-authoritative | TR-RIDE-SERVER-010 | TEST-RIDE-037 | UC-RIDE-031 | AC-RIDE-062-001, AC-RIDE-062-002 | P1 |
| FR-RIDE-201 | critical | TLS and secrets vault | TR-RIDE-SEC-001 | TEST-RIDE-029 | UC-RIDE-009 | AC-RIDE-201-001, AC-RIDE-201-002 | P2 |
| FR-RIDE-202 | ~~critical~~ | **OBSOLETE (killed invent)** Geolocation sensitive masking — precise location stays unmasked; see **FR-RIDE-077** / **TR-RIDE-VIDEO-007** | ~~TR-PRIV-002~~ | ~~TEST-012~~ | UC-RIDE-008 | OBSOLETE | P10 |
| FR-RIDE-203 | ~~critical~~ | **OBSOLETE (killed invent)** Append-only access logs (mask/RBAC cluster) — no AGREEd replacement yet | ~~TR-SEC-003~~ | TEST-RIDE-029 | UC-RIDE-008 | OBSOLETE | P10 |
| FR-RIDE-204 | KILLED | Concierge invent killed 2026-10-07 | - | - | - |
| FR-RIDE-205 | high | Scale multi-year histories | TR-RIDE-PERF-002 | TEST-RIDE-031 | UC-RIDE-021 | AC-RIDE-205-001, AC-RIDE-205-002 | P10 |
| FR-RIDE-206 | KILLED | Concierge invent killed 2026-10-07 | - | - | - |
| FR-RIDE-207 | high | Portable audit ZIP export | TR-RIDE-STORE-001, TR-RIDE-STORE-002 | TEST-RIDE-031 | UC-RIDE-007, UC-RIDE-021 | AC-RIDE-207-001 | P10 |
| FR-RIDE-208 | ~~high~~ | **OBSOLETE (killed invent)** Per-state / CA third-party retention frames — see **FR-RIDE-078** / **TR-RIDE-PRIV-004** | TR-RIDE-STORE-003 | ~~TEST-032~~ | UC-RIDE-008 | OBSOLETE | P10 |
| FR-RIDE-209 | medium | In-product API gap notice | TR-RIDE-ANAL-002 | TEST-RIDE-007 | UC-RIDE-005 | AC-RIDE-209-001 | P9 |
| FR-RIDE-210 | ~~critical~~ | **OBSOLETE (killed invent)** Legal hold suspends deletion — no legal-hold SoT | TR-RIDE-STORE-003, TR-RIDE-PRIV-003 | TEST-RIDE-010 | UC-RIDE-008 | OBSOLETE | P10 |
| FR-RIDE-211 | high | Cryptographic agility | TR-RIDE-SEAL-003 | TEST-RIDE-032 | UC-RIDE-009 | AC-RIDE-211-001, AC-RIDE-211-002 | P3 |
| FR-RIDE-212 | high | Configurable public chain | TR-RIDE-CHAIN-002 | TEST-RIDE-014 | UC-RIDE-009 | AC-RIDE-212-001, AC-RIDE-212-002, AC-RIDE-212-003 | P3 (OTS default) + P11a (alternate profiles) |
| FR-RIDE-213 | high | Seal/receipt latency budget | TR-RIDE-PERF-001 | TEST-RIDE-032 | UC-RIDE-009 | AC-RIDE-213-001, AC-RIDE-213-002 | P3 |
| FR-RIDE-214 | critical | HSM/KMS key custody | TR-RIDE-ESCROW-003 | TEST-RIDE-029 | UC-RIDE-010, UC-RIDE-011 | AC-RIDE-214-001, AC-RIDE-214-002 | P5 |
| FR-RIDE-216 | critical | Escrow resilience | TR-RIDE-ESCROW-001 | TEST-RIDE-017 | UC-RIDE-011 | AC-RIDE-216-001, AC-RIDE-216-002 | P5 |
| FR-RIDE-217 | critical | GPL-2.0 governance NFR | TR-RIDE-GPL-001 | TEST-RIDE-020 | UC-RIDE-013 | AC-RIDE-217-001, AC-RIDE-217-002 | P11b |
| FR-RIDE-218 | critical | Public-server admission capacity | TR-RIDE-SERVER-005 | TEST-RIDE-024 | UC-RIDE-015 | AC-RIDE-218-001, AC-RIDE-218-002 | P2 |
| FR-RIDE-063 | high | Octopus Deploy CD to LAB-OMARCHY | TR-RIDE-DEPLOY-001, TR-RIDE-DEPLOY-002 | TEST-RIDE-038, TEST-RIDE-040 | UC-RIDE-032 | AC-RIDE-063-001, AC-RIDE-063-002, AC-RIDE-063-003 | P11b |
| FR-RIDE-064 | high | ngrok ingress for RideAudit service | TR-RIDE-EDGE-001 | TEST-RIDE-039, TEST-RIDE-040 | UC-RIDE-033 | AC-RIDE-064-001, AC-RIDE-064-002, AC-RIDE-064-003 | P11b |

### 2.1 Linked record sets

| Kind | Count | IDs |
| --- | ---: | --- |
| FR | 58 | FR-RIDE-001, FR-RIDE-002, FR-RIDE-003, FR-RIDE-005, FR-RIDE-006, FR-RIDE-007, FR-RIDE-008, FR-RIDE-009, FR-RIDE-010, FR-RIDE-011, FR-RIDE-013, ~~FR-RIDE-014~~, FR-RIDE-017, FR-RIDE-018, FR-RIDE-019, FR-RIDE-020, FR-RIDE-021, FR-RIDE-022, FR-RIDE-023, FR-RIDE-024, FR-RIDE-026, FR-RIDE-028, FR-RIDE-029, FR-RIDE-030, FR-RIDE-032, FR-RIDE-033, FR-RIDE-034, FR-RIDE-035, FR-RIDE-036, FR-RIDE-037, FR-RIDE-038, FR-RIDE-039, FR-RIDE-040, FR-RIDE-059, FR-RIDE-060, FR-RIDE-061, FR-RIDE-062, FR-RIDE-063, FR-RIDE-064, FR-RIDE-201, ~~FR-RIDE-202~~, ~~FR-RIDE-203~~, FR-RIDE-205, FR-RIDE-207, ~~FR-RIDE-208~~, FR-RIDE-209, ~~FR-RIDE-210~~, FR-RIDE-211, FR-RIDE-212, FR-RIDE-213, FR-RIDE-214, FR-RIDE-216, FR-RIDE-217, FR-RIDE-218 |
| UC | 25 | UC-RIDE-001, UC-RIDE-002, UC-RIDE-004, UC-RIDE-005, UC-RIDE-006, UC-RIDE-007, UC-RIDE-008, UC-RIDE-009, UC-RIDE-010, UC-RIDE-011, UC-RIDE-012, UC-RIDE-013, UC-RIDE-014, UC-RIDE-015, UC-RIDE-016, UC-RIDE-019, UC-RIDE-021, UC-RIDE-028, UC-RIDE-029, UC-RIDE-030, UC-RIDE-031, UC-RIDE-032, UC-RIDE-033 |
| TR | 47 | TR-RIDE-ANAL-001, TR-RIDE-CHAIN-001, TR-RIDE-ESCROW-001, TR-RIDE-GPL-001, TR-RIDE-INGEST-001, TR-RIDE-PERF-001, TR-RIDE-PLAY-001, TR-RIDE-PRIV-001, TR-RIDE-SEC-001, TR-RIDE-SERVER-001, TR-RIDE-STORE-001, TR-RIDE-ANAL-002, TR-RIDE-CHAIN-002, TR-RIDE-ESCROW-002, TR-RIDE-GPL-002, TR-RIDE-INGEST-002, TR-RIDE-PERF-002, ~~TR-RIDE-PRIV-002~~, ~~TR-RIDE-SEC-002~~, TR-RIDE-SERVER-002, TR-RIDE-STORE-002, TR-RIDE-ANAL-003, TR-RIDE-CHAIN-003, TR-RIDE-ESCROW-003, TR-RIDE-INGEST-003, TR-RIDE-PLAY-003, TR-RIDE-PRIV-003, TR-RIDE-SEAL-003, ~~TR-RIDE-SEC-003~~, TR-RIDE-SERVER-003, TR-RIDE-STORE-003, TR-RIDE-ANAL-004, TR-RIDE-CHAIN-004, TR-RIDE-SERVER-004, TR-RIDE-GPL-005, TR-RIDE-INGEST-005, TR-RIDE-SERVER-005, TR-RIDE-INGEST-006, TR-RIDE-SERVER-006, TR-RIDE-SERVER-007, TR-RIDE-SERVER-008, TR-RIDE-SERVER-009, TR-RIDE-SERVER-010, TR-RIDE-DEPLOY-001, TR-RIDE-DEPLOY-002, TR-RIDE-EDGE-001 |
| TEST | 32 | TEST-RIDE-001, TEST-RIDE-002, TEST-RIDE-003, TEST-RIDE-005, TEST-RIDE-006, TEST-RIDE-007, TEST-RIDE-008, TEST-RIDE-009, TEST-RIDE-010, TEST-RIDE-011, ~~TEST-RIDE-012~~, TEST-RIDE-014, TEST-RIDE-015, TEST-RIDE-016, TEST-RIDE-017, TEST-RIDE-018, TEST-RIDE-019, TEST-RIDE-020, TEST-RIDE-021, TEST-RIDE-022, TEST-RIDE-023, TEST-RIDE-024, TEST-RIDE-029, TEST-RIDE-031, ~~TEST-RIDE-032~~, TEST-RIDE-036, TEST-RIDE-037, TEST-RIDE-038, TEST-RIDE-039, TEST-RIDE-040 |
| FR-owned ACs | 130 | (see matrix; planning ledger parent §2.7; post-planning ACs parent §2.8) |

**Cross-plan:** Client seal-at-collect (FR-015/016) and Play device binding (FR-025/027/215) live in Android; this plan owns store/admission/chain/escrow and **FR-026 reject-at-admission**. FR-029/030 notices apply to server packages. Desktop counsel UX is Android A4; counsel **service** containers are this plan (P8/S5).

**No orphan FRs in this slice:** every FR row carries TR, TEST, UC, and FR-owned AC IDs.

---

## 3. Phased server slices (parent P1–P5, P8–P11)

### S1 — Protos + skeleton (parent P1)

| Field | Value |
| --- | --- |
| FR IDs | FR-RIDE-029, FR-RIDE-030, FR-RIDE-060, FR-RIDE-062 |
| UC / TR / TEST | UC-RIDE-013, UC-RIDE-029, UC-RIDE-031; TR-RIDE-GPL-001/002/005, TR-RIDE-SERVER-010; TEST-RIDE-020, TEST-RIDE-037 |
| FR-owned ACs | AC-RIDE-029-001, AC-RIDE-029-002, AC-RIDE-029-003, AC-RIDE-030-001, AC-RIDE-030-002, AC-RIDE-060-001, AC-RIDE-060-002, AC-RIDE-062-001, AC-RIDE-062-002 |
| Files | `RideAudit.sln`, `src/RideAudit.Protos/`, `src/RideAudit.Contracts/`, `artifacts/server-api/` |
| Depends | Parent P0 + section 8 gate |
| Exit | Protos compile; TEST-RIDE-037 companion-authority Failed 0 Skipped 0 |

### S2 — gRPC admission (parent P2)

| Field | Value |
| --- | --- |
| FR IDs | FR-RIDE-032, FR-RIDE-033, FR-RIDE-034, FR-RIDE-035, FR-RIDE-036, FR-RIDE-039, FR-RIDE-040, FR-RIDE-059, FR-RIDE-061, FR-RIDE-201, FR-RIDE-218, FR-RIDE-026 |
| UC / TEST | UC-RIDE-014, UC-RIDE-015, UC-RIDE-028, UC-RIDE-030; TEST-RIDE-021, TEST-RIDE-022, TEST-RIDE-024, TEST-RIDE-036, TEST-RIDE-029, TEST-RIDE-019 |
| Files | `src/RideAudit.Server.Admission/`, `src/RideAudit.Server.Identity/`, `deploy/containers/admission/` |
| Depends | S1 |
| Exit | Fail-closed integrity miss; real custody acceptance after S3–S4 |

### S3 — Seal store + custody receipt + OTS (parent P3)

| Field | Value |
| --- | --- |
| FR IDs | FR-RIDE-017, FR-RIDE-018, FR-RIDE-019, FR-RIDE-211, FR-RIDE-212, FR-RIDE-213 (+ store support for client FR-015/016 ciphertext) |
| UC / TEST | UC-RIDE-009; TEST-RIDE-014, TEST-RIDE-015, TEST-RIDE-032 |
| Files | `src/RideAudit.Seal/`, `src/RideAudit.Chain/`, `src/RideAudit.Chain.OpenTimestamps/` |
| Depends | S1 |
| Exit | OTS primary `btc-ots`; non-admission on chain failure |

### S4 — Escrow M-of-N (parent P5)

| Field | Value |
| --- | --- |
| FR IDs | FR-RIDE-022, FR-RIDE-023, FR-RIDE-024, FR-RIDE-028, FR-RIDE-214, FR-RIDE-216 |
| UC / TEST | UC-RIDE-010, UC-RIDE-011, UC-RIDE-019; TEST-RIDE-017, TEST-RIDE-018 |
| Files | `src/RideAudit.Escrow/` |
| Depends | S3; Play attestation available from Android A2 |
| Exit | Court-authorized release into expiring working copy |

### S5 — Counsel APIs + analysis (parent P8)

| Field | Value |
| --- | --- |
| FR IDs | FR-RIDE-007, FR-RIDE-008, FR-RIDE-009, FR-RIDE-020, FR-RIDE-021, FR-RIDE-037, FR-RIDE-038 |
| UC / TEST | UC-RIDE-005..007, UC-RIDE-010, UC-RIDE-015, UC-RIDE-016; TEST-RIDE-007..009, TEST-RIDE-016, TEST-RIDE-023, TEST-RIDE-036 |
| Files | `src/RideAudit.Server.Counsel/`, `src/RideAudit.Anal/`, `deploy/containers/counsel/` |
| Depends | S2, S4; Android A4 for viewer consumption |
| Exit | Counsel container evidence for FR-059/TEST-036 partition |

### S6 — Ingest pipelines (parent P9)

| Field | Value |
| --- | --- |
| FR IDs | FR-RIDE-001, FR-RIDE-002, FR-RIDE-003, FR-RIDE-005, FR-RIDE-006, FR-RIDE-011, FR-RIDE-013, FR-RIDE-209 |
| TEST | TEST-RIDE-001..006, TEST-RIDE-011, TEST-RIDE-007 |
| Files | `src/RideAudit.Ingest/` |
| Depends | S2–S4 |
| Exit | No undocumented Lyft private APIs; working-copy-only analysis path |

### S7 — Privacy / RBAC / retention (parent P10)

| Field | Value |
| --- | --- |
| FR IDs | FR-RIDE-010, FR-RIDE-205, FR-RIDE-207, **FR-RIDE-077**, **FR-RIDE-078** (~~014/202/203/208/210 killed~~) |
| TEST | TEST-RIDE-010, TEST-RIDE-029, TEST-RIDE-031 (~~012/032 killed~~) |
| Files | `src/RideAudit.Privacy/`, `src/RideAudit.Sec/` |
| Depends | S2, S5, S6 |
| Exit | DSAR access-export + retention (**FR-078**) partitions green (~~legal hold killed~~) |

### S8 — Alternate chain profiles (parent P11a)

| Field | Value |
| --- | --- |
| FR IDs | FR-RIDE-212 (alternate profiles) |
| TEST | TEST-RIDE-014 |
| Files | `src/RideAudit.Chain.EthL2/`, `docs/architecture/blockchain-custody-receipts.md` |
| Depends | S3; Android A3 for end-to-end profile proof |
| Exit | Each supported profile Failed 0 Skipped 0 |

### S9 — Integrated acceptance + Octopus CD (parent P11b server portion)

| Field | Value |
| --- | --- |
| FR IDs | FR-RIDE-217, FR-RIDE-063, FR-RIDE-064 (+ server contribution to FR-031 portfolio gate) |
| TEST | TEST-RIDE-020, TEST-RIDE-038, TEST-RIDE-039, TEST-RIDE-040 |
| Depends | S1–S8 + Android A3/A4 + Bracket HW2 as applicable |
| Exit | Full suite Failed 0 Skipped 0; Octopus built and deployed images to LAB-OMARCHY with a receipt that names the instance or container and target and does not claim GHCR (FR-RIDE-063). If the default Octopus container is out of licenses, a new Octopus container on LAB-OMARCHY is the fallback, not GHCR. Recorded path: `octopus-rideaudit`, admission `192.168.1.182:28080`, counsel `192.168.1.182:28081` (`20260929T015822Z-octopus-payton-desktop.md`). Canonical ngrok target is that admission bind (FR-RIDE-064). Omarchy `127.0.0.1:18080` is the prior interim. This exit is not closed: full-suite, Play, and per-phase HV remain open. |

**Authoritative dependencies** (from parent): P1→P0+gate; P2→P1; P3→P1; P4 client on Android; P5→P3+P4; P8→P2+P5+P7; P9→P2..P5; P10→P2+P8+P9; P11a→P3+P6; P11b→all.

---

## 4. Architecture (backend)

| Service | Responsibility | FR |
| --- | --- | --- |
| Admission | Sealed-only submit; verify; no decrypt | FR-RIDE-035, 036, 059, 061 |
| Identity / vehicle | Driver account, vehicle, profiles | FR-RIDE-032–034 |
| Counsel | Multi-driver bundle, disclosure | FR-RIDE-037–038 |
| Chain writer | OTS primary; optional L2 | FR-RIDE-018, 212 |

Custody contracts: parent §4.5 (immutable receipt-core, OTS semantics, fail-closed states) remain binding before S1 implementation — see portfolio index §4.5.

Container CD: parent §4.6. Use Octopus Deploy. Build containers and deploy to LAB-OMARCHY. If you are out of licenses on the default container, create a new Octopus container on LAB-OMARCHY. Do not use GHCR. Cite FR-RIDE-063. Do not weaken it.

---

## 5. Artifact evolution

| Artifact | Today | Evolves into |
| --- | --- | --- |
| ART-RIDE-API-001 | Interim OpenAPI companion | S1: `src/RideAudit.Protos/` authoritative; OpenAPI labeled companion (FR-062) |

---

## 6. HV / risks / gate

- Product HV opposing-model AGREE per phase; retain `docs/receipts/hv/` + `docs/reviews/hv-pairs/`.
- Risks: OpenAPI-as-truth; decrypt at ingest; fabricated OTS tx metadata; gpt-6-sol unavailable — record truthfully.
- Parent section 8 gate binds this plan.

## 7. Acceptance

- [ ] Parent P0 + Astra R7 context acknowledged; child Astra only if required
- [ ] Payton AGREE on portfolio before S1 app code
- [ ] Phase HV AGREE + Failed 0 Skipped 0 for partitions

These boxes remain historically unchecked. Class C: they need Payton agreement and per-phase opposing HV, which this closeout does not invent. Payton 2026-09-28 authorized iterate-until-HV-agree remediation after code-hv-sol-r1 DISAGREE; code-hv-sol-r4 AGREE is the narrow CODE-HV gate only and is not a backdated S1–S9 HV pass. S9 Octopus receipt is on file; S9 full-suite exit stays open.

**Sibling plans:** [BRACKET](./PLAN-RIDEAUDIT-001-BRACKET.md) · [ANDROID](./PLAN-RIDEAUDIT-001-ANDROID.md) · [Portfolio index](./PLAN-RIDEAUDIT-001-implementation.md)
