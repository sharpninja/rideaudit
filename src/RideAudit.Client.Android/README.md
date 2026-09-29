# RideAudit.Client.Android

Avalonia UI 12 dual-phone capture client (FR-RIDE-056).

- Driver phone: session coordinator, clock master, seal admission, sealed submission.
- Passenger phone: video sync, compositing, spider-graph telematics overlay.
- Pairing is RideAudit Bluetooth device pairing. There is no Lyft private API.

## Debug visual tree

Debug builds reference `SharpNinja.Avalonia.RemoteControl.Runtime` 0.7.4 and listen on device loopback port 47100. Release builds do not. The bearer token is random per process and is written only to the package-private marker `files/avalonia-remote-control.json`. This is not a Play publication.

Desktop side, after `dotnet tool install --global SharpNinja.Avalonia.RemoteControl.Tool --version 0.7.4`:

```powershell
avalonia-remote adb connect --serial <device-serial> --package org.rideaudit.app --keep-forward
avalonia-remote
```

Use a second `--host-port` when two phones are forwarded at once. The transport is `arc-protobuf-v1`. Cleanup with `avalonia-remote adb cleanup --serial <device-serial> --host-port <port>`.

On PAYTON-LEGION2 the USB Fold forward completed GetCapabilities (frames and input supported). The motorola edge 2024 wireless forward reached the device port and did not complete GetCapabilities. That lab note is `docs/receipts/android/20260929T164101Z-android-remote-control.md`.

## Build

The Android head targets `net10.0-android` and needs the .NET Android workload. On PAYTON-LEGION2 (2026-09-28) the workload, Android SDK platform 36, build-tools 36.0.0, and Microsoft OpenJDK 17 were present. `dotnet build -c Release` produced an APK. `adb devices` listed no device, so the APK was not installed. That is not a Play Store receipt.

```bash
dotnet workload install android
# Android SDK platform 36 and build-tools are required.
export ANDROID_HOME="$HOME/android-sdk"
export JAVA_HOME=/usr/lib/jvm/java-21-openjdk-amd64
dotnet build src/RideAudit.Client.Android/RideAudit.Client.Android.csproj -c Release -p:AndroidSdkDirectory="$ANDROID_HOME" -p:JavaSdkDirectory="$JAVA_HOME"
```

A successful Release build writes `src/RideAudit.Client.Android/bin/Release/net10.0-android/org.rideaudit.app-Signed.apk`. Do not commit that APK.

Shared capture logic and Avalonia views build and test without the Android workload:

```bash
dotnet test tests/RideAudit.Client.Tests/RideAudit.Client.Tests.csproj
```

## Play Integrity

`AndroidProductionComposition` is the production APK composition root. It probes BLE and cameras, then wires Play, canonical admission, gRPC, and escrow. Missing hardware or configuration is recorded as `Unavailable*` and the capture shell fail-closes. The APK does not construct `CaptureShellView()` with a silent `UnavailableDiscoveryBus` success.

`UnavailablePlayIntegrityClient` is the production Play default in this tree. It returns no token, so the gate fail-closes before key generation, sealing, or upload. There is no user-reachable switch that disables the gate.

`StubPlayIntegrityClient` and `FixturePlayIntegrityClient` exist for tests. Both are labeled in code. Neither calls the Google Play Integrity API, and neither is a Play Store receipt. Fixture evidence uses provider `fixture-play-integrity` plus a stub notice and is rejected by the viewer's court-ready predicate. A stub success is recorded as provider `stub-play-integrity` and is not treated as court-ready unless a test explicitly opts into simulated attestation.

## What this build is not

- Not published on Google Play. See `src/RideAudit.Licensing/Distribution/client-distribution-manifest.json`.
- Not road-ready. Camera encode is a canonical on-device composite used to test the seal, quota, and custody contracts. It is not an H.264 production encoder.
- Not a claim that a physical Bluetooth radio was exercised in the cloud. Pairing tests use an in-memory discovery bus with the same role rules. The Android host fail-closes through `AndroidCaptureHardware` when the radio or camera is missing. Windows lab adapters are in `RideAudit.Host.Windows`.

Historical Kotlin and Gradle files are archived under `artifacts/android/legacy-kotlin/`.

License: GPL-2.0-or-later. See the repository `LICENSE`.
