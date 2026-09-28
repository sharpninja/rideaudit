# PAYTON-LEGION2 / PAYTON-OMARCHY lab receipt

Date: 2026-09-28. Host that ran the probes: PAYTON-LEGION2. SPDX: GPL-2.0-only.

This is not a Play Store receipt, not a live Bitcoin/OTS/L2 admission receipt, not a CD green, and not an HV verdict.

## Track 5 — Android

- .NET Android workload `36.1.69/10.0.100` installed.
- `ANDROID_HOME=C:\Users\kingd\AppData\Local\Android\Sdk` with `platforms/android-36` and `build-tools/36.0.0`.
- `JAVA_HOME=C:\Users\kingd\AppData\Local\Android\Jdk` is Microsoft OpenJDK 17.0.14 LTS.
- `dotnet build src/RideAudit.Client.Android/RideAudit.Client.Android.csproj -c Release` succeeded after switching the radio probe to `BluetoothManager.Adapter` (CA1422 rejected `BluetoothAdapter.DefaultAdapter` under warnings-as-errors).
- APK: `src/RideAudit.Client.Android/bin/Release/net10.0-android/org.rideaudit.app-Signed.apk` (47,392,626 bytes). Not committed.
- `adb devices` listed no device. Not installed. Not published.

## Track 3 — Windows BLE / camera on LEGION2

- `WindowsBleDiscoveryBus.Create` can see a BLE adapter (`RadioAvailable=true`) and still fail-closed on unpackaged WinRT advertise (`ArgumentException` → `BT_DISABLED`). No invented peers.
- `WindowsCameraSource` uses WinRT `DeviceInformation` / `MediaCapture`. Tests pass without returning fixture bytes.
- Avalonia capture shell no longer treats `new CaptureShellView()` as the production APK entry. Android installs `AndroidProductionComposition`; missing radio/camera/Play/admission is `PRODUCTION_UNAVAILABLE`. That is fail-closed, not a silent radio success. Physical dual-phone pairing is still not proven (`adb devices` listed no device).

## Track 6 — Docker

- LEGION2 Docker client 29.8.0. Contexts `desktop-linux` and `default` both return HTTP 500 on the named pipes. `com.docker.service` is STOPPED (`WIN32_EXIT_CODE 1077`). `docker-desktop` WSL is Running but the engine API is not usable. No image was built on LEGION2.
- PAYTON-OMARCHY (`192.168.0.149`) SSH BatchMode works. Engine 29.7.2, Compose 5.5.1, `dotnet` 10.0.111, 381G free. Existing Octopus/SQL/Caddy containers were left running.
- Deploy scripts: `deploy/omarchy/`. `scp` is unusable because Omarchy’s login shell is pwsh and prints profile banners (`Received message too long`). Sync uses stdin into `exec /usr/bin/bash --noprofile --norc`. An earlier Track 6 note recorded checkout `f51f454`; that is historical only. The cutover checkout is `2612693` at `/home/sharpninja/github/rideaudit` (see Cutover).
- Preferred path exercised: `dotnet publish` linux-x64 on LEGION2 (`artifacts/omarchy-publish/admission`, not committed) → `Sync-Publish.ps1` → `remote-runtime-build.sh` on Omarchy → `Confirm-Cutover.ps1 -ConfirmCutover`.
  - `rideaudit-admission:local` / `rideaudit-counsel:local` `sha256:031a4a6e21cc0424a6276a59b9d38cabe15f7c5670468d3a99e4dcb8aad9fee5` (runtime image from LEGION2 publish at `2612693`)
  - Earlier SDK rebuild from `dcb31bf` was superseded by this runtime build.
  - `Confirm-Cutover.ps1 -ConfirmCutover` ran. Existing Octopus/SQL/Caddy containers were left running. No GHCR push exists in this tree.

## Cutover (coordinator, post PR #7 merge)

- Merged commit on Omarchy checkout: `2612693` (PR #7 squash: Close deferred tracks and prepare LEGION2→Omarchy deploy).
- Publish on LEGION2: `artifacts/omarchy-publish/admission` (linux-x64, framework-dependent). Not committed.
- Sync: `Sync-FromLegion2.ps1` + `Sync-Publish.ps1` (stdin into `exec /usr/bin/bash --noprofile --norc`).
- Runtime image on Omarchy: `rideaudit-admission:local` / `rideaudit-counsel:local` `sha256:031a4a6e21cc0424a6276a59b9d38cabe15f7c5670468d3a99e4dcb8aad9fee5` (built 2026-09-28T15:20:20-05:00 from publish tree).
- `Confirm-Cutover.ps1 -ConfirmCutover`: compose up on loopback only. Octopus/SQL/Caddy left running.
- Container: `rideaudit-omarchy-admission-1` Up, `127.0.0.1:18080->8080/tcp`.
- Probe `GET http://127.0.0.1:18080/`: HTTP 200, body `RideAudit admission gRPC. Contract authority: grpc-protobuf. OpenAPI is a non-authoritative companion.`
- Still not a CD green, not GHCR, not TLS on Caddy edge, not Play Store.


## Chain probe

- Public OTS client code can POST to `https://alice.btc.calendar.opentimestamps.org/digest`. A pending calendar body is labeled `RIDEOTS-PENDING-1` and never writes a txid or block height.
- L2 `Commit` always fail-closes without a signer. `eth_blockNumber` probes do not admit.

## `dotnet test RideAudit.sln` on PAYTON-LEGION2

Passed 159, failed 0, skipped 0 (Host.Windows 2, Client 87, Chain 16, Protos 5, Seal 8, Escrow 4, Workflow 21, Admission 16). Android is not a test project; the Release APK was built separately.

## What remains blocked

- Live Play Integrity JWTs and Play publication.
- Live HSM hardware (in-process Shamir 2-of-3 only).
- Physical dual-phone Android pairing (no device attached).
- LEGION2 Docker Desktop engine.
- Caddy TLS 1.2+ if admission should leave Omarchy loopback.
- Opposing-model HV (separate agent).
