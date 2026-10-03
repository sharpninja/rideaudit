# Capture screens, controls, layout, and style

Device: Fold 4 USB `RFCW7078MVZ`, model `SM_F936U`. Debug APK `org.rideaudit.app`. Profile `codex-subscription`. Pixel ratios are advisory. `WithinThreshold` was false on every frame below. Frontier timed out at 60s on every frame below and did not return agree or disagree. xUnit failed each case on that timeout.

These rows are the named-click runs. Sealed, OTS stamped, Play Integrity OK, and the clock timestamp are the approved wireframe chrome. They are not live measurements. WF-06 and WF-07 were opened through View seal / submit status and the button labeled Cancel seal. That is not a successful live start, seal, or submit.

## WF-01

Receipt `docs/receipts/android/20260930T004637Z-wf01-reflection.md`. Controls, layout, and style pass. Overlapping controls pass. Low contrast pass (16 regions). The bottom About dock does not overlap the in-screen About on that row.

## WF-02

Latest log row, `actualScreen` `WF-02`:

- controls pass: missing 0, extra 0, wireframe labels 20
- layout pass: 20 shared labels
- style pass: 20 font or color samples
- low contrast pass: 20 regions
- text overflow pass
- differing pixel ratio 0.22995141531783514

## WF-03

Latest log row, `actualScreen` `WF-03`:

- controls pass: missing 0, extra 0, wireframe labels 22
- layout pass: 25 shared labels
- style pass: 25 font or color samples
- low contrast pass: 25 regions
- differing pixel ratio 0.1922963761138368

## WF-04

Read from the log immediately after the run that ended 2026-09-30T01:45:10Z. A later WF-02 and WF-03 run replaced that log file. `actualScreen` `WF-04`:

- controls pass: missing 0, extra 0, wireframe labels 24
- layout pass: 28 shared labels
- style pass: 28 font or color samples
- low contrast fail: contrast 1.55 on `TextBlock:Stop`
- differing pixel ratio 0.27735586990080546

Stop uses the wireframe fill `#93A0AE` on the disabled button. That color is what the style check matched. The contrast fail is reported as measured. It was not darkened to clear the floor.

## WF-05

Same prior log read. `actualScreen` `WF-05`:

- controls pass: missing 0, extra 0, wireframe labels 32
- layout pass: 32 shared labels
- style pass: 32 font or color samples
- low contrast fail: contrast 4.34 on `TextBlock:+Ax`
- differing pixel ratio 0.35012638131046814

`+Ax` uses the wireframe fill `#5C6B7C`. It was not darkened.

## WF-06

Same prior log read. `actualScreen` `WF-06`:

- controls pass: missing 0, extra 0, wireframe labels 27
- layout pass: 28 shared labels
- style pass: 28 font or color samples
- low contrast fail: contrast 2.94 on `TextBlock:4`
- differing pixel ratio 0.27847782643251257

Step number 4 uses the wireframe fill `#8B98A8`. It was not darkened.

## WF-07

Receipt `docs/receipts/android/20260930T011047Z-wf07-reflection.md`. Controls, layout, and style pass. Low contrast pass (21 regions). The bottom About dock is not on that screen.

## WF-08

Not rescored after the remedy font change. The earlier row on the same phone showed the live refusal, not the SVG example `PI_ATTESTATION_FAILED`. The visible body was `CAMERA_UNAVAILABLE: CAMERA_UNAVAILABLE: Android Camera2 frame pipeline is not implemented. Refusing fixture frames. | ATTESTATION_FAILED: Attestation token is missing.` Controls failed for that reason. That live sentence stays.

## Not claimed

Frontier agreement, pixel agreement, storyboard runs, review screens on the phone, the Edge, admission Health, and every FR and AC stay open.
