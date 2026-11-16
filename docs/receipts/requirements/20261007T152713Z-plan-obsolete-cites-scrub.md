# PRIORITY 3: Scrub stale kill-list cites from PLAN-RIDEAUDIT-001*

Written: 2026-10-07 10:25 CT (America/Chicago) / 20261007T152713Z UTC.
Machine: PAYTON-LEGION2
Author: Grok Bot executor

## Scope

Docs-only scrub of plan cites that still named killed invent (mask/RBAC/legal-hold / FR-014/202/203/208/210 / TR-PRIV-002/SEC-002/SEC-003 / TEST-012/032).
No new plan scope invented. No About UI / PrivacyDesk / Camera2 / H.264 / telematics code.

## Files touched

| File | Change |
| --- | --- |
| `docs/plans/PLAN-RIDEAUDIT-001-implementation.md` | OBSOLETE banners (1); FR/TR/TEST/UC/AC rows for kill-list marked obsolete or struck; P10 goal retargeted; pointers to **FR-077/078** where AGREEd replacements exist |
| `docs/plans/PLAN-RIDEAUDIT-001-SERVER.md` | Same pattern for S7 / FR table / linked ID dumps |

## Files checked, no kill-list invent rows to scrub

| File | Note |
| --- | --- |
| `PLAN-RIDEAUDIT-001-ANDROID.md` | Only legitimate court/counsel *product* cites (FR-047 etc.) — left alone |
| `PLAN-RIDEAUDIT-001-BRACKET.md` | No kill-list FR rows |

## Sample before → after

### FR-RIDE-014 (implementation FR table)
- **Before:** `| FR-RIDE-014 | critical | Role-based access | TR-RIDE-PRIV-002, TR-RIDE-SEC-002 | TEST-RIDE-012 | UC-RIDE-020 | AC-RIDE-014-001, AC-RIDE-014-002 | P10 |`
- **After:** `| FR-RIDE-014 | ~~critical~~ | **OBSOLETE (killed invent)** Role-based access / RBAC — see **FR-RIDE-077** | ~~TR-PRIV-002/SEC-002~~ | ~~TEST-012~~ | UC-RIDE-020 | OBSOLETE | P10 |`

### FR-RIDE-210 (legal hold)
- **Before:** `| FR-RIDE-210 | critical | Legal hold suspends deletion | ... |`
- **After:** `| FR-RIDE-210 | ~~critical~~ | **OBSOLETE (killed invent)** Legal hold suspends deletion — no legal-hold SoT | ... | OBSOLETE | P10 |`

### FR-RIDE-202 (mask)
- **Before:** Geolocation sensitive masking → TEST-012
- **After:** **OBSOLETE** — precise location stays unmasked; see **FR-RIDE-077** / **TR-RIDE-VIDEO-007**

### FR-RIDE-208 (third-party retention)
- **Before:** Per-state retention config → TEST-032
- **After:** **OBSOLETE** — see **FR-RIDE-078** / **TR-RIDE-PRIV-004**

### TEST-RIDE-010
- **Before:** DSAR access deletion and legal hold | FR-010, FR-210
- **After:** DSAR access export (legal-hold invent removed) | FR-010 (~~FR-210 killed~~)

### P10 / S7
- **Before:** Privacy, RBAC, retention; Goal: DSAR, legal hold, RBAC...
- **After:** Privacy, retention (~~RBAC/mask/legal-hold invent killed~~); Goal cites **FR-078** / **FR-077**; S7 FR IDs drop kill-list and add FR-077/078

### UC-RIDE-020
- **Before:** Admin RBAC and partnership gates | ... FR-014, FR-202
- **After:** Admin partnership gates (~~RBAC invent killed~~) | FR-011, FR-012 (~~014/202 killed~~)

## Accuracy notes

- FR-203 / TR-SEC-003 / TEST-012 marked obsolete with **no invented replacement** (no AGREE for access-log substitute).
- TEST-032 marked obsolete as kill-list; FR-211/213 remapping left TBD (not invented).
- Legitimate counsel *product* language (FR-028/038/047, court/counsel viewer) unchanged.

## Verify

- implementation.md / SERVER.md: active (non-OBSOLETE/non-struck) kill-list invent requirement rows = 0 after scrub.
- Single scrub banner per file.

## Not done

- PRIORITY 2 / 1 (PrivacyDesk GeoMask rename; feature work) — not this pass.
- No commit.
