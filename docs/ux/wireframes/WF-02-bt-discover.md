# WF-02 Bluetooth discover

**Platform:** Android  
**Storyboards:** SB-01  
**Artifact:** ART-RIDE-UX-001

```
+--------------------------------------+
| <-  Bluetooth discover        Role:D |
+--------------------------------------+
|  Scanning for RideAudit peers...     |
|  Invite code: 7K2M                   |
|  [=============............]  0:18   |
|                                      |
|  Nearby peers                        |
|  +--------------------------------+  |
|  | Pixel-7a  PASSENGER?   -42dBm  |  |
|  | code hint: 7K2M          [ > ] |  |
|  +--------------------------------+  |
|  +--------------------------------+  |
|  | SM-S911   unknown      -71dBm  |  |
|  | code hint: ----          [ > ] |  |
|  +--------------------------------+  |
|                                      |
|  Advertising as: RideAudit-Driver    |
|  Vehicle: VA-1042                    |
|                                      |
|  [ Cancel ]          [ Rescan ]      |
|                                      |
|  Tip: both phones must confirm roles |
|  This is RideAudit pairing, not Lyft |
+--------------------------------------+
```

## Behavior

- Driver advertises; passenger scans (mirror labels when Role:P).
- Timeout or BT off -> WF-08 fail-closed.
- Selecting a peer opens WF-03.
