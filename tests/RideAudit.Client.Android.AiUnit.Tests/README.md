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

If RemoteControl does not attach, the storyboard fails closed and no frame is compared. An alternate fail-closed branch, a review screen (`WF-R-*`), or any other step that cannot be reached without leaving the session fails that step and is not screenshot-compared. The harness does not press Return to jump back. Hidden pages are not treated as overlapping controls. `ConfirmCheck` is set with RemoteControl `SetProperty` `IsChecked=true`. A click on that checkbox did not leave it checked, and pairing confirm then fail-closed.

Usability runs on every wireframe screenshot and every driven storyboard frame, in addition to the pixel compare. A pixel match does not pass a frame that has a usability defect. Each frame records pass, fail, fail-closed, or not-detectable for:

- `clipped-text`: font taller than the box, or single-line text wider than the arranged width
- `truncated-text`: `TextTrimming` other than None
- `text-overflow`: text bounds outside the parent
- `overlapping-controls`: visible buttons, checks, or text that overlap and are not nested
- `empty-icon`: a square 24 to 80px slot with no mark inside
- `missing-icons`: baseline SVG has icon groups and the live tree has no Path, Image, or Icon node
- `low-contrast`: screenshot sample inside text bounds, 4.5:1 below 18px and 3:1 at 18px or larger, when light and dark pixels separate
- `broken-layout`: screen id differs from the frame, or a button sits outside the tree bounds
- `aiunit-frontier`: codex-subscription compares the baseline and screenshot and must report no usability defects. A timeout is fail-closed

Undriven storyboard steps record those checks as `not-run`. They are not a usability pass. Wireframe cases attach RemoteControl after the single-screen navigation so the tree checks can run. If that attach fails, the tree checks fail closed.

## Threshold

`SharpNinja.aiUnit` 3.0.0 has no pixel-threshold type. Its visual path attaches images to the active frontier strategy. This harness adds a fail-closed pixel gate:

- Scale the rasterized SVG to the screenshot size.
- A pixel mismatches when any RGB channel differs by more than `VisualThreshold.ChannelDelta` (24).
- The frame fails when the differing-pixel ratio is above `VisualThreshold.MaxDifferingPixelRatio` (0.08).
- The diff PNG paints mismatched pixels red. Paths are under `artifacts/aiunit-device/<id>/`.

The codex-subscription call runs for every compared wireframe and every driven storyboard frame. It is the `aiunit-frontier` check. A usability defect from that call fails the test even when the pixel ratio is inside 0.08. A missing client fails closed. It is not a skip.

Review wireframes (`WF-R-*`) are desktop review screens. The Android capture client does not open them. A wireframe case still captures the device and fails closed when `ScreenId` does not match. Review storyboards (`SB-R-*`, and `SB-06`) fail closed because those steps cannot be driven. They are not compared as capture-shell screenshots.

This suite does not claim a visual match and it is not a Play publication.
