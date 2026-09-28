# Remediation receipt — code-hv-sol-r2 round 2

**Generator:** grok-4.6 xhigh (Cursor Grok 4.6)  
**Workspace:** `F:\GitHub\rideaudit` on PAYTON-LEGION2  
**Base:** `origin/master` `bf8f6ac` (HV r2 custody, PR #10)  
**Opposing HV:** not this agent's job; Sol HV re-runs after merge  
**MCP session log:** `mcpserver-box` discovery failed; `mcpserver-grok-plugin` missing (`MCP_PLUGIN_UNAVAILABLE:GrokCode`). Not treated as green.

## B/D map

| ID | Status | Notes |
| --- | --- | --- |
| D01 | **CLOSED** | `AdmissionCoordinator` / `AbuseGuard` / `AttestationArchive` mutations take a lock. Aggregate bytes include retained receipt/token/nonce/key-id metadata. `ConcurrentQuotaSafetyTests` failed on rem-r1 races and metadata skip, then passed after the lock. |
| D02–D04 | CLOSED (r1) | Unchanged. Lab compose/cutover/receipt only. |
| B01 | **CLOSED (rem-phase custody)** | Rem-phase note + checklist record operator 2026-09-28 iterate-until-agree as the active gate. Historical P0 / section 8 / Payton AGREE boxes stay unchecked. Live tracks stay deferred. |
| B02 | **CLOSED (executable path)** | `CaptureShellView` start/stop call `CaptureRuntime` camera → Play → seal → escrow → authenticated admission. Missing hardware/Play/HSM/admission is Unavailable*. Not a dead composition root. Android runtime permission probe recorded as `UnavailablePermissions`. Camera2 encode and physical pairing remain B04/B07. |
| B03 | **CLOSED (real gRPC proof)** | `CanonicalGrpcAdmissionTests` send `CanonicalAdmissionRequestFactory` requests through in-process `AdmissionGrpcService` and assert admit/reject. `ServerAdmissionRequestFactory` is not this path. |
| B04 | **DEFERRED** | In-process dual-phone/source-payload tests exist. No Bluetooth media transport. Not H.264. |
| B05 | CLOSED (r1) | Fixture Play still fails court-ready. |
| B06 | **IMPROVED** | Named-AC coverage 111→170; deferred 34→36; missing 259→198 of 404. Remaining live ACs deferred-with-reason (B04/B07/B08). Not 404/404. Ledger: `docs/receipts/ac-coverage/20260928-ledger.md`. |
| B07 | **DEFERRED** | No real Play decoder, hardware HSM, OTS confirm, or L2 signer. Fail-closed / labeled fixtures only. |
| B08 | **DEFERRED** | Lab loopback only. No GHCR, edge TLS proof, signed release, Play publication, or Dev/Staging/Prod CD. |
| B09 | CLOSED (r1) | `gpt-5.6-sol` xhigh remains the approved opposing validator. |

## CODE-HV READY

Defined in `docs/process/code-hv-ready-remediation-loop-20260928.md`. Closed code defects proven + deferred live tracks fail-closed and non-overclaimed. Not product-complete.

## Evidence

- D01 red-before: `ConcurrentQuotaSafetyTests` 3 failed / 1 passed on unsynchronized rem-r1 dictionaries and metadata skip; 4/4 after the lock + retained-metadata accounting.
- `dotnet test RideAudit.sln -c Release`: **180 passed, 0 failed, 0 skipped** (Host.Windows 2, Client 99, Workflow 21, Admission 25, Protos 5, Seal 8, Escrow 4, Chain 16).
- Android Release (`dotnet build src/RideAudit.Client.Android/RideAudit.Client.Android.csproj -c Release`): **0 warnings, 0 errors**. CA1416 on API-31 Bluetooth permission constants closed with `OperatingSystem.IsAndroidVersionAtLeast(31)`.
- AC ledger: 170 covered / 36 deferred / 198 missing / 404 total.
- PR: https://github.com/sharpninja/rideaudit/pull/11
- MCP session log: unavailable; not green.

## Honesty

Do not read this receipt as Play Store, live HSM, dual-phone pairing, GHCR, CD-green, confirmed OTS, L2 signing, or chain txid evidence.
