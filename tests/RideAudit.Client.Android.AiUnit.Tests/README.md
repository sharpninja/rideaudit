# RideAudit Android aiUnit visual tests

These tests reference `SharpNinja.aiUnit` 3.0.0 and run on PAYTON-LEGION2 against a connected Android device.

```powershell
dotnet test tests/RideAudit.Client.Android.AiUnit.Tests/RideAudit.Client.Android.AiUnit.Tests.csproj
```

`appsettings.aiunit.json` sets `ActiveStrategy` to `codex-subscription`. Another profile fails `CodexVisualGate.RequireCodexSubscriptionProfile`.

## Device

The runner calls `adb devices -l` and prefers USB serial `RFCW7078MVZ` (SM-F936U). If that phone is not in the `device` state, it uses a `motorola_edge_2024` line. If neither is present, the device tests throw. They do not skip.

## What each test does

Wireframes: one theory case per `docs/ux/**/wireframes/WF-*.md` and `WF-R-*.md`. The case opens the capture client, navigates when the capture shell has that screen, captures `screencap`, and compares it to the SVG linked from the markdown.

Storyboards: one theory case per `docs/ux/**/storyboards/SB-*.md` and `SB-R-*.md`. Each case restarts the capture app once and attaches `SharpNinja.Avalonia.RemoteControl` on `127.0.0.1:47100`. It then drives the beats in markdown order on that same session. Consecutive duplicate SVG links inside one beat collapse to one step. A later repeat of the same screen stays a separate step. After each driven step the harness screenshots and compares that frame. A single static screenshot is not storyboard coverage. The 2026-09-29T19:00:09Z run jumped by screen id, dropped repeated frames, and compared review baselines to the capture shell. That run is not storyboard coverage.

If RemoteControl does not attach, the storyboard fails closed and no frame is compared. An alternate fail-closed branch, a review screen (`WF-R-*`), or any other step that cannot be reached without leaving the session fails that step and is not screenshot-compared. The harness does not press Return to jump back. Hidden pages are not treated as overlapping controls.

Usability uses the RemoteControl tree on driven frames: a visible button overlap, or a visible text block shorter than 10px with more than 12 characters, fails the frame. The codex-subscription call runs for wireframe `WF-01` and for a driven frame that is already inside the pixel ratio. It does not run on an undriven storyboard step. A timeout is a fail-closed result.

## Threshold

`SharpNinja.aiUnit` 3.0.0 has no pixel-threshold type. Its visual path attaches images to the active frontier strategy. This harness adds a fail-closed pixel gate:

- Scale the rasterized SVG to the screenshot size.
- A pixel mismatches when any RGB channel differs by more than `VisualThreshold.ChannelDelta` (24).
- The frame fails when the differing-pixel ratio is above `VisualThreshold.MaxDifferingPixelRatio` (0.08).
- The diff PNG paints mismatched pixels red. Paths are under `artifacts/aiunit-device/<id>/`.

The codex-subscription perceptual call runs for wireframe `WF-01` and for a driven frame that is already inside the pixel ratio. A usability defect from that call fails the test even if the pixel ratio passed. A missing client fails closed. It is not a skip.

Review wireframes (`WF-R-*`) are desktop review screens. The Android capture client does not open them. A wireframe case still captures the device and fails closed when `ScreenId` does not match. Review storyboards (`SB-R-*`, and `SB-06`) fail closed because those steps cannot be driven. They are not compared as capture-shell screenshots.

This suite does not claim a visual match and it is not a Play publication.
