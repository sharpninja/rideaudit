# ngrok ingress for RideAudit admission

GPL-2.0-only. Implements FR-RIDE-064 / UC-RIDE-033 / TR-RIDE-EDGE-001.

This is an operator tunnel. It is not Octopus CD, not GHCR, not Play Store, and not a Caddy TLS cutover.

Product CD is FR-RIDE-063: Use Octopus Deploy. Build containers and deploy to LAB-OMARCHY. If you are out of licenses on the default container, create a new Octopus container on LAB-OMARCHY. Do not use GHCR. The only live Octopus receipt, `docs/receipts/distribution/20260929T015822Z-octopus-payton-desktop.md`, records the earlier PAYTON-DESKTOP Docker host (`192.168.0.149`). No LAB-OMARCHY Octopus receipt exists yet. This wrapper publishes the live admission front door. It does not replace the Octopus receipt.

## Current vs prior interim

| Role | Host | Binding | Status |
| --- | --- | --- | --- |
| Canonical terminator | LAB-OMARCHY (SSH alias; Linux hostname on this box is LAB-OMARCHY) | `192.168.1.182:28080` -> Octopus admission container `8080` | Current public tunnel target |
| Prior interim | LAB-OMARCHY loopback | `127.0.0.1:18080` -> `rideaudit-omarchy` admission | Kept running. Not the canonical tunnel. |

`127.0.0.1:28080` is not the bind. Compose publishes admission on `192.168.1.182:28080` only. Counsel stays on `192.168.1.182:28081` and is not the public tunnel.

SSH aliases `LAB-OMARCHY`, `LAB-OMARCHY`, and `OMARCHY` resolve to `192.168.1.182`. Do not invent a second machine from the alias list.

## Secrets

- Authtoken lives on PAYTON-LEGION2 at `~/.creds/ngrok.yml` (keys: `version`, `authtoken`).
- Start scripts copy that file over SSH stdin to a non-git path on the tunnel host: `~/.config/ngrok/ngrok.yml`.
- Receipts may say `token sourced from ~/.creds/ngrok.yml` only. Never commit or print the token.
- Checked-in example: [ngrok.yml.example](ngrok.yml.example) uses `YOUR_NGROK_AUTHTOKEN` only.

`deploy/omarchy/.env` does not hold the ngrok token. Omarchy compose stays loopback-only on `:18080`.

## Start / stop from PAYTON-LEGION2

From the repository root:

```powershell
pwsh -NoProfile -File deploy/omarchy/ngrok/Start-Ngrok.ps1
pwsh -NoProfile -File deploy/omarchy/ngrok/Stop-Ngrok.ps1
```

`Start-Ngrok.ps1` defaults:

- `-SshHost LAB-OMARCHY`
- `-Addr 192.168.1.182:28080`

It:

1. Confirms admission `GET http://192.168.1.182:28080/` on that host returns HTTP 200.
2. Installs official Linux amd64 ngrok under `~/.local/bin` if missing, or reuses an existing binary.
3. Copies the token file with `Copy-OmarchyStdinFile` (no `scp`).
4. Starts a systemd user unit `rideaudit-ngrok.service` when a user bus is available, otherwise `nohup`.
5. Reads the public URL from `http://127.0.0.1:4040/api/tunnels` on the tunnel host.
6. Probes that URL from LEGION2. Expected body includes `RideAudit admission`. If the probe is not HTTP 200, the script fail-closes and does not advertise a URL.

Remote login shell is pwsh. All remote commands use `exec /usr/bin/bash --noprofile --norc` via [OmarchySsh.ps1](../OmarchySsh.ps1).

## Restart

```powershell
pwsh -NoProfile -File deploy/omarchy/ngrok/Stop-Ngrok.ps1
pwsh -NoProfile -File deploy/omarchy/ngrok/Start-Ngrok.ps1
```

Stop first so the prior interim agent (`ngrok http 127.0.0.1:18080`) is killed before the canonical agent starts. `remote-stop.sh` stops both the `:28080` agent and the prior `:18080` agent, but only processes started with this tree's `--config` and `--log` paths. Other ngrok tunnels under the same account are left running. A nohup start records its config path in `~/.local/state/rideaudit-ngrok/ngrok.config`, so a later stop still finds the agent after `RIDEAUDIT_NGROK_CONFIG` changes. An agent started before that file existed is still recognized through the PID file, the backend recorded in `ngrok.addr` and this tree's `--log` path. A start with a different config stops that agent before launching a new one, and a stop with a different config still kills it. A running agent is reused only when its live command line has the current `--config`. If one of our agents survives `kill -9`, the stop prints `FAIL_CLOSED=ngrok_still_running` and keeps the state files. A systemd-mode agent is tracked by the unit's MainPID; if the unit stays active or that process survives `systemctl --user stop`, the stop prints `FAIL_CLOSED` and exits 1.

On the tunnel host, if the systemd user unit is active:

```bash
export XDG_RUNTIME_DIR=/run/user/$(id -u)
systemctl --user restart rideaudit-ngrok.service
```

Restart alone reuses the unit written by the last `remote-start.sh`. After this tree change, run `Start-Ngrok.ps1` once so the unit `ExecStart` address becomes `192.168.1.182:28080`.

## Probe notes

Admission's documented companion HTTP front door is `GET /` on `192.168.1.182:28080`. A live tunnel must return that same 200 body. Free ngrok interstitial pages are skipped on the LEGION2 probe with `ngrok-skip-browser-warning: 1`.

This wrapper starts `ngrok http 192.168.1.182:28080`. gRPC clients that require HTTP/2 end-to-end can be added later; do not advertise a URL unless the HTTP health probe succeeds.

## Fail closed

If admission is down, ngrok is missing, the token file is missing, the agent API has no public URL, or the LEGION2 probe fails, Start-Ngrok stops the tunnel (unless `-KeepOnFailure`) and throws. Docs and receipts must not list a dead URL as live.

A free ngrok account typically has one reserved `*.ngrok-free.dev` hostname. If PAYTON-LEGION2 already has `ngrok` publishing a different local port (for example MCP Swagger on `:7147`), Start-Ngrok stops that local agent so the DESKTOP host can terminate the admission tunnel. Pass `-KeepConflictingLocal` to refuse instead of displacing. Do not enable ngrok pooling across mixed backends. MCP on `http://PAYTON-LEGION2:7147` stays on the LAN.
