# Hostile validator receipt

TimestampUtc: 2026-09-29T13:50:12Z

ValidatorIdentity: GrokSubagentHostile

Workspace: F:\GitHub\rideaudit

Host: PAYTON-LEGION2

Branch: cursor/p11b-lab-self-sign-3902

HEAD: fa5185683255a8856011a72cad48789e55e3c7bc

HEAD subject: docs(dist): drop inventory em dashes

git log -1 confirmed that subject. Short prefix fa51856 matches.

WorkClass: MIXED.

Class 2, user-directed lab ops: the lab self-signed certificate and the win-x64 Authenticode canary. Surface C is N/A for missing FR on that ops action. This review does not FAIL for the absence of a new FR for creating the certificate.

Class 1, project slice: the plan, receipts, deferral text, scripts, manifest, and honesty test. This review does not treat the lab signature as closing full P11b, section 9, Public Trust, or AC-RIDE-222-001.

add-profile: executed yes. Profile file count read: 19. Excluded skill port add-profile.grok.md. Files read in full: PROFILE.md, user-payton-byrd.md, accuracy-first-verify-sources.md, approve-before-execute.md, philosophical-dialogue-mode.md, log-decisions-as-conclusions.md, session-turn-title-summary.md, never-skip-explicit-actions.md, adversarial-review-global.md, hv-jsonl-and-session-log.md, bring-the-receipts.md, hostile-on-goal-state.md, hostile-ops-vs-requirements.md, hostile-phase-gates.md, lab-authorization.md, no-attitude-honesty-tell.md, no-python-lab.md, no-shortcuts-precision-over-convenience.md, requirement-change-plan-first.md.

AccuracyScore: 99

CompletenessScore: 99

OverallVerdict: AGREE

FAIL list: none

UNKNOWN list: none

This AGREE is only the gate that allows a later commit to check the two lab boxes (lab self-signed win-x64, and unsigned linux-x64). This review did not check those boxes, did not edit the plan, and did not commit.

MCP sessionlog tools were not in this subagent catalog. mcpserver-box namespaceStatus was error. Parent turn req-20260929T134340Z-prompt-2e49 was already active. That gap is recorded here. It is not an unevaluated implementer claim.

## A. Requested validation

### A1. HEAD is fa51856 on cursor/p11b-lab-self-sign-3902. PASS

git rev-parse HEAD returned fa5185683255a8856011a72cad48789e55e3c7bc. git branch showed cursor/p11b-lab-self-sign-3902. git log -1 subject was docs(dist): drop inventory em dashes. Merge base with origin/master is d1ce4d10f3cd041b19cbe7b6bc014bef15610bcc, which is also origin/master.

### A2. Added lines of git diff origin/master...HEAD contain zero U+2014 and zero U+2013. PASS

git diff --text origin/master...HEAD was written to a temp file (179122 bytes, 1424 lines). A UTF-8 character scan of added lines (prefix +, not +++) counted em dash (U+2014) added = 0 and en dash (U+2013) added = 0. A separate byte scan of those added lines counted UTF-8 E2 80 94 = 0 and E2 80 93 = 0 across 1224 added lines. The new inventory file docs/receipts/distribution/20260929T124653Z-p11b-signing-inventory.md is 7588 bytes and also contains 0 of each sequence.

Out of scope, not defects: em dash deleted = 1 (old plan revision line removed), em dash context = 2, en dash context = 2. Those sit on context or deleted lines inside docs/plans/PLAN-RIDEAUDIT-001-implementation.md. Pre-existing dashes that are not on added lines are out of scope. A deleted line that removes an old dash is not a new defect.

### A3. Lab certificate, private key, export policy None, no tracked pfx. PASS

Get-Item Cert:\CurrentUser\My\98B8942B143D2D788F635530531C1B2DF0EC3C79:

- Subject CN=RideAudit Lab Self-Signed
- Issuer CN=RideAudit Lab Self-Signed (self-signed)
- Thumbprint 98B8942B143D2D788F635530531C1B2DF0EC3C79
- HasPrivateKey True
- EKU 1.3.6.1.5.5.7.3.3 Code Signing
- RSACng ExportPolicy None
- KeySize 3072
- NotBefore 2026-09-29T07:41:51-05:00
- NotAfter 2029-09-29T07:51:51-05:00
- Exactly one certificate with that subject in CurrentUser\My

git ls-files for pfx and p12 returned no paths. A recursive search under F:\GitHub\rideaudit for pfx and p12 files returned none.

### A4. win-x64 exe signed by that thumbprint, not NotSigned, verify /pa exit 1, timestamp present, not Public Trust. PASS

File F:\GitHub\rideaudit\artifacts\desktop-publish\win-x64\RideAudit.Client.Desktop.exe exists. Length 170640. Magic 4D 5A 90 00. SHA256 DFCF5EDEDF8B51521855E56AB5EABFAC421E2E34758AEE7EA480AE2AFAFD5221. That hash matches the distribution receipt.

Get-AuthenticodeSignature Status=UnknownError. StatusMessage: A certificate chain processed, but terminated in a root certificate which is not trusted by the trust provider. That status is not NotSigned. SignerCertificate.Thumbprint=98B8942B143D2D788F635530531C1B2DF0EC3C79. Signer subject CN=RideAudit Lab Self-Signed. TimeStamperCertificate subject CN=DigiCert SHA256 RSA4096 Timestamp Responder 2026 1. Timestamp thumbprint 51D9ABDA034973D84F4266ACA48248E6B369C439.

signtool.exe at C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\signtool.exe verify /pa /v exited 1. Stdout shows Signature Index 0, SHA1 hash 98B8942B143D2D788F635530531C1B2DF0EC3C79, issued to and by RideAudit Lab Self-Signed, and "The signature is timestamped: Tue Sep 29 07:54:43 2026". Stderr: SignTool Error: A certificate chain processed, but terminated in a root certificate which is not trusted by the trust provider. Number of files successfully Verified: 0. Number of errors: 1.

Signed but not Public Trust. The win-x64 dll is NotSigned. The receipt says the exe is the canary.

### A5. linux-x64 unsigned. Publish script does not publish macOS. PASS

artifacts\desktop-publish directories: linux-x64 and win-x64 only. osx-arm64, osx-x64, and osx do not exist. linux-x64\RideAudit.Client.Desktop.exe is absent.

linux-x64\RideAudit.Client.Desktop magic 7F 45 4C 46 (ELF). Get-AuthenticodeSignature Status=UnknownError, message says the form is not supported by the trust provider, SignerCertificate null. SHA256 862760BD037B24E1A6731D273B4C2E5432691280CB5A90AE744A983CFFC7DCC3.

linux-x64\RideAudit.Client.Desktop.dll Status=NotSigned, SignerCertificate null. SHA256 37D088687B777480F6E8192F5B9EF0A1FE91F55C955BC835D61EEE92A4A41781. That hash matches the distribution receipt. win-x64 dll SHA256 2C1F09297C27DC95B3C39A25558D1FDAB6ADD27D2B2816D08E6F7D087C8DAA46 also matches the receipt.

deploy/desktop/Publish-RideAuditDesktopLab.ps1 calls Publish-Rid only for win-x64 (line 117) and linux-x64 (line 118). It signs only the win-x64 exe. macOS appears as "out of scope" and as MACOS=not-published. The script does not contain osx-arm64.

### A6. Blocked thumbprints are not the signer. PASS

Both blocked certificates are present in CurrentUser\My: CN=McpServerManager Dev FD1AC65B183E708D229E3D7A16C0D021CA3EB3C4 and CN=ClaudeMigrator 50ACCEC97BFD3A50A6C2EB7E34F454B2994D1919. The exe signer thumbprint is 98B8942B143D2D788F635530531C1B2DF0EC3C79. signtool showed one primary signature. The publish script refuses those two thumbprints.

### A7. explicit-deferrals.txt AC-RIDE-222-001 says the lab certificate was used and is not that AC. PASS

docs/receipts/ac-coverage/explicit-deferrals.txt line 2 says a lab self-signed win-x64 certificate (CN=RideAudit Lab Self-Signed, thumbprint 98B8942B143D2D788F635530531C1B2DF0EC3C79) was used on PAYTON-LEGION2, that the signature is not Public Trust, and that it is not this AC.

Select-String on that file for the sentence "no RideAudit code-signing certificate was used" returned no hits. The old sentence remains only as a deleted diff line and as quoted history inside older hostile receipts. It is not the live deferral. The ledger row for AC-RIDE-222-001 uses the same current reason and status deferred.

### A8. Plan boxes stay unchecked. The implementer is not claiming the lab boxes done in this commit. PASS

Checked-box search of added plan lines (git diff origin/master...HEAD, pattern added [x]) returned none.

Still open, marker [ ]:

- Line 1376 lab self-signed Authenticode. Text says the box stays open until a hostile AGREE.
- Line 1377 linux-x64 framework-dependent publish, unsigned. Same open-until-AGREE sentence.
- Line 1378 commercial OV/IV Authenticode plus cloud HSM.
- Line 1379 Public Trust and a SmartScreen-clean reputation.
- Line 1380 section 9 Class C boxes. Not closed.
- Line 1381 macOS codesign.
- Line 1382 full P11b exit. Text says full P11b is not closed.
- Lines 1564 through 1567 are the section 9 historical boxes (P0 repair, Astra AGREE, Payton AGREE, then P1). All [ ]. The following paragraph says they remain historically unchecked.

### A9. Manifest windows is not-produced and reproducibleSignedClaim is false. PASS

src/RideAudit.Licensing/Distribution/client-distribution-manifest.json desktopBuilds.windows is not-produced. linux is not-produced-as-signed-release. macos is not-produced. reproducibleSignedClaim is false. The note says a lab self-signed win-x64 file is not Public Trust and macOS was not produced.

### A10. Signer is self-signed and the branch diff has no purchase artifact. PASS

Issuer equals subject on the lab certificate. The script that creates it calls New-SelfSignedCertificate. git diff --name-only origin/master...HEAD has 17 paths: gitignore, two PowerShell scripts, the plan, the ledger, explicit-deferrals.txt, two distribution receipts, four hostile-validator files, four hv jsonl files, the manifest, and TestRide027And028Tests.cs. No invoice, order, pfx, or purchase file. The inventory states nothing was purchased. This review does not invent a finding about purchases outside the repo.

### A11. Honesty test does not close AC-RIDE-222-001, and it passes. PASS

Lab_self_signed_receipt_does_not_claim_public_trust_or_full_p11b has trait FR-RIDE-049 and no AC-RIDE-222-001 trait. It asserts the receipt says full P11b is not closed, verify /pa exit 1, and the plan lines stay [ ].

dotnet test tests/RideAudit.Client.Tests/RideAudit.Client.Tests.csproj -c Release --filter FullyQualifiedName~Lab_self_signed_receipt_does_not_claim_public_trust_or_full_p11b exited 0. Passed 1, Failed 0, Skipped 0, Total 1.

### A12. Lab signature is not full P11b, section 9, Public Trust, or AC-RIDE-222-001. PASS

Plan revision r3.7 says Public Trust, section 9 Class C boxes, and full P11b stay open. Section 11 says the lab checklist box stays open until a hostile AGREE, and that the signature is not Class C and not a reproducible public signed release. Functional-Requirements-Batch.yaml AC-RIDE-222-001 text is "Signed/reproducible GPL2 builds tested on Windows, Linux, macOS." isSatisfied false. That yaml is not in this branch name-status. The distribution receipt says verify /pa does not succeed and full P11b is not closed.

## B. Workspace rules

### B1. Honesty. PASS

Live thumbprint, hashes, Authenticode status, and signtool exit 1 match the distribution receipt and the plan section 11 lab row. The deferral no longer denies that a RideAudit lab certificate was used.

Observation, not a FAIL: the inventory verdict table still says ready to sign win-x64 today is N, and an earlier sentence says publish scripts are not yet wired. The same file addendum says the operator later chose lab self-sign and that the lab certificate was created and used. The addendum is the later statement. The table was not rewritten in place. It does not say the lab signature is Public Trust.

### B2. Byrd v4. PASS for this class split

Class 2 certificate creation is not a Byrd phase. The class 1 slice does not claim a phase complete, a plan exit, or AC-RIDE-222-001 satisfied. This review does not FAIL B2 from FR createdAt versus file times. Inter-phase hostile review is not required to score this non-closure.

### B3. Receipts. PASS

This receipt re-ran cert, hash, Authenticode, signtool, dash byte counts, and the honesty test. It does not accept the prior DISAGREE receipt as proof.

### B4. MCP-only storage. PASS

git diff --name-only origin/master...HEAD does not include todo.yaml, a session log store, or Functional-Requirements-Batch.yaml. No direct TODO or requirements store edit is in the branch delta.

### B5. PowerShell only. PASS

The new lab scripts are PowerShell. Select-String for python in those two scripts returned no hits. This review used PowerShell and dotnet test. It did not use Python.

### B6. No new em dash or en dash on added lines. PASS

Same measurement as A2. Added-line counts are zero for U+2014 and zero for U+2013.

## C. Requirements

### C0. Class 2 lab certificate creation. N/A

Surface C does not FAIL for a missing FR on the operator-directed certificate action.

### C1. AC-RIDE-222-001 stays deferred and the reason is true. PASS

YAML isSatisfied false. Ledger status deferred. The reason names the lab thumbprint and says that signature is not Public Trust and is not this AC. One self-signed win-x64 file is not signed reproducible Windows, Linux, and macOS public trust.

### C2. The honesty test does not close AC-RIDE-222-001. PASS

The test method is a prose guard with trait FR-RIDE-049. It does not set AC-RIDE-222-001 satisfied. The implementer did not claim that AC closed.

## D. Plan

### D1. Lab and linux checklist boxes are still open. PASS

Lines 1376 and 1377 are [ ], not [x]. This commit does not claim those boxes done. This AGREE allows a later commit to check them. This review did not check them.

### D2. Commercial, Public Trust, section 9 Class C, macOS, and full P11b stay open. PASS

Lines 1378 through 1382 are [ ]. The P11b exit paragraph says this revision does not close P11b. Manifest reproducibleSignedClaim is false.

### D3. Section 9 historical boxes stay open. PASS

Lines 1564 through 1567 are [ ]. Closeout text says they stay unchecked and that code-hv-sol-r4 does not check them.

### D4. Holistic DoD. PASS as non-closure

P11b exit still requires signed reproducible Win/Linux/macOS public release, Play and source receipts, and opposing-model AGREE. Those are not met. This verdict does not close them.

## Scores

Accuracy 99: every scoped claim was remeasured against git, the certificate store, Authenticode, signtool, and the honesty test. The historical inventory table is noted and does not contradict the later addendum.

Completeness 99: surfaces A, B, C, and D were scored. Class 2 surface C is N/A. No briefed claim was left UNKNOWN. Live MCP requirement query was unavailable and was not required to read the committed deferral, the yaml projection, or the certificate.
