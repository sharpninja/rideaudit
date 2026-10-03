# Fold WF-R chrome and contrast rescore

Host: PAYTON-LEGION2. Device: `RFCW7078MVZ` / `SM_F936U` only. Screenshot 904x2316. Branch `cursor/dual-phone-fold-moto-8aa2`.

Moto Edge was attached over TLS and was not installed, force-stopped, or driven.

## Commits on this push

| SHA | Summary |
| --- | --- |
| `3310c62` | cherry-pick PR #28: RemoteControl Runtime/Protocol 0.8.0, bind port 0, marker |
| `9d1c8f4` | review host chrome (nav/badges/fields/actions); contrast ink/span gate; ReviewCaptureHostTests |
| `7f8e615` | drop CheckBox chrome; tighten contrast span; Codex shim `~/.codex` sandbox fallback |

Installed debug APK built from the chrome host on this branch (`org.rideaudit.app`, `ShellMode.Capture`). Release APK not installed. AvaloniaRemote used for this device score.

## Headless

- `ReviewCaptureHostTests`: Passed 2, Failed 0
- `UsabilityInspectorTests` + `Diluted_short_label` + `RemoteBridgeMarker` + `PhaseBGate`: Passed after fixture update for the span gate

## Fold WF-R wireframe score

Filter: `FullyQualifiedName~WireframeDeviceTests&DisplayName~WF-R`. Ended about 2026-09-30T21:16Z. Duration 8 m 6 s. xUnit Failed 8, Passed 0, Total 8. Jsonl 8 lines under `artifacts/aiunit-device/results.jsonl`.

Frontier JSON returned on every frame (Codex shim found `%USERPROFILE%\.codex\.sandbox-bin\codex.exe`). Pixel `WithinThreshold` false on every frame (advisory).

| Screen | Controls | Layout | Style | Contrast | Overflow | Frontier |
| --- | --- | --- | --- | --- | --- | --- |
| WF-R-01 | pass, 37 | pass, 37 | pass, 37 | fail, Open admitted submission 1.56 | pass | disagree, disagree, disagree |
| WF-R-02 | pass, 43 | pass, 52 | pass, 52 | fail, Decrypt disabled 1.30 | fail, horizontal row | disagree, disagree, disagree |
| WF-R-03 | pass, 38 | pass, 43 | pass, 43 | fail, Record passenger-composite-01 1.56 | pass | disagree, disagree, disagree |
| WF-R-04 | pass, 27 | pass, 28 | pass, 28 | fail, Action blocked 1.56 | pass | disagree, disagree, disagree |
| WF-R-05 | pass, 38 | pass, 39 | pass, 39 | fail, Court release / escrow 1.56 | pass | disagree, disagree, disagree |
| WF-R-06 | pass, 42 | pass, 45 | pass, 45 | pass, 18 | fail, Timeline scrub row | disagree, disagree, disagree |
| WF-R-07 | pass, 42 | pass, 42 | pass, 42 | pass, 21 | fail, receipt value row | disagree, disagree, disagree |
| WF-R-08 | pass, 28 | pass, 32 | pass, 32 | fail, Export disclosure pack 1.56 | pass | disagree, disagree, disagree |

Compared with `20260930T202334Z-review-hosted.md`: local CLS still all pass; WF-R-08 layout now passes (option-row Y invert fixed by dropping horizontal badge+copy packing); contrast fails moved off diluted short-label Case 2.35 onto large titles that still sample ~1.56 (real crop evidence under `artifacts/aiunit-device/WF-R-*/`); frontier now returns JSON instead of PATH/circuit failures, and still disagrees (nav clipped, missing desktop sidebar/cards/icons).

WF-08 capture live `CAMERA_UNAVAILABLE` honesty was not changed. Capture WF-01..WF-08 were not rescored in this run.

## Blockers / open

- Frontier suite agree still open (phone column is not the desktop wireframe chrome).
- Remaining local red: text-overflow on a few same-Y horizontal short rows; title contrast crops at ~1.56 needing crop/mapper proof (floor not lowered).
- Storyboard SB-R / SB-05 / SB-06 not re-run in this pass.
- No FR or AC is satisfied. `isSatisfied` stays false. PLAN-PR24FOLD-001 stays open.
