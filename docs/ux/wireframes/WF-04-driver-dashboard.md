# WF-04 Driver dashboard

**Platform:** Android  
**Storyboards:** SB-02, SB-05  
**Artifact:** ART-RIDE-UX-001

```
+--------------------------------------+
|  Driver dashboard             Role:D |
+--------------------------------------+
|  Session DS-20260927-A1   VA-1042    |
|  Peer: Pixel-7a  LINK OK   lat 12ms  |
|                                      |
|  Readiness                           |
|  [OK] Bluetooth pair                 |
|  [OK] Passenger compositor ready     |
|  [OK] Play Integrity                 |
|  [OK] Seal module                    |
|  [OK] Storage                        |
|                                      |
|  Clock master (this phone)           |
|  Epoch: 2026-09-27T17:56:00Z         |
|  SyncClockOffset (peer): +3.2 ms     |
|                                      |
|  Status: PAIRED / IDLE               |
|  Elapsed: --:--:--                   |
|                                      |
|  +--------------------------------+  |
|  |        [ START SESSION ]       |  |
|  +--------------------------------+  |
|  +--------------------------------+  |
|  |        [ STOP  ]  (disabled)   |  |
|  +--------------------------------+  |
|                                      |
|  After stop: Seal -> Submit          |
|  [ View seal / submit status ]       |
+--------------------------------------+
```

## Behavior

- START enabled only when all readiness rows OK.
- START publishes clock + START command to passenger.
- STOP enabled only while ACTIVE; then routes to WF-06.
