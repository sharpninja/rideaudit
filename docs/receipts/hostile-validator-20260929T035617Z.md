# Hostile validator receipt

TimestampUtc: 2026-09-29T03:56:17Z
ValidatorIdentity: GrokSubagentHostile
Workspace: F:\GitHub\rideaudit
Branch: cursor/class-a-ac-coverage-55ce
HEAD: eeede94d1464bf974f7d3f544980a6b82f2bb576
WorkClass: project implementation scope (PLAN-RIDEAUDIT-001 Class A ledger honesty and P11b desktop publish evidence). Byrd v4 applies to the product, test, and docs slice. P11b is not closed.
add-profile: executed yes. Profile file count read: 19. Excluded skill port add-profile.grok.md. Files: PROFILE.md, user-payton-byrd.md, accuracy-first-verify-sources.md, approve-before-execute.md, philosophical-dialogue-mode.md, log-decisions-as-conclusions.md, session-turn-title-summary.md, never-skip-explicit-actions.md, adversarial-review-global.md, hv-jsonl-and-session-log.md, bring-the-receipts.md, hostile-on-goal-state.md, hostile-ops-vs-requirements.md, hostile-phase-gates.md, lab-authorization.md, no-attitude-honesty-tell.md, no-python-lab.md, no-shortcuts-precision-over-convenience.md, requirement-change-plan-first.md.

AccuracyScore: 98
CompletenessScore: 96
OverallVerdict: DISAGREE

Completeness is below 98 because the MCP session-log turn was not persisted. A sub-98 score blocks AGREE by the 2026-09-10 approval gate even before the claim FAILs below.

Request jsonl: F:\GitHub\rideaudit\docs\receipts\hv\20260929T035617Z-class-a-ledger.request.jsonl
Response jsonl: F:\GitHub\rideaudit\docs\receipts\hv\20260929T035617Z-class-a-ledger.response.jsonl
Json twin: F:\GitHub\rideaudit\docs\receipts\hostile-validator-20260929T035617Z.json

## Explicit FAIL list

1. A4. Keyword classification does not define the deferred set. `AcCoverageLedgerTests.Writes_honest_ledger_for_all_requirement_ac_ids` marks an id covered whenever the string appears in `tests/**/*.cs`, and only then calls `ClassifyDeferred`. Twenty-seven ids whose 400-character YAML windows still match partnership, Play, or TLS keywords moved from deferred to covered. `AC-RIDE-203-001` stays deferred as Lyft partnership while its own text is append-only access logs; the 400-character window reaches FR-RIDE-204 Concierge.
2. B5. Honesty. The same name-first short-circuit is published as an honest 0-missing ledger. Section 11 then treats that recount as closing the unnamed-AC gap.
3. C1. Fake coverage. Those 27 previously deferred ids now appear in `tests/**/*.cs` and are counted covered. Suite green is not AC satisfaction. `isSatisfied` on the sampled YAML rows remains false. Examples that are not the AC sentence: `AC-RIDE-205-001` (UI paginates GPS) is a trait on `Multi_year_window_uses_the_index_and_the_audit_zip_omits_plaintext`, which uses `TripIndex.Window`. `AC-UC-029-001` (published protos are GPL-2.0) is a trait on `Conformance_binds_to_grpc_when_the_companion_disagrees`, which does not assert the GPL sentence.
4. D4. Plan overclaim. Section 11 changed the ledger row from class `A remaining` to class `A`, and the closing sentence now says the remaining class A product gap is only the P11b suite. Plan section 2.7 still says P11b rejects incomplete whole-AC coverage. Name presence is not that exit.
5. HV approval gate. CompletenessScore 96 is below 98. OverallVerdict stays DISAGREE.

## Explicit UNKNOWN list

1. B7. MCP session-log persist. This Cursor tool list has no rideaudit session-log plugin wrapper. Raw REST is forbidden. The review turn was not written to the MCP session log. Jsonl twins were written under `docs/receipts/hv/`. This incomplete HV cannot AGREE.

## A. Requested claims

### A1. Ledger counts 411/13/0/424, before 190/36/198/424. PASS

Working ledger `docs/receipts/ac-coverage/20260928-ledger.md` parsed to 424 unique rows, 0 duplicate ids: covered 411, deferred 13, missing 0. Summary table matches the row recount. HEAD ledger (`git show HEAD:docs/receipts/ac-coverage/20260928-ledger.md`) independently recounted to covered 190, deferred 36, missing 198, total 424. HEAD plan closeout text records the same before numbers. Transitions: missing to covered 194, deferred to covered 27, missing to deferred 4. Same 424 ids in both revisions.

### A2. Covered means the AC id string in tests/**/*.cs, not semantic closure. PASS

Ledger line 5 and `AcCoverageLedgerTests.cs` lines 46-49 state that definition. Every covered id string was found in `tests/**/*.cs` (92 cs files). Zero covered ids were absent. This PASS is only the definition. It does not make those rows semantically closed. See A4 and C1.

### A3. Explicit deferrals are the four named ids, outside tests, and VIEW-001-001 is not in that file. PASS

`docs/receipts/ac-coverage/explicit-deferrals.txt` contains exactly `AC-RIDE-222-001`, `AC-RIDE-GPL-003-001`, `AC-RIDE-GPL-003-002`, `AC-UC-025-001`. `AC-RIDE-VIEW-001-001` is absent from that file. Ledger status for VIEW-001-001 is covered. The string is in `tests/RideAudit.Client.Tests/TestRide027And028Tests.cs` line 172.

### A4. The other deferred rows are keyword-classified, the 13 ids are those families, and none of the 13 appear in tests. FAIL

The 13 deferred ids, and their absence from `tests/**/*.cs`, were confirmed:

- AC-RIDE-004-001, AC-RIDE-004-002, AC-RIDE-004-003 (partnership, own text)
- AC-RIDE-203-001 (ledger reason is partnership; own text is access logs)
- AC-RIDE-222-001 (explicit file)
- AC-RIDE-GPL-003-001, AC-RIDE-GPL-003-002 (explicit file)
- AC-RIDE-INGEST-004-001, AC-RIDE-INGEST-004-002 (id prefix plus partnership window)
- AC-RIDE-SEC-001-001 (TLS 1.2 on its own text)
- AC-RIDE-STORE-003-001, AC-RIDE-STORE-003-002 (id prefix)
- AC-UC-025-001 (explicit file)

`DEFERRED_IN_TESTS=0` for those 13. That sub-fact is true and is not the verdict.

FAIL reason: the same classifier, applied to the 400-character window and ignoring the name-first short-circuit, still tags all 27 promoted ids as partnership, Play, or TLS. Covered wins before `ClassifyDeferred` (`AcCoverageLedgerTests.cs` lines 46-54). The deferred set is "not named in tests", not "the keyword families". `AC-RIDE-203-001` text is "Views/exports of sensitive location and identity write append-only access logs." The next FR, FR-RIDE-204, is Concierge ingestion resilience, inside the 400-character window.

Promoted ids that the keyword function still matches: AC-RIDE-003-001, AC-RIDE-003-002, AC-RIDE-010-001, AC-RIDE-010-002, AC-RIDE-011-002, AC-RIDE-012-001, AC-RIDE-012-002, AC-RIDE-024-002, AC-RIDE-205-001, AC-RIDE-205-002, AC-RIDE-INGEST-003-001, AC-RIDE-INGEST-003-002, AC-RIDE-INGEST-005-001, AC-RIDE-INGEST-005-002, AC-RIDE-PRIV-003-001, AC-RIDE-PRIV-003-002, AC-TEST-003-001, AC-TEST-003-002, AC-TEST-028-001, AC-TEST-028-002, AC-TEST-029-001, AC-TEST-029-002, AC-UC-003-001, AC-UC-003-002, AC-UC-020-001, AC-UC-020-002, AC-UC-029-001.

### A5. Unsigned three-RID publish receipt, NotSigned, signtool exit 1, codesign false. PASS

Receipt text `docs/receipts/distribution/20260929T033731Z-unsigned-desktop-rid-publish.md` matches the claim. Publish outputs exist under gitignored `artifacts/desktop-publish/` (`.gitignore` contains `artifacts/desktop-publish/`).

Re-hash SHA256, not the markdown alone:

- win-x64 RideAudit.Client.Desktop.dll eaced988fe0c6ca08aef1951523e1d5b7ad44d3513d1a1f16a2c4ec68dca3b7a
- win-x64 RideAudit.Client.Desktop.exe d7c62552d725b956280b71d5cad0beaed5d98ee2bd31a6aebaf7a68cd07834ba
- linux-x64 RideAudit.Client.Desktop.dll 0e15f4c1ffcf0623c4687ab41fe9fcfe420f8f69e2862631aa41304c3eac1c87
- osx-arm64 RideAudit.Client.Desktop.dll a613f01406cb6760b131312a42aad2d43915ed2445a11be3159c211d344807c6

`publish-log.txt` records RID=win-x64 EXIT=0, RID=linux-x64 EXIT=0, RID=osx-arm64 EXIT=0. This validator did not re-run `dotnet publish`. `Get-AuthenticodeSignature` on the win-x64 exe: Status=NotSigned. signtool.exe at `C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\signtool.exe` verify /pa: SignTool Error: No signature found. SIGNTOOL_EXIT=1. `Get-Command codesign` absent: CODESIGN_ON_PATH=False. No linux or osx `.exe`. Native apphost files named `RideAudit.Client.Desktop` are present for linux-x64 and osx-arm64. The receipt's "no native apphost exe" sentence is true for the `.exe` suffix. The test `Unsigned_framework_dependent_publish_receipt_names_three_rids` reads the markdown; it does not publish. The hashes above are from the binaries.

### A6. dotnet test RideAudit.sln -c Release Failed 0 Skipped 0 with the named totals. PASS

Re-ran `dotnet test RideAudit.sln -c Release --nologo --verbosity minimal` on 2026-09-29. Exit 0. dotnet 10.0.401.

- RideAudit.Host.Windows.Tests Passed 2, Failed 0, Skipped 0
- RideAudit.Protos.Tests Passed 5, Failed 0, Skipped 0
- RideAudit.Workflow.Tests Passed 26, Failed 0, Skipped 0
- RideAudit.Escrow.Tests Passed 4, Failed 0, Skipped 0
- RideAudit.Seal.Tests Passed 8, Failed 0, Skipped 0
- RideAudit.Server.Admission.Tests Passed 28, Failed 0, Skipped 0
- RideAudit.Chain.Tests Passed 16, Failed 0, Skipped 0
- RideAudit.Client.Tests Passed 101, Failed 0, Skipped 0

Suite green is not P11b closure and is not AC satisfaction.

### A7. P11b is not closed. Section 9 unchecked. Named Class C boxes unchecked. PASS

Parent plan diff does not check P11b done. Section 11 row "Signed reproducible desktop Win/Linux/macOS and full P11b suite" class remains `A remaining`, disposition "Not closed" and "P11b exit stays open." Section 9 lines 1554-1557 are all `- [ ]`. Parent plan has zero `[x]` checkboxes. Android plan lines 215-217 unchecked. Server plan lines 254-256 unchecked. Bracket plan line 125 (optional Astra) and line 127 (HW1) unchecked. `git diff --stat` on the three child plans is empty, so this slice did not check Bracket HW0, link, or HW2 boxes that were already `[x]`. Section 11 prose still says Play, HSM, dual-phone media, live OTS/L2, and Caddy TLS fail closed or stay unchecked.

### A8. Incomplete trip rows are flagged without invented values, and the DSAR zip writes provenance.csv, provenance.json, and summary.pdf. The tests assert that behavior. PASS

`PrivacyExportParser.NoteIncompleteTripRows` adds a field gap "required trip fields absent; values were not invented" and `ReadTrips` skips rows that lack parseable start and end. `ClassAMissingAcTests.Incomplete_trip_rows_are_flagged_and_not_invented` asserts status `imported-with-gap`, gap notice contains "not invented", the complete trip-1 remains, and no stored trip has an empty TripId. It does not assert a persisted row whose missing fields are null. The claim as stated (flagged, not invented) is what the test asserts.

`PrivacyDesk.BuildZip` writes `provenance.json` (same JSON as `manifest.json`), `provenance.csv`, and `summary.pdf` (`%PDF-1.4`). `IngestAnalysisTests.Multi_year_window_uses_the_index_and_the_audit_zip_omits_plaintext` asserts the three zip entries, csv header `import_id,content_hash` plus the content hash, and pdf text starting with `%PDF-`. It does not assert the body of `provenance.json` beyond entry presence. The write and the entry assertions are real, not trait-only for these three files. Extra traits on that same method are scored under C1, not here.

## B. Workspace rules

### B1. Byrd v4 phase-order is an inter-phase gate, not a file-timestamp reconstruction. PASS

No new phase checkbox was marked complete. P11b was not claimed closed. This review does not FAIL phase order from FR `createdAt` versus file times. The section 11 class change is scored under D4.

### B2. Receipts. PASS

Ledger recount, HEAD recount, publish re-hash, Authenticode, signtool, and the Release test run are in this receipt. The implementer publish markdown was not accepted alone.

### B3. MCP-only storage. PASS

`git status` shows no edit to `todo.yaml`, session logs, or the requirements store. Sampled requirement YAML rows remain `isSatisfied: false`. This slice did not claim an MCP requirement status flip.

### B4. PowerShell only, no Python, in the reviewed diff. PASS

`git diff -- src tests docs` has no `python`, `python3`, or `py -3` hit. This validator used PowerShell. Untracked `artifacts/*.ps1` and `artifacts/*.sh` files are outside the reviewed product diff and were not executed here.

### B5. Honesty. FAIL

See the explicit FAIL list. The ledger header honestly says a covered row is not semantic closure. That sentence does not repair the name-first short-circuit that counts keyword-window ids as covered, or the section 11 sentence that drops the unnamed-AC gap.

### B6. Look-before-delete. PASS

`git diff --diff-filter=D --name-only` is empty for the reviewed changes.

### B7. HV session log. UNKNOWN

See the UNKNOWN list. Plugin wrapper unavailable. Raw REST not used.

## C. Requirements

### C1. Fake coverage of keyword-deferred ids, and string presence is not AC coverage. FAIL

Operator rule applied: a covered ledger row is only an AC id string inside `tests/**/*.cs`. Fake coverage is FAIL if a deferred AC id appears in tests and is therefore counted covered. The 27 ids in A4 were deferred on HEAD and are covered now because the string is in tests. `AC-RIDE-004-*` stayed deferred. `AC-RIDE-012-001` and `AC-RIDE-012-002` (admin partnership status, Concierge disabled when denied) are covered via `Concierge_stays_behind_the_partnership_gate_and_does_not_call_a_private_api`, while section 11 still says Lyft Concierge stays disabled. `AC-RIDE-205-001` text is "UI paginates or downsamples large GPS tracks" and the carrying test does not drive a UI. `AC-UC-029-001` text is "Published protos are GPL-2.0 and used as the contract source" and the carrying test does not assert that GPL sentence. `dotnet test` green does not close these ACs. YAML `isSatisfied` remains false on the rows read (FR-RIDE-003, FR-RIDE-010, TR-RIDE-VIEW-001, TR-RIDE-PRIV-003).

### C2. Requirements were not marked satisfied in the YAML projection. PASS

The reviewed diff does not flip `isSatisfied` to true. That is not coverage. It avoids a false satisfied flag.

## D. Plan

Active plan: `docs/plans/PLAN-RIDEAUDIT-001-implementation.md`.

### D1. P11b remains open in section 11. PASS

Same evidence as A7. The signed reproducible desktop row is `A remaining` and says P11b exit stays open.

### D2. Section 9 boxes stay unchecked. PASS

Lines 1554-1557 are `- [ ]`. The following paragraph says the boxes remain historically unchecked.

### D3. Named Class C boxes stay unchecked. PASS

Same evidence as A7 for Play, HSM, dual-phone, live OTS/L2, Caddy TLS prose, Android and Server opposing HV boxes, Bracket Astra, and HW1.

### D4. Section 11 overclaims the ledger recount as closing the class A missing-row gap. FAIL

`git diff` of the plan: the ledger inventory row class changes from `A remaining` to `A`. The before text said the 198 missing rows are not marked done. The after text says 0 missing and "The remaining class A product gap is the P11b suite, and it is not marked done." That drops the unnamed AC rows from the remaining gap while section 2.7 still requires P11b whole-AC evidence. r3.6 header and status lines do say P11b is not closed and do not invent a new Astra AGREE. Those sentences PASS. The class change and the "only remaining gap is P11b" sentence FAIL.

## Counts

PASS: 16
FAIL: 4 claim FAILs (A4, B5, C1, D4) plus the HV completeness gate below 98
UNKNOWN: 1 (B7)

Claim rollup: A 7 PASS / 1 FAIL. B 5 PASS / 1 FAIL / 1 UNKNOWN. C 1 PASS / 1 FAIL. D 3 PASS / 1 FAIL.
