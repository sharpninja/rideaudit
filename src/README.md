# RideAudit clients

Avalonia UI 12 clients for PLAN-RIDEAUDIT-001-ANDROID. Package license expression is GPL-2.0-only (`/LICENSE`, `Directory.Build.props`). Client source headers remain GPL-2.0-or-later. See `/NOTICE`.

| Project | Role |
| --- | --- |
| `RideAudit.Client.Android` | Android dual-phone capture host |
| `RideAudit.Client.Desktop` | Desktop court / counsel viewer host (Windows, Linux, macOS TFM) |
| `RideAudit.Shared.Ui` | Shared Avalonia UI 12 views |
| `RideAudit.Bt` | Bluetooth driver / passenger roles |
| `RideAudit.Video` | On-device composite, spider graph, quotas |
| `RideAudit.Client.Seal` | Device-boundary seal-at-collect (RAES). Not the server `RideAudit.Seal` envelope |
| `RideAudit.PlayIntegrity` | Fail-closed Play Integrity gate. Fixtures and stubs are not live Play tokens |
| `RideAudit.Viewer` | ViewerSession, VerificationReport, fail-closed decrypt |
| `RideAudit.Client.Contracts` | `ISealedAdmissionClient` over `src/RideAudit.Protos` |
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

Play Integrity is not live in this tree. The Android host fail-closes until a real token provider is injected. Fixture and stub tokens are labeled and are not Google Play Integrity JWTs. See `src/RideAudit.Client.Android/README.md`.

Submission messages are the authoritative `SubmitSealedRequest` / `ReceiptCore` types from `src/RideAudit.Protos/`. The in-process client is a preflight: `admitted` stays false, and the anchor stays pending with `live_bitcoin_metadata` false. No OpenTimestamps or Play receipt is fabricated. Device ciphertext stays in the client RAES envelope; this tree does not re-seal it as the server `RIDESEAL1` envelope.

Do not treat `client-distribution-manifest.json` as a Play Store receipt. `playStore.published` is false. Bluetooth discovery in tests is an in-memory bus, not a live radio. Camera and video bytes in tests are fixtures.
