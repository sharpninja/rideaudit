# PRIORITY 2: PrivacyDesk GeoMask/MaskedLocation cleanup (FR-077/078)

Written: 2026-10-07 10:30 CT (America/Chicago) / 20261007T152955Z UTC.
Machine: PAYTON-LEGION2
Repo: F:\GitHub\RideAudit
Author: Grok Bot executor

## Census

| Symbol / path | Runtime hides precise location? | Action |
| --- | --- | --- |
| `GeoMask.Present` in `src/RideAudit.Privacy/PrivacyDesk.cs` | **No** — returned full `G17` lat/lon and `Precise: true` | Renamed → `GeoPresent` |
| `MaskedLocation` record same file | **No** — name-only leftover | Renamed → `PresentedLocation` |
| `PrivacyDesk.ViewLocations` | Uses `GeoPresent`; driver-self authorize only | Signature return type updated |
| gRPC `PlatformGrpcServices.ViewLocations` | Sets `Precise = true`; passes through strings | Unchanged (already unmasked) |
| `privacy.proto` LocationSample | Has `precise` field | Unchanged |
| `RetentionPolicy.For` | CA **365d** / else **730d**; ignores dataClass for duration | **No change** — already not CA 30/180 |
| `Expired(..., "precise-geo")` | Returns `false` (exempt) | Unchanged |
| `SweepRetention` | Only sweeps **trips**, not location rows | Unchanged |
| `PrivacySlice.Checklist` | Cited killed FR-014/202/203/208 | Updated to FR-077/078 + surviving 010/205/207 |
| `AppendOnlyAccessLog` XML comment | Cited FR-203 | Comment notes killed invent; helper retained for self-access |
| Tests `Precise_location_stays_visible_*`, `Retention_does_not_remove_location_rows` | Assert unmasked G17 + Precise | Traits retargeted FR-077 / FR-078 |

No other `GeoMask` / `MaskedLocation` hits under `src/` or `tests/` after rename.

## Retention evidence (FR-078 — no invent)

`csharp
// RetentionPolicy.For — still:
return california ? TimeSpan.FromDays(365) : TimeSpan.FromDays(730);
// Expired precise-geo:
if (string.Equals(dataClass, "precise-geo", ...)) return false;
// SweepRetention only RemoveAll on Trips — locations not swept
`

No CA 30-day / 180-day third-party deletion frames present. **Retention code not changed.**

## Edits

- `src/RideAudit.Privacy/PrivacyDesk.cs` — rename + FR-077/078 docs
- `src/RideAudit.Privacy/PrivacySlice.cs` — checklist
- `src/RideAudit.Sec/AppendOnlyAccessLog.cs` — comment only
- `tests/RideAudit.Workflow.Tests/IngestAnalysisTests.cs` — FR traits 077/078

## Build / test (LEGION2, no phones)

- `dotnet build` Privacy + Workflow.Tests: **succeeded** 0 warnings 0 errors
- Filter `Precise_location_stays_visible|Retention_does_not_remove_location_rows|Access_log_is_append_only`: **Passed 3 / Failed 0**

## Remaining risk

- H.264 SEI embed (TR-VIDEO-007) **not** implemented — out of PRIORITY 2 scope (no Camera2/SEI encoder).
- About UI is PRIORITY 1 (not this pass).
- Proto field name `precise` remains (accurate boolean, not a mask).
- Access-log helper still exists without FR-203 SoT; self-access only (actorId == driverId) — not RBAC invent.

## Not done

- No About UI
- No Camera2 / H.264 SEI
- No MCP requirement mutations this pass (FR-077/078 already AGREEd)
- No commit
