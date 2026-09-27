# WF-01 Splash / role select

**Platform:** Android  
**Storyboards:** SB-01  
**Artifact:** ART-RIDE-UX-001

```
+--------------------------------------+
| 12:56                        5G  BT  |
+--------------------------------------+
|                                      |
|           R i d e A u d i t          |
|      Dual-phone telematics audit     |
|                                      |
|   [ GPL-2.0 ]  [ Play Integrity OK ] |
|                                      |
|   --------------------------------   |
|   |  Select your role for session  | |
|   --------------------------------   |
|                                      |
|   +------------------------------+   |
|   |   DRIVER                     |   |
|   |   Session coordinator        |   |
|   |   Clock master / submit      |   |
|   +------------------------------+   |
|                                      |
|   +------------------------------+   |
|   |   PASSENGER                  |   |
|   |   Video sync + composite     |   |
|   |   Telematics overlay         |   |
|   +------------------------------+   |
|                                      |
|   Vehicle: [ VA-1042          v ]    |
|                                      |
|   [ Continue to Bluetooth pairing ]  |
|                                      |
|   About / source  |  Privacy notice  |
+--------------------------------------+
```

## Behavior

- Role required before Continue.
- Play Integrity FAIL disables Continue and routes to WF-08.
- GPL-2.0 badge links to license text.
