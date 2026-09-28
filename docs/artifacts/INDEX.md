# RideAudit artifacts index

**Author:** Sharp Ninja

License: GPL-2.0

| Artifact ID | Kind | Path |
|-------------|------|------|
| ART-RIDE-ANDROID-001 | android-client | [artifacts/android/](../../artifacts/android/) |
| ART-RIDE-MOUNT-001 | mechanical-3d (OpenSCAD) | [artifacts/hardware/headrest-phone-mount/](../../artifacts/hardware/headrest-phone-mount/) |
| ART-RIDE-API-001 | grpc-api | [artifacts/server-api/](../../artifacts/server-api/) |
| ART-RIDE-UX-001 | ux-wireframes-storyboards (mobile capture) | [docs/ux/](../ux/) |
| ART-RIDE-UX-REVIEW-001 | ux-wireframes-storyboards (desktop review) | [docs/ux/review-app/](../ux/review-app/) |

Stack decision: Avalonia UI 12 for Android and desktop apps; backend gRPC on .NET 10 containers. The Android artifact points at `src/RideAudit.Client.Android/` and archives historical Kotlin under `artifacts/android/legacy-kotlin/`. The server API artifact retains OpenAPI as an interim human-readable companion. See [docs/architecture/stack.md](../architecture/stack.md). BDPv4 stack FRs: FR-RIDE-056..062 (`docs/Project/Additive-Avalonia-Grpc-Stack-Batch.yaml`).

Each package contains an `ARTIFACT.yaml` with related FR-RIDE ids and GPL-2.0 notices.

## Process rules

- [Code generation and opposing-model hostile validation](../process/hostile-validation.md)
- [Code-generation procedure](../process/code-generation.md)
