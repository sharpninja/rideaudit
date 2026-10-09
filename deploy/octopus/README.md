# Octopus Deploy RideAudit to LAB-OMARCHY

GPL-2.0-only. FR-RIDE-063 / UC-RIDE-032 / TR-RIDE-DEPLOY-001 / TR-RIDE-DEPLOY-002 / TEST-RIDE-038 / TEST-RIDE-040.

Operator direction: Use Octopus Deploy. Build containers and deploy to LAB-OMARCHY. If you are out of licenses on the default container, create a new Octopus container on LAB-OMARCHY. Do not use GHCR.

This path is the product CD. The canonical ngrok tunnel (`deploy/omarchy/ngrok`) targets this stack's admission `192.168.1.182:28080` (FR-RIDE-064). Omarchy loopback `:18080` is the prior interim and does not satisfy FR-RIDE-063.

## What this tree does

From PAYTON-LEGION2:

1. Sync the git tree to the live Linux Docker host over SSH stdin (same constraint as `deploy/omarchy`: the login shell is pwsh, so scp/sftp break). Shell scripts are stripped of CR so bash does not see `\r`.
2. Prefer the RideAudit Octopus instance `octopus-rideaudit` on `192.168.1.182:18066` when `~\.creds\octopus-rideaudit.cred.xml` exists. The default `octopus-legion2` instance on `:8066` stays up; its node was observed with a dead task engine (`MaxConcurrentTasks=0` / queued since 2026-09-27).
3. Ensure project **RideAudit**, environment **Development**, and a polling Tentacle (`LAB-OMARCHY-DOCKER`, role `rideaudit-host-lab-omarchy`) that talks to the host Docker engine through `/var/run/docker.sock`.
4. Octopus runs `remote-build-and-run.sh` on that Tentacle. Images stay on the local Docker engine. No GHCR push or pull as the distribution path. Microsoft Container Registry base images (`mcr.microsoft.com/dotnet/*`) are allowed.
5. Compose project `rideaudit-octopus` binds admission on `192.168.1.182:28080` and counsel on `192.168.1.182:28081`. That is a different stack from `rideaudit-omarchy` on `127.0.0.1:18080`. Do not `docker compose down` Octopus, SQL, Caddy, or the interim Omarchy stack.

## Host facts (verify, do not invent)

SSH aliases `LAB-OMARCHY`, `LAB-OMARCHY`, and `OMARCHY` currently resolve to `192.168.1.182` as `sharpninja`. The Linux hostname is `LAB-OMARCHY`. DNS/mDNS also answers `LAB-OMARCHY.local` at that address.

The default Octopus instance already has a polling Tentacle named **LAB-OMARCHY**. That target is Windows 11. Its last successful connection in this work was 2026-09-23, health `HasWarnings`. WinRM `:5985`/`:5986` from LEGION2 did not connect. Grok Bot local-execution on Windows DESKTOP may be offline.

SSH from an Octopus Server container to host `:22` was observed CLOSED (hairpin / bridge). Do not treat container-to-host SSH as the worker path. The worker is the **polling Tentacle container** with the host Docker socket.

Default instance license observed from `/api/licenses/licenses-current` (no key material recorded here): subscription valid through 2027-07-07, project limit 10, machine limit 10. Nine projects existed before this work on the default instance.

## New instance (required when the default task engine is dead)

```powershell
pwsh -NoProfile -File deploy/octopus/Provision-RideAuditOctopusContainer.ps1
# After a lost MASTER_KEY / crash-loop (recreate without the key):
pwsh -NoProfile -File deploy/octopus/Provision-RideAuditOctopusContainer.ps1 -ResetData
pwsh -NoProfile -File deploy/octopus/Invoke-RideAuditOctopusRelease.ps1
```

`-ResetData` wipes only `octopus-rideaudit_*` volumes. It does not touch `octopus-legion2-*`.

The new stack is `octopus-rideaudit`:

| Container | Role |
| --- | --- |
| `octopus-rideaudit-db-1` | SQL 2022 on host `127.0.0.1:1404` |
| `octopus-rideaudit-octopus-1` | Server HTTP `192.168.1.182:18066`, Tentacle comms `:19112` |
| `octopus-rideaudit-tentacle-1` | Polling worker, host `docker.sock`, role `rideaudit-host-lab-omarchy` |

`MASTER_KEY` is generated on LEGION2, stored in the remote `.env` and in `~\.creds\octopus-rideaudit.cred.xml` (DPAPI). Recreating Server without that key cannot decrypt the existing database (certificate decrypt crash-loop). Provision creates environment **Development** before starting the Tentacle (registration fails if that environment is missing). Shell CR-stripping uses `sed s/\x0d$//` — GNU `s/\r$//` strips a trailing letter `r` and breaks `docker`. The default `octopus-legion2-*` containers stay up.

A new container starts on the free license (zero deployment targets). `Apply-RideAuditOctopusLicense.ps1` copies the subscription **LicenseText** from the default instance. Serials and XML are not printed.

## Secrets (never git)

| File | Use |
| --- | --- |
| `C:\Users\kingd\.creds\octopus-rideaudit.cred.xml` | API key + master key for `http://192.168.1.182:18066`. |
| `C:\Users\kingd\.creds\octopus-lab-omarchy.cred.xml` | API key. `ServerUrl` is `https://LAB-OMARCHY:8444` (default instance). |
| `C:\Users\kingd\.creds\octopus-legion2.cred.xml` | Alternate API key for `https://payton-legion2:8444` (not the live API from this host). |
| `C:\Users\kingd\.creds\octopus-payton.cred.xml` | Admin + API for the older `http://LAB-OMARCHY:8065` URL (port closed). |
| `C:\Users\kingd\.creds\lab-omarchy.yaml` | SSH user for the Linux host. Host field may say `LAB-OMARCHY`; verify aliases. |
| `C:\Users\kingd\.creds\lab-omarchy-winrm.cred.xml` | Windows `kingd` WinRM cred. Unused while WinRM is down. |
| `C:\Users\kingd\.ssh\id_ed25519_lab_omarchy` | SSH account material if WorkerMode=Ssh (BatchMode from LEGION2). |

Scripts do not print or commit those values.

## Repeatable commands (PAYTON-LEGION2)

```powershell
pwsh -NoProfile -File deploy/octopus/Sync-RideAuditTree.ps1
# Optional runtime-image fallback (LEGION2 publish, DESKTOP docker build):
# pwsh -NoProfile -File deploy/octopus/Sync-RideAuditTree.ps1 -WithPublish
pwsh -NoProfile -File deploy/octopus/Invoke-RideAuditOctopusRelease.ps1
```

`Invoke-RideAuditOctopusRelease.ps1` syncs, waits for the Tentacle, creates a release, deploys to **Development**, and probes `http://192.168.1.182:28080/`. Expect HTTP 200 and body `RideAudit admission gRPC. Contract authority: grpc-protobuf...`.

Useful URLs after a real release (IDs come from the script stdout, not from this README):

- RideAudit Octopus: `http://192.168.1.182:18066/app#/Spaces-1/projects/rideaudit`
- Default Octopus (not the FR-063 runner): `http://192.168.1.182:8066/app#/Spaces-1`
- Admission health: `http://192.168.1.182:28080/`
- Counsel (same host, role env): `http://192.168.1.182:28081/`

## Build path

Primary: `deploy/containers/admission/Dockerfile` and `deploy/containers/counsel/Dockerfile` on the target Docker engine.

Fallback: `deploy/omarchy/Dockerfile.runtime` against a linux-x64 publish tree synced by `-WithPublish`. That is the documented successor already used for Omarchy lab images. It is still an Octopus-orchestrated local build, not GHCR.

If the Tentacle image has no Compose plugin, `remote-build-and-run.sh` falls back to `docker run` on `:28080`/`:28081`.

## License fallback

If the default instance is out of licenses **or cannot run tasks**, do **not** switch to GHCR. Run `Provision-RideAuditOctopusContainer.ps1` and `Invoke-RideAuditOctopusRelease.ps1`. Record the new container name in the receipt. Existing `octopus-legion2-*` containers must stay up.

## What this is not

- Not Omarchy interim compose on `:18080`.
- Not a GHCR green.
- Not Play Store publication.
- Not a claim that the Windows LAB-OMARCHY Tentacle is online.
