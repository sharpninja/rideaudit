# WF-R-08 Export for opposing counsel

**Platform:** Desktop (Avalonia UI 12)  
**Storyboards:** SB-R-06  
**Artifact:** ART-RIDE-UX-REVIEW-001  
**Chain default:** Bitcoin OpenTimestamps (btc-ots)

<!-- wireframe-svg:start -->

## Visual wireframe

Realistic SVG mock with inline icon paths. The ASCII block below stays the structural spec.

![WF-R-08 Export for opposing counsel](../../assets/wireframes/WF-R-08-export-opposing-counsel.svg)

[Open WF-R-08-export-opposing-counsel.svg](../../assets/wireframes/WF-R-08-export-opposing-counsel.svg)

<!-- wireframe-svg:end -->


```
+----------------------------------------------------------------------+
| Export disclosure pack                                               |
+----------------------------------------------------------------------+
| Scope: selected records in RB-77821 (per-record reports retained)    |
|                                                                      |
| Include                                                              |
|  [x] Sealed ciphertext packages                                      |
|  [x] Portable .ots custody proofs (Bitcoin OpenTimestamps)           |
|  [x] Attestation summaries (Play Integrity / signing cert digests)   |
|  [x] VerificationReport (pass/fail detail)                           |
|  [x] SyncClockOffset / overlay manifest metadata                     |
|  [ ] Decrypted working copy (discouraged; legal process only)        |
|                                                                      |
| Destination: [ D:\Disclosure\CASE-2026-0914-A1\                 ]    |
|                                                                      |
| Notes for opposing counsel                                           |
|  Verify .ots against Bitcoin headers independently.                  |
|  Do not treat RideAudit servers as sole trust root.                  |
|  Unverified Lyft-native gaps remain labeled in report.               |
|                                                                      |
| [ Build pack ]   [ Close ViewerSession after export ]   [ Cancel ]   |
+----------------------------------------------------------------------+
```

## Behavior

- Default pack is sealed + proofs + VerificationReport (not plaintext).
- Multi-driver exports keep per-record reports (no merged custody).
- Close session writes audit log and enforces working-copy expiry/purge.
