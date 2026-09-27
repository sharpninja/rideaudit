# RideAudit wireframe SVGs

Realistic light-theme wireframes for the Avalonia UI 12 dual-phone capture app and the desktop court/counsel review app. Each screen is a phone or window mock: status bar, app bar or menu, cards, badges, and inline SVG path icons.

Icons are drawn inside each screen file (no emoji, no PNG/JPEG, no remote icon font). The same drawings are also saved as individual files in [`../icons/`](../icons/) for reuse.

License: GPL-2.0.

ASCII structural specs stay in the wireframe markdown next to these SVGs. These mocks are not a running Avalonia build. Spacing, type, and control metrics in a future implementation are **Unverified** until that build exists.

## Mobile phone chrome

| SVG | Wireframe | Storyboards | Screen |
| --- | --- | --- | --- |
| [WF-01-splash-role-select.svg](WF-01-splash-role-select.svg) | WF-01 | SB-01 | Splash / role select |
| [WF-02-bt-discover.svg](WF-02-bt-discover.svg) | WF-02 | SB-01 | Bluetooth discover |
| [WF-03-pairing-confirm.svg](WF-03-pairing-confirm.svg) | WF-03 | SB-01 | Pairing confirm |
| [WF-04-driver-dashboard.svg](WF-04-driver-dashboard.svg) | WF-04 | SB-02, SB-05 | Driver dashboard |
| [WF-05-passenger-capture-spider.svg](WF-05-passenger-capture-spider.svg) | WF-05 | SB-03 | Passenger capture and spider |
| [WF-06-seal-progress.svg](WF-06-seal-progress.svg) | WF-06 | SB-04 | Seal progress |
| [WF-07-submit-status.svg](WF-07-submit-status.svg) | WF-07 | SB-05 | Submit status |
| [WF-08-fail-closed-errors.svg](WF-08-fail-closed-errors.svg) | WF-08 | SB-01, SB-02, SB-03, SB-04, SB-05 | Fail-closed errors |

## Desktop window chrome

| SVG | Wireframe | Storyboards | Screen |
| --- | --- | --- | --- |
| [WF-R-01-splash-case-open.svg](WF-R-01-splash-case-open.svg) | WF-R-01 | SB-R-01, SB-06 | Splash / case open |
| [WF-R-02-bundle-contents.svg](WF-R-02-bundle-contents.svg) | WF-R-02 | SB-R-01, SB-R-05, SB-06 | Bundle contents (sealed) |
| [WF-R-03-verification-report.svg](WF-R-03-verification-report.svg) | WF-R-03 | SB-R-02, SB-R-05, SB-06 | Verification report |
| [WF-R-04-fail-closed-blocking.svg](WF-R-04-fail-closed-blocking.svg) | WF-R-04 | SB-R-02, SB-R-05 | Fail-closed blocking |
| [WF-R-05-escrow-release-quorum.svg](WF-R-05-escrow-release-quorum.svg) | WF-R-05 | SB-R-03 | Escrow release / quorum |
| [WF-R-06-synchronized-playback.svg](WF-R-06-synchronized-playback.svg) | WF-R-06 | SB-R-04, SB-06 | Synchronized playback and spider |
| [WF-R-07-provenance-custody-ots.svg](WF-R-07-provenance-custody-ots.svg) | WF-R-07 | SB-R-02, SB-R-06, SB-06 | Provenance / OTS custody |
| [WF-R-08-export-opposing-counsel.svg](WF-R-08-export-opposing-counsel.svg) | WF-R-08 | SB-R-06, SB-06 | Export for opposing counsel |

## Storyboard index

| Storyboard | Screens |
| --- | --- |
| SB-01 Pairing | WF-01, WF-02, WF-03, WF-08 |
| SB-02 Driver coordinate | WF-04, WF-06, WF-07, WF-08 |
| SB-03 Passenger composite | WF-05, WF-08 |
| SB-04 Seal and receipt | WF-06, WF-08 |
| SB-05 Submit admission | WF-07, WF-08 |
| SB-06 Counsel viewer overview | WF-R-01, WF-R-02, WF-R-03, WF-R-06, WF-R-07, WF-R-08 |
| SB-R-01 Open bundle | WF-R-01, WF-R-02 |
| SB-R-02 Verification gate | WF-R-03, WF-R-04, WF-R-07 |
| SB-R-03 Escrow release | WF-R-05 |
| SB-R-04 Timeline playback | WF-R-06 |
| SB-R-05 Multi-driver counsel bundle | WF-R-02, WF-R-03, WF-R-04 |
| SB-R-06 Export disclosure | WF-R-07, WF-R-08 |

## Icons

Inline path icons used by the wireframes. Standalone files live in `docs/ux/assets/icons/`.

`activity`, `alert`, `ban`, `battery`, `bluetooth`, `broadcast`, `camera`, `car`, `check`, `checkbox`, `checkbox-on`, `chevron-down`, `chevron-left`, `chevron-right`, `clock`, `compass`, `crosshair`, `download`, `eye`, `file`, `folder`, `gauge`, `gavel`, `hash`, `info`, `key`, `layers`, `link`, `list`, `lock`, `map-pin`, `minus`, `pause`, `phone`, `play`, `radio`, `radio-on`, `refresh`, `scale`, `search`, `shield`, `shield-check`, `signal`, `signal-2`, `signal-3`, `square`, `stamp`, `steering`, `stop`, `upload`, `user`, `video`, `wifi`, `x`.

## Not a screen mock

These stay sequence diagrams. They are not UI wireframes:

- `docs/ux/flows/mermaid-session-flow.md`
- `docs/ux/review-app/flows/mermaid-review-workflow.md`
- `docs/ux/review-app/flows/review-workflow.md`

## Gaps labeled Unverified

- Lyft-native driver score feed on WF-R-06 is labeled **Unverified** (not collected). The mocks do not invent a Lyft API.
- Bitcoin block height on WF-R-07 is shown as upgraded in the proof, not as a specific height.
- Control metrics for a future Avalonia UI 12 build are **Unverified**.
