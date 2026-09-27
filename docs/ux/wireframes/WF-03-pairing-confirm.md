# WF-03 Pairing confirm

**Platform:** Android  
**Storyboards:** SB-01  
**Artifact:** ART-RIDE-UX-001

```
+--------------------------------------+
| <-  Confirm pairing           Role:D |
+--------------------------------------+
|  Session draft                       |
|  +--------------------------------+  |
|  | Session   DS-20260927-A1       |  |
|  | Vehicle   VA-1042              |  |
|  | Invite    7K2M                 |  |
|  +--------------------------------+  |
|                                      |
|  This phone                          |
|  +--------------------------------+  |
|  | Role: DRIVER (coordinator)     |  |
|  | Device: Pixel-Fold             |  |
|  | Play Integrity: OK             |  |
|  +--------------------------------+  |
|                                      |
|  Peer phone                          |
|  +--------------------------------+  |
|  | Role: PASSENGER (compositor)   |  |
|  | Device: Pixel-7a               |  |
|  | Play Integrity: OK             |  |
|  +--------------------------------+  |
|                                      |
|  [ ] I confirm driver role and       |
|      shared session intent           |
|                                      |
|  [ Reject ]      [ Confirm pair ]    |
+--------------------------------------+
```

## Behavior

- Confirm disabled until checkbox checked and peer role complementary.
- Role mismatch or Integrity FAIL on either side -> WF-08.
- On success: Driver -> WF-04; Passenger -> WF-05 await state.
