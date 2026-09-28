# RideAudit clients

Avalonia UI 12 clients for PLAN-RIDEAUDIT-001-ANDROID. License: GPL-2.0-or-later (`/LICENSE`).

| Project | Role |
| --- | --- |
| `RideAudit.Client.Android` | Android dual-phone capture host |
| `RideAudit.Client.Desktop` | Desktop court / counsel viewer host (Windows, Linux, macOS TFM) |
| `RideAudit.Shared.Ui` | Shared Avalonia UI 12 views |
| `RideAudit.Bt` | Bluetooth driver / passenger roles |
| `RideAudit.Video` | On-device composite, spider graph, quotas |
| `RideAudit.Seal` | Seal-at-collect |
| `RideAudit.PlayIntegrity` | Fail-closed Play Integrity gate |
| `RideAudit.Viewer` | ViewerSession, VerificationReport, fail-closed decrypt |
| `RideAudit.Client.Contracts` | Interim gRPC stubs (`RideAudit.V1`) |
| `RideAudit.Licensing` | GPL notices and honest Play/source scaffold |

## Test

```bash
dotnet test tests/RideAudit.Client.Tests/RideAudit.Client.Tests.csproj
```

Partitions: TEST-RIDE-013, 019, 020, 025, 026, 027, 028, 033, 034, 035.

## Desktop

```bash
dotnet run --project src/RideAudit.Client.Desktop
```

## Android

```bash
dotnet workload install android
dotnet build src/RideAudit.Client.Android/RideAudit.Client.Android.csproj
```

Play Integrity is not live in this tree. The Android host fail-closes until a real token provider is injected. See `src/RideAudit.Client.Android/README.md`.

Do not treat `client-distribution-manifest.json` as a Play Store receipt. `playStore.published` is false.
