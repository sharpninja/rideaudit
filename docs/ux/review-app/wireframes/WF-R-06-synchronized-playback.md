# WF-R-06 Synchronized playback + spider scrub

**Platform:** Desktop (Avalonia UI 12)  
**Storyboards:** SB-R-04  
**Artifact:** ART-RIDE-UX-REVIEW-001

<!-- wireframe-svg:start -->

## Visual wireframe

Realistic SVG mock with inline icon paths. The ASCII block below stays the structural spec.

![WF-R-06 Synchronized playback and spider](../../assets/wireframes/WF-R-06-synchronized-playback.svg)

[Open WF-R-06-synchronized-playback.svg](../../assets/wireframes/WF-R-06-synchronized-playback.svg)

<!-- wireframe-svg:end -->


```
+----------------------------------------------------------------------+
| Playback  working copy expires_at: 2026-09-30 10:02 CT   [Valid]     |
+-------------------------------+--------------------------------------+
| Composite video               | Spider-graph telematics              |
| +---------------------------+ | +----------------------------------+ |
| |  [driver cam | pax cam]   | | |   accel X/Y/Z spider             | |
| |                           | | |      * current sample            | |
| |                           | | |                                  | |
| +---------------------------+ | +----------------------------------+ |
+-------------------------------+--------------------------------------+
| Timeline (SyncClockOffset +3.2 ms)                                   |
| |----|====*==============================================|----|      |
| 0:00      0:42 scrub                                  12:10          |
| Tracks: [Composite] [Spider] [GPS: present] [OBD: absent]            |
+----------------------------------------------------------------------+
| Coverage / gaps                                                      |
| - Dual-phone composite: present                                      |
| - RideAudit telematics overlay: present                              |
| - Lyft-native driver score feed: Unverified (not collected)          |
| - Lyft private APIs: not used                                        |
+----------------------------------------------------------------------+
| [Play] [Pause] [Scrub] [VerificationReport] [Provenance/OTS] [Export]|
+----------------------------------------------------------------------+
```

## Behavior

- Available only after fail-closed verify + CourtRelease + non-expired working copy.
- Scrub keeps composite and spider aligned via SyncClockOffset.
- Unverified gaps labeled; no invented Lyft API data.
