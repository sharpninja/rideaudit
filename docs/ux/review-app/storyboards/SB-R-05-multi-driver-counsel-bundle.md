# SB-R-05 Multi-driver counsel bundle

**Artifact:** ART-RIDE-UX-REVIEW-001  
**FR links:** FR-RIDE-038, FR-RIDE-050, FR-RIDE-052  
**Screens:** WF-R-02, WF-R-03, WF-R-04  
**UI:** Avalonia UI 12 desktop

## Goal

Open a counsel `RideBundle` containing submissions from many drivers and verify **each sealed record** independently. Do not merge custody into a single shared receipt.

## Actors

- Counsel / Auditor
- Avalonia review app
- gRPC bundle service (.NET 10 containers)

## Beats

1. **Bundle roster**  
   List drivers/vehicles/submissions inside the RideBundle. Each row links to its own sealed records and receipt ids (WF-R-02).

2. **Per-record verification**  
   Run the same fail-closed gate (OTS, hashes, Play Integrity binding, escrow auth) per record. Each produces its own `VerificationReport` row set (WF-R-03).

3. **Isolated fail-closed**  
   One record failing does not invent a merged "bundle OK". Failed records block their own decrypt/display (WF-R-04); others may proceed only if they independently pass and have their own CourtRelease scope.

4. **No merged custody**  
   UI and exports never collapse multiple content hashes into one custody claim.

## Success criteria

- Multi-driver bundle supported (FR-RIDE-038).
- Per-record VerificationReport retained (FR-RIDE-052).
- Custody and fail-closed behavior remain per sealed record.

## Notes

CourtRelease scope must be explicit per record or clearly enumerated set; vague "whole bundle decrypt" without per-record auth is out of policy.
