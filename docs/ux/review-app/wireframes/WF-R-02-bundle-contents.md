# WF-R-02 Bundle contents (sealed)

**Platform:** Desktop (Avalonia UI 12)  
**Storyboards:** SB-R-01, SB-R-05  
**Artifact:** ART-RIDE-UX-REVIEW-001

<!-- wireframe-svg:start -->

## Visual wireframe

Realistic SVG mock with inline icon paths. The ASCII block below stays the structural spec.

![WF-R-02 Bundle contents (sealed)](../../assets/wireframes/WF-R-02-bundle-contents.svg)

[Open WF-R-02-bundle-contents.svg](../../assets/wireframes/WF-R-02-bundle-contents.svg)

<!-- wireframe-svg:end -->


```
+----------------------------------------------------------------------+
| Bundle RB-77821  Case CASE-2026-0914-A1           ViewerSession open |
+----------------------------------------------------------------------+
| Vehicle: VA-1042     Drivers in bundle: 2                            |
| Decrypt: DISABLED (verify + escrow required)                         |
+----------------------------------------------------------------------+
| Sealed packages                                                      |
| +----+---------------------------+--------+--------+---------------+ |
| | #  | Package                   | Role   | Status | Receipt       | |
| +----+---------------------------+--------+--------+---------------+ |
| | 1  | passenger-composite-01    | Pax    | SEALED | ots: a3f2...  | |
| | 2  | driver-telematics-01      | Drv    | SEALED | ots: 91c0...  | |
| | 3  | optional-raw-pax-cam      | Pax    | SEALED | ots: 44ab...  | |
| +----+---------------------------+--------+--------+---------------+ |
|                                                                      |
| Selected: passenger-composite-01                                     |
| content_hash: sha256:9e2c...   SyncClockOffset: +3.2 ms (metadata)   |
| Dual-phone: Driver coordinator / Passenger compositor                |
|                                                                      |
| [ Run verification ]  [ Escrow... ]  [ Play ]<-disabled  [ Export ]   |
+----------------------------------------------------------------------+
```

## Behavior

- List is ciphertext/metadata only.
- Play disabled until verification pass + CourtRelease + non-expired working copy.
- Multi-driver bundles show per-record rows (no merged custody).
