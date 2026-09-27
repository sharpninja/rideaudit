# PLAN-RIDEAUDIT-001 — RideAudit implementation plan (BDPv4)

**Plan ID:** PLAN-RIDEAUDIT-001  
**Revision:** r3.2 (post Astra r6 BR6 residual)  
**Workspace:** `F:\GitHub\rideaudit` → https://github.com/sharpninja/rideaudit  
**Branch track:** `origin/master`  
**Author (git):** Sharp Ninja `<ninja@thesharp.ninja>`  
**Process:** Byrd Dev Process v4 (BDPv4)  
**Generator for this plan:** Grok (executor) — plan/docs only  
**Hostile plan reviewer:** Codex / **gpt-6-astra** at **xhigh**  
**Status:** REVISED r3 pending Astra re-review AGREE + Payton AGREE  
**Created:** 2026-09-27 (America/Chicago)  
**Prior HV:** r1/r2 DISAGREE@0 (OS206); r3 DISAGREE@78; r4 DISAGREE@94. This revision applies r4 path_to_98.

> **HARD GATE (section 8):** No Avalonia/gRPC application implementation, skeletons, application test projects, or generated application bindings until P0 docs repair is complete, Astra returns READY/AGREE with accuracy/completeness/confidence ≥98 on this revision, and Payton explicitly agrees. No waivers.

---

## 1. Scope, goals, non-goals, baseline

### 1.1 Problem / V²

RideAudit is a rideshare telematics audit system: dual-phone capture, seal-at-collect evidence, public blockchain custody receipts, fail-closed court/counsel review, sealed-only public admission. Stack locked: **Avalonia UI 12** (Android dual-phone + desktop court/counsel) and **gRPC on .NET 10 containers**. License: **GPL-2.0**. V² = true.

### 1.2 Goals

1. BDPv4-complete phased plan with FR ↔ UC ↔ AC ↔ TR ↔ TEST for every slice, including TR/TEST/UC-owned ACs.
2. Align architecture to locked stack, BT roles (FR-RIDE-053–055), OTS custody primary + optional ETH L2.
3. Evolve artifact packages into real code only after section 8 gate.
4. Opposing-model HV with durable receipts under `docs/receipts/hv/` **and** canonical pairs under `docs/reviews/hv-pairs/`.
5. Keep implementers from writing app code until Astra + Payton AGREE on this revision.

### 1.3 Non-goals

- Implementing Avalonia/gRPC/BT/video/chain writers before section 8 gate.
- McpServer wiki triage / plugin handoff.
- Inventing Lyft private APIs.
- Claiming Play Store publication complete without receipts.
- Committing secrets, `AGENTS-README-FIRST.yaml`, or `mcp.db`.
- Treating interim OpenAPI as authoritative wire contract.
- Private/permissioned chain as sole custody ledger.
- Reconstructing existing UC YAML records from Markdown (preserve YAML flows/ACs).

### 1.4 Current baseline

| Area | State |
| --- | --- |
| App source (`src/`) | Absent |
| Requirements YAML | FR 84, TR 64, TEST 37, mappings 84/84 |
| UC YAML | **31 unique UC-RIDE-001..031** under one `records:` key (duplicate keys at former lines 2/564/606 consolidated in P0 repair) |
| UC markdown | UC-RIDE-001..031 under `docs/ux/use-cases/` |
| Stack / custody / BT | Recorded under `docs/architecture/` |
| Artifacts | ART-RIDE-ANDROID-001, ART-RIDE-API-001, ART-RIDE-MOUNT-001, ART-RIDE-UX-001, ART-RIDE-UX-REVIEW-001 |
| MCP | `MCP_UNTRUSTED` until health recovers |
| Plan HV | Astra R1-R2 env-blocked; R3 DISAGREE@78; R4 DISAGREE@94; R5 DISAGREE@97; this r3 addresses BR4/BR5 residual |

### 1.5 BDPv4 order

1. Requirements captured + P0 YAML/traceability repair.
2. This plan revision + Astra READY/AGREE (≥98) + Payton AGREE.
3. Per construction phase: RED → mocks green → real green → refactor; Failed 0 Skipped 0.
4. Opposing-model product HV AGREE; retain JSONL + canonical pair; commit immediately.
5. Mark phase complete only after HV + suite green.

Code gen: `grok-4.6-xhigh` or `gpt-6-sol` xhigh. Product HV: opposing model per `docs/process/hostile-validation.md`. Plan HV: gpt-6-astra xhigh (operator-authorized exception; does not replace product matrix).

---

## 2. Inventory and coverage matrix

### 2.1 Counts

| Kind | Count | Notes |
| --- | ---: | --- |
| FR | 84 | Functional + additive BT + additive Avalonia/gRPC |
| TR | 64 | Technical + additives |
| TEST | 37 | Testing + additives |
| UC (YAML) | 31 | Single `records:` key; UC-RIDE-001..031 |
| Mappings | 84 | One row per FR |
| FR-owned ACs | 183 | |
| TR-owned ACs | 106 | |
| TEST-owned ACs | 66 | |
| UC-owned ACs | 49 | |
| **Total ACs** | **404** | Complete inventory; section 2.3 is FR-owned only |

### 2.2 Coverage integrity

The source contains 84 FRs, 64 TRs, 37 TESTs, 84 FR mapping rows, and **31 distinct UC records**. Every mapped TR, TEST, and UC ID exists in the source text.

**Historical parser issue (corrected):** `Use-Cases-Batch.yaml` previously repeated the top-level `records:` key at lines 2, 564, and 606. Strict `ConvertFrom-Yaml` rejected it with `Duplicate key records`. A parser result containing only UC-RIDE-025..031 was **not** evidence that earlier records were absent — UC-RIDE-001..024 were **present in source; blocked by duplicate records keys**.

**P0 repair status:** The three sequences were consolidated under one `records:` key, preserving existing records, flows, frLinks, and acceptance criteria (not reconstructed from Markdown). Exit requires: strict duplicate-key rejection passes; exactly 31 unique UC localIds; all 84 mapping rows resolve; Markdown reconciliation. Missing records, duplicate IDs, unresolved references, or silently discarded keys block P0.

### 2.3 Full FR coverage matrix (FR-owned ACs)

Section 2.3 lists **FR-owned ACs only** and must not be described as the complete AC inventory. See §2.7.

| FR | Priority | Title | TR | TEST | UC | FR-owned AC IDs | Impl owner |
| --- | --- | --- | --- | --- | --- | --- | --- |
| FR-RIDE-001 | high | Ingest Lyft privacy-export ZIP | TR-RIDE-INGEST-001, TR-RIDE-INGEST-006 | TEST-RIDE-001 | UC-RIDE-001 | AC-RIDE-001-001, AC-RIDE-001-002, AC-RIDE-001-003 | P9 |
| FR-RIDE-002 | high | Record Smooth Cruiser scores | TR-RIDE-INGEST-002 | TEST-RIDE-002 | UC-RIDE-002 | AC-RIDE-002-001, AC-RIDE-002-002, AC-RIDE-002-003 | P9 |
| FR-RIDE-003 | high | Ingest trip-level records | TR-RIDE-INGEST-003 | TEST-RIDE-003 | UC-RIDE-001 | AC-RIDE-003-001, AC-RIDE-003-002 | P9 |
| FR-RIDE-004 | medium | Optional Concierge/Business API integration | TR-RIDE-INGEST-004 | TEST-RIDE-004 | UC-RIDE-003 | AC-RIDE-004-001, AC-RIDE-004-002, AC-RIDE-004-003 | P9 |
| FR-RIDE-005 | high | Import third-party telematics | TR-RIDE-INGEST-005, TR-RIDE-INGEST-006 | TEST-RIDE-005 | UC-RIDE-004 | AC-RIDE-005-001, AC-RIDE-005-002, AC-RIDE-005-003 | P9 |
| FR-RIDE-006 | critical | Provenance and consent ledger | TR-RIDE-INGEST-006, TR-RIDE-PRIV-001 | TEST-RIDE-006 | UC-RIDE-001, UC-RIDE-002, UC-RIDE-004 | AC-RIDE-006-001, AC-RIDE-006-002 | P9 |
| FR-RIDE-007 | high | Coverage matrix | TR-RIDE-ANAL-002 | TEST-RIDE-007 | UC-RIDE-005 | AC-RIDE-007-001, AC-RIDE-007-002 | P8 |
| FR-RIDE-008 | high | Online-hours policy audit | TR-RIDE-ANAL-003 | TEST-RIDE-008 | UC-RIDE-006 | AC-RIDE-008-001, AC-RIDE-008-002, AC-RIDE-008-003 | P8 |
| FR-RIDE-009 | high | Time-window incident report | TR-RIDE-ANAL-004 | TEST-RIDE-009 | UC-RIDE-007 | AC-RIDE-009-001, AC-RIDE-009-002 | P8 |
| FR-RIDE-010 | critical | Data subject access and deletion | TR-RIDE-STORE-003, TR-RIDE-PRIV-001, TR-RIDE-PRIV-003 | TEST-RIDE-010 | UC-RIDE-008 | AC-RIDE-010-001, AC-RIDE-010-002 | P10 |
| FR-RIDE-011 | critical | No undocumented Lyft private APIs | TR-RIDE-INGEST-001, TR-RIDE-INGEST-002 | TEST-RIDE-004 | UC-RIDE-003, UC-RIDE-020 | AC-RIDE-011-001, AC-RIDE-011-002 | P9 |
| FR-RIDE-012 | medium | Admin partnership gates | TR-RIDE-INGEST-004 | TEST-RIDE-004 | UC-RIDE-003, UC-RIDE-020 | AC-RIDE-012-001, AC-RIDE-012-002 | P9 |
| FR-RIDE-013 | high | Hash and version raw imports | TR-RIDE-STORE-001, TR-RIDE-STORE-002 | TEST-RIDE-011 | UC-RIDE-001 | AC-RIDE-013-001, AC-RIDE-013-002 | P9 |
| FR-RIDE-014 | critical | Role-based access | TR-RIDE-PRIV-002, TR-RIDE-SEC-002 | TEST-RIDE-012 | UC-RIDE-020 | AC-RIDE-014-001, AC-RIDE-014-002 | P10 |
| FR-RIDE-015 | critical | Seal and encrypt at collection | TR-RIDE-STORE-001, TR-RIDE-STORE-002, TR-RIDE-SEAL-001 | TEST-RIDE-013 | UC-RIDE-009 | AC-RIDE-015-001, AC-RIDE-015-002 | P3 |
| FR-RIDE-016 | critical | Per-session or per-sample keys | TR-RIDE-SEAL-002 | TEST-RIDE-013 | UC-RIDE-009 | AC-RIDE-016-001, AC-RIDE-016-002, AC-RIDE-016-003 | P3 |
| FR-RIDE-017 | critical | Custody receipt content | TR-RIDE-CHAIN-001 | TEST-RIDE-014 | UC-RIDE-009 | AC-RIDE-017-001, AC-RIDE-017-002 | P3 |
| FR-RIDE-018 | critical | Blockchain receipt write | TR-RIDE-CHAIN-002 | TEST-RIDE-014 | UC-RIDE-009 | AC-RIDE-018-001, AC-RIDE-018-002, AC-RIDE-018-003 | P3 |
| FR-RIDE-019 | critical | Blockchain write failure policy | TR-RIDE-CHAIN-003 | TEST-RIDE-015 | UC-RIDE-009 | AC-RIDE-019-001, AC-RIDE-019-002, AC-RIDE-019-003 | P3 |
| FR-RIDE-020 | critical | Court-review decryption path docs | TR-RIDE-ANAL-001 | TEST-RIDE-016 | UC-RIDE-010 | AC-RIDE-020-001, AC-RIDE-020-002, AC-RIDE-020-003 | P8 |
| FR-RIDE-021 | critical | Verification UI/report | TR-RIDE-ANAL-001, TR-RIDE-CHAIN-004 | TEST-RIDE-016 | UC-RIDE-010 | AC-RIDE-021-001, AC-RIDE-021-002, AC-RIDE-021-003 | P8 |
| FR-RIDE-022 | critical | M-of-N key escrow | TR-RIDE-ESCROW-001 | TEST-RIDE-017 | UC-RIDE-011 | AC-RIDE-022-001, AC-RIDE-022-002 | P5 |
| FR-RIDE-023 | critical | Escrow separation from device | TR-RIDE-ESCROW-001, TR-RIDE-ESCROW-003 | TEST-RIDE-017 | UC-RIDE-011 | AC-RIDE-023-001, AC-RIDE-023-002, AC-RIDE-023-003 | P5 |
| FR-RIDE-024 | critical | Court-authorized escrow release | TR-RIDE-ESCROW-002 | TEST-RIDE-018 | UC-RIDE-011 | AC-RIDE-024-001, AC-RIDE-024-002, AC-RIDE-024-003 | P5 |
| FR-RIDE-025 | critical | Play Integrity key binding | TR-RIDE-PLAY-001 | TEST-RIDE-019 | UC-RIDE-012 | AC-RIDE-025-001, AC-RIDE-025-002 | P4 |
| FR-RIDE-026 | critical | Reject failed Play Integrity | TR-RIDE-PLAY-001, TR-RIDE-PLAY-003 | TEST-RIDE-019 | UC-RIDE-012 | AC-RIDE-026-001, AC-RIDE-026-002, AC-RIDE-026-003 | P4 |
| FR-RIDE-027 | critical | Attestation on custody receipt | TR-RIDE-CHAIN-001, TR-RIDE-PLAY-002 | TEST-RIDE-019 | UC-RIDE-012 | AC-RIDE-027-001, AC-RIDE-027-002 | P4 |
| FR-RIDE-028 | critical | Counsel verification steps | TR-RIDE-CHAIN-004, TR-RIDE-ESCROW-002 | TEST-RIDE-018 | UC-RIDE-010, UC-RIDE-019 | AC-RIDE-028-001, AC-RIDE-028-002 | P5 |
| FR-RIDE-029 | critical | GPL-2.0 licensing | TR-RIDE-GPL-001 | TEST-RIDE-020 | UC-RIDE-013 | AC-RIDE-029-001, AC-RIDE-029-002, AC-RIDE-029-003 | P1 |
| FR-RIDE-030 | high | GPL2 notices on artifacts | TR-RIDE-GPL-002 | TEST-RIDE-020 | UC-RIDE-013 | AC-RIDE-030-001, AC-RIDE-030-002 | P1 |
| FR-RIDE-031 | high | Publish client via Play and source repo | TR-RIDE-GPL-003 | TEST-RIDE-020 | UC-RIDE-013, UC-RIDE-014 | AC-RIDE-031-001, AC-RIDE-031-002 | P11b |
| FR-RIDE-032 | critical | Public-server driver account | TR-RIDE-SERVER-001 | TEST-RIDE-021 | UC-RIDE-014 | AC-RIDE-032-001, AC-RIDE-032-002, AC-RIDE-032-003 | P2 |
| FR-RIDE-033 | high | Vehicle registration | TR-RIDE-SERVER-002 | TEST-RIDE-021 | UC-RIDE-014 | AC-RIDE-033-001, AC-RIDE-033-002 | P2 |
| FR-RIDE-034 | high | Configuration profile gate | TR-RIDE-SERVER-002 | TEST-RIDE-021 | UC-RIDE-014 | AC-RIDE-034-001, AC-RIDE-034-002 | P2 |
| FR-RIDE-035 | critical | Sealed-only submission API | TR-RIDE-SERVER-003 | TEST-RIDE-022 | UC-RIDE-015 | AC-RIDE-035-001, AC-RIDE-035-002 | P2 |
| FR-RIDE-036 | critical | Admission verification chain | TR-RIDE-SERVER-004 | TEST-RIDE-022 | UC-RIDE-015 | AC-RIDE-036-001, AC-RIDE-036-002 | P2 |
| FR-RIDE-037 | critical | Multi-driver per-record provenance | TR-RIDE-SERVER-007 | TEST-RIDE-023 | UC-RIDE-015, UC-RIDE-016 | AC-RIDE-037-001, AC-RIDE-037-002 | P8 |
| FR-RIDE-038 | high | Counsel multi-driver bundle | TR-RIDE-SERVER-007 | TEST-RIDE-023 | UC-RIDE-007, UC-RIDE-016 | AC-RIDE-038-001, AC-RIDE-038-002 | P8 |
| FR-RIDE-039 | critical | Public-server abuse controls | TR-RIDE-SERVER-004, TR-RIDE-SERVER-005 | TEST-RIDE-024 | UC-RIDE-015 | AC-RIDE-039-001, AC-RIDE-039-002, AC-RIDE-039-003 | P2 |
| FR-RIDE-040 | critical | Multi-tenant isolation | TR-RIDE-SERVER-006 | TEST-RIDE-024 | UC-RIDE-015 | AC-RIDE-040-001, AC-RIDE-040-002 | P2 |
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
| FR-RIDE-201 | critical | TLS and secrets vault | TR-RIDE-SEC-001 | TEST-RIDE-029 | UC-RIDE-009 | AC-RIDE-201-001, AC-RIDE-201-002 | P2 |
| FR-RIDE-202 | critical | Geolocation sensitive masking | TR-RIDE-PRIV-002 | TEST-RIDE-012 | UC-RIDE-008, UC-RIDE-020 | AC-RIDE-202-001, AC-RIDE-202-002 | P10 |
| FR-RIDE-203 | critical | Append-only access logs | TR-RIDE-SEC-003 | TEST-RIDE-029 | UC-RIDE-008 | AC-RIDE-203-001 | P10 |
| FR-RIDE-204 | high | Concierge ingestion resilience | TR-RIDE-STORE-001, TR-RIDE-STORE-002 | TEST-RIDE-030 | UC-RIDE-003 | AC-RIDE-204-001, AC-RIDE-204-002 | P9 |
| FR-RIDE-205 | high | Scale multi-year histories | TR-RIDE-PERF-002 | TEST-RIDE-031 | UC-RIDE-021 | AC-RIDE-205-001, AC-RIDE-205-002 | P10 |
| FR-RIDE-206 | critical | No false Smooth Cruiser labeling | TR-RIDE-STORE-001, TR-RIDE-STORE-002 | TEST-RIDE-030 | UC-RIDE-003 | AC-RIDE-206-001, AC-RIDE-206-002 | P9 |
| FR-RIDE-207 | high | Portable audit ZIP export | TR-RIDE-STORE-001, TR-RIDE-STORE-002 | TEST-RIDE-031 | UC-RIDE-007, UC-RIDE-021 | AC-RIDE-207-001 | P10 |
| FR-RIDE-208 | high | Per-state retention config | TR-RIDE-STORE-003 | TEST-RIDE-032 | UC-RIDE-008 | AC-RIDE-208-001, AC-RIDE-208-002 | P10 |
| FR-RIDE-209 | medium | In-product API gap notice | TR-RIDE-ANAL-002 | TEST-RIDE-007 | UC-RIDE-005 | AC-RIDE-209-001 | P9 |
| FR-RIDE-210 | critical | Legal hold suspends deletion | TR-RIDE-STORE-003, TR-RIDE-PRIV-003 | TEST-RIDE-010 | UC-RIDE-008 | AC-RIDE-210-001 | P10 |
| FR-RIDE-211 | high | Cryptographic agility | TR-RIDE-SEAL-003 | TEST-RIDE-032 | UC-RIDE-009 | AC-RIDE-211-001, AC-RIDE-211-002 | P3 |
| FR-RIDE-212 | P3 (OTS default) + P11a (alternate profiles) |
| FR-RIDE-213 | high | Seal/receipt latency budget | TR-RIDE-PERF-001 | TEST-RIDE-032 | UC-RIDE-009 | AC-RIDE-213-001, AC-RIDE-213-002 | P3 |
| FR-RIDE-214 | P5 |
| FR-RIDE-215 | critical | Play authenticity allowlist | TR-RIDE-PLAY-001, TR-RIDE-PLAY-003 | TEST-RIDE-019 | UC-RIDE-012 | AC-RIDE-215-001, AC-RIDE-215-002, AC-RIDE-215-003 | P4 |
| FR-RIDE-216 | critical | Escrow resilience | TR-RIDE-ESCROW-001 | TEST-RIDE-017 | UC-RIDE-011 | AC-RIDE-216-001, AC-RIDE-216-002 | P5 |
| FR-RIDE-217 | critical | GPL-2.0 governance NFR | TR-RIDE-GPL-001 | TEST-RIDE-020 | UC-RIDE-013 | AC-RIDE-217-001, AC-RIDE-217-002 | P11b |
| FR-RIDE-218 | critical | Public-server admission capacity | TR-RIDE-SERVER-005 | TEST-RIDE-024 | UC-RIDE-015 | AC-RIDE-218-001, AC-RIDE-218-002 | P2 |
| FR-RIDE-219 | high | Video storage and bandwidth quotas | TR-RIDE-VIDEO-006 | TEST-RIDE-033 | UC-RIDE-017 | AC-RIDE-219-001, AC-RIDE-219-002, AC-RIDE-219-003 | P6 |
| FR-RIDE-220 | high | Video performance thresholds | TR-RIDE-VIDEO-006, TR-RIDE-PERF-003 | TEST-RIDE-033 | UC-RIDE-017 | AC-RIDE-220-001, AC-RIDE-220-002 | P6 |
| FR-RIDE-221 | high | Composite integrity for playback | TR-RIDE-VIDEO-001, TR-RIDE-VIDEO-002 | TEST-RIDE-027 | UC-RIDE-018 | AC-RIDE-221-001, AC-RIDE-221-002 | P7 |
| FR-RIDE-222 | critical | Desktop portability fail-closed | TR-RIDE-VIEW-001 | TEST-RIDE-028 | UC-RIDE-019 | AC-RIDE-222-001, AC-RIDE-222-002 | P7 |
| FR-RIDE-056 | high | Avalonia UI 12 Android dual-phone capture client | TR-RIDE-VIDEO-012 | TEST-RIDE-035 | UC-RIDE-025 | AC-RIDE-056-001, AC-RIDE-056-002 | P6 |
| FR-RIDE-057 | high | Avalonia UI 12 desktop court viewer | TR-RIDE-VIEW-005 | TEST-RIDE-035 | UC-RIDE-026 | AC-RIDE-057-001, AC-RIDE-057-002 | P7 |
| FR-RIDE-058 | high | Shared Avalonia UI 12 constraints under GPL-2.0 | TR-RIDE-GPL-004 | TEST-RIDE-035 | UC-RIDE-027 | AC-RIDE-058-001, AC-RIDE-058-002 | P7 |
| FR-RIDE-059 | critical | Backend gRPC on .NET 10 containers | TR-RIDE-SERVER-008 | TEST-RIDE-036 | UC-RIDE-028 | AC-RIDE-059-001, AC-RIDE-059-002 | P2 |
| FR-RIDE-060 | high | Proto and schema publication under GPL-2.0 | TR-RIDE-GPL-005 | TEST-RIDE-037 | UC-RIDE-029 | AC-RIDE-060-001, AC-RIDE-060-002 | P1 |
| FR-RIDE-061 | critical | Fail-closed admission over gRPC | TR-RIDE-SERVER-009 | TEST-RIDE-036 | UC-RIDE-030 | AC-RIDE-061-001, AC-RIDE-061-002 | P2 |
| FR-RIDE-062 | high | Interim OpenAPI companion non-authoritative | TR-RIDE-SERVER-010 | TEST-RIDE-037 | UC-RIDE-031 | AC-RIDE-062-001, AC-RIDE-062-002 | P1 |
| FR-RIDE-053 | high | Bluetooth driver-rider phone pairing | TR-RIDE-VIDEO-010 | TEST-RIDE-034 | UC-RIDE-022 | AC-RIDE-053-001, AC-RIDE-053-002 | P6 |
| FR-RIDE-054 | high | Driver phone session coordination | TR-RIDE-VIDEO-011 | TEST-RIDE-034 | UC-RIDE-023 | AC-RIDE-054-001, AC-RIDE-054-002 | P6 |
| FR-RIDE-055 | high | Passenger phone video sync join and telematics overlay | TR-RIDE-VIDEO-011 | TEST-RIDE-034 | UC-RIDE-024 | AC-RIDE-055-001, AC-RIDE-055-002 | P6 |

### 2.4 TR inventory

| TR | Subarea | Title | TR-owned ACs | Related FRs |
| --- | --- | --- | --- | --- |
| TR-RIDE-INGEST-001 | INGEST | Privacy-export ZIP parser | AC-RIDE-INGEST-001-001, AC-RIDE-INGEST-001-002 | FR-RIDE-001, FR-RIDE-011 |
| TR-RIDE-INGEST-002 | INGEST | Smooth Cruiser structured and manual ingest | AC-RIDE-INGEST-002-001, AC-RIDE-INGEST-002-002 | FR-RIDE-002, FR-RIDE-011 |
| TR-RIDE-INGEST-003 | INGEST | Trip and Business report ingest | AC-RIDE-INGEST-003-001, AC-RIDE-INGEST-003-002 | FR-RIDE-003 |
| TR-RIDE-INGEST-004 | INGEST | Concierge OAuth and status poller | AC-RIDE-INGEST-004-001, AC-RIDE-INGEST-004-002 | FR-RIDE-004, FR-RIDE-012 |
| TR-RIDE-INGEST-005 | INGEST | Third-party telematics importers | AC-RIDE-INGEST-005-001, AC-RIDE-INGEST-005-002 | FR-RIDE-005 |
| TR-RIDE-INGEST-006 | INGEST | Honesty provenance tagging | AC-RIDE-INGEST-006-001 | FR-RIDE-001, FR-RIDE-005, FR-RIDE-006 |
| TR-RIDE-STORE-001 | STORE | Sealed immutable blob store | AC-RIDE-STORE-001-001, AC-RIDE-STORE-001-002 | FR-RIDE-013, FR-RIDE-015, FR-RIDE-204, FR-RIDE-206, FR-RIDE-207 |
| TR-RIDE-STORE-002 | STORE | Versioned correction events | AC-RIDE-STORE-002-001, AC-RIDE-STORE-002-002 | FR-RIDE-013, FR-RIDE-015, FR-RIDE-204, FR-RIDE-206, FR-RIDE-207 |
| TR-RIDE-STORE-003 | STORE | Jurisdiction retention engine | AC-RIDE-STORE-003-001, AC-RIDE-STORE-003-002 | FR-RIDE-010, FR-RIDE-208, FR-RIDE-210 |
| TR-RIDE-ANAL-001 | ANAL | Authorized working-copy analysis | AC-RIDE-ANAL-001-001, AC-RIDE-ANAL-001-002 | FR-RIDE-020, FR-RIDE-021 |
| TR-RIDE-ANAL-002 | ANAL | Coverage matrix generator | AC-RIDE-ANAL-002-001, AC-RIDE-ANAL-002-002 | FR-RIDE-007, FR-RIDE-209 |
| TR-RIDE-ANAL-003 | ANAL | Online-hours policy engine | AC-RIDE-ANAL-003-001, AC-RIDE-ANAL-003-002 | FR-RIDE-008 |
| TR-RIDE-ANAL-004 | ANAL | Incident package builder | AC-RIDE-ANAL-004-001, AC-RIDE-ANAL-004-002 | FR-RIDE-009 |
| TR-RIDE-SEAL-001 | SEAL | Collection-boundary sealer | AC-RIDE-SEAL-001-001, AC-RIDE-SEAL-001-002 | FR-RIDE-015 |
| TR-RIDE-SEAL-002 | SEAL | Scoped key generation | AC-RIDE-SEAL-002-001, AC-RIDE-SEAL-002-002 | FR-RIDE-016 |
| TR-RIDE-SEAL-003 | SEAL | Cryptographic agility layer | AC-RIDE-SEAL-003-001, AC-RIDE-SEAL-003-002 | FR-RIDE-211 |
| TR-RIDE-CHAIN-001 | CHAIN | Custody receipt builder | AC-RIDE-CHAIN-001-001, AC-RIDE-CHAIN-001-002 | FR-RIDE-017, FR-RIDE-027 |
| TR-RIDE-CHAIN-002 | CHAIN | Configurable chain writer | AC-RIDE-CHAIN-002-001, AC-RIDE-CHAIN-002-002 | FR-RIDE-018, FR-RIDE-212 |
| TR-RIDE-CHAIN-003 | CHAIN | Non-admission on chain failure | AC-RIDE-CHAIN-003-001, AC-RIDE-CHAIN-003-002 | FR-RIDE-019 |
| TR-RIDE-CHAIN-004 | CHAIN | Receipt verification service | AC-RIDE-CHAIN-004-001, AC-RIDE-CHAIN-004-002 | FR-RIDE-021, FR-RIDE-028 |
| TR-RIDE-ESCROW-001 | ESCROW | M-of-N escrow packaging | AC-RIDE-ESCROW-001-001, AC-RIDE-ESCROW-001-002 | FR-RIDE-022, FR-RIDE-023, FR-RIDE-216 |
| TR-RIDE-ESCROW-002 | ESCROW | Court escrow-release workflow | AC-RIDE-ESCROW-002-001, AC-RIDE-ESCROW-002-002 | FR-RIDE-024, FR-RIDE-028 |
| TR-RIDE-ESCROW-003 | ESCROW | HSM/KMS private-key custody | AC-RIDE-ESCROW-003-001, AC-RIDE-ESCROW-003-002 | FR-RIDE-023, FR-RIDE-214 |
| TR-RIDE-PLAY-001 | PLAY | Play Integrity before seal | AC-RIDE-PLAY-001-001, AC-RIDE-PLAY-001-002 | FR-RIDE-025, FR-RIDE-026, FR-RIDE-215 |
| TR-RIDE-PLAY-002 | PLAY | Attestation evidence on receipt | AC-RIDE-PLAY-002-001, AC-RIDE-PLAY-002-002 | FR-RIDE-027 |
| TR-RIDE-PLAY-003 | PLAY | Package allowlist and rotation | AC-RIDE-PLAY-003-001, AC-RIDE-PLAY-003-002 | FR-RIDE-026, FR-RIDE-215 |
| TR-RIDE-GPL-001 | GPL | GPL-2.0 project licensing | AC-RIDE-GPL-001-001, AC-RIDE-GPL-001-002 | FR-RIDE-029, FR-RIDE-217 |
| TR-RIDE-GPL-002 | GPL | Artifact license metadata | AC-RIDE-GPL-002-001, AC-RIDE-GPL-002-002 | FR-RIDE-030 |
| TR-RIDE-GPL-003 | GPL | Play and source distribution | AC-RIDE-GPL-003-001, AC-RIDE-GPL-003-002 | FR-RIDE-031 |
| TR-RIDE-SERVER-001 | SERVER | Driver account and consent service | AC-RIDE-SERVER-001-001, AC-RIDE-SERVER-001-002 | FR-RIDE-032 |
| TR-RIDE-SERVER-002 | SERVER | Vehicle registry and config profiles | AC-RIDE-SERVER-002-001, AC-RIDE-SERVER-002-002 | FR-RIDE-033, FR-RIDE-034 |
| TR-RIDE-SERVER-003 | SERVER | Sealed-only submission API | AC-RIDE-SERVER-003-001, AC-RIDE-SERVER-003-002 | FR-RIDE-035 |
| TR-RIDE-SERVER-004 | SERVER | Admission verifier | AC-RIDE-SERVER-004-001, AC-RIDE-SERVER-004-002 | FR-RIDE-036, FR-RIDE-039 |
| TR-RIDE-SERVER-005 | SERVER | Abuse controls and backpressure | AC-RIDE-SERVER-005-001, AC-RIDE-SERVER-005-002 | FR-RIDE-039, FR-RIDE-218 |
| TR-RIDE-SERVER-006 | SERVER | Multi-tenant isolation | AC-RIDE-SERVER-006-001, AC-RIDE-SERVER-006-002 | FR-RIDE-040 |
| TR-RIDE-SERVER-007 | SERVER | Counsel multi-driver bundle builder | AC-RIDE-SERVER-007-001, AC-RIDE-SERVER-007-002 | FR-RIDE-037, FR-RIDE-038 |
| TR-RIDE-VIDEO-001 | VIDEO | Dual-phone capture session | AC-RIDE-VIDEO-001-001, AC-RIDE-VIDEO-001-002 | FR-RIDE-041, FR-RIDE-221 |
| TR-RIDE-VIDEO-002 | VIDEO | SyncClockOffset service | AC-RIDE-VIDEO-002-001, AC-RIDE-VIDEO-002-002 | FR-RIDE-042, FR-RIDE-221 |
| TR-RIDE-VIDEO-003 | VIDEO | On-device composite pipeline | AC-RIDE-VIDEO-003-001, AC-RIDE-VIDEO-003-002 | FR-RIDE-043, FR-RIDE-046 |
| TR-RIDE-VIDEO-004 | VIDEO | Spider-graph overlay manifest | AC-RIDE-VIDEO-004-001, AC-RIDE-VIDEO-004-002 | FR-RIDE-044, FR-RIDE-048 |
| TR-RIDE-VIDEO-005 | VIDEO | Composite seal and chain receipt | AC-RIDE-VIDEO-005-001, AC-RIDE-VIDEO-005-002 | FR-RIDE-045 |
| TR-RIDE-VIDEO-006 | VIDEO | Video quota and chunked upload | AC-RIDE-VIDEO-006-001, AC-RIDE-VIDEO-006-002 | FR-RIDE-219, FR-RIDE-220 |
| TR-RIDE-VIEW-001 | VIEW | Cross-platform GPL2 viewer | AC-RIDE-VIEW-001-001, AC-RIDE-VIEW-001-002 | FR-RIDE-049, FR-RIDE-222 |
| TR-RIDE-VIEW-002 | VIEW | Fail-closed independent verification | AC-RIDE-VIEW-002-001, AC-RIDE-VIEW-002-002 | FR-RIDE-047, FR-RIDE-050 |
| TR-RIDE-VIEW-003 | VIEW | Synchronized RideBundle timeline | AC-RIDE-VIEW-003-001, AC-RIDE-VIEW-003-002 | FR-RIDE-051 |
| TR-RIDE-VIEW-004 | VIEW | ViewerSession and VerificationReport | AC-RIDE-VIEW-004-001, AC-RIDE-VIEW-004-002 | FR-RIDE-052 |
| TR-RIDE-PRIV-001 | PRIV | Consent and purpose binding | AC-RIDE-PRIV-001-001 | FR-RIDE-006, FR-RIDE-010 |
| TR-RIDE-PRIV-002 | PRIV | Sensitive geolocation masking | AC-RIDE-PRIV-002-001, AC-RIDE-PRIV-002-002 | FR-RIDE-014, FR-RIDE-202 |
| TR-RIDE-PRIV-003 | PRIV | DSAR access and deletion | AC-RIDE-PRIV-003-001, AC-RIDE-PRIV-003-002 | FR-RIDE-010, FR-RIDE-210 |
| TR-RIDE-SEC-001 | SEC | TLS and vault secrets | AC-RIDE-SEC-001-001, AC-RIDE-SEC-001-002 | FR-RIDE-201 |
| TR-RIDE-SEC-002 | SEC | RBAC least privilege | AC-RIDE-SEC-002-001, AC-RIDE-SEC-002-002 | FR-RIDE-014 |
| TR-RIDE-SEC-003 | SEC | Append-only sensitive access logs | AC-RIDE-SEC-003-001 | FR-RIDE-203 |
| TR-RIDE-PERF-001 | PERF | Seal/receipt latency monitoring | AC-RIDE-PERF-001-001, AC-RIDE-PERF-001-002 | FR-RIDE-213 |
| TR-RIDE-PERF-002 | PERF | Large history UI pagination | AC-RIDE-PERF-002-001 | FR-RIDE-205 |
| TR-RIDE-PERF-003 | PERF | Video performance gates | AC-RIDE-PERF-003-001, AC-RIDE-PERF-003-002 | FR-RIDE-220 |
| TR-RIDE-VIDEO-012 | VIDEO | Avalonia UI 12 Android capture client | (inherits mapped FR ACs; additive without independent AC array) | FR-RIDE-056 |
| TR-RIDE-VIEW-005 | VIEW | Avalonia UI 12 desktop court viewer | (inherits mapped FR ACs; additive without independent AC array) | FR-RIDE-057 |
| TR-RIDE-GPL-004 | GPL | Shared Avalonia UI under GPL-2.0 | (inherits mapped FR ACs; additive without independent AC array) | FR-RIDE-058 |
| TR-RIDE-SERVER-008 | SERVER | gRPC services on .NET 10 containers | (inherits mapped FR ACs; additive without independent AC array) | FR-RIDE-059 |
| TR-RIDE-GPL-005 | GPL | Publish gRPC protos under GPL-2.0 | (inherits mapped FR ACs; additive without independent AC array) | FR-RIDE-060 |
| TR-RIDE-SERVER-009 | SERVER | Fail-closed gRPC admission | (inherits mapped FR ACs; additive without independent AC array) | FR-RIDE-061 |
| TR-RIDE-SERVER-010 | SERVER | OpenAPI companion non-authoritative | (inherits mapped FR ACs; additive without independent AC array) | FR-RIDE-062 |
| TR-RIDE-VIDEO-010 | VIDEO | Bluetooth pairing and role protocol | (inherits mapped FR ACs; additive without independent AC array) | FR-RIDE-053 |
| TR-RIDE-VIDEO-011 | VIDEO | Driver coordinator and passenger compositor split | (inherits mapped FR ACs; additive without independent AC array) | FR-RIDE-054, FR-RIDE-055 |

### 2.5 TEST inventory

| TEST | Title | TEST-owned ACs | Related FRs |
| --- | --- | --- | --- |
| TEST-RIDE-001 | Privacy-export ZIP parse and Unverified tagging | AC-TEST-001-001, AC-TEST-001-002 | FR-RIDE-001 |
| TEST-RIDE-002 | Smooth Cruiser ingest paths | AC-TEST-002-001, AC-TEST-002-002 | FR-RIDE-002 |
| TEST-RIDE-003 | Trip record ingest | AC-TEST-003-001, AC-TEST-003-002 | FR-RIDE-003 |
| TEST-RIDE-004 | Concierge optional poll and partnership gate | AC-TEST-004-001, AC-TEST-004-002 | FR-RIDE-004, FR-RIDE-011, FR-RIDE-012 |
| TEST-RIDE-005 | Third-party telematics import | AC-TEST-005-001, AC-TEST-005-002 | FR-RIDE-005 |
| TEST-RIDE-006 | Consent and provenance ledger | AC-TEST-006-001, AC-TEST-006-002 | FR-RIDE-006 |
| TEST-RIDE-007 | Coverage matrix and API-gap notice | AC-TEST-007-001, AC-TEST-007-002 | FR-RIDE-007, FR-RIDE-209 |
| TEST-RIDE-008 | Online-hours policy evaluation | AC-TEST-008-001, AC-TEST-008-002 | FR-RIDE-008 |
| TEST-RIDE-009 | Incident time-window export | AC-TEST-009-001, AC-TEST-009-002 | FR-RIDE-009 |
| TEST-RIDE-010 | DSAR access deletion and legal hold | AC-TEST-010-001, AC-TEST-010-002 | FR-RIDE-010, FR-RIDE-210 |
| TEST-RIDE-011 | Hash version integrity on imports | AC-TEST-011-001, AC-TEST-011-002 | FR-RIDE-013 |
| TEST-RIDE-012 | RBAC least privilege location | AC-TEST-012-001, AC-TEST-012-002 | FR-RIDE-014, FR-RIDE-202 |
| TEST-RIDE-013 | Seal-at-collect boundary | AC-TEST-013-001, AC-TEST-013-002 | FR-RIDE-015, FR-RIDE-016 |
| TEST-RIDE-014 | Custody receipt and chain write | AC-TEST-014-001, AC-TEST-014-002 | FR-RIDE-017, FR-RIDE-018, FR-RIDE-212 |
| TEST-RIDE-015 | Chain failure non-admission | AC-TEST-015-001, AC-TEST-015-002 | FR-RIDE-019 |
| TEST-RIDE-016 | Court decryption path documentation and verify UI | AC-TEST-016-001, AC-TEST-016-002 | FR-RIDE-020, FR-RIDE-021 |
| TEST-RIDE-017 | Escrow package and separation | AC-TEST-017-001, AC-TEST-017-002 | FR-RIDE-022, FR-RIDE-023, FR-RIDE-216 |
| TEST-RIDE-018 | Court escrow release workflow | AC-TEST-018-001, AC-TEST-018-002 | FR-RIDE-024, FR-RIDE-028 |
| TEST-RIDE-019 | Play Integrity gate and receipt attestation | AC-TEST-019-001, AC-TEST-019-002 | FR-RIDE-025, FR-RIDE-026, FR-RIDE-027, FR-RIDE-215 |
| TEST-RIDE-020 | GPL-2.0 licensing and distribution | AC-TEST-020-001, AC-TEST-020-002 | FR-RIDE-029, FR-RIDE-030, FR-RIDE-031, FR-RIDE-217 |
| TEST-RIDE-021 | Driver account vehicle config | AC-TEST-021-001, AC-TEST-021-002 | FR-RIDE-032, FR-RIDE-033, FR-RIDE-034 |
| TEST-RIDE-022 | Sealed-only submit and admission verify | AC-TEST-022-001, AC-TEST-022-002 | FR-RIDE-035, FR-RIDE-036 |
| TEST-RIDE-023 | Multi-driver provenance and counsel bundle | AC-TEST-023-001, AC-TEST-023-002 | FR-RIDE-037, FR-RIDE-038 |
| TEST-RIDE-024 | Abuse controls and tenant isolation | AC-TEST-024-001, AC-TEST-024-002 | FR-RIDE-039, FR-RIDE-040, FR-RIDE-218 |
| TEST-RIDE-025 | Dual-phone sync and composite | AC-TEST-025-001, AC-TEST-025-002 | FR-RIDE-041, FR-RIDE-042, FR-RIDE-043, FR-RIDE-044 |
| TEST-RIDE-026 | Composite seal optional raw and metadata | AC-TEST-026-001, AC-TEST-026-002 | FR-RIDE-045, FR-RIDE-046, FR-RIDE-048 |
| TEST-RIDE-027 | Counsel composite playback verification | AC-TEST-027-001, AC-TEST-027-002 | FR-RIDE-047, FR-RIDE-221 |
| TEST-RIDE-028 | Desktop viewer fail-closed timeline | AC-TEST-028-001, AC-TEST-028-002 | FR-RIDE-049, FR-RIDE-050, FR-RIDE-051, FR-RIDE-052, FR-RIDE-222 |
| TEST-RIDE-029 | Security TLS vault and access logs | AC-TEST-029-001, AC-TEST-029-002 | FR-RIDE-201, FR-RIDE-203, FR-RIDE-214 |
| TEST-RIDE-030 | Concierge resilience and accuracy labeling | AC-TEST-030-001, AC-TEST-030-002 | FR-RIDE-204, FR-RIDE-206 |
| TEST-RIDE-031 | Scalability pagination and portable export | AC-TEST-031-001, AC-TEST-031-002 | FR-RIDE-205, FR-RIDE-207 |
| TEST-RIDE-032 | Retention crypto agility latency | AC-TEST-032-001, AC-TEST-032-002 | FR-RIDE-208, FR-RIDE-211, FR-RIDE-213 |
| TEST-RIDE-033 | Video quotas and performance gates | AC-TEST-033-001, AC-TEST-033-002 | FR-RIDE-219, FR-RIDE-220 |
| TEST-RIDE-035 | Avalonia UI 12 clients and GPL share | (inherits mapped FR ACs) | FR-RIDE-056, FR-RIDE-057, FR-RIDE-058 |
| TEST-RIDE-036 | gRPC .NET 10 sealed fail-closed admission | (inherits mapped FR ACs) | FR-RIDE-059, FR-RIDE-061 |
| TEST-RIDE-037 | Proto GPL authority over OpenAPI companion | (inherits mapped FR ACs) | FR-RIDE-060, FR-RIDE-062 |
| TEST-RIDE-034 | Bluetooth pairing and role split | (inherits mapped FR ACs) | FR-RIDE-053, FR-RIDE-054, FR-RIDE-055 |

### 2.6 UC inventory

| UC | Title | YAML | UC-owned ACs | Related FRs | Bound FR ACs if no UC AC |
| --- | --- | --- | --- | --- | --- |
| UC-RIDE-001 | Ingest privacy-export ZIP | yes | AC-UC-001-001, AC-UC-001-002 | FR-RIDE-001, FR-RIDE-003, FR-RIDE-006, FR-RIDE-013 |  |
| UC-RIDE-002 | Record Smooth Cruiser evidence | yes | AC-UC-002-001, AC-UC-002-002 | FR-RIDE-002, FR-RIDE-006 |  |
| UC-RIDE-003 | Optional Concierge ride location poll | yes | AC-UC-003-001, AC-UC-003-002 | FR-RIDE-004, FR-RIDE-011, FR-RIDE-012, FR-RIDE-204, FR-RIDE-206 |  |
| UC-RIDE-004 | Import third-party telematics | yes | AC-UC-004-001, AC-UC-004-002 | FR-RIDE-005, FR-RIDE-006 |  |
| UC-RIDE-005 | Generate coverage matrix | yes | AC-UC-005-001, AC-UC-005-002 | FR-RIDE-007, FR-RIDE-209 |  |
| UC-RIDE-006 | Online-hours policy check | yes | AC-UC-006-001, AC-UC-006-002 | FR-RIDE-008 |  |
| UC-RIDE-007 | Build incident time-window package | yes | AC-UC-007-001, AC-UC-007-002 | FR-RIDE-009, FR-RIDE-038, FR-RIDE-207 |  |
| UC-RIDE-008 | Data subject access or deletion | yes | AC-UC-008-001, AC-UC-008-002 | FR-RIDE-010, FR-RIDE-202, FR-RIDE-203, FR-RIDE-208, FR-RIDE-210 |  |
| UC-RIDE-009 | Seal-at-collect with chain receipt | yes | AC-UC-009-001, AC-UC-009-002 | FR-RIDE-015, FR-RIDE-016, FR-RIDE-017, FR-RIDE-018, FR-RIDE-019, FR-RIDE-201, FR-RIDE-211, FR-RIDE-212, FR-RIDE-213 |  |
| UC-RIDE-010 | Counsel verification and decrypt path | yes | AC-UC-010-001, AC-UC-010-002 | FR-RIDE-020, FR-RIDE-021, FR-RIDE-028, FR-RIDE-214 |  |
| UC-RIDE-011 | Escrow key and court release | yes | AC-UC-011-001, AC-UC-011-002 | FR-RIDE-022, FR-RIDE-023, FR-RIDE-024, FR-RIDE-214, FR-RIDE-216 |  |
| UC-RIDE-012 | Play Integrity gated collection | yes | AC-UC-012-001, AC-UC-012-002 | FR-RIDE-025, FR-RIDE-026, FR-RIDE-027, FR-RIDE-215 |  |
| UC-RIDE-013 | GPL-2.0 publish and notice | yes | AC-UC-013-001, AC-UC-013-002 | FR-RIDE-029, FR-RIDE-030, FR-RIDE-031, FR-RIDE-217 |  |
| UC-RIDE-014 | Driver self-registers on public server | yes | AC-UC-014-001, AC-UC-014-002 | FR-RIDE-031, FR-RIDE-032, FR-RIDE-033, FR-RIDE-034 |  |
| UC-RIDE-015 | Submit sealed package to public server | yes | AC-UC-015-001, AC-UC-015-002 | FR-RIDE-035, FR-RIDE-036, FR-RIDE-037, FR-RIDE-039, FR-RIDE-040, FR-RIDE-218 |  |
| UC-RIDE-016 | Counsel multi-driver bundle | yes | AC-UC-016-001, AC-UC-016-002 | FR-RIDE-037, FR-RIDE-038 |  |
| UC-RIDE-017 | Dual-phone composite evidence | yes | AC-UC-017-001, AC-UC-017-002 | FR-RIDE-041, FR-RIDE-042, FR-RIDE-043, FR-RIDE-044, FR-RIDE-045, FR-RIDE-046, FR-RIDE-048, FR-RIDE-219, FR-RIDE-220 |  |
| UC-RIDE-018 | Counsel composite playback | yes | AC-UC-018-001, AC-UC-018-002 | FR-RIDE-047, FR-RIDE-048, FR-RIDE-050, FR-RIDE-221 |  |
| UC-RIDE-019 | Desktop court viewer review | yes | AC-UC-019-001, AC-UC-019-002 | FR-RIDE-028, FR-RIDE-049, FR-RIDE-050, FR-RIDE-051, FR-RIDE-052, FR-RIDE-222 |  |
| UC-RIDE-020 | Admin RBAC and partnership gates | yes | AC-UC-020-001, AC-UC-020-002 | FR-RIDE-011, FR-RIDE-012, FR-RIDE-014, FR-RIDE-202 |  |
| UC-RIDE-021 | Cross-cutting compliance and quality gates | yes | AC-UC-021-001, AC-UC-021-002 | FR-RIDE-205, FR-RIDE-207 |  |
| UC-RIDE-022 | Pair driver and passenger phones over Bluetooth | yes | none declared | FR-RIDE-053 | AC-RIDE-053-001/002 + TEST-RIDE-034 |
| UC-RIDE-023 | Driver coordinates dual-phone session | yes | none declared | FR-RIDE-054 | AC-RIDE-054-001/002 + TEST-RIDE-034 |
| UC-RIDE-024 | Passenger syncs joins and overlays telematics | yes | none declared | FR-RIDE-055 | AC-RIDE-055-001/002 + TEST-RIDE-034 |
| UC-RIDE-025 | Capture ride evidence with Avalonia Android client | yes | AC-UC-025-001 | FR-RIDE-056 |  |
| UC-RIDE-026 | Review sealed bundle with Avalonia desktop viewer | yes | AC-UC-026-001 | FR-RIDE-057 |  |
| UC-RIDE-027 | Reuse shared Avalonia UI under GPL-2.0 | yes | AC-UC-027-001 | FR-RIDE-058 |  |
| UC-RIDE-028 | Submit sealed package via gRPC .NET 10 | yes | AC-UC-028-001 | FR-RIDE-059 |  |
| UC-RIDE-029 | Consume published GPL-2.0 gRPC protos | yes | AC-UC-029-001 | FR-RIDE-060 |  |
| UC-RIDE-030 | Fail-closed gRPC admission | yes | AC-UC-030-001 | FR-RIDE-061 |  |
| UC-RIDE-031 | Prefer gRPC over interim OpenAPI companion | yes | AC-UC-031-001 | FR-RIDE-062 |  |

### 2.7 Complete acceptance-criteria closure

P0 must materialize a 404-row acceptance ledger with these columns: AC ID | Owning record ID | Related FR IDs | Primary implementation phases | Acceptance phase | Planned TEST IDs. Each source AC appears exactly once. Shared ACs preserve all related FRs. Missing phase or test assignments block plan acceptance. The ledger is section 2.7.4 below.

Before each construction increment, record its exact AC IDs, the behavior exercised for each ID, external mocks, required real adapters, and evidence paths. Broad TEST/UC ACs can have several evidence contributions under the same existing AC ID; these contributions do not create new AC IDs. P11b reconciles every contribution and rejects incomplete whole-AC coverage.

The source inventory contains **404** existing acceptance criteria: 183 FR-owned, 106 TR-owned, 66 TEST-owned, and 49 UC-owned. Section 2.3 lists FR-owned ACs only.

For each FR, required acceptance closure is the **union** of its own `acceptanceCriteria` and the `acceptanceCriteria` belonging to every TR, TEST, and UC referenced by its mapping row. Shared ACs retain all relationships without duplicate definitions.

UC-RIDE-022..024 have no separately declared UC ACs: bind flows to AC-RIDE-053-001/002, AC-RIDE-054-001/002, AC-RIDE-055-001/002 respectively and TEST-RIDE-034. Additive TR/TEST records without independent AC arrays inherit explicitly mapped FR obligations; do not fabricate AC IDs.

Every implementation increment must identify the exact AC IDs exercised by each executable test. A TEST record ID alone is not evidence that all of its mapped acceptance criteria pass.

#### 2.7.1 TR-owned AC appendix

| AC ID | Owning TR | Related FRs |
| --- | --- | --- |
| AC-RIDE-INGEST-001-001 | TR-RIDE-INGEST-001 | FR-RIDE-001, FR-RIDE-011 |
| AC-RIDE-INGEST-001-002 | TR-RIDE-INGEST-001 | FR-RIDE-001, FR-RIDE-011 |
| AC-RIDE-INGEST-002-001 | TR-RIDE-INGEST-002 | FR-RIDE-002, FR-RIDE-011 |
| AC-RIDE-INGEST-002-002 | TR-RIDE-INGEST-002 | FR-RIDE-002, FR-RIDE-011 |
| AC-RIDE-INGEST-003-001 | TR-RIDE-INGEST-003 | FR-RIDE-003 |
| AC-RIDE-INGEST-003-002 | TR-RIDE-INGEST-003 | FR-RIDE-003 |
| AC-RIDE-INGEST-004-001 | TR-RIDE-INGEST-004 | FR-RIDE-004, FR-RIDE-012 |
| AC-RIDE-INGEST-004-002 | TR-RIDE-INGEST-004 | FR-RIDE-004, FR-RIDE-012 |
| AC-RIDE-INGEST-005-001 | TR-RIDE-INGEST-005 | FR-RIDE-005 |
| AC-RIDE-INGEST-005-002 | TR-RIDE-INGEST-005 | FR-RIDE-005 |
| AC-RIDE-INGEST-006-001 | TR-RIDE-INGEST-006 | FR-RIDE-001, FR-RIDE-005, FR-RIDE-006 |
| AC-RIDE-STORE-001-001 | TR-RIDE-STORE-001 | FR-RIDE-013, FR-RIDE-015, FR-RIDE-204, FR-RIDE-206, FR-RIDE-207 |
| AC-RIDE-STORE-001-002 | TR-RIDE-STORE-001 | FR-RIDE-013, FR-RIDE-015, FR-RIDE-204, FR-RIDE-206, FR-RIDE-207 |
| AC-RIDE-STORE-002-001 | TR-RIDE-STORE-002 | FR-RIDE-013, FR-RIDE-015, FR-RIDE-204, FR-RIDE-206, FR-RIDE-207 |
| AC-RIDE-STORE-002-002 | TR-RIDE-STORE-002 | FR-RIDE-013, FR-RIDE-015, FR-RIDE-204, FR-RIDE-206, FR-RIDE-207 |
| AC-RIDE-STORE-003-001 | TR-RIDE-STORE-003 | FR-RIDE-010, FR-RIDE-208, FR-RIDE-210 |
| AC-RIDE-STORE-003-002 | TR-RIDE-STORE-003 | FR-RIDE-010, FR-RIDE-208, FR-RIDE-210 |
| AC-RIDE-ANAL-001-001 | TR-RIDE-ANAL-001 | FR-RIDE-020, FR-RIDE-021 |
| AC-RIDE-ANAL-001-002 | TR-RIDE-ANAL-001 | FR-RIDE-020, FR-RIDE-021 |
| AC-RIDE-ANAL-002-001 | TR-RIDE-ANAL-002 | FR-RIDE-007, FR-RIDE-209 |
| AC-RIDE-ANAL-002-002 | TR-RIDE-ANAL-002 | FR-RIDE-007, FR-RIDE-209 |
| AC-RIDE-ANAL-003-001 | TR-RIDE-ANAL-003 | FR-RIDE-008 |
| AC-RIDE-ANAL-003-002 | TR-RIDE-ANAL-003 | FR-RIDE-008 |
| AC-RIDE-ANAL-004-001 | TR-RIDE-ANAL-004 | FR-RIDE-009 |
| AC-RIDE-ANAL-004-002 | TR-RIDE-ANAL-004 | FR-RIDE-009 |
| AC-RIDE-SEAL-001-001 | TR-RIDE-SEAL-001 | FR-RIDE-015 |
| AC-RIDE-SEAL-001-002 | TR-RIDE-SEAL-001 | FR-RIDE-015 |
| AC-RIDE-SEAL-002-001 | TR-RIDE-SEAL-002 | FR-RIDE-016 |
| AC-RIDE-SEAL-002-002 | TR-RIDE-SEAL-002 | FR-RIDE-016 |
| AC-RIDE-SEAL-003-001 | TR-RIDE-SEAL-003 | FR-RIDE-211 |
| AC-RIDE-SEAL-003-002 | TR-RIDE-SEAL-003 | FR-RIDE-211 |
| AC-RIDE-CHAIN-001-001 | TR-RIDE-CHAIN-001 | FR-RIDE-017, FR-RIDE-027 |
| AC-RIDE-CHAIN-001-002 | TR-RIDE-CHAIN-001 | FR-RIDE-017, FR-RIDE-027 |
| AC-RIDE-CHAIN-002-001 | TR-RIDE-CHAIN-002 | FR-RIDE-018, FR-RIDE-212 |
| AC-RIDE-CHAIN-002-002 | TR-RIDE-CHAIN-002 | FR-RIDE-018, FR-RIDE-212 |
| AC-RIDE-CHAIN-003-001 | TR-RIDE-CHAIN-003 | FR-RIDE-019 |
| AC-RIDE-CHAIN-003-002 | TR-RIDE-CHAIN-003 | FR-RIDE-019 |
| AC-RIDE-CHAIN-004-001 | TR-RIDE-CHAIN-004 | FR-RIDE-021, FR-RIDE-028 |
| AC-RIDE-CHAIN-004-002 | TR-RIDE-CHAIN-004 | FR-RIDE-021, FR-RIDE-028 |
| AC-RIDE-ESCROW-001-001 | TR-RIDE-ESCROW-001 | FR-RIDE-022, FR-RIDE-023, FR-RIDE-216 |
| AC-RIDE-ESCROW-001-002 | TR-RIDE-ESCROW-001 | FR-RIDE-022, FR-RIDE-023, FR-RIDE-216 |
| AC-RIDE-ESCROW-002-001 | TR-RIDE-ESCROW-002 | FR-RIDE-024, FR-RIDE-028 |
| AC-RIDE-ESCROW-002-002 | TR-RIDE-ESCROW-002 | FR-RIDE-024, FR-RIDE-028 |
| AC-RIDE-ESCROW-003-001 | TR-RIDE-ESCROW-003 | FR-RIDE-023, FR-RIDE-214 |
| AC-RIDE-ESCROW-003-002 | TR-RIDE-ESCROW-003 | FR-RIDE-023, FR-RIDE-214 |
| AC-RIDE-PLAY-001-001 | TR-RIDE-PLAY-001 | FR-RIDE-025, FR-RIDE-026, FR-RIDE-215 |
| AC-RIDE-PLAY-001-002 | TR-RIDE-PLAY-001 | FR-RIDE-025, FR-RIDE-026, FR-RIDE-215 |
| AC-RIDE-PLAY-002-001 | TR-RIDE-PLAY-002 | FR-RIDE-027 |
| AC-RIDE-PLAY-002-002 | TR-RIDE-PLAY-002 | FR-RIDE-027 |
| AC-RIDE-PLAY-003-001 | TR-RIDE-PLAY-003 | FR-RIDE-026, FR-RIDE-215 |
| AC-RIDE-PLAY-003-002 | TR-RIDE-PLAY-003 | FR-RIDE-026, FR-RIDE-215 |
| AC-RIDE-GPL-001-001 | TR-RIDE-GPL-001 | FR-RIDE-029, FR-RIDE-217 |
| AC-RIDE-GPL-001-002 | TR-RIDE-GPL-001 | FR-RIDE-029, FR-RIDE-217 |
| AC-RIDE-GPL-002-001 | TR-RIDE-GPL-002 | FR-RIDE-030 |
| AC-RIDE-GPL-002-002 | TR-RIDE-GPL-002 | FR-RIDE-030 |
| AC-RIDE-GPL-003-001 | TR-RIDE-GPL-003 | FR-RIDE-031 |
| AC-RIDE-GPL-003-002 | TR-RIDE-GPL-003 | FR-RIDE-031 |
| AC-RIDE-SERVER-001-001 | TR-RIDE-SERVER-001 | FR-RIDE-032 |
| AC-RIDE-SERVER-001-002 | TR-RIDE-SERVER-001 | FR-RIDE-032 |
| AC-RIDE-SERVER-002-001 | TR-RIDE-SERVER-002 | FR-RIDE-033, FR-RIDE-034 |
| AC-RIDE-SERVER-002-002 | TR-RIDE-SERVER-002 | FR-RIDE-033, FR-RIDE-034 |
| AC-RIDE-SERVER-003-001 | TR-RIDE-SERVER-003 | FR-RIDE-035 |
| AC-RIDE-SERVER-003-002 | TR-RIDE-SERVER-003 | FR-RIDE-035 |
| AC-RIDE-SERVER-004-001 | TR-RIDE-SERVER-004 | FR-RIDE-036, FR-RIDE-039 |
| AC-RIDE-SERVER-004-002 | TR-RIDE-SERVER-004 | FR-RIDE-036, FR-RIDE-039 |
| AC-RIDE-SERVER-005-001 | TR-RIDE-SERVER-005 | FR-RIDE-039, FR-RIDE-218 |
| AC-RIDE-SERVER-005-002 | TR-RIDE-SERVER-005 | FR-RIDE-039, FR-RIDE-218 |
| AC-RIDE-SERVER-006-001 | TR-RIDE-SERVER-006 | FR-RIDE-040 |
| AC-RIDE-SERVER-006-002 | TR-RIDE-SERVER-006 | FR-RIDE-040 |
| AC-RIDE-SERVER-007-001 | TR-RIDE-SERVER-007 | FR-RIDE-037, FR-RIDE-038 |
| AC-RIDE-SERVER-007-002 | TR-RIDE-SERVER-007 | FR-RIDE-037, FR-RIDE-038 |
| AC-RIDE-VIDEO-001-001 | TR-RIDE-VIDEO-001 | FR-RIDE-041, FR-RIDE-221 |
| AC-RIDE-VIDEO-001-002 | TR-RIDE-VIDEO-001 | FR-RIDE-041, FR-RIDE-221 |
| AC-RIDE-VIDEO-002-001 | TR-RIDE-VIDEO-002 | FR-RIDE-042, FR-RIDE-221 |
| AC-RIDE-VIDEO-002-002 | TR-RIDE-VIDEO-002 | FR-RIDE-042, FR-RIDE-221 |
| AC-RIDE-VIDEO-003-001 | TR-RIDE-VIDEO-003 | FR-RIDE-043, FR-RIDE-046 |
| AC-RIDE-VIDEO-003-002 | TR-RIDE-VIDEO-003 | FR-RIDE-043, FR-RIDE-046 |
| AC-RIDE-VIDEO-004-001 | TR-RIDE-VIDEO-004 | FR-RIDE-044, FR-RIDE-048 |
| AC-RIDE-VIDEO-004-002 | TR-RIDE-VIDEO-004 | FR-RIDE-044, FR-RIDE-048 |
| AC-RIDE-VIDEO-005-001 | TR-RIDE-VIDEO-005 | FR-RIDE-045 |
| AC-RIDE-VIDEO-005-002 | TR-RIDE-VIDEO-005 | FR-RIDE-045 |
| AC-RIDE-VIDEO-006-001 | TR-RIDE-VIDEO-006 | FR-RIDE-219, FR-RIDE-220 |
| AC-RIDE-VIDEO-006-002 | TR-RIDE-VIDEO-006 | FR-RIDE-219, FR-RIDE-220 |
| AC-RIDE-VIEW-001-001 | TR-RIDE-VIEW-001 | FR-RIDE-049, FR-RIDE-222 |
| AC-RIDE-VIEW-001-002 | TR-RIDE-VIEW-001 | FR-RIDE-049, FR-RIDE-222 |
| AC-RIDE-VIEW-002-001 | TR-RIDE-VIEW-002 | FR-RIDE-047, FR-RIDE-050 |
| AC-RIDE-VIEW-002-002 | TR-RIDE-VIEW-002 | FR-RIDE-047, FR-RIDE-050 |
| AC-RIDE-VIEW-003-001 | TR-RIDE-VIEW-003 | FR-RIDE-051 |
| AC-RIDE-VIEW-003-002 | TR-RIDE-VIEW-003 | FR-RIDE-051 |
| AC-RIDE-VIEW-004-001 | TR-RIDE-VIEW-004 | FR-RIDE-052 |
| AC-RIDE-VIEW-004-002 | TR-RIDE-VIEW-004 | FR-RIDE-052 |
| AC-RIDE-PRIV-001-001 | TR-RIDE-PRIV-001 | FR-RIDE-006, FR-RIDE-010 |
| AC-RIDE-PRIV-002-001 | TR-RIDE-PRIV-002 | FR-RIDE-014, FR-RIDE-202 |
| AC-RIDE-PRIV-002-002 | TR-RIDE-PRIV-002 | FR-RIDE-014, FR-RIDE-202 |
| AC-RIDE-PRIV-003-001 | TR-RIDE-PRIV-003 | FR-RIDE-010, FR-RIDE-210 |
| AC-RIDE-PRIV-003-002 | TR-RIDE-PRIV-003 | FR-RIDE-010, FR-RIDE-210 |
| AC-RIDE-SEC-001-001 | TR-RIDE-SEC-001 | FR-RIDE-201 |
| AC-RIDE-SEC-001-002 | TR-RIDE-SEC-001 | FR-RIDE-201 |
| AC-RIDE-SEC-002-001 | TR-RIDE-SEC-002 | FR-RIDE-014 |
| AC-RIDE-SEC-002-002 | TR-RIDE-SEC-002 | FR-RIDE-014 |
| AC-RIDE-SEC-003-001 | TR-RIDE-SEC-003 | FR-RIDE-203 |
| AC-RIDE-PERF-001-001 | TR-RIDE-PERF-001 | FR-RIDE-213 |
| AC-RIDE-PERF-001-002 | TR-RIDE-PERF-001 | FR-RIDE-213 |
| AC-RIDE-PERF-002-001 | TR-RIDE-PERF-002 | FR-RIDE-205 |
| AC-RIDE-PERF-003-001 | TR-RIDE-PERF-003 | FR-RIDE-220 |
| AC-RIDE-PERF-003-002 | TR-RIDE-PERF-003 | FR-RIDE-220 |

#### 2.7.2 TEST-owned AC appendix

| AC ID | Owning TEST | Related FRs |
| --- | --- | --- |
| AC-TEST-001-001 | TEST-RIDE-001 | FR-RIDE-001 |
| AC-TEST-001-002 | TEST-RIDE-001 | FR-RIDE-001 |
| AC-TEST-002-001 | TEST-RIDE-002 | FR-RIDE-002 |
| AC-TEST-002-002 | TEST-RIDE-002 | FR-RIDE-002 |
| AC-TEST-003-001 | TEST-RIDE-003 | FR-RIDE-003 |
| AC-TEST-003-002 | TEST-RIDE-003 | FR-RIDE-003 |
| AC-TEST-004-001 | TEST-RIDE-004 | FR-RIDE-004, FR-RIDE-011, FR-RIDE-012 |
| AC-TEST-004-002 | TEST-RIDE-004 | FR-RIDE-004, FR-RIDE-011, FR-RIDE-012 |
| AC-TEST-005-001 | TEST-RIDE-005 | FR-RIDE-005 |
| AC-TEST-005-002 | TEST-RIDE-005 | FR-RIDE-005 |
| AC-TEST-006-001 | TEST-RIDE-006 | FR-RIDE-006 |
| AC-TEST-006-002 | TEST-RIDE-006 | FR-RIDE-006 |
| AC-TEST-007-001 | TEST-RIDE-007 | FR-RIDE-007, FR-RIDE-209 |
| AC-TEST-007-002 | TEST-RIDE-007 | FR-RIDE-007, FR-RIDE-209 |
| AC-TEST-008-001 | TEST-RIDE-008 | FR-RIDE-008 |
| AC-TEST-008-002 | TEST-RIDE-008 | FR-RIDE-008 |
| AC-TEST-009-001 | TEST-RIDE-009 | FR-RIDE-009 |
| AC-TEST-009-002 | TEST-RIDE-009 | FR-RIDE-009 |
| AC-TEST-010-001 | TEST-RIDE-010 | FR-RIDE-010, FR-RIDE-210 |
| AC-TEST-010-002 | TEST-RIDE-010 | FR-RIDE-010, FR-RIDE-210 |
| AC-TEST-011-001 | TEST-RIDE-011 | FR-RIDE-013 |
| AC-TEST-011-002 | TEST-RIDE-011 | FR-RIDE-013 |
| AC-TEST-012-001 | TEST-RIDE-012 | FR-RIDE-014, FR-RIDE-202 |
| AC-TEST-012-002 | TEST-RIDE-012 | FR-RIDE-014, FR-RIDE-202 |
| AC-TEST-013-001 | TEST-RIDE-013 | FR-RIDE-015, FR-RIDE-016 |
| AC-TEST-013-002 | TEST-RIDE-013 | FR-RIDE-015, FR-RIDE-016 |
| AC-TEST-014-001 | TEST-RIDE-014 | FR-RIDE-017, FR-RIDE-018, FR-RIDE-212 |
| AC-TEST-014-002 | TEST-RIDE-014 | FR-RIDE-017, FR-RIDE-018, FR-RIDE-212 |
| AC-TEST-015-001 | TEST-RIDE-015 | FR-RIDE-019 |
| AC-TEST-015-002 | TEST-RIDE-015 | FR-RIDE-019 |
| AC-TEST-016-001 | TEST-RIDE-016 | FR-RIDE-020, FR-RIDE-021 |
| AC-TEST-016-002 | TEST-RIDE-016 | FR-RIDE-020, FR-RIDE-021 |
| AC-TEST-017-001 | TEST-RIDE-017 | FR-RIDE-022, FR-RIDE-023, FR-RIDE-216 |
| AC-TEST-017-002 | TEST-RIDE-017 | FR-RIDE-022, FR-RIDE-023, FR-RIDE-216 |
| AC-TEST-018-001 | TEST-RIDE-018 | FR-RIDE-024, FR-RIDE-028 |
| AC-TEST-018-002 | TEST-RIDE-018 | FR-RIDE-024, FR-RIDE-028 |
| AC-TEST-019-001 | TEST-RIDE-019 | FR-RIDE-025, FR-RIDE-026, FR-RIDE-027, FR-RIDE-215 |
| AC-TEST-019-002 | TEST-RIDE-019 | FR-RIDE-025, FR-RIDE-026, FR-RIDE-027, FR-RIDE-215 |
| AC-TEST-020-001 | TEST-RIDE-020 | FR-RIDE-029, FR-RIDE-030, FR-RIDE-031, FR-RIDE-217 |
| AC-TEST-020-002 | TEST-RIDE-020 | FR-RIDE-029, FR-RIDE-030, FR-RIDE-031, FR-RIDE-217 |
| AC-TEST-021-001 | TEST-RIDE-021 | FR-RIDE-032, FR-RIDE-033, FR-RIDE-034 |
| AC-TEST-021-002 | TEST-RIDE-021 | FR-RIDE-032, FR-RIDE-033, FR-RIDE-034 |
| AC-TEST-022-001 | TEST-RIDE-022 | FR-RIDE-035, FR-RIDE-036 |
| AC-TEST-022-002 | TEST-RIDE-022 | FR-RIDE-035, FR-RIDE-036 |
| AC-TEST-023-001 | TEST-RIDE-023 | FR-RIDE-037, FR-RIDE-038 |
| AC-TEST-023-002 | TEST-RIDE-023 | FR-RIDE-037, FR-RIDE-038 |
| AC-TEST-024-001 | TEST-RIDE-024 | FR-RIDE-039, FR-RIDE-040, FR-RIDE-218 |
| AC-TEST-024-002 | TEST-RIDE-024 | FR-RIDE-039, FR-RIDE-040, FR-RIDE-218 |
| AC-TEST-025-001 | TEST-RIDE-025 | FR-RIDE-041, FR-RIDE-042, FR-RIDE-043, FR-RIDE-044 |
| AC-TEST-025-002 | TEST-RIDE-025 | FR-RIDE-041, FR-RIDE-042, FR-RIDE-043, FR-RIDE-044 |
| AC-TEST-026-001 | TEST-RIDE-026 | FR-RIDE-045, FR-RIDE-046, FR-RIDE-048 |
| AC-TEST-026-002 | TEST-RIDE-026 | FR-RIDE-045, FR-RIDE-046, FR-RIDE-048 |
| AC-TEST-027-001 | TEST-RIDE-027 | FR-RIDE-047, FR-RIDE-221 |
| AC-TEST-027-002 | TEST-RIDE-027 | FR-RIDE-047, FR-RIDE-221 |
| AC-TEST-028-001 | TEST-RIDE-028 | FR-RIDE-049, FR-RIDE-050, FR-RIDE-051, FR-RIDE-052, FR-RIDE-222 |
| AC-TEST-028-002 | TEST-RIDE-028 | FR-RIDE-049, FR-RIDE-050, FR-RIDE-051, FR-RIDE-052, FR-RIDE-222 |
| AC-TEST-029-001 | TEST-RIDE-029 | FR-RIDE-201, FR-RIDE-203, FR-RIDE-214 |
| AC-TEST-029-002 | TEST-RIDE-029 | FR-RIDE-201, FR-RIDE-203, FR-RIDE-214 |
| AC-TEST-030-001 | TEST-RIDE-030 | FR-RIDE-204, FR-RIDE-206 |
| AC-TEST-030-002 | TEST-RIDE-030 | FR-RIDE-204, FR-RIDE-206 |
| AC-TEST-031-001 | TEST-RIDE-031 | FR-RIDE-205, FR-RIDE-207 |
| AC-TEST-031-002 | TEST-RIDE-031 | FR-RIDE-205, FR-RIDE-207 |
| AC-TEST-032-001 | TEST-RIDE-032 | FR-RIDE-208, FR-RIDE-211, FR-RIDE-213 |
| AC-TEST-032-002 | TEST-RIDE-032 | FR-RIDE-208, FR-RIDE-211, FR-RIDE-213 |
| AC-TEST-033-001 | TEST-RIDE-033 | FR-RIDE-219, FR-RIDE-220 |
| AC-TEST-033-002 | TEST-RIDE-033 | FR-RIDE-219, FR-RIDE-220 |

#### 2.7.3 UC-owned AC appendix

| AC ID | Owning UC | Related FRs |
| --- | --- | --- |
| AC-UC-001-001 | UC-RIDE-001 | FR-RIDE-001, FR-RIDE-003, FR-RIDE-006, FR-RIDE-013 |
| AC-UC-001-002 | UC-RIDE-001 | FR-RIDE-001, FR-RIDE-003, FR-RIDE-006, FR-RIDE-013 |
| AC-UC-002-001 | UC-RIDE-002 | FR-RIDE-002, FR-RIDE-006 |
| AC-UC-002-002 | UC-RIDE-002 | FR-RIDE-002, FR-RIDE-006 |
| AC-UC-003-001 | UC-RIDE-003 | FR-RIDE-004, FR-RIDE-011, FR-RIDE-012, FR-RIDE-204, FR-RIDE-206 |
| AC-UC-003-002 | UC-RIDE-003 | FR-RIDE-004, FR-RIDE-011, FR-RIDE-012, FR-RIDE-204, FR-RIDE-206 |
| AC-UC-004-001 | UC-RIDE-004 | FR-RIDE-005, FR-RIDE-006 |
| AC-UC-004-002 | UC-RIDE-004 | FR-RIDE-005, FR-RIDE-006 |
| AC-UC-005-001 | UC-RIDE-005 | FR-RIDE-007, FR-RIDE-209 |
| AC-UC-005-002 | UC-RIDE-005 | FR-RIDE-007, FR-RIDE-209 |
| AC-UC-006-001 | UC-RIDE-006 | FR-RIDE-008 |
| AC-UC-006-002 | UC-RIDE-006 | FR-RIDE-008 |
| AC-UC-007-001 | UC-RIDE-007 | FR-RIDE-009, FR-RIDE-038, FR-RIDE-207 |
| AC-UC-007-002 | UC-RIDE-007 | FR-RIDE-009, FR-RIDE-038, FR-RIDE-207 |
| AC-UC-008-001 | UC-RIDE-008 | FR-RIDE-010, FR-RIDE-202, FR-RIDE-203, FR-RIDE-208, FR-RIDE-210 |
| AC-UC-008-002 | UC-RIDE-008 | FR-RIDE-010, FR-RIDE-202, FR-RIDE-203, FR-RIDE-208, FR-RIDE-210 |
| AC-UC-009-001 | UC-RIDE-009 | FR-RIDE-015, FR-RIDE-016, FR-RIDE-017, FR-RIDE-018, FR-RIDE-019, FR-RIDE-201, FR-RIDE-211, FR-RIDE-212, FR-RIDE-213 |
| AC-UC-009-002 | UC-RIDE-009 | FR-RIDE-015, FR-RIDE-016, FR-RIDE-017, FR-RIDE-018, FR-RIDE-019, FR-RIDE-201, FR-RIDE-211, FR-RIDE-212, FR-RIDE-213 |
| AC-UC-010-001 | UC-RIDE-010 | FR-RIDE-020, FR-RIDE-021, FR-RIDE-028, FR-RIDE-214 |
| AC-UC-010-002 | UC-RIDE-010 | FR-RIDE-020, FR-RIDE-021, FR-RIDE-028, FR-RIDE-214 |
| AC-UC-011-001 | UC-RIDE-011 | FR-RIDE-022, FR-RIDE-023, FR-RIDE-024, FR-RIDE-214, FR-RIDE-216 |
| AC-UC-011-002 | UC-RIDE-011 | FR-RIDE-022, FR-RIDE-023, FR-RIDE-024, FR-RIDE-214, FR-RIDE-216 |
| AC-UC-012-001 | UC-RIDE-012 | FR-RIDE-025, FR-RIDE-026, FR-RIDE-027, FR-RIDE-215 |
| AC-UC-012-002 | UC-RIDE-012 | FR-RIDE-025, FR-RIDE-026, FR-RIDE-027, FR-RIDE-215 |
| AC-UC-013-001 | UC-RIDE-013 | FR-RIDE-029, FR-RIDE-030, FR-RIDE-031, FR-RIDE-217 |
| AC-UC-013-002 | UC-RIDE-013 | FR-RIDE-029, FR-RIDE-030, FR-RIDE-031, FR-RIDE-217 |
| AC-UC-014-001 | UC-RIDE-014 | FR-RIDE-031, FR-RIDE-032, FR-RIDE-033, FR-RIDE-034 |
| AC-UC-014-002 | UC-RIDE-014 | FR-RIDE-031, FR-RIDE-032, FR-RIDE-033, FR-RIDE-034 |
| AC-UC-015-001 | UC-RIDE-015 | FR-RIDE-035, FR-RIDE-036, FR-RIDE-037, FR-RIDE-039, FR-RIDE-040, FR-RIDE-218 |
| AC-UC-015-002 | UC-RIDE-015 | FR-RIDE-035, FR-RIDE-036, FR-RIDE-037, FR-RIDE-039, FR-RIDE-040, FR-RIDE-218 |
| AC-UC-016-001 | UC-RIDE-016 | FR-RIDE-037, FR-RIDE-038 |
| AC-UC-016-002 | UC-RIDE-016 | FR-RIDE-037, FR-RIDE-038 |
| AC-UC-017-001 | UC-RIDE-017 | FR-RIDE-041, FR-RIDE-042, FR-RIDE-043, FR-RIDE-044, FR-RIDE-045, FR-RIDE-046, FR-RIDE-048, FR-RIDE-219, FR-RIDE-220 |
| AC-UC-017-002 | UC-RIDE-017 | FR-RIDE-041, FR-RIDE-042, FR-RIDE-043, FR-RIDE-044, FR-RIDE-045, FR-RIDE-046, FR-RIDE-048, FR-RIDE-219, FR-RIDE-220 |
| AC-UC-018-001 | UC-RIDE-018 | FR-RIDE-047, FR-RIDE-048, FR-RIDE-050, FR-RIDE-221 |
| AC-UC-018-002 | UC-RIDE-018 | FR-RIDE-047, FR-RIDE-048, FR-RIDE-050, FR-RIDE-221 |
| AC-UC-019-001 | UC-RIDE-019 | FR-RIDE-028, FR-RIDE-049, FR-RIDE-050, FR-RIDE-051, FR-RIDE-052, FR-RIDE-222 |
| AC-UC-019-002 | UC-RIDE-019 | FR-RIDE-028, FR-RIDE-049, FR-RIDE-050, FR-RIDE-051, FR-RIDE-052, FR-RIDE-222 |
| AC-UC-020-001 | UC-RIDE-020 | FR-RIDE-011, FR-RIDE-012, FR-RIDE-014, FR-RIDE-202 |
| AC-UC-020-002 | UC-RIDE-020 | FR-RIDE-011, FR-RIDE-012, FR-RIDE-014, FR-RIDE-202 |
| AC-UC-021-001 | UC-RIDE-021 | FR-RIDE-205, FR-RIDE-207 |
| AC-UC-021-002 | UC-RIDE-021 | FR-RIDE-205, FR-RIDE-207 |
| AC-UC-025-001 | UC-RIDE-025 | FR-RIDE-056 |
| AC-UC-026-001 | UC-RIDE-026 | FR-RIDE-057 |
| AC-UC-027-001 | UC-RIDE-027 | FR-RIDE-058 |
| AC-UC-028-001 | UC-RIDE-028 | FR-RIDE-059 |
| AC-UC-029-001 | UC-RIDE-029 | FR-RIDE-060 |
| AC-UC-030-001 | UC-RIDE-030 | FR-RIDE-061 |
| AC-UC-031-001 | UC-RIDE-031 | FR-RIDE-062 |


#### 2.7.4 Full 404-row acceptance ledger

Conservative closure policy: primary implementation phases = union of section 10 owners of related FRs; planned TEST IDs = union of those FR mapping `testIds`. FR-owned ACs use their owning FR. Final whole-AC acceptance is **P11b** for every row after complete integrated evidence; earlier phases provide scoped construction evidence only and must not mark a whole AC or FR satisfied prematurely. This does not defer phase-local testing or permit skipped tests.

| AC ID | Owning record | Related FRs | Primary impl phases | Acceptance phase | Planned TEST IDs |
| --- | --- | --- | --- | --- | --- |
| AC-RIDE-001-001 | FR-RIDE-001 | FR-RIDE-001 | P9 | P11b | TEST-RIDE-001 |
| AC-RIDE-001-002 | FR-RIDE-001 | FR-RIDE-001 | P9 | P11b | TEST-RIDE-001 |
| AC-RIDE-001-003 | FR-RIDE-001 | FR-RIDE-001 | P9 | P11b | TEST-RIDE-001 |
| AC-RIDE-002-001 | FR-RIDE-002 | FR-RIDE-002 | P9 | P11b | TEST-RIDE-002 |
| AC-RIDE-002-002 | FR-RIDE-002 | FR-RIDE-002 | P9 | P11b | TEST-RIDE-002 |
| AC-RIDE-002-003 | FR-RIDE-002 | FR-RIDE-002 | P9 | P11b | TEST-RIDE-002 |
| AC-RIDE-003-001 | FR-RIDE-003 | FR-RIDE-003 | P9 | P11b | TEST-RIDE-003 |
| AC-RIDE-003-002 | FR-RIDE-003 | FR-RIDE-003 | P9 | P11b | TEST-RIDE-003 |
| AC-RIDE-004-001 | FR-RIDE-004 | FR-RIDE-004 | P9 | P11b | TEST-RIDE-004 |
| AC-RIDE-004-002 | FR-RIDE-004 | FR-RIDE-004 | P9 | P11b | TEST-RIDE-004 |
| AC-RIDE-004-003 | FR-RIDE-004 | FR-RIDE-004 | P9 | P11b | TEST-RIDE-004 |
| AC-RIDE-005-001 | FR-RIDE-005 | FR-RIDE-005 | P9 | P11b | TEST-RIDE-005 |
| AC-RIDE-005-002 | FR-RIDE-005 | FR-RIDE-005 | P9 | P11b | TEST-RIDE-005 |
| AC-RIDE-005-003 | FR-RIDE-005 | FR-RIDE-005 | P9 | P11b | TEST-RIDE-005 |
| AC-RIDE-006-001 | FR-RIDE-006 | FR-RIDE-006 | P9 | P11b | TEST-RIDE-006 |
| AC-RIDE-006-002 | FR-RIDE-006 | FR-RIDE-006 | P9 | P11b | TEST-RIDE-006 |
| AC-RIDE-007-001 | FR-RIDE-007 | FR-RIDE-007 | P8 | P11b | TEST-RIDE-007 |
| AC-RIDE-007-002 | FR-RIDE-007 | FR-RIDE-007 | P8 | P11b | TEST-RIDE-007 |
| AC-RIDE-008-001 | FR-RIDE-008 | FR-RIDE-008 | P8 | P11b | TEST-RIDE-008 |
| AC-RIDE-008-002 | FR-RIDE-008 | FR-RIDE-008 | P8 | P11b | TEST-RIDE-008 |
| AC-RIDE-008-003 | FR-RIDE-008 | FR-RIDE-008 | P8 | P11b | TEST-RIDE-008 |
| AC-RIDE-009-001 | FR-RIDE-009 | FR-RIDE-009 | P8 | P11b | TEST-RIDE-009 |
| AC-RIDE-009-002 | FR-RIDE-009 | FR-RIDE-009 | P8 | P11b | TEST-RIDE-009 |
| AC-RIDE-010-001 | FR-RIDE-010 | FR-RIDE-010 | P10 | P11b | TEST-RIDE-010 |
| AC-RIDE-010-002 | FR-RIDE-010 | FR-RIDE-010 | P10 | P11b | TEST-RIDE-010 |
| AC-RIDE-011-001 | FR-RIDE-011 | FR-RIDE-011 | P9 | P11b | TEST-RIDE-004 |
| AC-RIDE-011-002 | FR-RIDE-011 | FR-RIDE-011 | P9 | P11b | TEST-RIDE-004 |
| AC-RIDE-012-001 | FR-RIDE-012 | FR-RIDE-012 | P9 | P11b | TEST-RIDE-004 |
| AC-RIDE-012-002 | FR-RIDE-012 | FR-RIDE-012 | P9 | P11b | TEST-RIDE-004 |
| AC-RIDE-013-001 | FR-RIDE-013 | FR-RIDE-013 | P9 | P11b | TEST-RIDE-011 |
| AC-RIDE-013-002 | FR-RIDE-013 | FR-RIDE-013 | P9 | P11b | TEST-RIDE-011 |
| AC-RIDE-014-001 | FR-RIDE-014 | FR-RIDE-014 | P10 | P11b | TEST-RIDE-012 |
| AC-RIDE-014-002 | FR-RIDE-014 | FR-RIDE-014 | P10 | P11b | TEST-RIDE-012 |
| AC-RIDE-015-001 | FR-RIDE-015 | FR-RIDE-015 | P3 | P11b | TEST-RIDE-013 |
| AC-RIDE-015-002 | FR-RIDE-015 | FR-RIDE-015 | P3 | P11b | TEST-RIDE-013 |
| AC-RIDE-016-001 | FR-RIDE-016 | FR-RIDE-016 | P3 | P11b | TEST-RIDE-013 |
| AC-RIDE-016-002 | FR-RIDE-016 | FR-RIDE-016 | P3 | P11b | TEST-RIDE-013 |
| AC-RIDE-016-003 | FR-RIDE-016 | FR-RIDE-016 | P3 | P11b | TEST-RIDE-013 |
| AC-RIDE-017-001 | FR-RIDE-017 | FR-RIDE-017 | P3 | P11b | TEST-RIDE-014 |
| AC-RIDE-017-002 | FR-RIDE-017 | FR-RIDE-017 | P3 | P11b | TEST-RIDE-014 |
| AC-RIDE-018-001 | FR-RIDE-018 | FR-RIDE-018 | P3 | P11b | TEST-RIDE-014 |
| AC-RIDE-018-002 | FR-RIDE-018 | FR-RIDE-018 | P3 | P11b | TEST-RIDE-014 |
| AC-RIDE-018-003 | FR-RIDE-018 | FR-RIDE-018 | P3 | P11b | TEST-RIDE-014 |
| AC-RIDE-019-001 | FR-RIDE-019 | FR-RIDE-019 | P3 | P11b | TEST-RIDE-015 |
| AC-RIDE-019-002 | FR-RIDE-019 | FR-RIDE-019 | P3 | P11b | TEST-RIDE-015 |
| AC-RIDE-019-003 | FR-RIDE-019 | FR-RIDE-019 | P3 | P11b | TEST-RIDE-015 |
| AC-RIDE-020-001 | FR-RIDE-020 | FR-RIDE-020 | P8 | P11b | TEST-RIDE-016 |
| AC-RIDE-020-002 | FR-RIDE-020 | FR-RIDE-020 | P8 | P11b | TEST-RIDE-016 |
| AC-RIDE-020-003 | FR-RIDE-020 | FR-RIDE-020 | P8 | P11b | TEST-RIDE-016 |
| AC-RIDE-021-001 | FR-RIDE-021 | FR-RIDE-021 | P8 | P11b | TEST-RIDE-016 |
| AC-RIDE-021-002 | FR-RIDE-021 | FR-RIDE-021 | P8 | P11b | TEST-RIDE-016 |
| AC-RIDE-021-003 | FR-RIDE-021 | FR-RIDE-021 | P8 | P11b | TEST-RIDE-016 |
| AC-RIDE-022-001 | FR-RIDE-022 | FR-RIDE-022 | P5 | P11b | TEST-RIDE-017 |
| AC-RIDE-022-002 | FR-RIDE-022 | FR-RIDE-022 | P5 | P11b | TEST-RIDE-017 |
| AC-RIDE-023-001 | FR-RIDE-023 | FR-RIDE-023 | P5 | P11b | TEST-RIDE-017 |
| AC-RIDE-023-002 | FR-RIDE-023 | FR-RIDE-023 | P5 | P11b | TEST-RIDE-017 |
| AC-RIDE-023-003 | FR-RIDE-023 | FR-RIDE-023 | P5 | P11b | TEST-RIDE-017 |
| AC-RIDE-024-001 | FR-RIDE-024 | FR-RIDE-024 | P5 | P11b | TEST-RIDE-018 |
| AC-RIDE-024-002 | FR-RIDE-024 | FR-RIDE-024 | P5 | P11b | TEST-RIDE-018 |
| AC-RIDE-024-003 | FR-RIDE-024 | FR-RIDE-024 | P5 | P11b | TEST-RIDE-018 |
| AC-RIDE-025-001 | FR-RIDE-025 | FR-RIDE-025 | P4 | P11b | TEST-RIDE-019 |
| AC-RIDE-025-002 | FR-RIDE-025 | FR-RIDE-025 | P4 | P11b | TEST-RIDE-019 |
| AC-RIDE-026-001 | FR-RIDE-026 | FR-RIDE-026 | P4 | P11b | TEST-RIDE-019 |
| AC-RIDE-026-002 | FR-RIDE-026 | FR-RIDE-026 | P4 | P11b | TEST-RIDE-019 |
| AC-RIDE-026-003 | FR-RIDE-026 | FR-RIDE-026 | P4 | P11b | TEST-RIDE-019 |
| AC-RIDE-027-001 | FR-RIDE-027 | FR-RIDE-027 | P4 | P11b | TEST-RIDE-019 |
| AC-RIDE-027-002 | FR-RIDE-027 | FR-RIDE-027 | P4 | P11b | TEST-RIDE-019 |
| AC-RIDE-028-001 | FR-RIDE-028 | FR-RIDE-028 | P5 | P11b | TEST-RIDE-018 |
| AC-RIDE-028-002 | FR-RIDE-028 | FR-RIDE-028 | P5 | P11b | TEST-RIDE-018 |
| AC-RIDE-029-001 | FR-RIDE-029 | FR-RIDE-029 | P1 | P11b | TEST-RIDE-020 |
| AC-RIDE-029-002 | FR-RIDE-029 | FR-RIDE-029 | P1 | P11b | TEST-RIDE-020 |
| AC-RIDE-029-003 | FR-RIDE-029 | FR-RIDE-029 | P1 | P11b | TEST-RIDE-020 |
| AC-RIDE-030-001 | FR-RIDE-030 | FR-RIDE-030 | P1 | P11b | TEST-RIDE-020 |
| AC-RIDE-030-002 | FR-RIDE-030 | FR-RIDE-030 | P1 | P11b | TEST-RIDE-020 |
| AC-RIDE-031-001 | FR-RIDE-031 | FR-RIDE-031 | P11b | P11b | TEST-RIDE-020 |
| AC-RIDE-031-002 | FR-RIDE-031 | FR-RIDE-031 | P11b | P11b | TEST-RIDE-020 |
| AC-RIDE-032-001 | FR-RIDE-032 | FR-RIDE-032 | P2 | P11b | TEST-RIDE-021 |
| AC-RIDE-032-002 | FR-RIDE-032 | FR-RIDE-032 | P2 | P11b | TEST-RIDE-021 |
| AC-RIDE-032-003 | FR-RIDE-032 | FR-RIDE-032 | P2 | P11b | TEST-RIDE-021 |
| AC-RIDE-033-001 | FR-RIDE-033 | FR-RIDE-033 | P2 | P11b | TEST-RIDE-021 |
| AC-RIDE-033-002 | FR-RIDE-033 | FR-RIDE-033 | P2 | P11b | TEST-RIDE-021 |
| AC-RIDE-034-001 | FR-RIDE-034 | FR-RIDE-034 | P2 | P11b | TEST-RIDE-021 |
| AC-RIDE-034-002 | FR-RIDE-034 | FR-RIDE-034 | P2 | P11b | TEST-RIDE-021 |
| AC-RIDE-035-001 | FR-RIDE-035 | FR-RIDE-035 | P2 | P11b | TEST-RIDE-022 |
| AC-RIDE-035-002 | FR-RIDE-035 | FR-RIDE-035 | P2 | P11b | TEST-RIDE-022 |
| AC-RIDE-036-001 | FR-RIDE-036 | FR-RIDE-036 | P2 | P11b | TEST-RIDE-022 |
| AC-RIDE-036-002 | FR-RIDE-036 | FR-RIDE-036 | P2 | P11b | TEST-RIDE-022 |
| AC-RIDE-037-001 | FR-RIDE-037 | FR-RIDE-037 | P8 | P11b | TEST-RIDE-023 |
| AC-RIDE-037-002 | FR-RIDE-037 | FR-RIDE-037 | P8 | P11b | TEST-RIDE-023 |
| AC-RIDE-038-001 | FR-RIDE-038 | FR-RIDE-038 | P8 | P11b | TEST-RIDE-023 |
| AC-RIDE-038-002 | FR-RIDE-038 | FR-RIDE-038 | P8 | P11b | TEST-RIDE-023 |
| AC-RIDE-039-001 | FR-RIDE-039 | FR-RIDE-039 | P2 | P11b | TEST-RIDE-024 |
| AC-RIDE-039-002 | FR-RIDE-039 | FR-RIDE-039 | P2 | P11b | TEST-RIDE-024 |
| AC-RIDE-039-003 | FR-RIDE-039 | FR-RIDE-039 | P2 | P11b | TEST-RIDE-024 |
| AC-RIDE-040-001 | FR-RIDE-040 | FR-RIDE-040 | P2 | P11b | TEST-RIDE-024 |
| AC-RIDE-040-002 | FR-RIDE-040 | FR-RIDE-040 | P2 | P11b | TEST-RIDE-024 |
| AC-RIDE-041-001 | FR-RIDE-041 | FR-RIDE-041 | P6 | P11b | TEST-RIDE-025 |
| AC-RIDE-041-002 | FR-RIDE-041 | FR-RIDE-041 | P6 | P11b | TEST-RIDE-025 |
| AC-RIDE-042-001 | FR-RIDE-042 | FR-RIDE-042 | P6 | P11b | TEST-RIDE-025 |
| AC-RIDE-042-002 | FR-RIDE-042 | FR-RIDE-042 | P6 | P11b | TEST-RIDE-025 |
| AC-RIDE-043-001 | FR-RIDE-043 | FR-RIDE-043 | P6 | P11b | TEST-RIDE-025 |
| AC-RIDE-043-002 | FR-RIDE-043 | FR-RIDE-043 | P6 | P11b | TEST-RIDE-025 |
| AC-RIDE-044-001 | FR-RIDE-044 | FR-RIDE-044 | P6 | P11b | TEST-RIDE-025 |
| AC-RIDE-044-002 | FR-RIDE-044 | FR-RIDE-044 | P6 | P11b | TEST-RIDE-025 |
| AC-RIDE-045-001 | FR-RIDE-045 | FR-RIDE-045 | P6 | P11b | TEST-RIDE-026 |
| AC-RIDE-045-002 | FR-RIDE-045 | FR-RIDE-045 | P6 | P11b | TEST-RIDE-026 |
| AC-RIDE-046-001 | FR-RIDE-046 | FR-RIDE-046 | P6 | P11b | TEST-RIDE-026 |
| AC-RIDE-046-002 | FR-RIDE-046 | FR-RIDE-046 | P6 | P11b | TEST-RIDE-026 |
| AC-RIDE-047-001 | FR-RIDE-047 | FR-RIDE-047 | P7 | P11b | TEST-RIDE-027 |
| AC-RIDE-047-002 | FR-RIDE-047 | FR-RIDE-047 | P7 | P11b | TEST-RIDE-027 |
| AC-RIDE-048-001 | FR-RIDE-048 | FR-RIDE-048 | P6 | P11b | TEST-RIDE-026 |
| AC-RIDE-048-002 | FR-RIDE-048 | FR-RIDE-048 | P6 | P11b | TEST-RIDE-026 |
| AC-RIDE-049-001 | FR-RIDE-049 | FR-RIDE-049 | P7 | P11b | TEST-RIDE-028 |
| AC-RIDE-049-002 | FR-RIDE-049 | FR-RIDE-049 | P7 | P11b | TEST-RIDE-028 |
| AC-RIDE-050-001 | FR-RIDE-050 | FR-RIDE-050 | P7 | P11b | TEST-RIDE-028 |
| AC-RIDE-050-002 | FR-RIDE-050 | FR-RIDE-050 | P7 | P11b | TEST-RIDE-028 |
| AC-RIDE-051-001 | FR-RIDE-051 | FR-RIDE-051 | P7 | P11b | TEST-RIDE-028 |
| AC-RIDE-051-002 | FR-RIDE-051 | FR-RIDE-051 | P7 | P11b | TEST-RIDE-028 |
| AC-RIDE-052-001 | FR-RIDE-052 | FR-RIDE-052 | P7 | P11b | TEST-RIDE-028 |
| AC-RIDE-052-002 | FR-RIDE-052 | FR-RIDE-052 | P7 | P11b | TEST-RIDE-028 |
| AC-RIDE-201-001 | FR-RIDE-201 | FR-RIDE-201 | P2 | P11b | TEST-RIDE-029 |
| AC-RIDE-201-002 | FR-RIDE-201 | FR-RIDE-201 | P2 | P11b | TEST-RIDE-029 |
| AC-RIDE-202-001 | FR-RIDE-202 | FR-RIDE-202 | P10 | P11b | TEST-RIDE-012 |
| AC-RIDE-202-002 | FR-RIDE-202 | FR-RIDE-202 | P10 | P11b | TEST-RIDE-012 |
| AC-RIDE-203-001 | FR-RIDE-203 | FR-RIDE-203 | P10 | P11b | TEST-RIDE-029 |
| AC-RIDE-204-001 | FR-RIDE-204 | FR-RIDE-204 | P9 | P11b | TEST-RIDE-030 |
| AC-RIDE-204-002 | FR-RIDE-204 | FR-RIDE-204 | P9 | P11b | TEST-RIDE-030 |
| AC-RIDE-205-001 | FR-RIDE-205 | FR-RIDE-205 | P10 | P11b | TEST-RIDE-031 |
| AC-RIDE-205-002 | FR-RIDE-205 | FR-RIDE-205 | P10 | P11b | TEST-RIDE-031 |
| AC-RIDE-206-001 | FR-RIDE-206 | FR-RIDE-206 | P9 | P11b | TEST-RIDE-030 |
| AC-RIDE-206-002 | FR-RIDE-206 | FR-RIDE-206 | P9 | P11b | TEST-RIDE-030 |
| AC-RIDE-207-001 | FR-RIDE-207 | FR-RIDE-207 | P10 | P11b | TEST-RIDE-031 |
| AC-RIDE-208-001 | FR-RIDE-208 | FR-RIDE-208 | P10 | P11b | TEST-RIDE-032 |
| AC-RIDE-208-002 | FR-RIDE-208 | FR-RIDE-208 | P10 | P11b | TEST-RIDE-032 |
| AC-RIDE-209-001 | FR-RIDE-209 | FR-RIDE-209 | P9 | P11b | TEST-RIDE-007 |
| AC-RIDE-210-001 | FR-RIDE-210 | FR-RIDE-210 | P10 | P11b | TEST-RIDE-010 |
| AC-RIDE-211-001 | FR-RIDE-211 | FR-RIDE-211 | P3 | P11b | TEST-RIDE-032 |
| AC-RIDE-211-002 | FR-RIDE-211 | FR-RIDE-211 | P3 | P11b | TEST-RIDE-032 |
| AC-RIDE-212-001 | FR-RIDE-212 | FR-RIDE-212 | P3, P11a | P11b | TEST-RIDE-014 |
| AC-RIDE-212-002 | FR-RIDE-212 | FR-RIDE-212 | P3, P11a | P11b | TEST-RIDE-014 |
| AC-RIDE-212-003 | FR-RIDE-212 | FR-RIDE-212 | P3, P11a | P11b | TEST-RIDE-014 |
| AC-RIDE-213-001 | FR-RIDE-213 | FR-RIDE-213 | P3 | P11b | TEST-RIDE-032 |
| AC-RIDE-213-002 | FR-RIDE-213 | FR-RIDE-213 | P3 | P11b | TEST-RIDE-032 |
| AC-RIDE-214-001 | FR-RIDE-214 | FR-RIDE-214 | P5 | P11b | TEST-RIDE-029 |
| AC-RIDE-214-002 | FR-RIDE-214 | FR-RIDE-214 | P5 | P11b | TEST-RIDE-029 |
| AC-RIDE-215-001 | FR-RIDE-215 | FR-RIDE-215 | P4 | P11b | TEST-RIDE-019 |
| AC-RIDE-215-002 | FR-RIDE-215 | FR-RIDE-215 | P4 | P11b | TEST-RIDE-019 |
| AC-RIDE-215-003 | FR-RIDE-215 | FR-RIDE-215 | P4 | P11b | TEST-RIDE-019 |
| AC-RIDE-216-001 | FR-RIDE-216 | FR-RIDE-216 | P5 | P11b | TEST-RIDE-017 |
| AC-RIDE-216-002 | FR-RIDE-216 | FR-RIDE-216 | P5 | P11b | TEST-RIDE-017 |
| AC-RIDE-217-001 | FR-RIDE-217 | FR-RIDE-217 | P11b | P11b | TEST-RIDE-020 |
| AC-RIDE-217-002 | FR-RIDE-217 | FR-RIDE-217 | P11b | P11b | TEST-RIDE-020 |
| AC-RIDE-218-001 | FR-RIDE-218 | FR-RIDE-218 | P2 | P11b | TEST-RIDE-024 |
| AC-RIDE-218-002 | FR-RIDE-218 | FR-RIDE-218 | P2 | P11b | TEST-RIDE-024 |
| AC-RIDE-219-001 | FR-RIDE-219 | FR-RIDE-219 | P6 | P11b | TEST-RIDE-033 |
| AC-RIDE-219-002 | FR-RIDE-219 | FR-RIDE-219 | P6 | P11b | TEST-RIDE-033 |
| AC-RIDE-219-003 | FR-RIDE-219 | FR-RIDE-219 | P6 | P11b | TEST-RIDE-033 |
| AC-RIDE-220-001 | FR-RIDE-220 | FR-RIDE-220 | P6 | P11b | TEST-RIDE-033 |
| AC-RIDE-220-002 | FR-RIDE-220 | FR-RIDE-220 | P6 | P11b | TEST-RIDE-033 |
| AC-RIDE-221-001 | FR-RIDE-221 | FR-RIDE-221 | P7 | P11b | TEST-RIDE-027 |
| AC-RIDE-221-002 | FR-RIDE-221 | FR-RIDE-221 | P7 | P11b | TEST-RIDE-027 |
| AC-RIDE-222-001 | FR-RIDE-222 | FR-RIDE-222 | P7 | P11b | TEST-RIDE-028 |
| AC-RIDE-222-002 | FR-RIDE-222 | FR-RIDE-222 | P7 | P11b | TEST-RIDE-028 |
| AC-RIDE-056-001 | FR-RIDE-056 | FR-RIDE-056 | P6 | P11b | TEST-RIDE-035 |
| AC-RIDE-056-002 | FR-RIDE-056 | FR-RIDE-056 | P6 | P11b | TEST-RIDE-035 |
| AC-RIDE-057-001 | FR-RIDE-057 | FR-RIDE-057 | P7 | P11b | TEST-RIDE-035 |
| AC-RIDE-057-002 | FR-RIDE-057 | FR-RIDE-057 | P7 | P11b | TEST-RIDE-035 |
| AC-RIDE-058-001 | FR-RIDE-058 | FR-RIDE-058 | P7 | P11b | TEST-RIDE-035 |
| AC-RIDE-058-002 | FR-RIDE-058 | FR-RIDE-058 | P7 | P11b | TEST-RIDE-035 |
| AC-RIDE-059-001 | FR-RIDE-059 | FR-RIDE-059 | P2 | P11b | TEST-RIDE-036 |
| AC-RIDE-059-002 | FR-RIDE-059 | FR-RIDE-059 | P2 | P11b | TEST-RIDE-036 |
| AC-RIDE-060-001 | FR-RIDE-060 | FR-RIDE-060 | P1 | P11b | TEST-RIDE-037 |
| AC-RIDE-060-002 | FR-RIDE-060 | FR-RIDE-060 | P1 | P11b | TEST-RIDE-037 |
| AC-RIDE-061-001 | FR-RIDE-061 | FR-RIDE-061 | P2 | P11b | TEST-RIDE-036 |
| AC-RIDE-061-002 | FR-RIDE-061 | FR-RIDE-061 | P2 | P11b | TEST-RIDE-036 |
| AC-RIDE-062-001 | FR-RIDE-062 | FR-RIDE-062 | P1 | P11b | TEST-RIDE-037 |
| AC-RIDE-062-002 | FR-RIDE-062 | FR-RIDE-062 | P1 | P11b | TEST-RIDE-037 |
| AC-RIDE-053-001 | FR-RIDE-053 | FR-RIDE-053 | P6 | P11b | TEST-RIDE-034 |
| AC-RIDE-053-002 | FR-RIDE-053 | FR-RIDE-053 | P6 | P11b | TEST-RIDE-034 |
| AC-RIDE-054-001 | FR-RIDE-054 | FR-RIDE-054 | P6 | P11b | TEST-RIDE-034 |
| AC-RIDE-054-002 | FR-RIDE-054 | FR-RIDE-054 | P6 | P11b | TEST-RIDE-034 |
| AC-RIDE-055-001 | FR-RIDE-055 | FR-RIDE-055 | P6 | P11b | TEST-RIDE-034 |
| AC-RIDE-055-002 | FR-RIDE-055 | FR-RIDE-055 | P6 | P11b | TEST-RIDE-034 |
| AC-RIDE-INGEST-001-001 | TR-RIDE-INGEST-001 | FR-RIDE-001, FR-RIDE-011 | P9 | P11b | TEST-RIDE-001, TEST-RIDE-004 |
| AC-RIDE-INGEST-001-002 | TR-RIDE-INGEST-001 | FR-RIDE-001, FR-RIDE-011 | P9 | P11b | TEST-RIDE-001, TEST-RIDE-004 |
| AC-RIDE-INGEST-002-001 | TR-RIDE-INGEST-002 | FR-RIDE-002, FR-RIDE-011 | P9 | P11b | TEST-RIDE-002, TEST-RIDE-004 |
| AC-RIDE-INGEST-002-002 | TR-RIDE-INGEST-002 | FR-RIDE-002, FR-RIDE-011 | P9 | P11b | TEST-RIDE-002, TEST-RIDE-004 |
| AC-RIDE-INGEST-003-001 | TR-RIDE-INGEST-003 | FR-RIDE-003 | P9 | P11b | TEST-RIDE-003 |
| AC-RIDE-INGEST-003-002 | TR-RIDE-INGEST-003 | FR-RIDE-003 | P9 | P11b | TEST-RIDE-003 |
| AC-RIDE-INGEST-004-001 | TR-RIDE-INGEST-004 | FR-RIDE-004, FR-RIDE-012 | P9 | P11b | TEST-RIDE-004 |
| AC-RIDE-INGEST-004-002 | TR-RIDE-INGEST-004 | FR-RIDE-004, FR-RIDE-012 | P9 | P11b | TEST-RIDE-004 |
| AC-RIDE-INGEST-005-001 | TR-RIDE-INGEST-005 | FR-RIDE-005 | P9 | P11b | TEST-RIDE-005 |
| AC-RIDE-INGEST-005-002 | TR-RIDE-INGEST-005 | FR-RIDE-005 | P9 | P11b | TEST-RIDE-005 |
| AC-RIDE-INGEST-006-001 | TR-RIDE-INGEST-006 | FR-RIDE-001, FR-RIDE-005, FR-RIDE-006 | P9 | P11b | TEST-RIDE-001, TEST-RIDE-005, TEST-RIDE-006 |
| AC-RIDE-STORE-001-001 | TR-RIDE-STORE-001 | FR-RIDE-013, FR-RIDE-015, FR-RIDE-204, FR-RIDE-206, FR-RIDE-207 | P3, P9, P10 | P11b | TEST-RIDE-011, TEST-RIDE-013, TEST-RIDE-030, TEST-RIDE-031 |
| AC-RIDE-STORE-001-002 | TR-RIDE-STORE-001 | FR-RIDE-013, FR-RIDE-015, FR-RIDE-204, FR-RIDE-206, FR-RIDE-207 | P3, P9, P10 | P11b | TEST-RIDE-011, TEST-RIDE-013, TEST-RIDE-030, TEST-RIDE-031 |
| AC-RIDE-STORE-002-001 | TR-RIDE-STORE-002 | FR-RIDE-013, FR-RIDE-015, FR-RIDE-204, FR-RIDE-206, FR-RIDE-207 | P3, P9, P10 | P11b | TEST-RIDE-011, TEST-RIDE-013, TEST-RIDE-030, TEST-RIDE-031 |
| AC-RIDE-STORE-002-002 | TR-RIDE-STORE-002 | FR-RIDE-013, FR-RIDE-015, FR-RIDE-204, FR-RIDE-206, FR-RIDE-207 | P3, P9, P10 | P11b | TEST-RIDE-011, TEST-RIDE-013, TEST-RIDE-030, TEST-RIDE-031 |
| AC-RIDE-STORE-003-001 | TR-RIDE-STORE-003 | FR-RIDE-010, FR-RIDE-208, FR-RIDE-210 | P10 | P11b | TEST-RIDE-010, TEST-RIDE-032 |
| AC-RIDE-STORE-003-002 | TR-RIDE-STORE-003 | FR-RIDE-010, FR-RIDE-208, FR-RIDE-210 | P10 | P11b | TEST-RIDE-010, TEST-RIDE-032 |
| AC-RIDE-ANAL-001-001 | TR-RIDE-ANAL-001 | FR-RIDE-020, FR-RIDE-021 | P8 | P11b | TEST-RIDE-016 |
| AC-RIDE-ANAL-001-002 | TR-RIDE-ANAL-001 | FR-RIDE-020, FR-RIDE-021 | P8 | P11b | TEST-RIDE-016 |
| AC-RIDE-ANAL-002-001 | TR-RIDE-ANAL-002 | FR-RIDE-007, FR-RIDE-209 | P8, P9 | P11b | TEST-RIDE-007 |
| AC-RIDE-ANAL-002-002 | TR-RIDE-ANAL-002 | FR-RIDE-007, FR-RIDE-209 | P8, P9 | P11b | TEST-RIDE-007 |
| AC-RIDE-ANAL-003-001 | TR-RIDE-ANAL-003 | FR-RIDE-008 | P8 | P11b | TEST-RIDE-008 |
| AC-RIDE-ANAL-003-002 | TR-RIDE-ANAL-003 | FR-RIDE-008 | P8 | P11b | TEST-RIDE-008 |
| AC-RIDE-ANAL-004-001 | TR-RIDE-ANAL-004 | FR-RIDE-009 | P8 | P11b | TEST-RIDE-009 |
| AC-RIDE-ANAL-004-002 | TR-RIDE-ANAL-004 | FR-RIDE-009 | P8 | P11b | TEST-RIDE-009 |
| AC-RIDE-SEAL-001-001 | TR-RIDE-SEAL-001 | FR-RIDE-015 | P3 | P11b | TEST-RIDE-013 |
| AC-RIDE-SEAL-001-002 | TR-RIDE-SEAL-001 | FR-RIDE-015 | P3 | P11b | TEST-RIDE-013 |
| AC-RIDE-SEAL-002-001 | TR-RIDE-SEAL-002 | FR-RIDE-016 | P3 | P11b | TEST-RIDE-013 |
| AC-RIDE-SEAL-002-002 | TR-RIDE-SEAL-002 | FR-RIDE-016 | P3 | P11b | TEST-RIDE-013 |
| AC-RIDE-SEAL-003-001 | TR-RIDE-SEAL-003 | FR-RIDE-211 | P3 | P11b | TEST-RIDE-032 |
| AC-RIDE-SEAL-003-002 | TR-RIDE-SEAL-003 | FR-RIDE-211 | P3 | P11b | TEST-RIDE-032 |
| AC-RIDE-CHAIN-001-001 | TR-RIDE-CHAIN-001 | FR-RIDE-017, FR-RIDE-027 | P3, P4 | P11b | TEST-RIDE-014, TEST-RIDE-019 |
| AC-RIDE-CHAIN-001-002 | TR-RIDE-CHAIN-001 | FR-RIDE-017, FR-RIDE-027 | P3, P4 | P11b | TEST-RIDE-014, TEST-RIDE-019 |
| AC-RIDE-CHAIN-002-001 | TR-RIDE-CHAIN-002 | FR-RIDE-018, FR-RIDE-212 | P3, P11a | P11b | TEST-RIDE-014 |
| AC-RIDE-CHAIN-002-002 | TR-RIDE-CHAIN-002 | FR-RIDE-018, FR-RIDE-212 | P3, P11a | P11b | TEST-RIDE-014 |
| AC-RIDE-CHAIN-003-001 | TR-RIDE-CHAIN-003 | FR-RIDE-019 | P3 | P11b | TEST-RIDE-015 |
| AC-RIDE-CHAIN-003-002 | TR-RIDE-CHAIN-003 | FR-RIDE-019 | P3 | P11b | TEST-RIDE-015 |
| AC-RIDE-CHAIN-004-001 | TR-RIDE-CHAIN-004 | FR-RIDE-021, FR-RIDE-028 | P5, P8 | P11b | TEST-RIDE-016, TEST-RIDE-018 |
| AC-RIDE-CHAIN-004-002 | TR-RIDE-CHAIN-004 | FR-RIDE-021, FR-RIDE-028 | P5, P8 | P11b | TEST-RIDE-016, TEST-RIDE-018 |
| AC-RIDE-ESCROW-001-001 | TR-RIDE-ESCROW-001 | FR-RIDE-022, FR-RIDE-023, FR-RIDE-216 | P5 | P11b | TEST-RIDE-017 |
| AC-RIDE-ESCROW-001-002 | TR-RIDE-ESCROW-001 | FR-RIDE-022, FR-RIDE-023, FR-RIDE-216 | P5 | P11b | TEST-RIDE-017 |
| AC-RIDE-ESCROW-002-001 | TR-RIDE-ESCROW-002 | FR-RIDE-024, FR-RIDE-028 | P5 | P11b | TEST-RIDE-018 |
| AC-RIDE-ESCROW-002-002 | TR-RIDE-ESCROW-002 | FR-RIDE-024, FR-RIDE-028 | P5 | P11b | TEST-RIDE-018 |
| AC-RIDE-ESCROW-003-001 | TR-RIDE-ESCROW-003 | FR-RIDE-023, FR-RIDE-214 | P5 | P11b | TEST-RIDE-017, TEST-RIDE-029 |
| AC-RIDE-ESCROW-003-002 | TR-RIDE-ESCROW-003 | FR-RIDE-023, FR-RIDE-214 | P5 | P11b | TEST-RIDE-017, TEST-RIDE-029 |
| AC-RIDE-PLAY-001-001 | TR-RIDE-PLAY-001 | FR-RIDE-025, FR-RIDE-026, FR-RIDE-215 | P4 | P11b | TEST-RIDE-019 |
| AC-RIDE-PLAY-001-002 | TR-RIDE-PLAY-001 | FR-RIDE-025, FR-RIDE-026, FR-RIDE-215 | P4 | P11b | TEST-RIDE-019 |
| AC-RIDE-PLAY-002-001 | TR-RIDE-PLAY-002 | FR-RIDE-027 | P4 | P11b | TEST-RIDE-019 |
| AC-RIDE-PLAY-002-002 | TR-RIDE-PLAY-002 | FR-RIDE-027 | P4 | P11b | TEST-RIDE-019 |
| AC-RIDE-PLAY-003-001 | TR-RIDE-PLAY-003 | FR-RIDE-026, FR-RIDE-215 | P4 | P11b | TEST-RIDE-019 |
| AC-RIDE-PLAY-003-002 | TR-RIDE-PLAY-003 | FR-RIDE-026, FR-RIDE-215 | P4 | P11b | TEST-RIDE-019 |
| AC-RIDE-GPL-001-001 | TR-RIDE-GPL-001 | FR-RIDE-029, FR-RIDE-217 | P1, P11b | P11b | TEST-RIDE-020 |
| AC-RIDE-GPL-001-002 | TR-RIDE-GPL-001 | FR-RIDE-029, FR-RIDE-217 | P1, P11b | P11b | TEST-RIDE-020 |
| AC-RIDE-GPL-002-001 | TR-RIDE-GPL-002 | FR-RIDE-030 | P1 | P11b | TEST-RIDE-020 |
| AC-RIDE-GPL-002-002 | TR-RIDE-GPL-002 | FR-RIDE-030 | P1 | P11b | TEST-RIDE-020 |
| AC-RIDE-GPL-003-001 | TR-RIDE-GPL-003 | FR-RIDE-031 | P11b | P11b | TEST-RIDE-020 |
| AC-RIDE-GPL-003-002 | TR-RIDE-GPL-003 | FR-RIDE-031 | P11b | P11b | TEST-RIDE-020 |
| AC-RIDE-SERVER-001-001 | TR-RIDE-SERVER-001 | FR-RIDE-032 | P2 | P11b | TEST-RIDE-021 |
| AC-RIDE-SERVER-001-002 | TR-RIDE-SERVER-001 | FR-RIDE-032 | P2 | P11b | TEST-RIDE-021 |
| AC-RIDE-SERVER-002-001 | TR-RIDE-SERVER-002 | FR-RIDE-033, FR-RIDE-034 | P2 | P11b | TEST-RIDE-021 |
| AC-RIDE-SERVER-002-002 | TR-RIDE-SERVER-002 | FR-RIDE-033, FR-RIDE-034 | P2 | P11b | TEST-RIDE-021 |
| AC-RIDE-SERVER-003-001 | TR-RIDE-SERVER-003 | FR-RIDE-035 | P2 | P11b | TEST-RIDE-022 |
| AC-RIDE-SERVER-003-002 | TR-RIDE-SERVER-003 | FR-RIDE-035 | P2 | P11b | TEST-RIDE-022 |
| AC-RIDE-SERVER-004-001 | TR-RIDE-SERVER-004 | FR-RIDE-036, FR-RIDE-039 | P2 | P11b | TEST-RIDE-022, TEST-RIDE-024 |
| AC-RIDE-SERVER-004-002 | TR-RIDE-SERVER-004 | FR-RIDE-036, FR-RIDE-039 | P2 | P11b | TEST-RIDE-022, TEST-RIDE-024 |
| AC-RIDE-SERVER-005-001 | TR-RIDE-SERVER-005 | FR-RIDE-039, FR-RIDE-218 | P2 | P11b | TEST-RIDE-024 |
| AC-RIDE-SERVER-005-002 | TR-RIDE-SERVER-005 | FR-RIDE-039, FR-RIDE-218 | P2 | P11b | TEST-RIDE-024 |
| AC-RIDE-SERVER-006-001 | TR-RIDE-SERVER-006 | FR-RIDE-040 | P2 | P11b | TEST-RIDE-024 |
| AC-RIDE-SERVER-006-002 | TR-RIDE-SERVER-006 | FR-RIDE-040 | P2 | P11b | TEST-RIDE-024 |
| AC-RIDE-SERVER-007-001 | TR-RIDE-SERVER-007 | FR-RIDE-037, FR-RIDE-038 | P8 | P11b | TEST-RIDE-023 |
| AC-RIDE-SERVER-007-002 | TR-RIDE-SERVER-007 | FR-RIDE-037, FR-RIDE-038 | P8 | P11b | TEST-RIDE-023 |
| AC-RIDE-VIDEO-001-001 | TR-RIDE-VIDEO-001 | FR-RIDE-041, FR-RIDE-221 | P6, P7 | P11b | TEST-RIDE-025, TEST-RIDE-027 |
| AC-RIDE-VIDEO-001-002 | TR-RIDE-VIDEO-001 | FR-RIDE-041, FR-RIDE-221 | P6, P7 | P11b | TEST-RIDE-025, TEST-RIDE-027 |
| AC-RIDE-VIDEO-002-001 | TR-RIDE-VIDEO-002 | FR-RIDE-042, FR-RIDE-221 | P6, P7 | P11b | TEST-RIDE-025, TEST-RIDE-027 |
| AC-RIDE-VIDEO-002-002 | TR-RIDE-VIDEO-002 | FR-RIDE-042, FR-RIDE-221 | P6, P7 | P11b | TEST-RIDE-025, TEST-RIDE-027 |
| AC-RIDE-VIDEO-003-001 | TR-RIDE-VIDEO-003 | FR-RIDE-043, FR-RIDE-046 | P6 | P11b | TEST-RIDE-025, TEST-RIDE-026 |
| AC-RIDE-VIDEO-003-002 | TR-RIDE-VIDEO-003 | FR-RIDE-043, FR-RIDE-046 | P6 | P11b | TEST-RIDE-025, TEST-RIDE-026 |
| AC-RIDE-VIDEO-004-001 | TR-RIDE-VIDEO-004 | FR-RIDE-044, FR-RIDE-048 | P6 | P11b | TEST-RIDE-025, TEST-RIDE-026 |
| AC-RIDE-VIDEO-004-002 | TR-RIDE-VIDEO-004 | FR-RIDE-044, FR-RIDE-048 | P6 | P11b | TEST-RIDE-025, TEST-RIDE-026 |
| AC-RIDE-VIDEO-005-001 | TR-RIDE-VIDEO-005 | FR-RIDE-045 | P6 | P11b | TEST-RIDE-026 |
| AC-RIDE-VIDEO-005-002 | TR-RIDE-VIDEO-005 | FR-RIDE-045 | P6 | P11b | TEST-RIDE-026 |
| AC-RIDE-VIDEO-006-001 | TR-RIDE-VIDEO-006 | FR-RIDE-219, FR-RIDE-220 | P6 | P11b | TEST-RIDE-033 |
| AC-RIDE-VIDEO-006-002 | TR-RIDE-VIDEO-006 | FR-RIDE-219, FR-RIDE-220 | P6 | P11b | TEST-RIDE-033 |
| AC-RIDE-VIEW-001-001 | TR-RIDE-VIEW-001 | FR-RIDE-049, FR-RIDE-222 | P7 | P11b | TEST-RIDE-028 |
| AC-RIDE-VIEW-001-002 | TR-RIDE-VIEW-001 | FR-RIDE-049, FR-RIDE-222 | P7 | P11b | TEST-RIDE-028 |
| AC-RIDE-VIEW-002-001 | TR-RIDE-VIEW-002 | FR-RIDE-047, FR-RIDE-050 | P7 | P11b | TEST-RIDE-027, TEST-RIDE-028 |
| AC-RIDE-VIEW-002-002 | TR-RIDE-VIEW-002 | FR-RIDE-047, FR-RIDE-050 | P7 | P11b | TEST-RIDE-027, TEST-RIDE-028 |
| AC-RIDE-VIEW-003-001 | TR-RIDE-VIEW-003 | FR-RIDE-051 | P7 | P11b | TEST-RIDE-028 |
| AC-RIDE-VIEW-003-002 | TR-RIDE-VIEW-003 | FR-RIDE-051 | P7 | P11b | TEST-RIDE-028 |
| AC-RIDE-VIEW-004-001 | TR-RIDE-VIEW-004 | FR-RIDE-052 | P7 | P11b | TEST-RIDE-028 |
| AC-RIDE-VIEW-004-002 | TR-RIDE-VIEW-004 | FR-RIDE-052 | P7 | P11b | TEST-RIDE-028 |
| AC-RIDE-PRIV-001-001 | TR-RIDE-PRIV-001 | FR-RIDE-006, FR-RIDE-010 | P9, P10 | P11b | TEST-RIDE-006, TEST-RIDE-010 |
| AC-RIDE-PRIV-002-001 | TR-RIDE-PRIV-002 | FR-RIDE-014, FR-RIDE-202 | P10 | P11b | TEST-RIDE-012 |
| AC-RIDE-PRIV-002-002 | TR-RIDE-PRIV-002 | FR-RIDE-014, FR-RIDE-202 | P10 | P11b | TEST-RIDE-012 |
| AC-RIDE-PRIV-003-001 | TR-RIDE-PRIV-003 | FR-RIDE-010, FR-RIDE-210 | P10 | P11b | TEST-RIDE-010 |
| AC-RIDE-PRIV-003-002 | TR-RIDE-PRIV-003 | FR-RIDE-010, FR-RIDE-210 | P10 | P11b | TEST-RIDE-010 |
| AC-RIDE-SEC-001-001 | TR-RIDE-SEC-001 | FR-RIDE-201 | P2 | P11b | TEST-RIDE-029 |
| AC-RIDE-SEC-001-002 | TR-RIDE-SEC-001 | FR-RIDE-201 | P2 | P11b | TEST-RIDE-029 |
| AC-RIDE-SEC-002-001 | TR-RIDE-SEC-002 | FR-RIDE-014 | P10 | P11b | TEST-RIDE-012 |
| AC-RIDE-SEC-002-002 | TR-RIDE-SEC-002 | FR-RIDE-014 | P10 | P11b | TEST-RIDE-012 |
| AC-RIDE-SEC-003-001 | TR-RIDE-SEC-003 | FR-RIDE-203 | P10 | P11b | TEST-RIDE-029 |
| AC-RIDE-PERF-001-001 | TR-RIDE-PERF-001 | FR-RIDE-213 | P3 | P11b | TEST-RIDE-032 |
| AC-RIDE-PERF-001-002 | TR-RIDE-PERF-001 | FR-RIDE-213 | P3 | P11b | TEST-RIDE-032 |
| AC-RIDE-PERF-002-001 | TR-RIDE-PERF-002 | FR-RIDE-205 | P10 | P11b | TEST-RIDE-031 |
| AC-RIDE-PERF-003-001 | TR-RIDE-PERF-003 | FR-RIDE-220 | P6 | P11b | TEST-RIDE-033 |
| AC-RIDE-PERF-003-002 | TR-RIDE-PERF-003 | FR-RIDE-220 | P6 | P11b | TEST-RIDE-033 |
| AC-TEST-001-001 | TEST-RIDE-001 | FR-RIDE-001 | P9 | P11b | TEST-RIDE-001 |
| AC-TEST-001-002 | TEST-RIDE-001 | FR-RIDE-001 | P9 | P11b | TEST-RIDE-001 |
| AC-TEST-002-001 | TEST-RIDE-002 | FR-RIDE-002 | P9 | P11b | TEST-RIDE-002 |
| AC-TEST-002-002 | TEST-RIDE-002 | FR-RIDE-002 | P9 | P11b | TEST-RIDE-002 |
| AC-TEST-003-001 | TEST-RIDE-003 | FR-RIDE-003 | P9 | P11b | TEST-RIDE-003 |
| AC-TEST-003-002 | TEST-RIDE-003 | FR-RIDE-003 | P9 | P11b | TEST-RIDE-003 |
| AC-TEST-004-001 | TEST-RIDE-004 | FR-RIDE-004, FR-RIDE-011, FR-RIDE-012 | P9 | P11b | TEST-RIDE-004 |
| AC-TEST-004-002 | TEST-RIDE-004 | FR-RIDE-004, FR-RIDE-011, FR-RIDE-012 | P9 | P11b | TEST-RIDE-004 |
| AC-TEST-005-001 | TEST-RIDE-005 | FR-RIDE-005 | P9 | P11b | TEST-RIDE-005 |
| AC-TEST-005-002 | TEST-RIDE-005 | FR-RIDE-005 | P9 | P11b | TEST-RIDE-005 |
| AC-TEST-006-001 | TEST-RIDE-006 | FR-RIDE-006 | P9 | P11b | TEST-RIDE-006 |
| AC-TEST-006-002 | TEST-RIDE-006 | FR-RIDE-006 | P9 | P11b | TEST-RIDE-006 |
| AC-TEST-007-001 | TEST-RIDE-007 | FR-RIDE-007, FR-RIDE-209 | P8, P9 | P11b | TEST-RIDE-007 |
| AC-TEST-007-002 | TEST-RIDE-007 | FR-RIDE-007, FR-RIDE-209 | P8, P9 | P11b | TEST-RIDE-007 |
| AC-TEST-008-001 | TEST-RIDE-008 | FR-RIDE-008 | P8 | P11b | TEST-RIDE-008 |
| AC-TEST-008-002 | TEST-RIDE-008 | FR-RIDE-008 | P8 | P11b | TEST-RIDE-008 |
| AC-TEST-009-001 | TEST-RIDE-009 | FR-RIDE-009 | P8 | P11b | TEST-RIDE-009 |
| AC-TEST-009-002 | TEST-RIDE-009 | FR-RIDE-009 | P8 | P11b | TEST-RIDE-009 |
| AC-TEST-010-001 | TEST-RIDE-010 | FR-RIDE-010, FR-RIDE-210 | P10 | P11b | TEST-RIDE-010 |
| AC-TEST-010-002 | TEST-RIDE-010 | FR-RIDE-010, FR-RIDE-210 | P10 | P11b | TEST-RIDE-010 |
| AC-TEST-011-001 | TEST-RIDE-011 | FR-RIDE-013 | P9 | P11b | TEST-RIDE-011 |
| AC-TEST-011-002 | TEST-RIDE-011 | FR-RIDE-013 | P9 | P11b | TEST-RIDE-011 |
| AC-TEST-012-001 | TEST-RIDE-012 | FR-RIDE-014, FR-RIDE-202 | P10 | P11b | TEST-RIDE-012 |
| AC-TEST-012-002 | TEST-RIDE-012 | FR-RIDE-014, FR-RIDE-202 | P10 | P11b | TEST-RIDE-012 |
| AC-TEST-013-001 | TEST-RIDE-013 | FR-RIDE-015, FR-RIDE-016 | P3 | P11b | TEST-RIDE-013 |
| AC-TEST-013-002 | TEST-RIDE-013 | FR-RIDE-015, FR-RIDE-016 | P3 | P11b | TEST-RIDE-013 |
| AC-TEST-014-001 | TEST-RIDE-014 | FR-RIDE-017, FR-RIDE-018, FR-RIDE-212 | P3, P11a | P11b | TEST-RIDE-014 |
| AC-TEST-014-002 | TEST-RIDE-014 | FR-RIDE-017, FR-RIDE-018, FR-RIDE-212 | P3, P11a | P11b | TEST-RIDE-014 |
| AC-TEST-015-001 | TEST-RIDE-015 | FR-RIDE-019 | P3 | P11b | TEST-RIDE-015 |
| AC-TEST-015-002 | TEST-RIDE-015 | FR-RIDE-019 | P3 | P11b | TEST-RIDE-015 |
| AC-TEST-016-001 | TEST-RIDE-016 | FR-RIDE-020, FR-RIDE-021 | P8 | P11b | TEST-RIDE-016 |
| AC-TEST-016-002 | TEST-RIDE-016 | FR-RIDE-020, FR-RIDE-021 | P8 | P11b | TEST-RIDE-016 |
| AC-TEST-017-001 | TEST-RIDE-017 | FR-RIDE-022, FR-RIDE-023, FR-RIDE-216 | P5 | P11b | TEST-RIDE-017 |
| AC-TEST-017-002 | TEST-RIDE-017 | FR-RIDE-022, FR-RIDE-023, FR-RIDE-216 | P5 | P11b | TEST-RIDE-017 |
| AC-TEST-018-001 | TEST-RIDE-018 | FR-RIDE-024, FR-RIDE-028 | P5 | P11b | TEST-RIDE-018 |
| AC-TEST-018-002 | TEST-RIDE-018 | FR-RIDE-024, FR-RIDE-028 | P5 | P11b | TEST-RIDE-018 |
| AC-TEST-019-001 | TEST-RIDE-019 | FR-RIDE-025, FR-RIDE-026, FR-RIDE-027, FR-RIDE-215 | P4 | P11b | TEST-RIDE-019 |
| AC-TEST-019-002 | TEST-RIDE-019 | FR-RIDE-025, FR-RIDE-026, FR-RIDE-027, FR-RIDE-215 | P4 | P11b | TEST-RIDE-019 |
| AC-TEST-020-001 | TEST-RIDE-020 | FR-RIDE-029, FR-RIDE-030, FR-RIDE-031, FR-RIDE-217 | P1, P11b | P11b | TEST-RIDE-020 |
| AC-TEST-020-002 | TEST-RIDE-020 | FR-RIDE-029, FR-RIDE-030, FR-RIDE-031, FR-RIDE-217 | P1, P11b | P11b | TEST-RIDE-020 |
| AC-TEST-021-001 | TEST-RIDE-021 | FR-RIDE-032, FR-RIDE-033, FR-RIDE-034 | P2 | P11b | TEST-RIDE-021 |
| AC-TEST-021-002 | TEST-RIDE-021 | FR-RIDE-032, FR-RIDE-033, FR-RIDE-034 | P2 | P11b | TEST-RIDE-021 |
| AC-TEST-022-001 | TEST-RIDE-022 | FR-RIDE-035, FR-RIDE-036 | P2 | P11b | TEST-RIDE-022 |
| AC-TEST-022-002 | TEST-RIDE-022 | FR-RIDE-035, FR-RIDE-036 | P2 | P11b | TEST-RIDE-022 |
| AC-TEST-023-001 | TEST-RIDE-023 | FR-RIDE-037, FR-RIDE-038 | P8 | P11b | TEST-RIDE-023 |
| AC-TEST-023-002 | TEST-RIDE-023 | FR-RIDE-037, FR-RIDE-038 | P8 | P11b | TEST-RIDE-023 |
| AC-TEST-024-001 | TEST-RIDE-024 | FR-RIDE-039, FR-RIDE-040, FR-RIDE-218 | P2 | P11b | TEST-RIDE-024 |
| AC-TEST-024-002 | TEST-RIDE-024 | FR-RIDE-039, FR-RIDE-040, FR-RIDE-218 | P2 | P11b | TEST-RIDE-024 |
| AC-TEST-025-001 | TEST-RIDE-025 | FR-RIDE-041, FR-RIDE-042, FR-RIDE-043, FR-RIDE-044 | P6 | P11b | TEST-RIDE-025 |
| AC-TEST-025-002 | TEST-RIDE-025 | FR-RIDE-041, FR-RIDE-042, FR-RIDE-043, FR-RIDE-044 | P6 | P11b | TEST-RIDE-025 |
| AC-TEST-026-001 | TEST-RIDE-026 | FR-RIDE-045, FR-RIDE-046, FR-RIDE-048 | P6 | P11b | TEST-RIDE-026 |
| AC-TEST-026-002 | TEST-RIDE-026 | FR-RIDE-045, FR-RIDE-046, FR-RIDE-048 | P6 | P11b | TEST-RIDE-026 |
| AC-TEST-027-001 | TEST-RIDE-027 | FR-RIDE-047, FR-RIDE-221 | P7 | P11b | TEST-RIDE-027 |
| AC-TEST-027-002 | TEST-RIDE-027 | FR-RIDE-047, FR-RIDE-221 | P7 | P11b | TEST-RIDE-027 |
| AC-TEST-028-001 | TEST-RIDE-028 | FR-RIDE-049, FR-RIDE-050, FR-RIDE-051, FR-RIDE-052, FR-RIDE-222 | P7 | P11b | TEST-RIDE-028 |
| AC-TEST-028-002 | TEST-RIDE-028 | FR-RIDE-049, FR-RIDE-050, FR-RIDE-051, FR-RIDE-052, FR-RIDE-222 | P7 | P11b | TEST-RIDE-028 |
| AC-TEST-029-001 | TEST-RIDE-029 | FR-RIDE-201, FR-RIDE-203, FR-RIDE-214 | P2, P5, P10 | P11b | TEST-RIDE-029 |
| AC-TEST-029-002 | TEST-RIDE-029 | FR-RIDE-201, FR-RIDE-203, FR-RIDE-214 | P2, P5, P10 | P11b | TEST-RIDE-029 |
| AC-TEST-030-001 | TEST-RIDE-030 | FR-RIDE-204, FR-RIDE-206 | P9 | P11b | TEST-RIDE-030 |
| AC-TEST-030-002 | TEST-RIDE-030 | FR-RIDE-204, FR-RIDE-206 | P9 | P11b | TEST-RIDE-030 |
| AC-TEST-031-001 | TEST-RIDE-031 | FR-RIDE-205, FR-RIDE-207 | P10 | P11b | TEST-RIDE-031 |
| AC-TEST-031-002 | TEST-RIDE-031 | FR-RIDE-205, FR-RIDE-207 | P10 | P11b | TEST-RIDE-031 |
| AC-TEST-032-001 | TEST-RIDE-032 | FR-RIDE-208, FR-RIDE-211, FR-RIDE-213 | P3, P10 | P11b | TEST-RIDE-032 |
| AC-TEST-032-002 | TEST-RIDE-032 | FR-RIDE-208, FR-RIDE-211, FR-RIDE-213 | P3, P10 | P11b | TEST-RIDE-032 |
| AC-TEST-033-001 | TEST-RIDE-033 | FR-RIDE-219, FR-RIDE-220 | P6 | P11b | TEST-RIDE-033 |
| AC-TEST-033-002 | TEST-RIDE-033 | FR-RIDE-219, FR-RIDE-220 | P6 | P11b | TEST-RIDE-033 |
| AC-UC-001-001 | UC-RIDE-001 | FR-RIDE-001, FR-RIDE-003, FR-RIDE-006, FR-RIDE-013 | P9 | P11b | TEST-RIDE-001, TEST-RIDE-003, TEST-RIDE-006, TEST-RIDE-011 |
| AC-UC-001-002 | UC-RIDE-001 | FR-RIDE-001, FR-RIDE-003, FR-RIDE-006, FR-RIDE-013 | P9 | P11b | TEST-RIDE-001, TEST-RIDE-003, TEST-RIDE-006, TEST-RIDE-011 |
| AC-UC-002-001 | UC-RIDE-002 | FR-RIDE-002, FR-RIDE-006 | P9 | P11b | TEST-RIDE-002, TEST-RIDE-006 |
| AC-UC-002-002 | UC-RIDE-002 | FR-RIDE-002, FR-RIDE-006 | P9 | P11b | TEST-RIDE-002, TEST-RIDE-006 |
| AC-UC-003-001 | UC-RIDE-003 | FR-RIDE-004, FR-RIDE-011, FR-RIDE-012, FR-RIDE-204, FR-RIDE-206 | P9 | P11b | TEST-RIDE-004, TEST-RIDE-030 |
| AC-UC-003-002 | UC-RIDE-003 | FR-RIDE-004, FR-RIDE-011, FR-RIDE-012, FR-RIDE-204, FR-RIDE-206 | P9 | P11b | TEST-RIDE-004, TEST-RIDE-030 |
| AC-UC-004-001 | UC-RIDE-004 | FR-RIDE-005, FR-RIDE-006 | P9 | P11b | TEST-RIDE-005, TEST-RIDE-006 |
| AC-UC-004-002 | UC-RIDE-004 | FR-RIDE-005, FR-RIDE-006 | P9 | P11b | TEST-RIDE-005, TEST-RIDE-006 |
| AC-UC-005-001 | UC-RIDE-005 | FR-RIDE-007, FR-RIDE-209 | P8, P9 | P11b | TEST-RIDE-007 |
| AC-UC-005-002 | UC-RIDE-005 | FR-RIDE-007, FR-RIDE-209 | P8, P9 | P11b | TEST-RIDE-007 |
| AC-UC-006-001 | UC-RIDE-006 | FR-RIDE-008 | P8 | P11b | TEST-RIDE-008 |
| AC-UC-006-002 | UC-RIDE-006 | FR-RIDE-008 | P8 | P11b | TEST-RIDE-008 |
| AC-UC-007-001 | UC-RIDE-007 | FR-RIDE-009, FR-RIDE-038, FR-RIDE-207 | P8, P10 | P11b | TEST-RIDE-009, TEST-RIDE-023, TEST-RIDE-031 |
| AC-UC-007-002 | UC-RIDE-007 | FR-RIDE-009, FR-RIDE-038, FR-RIDE-207 | P8, P10 | P11b | TEST-RIDE-009, TEST-RIDE-023, TEST-RIDE-031 |
| AC-UC-008-001 | UC-RIDE-008 | FR-RIDE-010, FR-RIDE-202, FR-RIDE-203, FR-RIDE-208, FR-RIDE-210 | P10 | P11b | TEST-RIDE-010, TEST-RIDE-012, TEST-RIDE-029, TEST-RIDE-032 |
| AC-UC-008-002 | UC-RIDE-008 | FR-RIDE-010, FR-RIDE-202, FR-RIDE-203, FR-RIDE-208, FR-RIDE-210 | P10 | P11b | TEST-RIDE-010, TEST-RIDE-012, TEST-RIDE-029, TEST-RIDE-032 |
| AC-UC-009-001 | UC-RIDE-009 | FR-RIDE-015, FR-RIDE-016, FR-RIDE-017, FR-RIDE-018, FR-RIDE-019, FR-RIDE-201, FR-RIDE-211, FR-RIDE-212, FR-RIDE-213 | P2, P3, P11a | P11b | TEST-RIDE-013, TEST-RIDE-014, TEST-RIDE-015, TEST-RIDE-029, TEST-RIDE-032 |
| AC-UC-009-002 | UC-RIDE-009 | FR-RIDE-015, FR-RIDE-016, FR-RIDE-017, FR-RIDE-018, FR-RIDE-019, FR-RIDE-201, FR-RIDE-211, FR-RIDE-212, FR-RIDE-213 | P2, P3, P11a | P11b | TEST-RIDE-013, TEST-RIDE-014, TEST-RIDE-015, TEST-RIDE-029, TEST-RIDE-032 |
| AC-UC-010-001 | UC-RIDE-010 | FR-RIDE-020, FR-RIDE-021, FR-RIDE-028, FR-RIDE-214 | P5, P8 | P11b | TEST-RIDE-016, TEST-RIDE-018, TEST-RIDE-029 |
| AC-UC-010-002 | UC-RIDE-010 | FR-RIDE-020, FR-RIDE-021, FR-RIDE-028, FR-RIDE-214 | P5, P8 | P11b | TEST-RIDE-016, TEST-RIDE-018, TEST-RIDE-029 |
| AC-UC-011-001 | UC-RIDE-011 | FR-RIDE-022, FR-RIDE-023, FR-RIDE-024, FR-RIDE-214, FR-RIDE-216 | P5 | P11b | TEST-RIDE-017, TEST-RIDE-018, TEST-RIDE-029 |
| AC-UC-011-002 | UC-RIDE-011 | FR-RIDE-022, FR-RIDE-023, FR-RIDE-024, FR-RIDE-214, FR-RIDE-216 | P5 | P11b | TEST-RIDE-017, TEST-RIDE-018, TEST-RIDE-029 |
| AC-UC-012-001 | UC-RIDE-012 | FR-RIDE-025, FR-RIDE-026, FR-RIDE-027, FR-RIDE-215 | P4 | P11b | TEST-RIDE-019 |
| AC-UC-012-002 | UC-RIDE-012 | FR-RIDE-025, FR-RIDE-026, FR-RIDE-027, FR-RIDE-215 | P4 | P11b | TEST-RIDE-019 |
| AC-UC-013-001 | UC-RIDE-013 | FR-RIDE-029, FR-RIDE-030, FR-RIDE-031, FR-RIDE-217 | P1, P11b | P11b | TEST-RIDE-020 |
| AC-UC-013-002 | UC-RIDE-013 | FR-RIDE-029, FR-RIDE-030, FR-RIDE-031, FR-RIDE-217 | P1, P11b | P11b | TEST-RIDE-020 |
| AC-UC-014-001 | UC-RIDE-014 | FR-RIDE-031, FR-RIDE-032, FR-RIDE-033, FR-RIDE-034 | P2, P11b | P11b | TEST-RIDE-020, TEST-RIDE-021 |
| AC-UC-014-002 | UC-RIDE-014 | FR-RIDE-031, FR-RIDE-032, FR-RIDE-033, FR-RIDE-034 | P2, P11b | P11b | TEST-RIDE-020, TEST-RIDE-021 |
| AC-UC-015-001 | UC-RIDE-015 | FR-RIDE-035, FR-RIDE-036, FR-RIDE-037, FR-RIDE-039, FR-RIDE-040, FR-RIDE-218 | P2, P8 | P11b | TEST-RIDE-022, TEST-RIDE-023, TEST-RIDE-024 |
| AC-UC-015-002 | UC-RIDE-015 | FR-RIDE-035, FR-RIDE-036, FR-RIDE-037, FR-RIDE-039, FR-RIDE-040, FR-RIDE-218 | P2, P8 | P11b | TEST-RIDE-022, TEST-RIDE-023, TEST-RIDE-024 |
| AC-UC-016-001 | UC-RIDE-016 | FR-RIDE-037, FR-RIDE-038 | P8 | P11b | TEST-RIDE-023 |
| AC-UC-016-002 | UC-RIDE-016 | FR-RIDE-037, FR-RIDE-038 | P8 | P11b | TEST-RIDE-023 |
| AC-UC-017-001 | UC-RIDE-017 | FR-RIDE-041, FR-RIDE-042, FR-RIDE-043, FR-RIDE-044, FR-RIDE-045, FR-RIDE-046, FR-RIDE-048, FR-RIDE-219, FR-RIDE-220 | P6 | P11b | TEST-RIDE-025, TEST-RIDE-026, TEST-RIDE-033 |
| AC-UC-017-002 | UC-RIDE-017 | FR-RIDE-041, FR-RIDE-042, FR-RIDE-043, FR-RIDE-044, FR-RIDE-045, FR-RIDE-046, FR-RIDE-048, FR-RIDE-219, FR-RIDE-220 | P6 | P11b | TEST-RIDE-025, TEST-RIDE-026, TEST-RIDE-033 |
| AC-UC-018-001 | UC-RIDE-018 | FR-RIDE-047, FR-RIDE-048, FR-RIDE-050, FR-RIDE-221 | P6, P7 | P11b | TEST-RIDE-026, TEST-RIDE-027, TEST-RIDE-028 |
| AC-UC-018-002 | UC-RIDE-018 | FR-RIDE-047, FR-RIDE-048, FR-RIDE-050, FR-RIDE-221 | P6, P7 | P11b | TEST-RIDE-026, TEST-RIDE-027, TEST-RIDE-028 |
| AC-UC-019-001 | UC-RIDE-019 | FR-RIDE-028, FR-RIDE-049, FR-RIDE-050, FR-RIDE-051, FR-RIDE-052, FR-RIDE-222 | P5, P7 | P11b | TEST-RIDE-018, TEST-RIDE-028 |
| AC-UC-019-002 | UC-RIDE-019 | FR-RIDE-028, FR-RIDE-049, FR-RIDE-050, FR-RIDE-051, FR-RIDE-052, FR-RIDE-222 | P5, P7 | P11b | TEST-RIDE-018, TEST-RIDE-028 |
| AC-UC-020-001 | UC-RIDE-020 | FR-RIDE-011, FR-RIDE-012, FR-RIDE-014, FR-RIDE-202 | P9, P10 | P11b | TEST-RIDE-004, TEST-RIDE-012 |
| AC-UC-020-002 | UC-RIDE-020 | FR-RIDE-011, FR-RIDE-012, FR-RIDE-014, FR-RIDE-202 | P9, P10 | P11b | TEST-RIDE-004, TEST-RIDE-012 |
| AC-UC-021-001 | UC-RIDE-021 | FR-RIDE-205, FR-RIDE-207 | P10 | P11b | TEST-RIDE-031 |
| AC-UC-021-002 | UC-RIDE-021 | FR-RIDE-205, FR-RIDE-207 | P10 | P11b | TEST-RIDE-031 |
| AC-UC-025-001 | UC-RIDE-025 | FR-RIDE-056 | P6 | P11b | TEST-RIDE-035 |
| AC-UC-026-001 | UC-RIDE-026 | FR-RIDE-057 | P7 | P11b | TEST-RIDE-035 |
| AC-UC-027-001 | UC-RIDE-027 | FR-RIDE-058 | P7 | P11b | TEST-RIDE-035 |
| AC-UC-028-001 | UC-RIDE-028 | FR-RIDE-059 | P2 | P11b | TEST-RIDE-036 |
| AC-UC-029-001 | UC-RIDE-029 | FR-RIDE-060 | P1 | P11b | TEST-RIDE-037 |
| AC-UC-030-001 | UC-RIDE-030 | FR-RIDE-061 | P2 | P11b | TEST-RIDE-036 |
| AC-UC-031-001 | UC-RIDE-031 | FR-RIDE-062 | P1 | P11b | TEST-RIDE-037 |

Ledger row count: **404** (must equal source AC inventory).


---

## 3. Phased implementation slices

### Phase completion and BDPv4 evidence

P0 is documentation-only. P1–P10 are construction increments; completing an increment does **not** automatically satisfy every FR or broad TEST record named in its table. Before each increment, partition work by explicit AC IDs and name any later acceptance gate. For each next small behavior retain: RED evidence, passing mock validation, passing tests against the real target implementation, and refactor evidence with current+prior suites at Failed 0 and Skipped 0. External dependency mocks must be identified. Mock or contract-only results must never be labeled real-provider or end-to-end acceptance.

P1 provides contract-only evidence. P2 provides admission orchestration and fail-closed adapter boundaries; **real custody integration is accepted only after P3–P5**. TEST-RIDE-032 P3 scope = cryptographic agility and latency; retention completes in P10. TEST-RIDE-016 P5 scope = court-release path; verification UI completes in P8. TEST-RIDE-035 P6 scope = Android and shared UI; desktop and cross-client reuse complete in P7. TEST-RIDE-020 early scope = licensing/metadata; publication completes in P11b. Each partition must list its exact FR/TR/TEST/UC AC closure. **P11b** runs the complete integrated acceptance suite with no skipped requirements.

### Authoritative dependency table

| Phase | Depends on |
| --- | --- |
| P0 | none |
| P1 | P0 + section 8 gate |
| P2 | P1 |
| P3 | P1 |
| P4 | P3 |
| P5 | P3, P4 |
| P6 | P2, P3, P4, P5 |
| P7 | P3, P4, P5, P6 |
| P8 | P2, P5, P7 |
| P9 | P2, P3, P4, P5 |
| P10 | P2, P8, P9 |
| P11a | P3, P6 |
| P11b | P0–P10 + P11a alternate-provider conformance (production L2 activation not a release prerequisite) |

Phase numbering does not authorize bypassing a dependency.

### P0 — Requirements YAML repair and traceability validation (docs-only)

**Goal:** Consolidate UC YAML (done: single records key, 31 UCs); validate AC inventory and mappings; freeze architecture decisions; no application code

| Field | Value |
| --- | --- |
| FR IDs | (no application FR implementation ownership) |
| Files / projects | docs/Project/Use-Cases-Batch.yaml; docs/Project/Requirements-Mappings-Batch.yaml; docs/Project/README.md; docs/architecture/*; docs/plans/PLAN-RIDEAUDIT-001-implementation.md; docs/receipts/hv/; docs/reviews/hv-pairs/; docs/process/hostile-validation.md |
| Dependencies | None for documentation repair; P1 remains blocked by section 8 |
| AC / evidence scope | Documentation validation only |
| Exit criteria | Use-Cases-Batch.yaml strict-parses with exactly 31 unique localIds; All 84 mappings resolve; AC inventory 404 IDs present in plan appendix; Custody contracts section 4.5 approved in plan text; HV plan AGREE retained; No src/ application code |
| HV gate | Opposing-model AGREE; retain `docs/receipts/hv/*` JSONL **and** `docs/reviews/hv-pairs/*` canonical pair; commit immediately |

**BDPv4 notes:** Documentation validation only: strict YAML parsing, unique-ID checks, reference resolution, acceptance-criteria inventory, and Markdown consistency. No production code or application test projects in P0.

### P1 — Solution skeleton + authoritative protos (tests first, contract-only)

**Goal:** Create solution layout and GPL-2.0 .proto contracts; contract-only RED/GREEN; OpenAPI companion labeled non-authoritative

| Field | Value |
| --- | --- |
| FR IDs (implementation ownership) | FR-RIDE-029, FR-RIDE-030, FR-RIDE-060, FR-RIDE-062 |
| UC IDs | UC-RIDE-013, UC-RIDE-029, UC-RIDE-031 |
| TR IDs | TR-RIDE-GPL-001, TR-RIDE-GPL-002, TR-RIDE-GPL-005, TR-RIDE-SERVER-010 |
| TEST IDs | TEST-RIDE-020, TEST-RIDE-037 |
| FR-owned AC IDs | AC-RIDE-029-001, AC-RIDE-029-002, AC-RIDE-029-003, AC-RIDE-030-001, AC-RIDE-030-002, AC-RIDE-060-001, AC-RIDE-060-002, AC-RIDE-062-001, AC-RIDE-062-002 |
| AC closure | Union of FR-owned ACs above plus TR/TEST/UC-owned ACs for mapped records (see §2.7); partition per phase ac_scope |
| Files / projects | RideAudit.sln; src/RideAudit.Protos/; src/RideAudit.Contracts/; tests/RideAudit.Protos.Tests/; Directory.Build.props; LICENSE; NOTICE; artifacts/server-api/ARTIFACT.yaml |
| Dependencies | P0 complete + section 8 Astra AGREE + Payton AGREE |
| AC / evidence scope | Contract-only evidence; not real admission or custody |
| Exit criteria | protos compile; TEST-RIDE-037 contract-authority asserts Failed 0 Skipped 0 (companion notices); OpenAPI labeled companion; HV AGREE on skeleton |
| HV gate | Opposing-model AGREE; retain `docs/receipts/hv/*` JSONL **and** `docs/reviews/hv-pairs/*` canonical pair; commit immediately |

**BDPv4 notes:** RED contract tests then green; mocks for transport; no real chain/Play.

### P2 — gRPC admission service (fail-closed, sealed-only)

**Goal:** Sealed submit + admission verifier on .NET 10 containers; no decrypt at ingest; fail-closed adapters

| Field | Value |
| --- | --- |
| FR IDs (implementation ownership) | FR-RIDE-032, FR-RIDE-033, FR-RIDE-034, FR-RIDE-035, FR-RIDE-036, FR-RIDE-039, FR-RIDE-040, FR-RIDE-059, FR-RIDE-061, FR-RIDE-201, FR-RIDE-218 |
| UC IDs | UC-RIDE-014, UC-RIDE-015, UC-RIDE-028, UC-RIDE-030, UC-RIDE-009 |
| TR IDs | TR-RIDE-SERVER-001, TR-RIDE-SERVER-002, TR-RIDE-SERVER-003, TR-RIDE-SERVER-004, TR-RIDE-SERVER-005, TR-RIDE-SERVER-006, TR-RIDE-SERVER-008, TR-RIDE-SERVER-009, TR-RIDE-SEC-001 |
| TEST IDs | TEST-RIDE-021, TEST-RIDE-022, TEST-RIDE-024, TEST-RIDE-036, TEST-RIDE-029 |
| FR-owned AC IDs | AC-RIDE-032-001, AC-RIDE-032-002, AC-RIDE-032-003, AC-RIDE-033-001, AC-RIDE-033-002, AC-RIDE-034-001, AC-RIDE-034-002, AC-RIDE-035-001, AC-RIDE-035-002, AC-RIDE-036-001, AC-RIDE-036-002, AC-RIDE-039-001, AC-RIDE-039-002, AC-RIDE-039-003, AC-RIDE-040-001, AC-RIDE-040-002, AC-RIDE-059-001, AC-RIDE-059-002, AC-RIDE-061-001, AC-RIDE-061-002, AC-RIDE-201-001, AC-RIDE-201-002, AC-RIDE-218-001, AC-RIDE-218-002 |
| AC closure | Union of FR-owned ACs above plus TR/TEST/UC-owned ACs for mapped records (see §2.7); partition per phase ac_scope |
| Files / projects | src/RideAudit.Server.Admission/; src/RideAudit.Server.Identity/; deploy/containers/admission/; tests/RideAudit.Server.Admission.Tests/ |
| Dependencies | P1 |
| AC / evidence scope | Admission orchestration + fail-closed adapter boundaries; mock custody/attest until P3-P5 real gate |
| Exit criteria | TEST-RIDE-021/022/024/036 Failed 0 Skipped 0 for P2 AC partition; fail-closed on integrity miss; real custody acceptance deferred to after P3-P5 gate; HV AGREE |
| HV gate | Opposing-model AGREE; retain `docs/receipts/hv/*` JSONL **and** `docs/reviews/hv-pairs/*` canonical pair; commit immediately |

**BDPv4 notes:** BDPv4 RED-green-refactor; identify external mocks.

### P3 — Seal-at-collect + custody receipt + OTS chain

**Goal:** Collection-boundary sealer, immutable receipt-core, Bitcoin OTS writer, non-admission on chain failure

| Field | Value |
| --- | --- |
| FR IDs (implementation ownership) | FR-RIDE-015, FR-RIDE-016, FR-RIDE-017, FR-RIDE-018, FR-RIDE-019, FR-RIDE-211, FR-RIDE-212, FR-RIDE-213 |
| UC IDs | UC-RIDE-009 |
| TR IDs | TR-RIDE-STORE-001, TR-RIDE-STORE-002, TR-RIDE-SEAL-001, TR-RIDE-SEAL-002, TR-RIDE-CHAIN-001, TR-RIDE-CHAIN-002, TR-RIDE-CHAIN-003, TR-RIDE-SEAL-003, TR-RIDE-PERF-001 |
| TEST IDs | TEST-RIDE-013, TEST-RIDE-014, TEST-RIDE-015, TEST-RIDE-032 |
| FR-owned AC IDs | AC-RIDE-015-001, AC-RIDE-015-002, AC-RIDE-016-001, AC-RIDE-016-002, AC-RIDE-016-003, AC-RIDE-017-001, AC-RIDE-017-002, AC-RIDE-018-001, AC-RIDE-018-002, AC-RIDE-018-003, AC-RIDE-019-001, AC-RIDE-019-002, AC-RIDE-019-003, AC-RIDE-211-001, AC-RIDE-211-002, AC-RIDE-212-001, AC-RIDE-212-002, AC-RIDE-212-003, AC-RIDE-213-001, AC-RIDE-213-002 |
| AC closure | Union of FR-owned ACs above plus TR/TEST/UC-owned ACs for mapped records (see §2.7); partition per phase ac_scope |
| Files / projects | src/RideAudit.Seal/; src/RideAudit.Chain/; src/RideAudit.Chain.OpenTimestamps/; tests/RideAudit.Seal.Tests/; tests/RideAudit.Chain.Tests/ |
| Dependencies | P1 |
| AC / evidence scope | Seal+OTS; retention portion of TEST-032 deferred to P10 |
| Exit criteria | TEST-RIDE-013/014/015 Failed 0 Skipped 0; TEST-RIDE-032 only crypto-agility+latency partition; OTS primary profile btc-ots; HV AGREE |
| HV gate | Opposing-model AGREE; retain `docs/receipts/hv/*` JSONL **and** `docs/reviews/hv-pairs/*` canonical pair; commit immediately |

**BDPv4 notes:** BDPv4; real OTS calendar or recorded fixture per plan custody contracts

### P4 — Play Integrity attestation binding

**Goal:** Play Integrity before seal; attestation on receipt; package allowlist

| Field | Value |
| --- | --- |
| FR IDs (implementation ownership) | FR-RIDE-025, FR-RIDE-026, FR-RIDE-027, FR-RIDE-215 |
| UC IDs | UC-RIDE-012 |
| TR IDs | TR-RIDE-PLAY-001, TR-RIDE-PLAY-003, TR-RIDE-CHAIN-001, TR-RIDE-PLAY-002 |
| TEST IDs | TEST-RIDE-019 |
| FR-owned AC IDs | AC-RIDE-025-001, AC-RIDE-025-002, AC-RIDE-026-001, AC-RIDE-026-002, AC-RIDE-026-003, AC-RIDE-027-001, AC-RIDE-027-002, AC-RIDE-215-001, AC-RIDE-215-002, AC-RIDE-215-003 |
| AC closure | Union of FR-owned ACs above plus TR/TEST/UC-owned ACs for mapped records (see §2.7); partition per phase ac_scope |
| Files / projects | src/RideAudit.Attest/; tests/RideAudit.Attest.Tests/ |
| Dependencies | P3 |
| AC / evidence scope | Full Play Integrity AC closure for mapped FRs |
| Exit criteria | TEST-RIDE-019 Failed 0 Skipped 0; HV AGREE |
| HV gate | Opposing-model AGREE; retain `docs/receipts/hv/*` JSONL **and** `docs/reviews/hv-pairs/*` canonical pair; commit immediately |

**BDPv4 notes:** BDPv4

### P5 — Escrow M-of-N + court release

**Goal:** Escrow packaging, HSM/KMS custody, court-authorized release into expiring working copy

| Field | Value |
| --- | --- |
| FR IDs (implementation ownership) | FR-RIDE-022, FR-RIDE-023, FR-RIDE-024, FR-RIDE-214, FR-RIDE-216, FR-RIDE-028 |
| UC IDs | UC-RIDE-011, UC-RIDE-010, UC-RIDE-019 |
| TR IDs | TR-RIDE-ESCROW-001, TR-RIDE-ESCROW-003, TR-RIDE-ESCROW-002, TR-RIDE-CHAIN-004 |
| TEST IDs | TEST-RIDE-017, TEST-RIDE-018, TEST-RIDE-029, TEST-RIDE-016 (court-release documentation/path contribution only; verification UI in P8) |
| FR-owned AC IDs | AC-RIDE-022-001, AC-RIDE-022-002, AC-RIDE-023-001, AC-RIDE-023-002, AC-RIDE-023-003, AC-RIDE-024-001, AC-RIDE-024-002, AC-RIDE-024-003, AC-RIDE-214-001, AC-RIDE-214-002, AC-RIDE-216-001, AC-RIDE-216-002, AC-RIDE-028-001, AC-RIDE-028-002 |
| AC closure | Union of FR-owned ACs above plus TR/TEST/UC-owned ACs for mapped records (see §2.7); partition per phase ac_scope |
| Files / projects | src/RideAudit.Escrow/; tests/RideAudit.Escrow.Tests/ |
| Dependencies | P3; P4 |
| AC / evidence scope | Escrow+release; verification UI of TEST-016 completes in P8 |
| Exit criteria | TEST-RIDE-017/018 Failed 0 Skipped 0; TEST-RIDE-016 court-release path partition only; HV AGREE |
| HV gate | Opposing-model AGREE; retain `docs/receipts/hv/*` JSONL **and** `docs/reviews/hv-pairs/*` canonical pair; commit immediately |

**BDPv4 notes:** BDPv4

### P6 — Bluetooth dual-phone roles + Avalonia Android client

**Goal:** BT pairing; driver coordinator; passenger compositor; Avalonia UI 12 Android

| Field | Value |
| --- | --- |
| FR IDs (implementation ownership) | FR-RIDE-041, FR-RIDE-042, FR-RIDE-043, FR-RIDE-044, FR-RIDE-045, FR-RIDE-046, FR-RIDE-048, FR-RIDE-053, FR-RIDE-054, FR-RIDE-055, FR-RIDE-056, FR-RIDE-219, FR-RIDE-220 |
| UC IDs | UC-RIDE-017, UC-RIDE-018, UC-RIDE-022, UC-RIDE-023, UC-RIDE-024, UC-RIDE-025 |
| TR IDs | TR-RIDE-VIDEO-001, TR-RIDE-VIDEO-002, TR-RIDE-VIDEO-003, TR-RIDE-VIDEO-004, TR-RIDE-VIDEO-005, TR-RIDE-VIDEO-010, TR-RIDE-VIDEO-011, TR-RIDE-VIDEO-012, TR-RIDE-VIDEO-006, TR-RIDE-PERF-003 |
| TEST IDs | TEST-RIDE-025, TEST-RIDE-026, TEST-RIDE-034, TEST-RIDE-035, TEST-RIDE-033 |
| FR-owned AC IDs | AC-RIDE-041-001, AC-RIDE-041-002, AC-RIDE-042-001, AC-RIDE-042-002, AC-RIDE-043-001, AC-RIDE-043-002, AC-RIDE-044-001, AC-RIDE-044-002, AC-RIDE-045-001, AC-RIDE-045-002, AC-RIDE-046-001, AC-RIDE-046-002, AC-RIDE-048-001, AC-RIDE-048-002, AC-RIDE-053-001, AC-RIDE-053-002, AC-RIDE-054-001, AC-RIDE-054-002, AC-RIDE-055-001, AC-RIDE-055-002, AC-RIDE-056-001, AC-RIDE-056-002, AC-RIDE-219-001, AC-RIDE-219-002, AC-RIDE-219-003, AC-RIDE-220-001, AC-RIDE-220-002 |
| AC closure | Union of FR-owned ACs above plus TR/TEST/UC-owned ACs for mapped records (see §2.7); partition per phase ac_scope |
| Files / projects | src/RideAudit.Client.Android/; src/RideAudit.Shared.Ui/; src/RideAudit.Bt/; src/RideAudit.Video/; tests/RideAudit.Bt.Tests/; tests/RideAudit.Video.Tests/; artifacts/android/ |
| Dependencies | P2; P3; P4; P5 |
| AC / evidence scope | Android+BT+video; desktop portion of TEST-035 in P7 |
| Exit criteria | TEST-RIDE-025/026/033/034 Failed 0 Skipped 0; TEST-RIDE-035 Android+shared UI partition only; no Lyft private APIs; HV AGREE |
| HV gate | Opposing-model AGREE; retain `docs/receipts/hv/*` JSONL **and** `docs/reviews/hv-pairs/*` canonical pair; commit immediately |

**BDPv4 notes:** BDPv4; demonstrate P2-P5 sealed upload path

### P7 — Avalonia desktop court/counsel viewer

**Goal:** Fail-closed verification, escrow decrypt working copy, synchronized timeline, ViewerSession/VerificationReport

| Field | Value |
| --- | --- |
| FR IDs (implementation ownership) | FR-RIDE-047, FR-RIDE-049, FR-RIDE-050, FR-RIDE-051, FR-RIDE-052, FR-RIDE-057, FR-RIDE-058, FR-RIDE-221, FR-RIDE-222 |
| UC IDs | UC-RIDE-018, UC-RIDE-019, UC-RIDE-026, UC-RIDE-027 |
| TR IDs | TR-RIDE-VIEW-002, TR-RIDE-VIEW-001, TR-RIDE-VIEW-003, TR-RIDE-VIEW-004, TR-RIDE-VIEW-005, TR-RIDE-GPL-004, TR-RIDE-VIDEO-001, TR-RIDE-VIDEO-002 |
| TEST IDs | TEST-RIDE-027, TEST-RIDE-028, TEST-RIDE-035 |
| FR-owned AC IDs | AC-RIDE-047-001, AC-RIDE-047-002, AC-RIDE-049-001, AC-RIDE-049-002, AC-RIDE-050-001, AC-RIDE-050-002, AC-RIDE-051-001, AC-RIDE-051-002, AC-RIDE-052-001, AC-RIDE-052-002, AC-RIDE-057-001, AC-RIDE-057-002, AC-RIDE-058-001, AC-RIDE-058-002, AC-RIDE-221-001, AC-RIDE-221-002, AC-RIDE-222-001, AC-RIDE-222-002 |
| AC closure | Union of FR-owned ACs above plus TR/TEST/UC-owned ACs for mapped records (see §2.7); partition per phase ac_scope |
| Files / projects | src/RideAudit.Client.Desktop/; src/RideAudit.Shared.Ui/; tests/RideAudit.Client.Desktop.Tests/ |
| Dependencies | P3; P4; P5; P6 |
| AC / evidence scope | Desktop viewer + shared GPL Avalonia reuse |
| Exit criteria | TEST-RIDE-027/028 Failed 0 Skipped 0; TEST-RIDE-035 desktop+cross-client reuse partition; fail-closed before decrypt; HV AGREE |
| HV gate | Opposing-model AGREE; retain `docs/receipts/hv/*` JSONL **and** `docs/reviews/hv-pairs/*` canonical pair; commit immediately |

**BDPv4 notes:** BDPv4

### P8 — Counsel multi-driver bundle + analysis working copy

**Goal:** Multi-driver provenance, counsel bundle, authorized working-copy analytics; counsel container evidence for FR-059/TEST-036

| Field | Value |
| --- | --- |
| FR IDs (implementation ownership) | FR-RIDE-007, FR-RIDE-008, FR-RIDE-009, FR-RIDE-020, FR-RIDE-021, FR-RIDE-037, FR-RIDE-038 |
| UC IDs | UC-RIDE-005, UC-RIDE-006, UC-RIDE-007, UC-RIDE-010, UC-RIDE-015, UC-RIDE-016 |
| TR IDs | TR-RIDE-ANAL-002, TR-RIDE-ANAL-003, TR-RIDE-ANAL-004, TR-RIDE-ANAL-001, TR-RIDE-CHAIN-004, TR-RIDE-SERVER-007 |
| TEST IDs | TEST-RIDE-007, TEST-RIDE-008, TEST-RIDE-009, TEST-RIDE-016, TEST-RIDE-023, TEST-RIDE-036 (counsel-service container contribution for FR-RIDE-059) |
| FR-owned AC IDs | AC-RIDE-007-001, AC-RIDE-007-002, AC-RIDE-008-001, AC-RIDE-008-002, AC-RIDE-008-003, AC-RIDE-009-001, AC-RIDE-009-002, AC-RIDE-020-001, AC-RIDE-020-002, AC-RIDE-020-003, AC-RIDE-021-001, AC-RIDE-021-002, AC-RIDE-021-003, AC-RIDE-037-001, AC-RIDE-037-002, AC-RIDE-038-001, AC-RIDE-038-002 |
| AC closure | Union of FR-owned ACs above plus TR/TEST/UC-owned ACs for mapped records (see §2.7); partition per phase ac_scope |
| Files / projects | src/RideAudit.Server.Counsel/; src/RideAudit.Anal/; deploy/containers/counsel/; tests/RideAudit.Anal.Tests/; tests/RideAudit.Server.Counsel.Tests/ |
| Dependencies | P2; P5; P7 |
| AC / evidence scope | Counsel+anal+verification UI completion for TEST-016 |
| Exit criteria | TEST-RIDE-007/008/009/016/023/036 counsel partition Failed 0 Skipped 0; HV AGREE |
| HV gate | Opposing-model AGREE; retain `docs/receipts/hv/*` JSONL **and** `docs/reviews/hv-pairs/*` canonical pair; commit immediately |

**BDPv4 notes:** BDPv4

### P9 — Ingest pipelines (privacy ZIP, Smooth Cruiser, telematics)

**Goal:** Honest provenance ingest; no undocumented Lyft APIs; partnership gates; authorized working-copy only after seal

| Field | Value |
| --- | --- |
| FR IDs (implementation ownership) | FR-RIDE-001, FR-RIDE-002, FR-RIDE-003, FR-RIDE-004, FR-RIDE-005, FR-RIDE-006, FR-RIDE-011, FR-RIDE-012, FR-RIDE-013, FR-RIDE-204, FR-RIDE-206, FR-RIDE-209 |
| UC IDs | UC-RIDE-001, UC-RIDE-002, UC-RIDE-003, UC-RIDE-004, UC-RIDE-020, UC-RIDE-005 |
| TR IDs | TR-RIDE-INGEST-001, TR-RIDE-INGEST-006, TR-RIDE-INGEST-002, TR-RIDE-INGEST-003, TR-RIDE-INGEST-004, TR-RIDE-INGEST-005, TR-RIDE-PRIV-001, TR-RIDE-STORE-001, TR-RIDE-STORE-002, TR-RIDE-ANAL-002 |
| TEST IDs | TEST-RIDE-001, TEST-RIDE-002, TEST-RIDE-003, TEST-RIDE-004, TEST-RIDE-005, TEST-RIDE-006, TEST-RIDE-011, TEST-RIDE-030, TEST-RIDE-007 |
| FR-owned AC IDs | AC-RIDE-001-001, AC-RIDE-001-002, AC-RIDE-001-003, AC-RIDE-002-001, AC-RIDE-002-002, AC-RIDE-002-003, AC-RIDE-003-001, AC-RIDE-003-002, AC-RIDE-004-001, AC-RIDE-004-002, AC-RIDE-004-003, AC-RIDE-005-001, AC-RIDE-005-002, AC-RIDE-005-003, AC-RIDE-006-001, AC-RIDE-006-002, AC-RIDE-011-001, AC-RIDE-011-002, AC-RIDE-012-001, AC-RIDE-012-002, AC-RIDE-013-001, AC-RIDE-013-002, AC-RIDE-204-001, AC-RIDE-204-002, AC-RIDE-206-001, AC-RIDE-206-002, AC-RIDE-209-001 |
| AC closure | Union of FR-owned ACs above plus TR/TEST/UC-owned ACs for mapped records (see §2.7); partition per phase ac_scope |
| Files / projects | src/RideAudit.Ingest/; tests/RideAudit.Ingest.Tests/ |
| Dependencies | P2; P3; P4; P5 |
| AC / evidence scope | Ingest; no plaintext at public ingest |
| Exit criteria | TEST-RIDE-001..006/011/030 Failed 0 Skipped 0; HV AGREE |
| HV gate | Opposing-model AGREE; retain `docs/receipts/hv/*` JSONL **and** `docs/reviews/hv-pairs/*` canonical pair; commit immediately |

**BDPv4 notes:** BDPv4

### P10 — Privacy, RBAC, retention, portability NFRs

**Goal:** DSAR, legal hold, RBAC, retention, portable ZIP, TLS/vault, access logs

| Field | Value |
| --- | --- |
| FR IDs (implementation ownership) | FR-RIDE-010, FR-RIDE-014, FR-RIDE-202, FR-RIDE-203, FR-RIDE-205, FR-RIDE-207, FR-RIDE-208, FR-RIDE-210 |
| UC IDs | UC-RIDE-008, UC-RIDE-020, UC-RIDE-021, UC-RIDE-007 |
| TR IDs | TR-RIDE-STORE-003, TR-RIDE-PRIV-001, TR-RIDE-PRIV-003, TR-RIDE-PRIV-002, TR-RIDE-SEC-002, TR-RIDE-SEC-003, TR-RIDE-PERF-002, TR-RIDE-STORE-001, TR-RIDE-STORE-002 |
| TEST IDs | TEST-RIDE-010, TEST-RIDE-012, TEST-RIDE-029, TEST-RIDE-031, TEST-RIDE-032 |
| FR-owned AC IDs | AC-RIDE-010-001, AC-RIDE-010-002, AC-RIDE-014-001, AC-RIDE-014-002, AC-RIDE-202-001, AC-RIDE-202-002, AC-RIDE-203-001, AC-RIDE-205-001, AC-RIDE-205-002, AC-RIDE-207-001, AC-RIDE-208-001, AC-RIDE-208-002, AC-RIDE-210-001 |
| AC closure | Union of FR-owned ACs above plus TR/TEST/UC-owned ACs for mapped records (see §2.7); partition per phase ac_scope |
| Files / projects | src/RideAudit.Privacy/; src/RideAudit.Sec/; tests/RideAudit.Privacy.Tests/; tests/RideAudit.Sec.Tests/ |
| Dependencies | P2; P8; P9 |
| AC / evidence scope | Privacy/RBAC/retention completion |
| Exit criteria | TEST-RIDE-010/012/029/031 Failed 0 Skipped 0; TEST-RIDE-032 retention partition; HV AGREE |
| HV gate | Opposing-model AGREE; retain `docs/receipts/hv/*` JSONL **and** `docs/reviews/hv-pairs/*` canonical pair; commit immediately |

**BDPv4 notes:** BDPv4

### P11a — Alternate public-chain provider and optional dual-anchor

**Goal:** Alternate-provider and profile-selection conformance for FR-RIDE-212; OTS remains default; production L2 optional

| Field | Value |
| --- | --- |
| FR IDs (implementation ownership) | FR-RIDE-212 |
| UC IDs | UC-RIDE-009 |
| TR IDs | TR-RIDE-CHAIN-002 |
| TEST IDs | TEST-RIDE-014 |
| FR-owned AC IDs | AC-RIDE-212-001, AC-RIDE-212-002, AC-RIDE-212-003 |
| AC closure | Union of FR-owned ACs above plus TR/TEST/UC-owned ACs for mapped records (see §2.7); partition per phase ac_scope |
| Files / projects | src/RideAudit.Chain.EthL2/; docs/architecture/blockchain-custody-receipts.md |
| Dependencies | P3; P6 |
| AC / evidence scope | Chain profile conformance; not general distribution |
| Exit criteria | TEST-RIDE-014 for each supported profile Failed 0 Skipped 0; HV AGREE |
| HV gate | Opposing-model AGREE; retain `docs/receipts/hv/*` JSONL **and** `docs/reviews/hv-pairs/*` canonical pair; commit immediately |

**BDPv4 notes:** BDPv4

### P11b — Mandatory integrated acceptance and distribution

**Goal:** GPL distribution, Play+source publication receipts, complete integrated suite, Dev/Staging/Prod CD

| Field | Value |
| --- | --- |
| FR IDs (implementation ownership) | FR-RIDE-031, FR-RIDE-217 |
| UC IDs | UC-RIDE-013, UC-RIDE-014 |
| TR IDs | TR-RIDE-GPL-003, TR-RIDE-GPL-001 |
| TEST IDs | TEST-RIDE-020 |
| FR-owned AC IDs | AC-RIDE-031-001, AC-RIDE-031-002, AC-RIDE-217-001, AC-RIDE-217-002 |
| AC closure | Union of FR-owned ACs above plus TR/TEST/UC-owned ACs for mapped records (see §2.7); partition per phase ac_scope |
| Files / projects | packaging/; deploy/cicd/; docs/reviews/hv-pairs/ |
| Dependencies | P0-P10 complete + P11a alternate-provider conformance (L2 prod activation not required) |
| AC / evidence scope | Full integrated acceptance + distribution |
| Exit criteria | Complete mapped acceptance suite Failed 0 Skipped 0; real Android-to-gRPC-to-custody-to-escrow-to-desktop flows; cross-tenant rejection; retention/legal-hold; signed/reproducible desktop builds Win/Linux/macOS; successful automated Development/Staging/Production delivery with retained deployment and verification receipts; opposing-model AGREE; Play+source receipts for FR-031 |
| HV gate | Opposing-model AGREE; retain `docs/receipts/hv/*` JSONL **and** `docs/reviews/hv-pairs/*` canonical pair; commit immediately |

**BDPv4 notes:** Full suite green; no skips

**P11b mandatory release gate:** P11b cannot close until automated CI/CD has built, tested, and deployed the release through Development, Staging, and Production. Retain pipeline-run references, source commit, artifact/container digests, environment configuration versions, deployment results, and post-deployment verification results for each environment. Production promotion requires successful staging integrated acceptance and the section 7.3 backup-restore, compatibility, and pending-operation replay rehearsal. Failed deployment or verification blocks release completion. Desktop release evidence must cover signed and reproducible builds on Windows, Linux, and macOS. Play and public-source publication require actual receipts.


---

## 4. Architecture alignment

### 4.1 Clients (Avalonia UI 12)

| Surface | Project | FR | Notes |
| --- | --- | --- | --- |
| Android dual-phone | `RideAudit.Client.Android` | FR-RIDE-056, 053–055, 041–048 | Driver coordinator + passenger compositor |
| Desktop court/counsel | `RideAudit.Client.Desktop` | FR-RIDE-057, 049–052 | Fail-closed verify before decrypt |
| Shared UI | `RideAudit.Shared.Ui` | FR-RIDE-058 | GPL-2.0 shared Avalonia; accepted with P7 |

### 4.2 Backend (gRPC .NET 10 containers)

| Service | Responsibility | FR |
| --- | --- | --- |
| Admission | Sealed-only submit; verify; no decrypt | FR-RIDE-035, 036, 059, 061 |
| Identity / vehicle | Driver account, vehicle registry, profiles | FR-RIDE-032–034 |
| Counsel | Multi-driver bundle, disclosure | FR-RIDE-037–038; container evidence in P8 for FR-059/TEST-036 |
| Chain writer | OTS primary; optional L2 | FR-RIDE-018, 212 |

Proto authority: `RideAudit.Protos` GPL-2.0 (FR-RIDE-060). OpenAPI companion non-authoritative (FR-RIDE-062).

### 4.3 Bluetooth pairing

Per dual-phone-bluetooth-roles.md and FR-RIDE-053–055 / TR-RIDE-VIDEO-010–011 / TEST-RIDE-034 / UC-RIDE-022–024: BT discovery → explicit roles → driver coordinates clock/start/stop/submit → passenger video sync/composite/telematics. RideAudit pairing only — not a Lyft BT API.

### 4.4 Video / telematics

On-device/edge composite is first-class sealed evidence (FR-RIDE-045). Optional raw streams (FR-RIDE-046). Quotas/perf (FR-RIDE-219/220, TEST-RIDE-033).

### 4.5 Custody anchoring

- Primary: Bitcoin OpenTimestamps (`btc-ots`).
- Optional dual-anchor: Base / Polygon L2 for fast confirm; OTS remains long-term unless jurisdiction config says otherwise.
- Fail-closed: chain write failure → non-admission (FR-RIDE-019).
- Viewer independently verifies OTS/hashes/Play/escrow **before** decrypt (FR-RIDE-050).

#### Custody contracts required before P1 (P0 must document)

P0 must approve and document:

1. **Immutable receipt-core schema**, canonical serialization and digest rules, attestation/key/session binding, and a separate append-only **anchor-proof envelope**. Confirmation updates must not rewrite sealed ciphertext or previously committed receipt core.
2. Exactly which required receipt fields are public vs cryptographically committed. Reconcile with FR-RIDE-017, FR-RIDE-018, FR-RIDE-027 and their ACs before implementation.
3. For **btc-ots**: submission, pending-proof storage, proof upgrade, independent verification, and the source/meaning of every required chain ID, transaction hash, block height, and timestamp. Do not fabricate unsupported transaction metadata or silently weaken an AC; any necessary requirement amendment must be reviewed before P1. A pending response alone cannot satisfy confirmation.
4. States: local sealed-pending, confirmed, rejected/quarantined, admitted — including retry, duplicate submission, crash/restart, confirmation loss, and policy-version behavior. Retry must preserve ciphertext and receipt-core identity.
5. Public admission requires all configured receipt, hash, attestation, key/session binding, authorization, and escrow prerequisites. Missing or unavailable real adapters fail closed.
6. Before capture integration: Play verification before key generation/sealing; controlled key-generation and off-device escrow boundary; passenger-composite-to-driver-upload handoff. P6 must demonstrate these with P2–P5 implementations. P9 parsing/analysis must use the authorized working-copy path after sealing and must not introduce plaintext processing at public ingest.
7. Required hostile cases assigned to TEST-RIDE-013/014/015/017/019/022/028/032 and their AC closure: receipt-field mutation, proof/record substitution, pending proof, failed confirmation, retries after restart, stale/mismatched attestation, unavailable escrow, unauthorized driver/vehicle, independent viewer verification.

P0 cannot close with these contract decisions unresolved in the plan/docs.

---

## 5. Artifact packages → code evolution

| Artifact | Today | Evolves into |
| --- | --- | --- |
| ART-RIDE-ANDROID-001 | Historical Kotlin/Gradle scaffold | P6 archives historical Kotlin/Gradle placeholders under `artifacts/android/legacy-kotlin/` in a dedicated commit, excluding them from active build entry points. Keep `artifacts/android/ARTIFACT.yaml` as the stable artifact record and add an explicit implementation pointer to `src/RideAudit.Client.Android/`. Update version, implementation status, build instructions, FR links, and validation receipts. Do not mark implemented before P6 evidence passes. |
| ART-RIDE-API-001 | Interim OpenAPI companion | P1 establishes `src/RideAudit.Protos/` as the sole authoritative API/code-generation source. Keep OpenAPI as an explicitly labeled, manually maintained human-readable companion. Update ARTIFACT.yaml and README with authoritative proto paths and contract version. P2 supplies admission container evidence and P8 supplies counsel container evidence; metadata must distinguish contract-ready from runtime-validated status. TEST-RIDE-037 checks authority and notices. |
| ART-RIDE-MOUNT-001 | Parametric OpenSCAD | Remains independent hardware artifact; existing path and identity authoritative. |
| ART-RIDE-UX-001 | Mobile UX | Implementation acceptance references for P6. |
| ART-RIDE-UX-REVIEW-001 | Desktop review UX | Use this ID consistently for desktop UX artifact; acceptance references for P7. |

Preserve stable artifact IDs. Update `docs/artifacts/INDEX.md` plus package metadata in the phase that changes the implementation pointer.

---

## 6. Hostile validation schedule

### 6.1 Plan HV (this document)

Astra plan review is an **explicit operator-authorized** review for PLAN-RIDEAUDIT-001. This exception does **not** replace the product generator/validator matrix in `docs/process/hostile-validation.md`. Record the exception in process documentation before accepting the plan (P0 docs).

| Round | Validator | Model | Result |
| --- | --- | --- | --- |
| R1 | Codex | gpt-6-astra xhigh | DISAGREE@0 — Windows sandbox OS206 blocked reads |
| R2 | Codex | gpt-6-astra xhigh | DISAGREE@0 — same OS206 (subst R: did not help) |
| R3 | Codex | gpt-6-astra xhigh | DISAGREE@78 — substantive; used `--dangerously-bypass-approvals-and-sandbox` for read-only review only |
| R4+ | Codex | gpt-6-astra xhigh | Re-review this revision |

The older sandbox invocation must not be represented as a successful recipe for this environment. Filesystem access success does not imply plan agreement.

### 6.2 Retention (plan and product)

For every plan and product review, retain:

1. Raw request/response JSONL under `docs/receipts/hv/`, and
2. Canonical single JSON pair under `docs/reviews/hv-pairs/` with `request`, `response`, `generator`, `validator`, and `committed_at` members.

Preserve exact payloads, errors, and actual model metadata. Commit immediately upon receipt before relying on the result.

The orchestration owner writes and commits review receipts; a read-only reviewer does not. Before P0 closes, reconcile every completed review, including R1-R3, into the required canonical JSON pair alongside its original raw JSONL. Preserve the exact request and response payloads, actual model metadata, errors, and failed or unavailable status. Record the original source paths and custody commits in a review index. For reconstructed pairs, record reconstruction time separately; do not invent historical receipt timestamps or backdate commits. Record any unavailable metadata truthfully. Every subsequent completed review, including this round, receives both retained forms before its result is relied upon.


### 6.3 Product HV

| Generator | Required HV |
| --- | --- |
| grok-4.6-xhigh | Codex gpt-6-sol xhigh |
| gpt-6-sol xhigh | Grok grok-4.6-xhigh |

A failed, unavailable, unauthenticated, or partial product HV blocks phase acceptance. Record the actual availability result; do not substitute a model or infer a pass. ChatGPT-authenticated Codex cannot use gpt-6-sol until API-key auth exists — record unavailable truthfully.

---

## 7. Risks, open questions, rollback

### 7.1 Risks

| ID | Risk | Mitigation |
| --- | --- | --- |
| R1 | Duplicate YAML keys / parser drift | P0 strict parse; single records key (done for UC) |
| R2 | Kotlin scaffold mistaken for target | Archive under legacy-kotlin in P6; ARTIFACT pointer |
| R3 | OpenAPI treated as wire truth | FR-RIDE-062 / TEST-RIDE-037 |
| R4 | Decrypt at public ingest | FR-RIDE-035/061; tests |
| R5 | BT/Lyft API confusion | FR-RIDE-011; non-goal |
| R6 | OTS latency / fabricated tx metadata | Section 4.5 contracts; FR-RIDE-019 |
| R7 | gpt-6-sol HV unavailable | Record unavailable; never fake pass |
| R8 | MCP_UNTRUSTED | File-based BDPv4 until healthy |
| R9 | Broad TEST IDs spanning phases | Explicit AC partitions (§3) |
| R10 | Escrow key loss | FR-RIDE-216; HSM/KMS |

### 7.2 Open questions (decision deadlines)

| Decision | Deadline |
| --- | --- |
| Payton-approved Android API/device matrix, desktop OS/test matrix, and pinned .NET SDK/Avalonia versions | Before P1 |
| Controlled key-custody and escrow choices | Before P3 |
| Chain profiles and confirmation policies | Before P3 |
| Quantitative sync, quota, performance thresholds | Before P6 |
| Retention profiles | Before P10 |
| OTS fee sponsorship vs per-driver wallet | Before P3 |
| Concierge partnership authorization | Before enabling FR-RIDE-004 |

Unresolved decisions block the affected phase. Concierge remains disabled without partnership authorization. **No UC/Markdown waivers.**

### 7.3 Rollback

- Plan/docs commits: `git revert`; no runtime impact.
- After code exists: phase tags `rideaudit-pN-exit`; restore prior application/container version with **compatible schemas, receipt/proof readers, key versions, pending uploads, and pending anchor operations**.
- Pause admission when compatibility cannot be proven; preserve sealed data and append-only audit history.
- Test backup restore and pending-operation replay in staging.
- Never delete public receipts or rewrite ciphertext to make rollback succeed.
- Failed HV: do not merge phase; keep DISAGREE pairs as custody of the finding.

---

## 8. Explicit no-app-code gate

No application implementation, application skeletons, application test projects, or generated application bindings may be created until **all three** conditions hold:

1. P0 documentation repair and traceability validation are complete.
2. Astra has reviewed that exact plan and supporting-document revision and returned READY, overallVerdict AGREE, accuracy ≥ 98, completeness ≥ 98, and confidence ≥ 98; the exact review pair is retained and committed.
3. Payton has explicitly agreed to the same reviewed revision.

All conditions are mandatory. Residual findings, unavailable validation, partial validation, Markdown substitutes, and documentation waivers do **not** satisfy this gate. Material changes to the reviewed scope or contracts require renewed review and agreement.

Before the gate, only documentation, requirement YAML, plans, process records, and review receipts may change. P0 documentation repair may proceed before agreement; it does **not** authorize P1.

---

## 9. Acceptance of this plan

- [ ] P0 documentation repair complete (UC YAML consolidated; AC inventory; custody contracts text; process HV exception note)
- [ ] Astra READY + AGREE with accuracy/completeness/confidence ≥98 on this revision; pairs committed
- [ ] Payton AGREE on the same revision
- [ ] Only then may implementers begin P1 tests-first skeleton

## 10. Primary implementation ownership; final acceptance tracked separately

Appendix rows name the **primary implementation owner**. Final acceptance of broad TEST records may complete in a later phase per §3 partitions. **No application FR has P0 as its implementation owner.**

| FR | Primary impl owner |
| --- | --- |
| FR-RIDE-001 | P9 |
| FR-RIDE-002 | P9 |
| FR-RIDE-003 | P9 |
| FR-RIDE-004 | P9 |
| FR-RIDE-005 | P9 |
| FR-RIDE-006 | P9 |
| FR-RIDE-007 | P8 |
| FR-RIDE-008 | P8 |
| FR-RIDE-009 | P8 |
| FR-RIDE-010 | P10 |
| FR-RIDE-011 | P9 |
| FR-RIDE-012 | P9 |
| FR-RIDE-013 | P9 |
| FR-RIDE-014 | P10 |
| FR-RIDE-015 | P3 |
| FR-RIDE-016 | P3 |
| FR-RIDE-017 | P3 |
| FR-RIDE-018 | P3 |
| FR-RIDE-019 | P3 |
| FR-RIDE-020 | P8 |
| FR-RIDE-021 | P8 |
| FR-RIDE-022 | P5 |
| FR-RIDE-023 | P5 |
| FR-RIDE-024 | P5 |
| FR-RIDE-025 | P4 |
| FR-RIDE-026 | P4 |
| FR-RIDE-027 | P4 |
| FR-RIDE-028 | P5 |
| FR-RIDE-029 | P1 |
| FR-RIDE-030 | P1 |
| FR-RIDE-031 | P11b |
| FR-RIDE-032 | P2 |
| FR-RIDE-033 | P2 |
| FR-RIDE-034 | P2 |
| FR-RIDE-035 | P2 |
| FR-RIDE-036 | P2 |
| FR-RIDE-037 | P8 |
| FR-RIDE-038 | P8 |
| FR-RIDE-039 | P2 |
| FR-RIDE-040 | P2 |
| FR-RIDE-041 | P6 |
| FR-RIDE-042 | P6 |
| FR-RIDE-043 | P6 |
| FR-RIDE-044 | P6 |
| FR-RIDE-045 | P6 |
| FR-RIDE-046 | P6 |
| FR-RIDE-047 | P7 |
| FR-RIDE-048 | P6 |
| FR-RIDE-049 | P7 |
| FR-RIDE-050 | P7 |
| FR-RIDE-051 | P7 |
| FR-RIDE-052 | P7 |
| FR-RIDE-201 | P2 |
| FR-RIDE-202 | P10 |
| FR-RIDE-203 | P10 |
| FR-RIDE-204 | P9 |
| FR-RIDE-205 | P10 |
| FR-RIDE-206 | P9 |
| FR-RIDE-207 | P10 |
| FR-RIDE-208 | P10 |
| FR-RIDE-209 | P9 |
| FR-RIDE-210 | P10 |
| FR-RIDE-211 | P3 |
| FR-RIDE-212 | P3 (OTS default) + P11a (alternate profiles) |
| FR-RIDE-213 | P3 |
| FR-RIDE-214 | P5 |
| FR-RIDE-215 | P4 |
| FR-RIDE-216 | P5 |
| FR-RIDE-217 | P11b |
| FR-RIDE-218 | P2 |
| FR-RIDE-219 | P6 |
| FR-RIDE-220 | P6 |
| FR-RIDE-221 | P7 |
| FR-RIDE-222 | P7 |
| FR-RIDE-056 | P6 |
| FR-RIDE-057 | P7 |
| FR-RIDE-058 | P7 |
| FR-RIDE-059 | P2 |
| FR-RIDE-060 | P1 |
| FR-RIDE-061 | P2 |
| FR-RIDE-062 | P1 |
| FR-RIDE-053 | P6 |
| FR-RIDE-054 | P6 |
| FR-RIDE-055 | P6 |

Unassigned FR count: 0 — (none)

---

**End of PLAN-RIDEAUDIT-001 revision r3**




