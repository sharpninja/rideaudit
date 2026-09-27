# WF-08 Fail-closed errors

**Platform:** Android  
**Storyboards:** SB-01 .. SB-05  
**Artifact:** ART-RIDE-UX-001

```
+--------------------------------------+
|  Action blocked (fail-closed)        |
+--------------------------------------+
|                                      |
|            !!  NOT ADMISSIBLE        |
|                                      |
|  +--------------------------------+  |
|  | Code: PI_ATTESTATION_FAILED    |  |
|  | Play Integrity did not pass.   |  |
|  | Sealed submit is blocked.      |  |
|  +--------------------------------+  |
|                                      |
|  What you can do                     |
|  - Use an approved Play build        |
|  - Re-run attestation                |
|  - Pair again after device fix       |
|                                      |
|  Evidence was NOT uploaded.          |
|  Local unsealed buffers are held     |
|  only until seal succeeds or purge.  |
|                                      |
|  [ View details ]   [ Dismiss ]      |
|  [ Return to safe screen ]           |
+--------------------------------------+
```

## Common reason codes (illustrative)

| Code | When |
| --- | --- |
| BT_DISABLED | Bluetooth off or permission denied |
| PAIR_ROLE_MISMATCH | Both sides claimed same role |
| PAIR_TIMEOUT | Discovery/confirm timed out |
| PI_ATTESTATION_FAILED | Play Integrity fail |
| SEAL_FAILED | Encrypt/hash/receipt step failed |
| OTS_INCOMPLETE | Receipt policy not met for admit |
| ADMISSION_REJECTED | Server rejected sealed package |
| DESYNC_THRESHOLD | Passenger SyncClockOffset too large |
| CAMERA_LOST | Passenger capture fault |

## Behavior

- All destructive or admit paths fail closed: no silent continue.
- Copy must never invent Lyft API success when data is missing.
