# Remediation receipt — code-hv-sol-r1 round 1

**Generator:** grok-4.6 xhigh (Cursor Grok 4.6)  
**Workspace:** `F:\GitHub\rideaudit` on PAYTON-LEGION2  
**Base:** `origin/master` `dadde67`  
**Opposing HV:** not this agent's job; Sol HV re-runs after merge  
**MCP session log:** `mcpserver-box` namespace discovery failed (error). Not treated as green.

## B/D map

| ID | Status | Notes |
| --- | --- | --- |
| D01 | **CLOSED** | Chunk uploads now have concurrent, per-upload count, aggregate-byte, assembled-size, and age bounds; completed/expired uploads are dropped. Tests in `ChunkUploadBoundsTests`. |
| D02 | **CLOSED** | `deploy/omarchy/compose.yaml` no longer has `build:`. Images are digest-pinned to the recorded lab digest. Rebuilds use `compose.build.yaml`. |
| D03 | **CLOSED** | `Confirm-Cutover.ps1 -ConfirmCutover` probes `http://127.0.0.1:18080/` and fails unless HTTP 200; also greps the recorded image digest. |
| D04 | **CLOSED** | Distribution receipt no longer presents `f51f454` as the current Omarchy checkout. Cutover checkout remains `2612693`. |
| B01 | **CLOSED (authorization recorded)** | Payton 2026-09-28 iterate-until-HV-agree is retained. P0/Payton/phase-HV boxes stay historically unchecked. Not a backdated construction-gate pass. |
| B02 | **CLOSED (composition root)** | Android `AndroidProductionComposition` wires BLE/camera/Play/canonical admission/gRPC/escrow seams. Missing hardware or config is honest `Unavailable*`. Production APK no longer constructs `CaptureShellView()` as a silent UnavailableDiscoveryBus success. Two-device physical proof remains deferred. |
| B03 | **CLOSED (canonical contract)** | Default capture session uses `CanonicalAdmissionRequestFactory` (tenant, driver, policy, raw token, `ReceiptCoreCodec`). `PreflightAdmissionRequestFactory` and `ServerAdmissionRequestFactory` are explicit test/fixture injects. Production Play decode is still fail-closed (B07). |
| B04 | **DEFERRED + partial** | Composite is now a source-payload container with hash binding tests. Not H.264. No Bluetooth media transport. Court-ready independently supplied dual-phone video remains blocked. |
| B05 | **CLOSED** | `FixturePlayIntegrityClient` returns provider `fixture-play-integrity` plus a stub notice. `VerificationGate` court-ready requires real provider and empty notice. Fixtures fail `play_attestation` unless a test opts into stub+`allowSimulated`. |
| B06 | **PARTIAL** | Honest ledger at `docs/receipts/ac-coverage/20260928-ledger.md`: 404 AC IDs, 111 covered, 34 deferred-with-reason, 259 missing. Added highest-risk chunk/admission/Play/custody tests. Full 404 executable greens are not claimed. |
| B07 | **DEFERRED** | Real Play decoder, hardware HSM, OTS upgrade/confirmation, and L2 signer remain absent. Adapters stay fail-closed. No invented tokens or txids. |
| B08 | **DEFERRED** | Lab loopback cutover only. No GHCR, edge TLS proof, signed release, Play publication, or automated Dev/Staging/Prod greens. D02/D03 harden the lab path; they do not invent CD. |
| B09 | **CLOSED** | Dated amendment: `gpt-5.6-sol` xhigh is the approved Sol-family opposing validator; `gpt-6-sol` is the family alias. Next Sol HV rerun is formally eligible. |

## Evidence

- `dotnet test RideAudit.sln -c Release`: 169 passed, 0 failed, 0 skipped (Host.Windows 2, Client 95, Workflow 21, Admission 18, Protos 5, Seal 8, Escrow 4, Chain 16).
- Android Release build: 0 warnings, 0 errors.
- MCP session log: `mcpserver-box` unavailable; not treated as green.

## Honesty

Do not read this receipt as Play Store, live HSM, dual-phone pairing, GHCR, CD-green, or chain confirmation evidence.
