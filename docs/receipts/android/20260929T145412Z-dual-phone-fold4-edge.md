# Android device session: Fold 4 primary, motorola edge 2024 secondary

TimestampUtc: 2026-09-29T14:54:12Z
Host: PAYTON-LEGION2
Workspace: F:\GitHub\rideaudit
Source HEAD when this receipt was written: 8550105b1355f719e5f0f2ce8980153bcd8d1dc9
Android `src/` diff from APK build base 3c93980bbc18fc31d65e275bab2cb9ab3f596b97 to that HEAD: empty
SPDX: GPL-2.0-only

This is a lab runtime receipt. It is not a Google Play publication. It does not close live OTS, Caddy edge TLS, or hardware HSM. It does not mark AC-UC-025-001 satisfied.

Shared UI title font stays `FontSize="20"` in `src/RideAudit.Shared.Ui/Views/MainView.axaml`. No larger-font edit was present in this worktree or on `cursor/fold4-android-validation-8aa2`. That file was left unchanged.

## adb device lines

After `adb connect 192.168.0.137:42825`:

```
RFCW7078MVZ            device product:q4qsqw model:SM_F936U device:q4q transport_id:7
192.168.0.137:42825    device product:avatrn_g model:motorola_edge_2024 device:avatrn transport_id:10
adb-ZD222QH58Q-DiWqEr._adb-tls-connect._tcp device product:avatrn_g model:motorola_edge_2024 device:avatrn transport_id:9
```

Three adb lines, two phones. The IP line and the mDNS line both report serial `ZD222QH58Q`.

## Primary: Samsung Galaxy Z Fold 4

| Field | Value |
| --- | --- |
| Role in this session | Driver (primary) |
| Manufacturer | samsung |
| Model | SM-F936U |
| Product / device | q4qsqw / q4q |
| Serial | RFCW7078MVZ |
| Build display id | BP2A.250605.031.A3.F936USQSAIZH2 |
| Build id | BP2A.250605.031.A3 |
| Incremental | F936USQSAIZH2 |
| Release / SDK | 16 / 36 |
| Transport | USB |
| Active display | Cover, 904x2316, density 420, state ON |
| Inner display | 1812x2176, state OFF |

Bluetooth `settings get global bluetooth_on` was `0` on the first Fold probe. `adb shell svc bluetooth enable` printed `enable: Success`. The setting then read `1` and stayed `1` through the dual-phone taps. The app was force-stopped and started again after that so the radio probe ran with the radio on.

## Secondary: motorola edge 2024

| Field | Value |
| --- | --- |
| Role in this session | Passenger (secondary) |
| Manufacturer | motorola |
| Model | motorola edge 2024 |
| Product / device | avatrn_g / avatrn |
| Serial | ZD222QH58Q |
| Build display id | W1UANS36H.29-25-2-6 |
| Release / SDK | 16 / 36 |
| Display | 1080x2400 |
| Bluetooth setting | `bluetooth_on=1` before install |

### Wireless debugging

`adb devices -l` already listed `adb-ZD222QH58Q-DiWqEr._adb-tls-connect._tcp` as `device` before the pair command.

`adb pair 192.168.0.137:41939` with the operator pairing code returned:

```
error: protocol fault (couldn't read status message): No error
```

Exit code 1. That command did not print a successful pair.

`adb mdns services` then listed:

```
adb-ZD222QH58Q-DiWqEr	_adb-tls-connect._tcp	192.168.0.137:42825
```

`adb connect 192.168.0.137:42825` printed `connected to 192.168.0.137:42825`. Both phones stayed in `adb devices` through the UI taps. The second phone was not missing.

## APK

Command: `dotnet build src/RideAudit.Client.Android/RideAudit.Client.Android.csproj -c Release -f net10.0-android`

Result: Build succeeded. 0 Warning(s). 0 Error(s). Time Elapsed 00:02:30.91.

| Field | Value |
| --- | --- |
| Path | `src/RideAudit.Client.Android/bin/Release/net10.0-android/org.rideaudit.app-Signed.apk` |
| Bytes | 47511601 |
| SHA256 | 5AFBD6C9A1E0C505D0984FD92D9E2E4567E09D404D831D69EAD28F55FD2CC17A |
| Package | org.rideaudit.app |
| versionName | 0.1.0 |
| versionCode | 1 |
| Activity | org.rideaudit.app/crc64ab21e1e23ed2ea51.MainActivity |

The APK is not committed.

`adb -s RFCW7078MVZ install -r` succeeded. Device `lastUpdateTime=2026-09-29 09:44:10`.
`adb -s 192.168.0.137:42825 install -r` succeeded. Device `lastUpdateTime=2026-09-29 09:52:46`.

`pm grant` was used for CAMERA, ACCESS_FINE_LOCATION, ACCESS_COARSE_LOCATION, BLUETOOTH_CONNECT, BLUETOOTH_SCAN, and BLUETOOTH_ADVERTISE on both packages. That is an adb grant, not a user dialog inside the app.

`am start -a android.intent.action.MAIN -c android.intent.category.LAUNCHER` with package `org.rideaudit.app` failed on the Fold with `unable to resolve Intent`. `cmd package resolve-activity` for that same action and category later resolved `org.rideaudit.app/crc64ab21e1e23ed2ea51.MainActivity`. Explicit `am start -n` of that component succeeded on both phones. The home-screen icon path was not proven.

## Screens exercised

String proof is the uiautomator dump. Dumps live under `docs/receipts/android/dual-20260929/`. Dark-theme PNGs were pulled from the same sessions. Pixel samples of an earlier Fold cover capture showed a header near RGB 22,29,43, which matches the dark theme. The XML text nodes are the readable record.

`CaptureShellView.OnDiscover` sets the pairing status when `RadioAvailable` is true. It does not call `IDiscoveryBus.Scan` or `Advertise`. A status string is not a confirmed peer.

### Fold 4, driver

| Step | ScreenId | Evidence |
| --- | --- | --- |
| Launch after Bluetooth enable | WF-01 | `fold-01-launch.xml`. Pairing status `Bluetooth discovery idle`. |
| Tap DRIVER | WF-04 | `fold-02-driver.xml`. `Role confirmed: driver coordinator`. |
| Tap Discover | WF-02 | `fold-03-discover.xml`. `RideAudit Bluetooth discovery on android-ble. No Lyft private API.` |
| Tap Start | WF-08 | `fold-04-start.xml`. Start disabled. |

Launch fail-closed banner, both phones, after the radio was on:

`PRODUCTION_UNAVAILABLE: UnavailablePlayIntegrityClient: Play Integrity API was not called. | AdmissionUnavailable: RIDEAUDIT_ADMISSION_ADDRESS/BEARER are not set. | UnavailableHsmEscrow: hardware HSM is not configured. | CanonicalAdmission: Canonical admission requires tenant and driver identity. Preflight hash-only mapping is not the production contract.`

Fold driver start fail-closed text:

`CAMERA_UNAVAILABLE: CAMERA_UNAVAILABLE: Android Camera2 frame pipeline is not implemented. Refusing fixture frames. | ATTESTATION_FAILED: Attestation token is missing.`

The Bluetooth-off banner from the earlier single-phone pass was gone after the radio enable and process restart.

### motorola edge 2024, passenger

| Step | ScreenId | Evidence |
| --- | --- | --- |
| Launch | WF-01 | `moto-01-launch.xml` |
| Tap PASSENGER | WF-05 | `moto-02-passenger.xml`. `Role confirmed: passenger compositor`. Spider text `Spider graph armed for telematics overlay`. |
| Tap Discover | WF-02 | `moto-03-discover.xml`. Same android-ble status string. No peer address. |
| Tap Start | WF-08 | `moto-04-start.xml`. `Only the driver phone may start the session.` Start disabled. |

The spider line is a text label. It is not a drawn telematics graph and not a video composite.

### Earlier Fold-only pass

`docs/receipts/android/fold4-20260929/` is the first pass, one USB Fold, Bluetooth off. That pass showed WF-01, WF-04, WF-05, and WF-08 on the same phone, including `BT_DISABLED` when Discover ran with the radio off. It is not the dual-phone session.

## Proven

- Two physical phones were attached at the same time: SM-F936U over USB and motorola edge 2024 over wireless debugging `192.168.0.137:42825`.
- The same Release APK installed on both.
- Avalonia capture shell ran on both. Control classes in the dumps include `MainView`, `CaptureShellView`, `TextBlock`, and `Button`.
- Fold cover showed WF-01, then WF-04 as driver, then WF-02, then WF-08.
- Edge showed WF-01, then WF-05 as passenger, then WF-02, then WF-08 refusing passenger start.
- With Bluetooth on, the production banner no longer listed the radio as unavailable.
- Driver start fail-closed on the real camera and Play attestation seams.
- Passenger start fail-closed because only the driver phone may start.

## Not proven

- `adb pair` success for port 41939. The command failed. The phone was already authorized.
- A RideAudit Bluetooth peer, advertise, or scan. The Discover button does not call those methods.
- One shared session clock, seal, or submission across the two phones.
- Camera frames. The pipeline text says it is not implemented and refuses fixture frames.
- Play Integrity token, admission, or hardware HSM.
- The unfolded inner display of the Fold.
- A home-screen launcher start. Explicit component start is what ran.
- Google Play listing or publication.
- Live OTS, Caddy edge TLS, or HSM closure.

## AC ids touched

| ID | This session |
| --- | --- |
| AC-UC-025-001 | Still deferred. Reason in `docs/receipts/ac-coverage/explicit-deferrals.txt` now cites this receipt. `isSatisfied` stays false. |
| AC-RIDE-056-001 | Runtime of the Avalonia Android client was exercised. Ledger "covered" means the id is named in test source. This session does not mark the AC satisfied. |
| AC-RIDE-056-002 | Primary screens that ran are Avalonia controls. The AC is not marked satisfied. |

Plan checkboxes were not changed.
