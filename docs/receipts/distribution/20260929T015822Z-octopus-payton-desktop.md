# Octopus RideAudit to PAYTON-DESKTOP

Date: 2026-09-29T01:58:22Z. Operator host: PAYTON-LEGION2. SPDX: GPL-2.0-only.

FR-RIDE-063 / UC-RIDE-032 / TR-RIDE-DEPLOY-001 / TR-RIDE-DEPLOY-002 / TEST-RIDE-038 / TEST-RIDE-040.

This is a live Octopus deploy receipt. It is not GHCR, not Play Store, and not a claim that Omarchy+ngrok (`:18080`, FR-RIDE-064) satisfies FR-RIDE-063.

## Instance (new container)

The default `octopus-legion2-octopus-server-1` instance on `192.168.0.149:8066` stayed up. Its task engine was previously observed stuck (`MaxConcurrentTasks=0`, tasks queued since 2026-09-27). This release did not use that instance.

A **new** Octopus stack `octopus-rideaudit` was created on the live Linux Docker host (SSH alias `PAYTON-DESKTOP` → `192.168.0.149`, hostname `PAYTON-OMARCHY`). That host is the lab PAYTON-DESKTOP Docker target. This receipt does not claim the Windows 11 Tentacle named PAYTON-DESKTOP is healthy.

After a recreate without `MASTER_KEY`, Server crash-looped (`Failed to decrypt the Octopus Server certificate`). This session reset only `octopus-rideaudit_*` volumes, persisted `MASTER_KEY` in the remote `.env` and in `~\.creds\octopus-rideaudit.cred.xml` (DPAPI, not git), and brought Server back healthy.

| Fact | Value |
| --- | --- |
| New container created | yes (`octopus-rideaudit-octopus-1`) |
| Image | `octopusdeploy/octopusdeploy:2026.2.13260` |
| Node | `payton-desktop-rideaudit` |
| InstallationId | `7c323750-954d-43f4-944e-0dfa318ccce4` |
| HTTP API / portal | `http://192.168.0.149:18066` |
| Tentacle comms | `192.168.0.149:19112` |
| SQL | host `127.0.0.1:1404` (container `octopus-rideaudit-db-1`) |
| Worker | `octopus-rideaudit-tentacle-1` (`octopusdeploy/tentacle`), polling, role `rideaudit-host` |
| Target | `Machines-1` `PAYTON-DESKTOP-DOCKER` `TentacleActive` health `Healthy` |

## License note

The new container boots on the free license (no serial, zero deployment targets). `Apply-RideAuditOctopusLicense.ps1` copied subscription **LicenseText** from the default instance (`text-len=824`, `serial-present=True` on source and dest). License XML, serials, and API keys are not recorded here. The default instance license was left in place.

## Project / release / deployment

| Object | Id |
| --- | --- |
| Project | `Projects-1` RideAudit |
| Environment | `Environments-1` Development |
| Channel | `Channels-1` |
| Release (failed) | `Releases-1` `0.1.0-a8511b5-20260928205506` `Deployments-1` `ServerTasks-11` Failed |
| Release (success) | `Releases-2` `0.1.0-a8511b5-20260928205822` |
| Deployment | `Deployments-2` |
| Task | `ServerTasks-12` Success |

Portal: `http://192.168.0.149:18066/app#/Spaces-1/projects/rideaudit/deployments/Deployments-2`

`Releases-1` failed in 6 seconds because a CRLF-normalization `sed s/\r$//` was interpreted by GNU sed as “strip a trailing `r`”, turning `command -v docker` into `command -v docke`. Scripts now strip `\x0d` only. That failure is not a green.

## Images (local Docker engine only)

Built on the host daemon via the Tentacle `docker.sock` mount from `deploy/containers/admission/Dockerfile` and `deploy/containers/counsel/Dockerfile`. Tags are local. `docker images` showed no registry host. No GHCR push or pull as the distribution path. Base images may come from `mcr.microsoft.com/dotnet/*`.

| Name | Local image id |
| --- | --- |
| `rideaudit-admission:octopus` | `sha256:17a16024c8f5772019847db3b7eed796791a62d016a0de7e73cc5e7cf4587f4e` |
| `rideaudit-counsel:octopus` | `sha256:28a6b1e7299ca580c609f0e6bd253737bb8e038486c584ec6d4efa3214866536` |

Repo digest JSON was not captured (Go template quoting on the inspect command). Absence of a `ghcr.io` repository name on `docker images` is the recorded fact. Do not invent a GHCR digest.

Compose project `rideaudit-octopus` (not `rideaudit-omarchy`):

- `rideaudit-octopus-admission-1` `192.168.0.149:28080->8080/tcp`
- `rideaudit-octopus-counsel-1` `192.168.0.149:28081->8080/tcp`

## DESKTOP probe

From PAYTON-LEGION2 immediately after `ServerTasks-12` Success:

- `GET http://192.168.0.149:28080/` → HTTP 200, 102 bytes
- Body: `RideAudit admission gRPC. Contract authority: grpc-protobuf. OpenAPI is a non-authoritative companion.`
- `GET http://192.168.0.149:28081/` → HTTP 200

Independent Omarchy interim loopback `GET http://127.0.0.1:18080/` from the same host still returned HTTP 200. That stack was not composed down. It is not this receipt’s CD path.

## Repeat

From PAYTON-LEGION2 repository root, with `~\.creds\octopus-rideaudit.cred.xml` present:

```powershell
pwsh -NoProfile -File deploy/octopus/Invoke-RideAuditOctopusRelease.ps1
```

If Server is missing or crash-looping on a lost master key:

```powershell
pwsh -NoProfile -File deploy/octopus/Provision-RideAuditOctopusContainer.ps1 -ResetData
```

`-ResetData` wipes only `octopus-rideaudit_*` volumes.

## What this is not

- Not a GHCR green.
- Not Play Store publication.
- Not satisfaction of FR-RIDE-064 (ngrok) by this work.
- Not a claim that `octopus-legion2` can run tasks.
- Not a claim that the Windows PAYTON-DESKTOP Tentacle is online.
