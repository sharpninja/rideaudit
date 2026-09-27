# WF-R-05 Escrow release / quorum status

**Platform:** Desktop (Avalonia UI 12)  
**Storyboards:** SB-R-03  
**Artifact:** ART-RIDE-UX-REVIEW-001  
**Backend:** gRPC .NET 10 containers (escrow service)

```
+----------------------------------------------------------------------+
| CourtRelease / Escrow (M-of-N dual control)                          |
+----------------------------------------------------------------------+
| Case: CASE-2026-0914-A1     Record: passenger-composite-01           |
| Verification gate: PASS (OTS, hashes, Play Integrity, binding)       |
+----------------------------------------------------------------------+
| Policy: 2-of-3 custodians                                            |
|                                                                      |
| Custodian A (Court clerk)     [ APPROVED ]  2026-09-27 10:02 CT      |
| Custodian B (Counsel of record)[ PENDING  ]                          |
| Custodian C (Special master)  [ PENDING  ]                           |
|                                                                      |
| Quorum: 1 / 2 required   Status: BLOCKED (awaiting approvals)        |
|                                                                      |
| Legal process ref: [ SUBPOENA-4419-B                           ]     |
| Working copy expiry: [ 72 hours v ]  expires_at preview: 2026-09-30  |
|                                                                      |
| Sealed blob + custody receipt: UNCHANGED by this release             |
|                                                                      |
| [ Request custodian approvals ]   [ Attach CourtRelease ]<-disabled  |
| [ Cancel ]                                                           |
+----------------------------------------------------------------------+
```

## Behavior

- Attach CourtRelease enabled only when quorum met and verification still PASS.
- Decrypt to expiring working copy happens only after attach succeeds.
- No bypass / casual plaintext control.
