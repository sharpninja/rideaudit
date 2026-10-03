# PLAN-RIDEPIPE-001 Camera2, H.264, and telematics design (BDPv4)

**Plan ID:** PLAN-RIDEPIPE-001
**Status:** Draft. Awaiting Payton approval. This document is not an agreement, and it does not satisfy any FR, AC, or TODO.
**Branch:** cursor/dual-phone-fold-moto-8aa2
**Process:** Byrd Dev Process v4
**Machine:** PAYTON-LEGION2
**Date:** 2026-10-03

Implementation of Camera2 encode, a production H.264 composite, and an Android telematics source does not start in this revision. New requirement sentences below are proposals only. Approved FR text is unchanged.

No product source was edited for these three pipelines. `AndroidCameraSource.Capture` still throws `CAMERA_UNAVAILABLE` / `FR-RIDE-041` with "Android Camera2 frame pipeline is not implemented. Refusing fixture frames." when `CameraAvailable` is true.

Hostile validation was not run. There is no pass, done, green, or complete claim for these pipelines.

## Module boundaries

| Module | Owns | Does not own |
| --- | --- | --- |
| `RideAudit.Video` | `ICameraSource`, `SourceStream`, `TelematicsSample`, `PassengerCompositor`, `CompositeSourceContainer`, and the new contract types below | Android APIs, Play, escrow |
| `RideAudit.Client.Android` | Platform adapters that implement those contracts | Changing approved container bytes or FR text |
| `RideAudit.Capture` / `CaptureRuntime` | Calling the adapters that are already wired | Play attestation and HSM escrow in this plan |
| `docs/ux` | Unchanged | Wireframe edits and WF-R |

Out of scope: Play attestation, HSM escrow, microphone, wireframes, WF-R.

## 1. Camera2 encode

The existing signature stays. No new camera interface.

```csharp
public interface ICameraSource
{
    string SourceKind { get; }
    bool CameraAvailable { get; }
    SourceStream Capture(CameraCaptureRequest request);
}
```

`AndroidCameraSource` remains the Android implementation. `SourceKind` stays `android-camera2`.

When `CameraAvailable` is true, a later implementation of `Capture` will:

- Select the Camera2 device for `CameraCaptureRequest.Facing`.
- Capture real frames into `SourceStream.Payload`.
- Fill `CameraMetadata` (camera id, facing, width, height) and `FrameTimestamps`.
- Return no fixture bytes.

When the camera cannot be opened, or CAMERA is not granted, `Capture` still throws `RideAuditFailClosedException` with `CAMERA_UNAVAILABLE` and `FR-RIDE-041`. It does not invent frames.

`android.permission.CAMERA` is already declared in `src/RideAudit.Client.Android/Properties/AndroidManifest.xml`. This design adds no further camera permission. A later runtime request may show the system dialog. That dialog stays for Payton. App UI is AvaloniaRemote only. No adb input tap.

Requirement stance: do not rewrite `FR-RIDE-041`. Pending `AC-RIDE-041-001` and `AC-RIDE-041-002` already require collection and per-stream camera metadata and timestamps. No new FR is proposed for Camera2. Coding still waits until Payton accepts this plan.

## 2. Production H.264 composite

The canonical container stays as it is.

- `CompositeSourceContainer.CodecId` is `rideaudit-composite-source-container-v2`.
- `Encode` writes `media=source-payload-container;not-h264;not-bt-transport`.
- `PassengerCompositor.Codec` uses that id. `CompressionId` is `none-canonical`.
- Existing tests expect the bytes to contain `not-h264`.

Replacing `PassengerCompositor` or `CompositeSourceContainer` with H.264 would change that contract. This plan does not rewrite `FR-RIDE-045`, `FR-RIDE-055`, or `FR-RIDE-221`.

H.264 is a separate package, proposed as draft text and not created in the requirement store:

- Proposed id: `FR-RIDE-223` (unused in the 2026-10-03 RIDE list; confirm again before any create).
- Proposed title: Production H.264 composite beside the canonical container.
- Proposed description: After both phones supply real Camera2 frames, an Android encoder produces an H.264 access-unit sequence. The canonical source container stays `rideaudit-composite-source-container-v2` and stays marked not-h264. The H.264 bytes are a separate package. Fixture frames are refused.
- Proposed criteria, all unsatisfied, not stored until Payton agrees:
  - `AC-RIDE-223-001`: The encoder returns no package when either stream has zero frame timestamps.
  - `AC-RIDE-223-002`: Encoder output is H.264 and is not written into `CompositeSourceContainer`.
  - `AC-RIDE-223-003`: `PassengerCompositor.Codec` stays `rideaudit-composite-source-container-v2`.

Contract types, design only, not added to the product tree in this revision:

```csharp
public sealed record H264CompositeRequest(
    string CompositeId,
    SourceStream Driver,
    SourceStream Passenger,
    OverlayManifest Overlay);

public sealed record H264CompositePackage(
    string CompositeId,
    string Codec,
    byte[] AnnexB,
    int Width,
    int Height);

public interface IProductionH264Encoder
{
    string CodecKind { get; }
    H264CompositePackage Encode(H264CompositeRequest request);
}
```

`IProductionH264Encoder` belongs in `RideAudit.Video`. A later `AndroidH264Encoder` belongs in `RideAudit.Client.Android`. Until `FR-RIDE-223` is agreed, nothing calls it. An unavailable encoder throws fail-closed. It does not relabel canonical container bytes as H.264.

## 3. Android telematics source

`TelematicsSample` stays unchanged:

```csharp
public sealed record TelematicsSample(
    TimeSpan SessionTime,
    double AccelX,
    double AccelY,
    double AccelZ,
    double SpeedMps,
    double? GpsLatitude,
    double? GpsLongitude,
    double? Obd2SpeedKph);
```

New contract, design only:

```csharp
public interface ITelematicsSource
{
    string SourceKind { get; }
    bool SensorsAvailable { get; }
    TelematicsSample Read(TimeSpan sessionTime);
}
```

`ITelematicsSource` belongs in `RideAudit.Video`. A later `AndroidTelematicsSource` belongs in `RideAudit.Client.Android`.

- Accelerometer: `SensorManager` linear or raw accelerometer samples fill `AccelX`, `AccelY`, and `AccelZ`. Pending `FR-RIDE-055` already names the accelerometer spider graph. This plan does not rewrite `FR-RIDE-055`. No extra manifest permission is required for the accelerometer.
- `Obd2SpeedKph` stays null. This design has no OBD source.
- GPS fields stay null until a separate requirement is agreed. `FR-RIDE-202` is geolocation masking, not an order to capture coordinates. This plan does not rewrite `FR-RIDE-202`.
- `SpeedMps` stays 0 while GPS is unset, so speed is not invented from a missing location fix.
- If the accelerometer is missing, `Read` throws fail-closed. It does not invent samples.

Proposed draft, not created and not agreed:

- Proposed id: `FR-RIDE-224`.
- Proposed title: Android location samples for telematics.
- Proposed description: When this text is agreed, `AndroidTelematicsSource` fills `GpsLatitude`, `GpsLongitude`, and `SpeedMps` from the platform location provider. Exact coordinates remain subject to `FR-RIDE-202` masking in the UI. A missing permission fails closed and does not invent coordinates.
- `ACCESS_FINE_LOCATION` and `ACCESS_COARSE_LOCATION` are already in the manifest. A later runtime dialog stays for Payton. No adb input tap.

Until `FR-RIDE-224` is agreed, the only telematics fields a later implementation may fill are the three accelerometer axes.

## BDPv4 phases

None of these phases are done.

| Phase | Work | State |
| --- | --- | --- |
| 0 | This design and MCP TODO `PLAN-RIDEPIPE-001` | Recorded as draft. Stop for Payton approval before any new FR text is treated as agreed. |
| 1 | Create `FR-RIDE-223` and `FR-RIDE-224` only after Payton agrees those sentences. Do not edit approved FR bodies. | Not started |
| 2 | Tests red: Camera2 returns real frame bytes on a granted device and still throws when the camera cannot open; H.264 package is separate from `not-h264`; accelerometer `Read` fills axes and leaves GPS null | Not started |
| 3 | Implementation in the modules above | Not started |
| 4 | Fold `RFCW7078MVZ` evidence. AvaloniaRemote for app UI. Leave any system permission dialog for Payton | Not started |
| 5 | Hostile AGREE only when accuracy and completeness are both at least 98. Request and response jsonl go in the session log | Not started |

No FR, AC, or TODO is marked done before that hostile gate passes.
