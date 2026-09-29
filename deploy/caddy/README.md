# Lab Caddy TLS in front of RideAudit

GPL-2.0-only.

This is a lab edge. Caddy terminates HTTPS with its internal CA and reverse-proxies the Octopus RideAudit stack:

| Listener | Upstream |
| --- | --- |
| `https://192.168.0.149:28443` | container `rideaudit-octopus-admission-1:8080` (also published as `http://192.168.0.149:28080`) |
| `https://192.168.0.149:28444` | container `rideaudit-octopus-counsel-1:8080` (also published as `http://192.168.0.149:28081`) |

`remote-up.sh` creates Docker network `rideaudit-caddy-edge` if needed and connects the two running containers to it. That connect does not recreate them. The TLS ports are published on `192.168.0.149` the same way `:28080` and `:28081` are, so the LAN path is not limited to host-local sockets.

Clients that connect to the IP without SNI (Windows schannel does this) still receive the certificate because the Caddyfile sets `default_sni 192.168.0.149`. The issuer is Caddy's local authority. It is not a public CA and not a purchased certificate. It is not installed into the host trust store (`skip_install_trust`). Plaintext `:28080`, `:28081`, and Omarchy loopback `:18080` stay up. ngrok HTTPS is a separate tunnel and is not this edge.

`octopus-legion2-octopus-api-tls-1` (`caddy:2.10.0-alpine` on `192.168.0.149:8445`) is the Octopus API terminator. This stack does not edit that Caddyfile and does not `docker compose down` Octopus, SQL, or that container.

The image bits are the copy already used by `octopus-legion2-octopus-api-tls-1` (`sha256:ae4458638da8e1a91aafffb231c5f8778e964bca650c8a8cb23a7e8ac557aa3c`). On this host that image is untagged (`caddy` / `<none>`). `remote-up.sh` retags that local id as `caddy:2.10.0-alpine` and refuses to pull.

```powershell
pwsh -NoProfile -File deploy/caddy/Apply-RideAuditCaddy.ps1
```
