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
- Avalonia capture shell still defaults to `UnavailableDiscoveryBus`. That is fail-closed, not a silent radio success.

## Track 6 — Docker

- LEGION2 Docker client 29.8.0. Contexts `desktop-linux` and `default` both return HTTP 500 on the named pipes. `com.docker.service` is STOPPED (`WIN32_EXIT_CODE 1077`). `docker-desktop` WSL is Running but the engine API is not usable. No image was built on LEGION2.
- PAYTON-OMARCHY (`192.168.0.149`) SSH BatchMode works. Engine 29.7.2, Compose 5.5.1, `dotnet` 10.0.111, 381G free. Existing Octopus/SQL/Caddy containers were left running.
- Deploy scripts: `deploy/omarchy/`. Coordinator cutover after merge is `docker compose -f deploy/omarchy/compose.yaml up -d` on loopback `:18080`. This agent does not start that stack as production.

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
- Production cutover on Omarchy (coordinator after merge).
- Opposing-model HV (separate agent).
