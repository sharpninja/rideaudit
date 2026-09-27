# RideAudit UX (dual-phone)

ASCII wireframes, storyboards, and session flow for the Android dual-phone capture path.

## Architecture reflected

1. Bluetooth discovery establishes an authenticated driver-rider pairing (not a Lyft Bluetooth API).
2. Driver phone coordinates the session: shared clock, start/stop, admission readiness, submission orchestration.
3. Passenger phone performs video sync, joins/composites streams, and overlays realtime telematics (spider graph).
4. Seal-at-collect encrypts evidence at the collection boundary before durable storage or upload.
5. Play Integrity attestation binds the collector; failed attestation is fail-closed.
6. License: GPL-2.0. Public sealed submit only (no plaintext at ingest).
7. Custody receipts anchor primarily via Bitcoin OpenTimestamps (OTS). See `docs/architecture/blockchain-custody-receipts.md`.

## Artifact

- `ARTIFACT.yaml` id: `ART-RIDE-UX-001`

## Index

### Storyboards

| ID | File | Focus |
| --- | --- | --- |
| SB-01 | [storyboards/SB-01-pairing.md](storyboards/SB-01-pairing.md) | Bluetooth discovery and role pairing |
| SB-02 | [storyboards/SB-02-driver-coordinate.md](storyboards/SB-02-driver-coordinate.md) | Driver session coordination |
| SB-03 | [storyboards/SB-03-passenger-composite.md](storyboards/SB-03-passenger-composite.md) | Passenger composite and telematics |
| SB-04 | [storyboards/SB-04-seal-receipt.md](storyboards/SB-04-seal-receipt.md) | Seal-at-collect and OTS receipt |
| SB-05 | [storyboards/SB-05-submit-admission.md](storyboards/SB-05-submit-admission.md) | Public sealed submit admission |
| SB-06 | [storyboards/SB-06-counsel-viewer.md](storyboards/SB-06-counsel-viewer.md) | Counsel desktop viewer overview |

### Wireframes (Android)

| ID | File | Screen |
| --- | --- | --- |
| WF-01 | [wireframes/WF-01-splash-role-select.md](wireframes/WF-01-splash-role-select.md) | Splash / role select |
| WF-02 | [wireframes/WF-02-bt-discover.md](wireframes/WF-02-bt-discover.md) | Bluetooth discover |
| WF-03 | [wireframes/WF-03-pairing-confirm.md](wireframes/WF-03-pairing-confirm.md) | Pairing confirm |
| WF-04 | [wireframes/WF-04-driver-dashboard.md](wireframes/WF-04-driver-dashboard.md) | Driver dashboard |
| WF-05 | [wireframes/WF-05-passenger-capture-spider.md](wireframes/WF-05-passenger-capture-spider.md) | Passenger capture + spider graph |
| WF-06 | [wireframes/WF-06-seal-progress.md](wireframes/WF-06-seal-progress.md) | Seal progress |
| WF-07 | [wireframes/WF-07-submit-status.md](wireframes/WF-07-submit-status.md) | Submit status |
| WF-08 | [wireframes/WF-08-fail-closed-errors.md](wireframes/WF-08-fail-closed-errors.md) | Fail-closed errors |

### Flows

- [flows/mermaid-session-flow.md](flows/mermaid-session-flow.md) sequence diagram for a full dual-phone session

## Related docs

- `docs/architecture/dual-phone-bluetooth-roles.md`
- `docs/architecture/blockchain-custody-receipts.md`
- `artifacts/android/ARTIFACT.yaml` (`ART-RIDE-ANDROID-001`)
- `artifacts/server-api/ARTIFACT.yaml` (`ART-RIDE-API-001`)

## Process notes

- BDPv4 UX artifact. No em dashes in authored text.
- Do not invent Lyft private APIs. Pairing is RideAudit device pairing only.
