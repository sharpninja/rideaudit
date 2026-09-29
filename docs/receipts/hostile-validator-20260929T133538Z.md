# Hostile validator receipt

TimestampUtc: 2026-09-29T13:35:38Z

ValidatorIdentity: GrokSubagentHostile

Workspace: F:\GitHub\rideaudit

Host: PAYTON-LEGION2

Branch: cursor/p11b-lab-self-sign-3902

HEAD: 4cc3ccd33963b3da201e6fbf9d22c7f64ecfee00

HEAD subject: docs(dist): correct lab signing honesty gaps

WorkClass: MIXED.

Class 2, user-directed lab ops: a self-signed code-signing certificate on PAYTON-LEGION2 was used to sign a local framework-dependent win-x64 publish. Surface C is N/A for that ops action. This review does not FAIL for the absence of a new FR for creating the certificate.

Class 1, project slice: plan text, distribution receipt, deferral text, scripts, manifest note, and the client honesty test. The implementer does not claim full P11b, section 9 Class C, Public Trust, AC-RIDE-222-001, or a commercial certificate closed.

add-profile: executed yes. Profile file count read: 19. Excluded skill port add-profile.grok.md. Files read in full: PROFILE.md, user-payton-byrd.md, accuracy-first-verify-sources.md, approve-before-execute.md, philosophical-dialogue-mode.md, log-decisions-as-conclusions.md, session-turn-title-summary.md, never-skip-explicit-actions.md, adversarial-review-global.md, hv-jsonl-and-session-log.md, bring-the-receipts.md, hostile-on-goal-state.md, hostile-ops-vs-requirements.md, hostile-phase-gates.md, lab-authorization.md, no-attitude-honesty-tell.md, no-python-lab.md, no-shortcuts-precision-over-convenience.md, requirement-change-plan-first.md.

Prior receipt docs/receipts/hostile-validator-20260929T131303Z.md was not trusted. Its FAIL list was re-checked against current HEAD.

AccuracyScore: 98

CompletenessScore: 98

OverallVerdict: DISAGREE

This review did not check plan boxes, did not edit product files, and did not commit or push. dotnet test rewrote nothing: ledger SHA256 before and after the test was 2E61616CCBCFB1E796469B9DCEDF5BC36EC688725EDB2F114BBB758560FA8A97.

## Explicit FAIL list

1. A11 and B6. git diff origin/master...HEAD still adds U+2014 on 10 lines (12 code points) in docs/receipts/distribution/20260929T124653Z-p11b-signing-inventory.md. That file is added by this branch. The dashes are punctuation, not numeric ranges. They are not pre-existing plan text from before the branch. Lines: 16, 18, 28, 41, 43, 64 (two), 65 (two), 74, 77, 83.

## Explicit UNKNOWN list

None. No mandatory implementer surface was left unevaluated.

## A. Requested validation

### A1. HEAD is 4cc3ccd, subject "docs(dist): correct lab signing honesty gaps", branch cursor/p11b-lab-self-sign-3902. PASS

git rev-parse HEAD returned 4cc3ccd33963b3da201e6fbf9d22c7f64ecfee00. git log -1 subject matched. git status --short --branch showed cursor/p11b-lab-self-sign-3902 tracking origin. Tracked worktree was clean. Untracked artifacts from earlier sessions were present and were not part of this commit.

### A2. Lab certificate in Cert:\CurrentUser\My. PASS

Get-Item Cert:\CurrentUser\My\98B8942B143D2D788F635530531C1B2DF0EC3C79 on PAYTON-LEGION2:

- Subject CN=RideAudit Lab Self-Signed
- Issuer CN=RideAudit Lab Self-Signed
- Thumbprint 98B8942B143D2D788F635530531C1B2DF0EC3C79
- NotBefore 2026-09-29T12:41:51Z
- NotAfter 2029-09-29T12:51:51Z
- EnhancedKeyUsageList Code Signing 1.3.6.1.5.5.7.3.3
- HasPrivateKey True
- RSACng KeySize 3072
- ExportPolicy None
- PFX export threw: Key not valid for use in specified state.

### A3. No tracked pfx or p12. PASS

git ls-files for *.pfx and *.p12 returned empty. A recursive search under F:\GitHub\rideaudit for those extensions returned none.

### A4. win-x64 exe signed by that thumbprint. PASS

File F:\GitHub\rideaudit\artifacts\desktop-publish\win-x64\RideAudit.Client.Desktop.exe exists. Magic 4D 5A 90 00. Length 170640.

Get-AuthenticodeSignature Status=UnknownError. StatusMessage: A certificate chain processed, but terminated in a root certificate which is not trusted by the trust provider. That status is not NotSigned. SignerCertificate.Thumbprint=98B8942B143D2D788F635530531C1B2DF0EC3C79. Signer subject CN=RideAudit Lab Self-Signed. TimeStamperCertificate subject CN=DigiCert SHA256 RSA4096 Timestamp Responder 2026 1. Timestamp thumbprint 51D9ABDA034973D84F4266ACA48248E6B369C439.

signtool verify /pa /v exit code 1. Output includes "The signature is timestamped: Tue Sep 29 07:54:43 2026", SHA1 hash 98B8942B143D2D788F635530531C1B2DF0EC3C79, Authenticode content hash 084F2D6DE636D82AC5397EF5D16AA86E99273CB4339168101D9EFD49D7D95118, Number of errors: 1, untrusted root. Whole-file SHA256 DFCF5EDEDF8B51521855E56AB5EABFAC421E2E34758AEE7EA480AE2AFAFD5221 matches the distribution receipt. Win dll SHA256 2C1F09297C27DC95B3C39A25558D1FDAB6ADD27D2B2816D08E6F7D087C8DAA46 matches. Linux dll SHA256 37D088687B777480F6E8192F5B9EF0A1FE91F55C955BC835D61EEE92A4A41781 matches.

### A5. SignTool path exists. PASS

Test-Path on C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\signtool.exe returned True. That binary produced the verify output above.

### A6. linux-x64 unsigned. macOS not published by the lab script. PASS

artifacts/desktop-publish contains linux-x64 and win-x64 only. No osx directory.

linux-x64\RideAudit.Client.Desktop magic 7F 45 4C 46 (ELF). Get-AuthenticodeSignature Status=UnknownError, message says the form is not supported by the trust provider, SignerCertificate null.

linux-x64\RideAudit.Client.Desktop.dll Status=NotSigned, SignerCertificate null.

deploy/desktop/Publish-RideAuditDesktopLab.ps1 publishes only win-x64 and linux-x64, signs only the win-x64 exe, and throws if linux-x64 produces a Windows exe. The script text does not contain osx-arm64.

### A7. Blocked thumbprints are not the signer. PASS

Both blocked certificates are present in CurrentUser\My (CN=McpServerManager Dev FD1AC65B183E708D229E3D7A16C0D021CA3EB3C4 and CN=ClaudeMigrator 50ACCEC97BFD3A50A6C2EB7E34F454B2994D1919). The exe signer thumbprint is 98B8942B143D2D788F635530531C1B2DF0EC3C79. signtool showed one primary signature. The publish script refuses those two thumbprints.

### A8. Deferral and ledger no longer deny that a RideAudit certificate was used. PASS

docs/receipts/ac-coverage/explicit-deferrals.txt line 2 for AC-RIDE-222-001 says a lab self-signed win-x64 certificate (CN=RideAudit Lab Self-Signed, thumbprint 98B8942B143D2D788F635530531C1B2DF0EC3C79) was used on PAYTON-LEGION2, that the signature is not Public Trust, and that it is not this AC.

docs/receipts/ac-coverage/20260928-ledger.md row AC-RIDE-222-001 is status deferred with the same reason. A repo search for the old sentence "no RideAudit code-signing certificate was used" hits only the prior hostile receipt, which quotes the old defect. It is not the live deferral.

### A9. Plan lab and linux boxes are unchecked. Commercial, Public Trust, section 9 Class C, macOS, and full P11b are unchecked. Section 9 historical boxes stay unchecked. PASS

docs/plans/PLAN-RIDEAUDIT-001-implementation.md at HEAD:

- Line 1376: `- [ ] Lab self-signed Authenticode` and "This box stays open until a hostile AGREE."
- Line 1377: `- [ ] linux-x64 framework-dependent publish` and the same open-until-AGREE sentence.
- Line 1378: `- [ ] Commercial OV/IV Authenticode + cloud HSM.`
- Line 1379: `- [ ] Public Trust and a SmartScreen-clean reputation.`
- Line 1380: `- [ ] Section 9 Class C boxes`
- Line 1381: `- [ ] macOS codesign.`
- Line 1382: `- [ ] Full P11b exit`
- Lines 1564-1567: section 9 boxes are `- [ ]`. Line 1569 says they remain historically unchecked.

git diff added `[x]` lines only inside the prior hostile receipt, which is quoting the earlier commit. Current plan text does not mark these goals done. This review does not FAIL B4 for the earlier commit's checked boxes.

### A10. Manifest windows not-produced and reproducibleSignedClaim false. PASS

src/RideAudit.Licensing/Distribution/client-distribution-manifest.json desktopBuilds.windows is not-produced. linux is not-produced-as-signed-release. macos is not-produced. reproducibleSignedClaim is false. The note says a lab self-signed win-x64 file is not Public Trust and macOS was not produced.

### A11. This branch adds no U+2014 or U+2013 on added lines, except numeric ranges. FAIL

pwsh scanned every added line of git diff --unified=0 origin/master...HEAD (937 diff lines). U+2013 count on added lines: 0. U+2014 hits: 10 lines, all in the new file docs/receipts/distribution/20260929T124653Z-p11b-signing-inventory.md. Two of those lines contain two U+2014 characters (lines 64 and 65), so 12 code points. Examples: line 16 "Deferred by operator" plus U+2014; line 43 "Cert store (READ ONLY)" plus U+2014; line 74 "Next steps" plus U+2014. None are numeric ranges. The self-signed receipt, both deploy scripts, explicit-deferrals.txt, the manifest, and TestRide027And028Tests.cs have no U+2014 or U+2013. Pre-existing plan dashes outside this branch's added lines were not scored.

### A12. Lab_self_signed_receipt and Writes_honest_ledger. PASS

dotnet test tests/RideAudit.Client.Tests/RideAudit.Client.Tests.csproj --filter "FullyQualifiedName~Lab_self_signed_receipt|FullyQualifiedName~Writes_honest_ledger" exited 0. Result: Passed 2, Failed 0, Skipped 0, Total 2, Duration 130 ms. Ledger file hash was unchanged.

### A13. Full P11b, section 9 Class C, Public Trust, AC-RIDE-222-001, and a commercial certificate are not claimed closed. PASS

Plan line 1373 says a lab self-signed win-x64 signature is not the P11b desktop evidence and "This revision does not close P11b." Section 11 lab row says the checklist box stays open until a hostile AGREE. Functional-Requirements-Batch.yaml AC-RIDE-222-001 isSatisfied false (that yaml is not in this branch's name-status). The distribution receipt says verify /pa does not succeed and full P11b is not closed.

Observation, not a separate FAIL: the inventory verdict table line 28 still says "Ready to sign win-x64 today? N" and "provision OV + cloud HSM first." The same file's addendum at lines 129-135 says the operator later chose lab self-sign and that the lab certificate was created and used. The addendum is the later statement. The table was not revised in place.

### A14. Scoped purchase claim. PASS

The signed exe's certificate is the lab self-signed certificate in A2 and A4. git diff origin/master...HEAD contains the words purchase, purchased, enroll, enrollment, invoice, and payment only as denials, as a deferred obtain-path, or inside the prior hostile receipt. Inventory line 19: "Purchase this turn | No". Inventory line 7 and the self-signed receipt: "Nothing purchased" / "Nothing was purchased". Inventory addendum: "was not purchased." No invoice number, order id, payment record, or commercial certificate thumbprint is the signer. No pfx was added. This PASS is that scoped claim only.

## B. Workspace rules

### B1. Byrd v4 phase-order. PASS

Class 2 certificate creation is not a Byrd phase. The class 1 slice does not claim a phase complete, a plan exit, or AC-RIDE-222-001 satisfied. This review does not FAIL B2 from FR createdAt versus file times.

### B2. Receipts match the live artifacts. PASS

Certificate fields, Authenticode status, signtool exit 1, timestamp, and the three whole-file SHA256 values in docs/receipts/distribution/20260929T125539Z-self-signed-desktop-rid-publish.md were re-read from the live cert and the live files. They match.

### B3. MCP-only storage. PASS

This branch's name-status does not include todo.yaml, a session log, or Functional-Requirements-Batch.yaml. AC-RIDE-222-001 remains isSatisfied false. The edited ledger is the projection Writes_honest_ledger rewrites from explicit-deferrals.txt. The test rewrite matched the committed bytes.

### B4. Plan boxes stay open until a hostile AGREE. PASS

Current HEAD marks the lab box and the linux box `- [ ]`. The plan text says both stay open until a hostile AGREE. Section 11 repeats that. This is not a goal-done claim. The earlier commit's `[x]` state is not the current tree.

### B5. Deferral text is no longer false. PASS

Same evidence as A8. The live deferral and the ledger row say the lab certificate was used and that the AC stays deferred and is not Public Trust.

### B6. No new em dash or en dash on lines this branch added. FAIL

Same evidence as A11. Operator rule: no em dash or en dash except numeric ranges. Twelve U+2014 characters remain on added lines of the signing inventory.

### B7. PowerShell only. No Python in this lab slice. PASS

Select-String for python, python3, and py in deploy/desktop/*.ps1 returned no matches. Branch name-status adds no .py file. This review used pwsh.exe -NoProfile -NonInteractive. No Python was used.

## C. Requirements

### C-ops. New FR for creating the lab certificate. N/A

Class 2 lab action. Not a FAIL.

### C1. AC-RIDE-222-001 stays deferred and the reason is now true. PASS

YAML text remains "Signed/reproducible GPL2 builds tested on Windows, Linux, macOS." isSatisfied false. Ledger status deferred. The reason names the lab thumbprint and says that signature is not Public Trust and is not this AC. One self-signed win-x64 file is not signed reproducible Windows, Linux, and macOS public trust.

### C2. The honesty test does not close AC-RIDE-222-001. PASS

Lab_self_signed_receipt_does_not_claim_public_trust_or_full_p11b asserts the receipt and the unchecked plan boxes. It is trait FR-RIDE-049. It does not set AC-RIDE-222-001 satisfied. The implementer did not claim that AC closed.

## D. Plan

### D1. Lab checklist is not marked done. PASS

Evidence is on file for the lab signature and the unsigned linux publish. The two boxes stay `- [ ]` until a hostile AGREE. This DISAGREE does not authorize checking them.

### D2. Section 9 historical boxes remain unchecked. PASS

Lines 1564-1567 are `- [ ]`. Section 11 says they stay unchecked.

### D3. Commercial, Public Trust, section 9 Class C, macOS, and full P11b stay open. PASS

Those boxes are `- [ ]`. The P11b exit paragraph says this revision does not close P11b. Manifest reproducibleSignedClaim is false.

## Scores

AccuracyScore 98. Each PASS and the FAIL is tied to a command, a file read, or a diff scan on this host during this run. The inventory table line 28 is recorded as an observation because the addendum in the same file states the later lab choice.

CompletenessScore 98. Surfaces A, B, C, and D were scored. Class 2 cert creation was N/A, not a fake FR FAIL. The purchase claim was scored only inside the scoped diff and the live signer. The prior receipt was re-checked rather than reused.

OverallVerdict is DISAGREE because A11 and B6 FAIL. Scores at or above 98 do not override a FAIL. This receipt does not authorize checking the lab boxes.
