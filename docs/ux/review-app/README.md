# RideAudit desktop court / counsel review app (UX)

SVG wireframes, ASCII structural specs, storyboards, and review workflow for the **Avalonia UI 12** desktop court/counsel viewer (Windows, Linux, macOS).

## Purpose

Counsel, auditors, and court staff review admitted sealed evidence (`RideBundle` / submissions) with independent provenance checks. The app:

1. Loads sealed packages without decrypting.
2. Independently verifies Bitcoin OpenTimestamps (OTS) custody receipts, payload hashes, Play Integrity / signing certificate + nonce/key binding, and escrow release authorization.
3. **Fails closed** on any verification failure (no decrypt, no plaintext display).
4. Attaches court-authorized `CourtRelease` / M-of-N escrow release, then decrypts only to an **expiring authorized working copy**.
5. Plays a synchronized timeline (composite + spider-graph telematics, GPS/OBD when present) using `SyncClockOffset`.
6. Surfaces coverage / gaps with **Unverified** labels where Lyft-native signals are absent (no invented Lyft APIs).
7. Exports a disclosure pack (sealed + `.ots` + attestation + `VerificationReport`).
8. Creates auditable `ViewerSession` and `VerificationReport` per review, then closes the session with an access log.

## Artifact

- `ARTIFACT.yaml` id: **ART-RIDE-UX-REVIEW-001**
- UI stack: **Avalonia UI 12** (same UI family as Android dual-phone apps; see `docs/architecture/stack.md`)
- Backend: **gRPC on .NET 10**, deployed as **containers** (review app may call sealed-admission / escrow / bundle services; never casual decrypt)

## Relation to ART-RIDE-UX-001

| Artifact | Focus |
| --- | --- |
| `ART-RIDE-UX-001` (`docs/ux/`) | Avalonia UI 12 **mobile** dual-phone capture: pairing, coordinate, composite, seal, submit |
| `ART-RIDE-UX-REVIEW-001` (this package) | Avalonia UI 12 **desktop** court/counsel review: verify, escrow, playback, export |

SB-06 in the mobile package is an overview handoff only. Detailed counsel screens live here.

## Architecture reflected

1. GPL-2.0 desktop viewer on Windows, Linux, macOS (FR-RIDE-049 / section 3.6).
2. Decrypt **only** via court-authorized escrow / M-of-N dual-control; never casual plaintext (FR-RIDE-020, FR-RIDE-022, FR-RIDE-049).
3. Before decrypt or display: independent verify OTS/Bitcoin receipt, hashes, Play Integrity binding, escrow auth; **fail closed** (FR-RIDE-028, FR-RIDE-050, NFR-22).
4. Dual-phone evidence: driver coordinator / passenger compositor; composite + spider-graph; `SyncClockOffset` (FR-RIDE-047, FR-RIDE-051, NFR-21).
5. `ViewerSession` + `VerificationReport` per review (FR-RIDE-052).
6. Counsel multi-driver bundle: **per-record** verification, not merged custody (FR-RIDE-038).
7. Expiring authorized working copy only (`CourtRelease.expires_at`).
8. Primary chain: Bitcoin OpenTimestamps (`btc-ots`).

## Index

### Storyboards

| ID | File | Focus |
| --- | --- | --- |
| SB-R-01 | [storyboards/SB-R-01-open-bundle.md](storyboards/SB-R-01-open-bundle.md) | Open admitted submission / RideBundle |
| SB-R-02 | [storyboards/SB-R-02-verification-gate.md](storyboards/SB-R-02-verification-gate.md) | Fail-closed verification gate |
| SB-R-03 | [storyboards/SB-R-03-escrow-release.md](storyboards/SB-R-03-escrow-release.md) | Escrow / CourtRelease M-of-N |
| SB-R-04 | [storyboards/SB-R-04-timeline-playback.md](storyboards/SB-R-04-timeline-playback.md) | Synchronized playback + spider |
| SB-R-05 | [storyboards/SB-R-05-multi-driver-counsel-bundle.md](storyboards/SB-R-05-multi-driver-counsel-bundle.md) | Multi-driver per-record verify |
| SB-R-06 | [storyboards/SB-R-06-export-disclosure.md](storyboards/SB-R-06-export-disclosure.md) | Export disclosure pack + close session |

### Wireframes (desktop Avalonia)

| ID | File | Screen |
| --- | --- | --- |
| WF-R-01 | [wireframes/WF-R-01-splash-case-open.md](wireframes/WF-R-01-splash-case-open.md) | Splash / case open |
| WF-R-02 | [wireframes/WF-R-02-bundle-contents.md](wireframes/WF-R-02-bundle-contents.md) | Bundle contents (sealed) |
| WF-R-03 | [wireframes/WF-R-03-verification-report.md](wireframes/WF-R-03-verification-report.md) | Verification report panel |
| WF-R-04 | [wireframes/WF-R-04-fail-closed-blocking.md](wireframes/WF-R-04-fail-closed-blocking.md) | Fail-closed blocking |
| WF-R-05 | [wireframes/WF-R-05-escrow-release-quorum.md](wireframes/WF-R-05-escrow-release-quorum.md) | Escrow release / quorum |
| WF-R-06 | [wireframes/WF-R-06-synchronized-playback.md](wireframes/WF-R-06-synchronized-playback.md) | Synchronized playback + spider |
| WF-R-07 | [wireframes/WF-R-07-provenance-custody-ots.md](wireframes/WF-R-07-provenance-custody-ots.md) | Provenance / OTS custody |
| WF-R-08 | [wireframes/WF-R-08-export-opposing-counsel.md](wireframes/WF-R-08-export-opposing-counsel.md) | Export for opposing counsel |

SVG window mocks with inline icons: [../assets/wireframes/README.md](../assets/wireframes/README.md).

### Flows

- [flows/mermaid-review-workflow.md](flows/mermaid-review-workflow.md) sequential mermaid for the full review path
- [flows/review-workflow.md](flows/review-workflow.md) prose steps with acceptance criteria (Byrd Dev Process)

## Related docs

- `docs/architecture/stack.md`
- `docs/architecture/blockchain-custody-receipts.md`
- `docs/source/lyft-telematics-audit-requirements.md` (section 3.6; FR-20/22/28/38/47/49/50/52; NFR-21/22; ViewerSession / VerificationReport / CourtRelease / RideBundle)
- `docs/ux/README.md` (`ART-RIDE-UX-001`)
- `docs/ux/storyboards/SB-06-counsel-viewer.md` (overview pointer)

## Process notes

- BDPv4 UX artifact. No em dashes in authored text.
- Do not invent Lyft private APIs. Label **Unverified** gaps.
- Docs and wireframes only in this package (SVG mocks and ASCII structural specs; no app scaffolds).
