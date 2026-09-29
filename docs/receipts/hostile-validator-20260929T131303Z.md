# Hostile validator receipt

TimestampUtc: 2026-09-29T13:13:03Z

ValidatorIdentity: GrokSubagentHostile

Workspace: F:\GitHub\rideaudit

Branch: cursor/p11b-lab-self-sign-3902

HEAD: 2cfdc0be599c2466820cacf2e938addf024954cc

HEAD subject: docs(dist): add lab self-signed win-x64 path

Host: PAYTON-LEGION2

WorkClass: MIXED.

Class 2, user-directed lab ops: create a dedicated self-signed code-signing certificate on PAYTON-LEGION2 and sign a local framework-dependent win-x64 publish. Surface C is not failed for the absence of a new FR for that ops action.

Class 1, project slice: plan r3.7 lab checklist, distribution receipt, publish scripts, manifest note, and the client test that locks overclaim language. Byrd and requirements are scored only on that slice. The implementer did not claim full P11b, section 9 Class C, Public Trust, AC-RIDE-222-001, or a commercial certificate closed.

add-profile: executed yes. Profile file count read: 19. Excluded skill port add-profile.grok.md. Files read in full: PROFILE.md, user-payton-byrd.md, accuracy-first-verify-sources.md, approve-before-execute.md, philosophical-dialogue-mode.md, log-decisions-as-conclusions.md, session-turn-title-summary.md, never-skip-explicit-actions.md, adversarial-review-global.md, hv-jsonl-and-session-log.md, bring-the-receipts.md, hostile-on-goal-state.md, hostile-ops-vs-requirements.md, hostile-phase-gates.md, lab-authorization.md, no-attitude-honesty-tell.md, no-python-lab.md, no-shortcuts-precision-over-convenience.md, requirement-change-plan-first.md.

OverallVerdict: DISAGREE

AccuracyScore: 98

CompletenessScore: 98

Both scores are at least 98. AGREE is still blocked because A5b is UNKNOWN and B4, B5, B6, and C2 are FAIL.

SessionLog: PASS. sessionId GrokCode-20260929T014118Z-plugin-session (existing verified session; openSession did not replace it). turnRequestId req-20260929T131125Z-hv-lab-self-sign. Local cache F:\GitHub\McpServer\.mcpServer\grok\current-turn.yaml status completed, openedAt 2026-09-29T13:13:03Z, marker F:\GitHub\rideaudit\AGENTS-README-FIRST.yaml. Bootstrap workflow.sessionlog.bootstrap returned initialized true, requestId req-20260929T131204Z-b7a6, deprecated true. appendDialog, appendActions, updateTurn, and completeTurn exited 0. client.SessionLog.QueryAsync agent=GrokCode limit=8 exited 0 and returned 64750 bytes containing that requestId, the title Hostile validate lab self-signed win-x64, the string OverallVerdict: DISAGREE, the lab thumbprint, and the deferral FAIL sentence. workflow.sessionlog.queryHistory returns session summaries: the same session lastUpdated 2026-09-29T13:17:59Z, turnCount 3, tags HV, P11b, lab-self-sign. It does not return turn bodies. The turn body proof is QueryAsync. completeTurn also printed a failsafe drain line: replayed=0 failed=4 quarantined=0 on an older GrokCode pending directory. This turn's body was still present in QueryAsync after that line.

JSON twin: F:\GitHub\rideaudit\docs\receipts\hostile-validator-20260929T131303Z.json

Request jsonl: F:\GitHub\rideaudit\docs\receipts\hv\20260929T131303Z-lab-self-sign.request.jsonl

Response jsonl: F:\GitHub\rideaudit\docs\receipts\hv\20260929T131303Z-lab-self-sign.response.jsonl

Proof turn: req-20260929T132037Z-hv-lab-proof on the same session. A later QueryAsync (103943 bytes) contains that requestId, CompletenessScore: 98, and the request jsonl path.

## Explicit FAIL list

1. B6. Operator profile forbids em-dashes. Commit 2cfdc0be added U+2014 on four added lines: the r3.7 revision header in docs/plans/PLAN-RIDEAUDIT-001-implementation.md, the inventory addendum heading in docs/receipts/distribution/20260929T124653Z-p11b-signing-inventory.md, and two bullets in docs/receipts/distribution/20260929T125539Z-self-signed-desktop-rid-publish.md. Commit diff count of added dash lines: 4. Receipt file em-dash count: 2.
2. B5 and C2. docs/receipts/ac-coverage/explicit-deferrals.txt still says for AC-RIDE-222-001: "no RideAudit code-signing certificate was used." That sentence is now false. A lab certificate was created and used to sign the win-x64 exe. HEAD did not edit that file. Last commit that touched it is d1ce4d1. The AC remains correctly unsatisfied (isSatisfied false; not Win/Linux/macOS reproducible public trust). The reason text was left false.
3. B4. hostile-on-goal-state requires hostile AGREE before a plan checkbox is marked done. docs/plans/PLAN-RIDEAUDIT-001-implementation.md lines 1376 and 1377 are already [x] in commit 2cfdc0be, and that commit is on origin/cursor/p11b-lab-self-sign-3902. No earlier hostile receipt AGREEs this r3.7 lab checkbox.

## Explicit UNKNOWN list

1. A5b. "Nothing was purchased." No vendor invoice, order, or payment ledger was inspected. This is not a proof that a purchase did not happen outside this repo. The signature half is separate and PASS (A5a): the exe signer is the self-signed lab certificate, not a commercial OV/IV certificate.

Session-log persistence was re-checked after the first draft and is PASS under B7. It is not an UNKNOWN.

## A. Requested validation

### A1. Dedicated lab certificate. PASS

Observation on PAYTON-LEGION2, Cert:\CurrentUser\My. Subject count for CN=RideAudit Lab Self-Signed is 1. Thumbprint 98B8942B143D2D788F635530531C1B2DF0EC3C79. Issuer equals subject. HasPrivateKey True. EKU 1.3.6.1.5.5.7.3.3 only. RSACng key length 3072. ExportPolicy name None, integer 0. NotBefore 2026-09-29T07:41:51-05:00. NotAfter 2029-09-29T07:51:51-05:00. Friendly name RideAudit Lab Self-Signed Code Signing. git ls-files of *.pfx and *.p12 is empty. Recursive worktree search for those extensions returned no files. .gitignore in this commit adds *.pfx and *.p12. The create script contains NonExportable and does not call Export-PfxCertificate.

### A2. win-x64 exe signed by that thumbprint, not Public Trust. PASS

File F:\GitHub\rideaudit\artifacts\desktop-publish\win-x64\RideAudit.Client.Desktop.exe length 170640, mtime 2026-09-29T12:54:46.1890445Z. Get-AuthenticodeSignature Status=UnknownError. Status message: A certificate chain processed, but terminated in a root certificate which is not trusted by the trust provider. Signer subject CN=RideAudit Lab Self-Signed. Signer thumbprint 98B8942B143D2D788F635530531C1B2DF0EC3C79. Status is not NotSigned.

signtool.exe C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\signtool.exe verify /pa /v exit 1. Same untrusted-root error. Files successfully verified: 0. Errors: 1. Authenticode content hash sha256 084F2D6DE636D82AC5397EF5D16AA86E99273CB4339168101D9EFD49D7D95118, matching the receipt. Signing chain issued to and by RideAudit Lab Self-Signed, SHA1 98B8942B143D2D788F635530531C1B2DF0EC3C79, expires Sat Sep 29 07:51:51 2029.

Timestamp is present. Get-AuthenticodeSignature TimeStamperCertificate subject CN=DigiCert SHA256 RSA4096 Timestamp Responder 2026 1, O=DigiCert, Inc., C=US. Thumbprint 51D9ABDA034973D84F4266ACA48248E6B369C439. signtool text: The signature is timestamped: Tue Sep 29 07:54:43 2026. Timestamp chain is DigiCert. verify /pa /tw also exits 1 because the signing chain, not the timestamp chain, ends in the untrusted self-signed root.

Signed but not Public Trust is the observed trust-provider result. SmartScreen will warn is an inference from that untrusted-root result. This review did not click a SmartScreen dialog.

### A3. linux-x64 framework-dependent publish exists and is unsigned. macOS was not published by the lab script. PASS

artifacts/desktop-publish contains linux-x64, win-x64, and lab-sign-console.txt. OSX_DIR_COUNT=0 under artifacts. Publish-RideAuditDesktopLab.ps1 publishes win-x64 and linux-x64 only. The test file asserts the publish script does not contain osx-arm64, and that assertion is in the passing test.

linux-x64 file count 68. RideAudit.Client.Desktop.runtimeconfig.json tfm net10.0 framework Microsoft.NETCore.App version 10.0.0. No hostfxr, coreclr, or hostpolicy file, and no .exe. The apphost RideAudit.Client.Desktop starts with ELF magic 7F-45-4C-46. Get-AuthenticodeSignature on that apphost is UnknownError, signer null, message: The form specified for the subject is not one supported or known by the specified trust provider. The linux dll Get-AuthenticodeSignature Status=NotSigned. Authenticode does not apply to that ELF publish. It is unsigned.

### A4. Blocked thumbprints were not used to sign the exe. PASS

Signer thumbprint equals 98B8942B143D2D788F635530531C1B2DF0EC3C79. SIGNER_IS_BLOCKED=False. Both blocked certificates are present in CurrentUser\My and were not the signer: CN=McpServerManager Dev FD1AC65B183E708D229E3D7A16C0D021CA3EB3C4, and CN=ClaudeMigrator 50ACCEC97BFD3A50A6C2EB7E34F454B2994D1919. Both have code-signing EKU and private keys. The publish script refuses those thumbprints. The receipt names them as not used. It does not set SIGNING_THUMBPRINT to either of them.

### A5a. No commercial OV/IV certificate was used. PASS

The signer issuer is CN=RideAudit Lab Self-Signed, equal to the subject. There is no organization field and no public CA in the signing chain. DigiCert appears only as the timestamp authority. The commercial OV/IV plan checkbox remains unchecked.

### A5b. Nothing was purchased. UNKNOWN

See the UNKNOWN list. The self-signed signer shows this signature did not use a purchased OV/IV certificate. It does not prove that no purchase occurred.

### A6. Lab checkbox checked. Commercial, Public Trust, section 9 Class C, macOS, and full P11b checkboxes unchecked. Manifest not-produced and reproducibleSignedClaim false. PASS

Plan lines 1376-1382, read from disk, not from the implementer chat:

- [x] Lab self-signed Authenticode for framework-dependent win-x64. Evidence for that box is A1 and A2.
- [x] linux-x64 framework-dependent publish, unsigned. Evidence is A3. Claim 6 named the lab box. The linux box is also checked, and the files support it.
- [ ] Commercial OV/IV Authenticode + cloud HSM.
- [ ] Public Trust and a SmartScreen-clean reputation.
- [ ] Section 9 Class C boxes.
- [ ] macOS codesign.
- [ ] Full P11b exit.

Section 9 lines 1564-1567 are all [ ]. The following paragraph says they remain historically unchecked. Section 11 says the lab path is closed for the lab path only, commercial stays deferred, and signed reproducible Win/Linux/macOS plus full P11b stay not closed. Android and Server agreement boxes checked in this review are still [ ].

client-distribution-manifest.json desktopBuilds.windows is not-produced. macos is not-produced. reproducibleSignedClaim is false. The note says a lab self-signed win-x64 file is not Public Trust and macOS was not produced.

AC-RIDE-222-001 in Functional-Requirements-Batch.yaml is isSatisfied false. The receipt says that AC stays deferred. This review does not treat the lab signature as whole-AC closure.

The [x] marks match the binaries. They were written before this hostile review. That order is B4 FAIL, not a false description of the binaries.

### A7. TestRide020LayoutTests and TestRide028ViewerTests passed 20/20. PASS

Re-run on this host, not the implementer sentence:

dotnet test tests/RideAudit.Client.Tests/RideAudit.Client.Tests.csproj -c Release --filter FullyQualifiedName~TestRide020LayoutTests|FullyQualifiedName~TestRide028ViewerTests

Passed. Failed: 0, Passed: 20, Skipped: 0, Total: 20, Duration: 767 ms. Exit code 0.

Source count of [Fact] methods is 7 in TestRide020LayoutTests and 13 in TestRide028ViewerTests, summing to 20.

The new fact Lab_self_signed_receipt_does_not_claim_public_trust_or_full_p11b locks committed receipt and plan phrases. It does not read the gitignored exe and does not assert SHA256 DFCF5EDEDF8B51521855E56AB5EABFAC421E2E34758AEE7EA480AE2AFAFD5221. That is a limit of the unit test, not a failed test run. The hash is checked in A8 against the live file.

### A8. Receipt thumbprint, verify exit, and hashes match the live exe. PASS

Live whole-file SHA256 of the exe: DFCF5EDEDF8B51521855E56AB5EABFAC421E2E34758AEE7EA480AE2AFAFD5221. Receipt line records the same value.

Live win-x64 dll SHA256: 2C1F09297C27DC95B3C39A25558D1FDAB6ADD27D2B2816D08E6F7D087C8DAA46. Receipt matches. Win dll Authenticode Status=NotSigned, matching the receipt "no" cell.

Live linux-x64 dll SHA256: 37D088687B777480F6E8192F5B9EF0A1FE91F55C955BC835D61EEE92A4A41781. Receipt matches.

Receipt SIGNING_THUMBPRINT=98B8942B143D2D788F635530531C1B2DF0EC3C79 matches the live signer. SIGNTOOL_VERIFY_PA_EXIT=1 matches the live signtool exit. AUTHENTICODE status text UnknownError matches.

## B. Workspace rules

### B1. Byrd v4 phase order. PASS (not applied to the ops action; not reconstructed from timestamps)

The lab cert creation is class 2. Phase order is not scored by comparing FR createdAt to file times. The project slice does not claim a Byrd phase exit or full P11b exit. P11b exit criteria in the plan still require signed reproducible Windows, Linux, and macOS builds, Play and source receipts, and opposing-model AGREE. Those boxes stay open.

### B2. Receipts. PASS for the signing facts that were re-run

The distribution receipt's signer, verify exit, timestamp, and three hashes were re-measured on the live files. This hostile receipt records those commands. The distribution receipt is not trusted as proof by itself.

### B3. MCP-only storage. PASS

Commit 2cfdc0be does not edit TODO storage, session logs, or the requirements YAML store. diff-tree names only .gitignore, the two deploy scripts, the plan, two distribution receipts, the manifest, and TestRide027And028Tests.cs.

### B4. Goal state before hostile AGREE. FAIL

See the FAIL list. The lab and linux checkboxes are already [x] on origin without a prior hostile AGREE for this slice.

### B5. Honesty of the deferral sentence. FAIL

See the FAIL list. The live certificate contradicts explicit-deferrals.txt.

### B6. No em-dash. FAIL

See the FAIL list. Profile rule: no em-dashes or en-dashes except numeric ranges. This commit added em-dashes. The two scripts, the manifest, and the test file have em count 0 and en count 0. The plan file still contains older dashes outside this commit's added lines. Those older dashes are not charged as new, but the four added lines are.

### B7. Session log completeness. PASS

See the SessionLog paragraph. QueryAsync returned the completed turn body, including OverallVerdict DISAGREE and the deferral FAIL sentence. The local cache status is completed.

### B8. PowerShell only, no Python. PASS

git grep for python, python3, and a py word in the two deploy scripts exited 1 (no matches). Lab automation in this slice is PowerShell. This review used pwsh.exe -NoProfile -NonInteractive. No Python was used to verify.

### B9. Look-before-delete. PASS

Publish-RideAuditDesktopLab.ps1 removes only artifacts/desktop-publish/<rid> before republishing that rid. This review did not delete repo data. No contradictory delete of data this review did not create was observed.

## C. Requirements (project slice only)

### C1. AC-RIDE-222-001 was not closed by the lab signature. PASS

FR-RIDE-222 status pending. AC-RIDE-222-001 text: "Signed/reproducible GPL2 builds tested on Windows, Linux, macOS." isSatisfied false. Ledger and explicit-deferrals.txt still list it deferred. The distribution receipt says the AC stays deferred. The plan full-P11b checkbox is unchecked. Class C for the ops action is N/A: no new FR is required for creating the lab certificate. This PASS is the non-closure finding. It is not a claim that the AC is satisfied.

### C2. Deferral reason text is false after the lab certificate was used. FAIL

Same defect as B5. The requirements projection says no RideAudit code-signing certificate was used. One was used for the lab canary. The AC is still not satisfied, because one self-signed win-x64 file is not signed reproducible Windows, Linux, and macOS public trust. Leaving the old sentence in place is a false requirement record.

The new test method has Trait FR-RIDE-049 and no AC trait. FR-RIDE-049 ACs stay isSatisfied false. The test is a prose guard. It is not offered as AC-RIDE-049-001 or AC-RIDE-222-001 satisfaction. That missing AC trait is not a false closure.

## D. Current plan

### D1. Checked lab boxes match evidence. PASS

The two [x] boxes describe a lab self-signed win-x64 signature that is not Public Trust, and an unsigned framework-dependent linux-x64 publish. A1, A2, and A3 support those descriptions. Section 11's lab row says closed for the lab path only, names the same thumbprint, and says verify /pa exit 1. That matches the live exe.

### D2. Open boxes were not falsely checked. PASS

Commercial OV/IV, Public Trust, section 9 Class C, macOS codesign, and full P11b are [ ]. Section 9 historical boxes are [ ]. The manifest does not claim a reproducible signed release. The implementer did not check those boxes.

### D3. Checking the lab box is not P11b exit. PASS as a scope statement, already failed on order by B4

The plan text says the r3.7 checklist is not the P11b exit and that a lab self-signed win-x64 signature is not the signed reproducible three-OS evidence. That sentence matches the open full-P11b checkbox. B4 still fails the order in which the lab box was checked.

## Scores

AccuracyScore 98: each PASS, FAIL, and UNKNOWN above is tied to a file read or a command re-run on this host. The SmartScreen sentence is marked as inference. The purchase sentence is UNKNOWN rather than asserted.

CompletenessScore 98: surfaces A, B, C, and D were scored. The purchase negative was not converted into a fake PASS. The completed turn body was read back from client.SessionLog.QueryAsync.

OverallVerdict DISAGREE. AGREE would require every applicable claim on A, B, C, and D to PASS. A5b is UNKNOWN. B4, B5, B6, and C2 are FAIL. Scores at 98 do not erase those findings.
