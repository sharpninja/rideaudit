# Wireframe UI rebuild and RemoteControl storyboard rerun

TimestampUtc: 2026-09-29T19:00:09Z
Host: PAYTON-LEGION2
SPDX: GPL-2.0-only

Payton directed this pass to stop hostile validation of the earlier aiUnit fail receipt, rebuild the Avalonia capture UI toward `docs/ux/assets/wireframes`, then rerun aiUnit with RemoteControl storyboard steps and a usability check. Acceptance criteria stay unsatisfied. This is not a visual match. It is not a Google Play publication.

## UI

`App.axaml` is light theme. Body type is 16 and the RideAudit wordmark is 28, which follows the approved splash mock. The earlier 28 body and 36 title scale is no longer the capture default.

`CaptureShellView` shows one capture screen at a time for WF-01 through WF-08: role select, Bluetooth discover, pairing confirm, driver dashboard, passenger capture, seal, sealed submit, and fail-closed. Cards, badges, and primary buttons use the mock palette (`#F4F7FA`, `#173E66`, `#FFFFFF`). Review wireframes WF-R-01 through WF-R-08 are still the desktop review shell. The Android capture client does not open them.

Headless `TestRide035ShellTests`: Passed 12, Failed 0.

Debug APK installed on Fold 4 USB `RFCW7078MVZ`: `src/RideAudit.Client.Android/bin/Debug/net10.0-android/org.rideaudit.app-Signed.apk`, 79082489 bytes, LastWriteTimeUtc 2026-09-29T18:31:36Z. RemoteControl stays debug-only.

## Device rerun

Serial `RFCW7078MVZ`, model `SM_F936U`, screenshots 904 by 2316. Skipped 0.

Threshold is unchanged: channel delta 24, differing-pixel ratio 0.08. `SharpNinja.aiUnit` 3.0.0 still has no pixel-threshold type.

Wireframe theory: Failed 16, Passed 0, Skipped 0, Duration 5 m 10 s. Capture screens that opened the named id: WF-01, WF-02, WF-03, WF-04, WF-05, WF-08. WF-06 and WF-07 landed on WF-08 because driver start fail-closed. Every WF-R screen stayed on WF-01. Ratios are about 0.15 to 0.38, down from about 0.92 on the dark shell, and still above 0.08. `WF-01` perceptual gate fail-closed on the 60 second codex timeout.

Storyboard theory: Failed 12, Passed 0, Skipped 0, Duration 14 m 3 s, ended 2026-09-29T19:00:09Z. Each storyboard restarts the app once and clicks through `SharpNinja.Avalonia.RemoteControl.Protocol` 0.7.4 on the debug bridge (`127.0.0.1:47100` after `adb forward`). SB-01 frames: WF-01, WF-02, WF-03, and WF-08 matched their screen ids. The following frame expected WF-04 and observed WF-08. Review frames were captured on the capture shell and failed closed. Tree usability found no overlapping visible buttons and no clipped text on the logged frames. That does not make the pixel compare pass.

Sample after the rebuild:

| File | Bytes | SHA256 |
| --- | --- | --- |
| WF-01-actual.png | 155872 | 623B890D535F7FB0A32C9686C4741D14402CF384A1899F82EF6CC7B49E0885B8 |
| WF-01-diff.png | 66456 | E125D73BD55B0236DC78BA94AE7B0D0052D7B2ED04A5500DBEB1A7260DF9EFC3 |

Logs: `docs/receipts/android/aiunit-wireframe-20260929/`.

## Not closed

Pixel agreement, perceptual agreement, review screens on the phone, seal and submit screens (WF-06, WF-07), AC-UC-025-001, FR-RIDE-073, UC-RIDE-043, TR-RIDE-VIDEO-017, TEST-RIDE-054, and plan section 9. `isSatisfied` stays false.
