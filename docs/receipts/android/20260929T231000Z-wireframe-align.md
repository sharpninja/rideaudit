# Capture screens aligned to the wireframes

TimestampUtc: 2026-09-29T23:10:00Z

Device: Fold 4 `RFCW7078MVZ` (`SM_F936U`), USB, transport id 7. Debug APK `org.rideaudit.app-Signed.apk` installed over the previous debug build. Activity `org.rideaudit.app/crc64ab21e1e23ed2ea51.MainActivity`.

This records that the Android capture shell now follows WF-01 through WF-08 in `docs/ux/assets/wireframes` and `docs/ux/wireframes`. The sequence index is `docs/ux/assets/wireframes/README.md`. There is no `_manifest.md` in that folder.

This does not satisfy a functional requirement or acceptance criterion. `isSatisfied` stays false. Pixel agreement is not claimed. The baseline comparer still stretches the full phone-bezel SVG onto the screenshot, and the bottom About dock sits outside the SVG footer. Desktop review screens (WF-R) were not rebuilt and are not on this capture client.

## What landed

`WireframeIcon` draws the approved icon paths as Avalonia `Path` nodes. The role screen has the shield mark, wordmark, GPL-2.0 and Play Integrity badges, driver and passenger cards, vehicle `VA-1042`, and Continue. Discover, confirm, driver dashboard, passenger capture, seal, submit, and fail-closed use the same card, badge, and icon structure. Body font stays 16. The wordmark stays 28. Copyright and the third-party credit list stay on About, opened from the bottom-panel About button.

`ScreenId` is a 16px strip in the page background color so the accessibility tree still exposes `WF-01` through `WF-08` and it does not cover the wordmark. Headless `TestRide035ShellTests` passed 12 after that move. Debug Android build: 0 warnings, 0 errors.

## Fold navigation

| Step | ScreenId | Shot | SHA256 |
| --- | --- | --- | --- |
| Launch | WF-01 | wf01-role.png | 4149C63703CD895B60362DBC94770F389BE59830E6269E3235C4943FB70AB1F6 |
| Driver, Continue | WF-02 | after-continue.png | FD519D85C54FE1901021449988546167C4E7F23207D0C6A078A017B302928A91 |
| Peer | WF-03 | wf03-pair.png | 50B1E1B3045B828AB2B30A9259EA8E295AC5D05229F1EB3463525AAFAA5BC5A0 |
| Confirm | WF-04 | wf04-driver.png | E3721801C2631C5D2D922F4A33C07F5005254B40731E58193366676AA716FAAA |
| Start | WF-08 | after-start.png | C8734B5E76797B781BC05E3A54DE563065A5353B92E5F7E9870E202C7EFA1ED2 |
| Passenger path | WF-05 | wf05-passenger.png | D09C35937001FF31D3424E3CC76B3AF5C24A5BAC8AC839E08F9516C044598E33 |
| About | ABOUT | about.png | 87578CDD8D3CAD710A64D71382E58B37C15FD20D0FC3D791D49F932E4D5C507A |

WF-02 pairing status text is `RideAudit Bluetooth discovery on android-ble. No Lyft private API.` WF-04 link line is `Peer link: not a live radio peer`. Readiness rows use the wireframe OK chrome. That chrome is not a live Bluetooth, Play Integrity, or storage measurement.

Start did not open WF-06. It fail-closed to WF-08. FailClosedText: `CAMERA_UNAVAILABLE: CAMERA_UNAVAILABLE: Android Camera2 frame pipeline is not implemented. Refusing fixture frames. | ATTESTATION_FAILED: Attestation token is missing.` WF-06 and WF-07 were not shown on this device. Seal and submit are not successful live captures.

About still shows the copyright and the third-party list, including the SkiaSharp 3.119.4 row, and Back.

## Edge

Motorola edge 2024 mDNS still advertises `adb-ZD222QH58Q-DiWqEr` at `192.168.0.137:42825`. `adb connect` returned Windows 10061 (connection refused). The debug APK was not installed on the Edge. `adb pair` was not run. Result: FAIL.

## Not claimed

Pixel agreement, perceptual agreement, usability agreement, review screens on the phone, WF-06 or WF-07 as live screens, a live peer, Play Integrity success, admission Health success, Caddy closure, OTS, hardware HSM, or Play publication.
