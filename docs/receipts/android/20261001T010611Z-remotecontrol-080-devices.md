# RemoteControl 0.8.0 port 0 on Fold 4 and motorola edge 2024

Host: PAYTON-LEGION2. Checkout: `F:\GitHub\rideaudit-rc080` on `cursor/remotecontrol-080-dynamic-port-178e` at `d3cdd1728b26eac50d6f488d2f8b2ee56dc02c09` (parent `d103d34`).

`F:\GitHub\rideaudit` stayed on `cursor/capture-operator-reqs-b19f` (`9f9c5f0`). `F:\GitHub\rideaudit-ui-font` stayed on `cursor/dual-phone-fold-moto-8aa2` (`88c8714`). The PR branch was already checked out in the `rideaudit-rc080` worktree, so the probe ran there.

Probe window: 2026-10-01T01:06:11.3627419Z through 2026-10-01T01:06:58.9852389Z. Moto focused follow-up: 2026-10-01T01:18:56.8132378Z through 2026-10-01T01:19:07.1604278Z.

No rebase. `gh pr view 28` reported `mergeable=MERGEABLE` and `mergeStateStatus=CLEAN` against base `cursor/dual-phone-fold-moto-8aa2` at `88c8714397a7ecf560f43c51f86d105a4f1e905e`. `git rev-list --left-right --count origin/cursor/dual-phone-fold-moto-8aa2...origin/cursor/remotecontrol-080-dynamic-port-178e` was `12 1`. Base commit `3310c62` already carries the same port-0 patch. `git diff d3cdd17 3310c62` is only `docs/receipts/android/20260930T202334Z-review-hosted.md` on the base side. A rebase was not required for a clean merge.

This pass does not mark a functional requirement or acceptance criterion satisfied. It does not score wireframes or WF-R frames. UI was not driven with `adb` input or shell clicks. `avalonia-remote adb connect` performs GetCapabilities. It does not walk a storyboard.

Bearer tokens are not copied here. Marker lines below keep `devicePort`, `bridgeProtocol`, and token length only.

## Build

```powershell
dotnet build src/RideAudit.Client.Android/RideAudit.Client.Android.csproj -c Debug
```

Result: 0 warnings, 0 errors, elapsed 00:02:15.44.

Debug APK `src/RideAudit.Client.Android/bin/Debug/net10.0-android/org.rideaudit.app-Signed.apk`:

| Field | Value |
| --- | --- |
| Bytes | 78844129 |
| SHA256 | `D1C33208A105DB060D9BC73D4250E84154BCAA8EF889D4FCAA56A22A9FC18FED` |
| LastWriteTimeUtc | 2026-10-01T01:00:09.8948833Z |

Zip entries present: `lib_Avalonia.RemoteControl.Runtime.dll.so`, `lib_Avalonia.RemoteControl.Protocol.dll.so`, and `lib_Microsoft.Extensions.DependencyInjection.dll.so` under `lib/arm64-v8a` and `lib/x86_64`, plus the DependencyInjection abstractions assembly. This pass did not repeat the Release build from `docs/receipts/android/20260930T201737Z-remotecontrol-080.md`. The Release APK was not installed.

## Install

```powershell
adb -s RFCW7078MVZ install -r -d src/RideAudit.Client.Android/bin/Debug/net10.0-android/org.rideaudit.app-Signed.apk
adb -s adb-ZD222QH58Q-DiWqEr._adb-tls-connect._tcp install -r -d src/RideAudit.Client.Android/bin/Debug/net10.0-android/org.rideaudit.app-Signed.apk
```

Both printed `Success` and exited 0. The Moto install began 2026-10-01T01:01:22.4802576Z and finished 2026-10-01T01:01:39.2868437Z. The Fold install finished immediately before that, also `Success`, exit 0.

After install, `dumpsys package org.rideaudit.app` on both phones: `versionName=0.1.0`, `versionCode=1`, `flags=[ DEBUGGABLE HAS_CODE ALLOW_CLEAR_USER_DATA ALLOW_BACKUP ]`. Device-local `lastUpdateTime`: Fold `2026-09-30 20:01:13`, Moto `2026-09-30 20:01:38`.

## Devices

`adb devices -l` at 2026-10-01T01:06:11Z:

| Phone | Model | getprop ro.serialno | adb serial used | Build display |
| --- | --- | --- | --- | --- |
| Fold 4 | SM-F936U `q4qsqw` | RFCW7078MVZ | RFCW7078MVZ | BP2A.250605.031.A3.F936USQSAIZH2 |
| motorola edge 2024 | motorola edge 2024 `avatrn_g` | ZD222QH58Q | adb-ZD222QH58Q-DiWqEr._adb-tls-connect._tcp | W1UANS36H.29-25-2-6 |

A second wireless line, `adb-ZD222QH58Q-DiWqEr (2)._adb-tls-connect._tcp`, returned the same `ro.serialno` ZD222QH58Q and the same model. Probes used the line without ` (2)`.

Activity component from `cmd package resolve-activity --brief org.rideaudit.app`: `org.rideaudit.app/crc64ab21e1e23ed2ea51.MainActivity`.

## Desktop tool

Global `sharpninja.avalonia.remotecontrol.tool` 0.7.4 did not update. Uninstall failed with access denied on `C:\Users\kingd\.dotnet\tools\.store\sharpninja.avalonia.remotecontrol.tool\0.7.4`. A side-by-side 0.8.0 install was used instead and was not committed:

```powershell
dotnet tool install SharpNinja.Avalonia.RemoteControl.Tool --version 0.8.0 --tool-path F:\GitHub\rideaudit-rc080\artifacts\avalonia-remote-080
```

Connect command shape:

```powershell
F:\GitHub\rideaudit-rc080\artifacts\avalonia-remote-080\avalonia-remote.exe adb connect --serial <adb-serial> --package org.rideaudit.app --host-port <port> --keep-forward
```

Windows PowerShell did not surface a numeric process exit code (`ExitCode` stayed empty). The runs were not the 60 second timeout path. The tool text and `adb forward --list` are the attach evidence.

## Shared probe steps

For each phone:

1. `adb shell am force-stop org.rideaudit.app`, wait 2 seconds, `run-as org.rideaudit.app cat files/avalonia-remote-control.json`.
2. Push a sentinel marker to `/data/local/tmp/rc080-stale.json` and copy it with `run-as` onto `files/avalonia-remote-control.json`. Sentinel JSON: `devicePort` 65535, token `STALE-SENTINEL-PORT0` (length 20), `bridgeProtocol` `arc-protobuf-v1`.
3. `adb logcat -c`, then `adb shell am start -n org.rideaudit.app/crc64ab21e1e23ed2ea51.MainActivity`.
4. Read the marker again. Record logcat tag `RideAuditRemote`, `/proc/net/tcp`, and `dumpsys window` `mCurrentFocus`.
5. `avalonia-remote adb connect` as above, then `adb forward --list`.
6. `adb shell am stack remove <id>` only for the RootTask whose sole package was `org.rideaudit.app`.
7. Read the marker again. `avalonia-remote adb cleanup --serial <adb-serial> --host-port <port>`.

`/data/local/tmp/rc080-stale.json` was removed on both phones after the follow-up. `adb forward --list` was empty after both cleanups.

## Fold 4

Force-stop left the previous marker in place: `devicePort` 34447, `bridgeProtocol` `arc-protobuf-v1`, token length 64. Force-stop did not delete it.

Sentinel plant: `devicePort` 65535, token length 20, sentinel true.

After `am start`: `devicePort` 40321, token length 64, sentinel false. Logcat pid 13988 at 09-30 20:06:18 device local:

```text
RideAuditRemote: Starting Avalonia.RemoteControl loopback bridge.
RideAuditRemote: Avalonia.RemoteControl bound loopback port 40321.
RideAuditRemote: Avalonia.RemoteControl loopback bridge is listening.
```

`mCurrentFocus` was `org.rideaudit.app/crc64ab21e1e23ed2ea51.MainActivity`.

`/proc/net/tcp` LISTEN (`0A`) line: `0100007F:9D81`, uid 10380. Hex `9D81` is 40321. That is `127.0.0.1:40321`.

`avalonia-remote` text:

```text
ADB forward ready.
Serial: RFCW7078MVZ
Endpoint: http://127.0.0.1:47110/
Protocol: 1.0
Audit identity: remote-client
Frame streaming: supported
Remote input: supported
Connection profile saved for desktop client.
```

`adb forward --list`: `RFCW7078MVZ tcp:47110 tcp:40321`.

`am stack remove 504` (package `org.rideaudit.app` only) exited 0. Two seconds later `run-as` cat printed `cat: files/avalonia-remote-control.json: No such file or directory` (exit 1). `pidof org.rideaudit.app` was empty. Cleanup removed `tcp:47110`.

Startup replaced the sentinel with an OS-assigned port. Focused stack removal deleted the marker without `rm`.

## motorola edge 2024, first pass

Force-stop left `devicePort` 47100, token length 64. That is the previous fixed port, still on disk. Force-stop did not delete it.

Sentinel plant matched the Fold plant. After `am start`: `devicePort` 39259, token length 64, sentinel false. Logcat pid 29510:

```text
RideAuditRemote: Avalonia.RemoteControl bound loopback port 39259.
RideAuditRemote: Avalonia.RemoteControl loopback bridge is listening.
```

`mCurrentFocus` was `com.openai.chatgpt/com.openai.chatgpt.MainActivity`, not RideAudit. The RideAudit task still existed (`RootTask id=6310`).

`/proc/net/tcp` LISTEN line: `0100007F:995B`, uid 10323. Hex `995B` is 39259.

`avalonia-remote` text matched the Fold capabilities result, with serial `adb-ZD222QH58Q-DiWqEr._adb-tls-connect._tcp` and endpoint `http://127.0.0.1:47111/`. `adb forward --list`: `adb-ZD222QH58Q-DiWqEr._adb-tls-connect._tcp tcp:47111 tcp:39259`.

`am stack remove 6310` exited 0. `pidof` was empty, and the marker was still present at `devicePort` 39259, token length 64. The unfocused stack removal ended the process and left the file. That file is the stale marker the next start is supposed to clear. Cleanup removed `tcp:47111`.

## motorola edge 2024, focused follow-up

Before the next `am start`, the leftover marker was still `devicePort` 39259, token length 64.

After `am start`: `devicePort` 41093, token length 64. Startup replaced the leftover 39259 marker. Logcat pid 31166:

```text
RideAuditRemote: Avalonia.RemoteControl bound loopback port 41093.
RideAuditRemote: Avalonia.RemoteControl loopback bridge is listening.
```

`mCurrentFocus` on the first try was `org.rideaudit.app/crc64ab21e1e23ed2ea51.MainActivity`. `pidof` was 31166.

`avalonia-remote` text again reported `ADB forward ready`, `Protocol: 1.0`, `Frame streaming: supported`, and `Remote input: supported`, endpoint `http://127.0.0.1:47111/`. `adb forward --list`: `adb-ZD222QH58Q-DiWqEr._adb-tls-connect._tcp tcp:47111 tcp:41093`.

`am stack remove 6312` exited 0. Marker cat then failed with `No such file or directory`. `pidof` was empty. Cleanup removed `tcp:47111`.

This `/proc/net/tcp` check was not repeated for port 41093. The marker, the log line, and the forward target agree on 41093.

## Host tests

```powershell
dotnet test tests/RideAudit.Client.Android.AiUnit.Tests/RideAudit.Client.Android.AiUnit.Tests.csproj -c Debug --filter FullyQualifiedName~RemoteBridgeMarkerTests|FullyQualifiedName~PhaseBGateTests|FullyQualifiedName~DeviceSerialChoiceTests
```

Passed 18, Failed 0, Skipped 0, Total 18, Duration 298 ms. The run finished inside the probe window that ended 2026-10-01T01:06:58.9852389Z.

## What was observed about marker deletion

| Event | Fold | Moto |
| --- | --- | --- |
| `am force-stop` | Marker remained (port 34447) | Marker remained (port 47100) |
| Startup after a planted `devicePort` 65535 sentinel | Replaced with 40321 | Replaced with 39259 |
| Startup after a leftover live marker | Not a separate step | Leftover 39259 replaced with 41093 |
| `am stack remove` while RideAudit was `mCurrentFocus` | File deleted | File deleted (follow-up, port 41093) |
| `am stack remove` while another app was focused | Not run | Process ended, file remained (port 39259) |

Port 0 was not written as `devicePort`. Each live marker port was the port in the `RideAuditRemote` bind line. The bind-failure `DeleteMarker` path was not induced. Both binds succeeded, so that exception path did not run on these phones.

The tool reported `Connection profile saved for desktop client` on each successful connect. The profile file was not found under the checked locations (`%USERPROFILE%\.avalonia-remote`, `%APPDATA%\avalonia-remote`, `%LOCALAPPDATA%\avalonia-remote`, `%APPDATA%\SharpNinja`). It was not opened and the token was not copied. After the focused stack removals both marker files were gone, so those tokens were no longer the live bridge tokens.

## Not claimed

- A numeric `avalonia-remote` exit code.
- Execution of the bind-failure marker delete.
- Storyboard steps, frame compares, wireframe scores, or WF-R.
- A Release install.
- Google Play publication.
- MCP acceptance criteria marked satisfied.
