# PAYTON-OMARCHY deploy from PAYTON-LEGION2

GPL-2.0-only. SSH host aliases: `PAYTON-OMARCHY`, `OMARCHY`, `PAYTON-DESKTOP` → `192.168.0.149` as `sharpninja`.

This is a lab deploy path. It is not a continuous-delivery receipt and not a production cutover.

## What LEGION2 cannot do

Docker Desktop on PAYTON-LEGION2 answers HTTP 500 on both `npipe:////./pipe/dockerDesktopLinuxEngine` and `npipe:////./pipe/docker_engine`. `com.docker.service` is stopped (`WIN32_EXIT_CODE 1077`). Image builds therefore happen on Omarchy, where Engine 29.7.2 is healthy.

Do not record a LEGION2 Docker/CD green from this tree.

## Layout on Omarchy

Default checkout: `/home/sharpninja/github/rideaudit`.

Existing stacks on that host (Octopus, SQL Server, Caddy) listen on `8066`, `8444`, `8445`, `11112`, and `1433`. RideAudit compose binds only `127.0.0.1:18080` so it does not steal those ports. Do not `docker compose down` those other projects.

## Sync the tree from LEGION2

From the repository root on LEGION2:

```powershell
pwsh -NoProfile -File deploy/omarchy/Sync-FromLegion2.ps1
```

The script creates a git bundle of `HEAD`, copies it over SSH, and clones or fast-forwards `/home/sharpninja/github/rideaudit`. It does not delete remote volumes or other repositories.

After this branch is merged, the coordinator may instead `git clone` / `git pull` from GitHub on Omarchy if that host has credentials.

## Build images on Omarchy (no start)

```bash
ssh PAYTON-OMARCHY 'bash ~/github/rideaudit/deploy/omarchy/remote-build.sh'
```

That tags `rideaudit-admission:local` and `rideaudit-counsel:local`. Both Dockerfiles publish the same `RideAudit.Server.Admission` host. Counsel is a role flag, not a second codebase. Building an image is not a CD green.

## Coordinator cutover (after merge, not this agent)

1. Confirm the checkout is the merged commit.
2. Copy `deploy/omarchy/env.example` to `/home/sharpninja/github/rideaudit/deploy/omarchy/.env` and set live calendars / Play Integrity only if those endpoints exist. Leave them unset to fail closed. Production refuses `documented-fixture` and `RIDEAUDIT_PLAY_INTEGRITY=fixture`.
3. `cd ~/github/rideaudit && docker compose -f deploy/omarchy/compose.yaml --env-file deploy/omarchy/.env up -d --build`
4. Probe `127.0.0.1:18080` on Omarchy. Public ingest still requires a real bearer and fails closed without OTS/Play configuration.
5. Put TLS 1.2+ on the existing Caddy edge if this host should be reachable beyond loopback. The image already sets `RIDEAUDIT_EDGE_TLS=true`.

This agent does not start those containers as a production cutover.
