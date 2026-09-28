# RideAudit

**Author:** Sharp Ninja

Lyft / rideshare driver telematics audit application.

Process: Byrd Dev Process v4 (BDPv4).
MCP workspace: registered via mcpserver-grok-plugin.

## Stack

- Android and desktop apps: **Avalonia UI 12**
- Backend API: **gRPC on .NET 10 containers**

## Artifacts

- [Android client](artifacts/android/) (ART-RIDE-ANDROID-001) - Avalonia UI 12 dual-phone client under [src/](src/); historical Kotlin scaffold archived at `artifacts/android/legacy-kotlin/`
- [Client build notes](src/README.md) - how to build and test the Avalonia Android and desktop clients. Not road-ready and not Play-published
- [Headrest phone mount (3D)](artifacts/hardware/headrest-phone-mount/) (ART-RIDE-MOUNT-001) - parametric OpenSCAD dual-phone mount
- [Server API](artifacts/server-api/) (ART-RIDE-API-001) - gRPC on .NET 10 containers; interim OpenAPI companion

See [docs/artifacts/INDEX.md](docs/artifacts/INDEX.md).
