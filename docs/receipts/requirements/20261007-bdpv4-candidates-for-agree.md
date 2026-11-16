# BDPv4 AGREE status update (2026-10-07 10:14 CT)

Payton AGREED Candidates A+B. priority high. **WRITTEN TO MCP** (FR-RIDE-077, TR-RIDE-VIDEO-007, FR-RIDE-078, TR-RIDE-PRIV-004). status pending; notes include approval: Payton AGREE 2026-10-07.
Receipt: `docs/receipts/requirements/20261007T151421Z-bdpv4-agree-mcp-write.md`

---
# BDPv4 AGREE status update (2026-10-07 10:12 CT)

Payton AGREED Candidates A+B. MCP write **STOPPED** on createFr schema gap: `priority` required and not in this draft.
Receipt: `docs/receipts/requirements/20261007T151201Z-bdpv4-agree-schema-gap-stop.md`
Ids FR-RIDE-077 / FR-RIDE-078 / TR-RIDE-VIDEO-007 / TR-RIDE-PRIV-004 remain not_found. No paraphrasing / no invented AC.

---
# BDPv4 candidates for Payton AGREE (not in MCP)

Written: 2026-10-07 ~10:05 CT (America/Chicago).
Machine: PAYTON-LEGION2
Workspace: F:\GitHub\RideAudit
Author: Grok Bot executor (PRIORITY 3 worker).

## Status

**awaiting Payton AGREE - not in MCP**

No `workflow.requirements.createFr` / `createTr` / `createTest` / `update*` / `ingestDocument` was called for these candidates.
Do not implement Camera2, H.264 encode, or telematics from this draft.
Do not invent legal-hold, RBAC, or masking replacements beyond the two candidate sentences below.

## Context (read-only)

- Purpose: certifiable legal data; driver collects in own vehicle; precise location unmasked.
- Kill-list already absent from MCP SoT and removed from disk YAML projections (receipt `20261007T145248Z-invalid-rows-disk-sync.md`).
- Surviving related rows do **not** cover these gaps:
  - FR-RIDE-044 / FR-RIDE-051: spider-graph **UI overlay** and timeline display of telematics/accel/GPS (not H.264 SEI embed).
  - No FR/TR currently states unmasked precise location as a capture rule, or per-picture SEI embed of accel + precise location in the H.264 stream.
  - No FR/TR currently states that driver-collected evidentiary data is outside California third-party 30-day / location 180-day deletion frames.
- Repo already has an open uncommitted requirements/docs set; this pass **only adds this draft file**.

## Candidate A - Unmasked capture + H.264 per-picture SEI

### Exact candidate sentence (BDPv4)

Precise location and other capture data stay unmasked; accelerometer and precise location are embedded in the H.264 stream as real-time per-picture SEI for certifiable legal data.

### Proposed ids (PROPOSED - not created)

| Kind | Proposed id | Role |
| --- | --- | --- |
| FR | **FR-RIDE-077** (PROPOSED) | Functional rule: unmasked capture; accel + precise location in H.264 SEI per picture |
| TR | **TR-RIDE-VIDEO-007** (PROPOSED) | Technical: SEI NAL units carry real-time per-picture accelerometer and precise location matched to each picture |

Do not reuse killed ids FR-RIDE-014, FR-RIDE-202, TR-RIDE-PRIV-002, or TR-RIDE-SEC-002.

### Killed rows this candidate replaces (gap after removal)

| Killed id | Why it is in scope for replacement |
| --- | --- |
| FR-RIDE-014 | Role-based access / least privilege for precise location views (RBAC + location privilege) |
| FR-RIDE-202 | Geolocation sensitive masking (default UI masks exact coordinates) |
| TR-RIDE-PRIV-002 | Sensitive geolocation masking implementation |
| TR-RIDE-SEC-002 | RBAC least privilege for precise location views |

Note: FR-RIDE-203 / TR-RIDE-SEC-003 (append-only sensitive access logs) were killed with the mask/RBAC cluster. This candidate does **not** reintroduce access-log or role text. If Payton wants a separate audit-log sentence later, that is a different AGREE.

### Gap vs surviving rows

FR-RIDE-044 (spider-graph overlay) and FR-RIDE-051 (synchronized timeline) remain display/manifest concerns. Candidate A is about **bitstream embed** for certifiable legal data, not the UI overlay.

---

## Candidate B - Retention: driver not a third party

### Exact candidate sentence (BDPv4)

Retention does not treat the driver as a third party; do not apply California 30-day or 180-day location third-party deletion frames to driver-collected evidentiary data in their own vehicle.

### Proposed ids (PROPOSED - not created)

| Kind | Proposed id | Role |
| --- | --- | --- |
| FR | **FR-RIDE-078** (PROPOSED) | Functional rule: driver-collected evidentiary retention is not third-party retention |
| TR | **TR-RIDE-PRIV-004** (PROPOSED) | Technical: retention timers must not apply CA third-party 30-day / location 180-day deletion frames to driver-owned vehicle evidentiary capture |

Do not reuse killed id FR-RIDE-208 or invent legal-hold / counsel / admin RBAC language.

### Killed rows this candidate replaces (gap after removal)

| Killed id | Why it is in scope for replacement |
| --- | --- |
| FR-RIDE-208 | Per-state retention config with California sensitive-PI as default strict profile |

### Explicitly out of scope for this candidate

- Do not reintroduce legal-hold suspension of deletion, counsel designation, Admin RBAC, or third-party "subject" framing for the driver.
- UC-RIDE-008 legal-hold language and UC-RIDE-020 Admin RBAC title/diagram remain separate hygiene (PRIORITY elsewhere); this draft does not invent UC replacements.
- Stale surviving wording that still mentions legal holds (for example FR-RIDE-219 capacity/lifecycle text, TR-RIDE-PRIV-003 DSAR text) is **not** edited here. Fix those only after Payton AGREE on Candidate B (and any separate legal-hold kill instructions).

---

## Payton decision block

Reply AGREE, AGREE-with-edits, or reject per candidate:

1. **Candidate A** (unmasked + H.264 SEI): ________
2. **Candidate B** (driver not third party / no CA 30/180 third-party frames): ________
3. Proposed ids FR-RIDE-077 / TR-RIDE-VIDEO-007 / FR-RIDE-078 / TR-RIDE-PRIV-004: keep / renumber: ________

Only after AGREE may an agent create MCP rows from the agreed sentences. Until then: **awaiting Payton AGREE - not in MCP**.

## MCP mutation confirmation

- This worker did **not** create, update, ingest, or delete any FR/TR/TEST/mapping for Candidates A or B.
- Kill-list absence was already observed earlier today (`not_found` for FR-RIDE-014/202/203/208, TR-RIDE-PRIV-002/SEC-002/SEC-003, TEST-RIDE-012/032).
- No git commit in this pass.

