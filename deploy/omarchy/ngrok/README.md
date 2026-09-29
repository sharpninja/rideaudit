# ngrok ingress for RideAudit admission

GPL-2.0-only. Implements FR-RIDE-064 / UC-RIDE-033 / TR-RIDE-EDGE-001.

This is an operator tunnel. It is not Octopus CD, not GHCR, not Play Store, and not a Caddy TLS cutover.

## Current vs target

| Role | Host | Binding | Status |
| --- | --- | --- | --- |
| Current terminator (interim) | PAYTON-OMARCHY | `127.0.0.1:18080` -> admission container `8080` | Lab loopback plus ngrok when this wrapper is live |
| Target terminator | PAYTON-DESKTOP | Same admission front door after FR-RIDE-063 Octopus CD | Not claimed by this tree |

Existing Omarchy SSH aliases (`PAYTON-OMARCHY`, `OMARCHY`, `PAYTON-DESKTOP`) currently resolve to the same lab host (`192.168.0.149` as `sharpninja`). Do not invent a second machine from the alias list. The Octopus CD target in FR-RIDE-063 remains PAYTON-DESKTOP; this wrapper stays honest that today's live bind is the Omarchy loopback.

## Secrets

- Authtoken lives on PAYTON-LEGION2 at `~/.creds/ngrok.yml` (keys: `version`, `authtoken`).
- Start scripts copy that file over SSH stdin to a non-git path on Omarchy: `~/.config/ngrok/ngrok.yml`.
- Receipts may say `token sourced from ~/.creds/ngrok.yml` only. Never commit or print the token.
- Checked-in example: [ngrok.yml.example](ngrok.yml.example) uses `YOUR_NGROK_AUTHTOKEN` only.

`deploy/omarchy/.env` does not hold the ngrok token. Compose stays loopback-only.

## Start / stop from PAYTON-LEGION2

From the repository root:

```powershell
pwsh -NoProfile -File deploy/omarchy/ngrok/Start-Ngrok.ps1
pwsh -NoProfile -File deploy/omarchy/ngrok/Stop-Ngrok.ps1
```

`Start-Ngrok.ps1`:

1. Confirms admission `GET http://127.0.0.1:18080/` on Omarchy returns HTTP 200.
2. Installs official Linux amd64 ngrok under `~/.local/bin` if missing, or reuses an existing binary.
3. Copies the token file with `Copy-OmarchyStdinFile` (no `scp`).
4. Starts a systemd user unit `rideaudit-ngrok.service` when a user bus is available, otherwise `nohup`.
5. Reads the public URL from `http://127.0.0.1:4040/api/tunnels`.
6. Probes that URL from LEGION2. Expected body includes `RideAudit admission`. If the probe is not HTTP 200, the script fail-closes and does not advertise a URL.

Remote login shell is pwsh. All remote commands use `exec /usr/bin/bash --noprofile --norc` via [OmarchySsh.ps1](../OmarchySsh.ps1).

## Restart

```powershell
pwsh -NoProfile -File deploy/omarchy/ngrok/Start-Ngrok.ps1
```

That restart is idempotent enough to replace a dead agent. Stop first if you need a clean supervisor restart:

```powershell
pwsh -NoProfile -File deploy/omarchy/ngrok/Stop-Ngrok.ps1
pwsh -NoProfile -File deploy/omarchy/ngrok/Start-Ngrok.ps1
```

On Omarchy, if the systemd user unit is active:

```bash
export XDG_RUNTIME_DIR=/run/user/$(id -u)
systemctl --user restart rideaudit-ngrok.service
```

## Probe notes

Admission's documented companion HTTP front door is `GET /` on the loopback mapping. A live tunnel must return that same 200 body. Free ngrok interstitial pages are skipped on the LEGION2 probe with `ngrok-skip-browser-warning: 1`.

This wrapper starts `ngrok http 127.0.0.1:18080` first. gRPC clients that require HTTP/2 end-to-end can be added later; do not advertise a URL unless the HTTP health probe succeeds.

## Fail closed

If admission is down, ngrok is missing, the token file is missing, the agent API has no public URL, or the LEGION2 probe fails, Start-Ngrok stops the tunnel (unless `-KeepOnFailure`) and throws. Docs and receipts must not list a dead URL as live.

A free ngrok account typically has one reserved `*.ngrok-free.dev` hostname. If PAYTON-LEGION2 already has `ngrok` publishing a different local port (for example MCP Swagger on `:7147`), Start-Ngrok stops that local agent so Omarchy can terminate the admission tunnel. Pass `-KeepConflictingLocal` to refuse instead of displacing. Do not enable ngrok pooling across mixed backends. MCP on `http://PAYTON-LEGION2:7147` stays on the LAN.
