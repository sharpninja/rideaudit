# RideAudit technology stack

Status: Decision recorded 2026-09-27.  
License: GPL-2.0 for in-scope application, schema, verification, and evidence-network software.

## Applications (UI)

All RideAudit **apps** use **Avalonia UI 12**:

| Surface | Stack | Notes |
| --- | --- | --- |
| Desktop court / counsel review app | Avalonia UI 12 | Windows, Linux, macOS (FR-RIDE-049 / section 3.6). Target of `ART-RIDE-UX-REVIEW-001`. |
| Android dual-phone client | Avalonia UI 12 | Driver coordinator and passenger compositor capture UX. Mobile wireframes live under `docs/ux/` (`ART-RIDE-UX-001`). |

Do not invent Lyft private APIs. Capture pairing is RideAudit device pairing only.

## Backend

| Layer | Stack | Notes |
| --- | --- | --- |
| Service API | **gRPC on .NET 10** | Sealed-only admission and counsel/review support services. |
| Deployment | **Containers** | Backend services ship as container images. |

Public sealed submit remains ciphertext-only at ingest. No decrypt at public-server ingest.

## Custody and verification (cross-cutting)

- Primary custody receipt path: **Bitcoin OpenTimestamps (OTS)** (`btc-ots`). See `blockchain-custody-receipts.md`.
- Desktop review: independently verify OTS/Bitcoin custody receipt, payload hashes, Play Integrity / signing cert + nonce/key binding, and escrow release authorization **before** decrypt or display. **Fail closed** on any failure (FR-RIDE-050, NFR-22).
- Decrypt only via court-authorized escrow / M-of-N dual-control release into an **expiring authorized working copy**. Never casual plaintext.

## Related artifacts

- Mobile UX: `docs/ux/` (`ART-RIDE-UX-001`)
- Desktop review UX: `docs/ux/review-app/` (`ART-RIDE-UX-REVIEW-001`)
- Android client package (scaffold path): `artifacts/android/` (`ART-RIDE-ANDROID-001`)
- Server API package: `artifacts/server-api/` (`ART-RIDE-API-001`)

## BDPv4 requirements (stack decision)

Additive batch: `docs/Project/Additive-Avalonia-Grpc-Stack-Batch.yaml` (Author: Sharp Ninja).

| Topic | FR | TR | TEST | UC |
| --- | --- | --- | --- | --- |
| Avalonia Android capture | FR-RIDE-056 | TR-RIDE-VIDEO-012 | TEST-RIDE-035 | UC-RIDE-025 |
| Avalonia desktop court viewer | FR-RIDE-057 | TR-RIDE-VIEW-005 | TEST-RIDE-035 | UC-RIDE-026 |
| Shared Avalonia / GPL-2.0 | FR-RIDE-058 | TR-RIDE-GPL-004 | TEST-RIDE-035 | UC-RIDE-027 |
| gRPC on .NET 10 containers | FR-RIDE-059 | TR-RIDE-SERVER-008 | TEST-RIDE-036 | UC-RIDE-028 |
| Proto/schema GPL-2.0 publish | FR-RIDE-060 | TR-RIDE-GPL-005 | TEST-RIDE-037 | UC-RIDE-029 |
| Fail-closed admission over gRPC | FR-RIDE-061 | TR-RIDE-SERVER-009 | TEST-RIDE-036 | UC-RIDE-030 |
| OpenAPI companion non-authoritative | FR-RIDE-062 | TR-RIDE-SERVER-010 | TEST-RIDE-037 | UC-RIDE-031 |

Mappings live in `docs/Project/Requirements-Mappings-Batch.yaml`.
