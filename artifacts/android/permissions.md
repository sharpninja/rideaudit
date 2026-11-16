# Android permissions (minimization)

License: GPL-2.0

## Declared capabilities

| Permission / capability | Why | Minimization |
|-------------------------|-----|--------------|
| Camera | Dual-phone video for audit evidence | Only while an active audit session is running; stop on session end. |
| Fine / coarse location | Trip correlation and custody metadata; may be required for Bluetooth scan on some API levels | Collect at session-appropriate rate; keep precise location unmasked per approved FR-RIDE-077 (no FR-RIDE-202 mask/RBAC). |
| Body sensors / motion (accelerometer, gyro) | Telematics samples and spider-graph overlay (passenger phone) | Prefer SENSOR delay appropriate to audit, not continuous highest rate when idle. |
| Bluetooth (connect / scan / advertise) | Driver-rider phone discovery and session control | Only for RideAudit pairing; stop advertising when session ends. |
| Internet | Sealed upload (driver phone), attestation, clock assist | TLS only; no plaintext media egress. |
| Foreground service (if used) | Keep capture alive during trip | User-visible ongoing notification; stop when session ends. |

## Not requested by default

- Contacts, SMS, call log, microphone (unless a future FR explicitly requires cabin audio with consent).
- All-files storage access; prefer app-scoped storage for sealed blobs.
- Background location when no audit session is active.

## Consent and provenance

Permission grants and consent events should be recorded in the provenance / consent ledger path (FR-RIDE-006) where applicable. Drivers must understand that sealed evidence may be decryptable only under court-authorized escrow release.
