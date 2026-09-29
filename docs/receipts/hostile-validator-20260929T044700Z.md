# Hostile validator receipt

- TimestampUtc: 2026-09-29T04:47:00Z
- ValidatorIdentity: GrokSubagentHostile
- Workspace: F:\GitHub\rideaudit
- Work class: project implementation
- add-profile: executed yes. Profile file count read: 19. Excluded skill port `add-profile.grok.md`. Directory had 20 `*.md` files.
- Prior receipt: `docs/receipts/hostile-validator-20260929T041746Z.md` was not used as evidence. This run re-read files and re-ran tests.
- Accuracy: 99
- Completeness: 98
- OverallVerdict: AGREE

## A. Requested validation

### A1. Ledger is 401/23/0/424, covered+deferred=424, deferred wins, AC-RIDE-031-001 and AC-RIDE-031-002 are deferred and are not traits. PASS

Working-tree `docs/receipts/ac-coverage/20260928-ledger.md` header and parsed rows: covered 401, deferred 23, missing 0, total 424, row count 424, duplicate ids 0, sum 424. SHA256 before and after `dotnet test` was `975b74c93303ea6d645b63c5a9d370e4150348f89d5dabeb7c6ff6ae28d11999` (54894 bytes). The test run did not change those bytes.

Independent PowerShell replica of `AcCoverageLedgerTests.ClassifyDeferred` over 8 `docs/Project/*Batch.yaml` files: raw AC matches 424, unique 424, duplicate groups 0. Replica covered 401, deferred 23, missing 0, sum 424. Compared to the ledger rows: MISMATCH=0, LEDGER_ONLY=0, REPLICA_ONLY=0.

`explicit-deferrals.txt` has 12 ids, including AC-RIDE-031-001 and AC-RIDE-031-002. Classifier checks that map before a test-source name. Replica status for both is deferred, named=False. Ledger status for both is deferred. `tests/**/*.cs` text hits for those two ids: 0. They are not traits.

Deferred still wins when a test names the id. Three deferred rows are also named in test source and stay deferred: AC-RIDE-201-001, AC-RIDE-206-001, AC-RIDE-CHAIN-001-002. DEFERRED_ALSO_NAMED=3.

YAML text remains unsatisfied: AC-RIDE-031-001 "Client is available on Google Play and public source repository." `isSatisfied: false`. AC-RIDE-031-002 "Build and signing metadata are versioned and reproducible." `isSatisfied: false`.

### A2. Octopus receipt has no ghcr.io substring. The named test asserts that and still asserts octopus-rideaudit and PAYTON-DESKTOP. PASS

`docs/receipts/distribution/20260929T015822Z-octopus-payton-desktop.md`: `IndexOf("ghcr.io", OrdinalIgnoreCase)` = -1. The file contains `octopus-rideaudit`, `PAYTON-DESKTOP`, `No GHCR push or pull`, and `Not a GHCR green`. Case-insensitive `ghcr` occurs 5 times, each without `.io` (not GHCR, No GHCR push or pull, Absence of a GHCR repository name, Do not invent a GHCR digest, Not a GHCR green).

`Octopus_desktop_receipt_names_instance_and_does_not_claim_ghcr` in `tests/RideAudit.Server.Admission.Tests/NgrokDeploySecretsTests.cs` contains `Assert.Contains("octopus-rideaudit", receipt, ...)`, `Assert.Contains("PAYTON-DESKTOP", receipt, ...)`, `Assert.Contains("No GHCR push or pull", ...)`, `Assert.Contains("Not a GHCR green", ...)`, and `Assert.DoesNotContain("ghcr.io", receipt, StringComparison.OrdinalIgnoreCase)`. That method body does not use the trailing-slash needle `ghcr.io/`. Admission tests passed in the Release run below, so this assert held against the file on disk.

### A3. dotnet test RideAudit.sln -c Release is green with the stated counts. PASS

Command: `dotnet test F:\GitHub\rideaudit\RideAudit.sln -c Release --nologo`. Exit code 0. Ended 2026-09-29T04:33:52Z. Elapsed about 20 seconds. Console lines:

- RideAudit.Host.Windows.Tests.dll Failed 0, Passed 2, Skipped 0, Total 2
- RideAudit.Protos.Tests.dll Failed 0, Passed 5, Skipped 0, Total 5
- RideAudit.Workflow.Tests.dll Failed 0, Passed 26, Skipped 0, Total 26
- RideAudit.Escrow.Tests.dll Failed 0, Passed 4, Skipped 0, Total 4
- RideAudit.Seal.Tests.dll Failed 0, Passed 8, Skipped 0, Total 8
- RideAudit.Server.Admission.Tests.dll Failed 0, Passed 28, Skipped 0, Total 28
- RideAudit.Chain.Tests.dll Failed 0, Passed 16, Skipped 0, Total 16
- RideAudit.Client.Tests.dll Failed 0, Passed 101, Skipped 0, Total 101

Assembly sum 190. Failed 0 and Skipped 0 on every assembly. The trx logger reused one file name, so the trx on disk is the last assembly only. The counts above are the console lines, not that trx.

### A4. P11b is not closed. Unsigned publish receipt remains. Section 9 unchecked. Class C boxes unchecked. Ledger row class is A remaining. PASS

`docs/plans/PLAN-RIDEAUDIT-001-implementation.md` status says P11b is not closed. P11b exit text says this revision does not close P11b. Section 11 says P11b and whole-AC acceptance remain open and are not marked done.

Unsigned receipt exists: `docs/receipts/distribution/20260929T033731Z-unsigned-desktop-rid-publish.md`. It records win-x64, linux-x64, and osx-arm64 publish EXIT=0, `AUTHENTICODE_win-x64 Status=NotSigned`, `SIGNTOOL_EXIT=1`, `CODESIGN_ON_PATH=False`, and "P11b is not closed."

Section 9 has four boxes, each `- [ ]`. Android plan section 7 has three `- [ ]`. Server plan section 7 has three `- [ ]`. Bracket plan section 6 Class C lines are unchecked: optional Astra/child HV, and HW1 on-vehicle print. Bracket `[x]` boxes are HW0, child-plan links, and HW2. Those lines are not class C.

Section 11 class column is `A remaining` for signed reproducible desktop builds and the full P11b suite, and `A remaining` for the AC ledger row. That ledger cell states 401 covered / 23 deferred / 0 missing / 424.

### A5. AC-RIDE-203-001 and AC-UC-029-001 stay covered by tests that assert their sentences. PASS

Ledger and replica: both covered, both named in test source.

AC-RIDE-203-001 text: "Views/exports of sensitive location and identity write append-only access logs." `isSatisfied: false`. `IngestAnalysisTests.Access_log_is_append_only` has trait `AC-RIDE-203-001`. It calls `ViewLocations` twice, asserts the access-log count grows by one then by two, and asserts `AppendOnlyAccessLog` method names do not contain Clear or Remove. That is the append-only sentence for a location view. The same method does not call an export path and does not assert an identity field. `isSatisfied` stays false. Coverage here is the test-source name plus those asserts, which the ledger calls not semantic closure.

AC-UC-029-001 text: "Published protos are GPL-2.0 and used as the contract source." `isSatisfied: false`. `ContractAuthorityTests.Protos_publish_gpl_notice_and_grpc_authority` has trait `AC-UC-029-001`. For each `src/RideAudit.Protos/Protos/**/*.proto` it asserts `SPDX-License-Identifier: GPL-2.0-only` and `CONTRACT_AUTHORITY: grpc-protobuf`. Those files are the in-tree contract sources. The test does not read a separate publish drop.

## B. Workspace rules

### B1. Honesty. PASS

The five requested claims match the working-tree files and the Release console counts. No count was taken from the prior DISAGREE receipt.

### B2. Byrd Development Process v4, project implementation only. PASS

Work class is project implementation. This review does not treat P11b, section 9, or a whole AC as complete. No plan step was marked done. Phase order was not scored from file timestamps.

### B3. Receipts. PASS

Ledger hash, replica mismatch count, receipt substring index, plan checkbox scan, and the Release test console are in this receipt. The unsigned publish file was re-read. signtool was not re-run; the claim is that the unsigned receipt remains.

### B4. MCP-only storage. PASS

`docs/Project/*Batch.yaml` has no `isSatisfied: true`. This review did not edit TODO storage or requirement storage. Session log uses the Grok plugin wrapper `lib/repl-invoke.ps1`, not a hand-edited log file.

### B5. PowerShell only, no Python. PASS

Ledger replica, receipt scan, plan scan, and `dotnet test` were PowerShell. `git status --short -- '*.py'` printed no paths. No Python interpreter was invoked.

### B6. Session-log persist. PASS

Plugin wrapper path: `C:\Users\kingd\.grok\installed-plugins\f--github-mcpserver-grok-plugin-67f1f31f\lib\repl-invoke.ps1`. It is present. Bootstrap `workflow.sessionlog.bootstrap` returned `initialized: true` (requestId `req-20260929T043557Z-8b0a`, `deprecated: true` as success metadata).

PASS. Wrapper C:\Users\kingd\.grok\installed-plugins\f--github-mcpserver-grok-plugin-67f1f31f\lib\repl-invoke.ps1 is present. Bootstrap workflow.sessionlog.bootstrap returned initialized true (requestId req-20260929T043557Z-8b0a, deprecated true as success metadata). openSession, beginTurn, appendDialog, appendActions, and completeTurn returned success. Local cache directory resolved to F:\GitHub\McpServer\.mcpServer\grok. The turn marker path is F:\GitHub\rideaudit\AGENTS-README-FIRST.yaml. current-turn.yaml sessionId is GrokCode-20260929T014118Z-plugin-session. requestId is req-20260929T044058Z-hv-p11b-ledger. Local status after completeTurn is completed (auditDialog 2, auditActions 3). client.SessionLog.QueryAsync agent=GrokCode limit=8 contained that requestId, the title Hostile validate ledger ghcr tests and open P11b, and ledger SHA256 975b74c93303ea6d645b63c5a9d370e4150348f89d5dabeb7c6ff6ae28d11999 both before and after completeTurn. After complete, the same query contained OverallVerdict AGREE. workflow.sessionlog.queryHistory did not list child session id GrokCode-20260929T044058Z-hv-p11b-ledger. The durable turn is on GrokCode-20260929T014118Z-plugin-session. QueryAsync is the server proof. No session record was invented.

## C. Requirements

### C1. Claimed AC ids have text, and satisfaction is not flipped. PASS

AC-RIDE-031-001, AC-RIDE-031-002, AC-RIDE-203-001, and AC-UC-029-001 exist in the batch YAML. All four sampled criteria are `isSatisfied: false`. A repo search of `docs/Project/*Batch.yaml` found no `isSatisfied: true`. 031 are deferred, not covered. 203 and 029 are covered by the tests in A5. This slice does not claim those FRs satisfied.

### C2. No false completion of FR-RIDE-031. PASS

Play availability and reproducible signing stay deferred in `explicit-deferrals.txt` and in the ledger. The unsigned publish receipt says it is not a signed release and not P11b closure.

## D. Current plan

### D1. Recount numbers in the plan match 401/23/0/424. PASS

`401 covered` occurs 3 times and `23 deferred` occurs 3 times in `PLAN-RIDEAUDIT-001-implementation.md` (revision line, status paragraph, section 11 ledger row). Status and section 11 also say `0 missing / 424`. The revision line says the missing count is 0 and `401 covered / 23 deferred / 424`. Section 2.1 still records the planning split 404 plus 20 post-planning rows = 424. That is the inventory split, not a second coverage count. No `403 covered` or `21 deferred` remains as the current recount.

### D2. P11b, section 9, class C, and the ledger row. PASS

Same evidence as A4. Section 11 ledger row class is `A remaining`. Signed desktop row class is `A remaining`. Neither row is marked done.

### D3. Plan DoD is not claimed met. PASS

P11b exit still requires signed reproducible desktop builds, Play and source receipts, and opposing-model AGREE. The plan text says this revision does not close P11b. A green unit suite does not satisfy that exit.

## FAIL list

None.

## UNKNOWN list

None.

## Counts

- PASS: 16
- FAIL: 0
- UNKNOWN: 0

## Notes that do not flip a PASS

- `Writes_honest_ledger_for_all_requirement_ac_ids` asserts missing is 0, total is 424, and covered+deferred equals the row count. It does not assert the literals 401 and 23. Those literals were checked by the replica and by parsing the ledger.
- AC-RIDE-203-001's export and identity clauses are not executed by `Access_log_is_append_only`. The trait and the append-only view asserts are present. `isSatisfied` remains false.
- Bracket section 6 has three `[x]` boxes. They are HW0, link closure, and HW2, not the class C boxes.
- Git status shows `M` for the ledger and the implementation plan versus the index. The verified bytes are the working tree. HEAD was not used as the source.
