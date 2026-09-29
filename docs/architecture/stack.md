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
| Deployment | **Containers via Octopus Deploy** | Use Octopus Deploy. Build containers and deploy to PAYTON-DESKTOP. If you are out of licenses on the default container, create a new Octopus container on PAYTON-DESKTOP. Do not use GHCR. |

Public sealed submit remains ciphertext-only at ingest. No decrypt at public-server ingest.

## Continuous delivery (operator 2026-09-28)

This is the binding CD path. It supersedes GHCR, GitHub Actions container-registry, and any plan text that treated Octopus license exhaustion as deferral or out of scope.

1. **Use Octopus Deploy.** Octopus builds RideAudit admission, counsel, and related images from `deploy/containers` (or the documented successor) and deploys them to **PAYTON-DESKTOP** (FR-RIDE-063).
2. **License exhaustion is not a deferral.** If the default Octopus container is out of licenses, create a new Octopus container on PAYTON-DESKTOP and continue from that instance (AC-RIDE-063-002, TR-RIDE-DEPLOY-002).
3. **Do not use GHCR.** Receipts must never claim a GHCR green. GitHub Actions container registry is not the distribution path.
4. **Public tunnel.** The canonical ngrok target is PAYTON-DESKTOP admission `192.168.0.149:28080` (FR-RIDE-064). Omarchy loopback `127.0.0.1:18080` is the prior interim and stays documented. Counsel `192.168.0.149:28081` is not the public tunnel.
5. **Honesty.** Octopus CD to PAYTON-DESKTOP is recorded in `docs/receipts/distribution/20260929T015822Z-octopus-payton-desktop.md` (`octopus-rideaudit`, not GHCR). That receipt is not Play publication and not a claim that every P11b acceptance row is closed. Lab Caddy TLS in front of that stack is a separate receipt, `docs/receipts/distribution/20260929T145508Z-caddy-edge-tls.md` (internal CA, not a public CA, not ngrok). Omarchy compose cutover receipts remain the prior interim path.

Authoritative requirements: `docs/Project/Additive-PostPlanning-Deploy-Ngrok-Batch.yaml`. Plan citation: PLAN-RIDEAUDIT-001 r3.4 §4.6.

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

Mappings live in `docs/Project/Requirements-Mappings-Batch.yaml`. Post-planning CD and ingress: FR-RIDE-063 / FR-RIDE-064 in `docs/Project/Additive-PostPlanning-Deploy-Ngrok-Batch.yaml`.

| Topic | FR | TR | TEST | UC |
| --- | --- | --- | --- | --- |
| Octopus Deploy CD to PAYTON-DESKTOP | FR-RIDE-063 | TR-RIDE-DEPLOY-001, TR-RIDE-DEPLOY-002 | TEST-RIDE-038, TEST-RIDE-040 | UC-RIDE-032 |
| ngrok ingress (canonical PAYTON-DESKTOP `:28080`; Omarchy `:18080` prior interim) | FR-RIDE-064 | TR-RIDE-EDGE-001 | TEST-RIDE-039, TEST-RIDE-040 | UC-RIDE-033 |
