# Hostile validator receipt

TimestampUtc: 2026-09-29T04:17:46Z

ValidatorIdentity: GrokSubagentHostile

Work class: project implementation scope. RideAudit PLAN-RIDEAUDIT-001 Class A ledger honesty and P11b desktop publish evidence. Byrd v4 applies. P11b is not closed.

add-profile: yes. Profile file count: 19. Read in full from C:\Users\kingd\.claude\profile: PROFILE.md, user-payton-byrd.md, accuracy-first-verify-sources.md, approve-before-execute.md, philosophical-dialogue-mode.md, log-decisions-as-conclusions.md, session-turn-title-summary.md, never-skip-explicit-actions.md, adversarial-review-global.md, hv-jsonl-and-session-log.md, bring-the-receipts.md, hostile-on-goal-state.md, hostile-ops-vs-requirements.md, hostile-phase-gates.md, lab-authorization.md, no-attitude-honesty-tell.md, no-python-lab.md, no-shortcuts-precision-over-convenience.md, requirement-change-plan-first.md. Excluded skill port add-profile.grok.md.

Prior receipt docs/receipts/hostile-validator-20260929T035617Z.md was re-read and not inherited. Its name-first short-circuit and the "only remaining class A gap is P11b" sentence are not the current tree. This verdict is from the files and a Release test run on this tree.

OverallVerdict: DISAGREE

AccuracyScore: 98

CompletenessScore: 96

Completeness is below 98 because the MCP session-log turn was not persisted. A sub-98 score blocks AGREE under the 2026-09-10 approval gate.

## Explicit FAIL list

1. A5. The tests that read the Octopus receipt assert absence of `ghcr.io/` (trailing slash). They do not assert absence of `ghcr.io`. docs/receipts/distribution/20260929T015822Z-octopus-payton-desktop.md line 58 contains `ghcr.io`.
2. B5. Honesty. A5 does not match the receipt file or the assertion that reads it.
3. C1. AC-RIDE-031-001 and AC-RIDE-031-002 are covered live Play and signing ACs because the id string is on tests that assert the live claim is false. Play needles do not match "available on Google Play". The explicit-deferral siblings AC-RIDE-GPL-003-001 and AC-RIDE-GPL-003-002 stay deferred.
4. HV approval gate. CompletenessScore 96 is below 98. OverallVerdict stays DISAGREE.

## Explicit UNKNOWN list

1. B7. MCP session log. Namespace mcpserver-box failed tool discovery. Turn req-20260929T040709Z-prompt-8bce was announced active. The full verdict was not appended. Raw REST was not used.

## A. Requested validation

### A1. Ledger is 403 covered / 21 deferred / 0 missing / 424, and covered+deferred=424. PASS

Working tree docs/receipts/ac-coverage/20260928-ledger.md header rows: covered 403, deferred 21, missing 0, total 424. PowerShell row parse: ROW_COUNT=424, COVERED=403, DEFERRED=21, MISSING=0, SUM=424, DUP=0. Independent classifier replica over the 8 docs/Project/*Batch.yaml files: UNIQUE=424, RE_COVERED=403, RE_DEFERRED=21, RE_MISSING=0, MISMATCH=0 against that working-tree ledger. HEAD still has the older 190/36/198 ledger. The 403 recount is the working tree, and it matches the current ClassifyDeferred.

### A2. Every deferred row comes from the AC's own text, an id prefix, or explicit-deferrals.txt, and a test-source name does not override that. PASS

AcCoverageLedgerTests.ClassifyDeferred reads ExtractAcText (first `text:` field inside 500 characters after the id), then explicit-deferrals.txt, then own-text needles, then id rules. The row builder calls ClassifyDeferred before the test-name check (AcCoverageLedgerTests.cs lines 46-54). A neighboring YAML requirement is not the blob.

Three deferred ids are also present in tests/**/*.cs and stay deferred: AC-RIDE-201-001, AC-RIDE-206-001, AC-RIDE-CHAIN-001-002. DEFERRED_ALSO_COVERED_ROWS=0.

AC-RIDE-CHAIN-001-002 is an exact id constant in that function, not a StartsWith prefix and not an explicit-deferrals.txt row. Its own text is "Tx reference filled on confirmation." The test name does not flip it to covered. The other 20 deferred reasons match own text, an id prefix (AC-RIDE-STORE-003, and AC-RIDE-INGEST-004 when the Concierge/partnership needle does not already fire), or explicit-deferrals.txt. Replica MISMATCH=0.

### A3. AC-RIDE-203-001 is covered, its text is append-only access logs, and the carrying test asserts the log grows and has no Clear/Remove. PASS

Functional-Requirements-Batch.yaml AC-RIDE-203-001 text: "Views/exports of sensitive location and identity write append-only access logs." Ledger status: covered. The text does not contain Concierge or partnership. IngestAnalysisTests.Access_log_is_append_only carries Trait AC-RIDE-203-001, asserts the log count grows by one then two, and Assert.DoesNotContain Clear and Remove on AppendOnlyAccessLog method names. AppendOnlyAccessLog has Append and Entries only. The test does not call an export path. The claim as stated matches the test.

### A4. AC-UC-029-001 is covered by a test that asserts SPDX GPL-2.0-only on the protos and grpc contract authority. PASS

Use-Cases-Batch.yaml text: "Published protos are GPL-2.0 and used as the contract source." Ledger status: covered. ContractAuthorityTests.Protos_publish_gpl_notice_and_grpc_authority has Trait AC-UC-029-001 and asserts "SPDX-License-Identifier: GPL-2.0-only" and "CONTRACT_AUTHORITY: grpc-protobuf" on each file under src/RideAudit.Protos/Protos. Those eight proto files carry both lines. The test reads the in-tree proto sources, not a separate publish drop. That is the assertion the claim describes. Ledger covered is still not semantic closure.

### A5. The five GHCR ids are covered by tests that read the Octopus receipt and assert no ghcr.io, and they are not deferred merely for the word GHCR. FAIL

Covered, and not deferred for the word GHCR: PASS as a status fact. Ledger rows for AC-RIDE-063-003, AC-TEST-038-002, AC-UC-032-001, AC-RIDE-DEPLOY-001-002, and AC-RIDE-DEPLOY-002-002 are covered. ClassifyDeferred has no GHCR needle. Their own text contains GHCR as a prohibition. explicit-deferrals.txt does not list them.

The assertion claim is false. NgrokDeploySecretsTests.Octopus_desktop_receipt_names_instance_and_does_not_claim_ghcr reads docs/receipts/distribution/20260929T015822Z-octopus-payton-desktop.md and calls Assert.DoesNotContain("ghcr.io/", ...). The receipt line 58 says: Absence of a `ghcr.io` repository name. That is the substring ghcr.io without a trailing slash, so this assert does not reject it. IngestAnalysisTests.Octopus_desktop_pointer_is_a_receipt_not_a_live_probe_or_ghcr_row asserts DoesNotContain("ghcr.io") on DistributionReceipts.OctopusDesktopOnFile.Detail, not on the receipt body.

### A6. The 21 deferred ids are not counted covered. PASS

Parsed deferred ids, each status deferred, ID_OVERLAP=0:

- AC-RIDE-004-001
- AC-RIDE-004-002
- AC-RIDE-004-003
- AC-RIDE-012-001
- AC-RIDE-012-002
- AC-RIDE-201-001
- AC-RIDE-205-001
- AC-RIDE-205-002
- AC-RIDE-206-001
- AC-RIDE-222-001
- AC-RIDE-CHAIN-001-002
- AC-RIDE-GPL-003-001
- AC-RIDE-GPL-003-002
- AC-RIDE-INGEST-004-001
- AC-RIDE-INGEST-004-002
- AC-RIDE-SEC-001-001
- AC-RIDE-STORE-003-001
- AC-RIDE-STORE-003-002
- AC-UC-003-001
- AC-UC-003-002
- AC-UC-025-001

### A7. dotnet test RideAudit.sln -c Release was green after the classifier change, with the named totals. PASS

Re-ran `dotnet test F:\GitHub\rideaudit\RideAudit.sln -c Release --nologo --verbosity minimal` on 2026-09-29. Exit 0.

- RideAudit.Host.Windows.Tests Passed 2, Failed 0, Skipped 0
- RideAudit.Protos.Tests Passed 5, Failed 0, Skipped 0
- RideAudit.Workflow.Tests Passed 26, Failed 0, Skipped 0
- RideAudit.Escrow.Tests Passed 4, Failed 0, Skipped 0
- RideAudit.Seal.Tests Passed 8, Failed 0, Skipped 0
- RideAudit.Server.Admission.Tests Passed 28, Failed 0, Skipped 0
- RideAudit.Chain.Tests Passed 16, Failed 0, Skipped 0
- RideAudit.Client.Tests Passed 101, Failed 0, Skipped 0

Suite green is not P11b closure and is not AC satisfaction. The run rewrote the working-tree ledger through File.WriteAllText. Semantic recount stayed 403/21/0/424.

### A8. P11b is not closed. The unsigned publish receipt records the three RID exits and the signing blocker. Section 11 signed-desktop row is A remaining. PASS

docs/receipts/distribution/20260929T033731Z-unsigned-desktop-rid-publish.md records RID=win-x64 EXIT=0, RID=linux-x64 EXIT=0, RID=osx-arm64 EXIT=0, AUTHENTICODE Status=NotSigned, SIGNTOOL_EXIT=1, CODESIGN_ON_PATH=False, and "P11b is not closed."

Re-checked binaries under artifacts/desktop-publish (this validator did not re-run dotnet publish):

- win-x64 RideAudit.Client.Desktop.dll eaced988fe0c6ca08aef1951523e1d5b7ad44d3513d1a1f16a2c4ec68dca3b7a
- win-x64 RideAudit.Client.Desktop.exe d7c62552d725b956280b71d5cad0beaed5d98ee2bd31a6aebaf7a68cd07834ba
- linux-x64 RideAudit.Client.Desktop.dll 0e15f4c1ffcf0623c4687ab41fe9fcfe420f8f69e2862631aa41304c3eac1c87
- osx-arm64 RideAudit.Client.Desktop.dll a613f01406cb6760b131312a42aad2d43915ed2445a11be3159c211d344807c6

Get-AuthenticodeSignature on the win-x64 exe: NotSigned. signtool.exe verify /pa: SignTool Error: No signature found. SIGNTOOL_EXIT=1. Get-Command codesign: absent. publish-log.txt matches the three EXIT=0 lines. Section 11 row "Signed reproducible desktop Win/Linux/macOS and full P11b suite" class is "A remaining" and says "Not closed" and "P11b exit stays open."

### A9. Section 9 boxes stay unchecked. Named Class C rows stay unchecked. PASS

PLAN-RIDEAUDIT-001-implementation.md lines 1554-1557 are all `- [ ]`. Parent plan checkbox grep finds only those four, all unchecked. Android plan lines 215-217 unchecked. Server plan lines 254-256 unchecked. Bracket plan line 125 (optional Astra) and line 127 (HW1) unchecked. Section 11 prose says Play, hardware HSM, live OTS/txid, live L2, physical dual-phone H.264, and Caddy TLS fail closed or stay unchecked. Bracket HW0, link, and HW2 boxes are already `[x]` and are not the Class C items named in this claim.

### A10. Section 11 does not say the only remaining class A gap is P11b. The ledger row class is A remaining. PASS

Search of the parent plan found no "only remaining" and no "The remaining class A product gap". Two section 11 rows are class "A remaining": the signed desktop / P11b row, and the ledger row. The ledger row says a covered row is not whole-AC closure, "P11b still owns that closure", and "This row is not marked done."

## B. Workspace rules

### B1. Byrd v4 phase order is an inter-phase gate. PASS

No phase checkbox was marked complete in this review. P11b was not claimed closed. This review does not FAIL phase order from FR createdAt versus file times.

### B2. Receipts. PASS

Ledger recount, classifier replica, Octopus receipt read, publish re-hash, Authenticode, signtool, and the Release test run are in this receipt.

### B3. MCP-only storage. PASS

git status --short on todo.yaml, TODO.yaml, docs/Project, .mcp, and session-log was empty. Sampled acceptance criteria remain isSatisfied: false, including AC-RIDE-031-001, AC-RIDE-203-001, and AC-UC-029-001. This slice does not show a direct requirements-store edit or an isSatisfied flip.

### B4. PowerShell only, no Python. PASS

This validator used PowerShell. git diff -U0 on AcCoverageLedgerTests.cs, the ac-coverage receipt, and the parent plan had no python, python3, or py -3 hit. The ledger test is C#.

### B5. Honesty. FAIL

See A5 and C1. The ledger header does say a covered row is not semantic closure. That sentence does not make "assert no ghcr.io" true, and it does not keep a live Play availability AC out of the covered count.

### B6. Look-before-delete. PASS

git diff --diff-filter=D --name-only -- src tests docs was empty. This review created a new receipt and did not delete unrelated data.

### B7. HV session log and full verdict persist. UNKNOWN

GetDynamicTools for session, workflow, and mcpserver returned mcpserver-box namespaceStatus error: tool discovery failed. The announced turn was not updated with this verdict. See the UNKNOWN list.

## C. Requirements

### C1. A covered ledger row must not be a live third-party AC counted covered only because its id is in a test. FAIL

AC-RIDE-031-001 text: "Client is available on Google Play and public source repository." isSatisfied: false. Ledger status: covered. Needles are "Play Store", "Play publication", and "published on Google Play". "available on Google Play" matches none of them. TestRide020LayoutTests.Play_published_claim_without_receipt_is_rejected rejects a forged PlayStore.Published=true with a missing receipt. It does not show a Play listing. Sibling AC-RIDE-GPL-003-001 ("Play listing and source repo available.") is deferred from explicit-deferrals.txt.

AC-RIDE-031-002 text: "Build and signing metadata are versioned and reproducible." Ledger status: covered. Distribution_scaffold_is_present_and_does_not_invent_a_play_receipt asserts PlayStore.Published is false and DesktopBuilds.ReproducibleSignedClaim is false. Sibling AC-RIDE-GPL-003-002 is explicit-deferred for the missing RideAudit signing certificate.

Section 11 still says Play Store publication is fail closed and not claimed. Suite green does not satisfy these ACs.

Observed needle gap, not a second verdict: AC-RIDE-214-002 text "HSM/KMS or equivalent..." does not match "hardware HSM", "live HSM", or "HSM hardware", so it stays covered. ProductionCaptureGraph labels "in-process escrow deposit is not hardware HSM." The carrying escrow test does exercise quorum release, so it is not trait-only. The Play pair above is the FAIL.

### C2. Suite green is not AC satisfaction. PASS

The ledger header, every covered reason ("Not semantic closure."), section 11 ("P11b still owns that closure"), and sampled YAML isSatisfied: false all refuse to treat the green Release run as whole-AC closure. A7 passing does not close C1.

## D. Plan

D uses the same evidence as A8, A9, and A10. No extra plan FAIL.

### D1. P11b and the signed desktop row stay open. PASS

Same evidence as A8. Plan status line says P11b is not closed and does not invent a new Astra AGREE.

### D2. Section 9 and the named Class C boxes stay unchecked. PASS

Same evidence as A9.

### D3. Section 11 does not reduce the remaining class A gap to P11b alone. PASS

Same evidence as A10. The prior receipt's D4 sentence is gone. The ledger row class is "A remaining".

## Claim rollup

A: 9 PASS, 1 FAIL (A5), 0 UNKNOWN.

B: 5 PASS, 1 FAIL (B5), 1 UNKNOWN (B7).

C: 1 PASS, 1 FAIL (C1).

D: 3 PASS, 0 FAIL, 0 UNKNOWN. D1-D3 are the plan reading of A8-A10, not a second set of product claims.

Unique requested claims A1-A10 plus B1-B7 plus C1-C2: PASS 15, FAIL 3, UNKNOWN 1.

Approval-gate FAIL sits on top of that rollup: CompletenessScore 96.

Jsonl: docs/receipts/hv/20260929T041746Z-ledger-honesty.request.jsonl and docs/receipts/hv/20260929T041746Z-ledger-honesty.response.jsonl.
