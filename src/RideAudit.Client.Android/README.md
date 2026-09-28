# RideAudit.Client.Android

Avalonia UI 12 dual-phone capture client (FR-RIDE-056).

- Driver phone: session coordinator, clock master, seal admission, sealed submission.
- Passenger phone: video sync, compositing, spider-graph telematics overlay.
- Pairing is RideAudit Bluetooth device pairing. There is no Lyft private API.

## Build

The Android head targets `net10.0-android` and needs the .NET Android workload:

```bash
dotnet workload install android
# Android SDK platform 36 and build-tools are required.
export ANDROID_HOME="$HOME/android-sdk"
export JAVA_HOME=/usr/lib/jvm/java-21-openjdk-amd64
dotnet build src/RideAudit.Client.Android/RideAudit.Client.Android.csproj -p:AndroidSdkDirectory="$ANDROID_HOME" -p:JavaSdkDirectory="$JAVA_HOME"
```

Shared capture logic and Avalonia views build and test without the Android workload:

```bash
dotnet test tests/RideAudit.Client.Tests/RideAudit.Client.Tests.csproj
```

## Play Integrity

`UnavailablePlayIntegrityClient` is the production default in this tree. It returns no token, so the gate fail-closes before key generation, sealing, or upload. There is no user-reachable switch that disables the gate.

`StubPlayIntegrityClient` and `FixturePlayIntegrityClient` exist for tests. Both are labeled in code. Neither calls the Google Play Integrity API, and neither is a Play Store receipt. A stub success is recorded as provider `stub-play-integrity` and is not treated as court-ready by the viewer unless a test explicitly opts into simulated attestation.

## What this build is not

- Not published on Google Play. See `src/RideAudit.Licensing/Distribution/client-distribution-manifest.json`.
- Not road-ready. Camera encode is a canonical on-device composite used to test the seal, quota, and custody contracts. It is not an H.264 production encoder.
- Not a claim that a physical Bluetooth radio was exercised in the cloud. Pairing tests use an in-memory discovery bus with the same role rules.

Historical Kotlin and Gradle files are archived under `artifacts/android/legacy-kotlin/`.

License: GPL-2.0-or-later. See the repository `LICENSE`.
