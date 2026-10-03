# Storyboard beat sequence via RemoteControl

TimestampUtc: 2026-09-29T19:21:22Z
Host: PAYTON-LEGION2
SPDX: GPL-2.0-only

Storyboard tests walk each markdown beat on one SharpNinja.Avalonia.RemoteControl session. A static screenshot is not storyboard coverage. The 2026-09-29T19:00:09Z run jumped by screen id, dropped repeated frames, and compared review baselines to the capture shell. That run is not storyboard coverage.

## Harness

`SharpNinja.aiUnit` 3.0.0, `SharpNinja.Avalonia.RemoteControl.Protocol` 0.7.4, runtime 0.7.4. `ActiveStrategy` is `codex-subscription`. Threshold is channel delta 24 and differing-pixel ratio 0.08. aiUnit 3.0.0 still has no pixel-threshold type.

Each storyboard restarts the capture app once and attaches the debug bridge on `127.0.0.1:47100`. Beats stay in markdown order. Consecutive duplicate SVG links inside one beat collapse to one step. A later repeat of the same screen stays a separate step. After each driven step the harness screenshots and compares that frame. If RemoteControl does not attach, the storyboard fails closed and no frame is compared.

Alternate fail-closed branches and review screens are not screenshot-compared. The harness does not press Return to jump back. `ConfirmCheck` is set with RemoteControl `SetProperty` `IsChecked=true`. An `InvokeClick` on that checkbox did not leave it checked, and pairing confirm then opened WF-08. The debug host allows `CheckBox.IsChecked` for that set. Other controls are still clicks.

## Device

Serial `RFCW7078MVZ`, model `SM_F936U`, screenshots 904 by 2316. Debug APK `src/RideAudit.Client.Android/bin/Debug/net10.0-android/org.rideaudit.app-Signed.apk`, 78753867 bytes, LastWriteTimeUtc 2026-09-29T19:19:14Z. The Release APK was not installed. Edge wireless attach is not claimed.

Storyboard theory: Failed 12, Passed 0, Skipped 0, Duration 1 m 39 s, ended 2026-09-29T19:21:22Z.

## What was driven

SB-01, one session: Driver on WF-01 (ratio 0.2753), Continue to WF-02 (0.2448), both WF-08 branches not taken, Peer to WF-03 (0.2196), Confirm to WF-04 (0.2883). Screen ids matched. Every ratio is above 0.08.

SB-02: entry landed on WF-04 (0.2883). The readiness beat stayed on WF-04 (0.2883) with no extra control. The fail-closed branch was not taken. Start was clicked and the screen became WF-08 (0.2533 against the WF-04 baseline). The active-coordination and seal beats were not driven after that, because returning would leave the session.

SB-03: passenger entry landed on WF-05. Four WF-05 beats were captured at 0.3130. The fail-closed branch was not taken. Seal (WF-06) cannot be driven from the passenger screen.

SB-04: the entry clicks include Start and Stop. The screen was WF-08, expected WF-06, ratio 0.2472. Later seal and submit beats were not driven.

SB-05: the entry clicks include Submit. Four WF-07 frames were captured at 0.2005. Submit opens WF-07 after start has already fail-closed. That is not a successful seal. The reject branch and the counsel handoff (WF-R-01) were not driven.

SB-06 and every SB-R storyboard: each step failed closed because the review screen is not on the Android capture client. Those steps were not screenshot-compared.

No driven frame was inside 0.08, so the codex-subscription perceptual call was not invoked on this run. A timeout is still a fail-closed result when that call runs.

## Sample

Driven frame SB-01 beat 4, pairing confirm, actual screen WF-04:

| File | Bytes | SHA256 |
| --- | --- | --- |
| SB-01-b04-f06-actual.png | 178779 | 8B691100345B3702EC2A67E73FCB856AE11D43649EA48DB0AD82ED618603E0D6 |
| SB-01-b04-f06-diff.png | 63148 | 1D4C4362C61E3F4D8A26FE9752C518BCCA91DA9B3A01F855EB5166B698564711 |

Log: `docs/receipts/android/aiunit-storyboard-20260929/storyboard-results.jsonl`.

## Not closed

Pixel agreement, perceptual agreement, review screens on the phone, seal and submit as successful live captures, AC-UC-025-001, FR-RIDE-073, UC-RIDE-043, TR-RIDE-VIDEO-017, TEST-RIDE-054, storyboard acceptance criteria, and plan section 9. `isSatisfied` stays false. This is not a visual match, a Play publication, or an OTS, Caddy, or hardware HSM claim.
