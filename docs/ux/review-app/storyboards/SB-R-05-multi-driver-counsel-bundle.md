# SB-R-05 Multi-driver counsel bundle

**Artifact:** ART-RIDE-UX-REVIEW-001  
**FR links:** FR-RIDE-038, FR-RIDE-050, FR-RIDE-052  
**Screens:** [WF-R-02](../../assets/wireframes/WF-R-02-bundle-contents.svg), [WF-R-03](../../assets/wireframes/WF-R-03-verification-report.svg), [WF-R-04](../../assets/wireframes/WF-R-04-fail-closed-blocking.svg)
**UI:** Avalonia UI 12 desktop

<!-- wireframe-svg:start -->

## Visual wireframes

SVG mocks for the screens in this storyboard. Icons are inline SVG paths.

![WF-R-02 Bundle contents (sealed)](../../assets/wireframes/WF-R-02-bundle-contents.svg)

[Open WF-R-02-bundle-contents.svg](../../assets/wireframes/WF-R-02-bundle-contents.svg)

![WF-R-03 Verification report](../../assets/wireframes/WF-R-03-verification-report.svg)

[Open WF-R-03-verification-report.svg](../../assets/wireframes/WF-R-03-verification-report.svg)

![WF-R-04 Fail-closed blocking](../../assets/wireframes/WF-R-04-fail-closed-blocking.svg)

[Open WF-R-04-fail-closed-blocking.svg](../../assets/wireframes/WF-R-04-fail-closed-blocking.svg)

<!-- wireframe-svg:end -->


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
