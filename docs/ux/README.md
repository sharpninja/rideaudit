# RideAudit UX (dual-phone)

SVG wireframes, ASCII structural specs, storyboards, and session flow for the Avalonia UI 12 dual-phone capture path (Android).

## Architecture reflected

1. Bluetooth discovery establishes an authenticated driver-rider pairing (not a Lyft Bluetooth API).
2. Driver phone coordinates the session: shared clock, start/stop, admission readiness, submission orchestration.
3. Passenger phone performs video sync, joins/composites streams, and overlays realtime telematics (spider graph).
4. Seal-at-collect encrypts evidence at the collection boundary before durable storage or upload.
5. Play Integrity attestation binds the collector; failed attestation is fail-closed.
6. License: GPL-2.0. Public sealed submit only (no plaintext at ingest).
7. Custody receipts anchor primarily via Bitcoin OpenTimestamps (OTS). See `docs/architecture/blockchain-custody-receipts.md`.
8. UI stack: **Avalonia UI 12** for Android dual-phone apps and for the desktop court/counsel review app. Backend: **gRPC on .NET 10** containers. See `docs/architecture/stack.md`.

## Artifact

- `ARTIFACT.yaml` id: `ART-RIDE-UX-001` (mobile capture UX)
- Desktop court/counsel review UX: [`review-app/`](review-app/) id **`ART-RIDE-UX-REVIEW-001`**

## Index

### Storyboards

| ID | File | Focus |
| --- | --- | --- |
| SB-01 | [storyboards/SB-01-pairing.md](storyboards/SB-01-pairing.md) | Bluetooth discovery and role pairing |
| SB-02 | [storyboards/SB-02-driver-coordinate.md](storyboards/SB-02-driver-coordinate.md) | Driver session coordination |
| SB-03 | [storyboards/SB-03-passenger-composite.md](storyboards/SB-03-passenger-composite.md) | Passenger composite and telematics |
| SB-04 | [storyboards/SB-04-seal-receipt.md](storyboards/SB-04-seal-receipt.md) | Seal-at-collect and OTS receipt |
| SB-05 | [storyboards/SB-05-submit-admission.md](storyboards/SB-05-submit-admission.md) | Public sealed submit admission |
| SB-06 | [storyboards/SB-06-counsel-viewer.md](storyboards/SB-06-counsel-viewer.md) | Counsel desktop viewer overview (details in ART-RIDE-UX-REVIEW-001) |

### Wireframes (Avalonia UI 12 mobile)

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

SVG phone mocks with inline icons: [assets/wireframes/README.md](assets/wireframes/README.md).

### Flows

- [flows/mermaid-session-flow.md](flows/mermaid-session-flow.md) sequence diagram for a full dual-phone session

### Desktop review app (separate ART)

- [review-app/README.md](review-app/README.md) (`ART-RIDE-UX-REVIEW-001`)
- Workflow: [review-app/flows/review-workflow.md](review-app/flows/review-workflow.md), [review-app/flows/mermaid-review-workflow.md](review-app/flows/mermaid-review-workflow.md)
- Storyboards SB-R-01 .. SB-R-06 and wireframes WF-R-01 .. WF-R-08 under `review-app/`

## Related docs

- `docs/architecture/stack.md`
- `docs/architecture/dual-phone-bluetooth-roles.md`
- `docs/architecture/blockchain-custody-receipts.md`
- `artifacts/android/ARTIFACT.yaml` (`ART-RIDE-ANDROID-001`)
- `artifacts/server-api/ARTIFACT.yaml` (`ART-RIDE-API-001`)

## Process notes

- BDPv4 UX artifact. No em dashes in authored text.
- Do not invent Lyft private APIs. Pairing is RideAudit device pairing only.
