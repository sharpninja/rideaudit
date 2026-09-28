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

Submission messages are the authoritative `SubmitSealedRequest` / `ReceiptCore` types from `src/RideAudit.Protos/`. `InterimInProcessAdmissionClient` is still a preflight: `admitted` stays false, and the anchor stays pending with `live_bitcoin_metadata` false. `GrpcSealedAdmissionClient` and `CaptureAdmissionChannel` are the deployed admission channel. Capture can submit an untouched RAES envelope through that client when a fixture `fixture.v1.` token, enrolled identity, and HSM escrow deposit are supplied. Admission does not re-encode that envelope as `RIDESEAL1` and public ingest does not decrypt it. Court working-copy decrypt can open escrowed RAES after M-of-N release. Fixture calendars and fixture Play tokens are not live Bitcoin, OpenTimestamps, L2, or Google Play Integrity.

`RIDEAUDIT_OTS_CALENDAR` accepts `documented-fixture` or an `https://` calendar URL. An unset or unknown value fail-closes and writes no transaction id. `RIDEAUDIT_L2_RPC` / `RIDEAUDIT_L2_CALENDAR` follow the same rule. A live calendar POST that returns pending proof does not admit the record and does not invent a txid.

Do not treat `client-distribution-manifest.json` as a Play Store receipt. `playStore.published` is false. Bluetooth discovery in tests is an in-memory bus unless a platform adapter is injected. The Android production composition root wires BLE/camera/Play/admission seams and fail-closes through `Unavailable*` when they are missing. `UnavailableDiscoveryBus` is not a silent APK success default. Windows lab adapters live in `RideAudit.Host.Windows` and never invent peers or frames.
