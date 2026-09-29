# Octopus Deploy RideAudit to PAYTON-DESKTOP

GPL-2.0-only. FR-RIDE-063 / UC-RIDE-032 / TR-RIDE-DEPLOY-001 / TR-RIDE-DEPLOY-002 / TEST-RIDE-038 / TEST-RIDE-040.

Operator direction: Use Octopus Deploy. Build containers and deploy to PAYTON-DESKTOP. If you are out of licenses on the default container, create a new Octopus container on PAYTON-DESKTOP. Do not use GHCR.

This path is the product CD. Omarchy loopback plus ngrok (`deploy/omarchy`, `:18080`) stays interim only (FR-RIDE-064) and does not satisfy FR-RIDE-063.

## What this tree does

From PAYTON-LEGION2:

1. Sync the git tree to the live Linux Docker host over SSH stdin (same constraint as `deploy/omarchy`: the login shell is pwsh, so scp/sftp break).
2. Talk to the existing Octopus Server container (`octopus-legion2-octopus-server-1`, image `octopusdeploy/octopusdeploy:2026.2.13260`) on that host.
3. Ensure project **RideAudit**, a Linux SSH deployment target with Docker, a script process that builds from `deploy/containers/*`, and a release.
4. Octopus runs `remote-build-and-run.sh` on the target. Images stay on the local Docker engine. No GHCR push or pull as the distribution path. Microsoft Container Registry base images (`mcr.microsoft.com/dotnet/*`) are allowed.
5. Compose project `rideaudit-octopus` binds admission on `192.168.0.149:28080` and counsel on `192.168.0.149:28081`. That is a different stack from `rideaudit-omarchy` on `127.0.0.1:18080`. Do not `docker compose down` Octopus, SQL, Caddy, or the interim Omarchy stack.

## Host facts (verify, do not invent)

SSH aliases `PAYTON-DESKTOP`, `PAYTON-OMARCHY`, and `OMARCHY` currently resolve to `192.168.0.149` as `sharpninja`. The Linux hostname is `PAYTON-OMARCHY`. DNS/mDNS also answers `PAYTON-DESKTOP.local` at that address.

The default Octopus instance already has a polling Tentacle named **PAYTON-DESKTOP**. That target is Windows 11. Its last successful connection in this work was 2026-09-23, health `HasWarnings`. WinRM `:5985`/`:5986` from LEGION2 did not connect. Grok Bot local-execution on Windows DESKTOP may be offline.

This path therefore registers a **Linux SSH target** (`PAYTON-DESKTOP-LINUX`, role `rideaudit-host`) at the Octopus container's docker-bridge gateway (`172.19.0.1`) so Calamari SSHs to the same machine that already runs Docker and the default Octopus container. That is the live PAYTON-DESKTOP Docker host for this lab. It is not a claim that the Windows Tentacle is healthy.

Default instance license observed from `/api/licenses/licenses-current` (no key material recorded here): subscription valid through 2027-07-07, project limit 10, machine limit 10. Nine projects existed before RideAudit.

The default node `legion2-octopus` was found with `MaxConcurrentTasks=0` and every task queued since 2026-09-27 (including after raising the cap to 5: `RunningTasks` stayed 0). That is a blocked default container. FR-RIDE-063 then requires a **new Octopus container** on PAYTON-DESKTOP:

```powershell
pwsh -NoProfile -File deploy/octopus/Provision-RideAuditOctopusContainer.ps1
pwsh -NoProfile -File deploy/octopus/Invoke-RideAuditOctopusRelease.ps1 -ApiBase http://192.168.0.149:18066
```

The new stack is `octopus-rideaudit` on `192.168.0.149:18066` (HTTP) and `:19112` (Tentacle comms). It does not steal `8066`/`8444`/`11112`. Creds are written to `~\.creds\octopus-rideaudit.cred.xml` (DPAPI, not git). The default `octopus-legion2-*` containers stay up.

## Secrets (never git)

| File | Use |
| --- | --- |
| `C:\Users\kingd\.creds\octopus-desktop.cred.xml` | API key. `ServerUrl` is `https://payton-desktop:8444`. |
| `C:\Users\kingd\.creds\octopus-legion2.cred.xml` | Alternate API key for `https://payton-legion2:8444` (not the live API from this host). |
| `C:\Users\kingd\.creds\octopus-payton.cred.xml` | Admin + API for the older `http://payton-desktop:8065` URL (port closed). |
| `C:\Users\kingd\.creds\payton-omarchy.yaml` | SSH user for the Linux host. Host field may say `PAYTON-DESKTOP`; verify aliases. |
| `C:\Users\kingd\.creds\paytondesktop.cred.xml` | Windows `kingd` WinRM cred. Unused while WinRM is down. |
| `C:\Users\kingd\.ssh\id_ed25519_payton_desktop` | Preferred Octopus SSH account material (BatchMode from LEGION2). |

Scripts create an Octopus SSH account at runtime from the key file (or the yaml password). They do not print or commit those values.

## Repeatable commands (PAYTON-LEGION2)

```powershell
pwsh -NoProfile -File deploy/octopus/Sync-RideAuditTree.ps1
# Optional runtime-image fallback (LEGION2 publish, DESKTOP docker build):
# pwsh -NoProfile -File deploy/octopus/Sync-RideAuditTree.ps1 -WithPublish
pwsh -NoProfile -File deploy/octopus/Invoke-RideAuditOctopusRelease.ps1
```

`Invoke-RideAuditOctopusRelease.ps1` syncs, ensures infrastructure, creates a release, deploys to the **Development** environment, and probes `http://192.168.0.149:28080/`. Expect HTTP 200 and body `RideAudit admission gRPC. Contract authority: grpc-protobuf...`.

Useful URLs after a real release (IDs come from the script stdout, not from this README):

- Portal: `https://payton-desktop:8444/app#/Spaces-1/projects/rideaudit`
- HTTP API/UI: `http://192.168.0.149:8066/app#/Spaces-1/projects/rideaudit`
- Admission health: `http://192.168.0.149:28080/`
- Counsel (same host, role env): `http://192.168.0.149:28081/`

## Build path

Primary: `deploy/containers/admission/Dockerfile` and `deploy/containers/counsel/Dockerfile` on the target Docker engine.

Fallback: `deploy/omarchy/Dockerfile.runtime` against a linux-x64 publish tree synced by `-WithPublish`. That is the documented successor already used for Omarchy lab images. It is still an Octopus-orchestrated local build, not GHCR.

## License fallback

If the default instance is out of licenses **or cannot run tasks**, do **not** switch to GHCR. Run `Provision-RideAuditOctopusContainer.ps1` and point `Invoke-RideAuditOctopusRelease.ps1 -ApiBase` at `http://192.168.0.149:18066`. Record the new container name in the receipt. Existing `octopus-legion2-*` containers must stay up.

## What this is not

- Not Omarchy interim compose on `:18080`.
- Not a GHCR green.
- Not Play Store publication.
- Not a claim that the Windows PAYTON-DESKTOP Tentacle is online.
