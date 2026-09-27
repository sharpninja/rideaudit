# SB-R-04 Timeline playback

**Artifact:** ART-RIDE-UX-REVIEW-001  
**FR links:** FR-RIDE-047, FR-RIDE-051  
**NFR:** NFR-21  
**Screens:** [WF-R-06](../../assets/wireframes/WF-R-06-synchronized-playback.svg)
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

   ![WF-R-06 Synchronized playback and spider](../../assets/wireframes/WF-R-06-synchronized-playback.svg)

   [Open WF-R-06-synchronized-playback.svg](../../assets/wireframes/WF-R-06-synchronized-playback.svg)

2. **Synchronized scrub**  
   Timeline scrub keeps video, spider graph, and optional GPS/OBD tracks aligned via `SyncClockOffset`. Missing components are listed, not invented.

   ![WF-R-06 Synchronized playback and spider](../../assets/wireframes/WF-R-06-synchronized-playback.svg)

   [Open WF-R-06-synchronized-playback.svg](../../assets/wireframes/WF-R-06-synchronized-playback.svg)

3. **Coverage / gaps**  
   Side panel shows coverage matrix and **Unverified** caveats for absent Lyft-native signals. No Lyft private API claims.

   ![WF-R-06 Synchronized playback and spider](../../assets/wireframes/WF-R-06-synchronized-playback.svg)

   [Open WF-R-06-synchronized-playback.svg](../../assets/wireframes/WF-R-06-synchronized-playback.svg)

4. **Integrity recall**  
   Counsel can jump to VerificationReport / provenance without leaving the review context.

   Verification report:

   ![WF-R-03 Verification report](../../assets/wireframes/WF-R-03-verification-report.svg)

   [Open WF-R-03-verification-report.svg](../../assets/wireframes/WF-R-03-verification-report.svg)

   Provenance:

   ![WF-R-07 Provenance / OTS custody](../../assets/wireframes/WF-R-07-provenance-custody-ots.svg)

   [Open WF-R-07-provenance-custody-ots.svg](../../assets/wireframes/WF-R-07-provenance-custody-ots.svg)

## Success criteria

- Playback requires prior verify + escrow success (FR-RIDE-047).
- Dual-phone composite and spider overlay are first-class.
- Gaps are labeled Unverified when Lyft-native data was not collected.

## Notes

Viewer operates independently of the original collection devices (FR-RIDE-051).
