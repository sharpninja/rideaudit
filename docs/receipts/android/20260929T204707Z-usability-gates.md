# Usability gates on Fold 4 screenshots

TimestampUtc: 2026-09-29T20:47:07Z
Host: PAYTON-LEGION2
SPDX: GPL-2.0-only

Pixel compare and usability checks both ran. A pixel match is not agreement when a usability check fails. This run had no pixel match. Every compared frame also failed at least one usability gate.

The 2026-09-29T19:00:09Z storyboard run jumped by screen id and is not storyboard coverage. The 2026-09-29T19:21:22Z beat-order run drove the storyboards and did not record these per-check gates.

## Harness

`SharpNinja.aiUnit` 3.0.0 has no usability or pixel-threshold type. The aiUnit path is `codex-subscription` (`FrontierAttachment` image/png). `SharpNinja.Avalonia.RemoteControl.Protocol` 0.7.4 supplies the visual tree. Threshold remains channel delta 24 and differing-pixel ratio 0.08.

Each compared frame runs these checks. `fail` and `fail-closed` fail the frame. `not-detectable` and `not-run` do not.

| Check | What it uses |
| --- | --- |
| clipped-text | Tree. Font taller than the box, or non-wrapping text wider than the arranged width. |
| truncated-text | Tree. `TextTrimming` other than `None`. |
| text-overflow | Tree. Text bounds outside the parent by more than 4. |
| overlapping-controls | Tree. Visible button, check, or text bounds intersect by more than 8 on both axes. Ancestors are skipped. |
| empty-icon | Tree. Visible square Border, Image, Path, or Viewbox from 24 to 80 with no mark inside. |
| missing-icons | Baseline SVG `<g transform=` count of 4 or more, and no live Path, Image, or name containing Icon. |
| low-contrast | Screenshot crop of each visible text box. Fail when the 5th-to-95th luminance ratio is below 4.5 (below 3.0 when font size is 18 or more). |
| broken-layout | Tree plus uiautomator. Actual `ScreenId` must match the frame. Buttons must stay inside the tree bounds. |
| aiunit-frontier | Codex image pair. The prompt treats clipped text, missing or empty icons, overlap, overflow, low contrast, and broken layout as defects. A timeout is fail-closed. |

Wireframes navigate to one screen, then attach RemoteControl. Storyboards attach once and drive each markdown beat. Undriven steps record the eight tree checks as `not-run` and do not call `aiunit-frontier`.

## Device

Serial `RFCW7078MVZ`, model `SM_F936U`, screenshots 904 by 2316. Debug APK `src/RideAudit.Client.Android/bin/Debug/net10.0-android/org.rideaudit.app-Signed.apk`, 78753867 bytes, LastWriteTimeUtc 2026-09-29T19:19:14Z. The Release APK was not installed. Edge wireless attach is not claimed.

Command filter: `WireframeDeviceTests|StoryboardDeviceTests`. Failed 28, Passed 0, Skipped 0, Duration 1 h 10 m, ended 2026-09-29T20:47:07Z.

Log rows: 82. Compared frames: 32 (16 wireframes and 16 driven storyboard frames). Undriven storyboard steps: 50. `withinThreshold` count: 0. No 64-hex bridge token is in the log. The word token appears only in the attestation string "Attestation token is missing."

## Check totals

Counts are over 32 compared frames, plus 50 undriven steps for the eight tree checks. Frontier is absent on undriven steps.

| Check | pass | fail | fail-closed | not-run |
| --- | --- | --- | --- | --- |
| clipped-text | 32 | 0 | 0 | 50 |
| truncated-text | 32 | 0 | 0 | 50 |
| text-overflow | 32 | 0 | 0 | 50 |
| overlapping-controls | 0 | 32 | 0 | 50 |
| empty-icon | 22 | 10 | 0 | 50 |
| missing-icons | 2 | 30 | 0 | 50 |
| low-contrast | 11 | 21 | 0 | 50 |
| broken-layout | 20 | 12 | 0 | 50 |
| aiunit-frontier | 0 | 0 | 32 | 0 |

## Compared frames

Status letters: P pass, F fail, C fail-closed. Pixel is the differing-pixel ratio. Every ratio is above 0.08.

| Frame | Actual | Pixel | Clip | Trim | Overflow | Overlap | Empty | Icons | Contrast | Layout | Frontier |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| WF-01 | WF-01 | 0.2741 | P | P | P | F | F | F | F | P | C |
| WF-02 | WF-02 | 0.2448 | P | P | P | F | P | F | F | P | C |
| WF-03 | WF-03 | 0.2196 | P | P | P | F | P | P | P | P | C |
| WF-04 | WF-04 | 0.2883 | P | P | P | F | P | F | P | P | C |
| WF-05 | WF-05 | 0.3129 | P | P | P | F | P | F | F | P | C |
| WF-06 | WF-08 | 0.2615 | P | P | P | F | P | F | P | F | C |
| WF-07 | WF-08 | 0.2469 | P | P | P | F | P | F | P | F | C |
| WF-08 | WF-08 | 0.2550 | P | P | P | F | P | F | P | P | C |
| WF-R-01 | WF-01 | 0.3761 | P | P | P | F | F | F | F | F | C |
| WF-R-02 | WF-01 | 0.1791 | P | P | P | F | F | F | F | F | C |
| WF-R-03 | WF-01 | 0.1949 | P | P | P | F | F | F | F | F | C |
| WF-R-04 | WF-01 | 0.1506 | P | P | P | F | F | F | F | F | C |
| WF-R-05 | WF-01 | 0.1700 | P | P | P | F | F | F | F | F | C |
| WF-R-06 | WF-01 | 0.2913 | P | P | P | F | F | F | F | F | C |
| WF-R-07 | WF-01 | 0.1548 | P | P | P | F | F | F | F | F | C |
| WF-R-08 | WF-01 | 0.1646 | P | P | P | F | F | F | F | F | C |
| SB-01-b01-f01 | WF-01 | 0.2754 | P | P | P | F | F | F | F | P | C |
| SB-01-b02-f02 | WF-02 | 0.2448 | P | P | P | F | P | F | F | P | C |
| SB-01-b03-f04 | WF-03 | 0.2196 | P | P | P | F | P | P | P | P | C |
| SB-01-b04-f06 | WF-04 | 0.2884 | P | P | P | F | P | F | P | P | C |
| SB-02-b01-f01 | WF-04 | 0.2883 | P | P | P | F | P | F | P | P | C |
| SB-02-b02-f02 | WF-04 | 0.2883 | P | P | P | F | P | F | P | P | C |
| SB-02-b03-f04 | WF-08 | 0.2533 | P | P | P | F | P | F | P | F | C |
| SB-03-b01-f01 | WF-05 | 0.3129 | P | P | P | F | P | F | F | P | C |
| SB-03-b02-f02 | WF-05 | 0.3129 | P | P | P | F | P | F | F | P | C |
| SB-03-b03-f03 | WF-05 | 0.3130 | P | P | P | F | P | F | F | P | C |
| SB-03-b04-f04 | WF-05 | 0.3129 | P | P | P | F | P | F | F | P | C |
| SB-04-b01-f01 | WF-08 | 0.2472 | P | P | P | F | P | F | P | F | C |
| SB-05-b01-f01 | WF-07 | 0.2005 | P | P | P | F | P | F | F | P | C |
| SB-05-b02-f02 | WF-07 | 0.2005 | P | P | P | F | P | F | F | P | C |
| SB-05-b03-f04 | WF-07 | 0.2005 | P | P | P | F | P | F | F | P | C |
| SB-05-b04-f05 | WF-07 | 0.2005 | P | P | P | F | P | F | F | P | C |

SB-01 drove WF-01, WF-02, WF-03, then WF-04. Both WF-08 branches were not taken. SB-02 Start landed on WF-08, expected WF-04. SB-03 stayed on WF-05. SB-04 entry landed on WF-08, expected WF-06. SB-05 Submit opened WF-07 after start had already fail-closed. That is not a successful seal. SB-06 and every SB-R storyboard step was not driven because the review screen is not on the Android capture client. Those 50 steps were not screenshot-compared.

## Findings and evidence

Overlapping controls failed on all 32 compared frames. The tree pair is `ScreenId` against the page title, for example `ScreenId:WF-01 overlaps TextBlock:RideAudit`. `ScreenId` is a `TextBlock` in the same grid cell as the page, so its arranged bounds cover the page and intersect the title. That is the recorded tree snippet. The same pattern is `ScreenId:WF-04 overlaps TextBlock:Driver dashboard` and `ScreenId:WF-08 overlaps TextBlock:NOT ADMISSIBLE`.

Empty icon failed on the 10 frames whose live screen was WF-01 (WF-01, eight WF-R wireframes, and SB-01 beat 1). Evidence: `Border: 64x64`. The role screen draws a 64 by 64 border and no path inside it. The WF-01 screenshot shows that empty navy circle where the wireframe draws a shield.

Missing icons failed on 30 compared frames. The live tree has no Path, Image, or Icon-named node while the baseline SVG has 11 or more `<g transform=` groups. WF-03 and SB-01 beat 3 passed this one check because the tree reported one Icon-named node against 13 baseline groups. Those two frames still failed overlap, pixel ratio, and the frontier timeout.

Low contrast failed on 21 compared frames. On WF-01 the sampled node text is `VA-1042`, ratio 1.11, below 4.5. The crop is 692 by 50 and is almost entirely light pixels (5th percentile channel luminance about 242, 95th percentile 255). The gate failed that crop. Other contrast fails use the same rule: WF-02 ratio 2.72 on `Invite code: 7K2M`; WF-05 and SB-03 ratio 1.08 on `ScreenId`; SB-05 ratio 1.08 on `ScreenId`. Eleven frames passed, including WF-03, WF-04, WF-06, WF-07, WF-08, SB-01 beat 4, SB-02, and SB-04.

Broken layout failed when the uiautomator screen id differed from the frame: every WF-R wireframe stayed on WF-01; WF-06 and WF-07 navigation stayed on WF-08; SB-02 Start and SB-04 entry stayed on WF-08. Clipped text, trimmed text, and text overflow passed on all 32 compared frames.

`aiunit-frontier` was fail-closed on all 32 compared frames. Each call hit the 60 second `codex-subscription` timeout. A timeout is not a skip and is not a match.

## Sample files

WF-01 wireframe, role screen, empty icon and contrast crop:

| File | Bytes | SHA256 |
| --- | --- | --- |
| WF-01-actual.png | 156719 | 35EF21C2AF89649550CFE767989D50515885D154C38A1D8DD080D67A1602D625 |
| WF-01-diff.png | 66507 | C70C007FAC5F97CB47922E230EF3FFDAC14EF143A2BF2851AC796BF4637BB429 |
| WF-01-contrast-text.png | 4698 | B067836E0855A073D67CCEC293C9034ADF533EF67FC231667410EAAAA804C572 |

SB-01 beat 4, driven through RemoteControl to WF-04. Pixel ratio 0.2884. Overlap and missing icons failed. Contrast and layout passed. Frontier timed out.

| File | Bytes | SHA256 |
| --- | --- | --- |
| SB-01-b04-f06-actual.png | 179210 | 90DDF5E5C9B149ABCA5AE19D6B357C6B9967A9443013E36C2DA70D0B7517C944 |
| SB-01-b04-f06-diff.png | 63314 | DE22D799B15C37D8D4E4DEA7092D16B830FFCD624BA9021E87B20E3B66B3C2D1 |

Log: `docs/receipts/android/aiunit-usability-20260929/results.jsonl` (144763 bytes, SHA256 5C880010375D93BBFF4B6C8FFD64587729BF7C16E10C68D29CD8F62964668599).

## Not closed

Pixel agreement, perceptual agreement, usability agreement, review screens on the phone, seal and submit as successful live captures, AC-UC-025-001, FR-RIDE-073, UC-RIDE-043, TR-RIDE-VIDEO-017, TEST-RIDE-054, storyboard acceptance criteria, and plan section 9. `isSatisfied` stays false. This is not a visual match, a Play publication, or an OTS, Caddy, or hardware HSM claim.
