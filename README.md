# RideAudit

**Author:** Sharp Ninja

Lyft / rideshare driver telematics audit application.

Process: Byrd Dev Process v4 (BDPv4).
MCP workspace: registered via mcpserver-grok-plugin.

## Stack

- Android and desktop apps: **Avalonia UI 12**
- Backend API: **gRPC on .NET 10 containers**

## Artifacts

- [Android client](artifacts/android/) (ART-RIDE-ANDROID-001) - Avalonia UI 12 target for dual-phone Bluetooth driver-rider pairing, Play Integrity, and seal-at-collect; historical Kotlin scaffold retained as a superseded placeholder
- [Headrest phone mount (3D)](artifacts/hardware/headrest-phone-mount/) (ART-RIDE-MOUNT-001) - parametric OpenSCAD dual-phone mount
- [Server API](artifacts/server-api/) (ART-RIDE-API-001) - authoritative protos in `src/RideAudit.Protos/`; OpenAPI is a non-authoritative companion

## Server (gRPC, .NET 10)

Local admission uses documented fixtures. They are not live Bitcoin and not Google Play. See [deploy/containers/admission/README.md](deploy/containers/admission/README.md).

```bash
export ASPNETCORE_ENVIRONMENT=Development
export RIDEAUDIT_ALLOW_INSECURE_DEV_HTTP=true
export RIDEAUDIT_OTS_CALENDAR=documented-fixture
export RIDEAUDIT_PLAY_INTEGRITY=fixture
export ASPNETCORE_URLS=http://127.0.0.1:8080
dotnet run --project src/RideAudit.Server.Admission
```

```bash
dotnet test RideAudit.sln
```

See [docs/artifacts/INDEX.md](docs/artifacts/INDEX.md).
