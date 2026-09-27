# PLAN-RIDEAUDIT-001 — RideAudit implementation plan (BDPv4)

**Plan ID:** PLAN-RIDEAUDIT-001  
**Workspace:** `F:\GitHub\rideaudit` → https://github.com/sharpninja/rideaudit  
**Branch track:** `origin/master`  
**Author (git):** Sharp Ninja `<ninja@thesharp.ninja>`  
**Process:** Byrd Dev Process v4 (BDPv4) — `McpServer/docs/Development-Process-draft-v4.md`  
**Generator for this plan:** Grok (executor) — plan-only; **no Avalonia/gRPC application code in this task**  
**Hostile plan reviewer (required):** Codex / **gpt-6-astra** at **xhigh** (opposing model; same pattern as McpServer session-lifecycle plan reviews)  
**Status:** DRAFT pending Astra AGREE + Payton AGREE  
**Created:** 2026-09-27 (America/Chicago)  

> **HARD GATE:** Still **no** Avalonia UI / gRPC application implementation until this plan receives **Astra `overallVerdict: AGREE`** and **Payton AGREE**. Docs, plans, HV receipts, and requirement YAML completion only until then.

---

## 1. Scope, goals, non-goals, baseline

### 1.1 Problem / V²

RideAudit is a rideshare telematics audit system for Lyft-class drivers: dual-phone capture, seal-at-collect evidence, public blockchain custody receipts, fail-closed court/counsel review, and sealed-only public admission. Stack is locked: **Avalonia UI 12** (Android dual-phone + desktop court/counsel) and **gRPC on .NET 10 containers**. License: **GPL-2.0**.

Viable and valuable (`V²`): yes — court-admissible custody + honest provenance without inventing Lyft private APIs.

### 1.2 Goals

1. Produce a BDPv4-complete phased implementation plan with FR ↔ UC ↔ AC ↔ TR ↔ TEST for every slice.
2. Align architecture to locked stack, BT roles (FR-RIDE-053–055), OTS custody (primary) + optional ETH L2.
3. Evolve existing artifact packages into real code **after** plan AGREE.
4. Schedule opposing-model HV gates; retain request/response JSON pairs under `docs/receipts/hv/` (and mirror process path `docs/reviews/hv-pairs/` when product HV runs).
5. Keep implementers from writing app code until Astra + Payton AGREE.

### 1.3 Non-goals (this plan / this task)

- Implementing Avalonia screens, gRPC service bodies, BT stacks, video pipelines, or chain writers in this task.
- McpServer wiki triage / BUG-TRIAGE / plugin handoff work.
- Inventing Lyft private APIs or Concierge partnership without admin gates.
- Claiming Play Store publication complete.
- Committing secrets, `AGENTS-README-FIRST.yaml`, or `mcp.db`.
- Treating interim `artifacts/server-api/openapi.yaml` as the authoritative wire contract.
- Using private/permissioned chains as the sole custody ledger.

### 1.4 Current baseline (what exists)

| Area | State |
| --- | --- |
| App source (`src/`) | **Absent** — docs-only repo |
| Requirements YAML | FR 84, TR 64, TEST 37, mappings 84/84; UC YAML **only** UC-RIDE-025..031 |
| UC markdown | UC-RIDE-001..031 under `docs/ux/use-cases/` |
| Stack decision | Recorded `docs/architecture/stack.md` (2026-09-27) |
| Custody arch | `docs/architecture/blockchain-custody-receipts.md` (OTS primary) |
| BT roles | `docs/architecture/dual-phone-bluetooth-roles.md` |
| Artifacts | ART-RIDE-ANDROID-001 (Kotlin scaffold superseded), ART-RIDE-API-001 (OpenAPI companion), ART-RIDE-MOUNT-001 (OpenSCAD), ART-RIDE-UX-001 / UX-REVIEW-001 |
| Wiki | GitHub + Azure wiki exports under `docs/Project/wiki/` |
| Process | `docs/process/hostile-validation.md`, `code-generation.md` |
| MCP | `MCP_UNTRUSTED` — do not treat MCP as registered until health recovers |
| Git | tracks `origin/master` |

### 1.5 BDPv4 order (mandatory)

1. Requirements captured (mostly done; **P0 closes UC YAML gap**).
2. This plan + **Astra READY/AGREE** + Payton AGREE.
3. Per phase: write tests for next small behavior (RED) → mocks green → real implementation green → refactor.
4. Full suite Failed 0 Skipped 0 to exit a phase.
5. Opposing-model HV AGREE; retain JSON pairs; commit immediately.
6. Only then mark phase TODO done.

Code gen may use `grok-4.6-xhigh` or `gpt-6-sol` xhigh. Product HV must use the **opposing** agent/model per `docs/process/hostile-validation.md`. **Plan** HV for this document uses **gpt-6-astra** xhigh (Codex), matching prior successful McpServer plan-review receipts.

---

## 2. Inventory and coverage matrix

### 2.1 Counts

| Kind | Count | Source files |
| --- | ---: | --- |
| FR | 84 | Functional-Requirements-Batch + Additive Bluetooth + Additive Avalonia/gRPC |
| TR | 64 | Technical-Requirements-Batch + additives |
| TEST | 37 | Testing-Requirements-Batch + additives |
| UC (YAML) | 7 | Use-Cases-Batch.yaml (**gap:** 001..024 missing) |
| UC (markdown) | 31 | docs/ux/use-cases/UC-RIDE-001..031.md |
| Mappings | 84 | Requirements-Mappings-Batch.yaml (1:1 per FR) |

### 2.2 Coverage integrity (verified 2026-09-27)

- Every FR has a mapping row (trIds + testIds + useCaseLocalIds).
- Every mapped TR/TEST ID exists in TR/TEST batches.
- **BLOCKING DOC GAP:** mappings reference UC-RIDE-001..024 but `Use-Cases-Batch.yaml` only contains UC-RIDE-025..031. Markdown UC docs exist. **P0 must add YAML UC records (or an explicit Payton-approved waiver).**

### 2.3 Full FR coverage matrix

| FR | Priority | Title | TR | TEST | UC | AC IDs |
| --- | --- | --- | --- | --- | --- | --- |
| FR-RIDE-001 | high | Ingest Lyft privacy-export ZIP | TR-RIDE-INGEST-001, TR-RIDE-INGEST-006 | TEST-RIDE-001 | UC-RIDE-001 | AC-RIDE-001-001, AC-RIDE-001-002, AC-RIDE-001-003 |
| FR-RIDE-002 | high | Record Smooth Cruiser scores | TR-RIDE-INGEST-002 | TEST-RIDE-002 | UC-RIDE-002 | AC-RIDE-002-001, AC-RIDE-002-002, AC-RIDE-002-003 |
| FR-RIDE-003 | high | Ingest trip-level records | TR-RIDE-INGEST-003 | TEST-RIDE-003 | UC-RIDE-001 | AC-RIDE-003-001, AC-RIDE-003-002 |
| FR-RIDE-004 | medium | Optional Concierge/Business API integration | TR-RIDE-INGEST-004 | TEST-RIDE-004 | UC-RIDE-003 | AC-RIDE-004-001, AC-RIDE-004-002, AC-RIDE-004-003 |
| FR-RIDE-005 | high | Import third-party telematics | TR-RIDE-INGEST-005, TR-RIDE-INGEST-006 | TEST-RIDE-005 | UC-RIDE-004 | AC-RIDE-005-001, AC-RIDE-005-002, AC-RIDE-005-003 |
| FR-RIDE-006 | critical | Provenance and consent ledger | TR-RIDE-INGEST-006, TR-RIDE-PRIV-001 | TEST-RIDE-006 | UC-RIDE-001, UC-RIDE-002, UC-RIDE-004 | AC-RIDE-006-001, AC-RIDE-006-002 |
| FR-RIDE-007 | high | Coverage matrix | TR-RIDE-ANAL-002 | TEST-RIDE-007 | UC-RIDE-005 | AC-RIDE-007-001, AC-RIDE-007-002 |
| FR-RIDE-008 | high | Online-hours policy audit | TR-RIDE-ANAL-003 | TEST-RIDE-008 | UC-RIDE-006 | AC-RIDE-008-001, AC-RIDE-008-002, AC-RIDE-008-003 |
| FR-RIDE-009 | high | Time-window incident report | TR-RIDE-ANAL-004 | TEST-RIDE-009 | UC-RIDE-007 | AC-RIDE-009-001, AC-RIDE-009-002 |
| FR-RIDE-010 | critical | Data subject access and deletion | TR-RIDE-STORE-003, TR-RIDE-PRIV-001, TR-RIDE-PRIV-003 | TEST-RIDE-010 | UC-RIDE-008 | AC-RIDE-010-001, AC-RIDE-010-002 |
| FR-RIDE-011 | critical | No undocumented Lyft private APIs | TR-RIDE-INGEST-001, TR-RIDE-INGEST-002 | TEST-RIDE-004 | UC-RIDE-003, UC-RIDE-020 | AC-RIDE-011-001, AC-RIDE-011-002 |
| FR-RIDE-012 | medium | Admin partnership gates | TR-RIDE-INGEST-004 | TEST-RIDE-004 | UC-RIDE-003, UC-RIDE-020 | AC-RIDE-012-001, AC-RIDE-012-002 |
| FR-RIDE-013 | high | Hash and version raw imports | TR-RIDE-STORE-001, TR-RIDE-STORE-002 | TEST-RIDE-011 | UC-RIDE-001 | AC-RIDE-013-001, AC-RIDE-013-002 |
| FR-RIDE-014 | critical | Role-based access | TR-RIDE-PRIV-002, TR-RIDE-SEC-002 | TEST-RIDE-012 | UC-RIDE-020 | AC-RIDE-014-001, AC-RIDE-014-002 |
| FR-RIDE-015 | critical | Seal and encrypt at collection | TR-RIDE-STORE-001, TR-RIDE-STORE-002, TR-RIDE-SEAL-001 | TEST-RIDE-013 | UC-RIDE-009 | AC-RIDE-015-001, AC-RIDE-015-002 |
| FR-RIDE-016 | critical | Per-session or per-sample keys | TR-RIDE-SEAL-002 | TEST-RIDE-013 | UC-RIDE-009 | AC-RIDE-016-001, AC-RIDE-016-002, AC-RIDE-016-003 |
| FR-RIDE-017 | critical | Custody receipt content | TR-RIDE-CHAIN-001 | TEST-RIDE-014 | UC-RIDE-009 | AC-RIDE-017-001, AC-RIDE-017-002 |
| FR-RIDE-018 | critical | Blockchain receipt write | TR-RIDE-CHAIN-002 | TEST-RIDE-014 | UC-RIDE-009 | AC-RIDE-018-001, AC-RIDE-018-002, AC-RIDE-018-003 |
| FR-RIDE-019 | critical | Blockchain write failure policy | TR-RIDE-CHAIN-003 | TEST-RIDE-015 | UC-RIDE-009 | AC-RIDE-019-001, AC-RIDE-019-002, AC-RIDE-019-003 |
| FR-RIDE-020 | critical | Court-review decryption path docs | TR-RIDE-ANAL-001 | TEST-RIDE-016 | UC-RIDE-010 | AC-RIDE-020-001, AC-RIDE-020-002, AC-RIDE-020-003 |
| FR-RIDE-021 | critical | Verification UI/report | TR-RIDE-ANAL-001, TR-RIDE-CHAIN-004 | TEST-RIDE-016 | UC-RIDE-010 | AC-RIDE-021-001, AC-RIDE-021-002, AC-RIDE-021-003 |
| FR-RIDE-022 | critical | M-of-N key escrow | TR-RIDE-ESCROW-001 | TEST-RIDE-017 | UC-RIDE-011 | AC-RIDE-022-001, AC-RIDE-022-002 |
| FR-RIDE-023 | critical | Escrow separation from device | TR-RIDE-ESCROW-001, TR-RIDE-ESCROW-003 | TEST-RIDE-017 | UC-RIDE-011 | AC-RIDE-023-001, AC-RIDE-023-002, AC-RIDE-023-003 |
| FR-RIDE-024 | critical | Court-authorized escrow release | TR-RIDE-ESCROW-002 | TEST-RIDE-018 | UC-RIDE-011 | AC-RIDE-024-001, AC-RIDE-024-002, AC-RIDE-024-003 |
| FR-RIDE-025 | critical | Play Integrity key binding | TR-RIDE-PLAY-001 | TEST-RIDE-019 | UC-RIDE-012 | AC-RIDE-025-001, AC-RIDE-025-002 |
| FR-RIDE-026 | critical | Reject failed Play Integrity | TR-RIDE-PLAY-001, TR-RIDE-PLAY-003 | TEST-RIDE-019 | UC-RIDE-012 | AC-RIDE-026-001, AC-RIDE-026-002, AC-RIDE-026-003 |
| FR-RIDE-027 | critical | Attestation on custody receipt | TR-RIDE-CHAIN-001, TR-RIDE-PLAY-002 | TEST-RIDE-019 | UC-RIDE-012 | AC-RIDE-027-001, AC-RIDE-027-002 |
| FR-RIDE-028 | critical | Counsel verification steps | TR-RIDE-CHAIN-004, TR-RIDE-ESCROW-002 | TEST-RIDE-018 | UC-RIDE-010, UC-RIDE-019 | AC-RIDE-028-001, AC-RIDE-028-002 |
| FR-RIDE-029 | critical | GPL-2.0 licensing | TR-RIDE-GPL-001 | TEST-RIDE-020 | UC-RIDE-013 | AC-RIDE-029-001, AC-RIDE-029-002, AC-RIDE-029-003 |
| FR-RIDE-030 | high | GPL2 notices on artifacts | TR-RIDE-GPL-002 | TEST-RIDE-020 | UC-RIDE-013 | AC-RIDE-030-001, AC-RIDE-030-002 |
| FR-RIDE-031 | high | Publish client via Play and source repo | TR-RIDE-GPL-003 | TEST-RIDE-020 | UC-RIDE-013, UC-RIDE-014 | AC-RIDE-031-001, AC-RIDE-031-002 |
| FR-RIDE-032 | critical | Public-server driver account | TR-RIDE-SERVER-001 | TEST-RIDE-021 | UC-RIDE-014 | AC-RIDE-032-001, AC-RIDE-032-002, AC-RIDE-032-003 |
| FR-RIDE-033 | high | Vehicle registration | TR-RIDE-SERVER-002 | TEST-RIDE-021 | UC-RIDE-014 | AC-RIDE-033-001, AC-RIDE-033-002 |
| FR-RIDE-034 | high | Configuration profile gate | TR-RIDE-SERVER-002 | TEST-RIDE-021 | UC-RIDE-014 | AC-RIDE-034-001, AC-RIDE-034-002 |
| FR-RIDE-035 | critical | Sealed-only submission API | TR-RIDE-SERVER-003 | TEST-RIDE-022 | UC-RIDE-015 | AC-RIDE-035-001, AC-RIDE-035-002 |
| FR-RIDE-036 | critical | Admission verification chain | TR-RIDE-SERVER-004 | TEST-RIDE-022 | UC-RIDE-015 | AC-RIDE-036-001, AC-RIDE-036-002 |
| FR-RIDE-037 | critical | Multi-driver per-record provenance | TR-RIDE-SERVER-007 | TEST-RIDE-023 | UC-RIDE-015, UC-RIDE-016 | AC-RIDE-037-001, AC-RIDE-037-002 |
| FR-RIDE-038 | high | Counsel multi-driver bundle | TR-RIDE-SERVER-007 | TEST-RIDE-023 | UC-RIDE-007, UC-RIDE-016 | AC-RIDE-038-001, AC-RIDE-038-002 |
| FR-RIDE-039 | critical | Public-server abuse controls | TR-RIDE-SERVER-004, TR-RIDE-SERVER-005 | TEST-RIDE-024 | UC-RIDE-015 | AC-RIDE-039-001, AC-RIDE-039-002, AC-RIDE-039-003 |
| FR-RIDE-040 | critical | Multi-tenant isolation | TR-RIDE-SERVER-006 | TEST-RIDE-024 | UC-RIDE-015 | AC-RIDE-040-001, AC-RIDE-040-002 |
| FR-RIDE-041 | high | Dual-phone video capture | TR-RIDE-VIDEO-001 | TEST-RIDE-025 | UC-RIDE-017 | AC-RIDE-041-001, AC-RIDE-041-002 |
| FR-RIDE-042 | high | Shared session clock sync | TR-RIDE-VIDEO-002 | TEST-RIDE-025 | UC-RIDE-017 | AC-RIDE-042-001, AC-RIDE-042-002 |
| FR-RIDE-043 | high | On-device/edge compositing | TR-RIDE-VIDEO-003 | TEST-RIDE-025 | UC-RIDE-017 | AC-RIDE-043-001, AC-RIDE-043-002 |
| FR-RIDE-044 | high | Spider-graph overlay | TR-RIDE-VIDEO-004 | TEST-RIDE-025 | UC-RIDE-017 | AC-RIDE-044-001, AC-RIDE-044-002 |
| FR-RIDE-045 | critical | Seal composite as first-class evidence | TR-RIDE-VIDEO-005 | TEST-RIDE-026 | UC-RIDE-017 | AC-RIDE-045-001, AC-RIDE-045-002 |
| FR-RIDE-046 | medium | Optional raw stream sealing | TR-RIDE-VIDEO-003 | TEST-RIDE-026 | UC-RIDE-017 | AC-RIDE-046-001, AC-RIDE-046-002 |
| FR-RIDE-047 | critical | Counsel composite playback | TR-RIDE-VIEW-002 | TEST-RIDE-027 | UC-RIDE-018 | AC-RIDE-047-001, AC-RIDE-047-002 |
| FR-RIDE-048 | high | Composite metadata in custody package | TR-RIDE-VIDEO-004 | TEST-RIDE-026 | UC-RIDE-017, UC-RIDE-018 | AC-RIDE-048-001, AC-RIDE-048-002 |
| FR-RIDE-049 | critical | GPL-2.0 desktop court viewer | TR-RIDE-VIEW-001 | TEST-RIDE-028 | UC-RIDE-019 | AC-RIDE-049-001, AC-RIDE-049-002 |
| FR-RIDE-050 | critical | Viewer fail-closed verification | TR-RIDE-VIEW-002 | TEST-RIDE-028 | UC-RIDE-018, UC-RIDE-019 | AC-RIDE-050-001, AC-RIDE-050-002 |
| FR-RIDE-051 | high | Synchronized timeline display | TR-RIDE-VIEW-003 | TEST-RIDE-028 | UC-RIDE-019 | AC-RIDE-051-001, AC-RIDE-051-002 |
| FR-RIDE-052 | critical | ViewerSession and VerificationReport | TR-RIDE-VIEW-004 | TEST-RIDE-028 | UC-RIDE-019 | AC-RIDE-052-001, AC-RIDE-052-002 |
| FR-RIDE-201 | critical | TLS and secrets vault | TR-RIDE-SEC-001 | TEST-RIDE-029 | UC-RIDE-009 | AC-RIDE-201-001, AC-RIDE-201-002 |
| FR-RIDE-202 | critical | Geolocation sensitive masking | TR-RIDE-PRIV-002 | TEST-RIDE-012 | UC-RIDE-008, UC-RIDE-020 | AC-RIDE-202-001, AC-RIDE-202-002 |
| FR-RIDE-203 | critical | Append-only access logs | TR-RIDE-SEC-003 | TEST-RIDE-029 | UC-RIDE-008 | AC-RIDE-203-001 |
| FR-RIDE-204 | high | Concierge ingestion resilience | TR-RIDE-STORE-001, TR-RIDE-STORE-002 | TEST-RIDE-030 | UC-RIDE-003 | AC-RIDE-204-001, AC-RIDE-204-002 |
| FR-RIDE-205 | high | Scale multi-year histories | TR-RIDE-PERF-002 | TEST-RIDE-031 | UC-RIDE-021 | AC-RIDE-205-001, AC-RIDE-205-002 |
| FR-RIDE-206 | critical | No false Smooth Cruiser labeling | TR-RIDE-STORE-001, TR-RIDE-STORE-002 | TEST-RIDE-030 | UC-RIDE-003 | AC-RIDE-206-001, AC-RIDE-206-002 |
| FR-RIDE-207 | high | Portable audit ZIP export | TR-RIDE-STORE-001, TR-RIDE-STORE-002 | TEST-RIDE-031 | UC-RIDE-007, UC-RIDE-021 | AC-RIDE-207-001 |
| FR-RIDE-208 | high | Per-state retention config | TR-RIDE-STORE-003 | TEST-RIDE-032 | UC-RIDE-008 | AC-RIDE-208-001, AC-RIDE-208-002 |
| FR-RIDE-209 | medium | In-product API gap notice | TR-RIDE-ANAL-002 | TEST-RIDE-007 | UC-RIDE-005 | AC-RIDE-209-001 |
| FR-RIDE-210 | critical | Legal hold suspends deletion | TR-RIDE-STORE-003, TR-RIDE-PRIV-003 | TEST-RIDE-010 | UC-RIDE-008 | AC-RIDE-210-001 |
| FR-RIDE-211 | high | Cryptographic agility | TR-RIDE-SEAL-003 | TEST-RIDE-032 | UC-RIDE-009 | AC-RIDE-211-001, AC-RIDE-211-002 |
| FR-RIDE-212 | high | Configurable public chain | TR-RIDE-CHAIN-002 | TEST-RIDE-014 | UC-RIDE-009 | AC-RIDE-212-001, AC-RIDE-212-002, AC-RIDE-212-003 |
| FR-RIDE-213 | high | Seal/receipt latency budget | TR-RIDE-PERF-001 | TEST-RIDE-032 | UC-RIDE-009 | AC-RIDE-213-001, AC-RIDE-213-002 |
| FR-RIDE-214 | critical | HSM/KMS key custody | TR-RIDE-ESCROW-003 | TEST-RIDE-029 | UC-RIDE-010, UC-RIDE-011 | AC-RIDE-214-001, AC-RIDE-214-002 |
| FR-RIDE-215 | critical | Play authenticity allowlist | TR-RIDE-PLAY-001, TR-RIDE-PLAY-003 | TEST-RIDE-019 | UC-RIDE-012 | AC-RIDE-215-001, AC-RIDE-215-002, AC-RIDE-215-003 |
| FR-RIDE-216 | critical | Escrow resilience | TR-RIDE-ESCROW-001 | TEST-RIDE-017 | UC-RIDE-011 | AC-RIDE-216-001, AC-RIDE-216-002 |
| FR-RIDE-217 | critical | GPL-2.0 governance NFR | TR-RIDE-GPL-001 | TEST-RIDE-020 | UC-RIDE-013 | AC-RIDE-217-001, AC-RIDE-217-002 |
| FR-RIDE-218 | critical | Public-server admission capacity | TR-RIDE-SERVER-005 | TEST-RIDE-024 | UC-RIDE-015 | AC-RIDE-218-001, AC-RIDE-218-002 |
| FR-RIDE-219 | high | Video storage and bandwidth quotas | TR-RIDE-VIDEO-006 | TEST-RIDE-033 | UC-RIDE-017 | AC-RIDE-219-001, AC-RIDE-219-002, AC-RIDE-219-003 |
| FR-RIDE-220 | high | Video performance thresholds | TR-RIDE-VIDEO-006, TR-RIDE-PERF-003 | TEST-RIDE-033 | UC-RIDE-017 | AC-RIDE-220-001, AC-RIDE-220-002 |
| FR-RIDE-221 | high | Composite integrity for playback | TR-RIDE-VIDEO-001, TR-RIDE-VIDEO-002 | TEST-RIDE-027 | UC-RIDE-018 | AC-RIDE-221-001, AC-RIDE-221-002 |
| FR-RIDE-222 | critical | Desktop portability fail-closed | TR-RIDE-VIEW-001 | TEST-RIDE-028 | UC-RIDE-019 | AC-RIDE-222-001, AC-RIDE-222-002 |
| FR-RIDE-056 | high | Avalonia UI 12 Android dual-phone capture client | TR-RIDE-VIDEO-012 | TEST-RIDE-035 | UC-RIDE-025 | AC-RIDE-056-001, AC-RIDE-056-002 |
| FR-RIDE-057 | high | Avalonia UI 12 desktop court viewer | TR-RIDE-VIEW-005 | TEST-RIDE-035 | UC-RIDE-026 | AC-RIDE-057-001, AC-RIDE-057-002 |
| FR-RIDE-058 | high | Shared Avalonia UI 12 constraints under GPL-2.0 | TR-RIDE-GPL-004 | TEST-RIDE-035 | UC-RIDE-027 | AC-RIDE-058-001, AC-RIDE-058-002 |
| FR-RIDE-059 | critical | Backend gRPC on .NET 10 containers | TR-RIDE-SERVER-008 | TEST-RIDE-036 | UC-RIDE-028 | AC-RIDE-059-001, AC-RIDE-059-002 |
| FR-RIDE-060 | high | Proto and schema publication under GPL-2.0 | TR-RIDE-GPL-005 | TEST-RIDE-037 | UC-RIDE-029 | AC-RIDE-060-001, AC-RIDE-060-002 |
| FR-RIDE-061 | critical | Fail-closed admission over gRPC | TR-RIDE-SERVER-009 | TEST-RIDE-036 | UC-RIDE-030 | AC-RIDE-061-001, AC-RIDE-061-002 |
| FR-RIDE-062 | high | Interim OpenAPI companion non-authoritative | TR-RIDE-SERVER-010 | TEST-RIDE-037 | UC-RIDE-031 | AC-RIDE-062-001, AC-RIDE-062-002 |
| FR-RIDE-053 | high | Bluetooth driver-rider phone pairing | TR-RIDE-VIDEO-010 | TEST-RIDE-034 | UC-RIDE-022 | AC-RIDE-053-001, AC-RIDE-053-002 |
| FR-RIDE-054 | high | Driver phone session coordination | TR-RIDE-VIDEO-011 | TEST-RIDE-034 | UC-RIDE-023 | AC-RIDE-054-001, AC-RIDE-054-002 |
| FR-RIDE-055 | high | Passenger phone video sync join and telematics overlay | TR-RIDE-VIDEO-011 | TEST-RIDE-034 | UC-RIDE-024 | AC-RIDE-055-001, AC-RIDE-055-002 |

### 2.4 TR inventory

| TR | Subarea | Title |
| --- | --- | --- |
| TR-RIDE-INGEST-001 | INGEST | Privacy-export ZIP parser |
| TR-RIDE-INGEST-002 | INGEST | Smooth Cruiser structured and manual ingest |
| TR-RIDE-INGEST-003 | INGEST | Trip and Business report ingest |
| TR-RIDE-INGEST-004 | INGEST | Concierge OAuth and status poller |
| TR-RIDE-INGEST-005 | INGEST | Third-party telematics importers |
| TR-RIDE-INGEST-006 | INGEST | Honesty provenance tagging |
| TR-RIDE-STORE-001 | STORE | Sealed immutable blob store |
| TR-RIDE-STORE-002 | STORE | Versioned correction events |
| TR-RIDE-STORE-003 | STORE | Jurisdiction retention engine |
| TR-RIDE-ANAL-001 | ANAL | Authorized working-copy analysis |
| TR-RIDE-ANAL-002 | ANAL | Coverage matrix generator |
| TR-RIDE-ANAL-003 | ANAL | Online-hours policy engine |
| TR-RIDE-ANAL-004 | ANAL | Incident package builder |
| TR-RIDE-SEAL-001 | SEAL | Collection-boundary sealer |
| TR-RIDE-SEAL-002 | SEAL | Scoped key generation |
| TR-RIDE-SEAL-003 | SEAL | Cryptographic agility layer |
| TR-RIDE-CHAIN-001 | CHAIN | Custody receipt builder |
| TR-RIDE-CHAIN-002 | CHAIN | Configurable chain writer |
| TR-RIDE-CHAIN-003 | CHAIN | Non-admission on chain failure |
| TR-RIDE-CHAIN-004 | CHAIN | Receipt verification service |
| TR-RIDE-ESCROW-001 | ESCROW | M-of-N escrow packaging |
| TR-RIDE-ESCROW-002 | ESCROW | Court escrow-release workflow |
| TR-RIDE-ESCROW-003 | ESCROW | HSM/KMS private-key custody |
| TR-RIDE-PLAY-001 | PLAY | Play Integrity before seal |
| TR-RIDE-PLAY-002 | PLAY | Attestation evidence on receipt |
| TR-RIDE-PLAY-003 | PLAY | Package allowlist and rotation |
| TR-RIDE-GPL-001 | GPL | GPL-2.0 project licensing |
| TR-RIDE-GPL-002 | GPL | Artifact license metadata |
| TR-RIDE-GPL-003 | GPL | Play and source distribution |
| TR-RIDE-SERVER-001 | SERVER | Driver account and consent service |
| TR-RIDE-SERVER-002 | SERVER | Vehicle registry and config profiles |
| TR-RIDE-SERVER-003 | SERVER | Sealed-only submission API |
| TR-RIDE-SERVER-004 | SERVER | Admission verifier |
| TR-RIDE-SERVER-005 | SERVER | Abuse controls and backpressure |
| TR-RIDE-SERVER-006 | SERVER | Multi-tenant isolation |
| TR-RIDE-SERVER-007 | SERVER | Counsel multi-driver bundle builder |
| TR-RIDE-VIDEO-001 | VIDEO | Dual-phone capture session |
| TR-RIDE-VIDEO-002 | VIDEO | SyncClockOffset service |
| TR-RIDE-VIDEO-003 | VIDEO | On-device composite pipeline |
| TR-RIDE-VIDEO-004 | VIDEO | Spider-graph overlay manifest |
| TR-RIDE-VIDEO-005 | VIDEO | Composite seal and chain receipt |
| TR-RIDE-VIDEO-006 | VIDEO | Video quota and chunked upload |
| TR-RIDE-VIEW-001 | VIEW | Cross-platform GPL2 viewer |
| TR-RIDE-VIEW-002 | VIEW | Fail-closed independent verification |
| TR-RIDE-VIEW-003 | VIEW | Synchronized RideBundle timeline |
| TR-RIDE-VIEW-004 | VIEW | ViewerSession and VerificationReport |
| TR-RIDE-PRIV-001 | PRIV | Consent and purpose binding |
| TR-RIDE-PRIV-002 | PRIV | Sensitive geolocation masking |
| TR-RIDE-PRIV-003 | PRIV | DSAR access and deletion |
| TR-RIDE-SEC-001 | SEC | TLS and vault secrets |
| TR-RIDE-SEC-002 | SEC | RBAC least privilege |
| TR-RIDE-SEC-003 | SEC | Append-only sensitive access logs |
| TR-RIDE-PERF-001 | PERF | Seal/receipt latency monitoring |
| TR-RIDE-PERF-002 | PERF | Large history UI pagination |
| TR-RIDE-PERF-003 | PERF | Video performance gates |
| TR-RIDE-VIDEO-012 | VIDEO | Avalonia UI 12 Android capture client |
| TR-RIDE-VIEW-005 | VIEW | Avalonia UI 12 desktop court viewer |
| TR-RIDE-GPL-004 | GPL | Shared Avalonia UI under GPL-2.0 |
| TR-RIDE-SERVER-008 | SERVER | gRPC services on .NET 10 containers |
| TR-RIDE-GPL-005 | GPL | Publish gRPC protos under GPL-2.0 |
| TR-RIDE-SERVER-009 | SERVER | Fail-closed gRPC admission |
| TR-RIDE-SERVER-010 | SERVER | OpenAPI companion non-authoritative |
| TR-RIDE-VIDEO-010 | VIDEO | Bluetooth pairing and role protocol |
| TR-RIDE-VIDEO-011 | VIDEO | Driver coordinator and passenger compositor split |

### 2.5 TEST inventory

| TEST | Title |
| --- | --- |
| TEST-RIDE-001 | Privacy-export ZIP parse and Unverified tagging |
| TEST-RIDE-002 | Smooth Cruiser ingest paths |
| TEST-RIDE-003 | Trip record ingest |
| TEST-RIDE-004 | Concierge optional poll and partnership gate |
| TEST-RIDE-005 | Third-party telematics import |
| TEST-RIDE-006 | Consent and provenance ledger |
| TEST-RIDE-007 | Coverage matrix and API-gap notice |
| TEST-RIDE-008 | Online-hours policy evaluation |
| TEST-RIDE-009 | Incident time-window export |
| TEST-RIDE-010 | DSAR access deletion and legal hold |
| TEST-RIDE-011 | Hash version integrity on imports |
| TEST-RIDE-012 | RBAC least privilege location |
| TEST-RIDE-013 | Seal-at-collect boundary |
| TEST-RIDE-014 | Custody receipt and chain write |
| TEST-RIDE-015 | Chain failure non-admission |
| TEST-RIDE-016 | Court decryption path documentation and verify UI |
| TEST-RIDE-017 | Escrow package and separation |
| TEST-RIDE-018 | Court escrow release workflow |
| TEST-RIDE-019 | Play Integrity gate and receipt attestation |
| TEST-RIDE-020 | GPL-2.0 licensing and distribution |
| TEST-RIDE-021 | Driver account vehicle config |
| TEST-RIDE-022 | Sealed-only submit and admission verify |
| TEST-RIDE-023 | Multi-driver provenance and counsel bundle |
| TEST-RIDE-024 | Abuse controls and tenant isolation |
| TEST-RIDE-025 | Dual-phone sync and composite |
| TEST-RIDE-026 | Composite seal optional raw and metadata |
| TEST-RIDE-027 | Counsel composite playback verification |
| TEST-RIDE-028 | Desktop viewer fail-closed timeline |
| TEST-RIDE-029 | Security TLS vault and access logs |
| TEST-RIDE-030 | Concierge resilience and accuracy labeling |
| TEST-RIDE-031 | Scalability pagination and portable export |
| TEST-RIDE-032 | Retention crypto agility latency |
| TEST-RIDE-033 | Video quotas and performance gates |
| TEST-RIDE-035 | Avalonia UI 12 clients and GPL share |
| TEST-RIDE-036 | gRPC .NET 10 sealed fail-closed admission |
| TEST-RIDE-037 | Proto GPL authority over OpenAPI companion |
| TEST-RIDE-034 | Bluetooth pairing and role split |

### 2.6 UC inventory

| UC | Title | YAML? | Markdown? |
| --- | --- | --- | --- |
| UC-RIDE-001 | Ingest privacy-export ZIP with provenance | **NO — P0** | yes |
| UC-RIDE-002 | Record Smooth Cruiser scores | **NO — P0** | yes |
| UC-RIDE-003 | Optional Concierge/Business partnership ingest | **NO — P0** | yes |
| UC-RIDE-004 | Import third-party telematics | **NO — P0** | yes |
| UC-RIDE-005 | Generate coverage matrix | **NO — P0** | yes |
| UC-RIDE-006 | Audit online-hours policy | **NO — P0** | yes |
| UC-RIDE-007 | Build time-window incident report | **NO — P0** | yes |
| UC-RIDE-008 | Data subject access and deletion | **NO — P0** | yes |
| UC-RIDE-009 | Seal-at-collect with custody receipt | **NO — P0** | yes |
| UC-RIDE-010 | Court-review verification path | **NO — P0** | yes |
| UC-RIDE-011 | Escrow release under court order | **NO — P0** | yes |
| UC-RIDE-012 | Play Integrity attestation gate | **NO — P0** | yes |
| UC-RIDE-013 | GPL-2.0 license and distribution | **NO — P0** | yes |
| UC-RIDE-014 | Driver account and vehicle config | **NO — P0** | yes |
| UC-RIDE-015 | Sealed submit and admission | **NO — P0** | yes |
| UC-RIDE-016 | Counsel multi-driver bundle | **NO — P0** | yes |
| UC-RIDE-017 | Dual-phone capture and composite | **NO — P0** | yes |
| UC-RIDE-018 | Counsel composite playback | **NO — P0** | yes |
| UC-RIDE-019 | Desktop court viewer session | **NO — P0** | yes |
| UC-RIDE-020 | RBAC and partnership gates | **NO — P0** | yes |
| UC-RIDE-021 | Scale histories and portable export | **NO — P0** | yes |
| UC-RIDE-022 | Bluetooth driver-rider pairing | **NO — P0** | yes |
| UC-RIDE-023 | Driver phone session coordination | **NO — P0** | yes |
| UC-RIDE-024 | Passenger video sync and telematics overlay | **NO — P0** | yes |
| UC-RIDE-025 | Capture ride evidence with Avalonia Android client | yes | yes |
| UC-RIDE-026 | Review sealed bundle with Avalonia desktop viewer | yes | yes |
| UC-RIDE-027 | Reuse shared Avalonia UI under GPL-2.0 | yes | yes |
| UC-RIDE-028 | Submit sealed package via gRPC .NET 10 | yes | yes |
| UC-RIDE-029 | Consume published GPL-2.0 gRPC protos | yes | yes |
| UC-RIDE-030 | Fail-closed gRPC admission | yes | yes |
| UC-RIDE-031 | Prefer gRPC over interim OpenAPI companion | yes | yes |

---

## 3. Phased implementation slices

Each phase: FR IDs, derived UC/AC/TEST from mappings, files/projects, dependencies, exit criteria, HV gate. BDPv4 inside each phase: **tests first (RED) → green → refactor**; exit only when focused + prior suites are Failed 0 Skipped 0.

### P0 — Requirements completeness gate (docs-only)

**Goal:** Close UC YAML gap UC-RIDE-001..024; freeze stack; no app code

| Field | Value |
| --- | --- |
| FR IDs | FR-RIDE-029, FR-RIDE-030, FR-RIDE-056, FR-RIDE-057, FR-RIDE-058, FR-RIDE-059, FR-RIDE-060, FR-RIDE-061, FR-RIDE-062, FR-RIDE-053, FR-RIDE-054, FR-RIDE-055 |
| UC IDs | UC-RIDE-013, UC-RIDE-025, UC-RIDE-026, UC-RIDE-027, UC-RIDE-028, UC-RIDE-029, UC-RIDE-030, UC-RIDE-031, UC-RIDE-022, UC-RIDE-023, UC-RIDE-024 |
| TR IDs | TR-RIDE-GPL-001, TR-RIDE-GPL-002, TR-RIDE-VIDEO-012, TR-RIDE-VIEW-005, TR-RIDE-GPL-004, TR-RIDE-SERVER-008, TR-RIDE-GPL-005, TR-RIDE-SERVER-009, TR-RIDE-SERVER-010, TR-RIDE-VIDEO-010, TR-RIDE-VIDEO-011 |
| TEST IDs | TEST-RIDE-020, TEST-RIDE-035, TEST-RIDE-036, TEST-RIDE-037, TEST-RIDE-034 |
| AC IDs | AC-RIDE-029-001, AC-RIDE-029-002, AC-RIDE-029-003, AC-RIDE-030-001, AC-RIDE-030-002, AC-RIDE-056-001, AC-RIDE-056-002, AC-RIDE-057-001, AC-RIDE-057-002, AC-RIDE-058-001, AC-RIDE-058-002, AC-RIDE-059-001, AC-RIDE-059-002, AC-RIDE-060-001, AC-RIDE-060-002, AC-RIDE-061-001, AC-RIDE-061-002, AC-RIDE-062-001, AC-RIDE-062-002, AC-RIDE-053-001, AC-RIDE-053-002, AC-RIDE-054-001, AC-RIDE-054-002, AC-RIDE-055-001, AC-RIDE-055-002 |
| Files / projects | docs/Project/Use-Cases-Batch.yaml; docs/Project/Requirements-Mappings-Batch.yaml; docs/architecture/stack.md; docs/plans/PLAN-RIDEAUDIT-001-implementation.md; docs/receipts/hv/ |
| Dependencies | Astra AGREE on this plan; Payton AGREE |
| Exit criteria | UC-RIDE-001..031 present in YAML or explicit deferred waiver; Mappings 84/84; HV plan AGREE committed; No src/ Avalonia or gRPC app code |
| HV gate | Opposing model AGREE; save `docs/receipts/hv/<utc>-p0-*-request/response.jsonl` (or combined pair JSON); commit+push immediately |

**BDPv4 TDD notes for P0:**
- Write the next small failing tests covering the AC IDs listed above (mocks first where IO/chain/Play/BT).
- Implement only enough production code to go green; refactor with suite green.
- Do not exit with skips. Integration tests follow once unit surfaces stabilize.

### P1 — Solution skeleton + proto contracts (tests first)

**Goal:** Create solution layout, .proto contracts under GPL-2.0, empty test projects RED for contract surfaces

| Field | Value |
| --- | --- |
| FR IDs | FR-RIDE-059, FR-RIDE-060, FR-RIDE-062, FR-RIDE-029, FR-RIDE-030, FR-RIDE-035, FR-RIDE-061 |
| UC IDs | UC-RIDE-028, UC-RIDE-029, UC-RIDE-031, UC-RIDE-013, UC-RIDE-015, UC-RIDE-030 |
| TR IDs | TR-RIDE-SERVER-008, TR-RIDE-GPL-005, TR-RIDE-SERVER-010, TR-RIDE-GPL-001, TR-RIDE-GPL-002, TR-RIDE-SERVER-003, TR-RIDE-SERVER-009 |
| TEST IDs | TEST-RIDE-036, TEST-RIDE-037, TEST-RIDE-020, TEST-RIDE-022 |
| AC IDs | AC-RIDE-059-001, AC-RIDE-059-002, AC-RIDE-060-001, AC-RIDE-060-002, AC-RIDE-062-001, AC-RIDE-062-002, AC-RIDE-029-001, AC-RIDE-029-002, AC-RIDE-029-003, AC-RIDE-030-001, AC-RIDE-030-002, AC-RIDE-035-001, AC-RIDE-035-002, AC-RIDE-061-001, AC-RIDE-061-002 |
| Files / projects | RideAudit.sln; src/RideAudit.Protos/; src/RideAudit.Contracts/; src/RideAudit.Server.Admission/; src/RideAudit.Server.Counsel/; src/RideAudit.Client.Android/; src/RideAudit.Client.Desktop/; src/RideAudit.Shared.Ui/; tests/RideAudit.Protos.Tests/; tests/RideAudit.Server.Admission.Tests/; Directory.Build.props; LICENSE; NOTICE |
| Dependencies | P0 AGREE |
| Exit criteria | protos compile; TEST-RIDE-036/037 RED then GREEN for contract-only asserts; OpenAPI marked companion; HV AGREE on skeleton |
| HV gate | Opposing model AGREE; save `docs/receipts/hv/<utc>-p1-*-request/response.jsonl` (or combined pair JSON); commit+push immediately |

**BDPv4 TDD notes for P1:**
- Write the next small failing tests covering the AC IDs listed above (mocks first where IO/chain/Play/BT).
- Implement only enough production code to go green; refactor with suite green.
- Do not exit with skips. Integration tests follow once unit surfaces stabilize.

### P2 — gRPC admission service (fail-closed, sealed-only)

**Goal:** Implement sealed submit + admission verifier on .NET 10 containers; no decrypt at ingest

| Field | Value |
| --- | --- |
| FR IDs | FR-RIDE-032, FR-RIDE-033, FR-RIDE-034, FR-RIDE-035, FR-RIDE-036, FR-RIDE-039, FR-RIDE-040, FR-RIDE-061, FR-RIDE-201, FR-RIDE-218, FR-RIDE-026 |
| UC IDs | UC-RIDE-014, UC-RIDE-015, UC-RIDE-030, UC-RIDE-009, UC-RIDE-012 |
| TR IDs | TR-RIDE-SERVER-001, TR-RIDE-SERVER-002, TR-RIDE-SERVER-003, TR-RIDE-SERVER-004, TR-RIDE-SERVER-005, TR-RIDE-SERVER-006, TR-RIDE-SERVER-009, TR-RIDE-SEC-001, TR-RIDE-PLAY-001, TR-RIDE-PLAY-003 |
| TEST IDs | TEST-RIDE-021, TEST-RIDE-022, TEST-RIDE-024, TEST-RIDE-036, TEST-RIDE-029, TEST-RIDE-019 |
| AC IDs | AC-RIDE-032-001, AC-RIDE-032-002, AC-RIDE-032-003, AC-RIDE-033-001, AC-RIDE-033-002, AC-RIDE-034-001, AC-RIDE-034-002, AC-RIDE-035-001, AC-RIDE-035-002, AC-RIDE-036-001, AC-RIDE-036-002, AC-RIDE-039-001, AC-RIDE-039-002, AC-RIDE-039-003, AC-RIDE-040-001, AC-RIDE-040-002, AC-RIDE-061-001, AC-RIDE-061-002, AC-RIDE-201-001, AC-RIDE-201-002, AC-RIDE-218-001, AC-RIDE-218-002, AC-RIDE-026-001, AC-RIDE-026-002, AC-RIDE-026-003 |
| Files / projects | src/RideAudit.Server.Admission/; src/RideAudit.Server.Identity/; deploy/containers/admission/; tests/RideAudit.Server.Admission.Tests/ |
| Dependencies | P1 |
| Exit criteria | TEST-RIDE-021/022/024/036 Failed 0 Skipped 0; fail-closed on integrity miss; HV AGREE |
| HV gate | Opposing model AGREE; save `docs/receipts/hv/<utc>-p2-*-request/response.jsonl` (or combined pair JSON); commit+push immediately |

**BDPv4 TDD notes for P2:**
- Write the next small failing tests covering the AC IDs listed above (mocks first where IO/chain/Play/BT).
- Implement only enough production code to go green; refactor with suite green.
- Do not exit with skips. Integration tests follow once unit surfaces stabilize.

### P3 — Seal-at-collect + custody receipt + OTS chain

**Goal:** Collection-boundary sealer, custody receipt builder, Bitcoin OTS writer, non-admission on chain failure

| Field | Value |
| --- | --- |
| FR IDs | FR-RIDE-015, FR-RIDE-016, FR-RIDE-017, FR-RIDE-018, FR-RIDE-019, FR-RIDE-211, FR-RIDE-212, FR-RIDE-213, FR-RIDE-027 |
| UC IDs | UC-RIDE-009, UC-RIDE-012 |
| TR IDs | TR-RIDE-STORE-001, TR-RIDE-STORE-002, TR-RIDE-SEAL-001, TR-RIDE-SEAL-002, TR-RIDE-CHAIN-001, TR-RIDE-CHAIN-002, TR-RIDE-CHAIN-003, TR-RIDE-SEAL-003, TR-RIDE-PERF-001, TR-RIDE-PLAY-002 |
| TEST IDs | TEST-RIDE-013, TEST-RIDE-014, TEST-RIDE-015, TEST-RIDE-032, TEST-RIDE-019 |
| AC IDs | AC-RIDE-015-001, AC-RIDE-015-002, AC-RIDE-016-001, AC-RIDE-016-002, AC-RIDE-016-003, AC-RIDE-017-001, AC-RIDE-017-002, AC-RIDE-018-001, AC-RIDE-018-002, AC-RIDE-018-003, AC-RIDE-019-001, AC-RIDE-019-002, AC-RIDE-019-003, AC-RIDE-211-001, AC-RIDE-211-002, AC-RIDE-212-001, AC-RIDE-212-002, AC-RIDE-212-003, AC-RIDE-213-001, AC-RIDE-213-002, AC-RIDE-027-001, AC-RIDE-027-002 |
| Files / projects | src/RideAudit.Seal/; src/RideAudit.Chain/; src/RideAudit.Chain.OpenTimestamps/; tests/RideAudit.Seal.Tests/; tests/RideAudit.Chain.Tests/ |
| Dependencies | P1 |
| Exit criteria | TEST-RIDE-013/014/015/032 Failed 0 Skipped 0; OTS primary profile btc-ots; HV AGREE |
| HV gate | Opposing model AGREE; save `docs/receipts/hv/<utc>-p3-*-request/response.jsonl` (or combined pair JSON); commit+push immediately |

**BDPv4 TDD notes for P3:**
- Write the next small failing tests covering the AC IDs listed above (mocks first where IO/chain/Play/BT).
- Implement only enough production code to go green; refactor with suite green.
- Do not exit with skips. Integration tests follow once unit surfaces stabilize.

### P4 — Play Integrity attestation binding

**Goal:** Play Integrity before seal; attestation on receipt; package allowlist

| Field | Value |
| --- | --- |
| FR IDs | FR-RIDE-025, FR-RIDE-026, FR-RIDE-027, FR-RIDE-215 |
| UC IDs | UC-RIDE-012 |
| TR IDs | TR-RIDE-PLAY-001, TR-RIDE-PLAY-003, TR-RIDE-CHAIN-001, TR-RIDE-PLAY-002 |
| TEST IDs | TEST-RIDE-019 |
| AC IDs | AC-RIDE-025-001, AC-RIDE-025-002, AC-RIDE-026-001, AC-RIDE-026-002, AC-RIDE-026-003, AC-RIDE-027-001, AC-RIDE-027-002, AC-RIDE-215-001, AC-RIDE-215-002, AC-RIDE-215-003 |
| Files / projects | src/RideAudit.Attest/; tests/RideAudit.Attest.Tests/ |
| Dependencies | P3 |
| Exit criteria | TEST-RIDE-019 Failed 0 Skipped 0; HV AGREE |
| HV gate | Opposing model AGREE; save `docs/receipts/hv/<utc>-p4-*-request/response.jsonl` (or combined pair JSON); commit+push immediately |

**BDPv4 TDD notes for P4:**
- Write the next small failing tests covering the AC IDs listed above (mocks first where IO/chain/Play/BT).
- Implement only enough production code to go green; refactor with suite green.
- Do not exit with skips. Integration tests follow once unit surfaces stabilize.

### P5 — Escrow M-of-N + court release

**Goal:** Escrow packaging, HSM/KMS custody, court-authorized release into expiring working copy

| Field | Value |
| --- | --- |
| FR IDs | FR-RIDE-020, FR-RIDE-022, FR-RIDE-023, FR-RIDE-024, FR-RIDE-214, FR-RIDE-216, FR-RIDE-028 |
| UC IDs | UC-RIDE-010, UC-RIDE-011, UC-RIDE-019 |
| TR IDs | TR-RIDE-ANAL-001, TR-RIDE-ESCROW-001, TR-RIDE-ESCROW-003, TR-RIDE-ESCROW-002, TR-RIDE-CHAIN-004 |
| TEST IDs | TEST-RIDE-016, TEST-RIDE-017, TEST-RIDE-018, TEST-RIDE-029 |
| AC IDs | AC-RIDE-020-001, AC-RIDE-020-002, AC-RIDE-020-003, AC-RIDE-022-001, AC-RIDE-022-002, AC-RIDE-023-001, AC-RIDE-023-002, AC-RIDE-023-003, AC-RIDE-024-001, AC-RIDE-024-002, AC-RIDE-024-003, AC-RIDE-214-001, AC-RIDE-214-002, AC-RIDE-216-001, AC-RIDE-216-002, AC-RIDE-028-001, AC-RIDE-028-002 |
| Files / projects | src/RideAudit.Escrow/; tests/RideAudit.Escrow.Tests/ |
| Dependencies | P3 |
| Exit criteria | TEST-RIDE-016/017/018 Failed 0 Skipped 0; HV AGREE |
| HV gate | Opposing model AGREE; save `docs/receipts/hv/<utc>-p5-*-request/response.jsonl` (or combined pair JSON); commit+push immediately |

**BDPv4 TDD notes for P5:**
- Write the next small failing tests covering the AC IDs listed above (mocks first where IO/chain/Play/BT).
- Implement only enough production code to go green; refactor with suite green.
- Do not exit with skips. Integration tests follow once unit surfaces stabilize.

### P6 — Bluetooth dual-phone roles + Avalonia Android client

**Goal:** BT pairing protocol; driver coordinator; passenger compositor; Avalonia UI 12 Android surfaces

| Field | Value |
| --- | --- |
| FR IDs | FR-RIDE-041, FR-RIDE-042, FR-RIDE-043, FR-RIDE-044, FR-RIDE-045, FR-RIDE-046, FR-RIDE-048, FR-RIDE-053, FR-RIDE-054, FR-RIDE-055, FR-RIDE-056, FR-RIDE-058, FR-RIDE-219, FR-RIDE-220 |
| UC IDs | UC-RIDE-017, UC-RIDE-018, UC-RIDE-022, UC-RIDE-023, UC-RIDE-024, UC-RIDE-025, UC-RIDE-027 |
| TR IDs | TR-RIDE-VIDEO-001, TR-RIDE-VIDEO-002, TR-RIDE-VIDEO-003, TR-RIDE-VIDEO-004, TR-RIDE-VIDEO-005, TR-RIDE-VIDEO-010, TR-RIDE-VIDEO-011, TR-RIDE-VIDEO-012, TR-RIDE-GPL-004, TR-RIDE-VIDEO-006, TR-RIDE-PERF-003 |
| TEST IDs | TEST-RIDE-025, TEST-RIDE-026, TEST-RIDE-034, TEST-RIDE-035, TEST-RIDE-033 |
| AC IDs | AC-RIDE-041-001, AC-RIDE-041-002, AC-RIDE-042-001, AC-RIDE-042-002, AC-RIDE-043-001, AC-RIDE-043-002, AC-RIDE-044-001, AC-RIDE-044-002, AC-RIDE-045-001, AC-RIDE-045-002, AC-RIDE-046-001, AC-RIDE-046-002, AC-RIDE-048-001, AC-RIDE-048-002, AC-RIDE-053-001, AC-RIDE-053-002, AC-RIDE-054-001, AC-RIDE-054-002, AC-RIDE-055-001, AC-RIDE-055-002, AC-RIDE-056-001, AC-RIDE-056-002, AC-RIDE-058-001, AC-RIDE-058-002, AC-RIDE-219-001, AC-RIDE-219-002, AC-RIDE-219-003, AC-RIDE-220-001, AC-RIDE-220-002 |
| Files / projects | src/RideAudit.Client.Android/; src/RideAudit.Shared.Ui/; src/RideAudit.Bt/; src/RideAudit.Video/; tests/RideAudit.Bt.Tests/; tests/RideAudit.Video.Tests/; artifacts/android/ (evolve from Kotlin scaffold) |
| Dependencies | P1; P3; P4 |
| Exit criteria | TEST-RIDE-025/026/033/034/035 Failed 0 Skipped 0; no Lyft private APIs; HV AGREE |
| HV gate | Opposing model AGREE; save `docs/receipts/hv/<utc>-p6-*-request/response.jsonl` (or combined pair JSON); commit+push immediately |

**BDPv4 TDD notes for P6:**
- Write the next small failing tests covering the AC IDs listed above (mocks first where IO/chain/Play/BT).
- Implement only enough production code to go green; refactor with suite green.
- Do not exit with skips. Integration tests follow once unit surfaces stabilize.

### P7 — Avalonia desktop court/counsel viewer

**Goal:** Fail-closed verification gate, escrow decrypt working copy, synchronized timeline, ViewerSession/VerificationReport

| Field | Value |
| --- | --- |
| FR IDs | FR-RIDE-047, FR-RIDE-049, FR-RIDE-050, FR-RIDE-051, FR-RIDE-052, FR-RIDE-057, FR-RIDE-221, FR-RIDE-222 |
| UC IDs | UC-RIDE-018, UC-RIDE-019, UC-RIDE-026 |
| TR IDs | TR-RIDE-VIEW-002, TR-RIDE-VIEW-001, TR-RIDE-VIEW-003, TR-RIDE-VIEW-004, TR-RIDE-VIEW-005, TR-RIDE-VIDEO-001, TR-RIDE-VIDEO-002 |
| TEST IDs | TEST-RIDE-027, TEST-RIDE-028, TEST-RIDE-035 |
| AC IDs | AC-RIDE-047-001, AC-RIDE-047-002, AC-RIDE-049-001, AC-RIDE-049-002, AC-RIDE-050-001, AC-RIDE-050-002, AC-RIDE-051-001, AC-RIDE-051-002, AC-RIDE-052-001, AC-RIDE-052-002, AC-RIDE-057-001, AC-RIDE-057-002, AC-RIDE-221-001, AC-RIDE-221-002, AC-RIDE-222-001, AC-RIDE-222-002 |
| Files / projects | src/RideAudit.Client.Desktop/; src/RideAudit.Shared.Ui/; tests/RideAudit.Client.Desktop.Tests/ |
| Dependencies | P3; P4; P5 |
| Exit criteria | TEST-RIDE-027/028/035 Failed 0 Skipped 0; fail-closed before decrypt; HV AGREE |
| HV gate | Opposing model AGREE; save `docs/receipts/hv/<utc>-p7-*-request/response.jsonl` (or combined pair JSON); commit+push immediately |

**BDPv4 TDD notes for P7:**
- Write the next small failing tests covering the AC IDs listed above (mocks first where IO/chain/Play/BT).
- Implement only enough production code to go green; refactor with suite green.
- Do not exit with skips. Integration tests follow once unit surfaces stabilize.

### P8 — Counsel multi-driver bundle + analysis working copy

**Goal:** Multi-driver provenance, counsel bundle, authorized working-copy analytics (coverage, hours, incidents)

| Field | Value |
| --- | --- |
| FR IDs | FR-RIDE-007, FR-RIDE-008, FR-RIDE-009, FR-RIDE-037, FR-RIDE-038, FR-RIDE-020, FR-RIDE-021 |
| UC IDs | UC-RIDE-005, UC-RIDE-006, UC-RIDE-007, UC-RIDE-015, UC-RIDE-016, UC-RIDE-010 |
| TR IDs | TR-RIDE-ANAL-002, TR-RIDE-ANAL-003, TR-RIDE-ANAL-004, TR-RIDE-SERVER-007, TR-RIDE-ANAL-001, TR-RIDE-CHAIN-004 |
| TEST IDs | TEST-RIDE-007, TEST-RIDE-008, TEST-RIDE-009, TEST-RIDE-023, TEST-RIDE-016 |
| AC IDs | AC-RIDE-007-001, AC-RIDE-007-002, AC-RIDE-008-001, AC-RIDE-008-002, AC-RIDE-008-003, AC-RIDE-009-001, AC-RIDE-009-002, AC-RIDE-037-001, AC-RIDE-037-002, AC-RIDE-038-001, AC-RIDE-038-002, AC-RIDE-020-001, AC-RIDE-020-002, AC-RIDE-020-003, AC-RIDE-021-001, AC-RIDE-021-002, AC-RIDE-021-003 |
| Files / projects | src/RideAudit.Server.Counsel/; src/RideAudit.Anal/; tests/RideAudit.Anal.Tests/; tests/RideAudit.Server.Counsel.Tests/ |
| Dependencies | P2; P5; P7 |
| Exit criteria | TEST-RIDE-007/008/009/016/023 Failed 0 Skipped 0; HV AGREE |
| HV gate | Opposing model AGREE; save `docs/receipts/hv/<utc>-p8-*-request/response.jsonl` (or combined pair JSON); commit+push immediately |

**BDPv4 TDD notes for P8:**
- Write the next small failing tests covering the AC IDs listed above (mocks first where IO/chain/Play/BT).
- Implement only enough production code to go green; refactor with suite green.
- Do not exit with skips. Integration tests follow once unit surfaces stabilize.

### P9 — Ingest pipelines (privacy ZIP, Smooth Cruiser, telematics)

**Goal:** Honest provenance ingest; no undocumented Lyft APIs; partnership gates

| Field | Value |
| --- | --- |
| FR IDs | FR-RIDE-001, FR-RIDE-002, FR-RIDE-003, FR-RIDE-004, FR-RIDE-005, FR-RIDE-006, FR-RIDE-011, FR-RIDE-012, FR-RIDE-013, FR-RIDE-204, FR-RIDE-206, FR-RIDE-209 |
| UC IDs | UC-RIDE-001, UC-RIDE-002, UC-RIDE-003, UC-RIDE-004, UC-RIDE-020, UC-RIDE-005 |
| TR IDs | TR-RIDE-INGEST-001, TR-RIDE-INGEST-006, TR-RIDE-INGEST-002, TR-RIDE-INGEST-003, TR-RIDE-INGEST-004, TR-RIDE-INGEST-005, TR-RIDE-PRIV-001, TR-RIDE-STORE-001, TR-RIDE-STORE-002, TR-RIDE-ANAL-002 |
| TEST IDs | TEST-RIDE-001, TEST-RIDE-002, TEST-RIDE-003, TEST-RIDE-004, TEST-RIDE-005, TEST-RIDE-006, TEST-RIDE-011, TEST-RIDE-030, TEST-RIDE-007 |
| AC IDs | AC-RIDE-001-001, AC-RIDE-001-002, AC-RIDE-001-003, AC-RIDE-002-001, AC-RIDE-002-002, AC-RIDE-002-003, AC-RIDE-003-001, AC-RIDE-003-002, AC-RIDE-004-001, AC-RIDE-004-002, AC-RIDE-004-003, AC-RIDE-005-001, AC-RIDE-005-002, AC-RIDE-005-003, AC-RIDE-006-001, AC-RIDE-006-002, AC-RIDE-011-001, AC-RIDE-011-002, AC-RIDE-012-001, AC-RIDE-012-002, AC-RIDE-013-001, AC-RIDE-013-002, AC-RIDE-204-001, AC-RIDE-204-002, AC-RIDE-206-001, AC-RIDE-206-002, AC-RIDE-209-001 |
| Files / projects | src/RideAudit.Ingest/; tests/RideAudit.Ingest.Tests/ |
| Dependencies | P2; P3 |
| Exit criteria | TEST-RIDE-001..006/011/030 Failed 0 Skipped 0; HV AGREE |
| HV gate | Opposing model AGREE; save `docs/receipts/hv/<utc>-p9-*-request/response.jsonl` (or combined pair JSON); commit+push immediately |

**BDPv4 TDD notes for P9:**
- Write the next small failing tests covering the AC IDs listed above (mocks first where IO/chain/Play/BT).
- Implement only enough production code to go green; refactor with suite green.
- Do not exit with skips. Integration tests follow once unit surfaces stabilize.

### P10 — Privacy, RBAC, retention, portability NFRs

**Goal:** DSAR, legal hold, RBAC, retention, portable ZIP, TLS/vault, access logs

| Field | Value |
| --- | --- |
| FR IDs | FR-RIDE-010, FR-RIDE-014, FR-RIDE-202, FR-RIDE-203, FR-RIDE-205, FR-RIDE-207, FR-RIDE-208, FR-RIDE-210 |
| UC IDs | UC-RIDE-008, UC-RIDE-020, UC-RIDE-021, UC-RIDE-007 |
| TR IDs | TR-RIDE-STORE-003, TR-RIDE-PRIV-001, TR-RIDE-PRIV-003, TR-RIDE-PRIV-002, TR-RIDE-SEC-002, TR-RIDE-SEC-003, TR-RIDE-PERF-002, TR-RIDE-STORE-001, TR-RIDE-STORE-002 |
| TEST IDs | TEST-RIDE-010, TEST-RIDE-012, TEST-RIDE-029, TEST-RIDE-031, TEST-RIDE-032 |
| AC IDs | AC-RIDE-010-001, AC-RIDE-010-002, AC-RIDE-014-001, AC-RIDE-014-002, AC-RIDE-202-001, AC-RIDE-202-002, AC-RIDE-203-001, AC-RIDE-205-001, AC-RIDE-205-002, AC-RIDE-207-001, AC-RIDE-208-001, AC-RIDE-208-002, AC-RIDE-210-001 |
| Files / projects | src/RideAudit.Privacy/; src/RideAudit.Sec/; tests/RideAudit.Privacy.Tests/; tests/RideAudit.Sec.Tests/ |
| Dependencies | P2; P8; P9 |
| Exit criteria | TEST-RIDE-010/012/029/031/032 Failed 0 Skipped 0; HV AGREE |
| HV gate | Opposing model AGREE; save `docs/receipts/hv/<utc>-p10-*-request/response.jsonl` (or combined pair JSON); commit+push immediately |

**BDPv4 TDD notes for P10:**
- Write the next small failing tests covering the AC IDs listed above (mocks first where IO/chain/Play/BT).
- Implement only enough production code to go green; refactor with suite green.
- Do not exit with skips. Integration tests follow once unit surfaces stabilize.

### P11 — Optional ETH L2 dual-anchor + distribution

**Goal:** Optional Base/Polygon dual-anchor; Play+source distribution; GPL governance

| Field | Value |
| --- | --- |
| FR IDs | FR-RIDE-212, FR-RIDE-031, FR-RIDE-217 |
| UC IDs | UC-RIDE-009, UC-RIDE-013, UC-RIDE-014 |
| TR IDs | TR-RIDE-CHAIN-002, TR-RIDE-GPL-003, TR-RIDE-GPL-001 |
| TEST IDs | TEST-RIDE-014, TEST-RIDE-020 |
| AC IDs | AC-RIDE-212-001, AC-RIDE-212-002, AC-RIDE-212-003, AC-RIDE-031-001, AC-RIDE-031-002, AC-RIDE-217-001, AC-RIDE-217-002 |
| Files / projects | src/RideAudit.Chain.EthL2/; docs/architecture/blockchain-custody-receipts.md; packaging/ |
| Dependencies | P3; P6 |
| Exit criteria | TEST-RIDE-014/020 Failed 0 Skipped 0 for dual-anchor profile; HV AGREE |
| HV gate | Opposing model AGREE; save `docs/receipts/hv/<utc>-p11-*-request/response.jsonl` (or combined pair JSON); commit+push immediately |

**BDPv4 TDD notes for P11:**
- Write the next small failing tests covering the AC IDs listed above (mocks first where IO/chain/Play/BT).
- Implement only enough production code to go green; refactor with suite green.
- Do not exit with skips. Integration tests follow once unit surfaces stabilize.

### Phase dependency graph

```
P0 (plan+UC YAML)
 └─► P1 (skeleton+protos)
      ├─► P2 (admission gRPC)
      ├─► P3 (seal+OTS) ─► P4 (Play) ─► P6 (BT+Android)
      │                    └─► P5 (escrow) ─► P7 (desktop viewer)
      │                              └─► P8 (counsel+anal) ◄─ P2
      └─► P9 (ingest) ◄─ P2,P3
P8+P9 ─► P10 (privacy/RBAC/NFR)
P3+P6 ─► P11 (optional L2 + distribution)
```

---

## 4. Architecture alignment

### 4.1 Clients (Avalonia UI 12)

| Surface | Project (planned) | FR | Notes |
| --- | --- | --- | --- |
| Android dual-phone | `RideAudit.Client.Android` | FR-RIDE-056, 053–055, 041–048 | Driver coordinator + passenger compositor; shared UI lib |
| Desktop court/counsel | `RideAudit.Client.Desktop` | FR-RIDE-057, 049–052 | Fail-closed verify before decrypt |
| Shared UI | `RideAudit.Shared.Ui` | FR-RIDE-058 | GPL-2.0 shared Avalonia controls |

### 4.2 Backend (gRPC .NET 10 containers)

| Service | Responsibility | FR |
| --- | --- | --- |
| Admission | Sealed-only submit; verify chain; no decrypt | FR-RIDE-035, 036, 059, 061 |
| Identity / vehicle | Driver account, vehicle registry, config profiles | FR-RIDE-032–034 |
| Counsel | Multi-driver bundle, disclosure export | FR-RIDE-037–038 |
| Chain writer | OTS primary; optional L2 | FR-RIDE-018, 212 |

Containers under `deploy/containers/*`. Proto authority: `RideAudit.Protos` GPL-2.0 (FR-RIDE-060). OpenAPI companion non-authoritative (FR-RIDE-062).

### 4.3 Bluetooth pairing

Per `dual-phone-bluetooth-roles.md` and FR-RIDE-053–055 / TR-RIDE-VIDEO-010–011 / TEST-RIDE-034 / UC-RIDE-022–024:

1. BT discovery between approved phones.
2. Explicit driver/rider roles.
3. Driver: session clock, start/stop, seal admission orchestration, submit.
4. Passenger: video sync, stream join/composite, realtime telematics spider-graph.
5. RideAudit device pairing only — **not** a Lyft BT API.

### 4.4 Video / telematics

On-device/edge composite is first-class sealed evidence (FR-RIDE-045). Optional raw stream sealing (FR-RIDE-046). Quotas/perf (FR-RIDE-219/220, TEST-RIDE-033).

### 4.5 Custody anchoring

- Primary: Bitcoin OpenTimestamps (`btc-ots`).
- Optional dual-anchor: Base / Polygon L2 for fast confirm; OTS remains long-term unless jurisdiction config says otherwise.
- Fail-closed: chain write failure → non-admission (FR-RIDE-019).
- Viewer independently verifies OTS/hashes/Play/escrow **before** decrypt (FR-RIDE-050).

---

## 5. Artifact packages → code evolution

| Artifact | Today | Evolves into |
| --- | --- | --- |
| ART-RIDE-ANDROID-001 `artifacts/android/` | Historical Kotlin/Gradle scaffold; status skeleton | Replace with Avalonia UI 12 Android project in `src/RideAudit.Client.Android`; keep ARTIFACT.yaml; delete or archive Kotlin placeholders in a dedicated P6 commit |
| ART-RIDE-API-001 `artifacts/server-api/` | OpenAPI companion + security/error docs | Authoritative `.proto` in `src/RideAudit.Protos`; keep openapi.yaml as generated/companion only; containerize Admission/Counsel |
| ART-RIDE-MOUNT-001 | Parametric OpenSCAD dual cradle | Remains hardware; supports P6 capture; no app code dependency beyond dimensions docs |
| ART-RIDE-UX-001 / UX-REVIEW-001 | Wireframes + storyboards SB-01..06 / SB-R-01..06 | Implementation acceptance references for P6/P7 UI; do not invent screens outside storyboards without FR change |

---

## 6. Hostile validation schedule

### 6.1 Plan HV (this document)

| Round | Validator | Model | Effort | Artifact |
| --- | --- | --- | --- | --- |
| R1+ | Codex | gpt-6-astra | xhigh | `docs/receipts/hv/<utc>-plan-review-astra*.request/response.jsonl` |

Invocation pattern (proven on McpServer session-lifecycle):

```text
C:\Users\kingd\.codex\packages\standalone\current\bin\codex.exe exec \
  --model gpt-6-astra --json --skip-git-repo-check -s read-only \
  -c approval_policy="never" \
  -c sandbox_permissions=["disk-full-read-access"] \
  -c model_reasoning_effort=xhigh \
  -C F:\GitHub\rideaudit \
  -o docs/receipts/hv/<utc>-plan-review-astra.verdict.md -
```

Prompt stdin must instruct: re-read plan from disk; score accuracy/completeness/confidence; threshold 98 for AGREE; emit `=== VERDICT JSON ===` with READY|NOT-READY and AGREE|DISAGREE; path_to_98 copy-paste edits if below; **do not modify files**; prefer native filesystem/git (avoid PowerShell.Mcp approval traps from r1).

On DISAGREE: revise plan, re-run Astra, commit each HV pair immediately.

### 6.2 Product HV (after code gen begins)

| Generator | Required HV |
| --- | --- |
| grok-4.6-xhigh | Codex gpt-6-sol xhigh |
| gpt-6-sol xhigh | Grok grok-4.6-xhigh |

Auth limitation: ChatGPT-authenticated Codex cannot use gpt-6-sol until API-key auth exists — record unavailable/failed truthfully; never fake a pass. Pair files also under `docs/reviews/hv-pairs/` per process rule when product HV runs.

### 6.3 Per-phase HV

Every P1–P11 exit requires opposing-model AGREE on that phase diff + test evidence, with JSON pairs committed before marking phase complete.

---

## 7. Risks, open questions, rollback

### 7.1 Risks

| ID | Risk | Mitigation |
| --- | --- | --- |
| R1 | UC-RIDE-001..024 missing from YAML blocks MCP ingest / BDPv4 completeness | P0 fills YAML from markdown + mappings |
| R2 | Kotlin scaffold mistaken for target | ARTIFACT.yaml already marks superseded; P6 replaces |
| R3 | OpenAPI treated as wire truth | FR-RIDE-062 / TR-RIDE-SERVER-010 / TEST-RIDE-037 |
| R4 | Decrypt at public ingest | FR-RIDE-035/061 fail-closed; tests assert ciphertext-only |
| R5 | BT/Lyft API confusion | Explicit non-goal; FR-RIDE-011 |
| R6 | OTS latency / fee congestion | Offline pending receipt; FR-RIDE-019 non-admission until confirm |
| R7 | gpt-6-sol HV unavailable | Record unavailable; do not fake; prefer astra for plan, sol when API key ready for product |
| R8 | MCP_UNTRUSTED | Continue file-based BDPv4; ingest when healthy |
| R9 | Dual-phone clock skew | TR-RIDE-VIDEO-002 SyncClockOffset; TEST-RIDE-025 |
| R10 | Escrow key loss | FR-RIDE-216 resilience; HSM/KMS TR-RIDE-ESCROW-003 |

### 7.2 Open questions (need Payton)

1. Production OTS fee sponsorship vs per-driver wallet.
2. Jurisdiction dual-anchor mandates (Base vs Polygon vs OTS-only).
3. Whether UC-RIDE-001..024 YAML backfill is P0-blocking or waived with markdown as interim SoT.
4. Target Android API / device matrix for Avalonia 12.
5. HSM/KMS vendor for escrow (dev software mock vs cloud KMS).
6. Concierge/Business API partnership timeline (FR-RIDE-004 gated).

### 7.3 Rollback

- Plan-only commits: `git revert` plan/HV commits; no runtime impact.
- After code exists: phase tags `rideaudit-pN-exit`; revert phase merge; containers previous image; chain writes are append-only — never delete public receipts; mark superseded in application metadata only.
- Failed HV: do not merge phase; keep DISAGREE pairs as custody of the finding.

---

## 8. Explicit no-app-code gate

**Still no Avalonia / gRPC application code** until:

1. Astra plan review `overallVerdict: "AGREE"` with accuracy≥98, completeness≥98, confidence≥98 (or Payton explicitly accepts documented residual findings), and
2. Payton AGREE on this plan, and
3. P0 UC YAML gap closed or waived in writing.

Allowed before that gate: this plan, HV receipts, requirement/docs fixes, wiki exports, process docs.

---

## 9. Acceptance of this plan

- [ ] Astra AGREE committed under `docs/receipts/hv/`
- [ ] Payton AGREE (human)
- [ ] P0 UC YAML backfill merged (or waiver recorded)
- [ ] Implementer may begin P1 tests-first skeleton only after the above

## 10. Traceability appendix — FR → phase

| FR | Phase |
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
| FR-RIDE-020 | P5 |
| FR-RIDE-021 | P8 |
| FR-RIDE-022 | P5 |
| FR-RIDE-023 | P5 |
| FR-RIDE-024 | P5 |
| FR-RIDE-025 | P4 |
| FR-RIDE-026 | P2 |
| FR-RIDE-027 | P3 |
| FR-RIDE-028 | P5 |
| FR-RIDE-029 | P0 |
| FR-RIDE-030 | P0 |
| FR-RIDE-031 | P11 |
| FR-RIDE-032 | P2 |
| FR-RIDE-033 | P2 |
| FR-RIDE-034 | P2 |
| FR-RIDE-035 | P1 |
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
| FR-RIDE-212 | P3 |
| FR-RIDE-213 | P3 |
| FR-RIDE-214 | P5 |
| FR-RIDE-215 | P4 |
| FR-RIDE-216 | P5 |
| FR-RIDE-217 | P11 |
| FR-RIDE-218 | P2 |
| FR-RIDE-219 | P6 |
| FR-RIDE-220 | P6 |
| FR-RIDE-221 | P7 |
| FR-RIDE-222 | P7 |
| FR-RIDE-056 | P0 |
| FR-RIDE-057 | P0 |
| FR-RIDE-058 | P0 |
| FR-RIDE-059 | P0 |
| FR-RIDE-060 | P0 |
| FR-RIDE-061 | P0 |
| FR-RIDE-062 | P0 |
| FR-RIDE-053 | P0 |
| FR-RIDE-054 | P0 |
| FR-RIDE-055 | P0 |

Unassigned FR count: 0 — (none)

---

**End of PLAN-RIDEAUDIT-001**
