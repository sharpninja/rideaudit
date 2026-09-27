# WF-06 Seal progress

**Platform:** Android (both roles)  
**Storyboards:** SB-04  
**Artifact:** ART-RIDE-UX-001  
**Chain default:** btc-ots (Bitcoin OpenTimestamps)

<!-- wireframe-svg:start -->

## Visual wireframe

Realistic SVG mock with inline icon paths. The ASCII block below stays the structural spec.

![WF-06 Seal progress](../assets/wireframes/WF-06-seal-progress.svg)

[Open WF-06-seal-progress.svg](../assets/wireframes/WF-06-seal-progress.svg)

<!-- wireframe-svg:end -->


```
+--------------------------------------+
|  Seal at collect              Role:* |
+--------------------------------------+
|  Session DS-20260927-A1              |
|  Package: passenger-composite-01     |
|                                      |
|  [DONE] Freeze buffers               |
|  [DONE] Hash payload                 |
|  [>>>>] Encrypt (session key)   62%  |
|  [    ] Assemble custody receipt     |
|  [    ] Bind Play Integrity digest   |
|  [    ] OTS stamp (Bitcoin)          |
|                                      |
|  +--------------------------------+  |
|  | chain_id: btc-ots              |  |
|  | receipt: pending               |  |
|  | attest: OK                     |  |
|  | SyncClockOffset: +3.2 ms       |  |
|  +--------------------------------+  |
|                                      |
|  Do not power off.                   |
|  Unsealed data will not be uploaded. |
|                                      |
|  [ Cancel seal ]  (marks fail-closed)|
+--------------------------------------+
```

## Behavior

- Seal must finish before submit path unlocks.
- OTS may remain pending offline; admission waits for policy (SB-05).
- Any step failure -> WF-08; package not admissible.
