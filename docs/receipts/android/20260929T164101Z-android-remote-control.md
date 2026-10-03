# Debug RemoteControl on Fold 4 and Edge

TimestampUtc: 2026-09-29T16:41:01Z
Host: PAYTON-LEGION2
SPDX: GPL-2.0-only

This is a lab debug receipt. It is not a Google Play publication. It does not close live OTS, Caddy edge TLS, or hardware HSM. It does not mark AC-UC-025-001 satisfied. It does not mark FR-RIDE-067, storyboard RemoteControl acceptance criteria, or an aiUnit visual compare satisfied.

The shared Avalonia font resources are unchanged in this pass (body 28, title 36). Text bounds measured on the earlier Release APK stay in `docs/receipts/android/20260929T154510Z-dual-font-both.md`. This pass did not recapture those bounds.

## What was wired

Debug builds of `RideAudit.Client.Android` reference `SharpNinja.Avalonia.RemoteControl.Runtime` 0.7.4 and `Microsoft.Extensions.DependencyInjection` 10.0.8 from `Directory.Packages.props`. `AndroidRemoteControlHost` listens on `127.0.0.1:47100` with a random 32-byte token (64 hex characters), `IsAdbTunnel` true, and remote actions, frames, and input allowed. Mutable properties stay deny-by-default. The root is `App.ShellRoot`, because `IActivityApplicationLifetime` only exposes `MainViewFactory`.

`MainActivity` starts that host only inside `#if DEBUG`. Release compiles with `AndroidRemoteControlHost.cs` removed. The desktop tool reads the token from the package-private marker `files/avalonia-remote-control.json` via `run-as`. Do not copy the token into a receipt or a log.

`EmbedAssembliesIntoApk` is true so a debug install does not depend on fast deployment. An earlier debug APK without embedded assemblies aborted at launch with no assemblies under `files/.__override__/arm64-v8a`.

## Desktop connect

Global tool on this host: `sharpninja.avalonia.remotecontrol.tool` 0.7.4, command `avalonia-remote`. `avalonia-remote --version` is not a supported command. Install:

```powershell
dotnet tool install --global SharpNinja.Avalonia.RemoteControl.Tool --version 0.7.4
```

One phone, USB serial:

```powershell
avalonia-remote adb connect --serial RFCW7078MVZ --package org.rideaudit.app --keep-forward
avalonia-remote
```

A second phone needs its own host port. This probe used `47101` for the Edge while `47100` stayed the device port on both:

```powershell
avalonia-remote adb connect --serial adb-ZD222QH58Q-DiWqEr._adb-tls-connect._tcp --package org.rideaudit.app --host-port 47101 --keep-forward
```

Cleanup:

```powershell
avalonia-remote adb cleanup --serial RFCW7078MVZ --host-port 47100
avalonia-remote adb cleanup --serial adb-ZD222QH58Q-DiWqEr._adb-tls-connect._tcp --host-port 47101
```

The tool reads the bearer token from the marker when `--package org.rideaudit.app` is set. The marker fields saved here, with the token redacted, are `schemaVersion` 1, `devicePort` 47100, `bridgeProtocol` `arc-protobuf-v1`, token length 64.

## Devices and APK

Pairing was not repeated. `adb devices -l` showed the Fold on USB and the Edge twice (`192.168.0.137:42825` and `adb-ZD222QH58Q-DiWqEr._adb-tls-connect._tcp`). Those two Edge lines are the same phone, serial `ZD222QH58Q`. Probes used the mDNS serial.

| Phone | Model | Serial | Build | Package flags | lastUpdateTime (host local) | pid at probe |
| --- | --- | --- | --- | --- | --- | --- |
| Fold 4 | SM-F936U `q4qsqw` | RFCW7078MVZ | BP2A.250605.031.A3.F936USQSAIZH2 | DEBUGGABLE | 2026-09-29 11:31:48 | 12390 |
| Edge | motorola edge 2024 `avatrn_g` | ZD222QH58Q | W1UANS36H.29-25-2-6 | DEBUGGABLE | 2026-09-29 11:31:58 | 20550 |

Both packages are `org.rideaudit.app` versionName `0.1.0` versionCode `1`. The debug reinstall cleared runtime permission grants. CAMERA, location, and Bluetooth permissions were `granted=false` at probe time. This pass did not grant them again and did not tap WF screens.

Installed debug APK, not installed as a store build:

| Field | Value |
| --- | --- |
| Path | `src/RideAudit.Client.Android/bin/Debug/net10.0-android/org.rideaudit.app-Signed.apk` |
| Bytes | 79070201 |
| SHA256 | 44906182FE4B0513A2FAD875112BA07C7D5EF9A109ACC69DA154641A3E36E518 |
| LastWriteTimeUtc | 2026-09-29T16:31:30.6448521Z |

Zip entries present in that debug APK and absent from the Release APK: `Avalonia.RemoteControl.Runtime`, `Avalonia.RemoteControl.Protocol`, and `Microsoft.Extensions.DependencyInjection` (arm64-v8a and x86_64). Release `dotnet build -c Release` finished with 0 warnings and 0 errors. The Release APK is 47523889 bytes, SHA256 `5AC813EBF3F8DE4FA9F1862ED1CB5BA357EBA0C78BF3B479AF9B79D381FF7F67`, and contains zero RemoteControl entries. That Release APK was not installed. The phones still have the debug APK.

## Probe

Evidence directory: `docs/receipts/android/remote-20260929/`.

Both markers matched the redacted shape above. `/proc/net/tcp` showed local address `0100007F:B7FC` (`127.0.0.1:47100`) state `0A` (LISTEN) on both phones. Fold uid in that line was 10380. Edge uid was 10323.

`logcat -d -s RideAuditRemote:I` returned an empty buffer on both phones during this probe. Startup log lines from the earlier launch are not evidence in this file.

Fold, USB:

`avalonia-remote adb connect --serial RFCW7078MVZ --package org.rideaudit.app --keep-forward` exited 0. Saved text in `fold-connect.txt`:

```
ADB forward ready.
Serial: RFCW7078MVZ
Endpoint: http://127.0.0.1:47100/
Protocol: 1.0
Audit identity: remote-client
Frame streaming: supported
Remote input: supported
Connection profile saved for desktop client.
```

That is a live capabilities response from the debug bridge. This pass did not walk a storyboard, compare frames, or drive the shell through the visual tree after that response.

Edge, wireless:

`toybox nc -w 2 -z 127.0.0.1 47100` from `adb shell` printed `nc: Timeout`. A following `NC:True` in `probe-log.txt` is not a device exit code. The probe script expanded PowerShell `$?` before the shell command ran.

`avalonia-remote adb connect --serial adb-ZD222QH58Q-DiWqEr._adb-tls-connect._tcp --package org.rideaudit.app --host-port 47101 --keep-forward` exited 1. The tool text was: `Bridge connection for GetCapabilities closed before a complete response was received.` After that attempt, `adb forward --list` contained `adb-ZD222QH58Q-DiWqEr._adb-tls-connect._tcp tcp:47101 tcp:47100` and the Fold `tcp:47100 tcp:47100`. The forward was created. The capabilities call did not complete. This receipt does not claim a desktop visual tree on the Edge.

Cleanup then removed both forwards. `adb forward --list` was empty at 2026-09-29T16:41:01Z.

After that probe, a local marker read printed the live token into the shell error stream. Both apps were force-stopped and started again so that token is not the live one. It is not copied here. New pids were Fold `13054` and Edge `25720`. Each again had one `/proc/net/tcp` line for port `47100` and a new marker with token length 64, port 47100, protocol `arc-protobuf-v1`. The Fold GetCapabilities result above is from pid `12390`, not from pid `13054`. The desktop connection profile saved during that connect is stale. Run the `avalonia-remote adb connect` command again. It reads the current marker. This restart was not a second capabilities proof.

## Not claimed

- Play listing or publication.
- A desktop visual-tree attach on the motorola edge 2024.
- Storyboard steps, frame compares, or aiUnit.
- A new font measurement on this debug APK.
- A Bluetooth peer, camera frames, Play Integrity JWT, admission, or hardware HSM.
- A home-screen launcher launch.
- Closure of AC-UC-025-001.
