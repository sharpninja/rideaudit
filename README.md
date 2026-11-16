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

Counsel, ingest, and privacy RPCs are on the same host. L2 profiles need `RIDEAUDIT_L2_CALENDAR=documented-fixture` and still record `fixture:` references, not live chain transactions. See [deploy/containers/counsel/README.md](deploy/containers/counsel/README.md). Container CD is Octopus Deploy to LAB-OMARCHY (FR-RIDE-063). Do not use GHCR. If the default Octopus container is out of licenses, create a new Octopus container on LAB-OMARCHY. Octopus receipts are `not-run` until a named instance and target exist ([docs/receipts/distribution/cd-receipts.md](docs/receipts/distribution/cd-receipts.md)). Omarchy plus ngrok is interim admission hosting.

```bash
dotnet test RideAudit.sln
```

See [docs/artifacts/INDEX.md](docs/artifacts/INDEX.md).
