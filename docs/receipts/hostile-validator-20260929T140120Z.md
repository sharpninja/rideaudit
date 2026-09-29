# Hostile validator receipt

TimestampUtc: 2026-09-29T14:01:20Z

ValidatorIdentity: GrokSubagentHostile

Workspace: F:\GitHub\rideaudit

Host: PAYTON-LEGION2

Branch: cursor/p11b-lab-self-sign-3902

HEAD: 2935270c6b5551ab0adf38c780d763fa540504b6

HEAD subject: docs(dist): check lab self-sign plan boxes

Parent: 316da46b7332502e892b4fffcceaea784927208a

Parent subject: docs(dist): record lab signing hostile AGREE

WorkClass: MIXED.

Class 2, user-directed lab ops: the lab self-signed certificate and the win-x64 Authenticode canary. Surface C is N/A for that ops action. This review does not FAIL for the absence of a new FR for creating the certificate.

Class 1, project slice: the checkbox gate on the lab win-x64 item and the unsigned linux-x64 item, after the prior hostile AGREE. This review does not treat those boxes as closing full P11b, section 9, Public Trust, or AC-RIDE-222-001.

add-profile: executed yes. Profile file count read: 19. Directory markdown count: 20. Excluded skill port add-profile.grok.md. Files read in full: PROFILE.md, user-payton-byrd.md, accuracy-first-verify-sources.md, approve-before-execute.md, philosophical-dialogue-mode.md, log-decisions-as-conclusions.md, session-turn-title-summary.md, never-skip-explicit-actions.md, adversarial-review-global.md, hv-jsonl-and-session-log.md, bring-the-receipts.md, hostile-on-goal-state.md, hostile-ops-vs-requirements.md, hostile-phase-gates.md, lab-authorization.md, no-attitude-honesty-tell.md, no-python-lab.md, no-shortcuts-precision-over-convenience.md, requirement-change-plan-first.md.

AccuracyScore: 99

CompletenessScore: 99

OverallVerdict: AGREE

FAIL list: none

UNKNOWN list: none

This review did not edit the plan and did not commit.

MCP sessionlog tools were not in this subagent catalog. mcpserver-box namespaceStatus was error and its tool list was empty. Parent turn req-20260929T135506Z-prompt-905f was already active. That gap is recorded here. It is not an unevaluated implementer claim. Request and response jsonl for this review are under docs/receipts/hv/.

Signtool evidence time: 2026-09-29T13:58:39Z.

## A. Requested validation

### A1. Lab win-x64 and linux-x64 plan lines are checked. Commercial OV/IV, Public Trust, section 9 Class C, macOS, and full P11b stay unchecked. Section 9 historical P0, Astra, and Payton boxes stay unchecked. PASS

Working tree matches HEAD for docs/plans/PLAN-RIDEAUDIT-001-implementation.md (git diff --stat HEAD on that file printed no diff).

Current lines:

- Line 1376: `- [x] Lab self-signed Authenticode` for framework-dependent win-x64. Text says Signed but not Public Trust. It cites docs/receipts/distribution/20260929T125539Z-self-signed-desktop-rid-publish.md and docs/receipts/hostile-validator-20260929T135012Z.md.
- Line 1377: `- [x] linux-x64 framework-dependent publish` unsigned. Authenticode does not apply. Same AGREE receipt.
- Line 1378: `- [ ] Commercial OV/IV Authenticode + cloud HSM.`
- Line 1379: `- [ ] Public Trust and a SmartScreen-clean reputation.`
- Line 1380: `- [ ] Section 9 Class C boxes (Astra/Payton plan acceptance).`
- Line 1381: `- [ ] macOS codesign.`
- Line 1382: `- [ ] Full P11b exit` and the sentence full P11b is not closed.
- Lines 1564 through 1567, section 9: P0 documentation repair, Astra READY + AGREE, Payton AGREE, and the P1 skeleton gate. All `[ ]`. The following paragraph says they remain historically unchecked.

### A2. The checked marks were introduced in 2935270, which is a descendant of AGREE commit 316da46. The AGREE commit does not itself check the boxes. PASS

git log -2:

- 2935270c6b5551ab0adf38c780d763fa540504b6 parent 316da46b7332502e892b4fffcceaea784927208a subject docs(dist): check lab self-sign plan boxes. CommitDate 2026-09-29 08:53:35 -0500.
- 316da46b7332502e892b4fffcceaea784927208a parent fa5185683255a8856011a72cad48789e55e3c7bc subject docs(dist): record lab signing hostile AGREE. CommitDate 2026-09-29 08:53:35 -0500.

The two commit clocks are the same second. Order is the parent pointer, not the clock. git merge-base --is-ancestor 316da46 HEAD exited 0.

git show --name-status 316da46 adds only:

- docs/receipts/hostile-validator-20260929T135012Z.md
- docs/receipts/hostile-validator-20260929T135012Z.json
- docs/receipts/hv/20260929T135012Z-lab-self-sign-dash-recheck.request.jsonl
- docs/receipts/hv/20260929T135012Z-lab-self-sign-dash-recheck.response.jsonl

That commit does not touch the plan. git show of the parent plan blob still has `- [ ]` on the lab win-x64 line and the linux-x64 line, and the sentence that each box stays open until a hostile AGREE. The parent receipt blob contains OverallVerdict: AGREE and the sentence that the review did not check those boxes.

git show 2935270 changes only the plan and tests/RideAudit.Client.Tests/TestRide027And028Tests.cs. The plan diff replaces the two `- [ ]` lab lines with `- [x]` and points them at the AGREE receipt. It does not check lines 1378 through 1382 or lines 1564 through 1567. The same commit changes the section 11 lab row from "stays open until a hostile AGREE" to "Closed for the lab path only, after hostile AGREE" and still says Not Class C and not a reproducible public signed release. The honesty test assertion changes from `- [ ] Lab self-signed Authenticode` to `- [x] Lab self-signed Authenticode` and still requires the commercial, section 9 Class C, and P0 lines to stay `[ ]`.

### A3. Added lines of git diff origin/master...HEAD contain zero em dash (U+2014) and zero en dash (U+2013). PASS

git diff --text --output to a temp file against origin/master...HEAD. Merge base and origin/master are both d1ce4d10f3cd041b19cbe7b6bc014bef15610bcc. Diff size 234061 bytes, 1824 lines, 1599 added lines (prefix +, not +++).

UTF-8 character scan of those added lines: em dash count 0, en dash count 0. UTF-8 byte scan of the same added lines: E2 80 94 count 0, E2 80 93 count 0. Temp diff file removed. Pre-existing plan dashes that are not on added lines were not scored.

### A4. Distribution receipt and live exe match thumbprint 98B8942B143D2D788F635530531C1B2DF0EC3C79. signtool verify /pa exits 1. This is not Public Trust. PASS

docs/receipts/distribution/20260929T125539Z-self-signed-desktop-rid-publish.md records SIGNING_THUMBPRINT 98B8942B143D2D788F635530531C1B2DF0EC3C79, SIGNTOOL_VERIFY_PA_EXIT=1, and Signed but not Public Trust. It says full P11b is not closed.

Live file F:\GitHub\rideaudit\artifacts\desktop-publish\win-x64\RideAudit.Client.Desktop.exe exists. Length 170640. Get-AuthenticodeSignature Status=UnknownError. Subject CN=RideAudit Lab Self-Signed. Thumbprint 98B8942B143D2D788F635530531C1B2DF0EC3C79. Status message: a certificate chain processed, but terminated in a root certificate which is not trusted by the trust provider. That status means a signature is present and the root is not trusted. It is not NotSigned.

signtool.exe C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\signtool.exe verify /pa on that exe: Number of errors 1, SignTool Error the chain terminated in an untrusted root, PA_EXIT=1.

Same binary, verify /pa /v: SHA1 hash 98B8942B143D2D788F635530531C1B2DF0EC3C79, issued to and issued by RideAudit Lab Self-Signed, timestamp Tue Sep 29 07:54:43 2026, Number of files successfully Verified: 0, Number of errors: 1, PA_V_EXIT=1.

Signed but not Public Trust. SmartScreen is not claimed clean.

### A5. AC-RIDE-222-001 remains deferred. full P11b is not closed. PASS

docs/Project/Functional-Requirements-Batch.yaml AC-RIDE-222-001 text is Signed/reproducible GPL2 builds tested on Windows, Linux, macOS. isSatisfied false. That yaml is not in the 2935270 name-status.

docs/receipts/ac-coverage/explicit-deferrals.txt line 2 and docs/receipts/ac-coverage/20260928-ledger.md row AC-RIDE-222-001 are status deferred. The reason says the lab thumbprint was used and that signature is not Public Trust and is not this AC.

Plan line 1373 says a lab self-signed win-x64 signature is not the P11b desktop evidence and this revision does not close P11b. Plan line 1382 remains `[ ]`.

## B. Workspace rules

### B1. Honesty. PASS

The two checked lines match the parent AGREE scope: lab win-x64 signed but not Public Trust, and unsigned linux-x64. The check commit does not mark commercial, Public Trust, Class C, macOS, full P11b, or the section 9 historical boxes. The live thumbprint and verify /pa exit match the distribution receipt.

Observation, not a FAIL: Lab_self_signed_receipt_does_not_claim_public_trust_or_full_p11b now locks the lab `[x]` line and still locks commercial, section 9 Class C, and P0 as `[ ]`. It does not assert the linux-x64 `[x]` line. The plan line itself is `[x]`.

### B2. Receipts. PASS

This review re-ran git log -2, git show on both commits, the ancestor test, the added-line dash byte count, Get-AuthenticodeSignature, and signtool verify /pa. It does not treat the prior AGREE prose as proof of the live signature.

### B3. MCP-only storage. PASS

2935270 name-status is the plan and TestRide027And028Tests.cs only. It does not edit todo storage, a session log, or the requirements yaml.

### B4. PowerShell only, no Python. PASS

This gate is a plan checkbox and a C# assertion update. This review used pwsh. It did not invoke python, python3, or py.

### B5. Byrd v4 only on the project slice, and only as applicable. PASS

Class 2 certificate creation is not a Byrd phase. The class 1 slice checks two lab boxes only after parent commit 316da46, whose receipt OverallVerdict is AGREE and which states it did not check the boxes. This review does not FAIL B2 from FR createdAt versus file times. It does not treat the lab boxes as a P11b phase exit.

### B6. Reviewer session log. Not an implementer FAIL

mcpserver-box discovery returned namespaceStatus error and an empty tool list. This subagent did not hand-edit session logs. The parent turn was already active. The durable jsonl paths below hold this review's request and verdict.

## C. Requirements

### C0. Lab cert ops. N/A

Surface C is N/A for the user-directed lab certificate action. Missing a new FR for that action is not a FAIL. No out-of-repo purchase finding is invented. The plan and the distribution receipt say nothing was purchased.

### C1. AC-RIDE-222-001 stays deferred. PASS

isSatisfied false. Ledger and explicit-deferrals.txt say deferred. The honesty test trait is FR-RIDE-049. It has no AC-RIDE-222-001 trait. The check commit does not mark that AC satisfied.

## D. Current plan

### D1. The two lab boxes are supported and were checked after the AGREE commit. PASS

Evidence for the win-x64 box is the distribution receipt plus the live signtool result in A4. Evidence for the linux-x64 box: artifacts\desktop-publish\linux-x64\RideAudit.Client.Desktop exists, magic 7F 45 4C 46, Get-AuthenticodeSignature Status=UnknownError, message says the form is not supported or known by the trust provider, SignerCertificate null. The linux-x64 dll Status=NotSigned and SignerCertificate null. That matches "unsigned" and "Authenticode does not apply."

### D2. The rest of the lab checklist and section 9 stay open. PASS

Lines 1378 through 1382 and 1564 through 1567 are `[ ]`. Section 11 still says the signed reproducible Win/Linux/macOS and full P11b suite is not closed, Public Trust is not closed, and the P11b exit stays open. The lab row is closed for the lab path only.

### D3. Plan DoD for full P11b is not claimed. PASS

Line 1373 and line 1382 keep full P11b open. This AGREE does not close it.

## Scores

Accuracy 99: the checkbox order, the parent AGREE blob, the added-line dash bytes, the live thumbprint, and verify /pa exit 1 were remeasured. The same-second commit clocks are disclosed and do not override the parent pointer.

Completeness 99: surfaces A, B, C, and D were scored. Class 2 surface C is N/A. No briefed claim was left UNKNOWN. Live MCP requirement and sessionlog calls were unavailable. The committed yaml, deferral file, and ledger were read instead. This subagent wrote the jsonl pair because the sessionlog tool was down.

Request jsonl: F:\GitHub\rideaudit\docs\receipts\hv\20260929T140120Z-checkbox-gate.request.jsonl

Response jsonl: F:\GitHub\rideaudit\docs\receipts\hv\20260929T140120Z-checkbox-gate.response.jsonl
