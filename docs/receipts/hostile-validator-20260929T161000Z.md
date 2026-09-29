# Hostile validator receipt

TimestampUtc: 2026-09-29T16:10:00Z
VerificationObservedUtc: 2026-09-29T16:16:16Z
ValidatorIdentity: GrokSubagentHostile
Workspace: F:\GitHub\rideaudit-ui-font
Host: PAYTON-LEGION2
Branch: cursor/dual-phone-fold-moto-8aa2
HEAD: d3a5ca079033338037d6bac4b51cb10090a18b0b
HEAD subject: docs(android): drop unsaved banner claim
Parent: a57f1d3daa0d40d1f8905f895fb512799c47cf7b
ImplementerReceipt: docs/receipts/android/20260929T154510Z-dual-font-both.md
EvidenceDir: docs/receipts/android/dual-font-20260929/
WorkClass: MIXED
Class2Lab: adb install and taps on the already paired Fold 4 and Edge. Surface C is N/A for a new font FR on that redeploy.
Class1Docs: deferral reason and ledger row for AC-UC-025-001. Scored on the AC staying false and deferred.
add-profile: executed yes. Profile file count read: 19. Excluded skill port add-profile.grok.md. This recheck read those 19 files again before scoring.
OverallVerdict: AGREE
AccuracyScore: 100
CompletenessScore: 100
PassCount: 18
FailCount: 0
UnknownCount: 0

This AGREE does not satisfy AC-UC-025-001. It does not check a plan box. It does not authorize a later done-state change by itself. Both scores are at least 98.

Prior verdict hostile-validator-20260929T160000Z was DISAGREE only on B5. That FAIL is closed on this HEAD. The corrected sentence matches the committed XML.

## FAIL list

None.

## UNKNOWN list

None.

## add-profile files read

19 non-skill markdown files, read in full on this recheck: PROFILE.md, user-payton-byrd.md, accuracy-first-verify-sources.md, approve-before-execute.md, philosophical-dialogue-mode.md, log-decisions-as-conclusions.md, session-turn-title-summary.md, never-skip-explicit-actions.md, adversarial-review-global.md, hv-jsonl-and-session-log.md, bring-the-receipts.md, hostile-on-goal-state.md, hostile-ops-vs-requirements.md, hostile-phase-gates.md, lab-authorization.md, no-attitude-honesty-tell.md, no-python-lab.md, no-shortcuts-precision-over-convenience.md, requirement-change-plan-first.md. Skill port add-profile.grok.md was excluded.

## Correction under attack

Commit d3a5ca079033338037d6bac4b51cb10090a18b0b has parent a57f1d3daa0d40d1f8905f895fb512799c47cf7b. `git show --name-status` lists only `docs/receipts/android/20260929T154510Z-dual-font-both.md`.

The on-disk sentence at line 70 is: "The committed Fold start dump replaces that control with the camera and attestation string above. This pass does not include a saved dump of the earlier production unavailable banner. That banner remains in `docs/receipts/android/dual-20260929/fold-01-launch.xml` from the previous session."

`fold-04-start.xml` has one `FailClosedText` node. Text is `CAMERA_UNAVAILABLE: CAMERA_UNAVAILABLE: Android Camera2 frame pipeline is not implemented. Refusing fixture frames. | ATTESTATION_FAILED: Attestation token is missing.` PRODUCTION_UNAVAILABLE is absent. A phrase scan of every xml file in `dual-font-20260929` found zero hits for PRODUCTION_UNAVAILABLE, canonical admission, Play Integrity, and hardware HSM.

`docs/receipts/android/dual-20260929/fold-01-launch.xml` has one `FailClosedText` node. Text starts `PRODUCTION_UNAVAILABLE: UnavailablePlayIntegrityClient: Play Integrity API was not called.` and includes `UnavailableHsmEscrow: hardware HSM is not configured.` and `CanonicalAdmission: Canonical admission requires tenant and driver identity.`

Receipt length 4969. U+2013 count 0. U+2014 count 0.

## A. Requested validation

### A1. Both phones have the stated APK. PASS

Host file `src/RideAudit.Client.Android/bin/Release/net10.0-android/org.rideaudit.app-Signed.apk` re-hashed at this HEAD. Length 47523889. SHA256 67669B76A27139BBB215E8D777D5519650E749182BDF2C9732070A4FEC539869. d3a5ca0 does not change the apk. The prior live pull of both base.apk files matched this digest and the dumpsys lastUpdateTime lines. This commit does not alter that evidence.

### A2. Fold WF-01 heights. PASS

Re-parsed `fold-01-wf01.xml`. TitleText height 115, y 119-234. FrameworkNotice Avalonia UI 12 height 89, y 878-967. ScreenId WF-01 height 89, y 1031-1120. DriverButton height 124, y 1382-1506.

### A3. Edge WF-01 heights. PASS

Re-parsed `moto-01-wf01.xml`. TitleText height 122, y 139-261. FrameworkNotice height 95, y 945-1040. ScreenId WF-01 height 95, y 1107-1202. DriverButton height 132, y 1386-1518.

### A4. Fold driver strings. PASS

fold-02-role.xml ScreenId WF-04 and PairingStatus `Role confirmed: driver coordinator`. fold-03-discover.xml ScreenId WF-02 and PairingStatus `RideAudit Bluetooth discovery on android-ble. No Lyft private API.` fold-04-start.xml ScreenId WF-08, StartButton enabled false, ClockText `Session clock not started`, FailClosedText the camera and attestation string above.

### A5. Edge passenger strings. PASS

moto-02-role.xml ScreenId WF-05 and PairingStatus `Role confirmed: passenger compositor`. moto-03-discover.xml ScreenId WF-02 and the same discovery string. moto-04-screen.xml ScreenId WF-08 and StartButton enabled false. moto-04-fail.xml FailClosedText `Only the driver phone may start the session.` SpiderGraph `Spider graph armed for telematics overlay`. SubmitStatus `No submission`.

### A6. Receipt does not claim a new pair, a BLE peer, Play publication, OTS, Caddy, or HSM closure, and does not mark AC-UC-025-001 satisfied. PASS

Lines 73-81 still deny those closures. `isSatisfied: true` count 0. `adb pair` count 0. `pair succeeded` count 0. The two `satisfied` hits are the line 5 denial and the substring inside `isSatisfied`.

### A7. PNG sizes. PASS

IHDR: four Fold pngs 904x2316. Four Edge pngs 1080x2400.

### A8. U+2013 and U+2014 counts are 0. PASS

New receipt length 4969. Both counts 0. AC-UC-025-001 deferral line both counts 0.

### B5 correction. PASS

The sentence that previously claimed the production banner was in this pass's Fold tree is gone. The replacement sentence matches fold-04-start.xml and the prior fold-01-launch.xml. Prior B5 FAIL is not still open.

## B. Workspace rules

### B1. Byrd v4 on the applicable slice. PASS

Class 2 redeploy stays outside Byrd. The docs commits do not claim a phase complete or a plan checkbox.

### B2. Receipts for the measured claims. PASS

Heights, strings, png sizes, apk hash, banner texts, ledger equality, and yaml isSatisfied were re-read at HEAD d3a5ca0.

### B3. MCP-only requirements storage. PASS

`docs/Project/Use-Cases-Batch.yaml` is not in the diff from a57f1d3 parent through d3a5ca0. isSatisfied stays false.

### B4. PowerShell only, no Python, for this slice. PASS

d3a5ca0 changes one markdown file. No Python file is in the commit.

### B5. Honesty. PASS

The corrected banner sentence matches the two XML files named above. The camera string and the prior PRODUCTION_UNAVAILABLE string were re-read from the nodes.

### B6. Other standing rules. PASS

Host is PAYTON-LEGION2. `.version` is absent from both a57f1d3 and d3a5ca0 (`git ls-tree` count 0). Working tree still shows `?? .version`, which is outside both commits.

## C. Requirements

Class 2 redeploy: N/A. Missing a new font FR is not a FAIL.

### C1. AC-UC-025-001 isSatisfied false. PASS

`docs/Project/Use-Cases-Batch.yaml` lines 675-677: id AC-UC-025-001, isSatisfied false. That file is not in either commit's change set versus a57f1d3 parent.

### C2. Ledger row stays deferred and matches explicit-deferrals.txt. PASS

Status cell `deferred`. Deferral body equals the ledger reason cell. Both length 460. d3a5ca0 does not edit those files.

## D. Plan

### D1. Plan files were not edited by a57f1d3 or d3a5ca0. PASS

`git diff --name-only a57f1d3^ d3a5ca0 -- docs/plans` is empty.

### D2. These commits do not check plan boxes. PASS

Neither commit lists a plan path. The receipt still says AC-UC-025-001 stays deferred. This review did not check a box.

## Score

18 claims scored: A1 A2 A3 A4 A5 A6 A7 A8, B1 B2 B3 B4 B5 B6, C1 C2, D1 D2. 18 PASS. 0 FAIL. 0 UNKNOWN. AccuracyScore 100. CompletenessScore 100. OverallVerdict AGREE.

RequestJsonl: docs/receipts/hv/20260929T161000Z-dual-font-recheck.request.jsonl
ResponseJsonl: docs/receipts/hv/20260929T161000Z-dual-font-recheck.response.jsonl
