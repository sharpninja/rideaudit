# WF-R-01 Splash / case open

**Platform:** Desktop (Avalonia UI 12; Windows / Linux / macOS)  
**Storyboards:** SB-R-01  
**Artifact:** ART-RIDE-UX-REVIEW-001

```
+----------------------------------------------------------------------+
| RideAudit Court Review                          [GPL-2.0]  v0.1.0    |
+----------------------------------------------------------------------+
|                                                                      |
|              R i d e A u d i t   C o u r t   R e v i e w             |
|         Avalonia UI 12  |  Fail-closed verify before decrypt         |
|                                                                      |
|   +------------------------------------------------------------+     |
|   | Open admitted submission / RideBundle                      |     |
|   | Case id:     [ CASE-2026-0914-A1                      ]    |     |
|   | Bundle id:   [ RB-77821                               ]    |     |
|   | Reviewer:    [ Counsel  v ]   Org: [ Firm / Court     ]    |     |
|   +------------------------------------------------------------+     |
|                                                                      |
|   Chain default: Bitcoin OpenTimestamps (btc-ots)                    |
|   Backend: gRPC .NET 10 (containers)                                 |
|                                                                      |
|   [ Open bundle ]          [ Quit ]                                  |
|                                                                      |
|   About / source  |  License  |  Privacy / legal process notice      |
+----------------------------------------------------------------------+
```

## Behavior

- Opens only admitted sealed submissions/bundles.
- Starts ViewerSession on successful open.
- Does not decrypt on open.
