# SB-R-04 Timeline playback

**Artifact:** ART-RIDE-UX-REVIEW-001  
**FR links:** FR-RIDE-047, FR-RIDE-051  
**NFR:** NFR-21  
**Screens:** WF-R-06  
**UI:** Avalonia UI 12 desktop

## Goal

After authorized decrypt, counsel plays the sealed composite on a synchronized timeline with spider-graph telematics overlay (and GPS/OBD when present), using recorded `SyncClockOffset`.

## Actors

- Counsel / Auditor
- Avalonia playback surface
- Expiring working copy

## Beats

1. **Enter playback (WF-R-06)**  
   Working copy valid and non-expired. Composite loads with overlay manifest versions preserved.

2. **Synchronized scrub**  
   Timeline scrub keeps video, spider graph, and optional GPS/OBD tracks aligned via `SyncClockOffset`. Missing components are listed, not invented.

3. **Coverage / gaps**  
   Side panel shows coverage matrix and **Unverified** caveats for absent Lyft-native signals. No Lyft private API claims.

4. **Integrity recall**  
   Counsel can jump to VerificationReport / provenance without leaving the review context.

## Success criteria

- Playback requires prior verify + escrow success (FR-RIDE-047).
- Dual-phone composite and spider overlay are first-class.
- Gaps are labeled Unverified when Lyft-native data was not collected.

## Notes

Viewer operates independently of the original collection devices (FR-RIDE-051).
