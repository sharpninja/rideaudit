# PAYTON-OMARCHY deploy from PAYTON-LEGION2

GPL-2.0-only. SSH host aliases: `PAYTON-OMARCHY`, `OMARCHY`, `PAYTON-DESKTOP` → `192.168.0.149` as `sharpninja`.

This is a lab deploy path. It is not a continuous-delivery receipt and not a production cutover.

## What LEGION2 cannot do

Docker Desktop on PAYTON-LEGION2 answers HTTP 500 on both `npipe:////./pipe/dockerDesktopLinuxEngine` and `npipe:////./pipe/docker_engine`. `com.docker.service` is stopped (`WIN32_EXIT_CODE 1077`). There is no GHCR push in this tree.

Preferred honest path: `dotnet publish` linux-x64 on LEGION2, stdin-copy the publish tree, then `docker build` the runtime image on Omarchy. Alternate path: sync the git bundle and run the full SDK Dockerfiles on Omarchy.

Do not record a LEGION2 Docker/CD green from this tree.

## Layout on Omarchy

Default checkout: `/home/sharpninja/github/rideaudit`.

Existing stacks on that host (Octopus, SQL Server, Caddy) listen on `8066`, `8444`, `8445`, `11112`, and `1433`. RideAudit compose binds only `127.0.0.1:18080` so it does not steal those ports. Do not `docker compose down` those other projects.

## Sync the tree from LEGION2

From the repository root on LEGION2:

```powershell
pwsh -NoProfile -File deploy/omarchy/Sync-FromLegion2.ps1
# Default remote path: /home/sharpninja/github/rideaudit
# Remote commands use bash --noprofile --norc so login snippets cannot rewrite paths.
```

The script creates a git bundle of `HEAD` and copies it over SSH stdin into `exec /usr/bin/bash --noprofile --norc`. `scp`/`sftp` cannot be used while the Omarchy login shell is pwsh: profile banners break the SFTP handshake (`Received message too long`). The remote side `git init`s if needed, fetches the bundle, and checks out `FETCH_HEAD`. It does not delete remote volumes or other repositories.

After this branch is merged, the coordinator may instead `git clone` / `git pull` from GitHub on Omarchy if that host has credentials.

## Preferred: publish on LEGION2, runtime image on Omarchy (no start)

```powershell
pwsh -NoProfile -File deploy/omarchy/Publish-Admission.ps1
pwsh -NoProfile -File deploy/omarchy/Sync-FromLegion2.ps1
pwsh -NoProfile -File deploy/omarchy/Sync-Publish.ps1
ssh PAYTON-OMARCHY "exec /usr/bin/bash --noprofile --norc -c 'bash /home/sharpninja/github/rideaudit/deploy/omarchy/remote-runtime-build.sh'"
```

That tags `rideaudit-admission:local` (and `rideaudit-counsel:local` as the same bits). Counsel is `RIDEAUDIT_SERVICE_ROLE=counsel` on the same host. Building an image is not a CD green.

## Alternate: full SDK rebuild on Omarchy (no start)

```bash
ssh PAYTON-OMARCHY "exec /usr/bin/bash --noprofile --norc -c 'bash /home/sharpninja/github/rideaudit/deploy/omarchy/remote-build.sh'"
```

## Coordinator cutover (after merge, not this agent)

Print the steps (safe):

```powershell
pwsh -NoProfile -File deploy/omarchy/Confirm-Cutover.ps1
```

After merge, the coordinator may pass `-ConfirmCutover` to run compose on loopback `:18080`. Leave calendars unset to fail closed. Production refuses `documented-fixture` and `RIDEAUDIT_PLAY_INTEGRITY=fixture`. Do not `docker compose down` Octopus/SQL/Caddy. Put TLS 1.2+ on the existing Caddy edge if this host should be reachable beyond loopback.

This agent does not pass `-ConfirmCutover`.

## ngrok (interim public URL)

Admission remains bound to Omarchy loopback `:18080`. The current public path is an ngrok tunnel started from LEGION2; the target host after Octopus CD is PAYTON-DESKTOP (FR-RIDE-063). Token via `~/.creds/ngrok.yml`, never git. See [ngrok/README.md](ngrok/README.md).

```powershell
pwsh -NoProfile -File deploy/omarchy/ngrok/Start-Ngrok.ps1
pwsh -NoProfile -File deploy/omarchy/ngrok/Stop-Ngrok.ps1
```

No GHCR. A down tunnel is not advertised as a live URL.
