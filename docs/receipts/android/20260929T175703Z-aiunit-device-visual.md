# aiUnit device visual compares on Fold 4

TimestampUtc: 2026-09-29T17:57:03Z
Host: PAYTON-LEGION2
SPDX: GPL-2.0-only

This is a lab receipt for a connected-device screenshot compare. It is not a visual match. It is not a Google Play publication. It does not close live OTS, Caddy edge TLS, or hardware HSM. It does not mark AC-UC-025-001, FR-RIDE-073, UC-RIDE-043, TR-RIDE-VIDEO-017, TEST-RIDE-054, or any storyboard acceptance criterion satisfied. Plan section 9 is not closed. Plan boxes are unchanged.

## Package

`Directory.Packages.props` pins `SharpNinja.aiUnit` 3.0.0 from nuget.org. That package is an xUnit frontier-model extension. It has no pixel-threshold type. Visual input is `FrontierAttachment` (`image/png`) on `IFrontierModelClient.SendAsync`.

`RideAudit.Client.Android.csproj` references it with `PrivateAssets` all and `ExcludeAssets` runtime, so the package is a compile reference and its runtime assets stay out of the APK. `tests/RideAudit.Client.Android.AiUnit.Tests` references the same package for the runner. `Svg.Skia` 5.2.3 rasterizes SVG baselines. It is not a substitute for aiUnit.

`appsettings.aiunit.json` sets `AiUnit.ActiveStrategy` to `codex-subscription` (`Kind` cli, `Command` codex, `Model` `(cli-managed)`, `TimeoutSeconds` 60). `CodexVisualGate.RequireCodexSubscriptionProfile` throws when the active strategy is anything else.

Android project restore of that PackageReference succeeded earlier in this session (restore reported up to date on the test project before both device runs below).

## How to run on LEGION2

With `adb` on PATH or `ANDROID_HOME` set, and a device in the `device` state:

```powershell
dotnet test tests/RideAudit.Client.Android.AiUnit.Tests/RideAudit.Client.Android.AiUnit.Tests.csproj
```

The runner prefers USB serial `RFCW7078MVZ`. If that phone is absent it uses a `motorola_edge_2024` line. If neither is present the device tests throw. They do not skip.

## Threshold

Harness constants in `VisualThreshold`, not an aiUnit API:

- A pixel mismatches when any RGB channel differs by more than 24 (`ChannelDelta`).
- The frame fails when the differing-pixel ratio is above 0.08 (`MaxDifferingPixelRatio`).
- The diff PNG paints mismatched pixels red `(220,40,40)` and matching pixels gray.
- The SVG is scaled to the screenshot size before the loop.

The codex-subscription perceptual call runs for wireframe `WF-01` and for any frame already inside the pixel ratio. A usability defect from that call fails the test even when the pixel ratio passed. A missing client fails closed. It is not a skip. This run never reached a within-threshold frame, so the perceptual call ran only for `WF-01`.

## Device

`adb devices -l` at the start of this pass:

- `RFCW7078MVZ` product `q4qsqw` model `SM_F936U` (chosen)
- `192.168.0.137:42825` and `adb-ZD222QH58Q-DiWqEr._adb-tls-connect._tcp`, both motorola edge 2024 (not chosen)

Every jsonl row records `Serial` `RFCW7078MVZ` and `Model` `SM_F936U`. Screenshot size on every row is 904 by 2316. The installed package is the existing debug APK `org.rideaudit.app`. This pass did not reinstall and did not re-pair.

Capture path that produced PNGs: `uiautomator dump /data/local/tmp/rideaudit-ui.xml` then `cat`, and `screencap -p /data/local/tmp/rideaudit-screen.png` then `adb pull`. PNG magic of the committed sample is `89 50 4E 47`.

## Runs

Skipped count is 0 on every run. No wireframe or storyboard was omitted.

Earlier attempts the same day, before a PNG existed:

1. Filter `FullyQualifiedName~Device_screenshot`: Failed 28, Passed 0, Skipped 0, Total 28, Duration 3 m 27 s. Cause: `uiautomator dump /dev/tty` returned the dump path text and no XML.
2. Filter `FullyQualifiedName~WireframeDeviceTests` after the dump-to-file fix: Failed 16, Passed 0, Skipped 0, Total 16, Duration 1 m 35 s. Cause: `exec-out screencap -p` did not yield a PNG (`screencap failed:` with empty detail).

Capture run after `adb pull`, filter `FullyQualifiedName~WireframeDeviceTests`: Failed 16, Passed 0, Skipped 0, Total 16, Duration 3 m 44 s, exit 1.

Capture run, filter `FullyQualifiedName~StoryboardDeviceTests`: Failed 12, Passed 0, Skipped 0, Total 12, Duration 4 m 29 s, exit 1. Ended 2026-09-29T17:57:03Z.

The catalog fact (16 wireframe markdown files and 12 storyboard markdown files, each with at least one SVG link) passed in the earlier test commit. This receipt does not restate that as a new device result.

## Wireframes

One theory case per markdown file. Each case navigates with adb taps when the capture shell has that screen, captures one screenshot, and compares it to the first SVG linked from that file. All 16 ratios are above 0.08. Diff paths are `artifacts/aiunit-device/<id>/<id>-diff.png` on the host. Those full trees are local (174 PNG files, 35519716 bytes) and are not committed. The `WF-01` trio is committed under `docs/receipts/android/aiunit-20260929/`.

| Id | ScreenId on device | Ratio | Result |
| --- | --- | --- | --- |
| WF-01 | WF-01 | 0.9256 | pixel fail; perceptual fail-closed |
| WF-02 | WF-02 | 0.9185 | pixel fail |
| WF-03 | WF-01 | 0.9199 | screen mismatch and pixel fail |
| WF-04 | WF-04 | 0.9260 | pixel fail |
| WF-05 | WF-05 | 0.9215 | pixel fail |
| WF-06 | WF-04 | 0.9258 | screen mismatch and pixel fail |
| WF-07 | WF-04 | 0.9242 | screen mismatch and pixel fail |
| WF-08 | WF-05 | 0.9221 | screen mismatch and pixel fail |
| WF-R-01 | WF-01 | 0.9588 | screen mismatch and pixel fail |
| WF-R-02 | WF-01 | 0.9360 | screen mismatch and pixel fail |
| WF-R-03 | WF-01 | 0.9363 | screen mismatch and pixel fail |
| WF-R-04 | WF-01 | 0.9329 | screen mismatch and pixel fail |
| WF-R-05 | WF-01 | 0.9353 | screen mismatch and pixel fail |
| WF-R-06 | WF-01 | 0.9483 | screen mismatch and pixel fail |
| WF-R-07 | WF-01 | 0.9327 | screen mismatch and pixel fail |
| WF-R-08 | WF-01 | 0.9317 | screen mismatch and pixel fail |

`WF-01` perceptual detail: `timeout: The operation didn't complete within the allowed timeout of '00:01:00'.` The client resolved far enough to start the call. The call did not return JSON. That is a fail-closed result, not a skip, and not a usability AGREE.

`WF-R-*` are desktop review wireframes. The Android capture client does not open them. Launch stays on `WF-01`. `WF-03` has no capture `ScreenId`. `WF-06` and `WF-07` stayed on `WF-04`. `WF-08` stayed on `WF-05` (passenger role, start did not reach the fail-closed screen).

The light SVG mock and the dark Fluent device UI differ on most pixels. A ratio near 0.92 is the expected fail for that pair. It is not a claim that the screenshot is blank. The committed `WF-01-actual.png` shows the dark capture shell: RideAudit header, role prompt, Driver and Passenger buttons, and the production-unavailable status. `WF-01-diff.png` is predominantly red where the light baseline and the dark screenshot disagree. SHA256:

| File | Bytes | SHA256 |
| --- | --- | --- |
| WF-01-actual.png | 263672 | 6ED3EB10A592F058043DD08315F98760236AF69F6909E7901409DD8A69467246 |
| WF-01-baseline.png | 249626 | 99A16ECBE1E5F9648F771AA5D5DB4E0786FF7265124D22199F671E5DF76A2C60 |
| WF-01-diff.png | 105642 | E78DBC425EF10777B0D188100EA9A12E9F239EC3B927E858F692CDC79736BA7C |

Row log: `docs/receipts/android/aiunit-20260929/wireframe-results.jsonl`.

## Storyboards

One theory case per markdown file. The case walks each unique SVG link in document order. Duplicate links in one file are one frame. Each frame force-stops the app, starts it, taps from the screen id in the SVG name, captures a screenshot, and compares. That is adb navigation. It is not a `SharpNinja.Avalonia.RemoteControl` step sequence. A storyboard acceptance criterion that requires RemoteControl driving is not met by this run.

12 storyboards, 42 unique frames, 0 within the pixel threshold, 0 skipped. Five frames had a matching `ScreenId` and still failed the pixel ratio: `SB-01-f01` (`WF-01`, 0.9256), `SB-01-f02` (`WF-02`, 0.9185), `SB-01-f05` (`WF-04`, 0.9260), `SB-02-f01` (`WF-04`, 0.9260), `SB-03-f01` (`WF-05`, 0.9215).

| Id | Unique frames | ScreenId matches | Ratio range |
| --- | --- | --- | --- |
| SB-01 | 5 | 3 | 0.9185 to 0.9260 |
| SB-02 | 4 | 1 | 0.9221 to 0.9260 |
| SB-03 | 3 | 1 | 0.9215 to 0.9258 |
| SB-04 | 3 | 0 | 0.9221 to 0.9258 |
| SB-05 | 3 | 0 | 0.9221 to 0.9588 |
| SB-06 | 7 | 0 | 0.9317 to 0.9588 |
| SB-R-01 | 2 | 0 | 0.9360 to 0.9588 |
| SB-R-02 | 4 | 0 | 0.9327 to 0.9363 |
| SB-R-03 | 2 | 0 | 0.9353 to 0.9483 |
| SB-R-04 | 3 | 0 | 0.9327 to 0.9483 |
| SB-R-05 | 4 | 0 | 0.9317 to 0.9364 |
| SB-R-06 | 2 | 0 | 0.9317 to 0.9327 |

`SB-R-*` frames expected review screen ids and observed `WF-01`. Row log: `docs/receipts/android/aiunit-20260929/storyboard-results.jsonl`.

## Not proven

Pixel agreement, perceptual agreement, RemoteControl storyboard driving, review-app screens on the phone, `WF-03`, `WF-06`, `WF-07`, and `WF-08` as named capture screens, Bluetooth peer discovery, camera frames, Play Integrity, admission, or Play publication.
