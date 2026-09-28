# Admission container

GPL-2.0-only. gRPC on .NET 10.

The authoritative API is `src/RideAudit.Protos/`. `artifacts/server-api/openapi.yaml` is a non-authoritative companion.

## Run locally (Development fixtures)

Fixtures do not contact Bitcoin and do not call Google Play. Production refuses both flags.

```bash
export PATH="$HOME/.dotnet:$PATH"
export ASPNETCORE_ENVIRONMENT=Development
export RIDEAUDIT_ALLOW_INSECURE_DEV_HTTP=true
export RIDEAUDIT_OTS_CALENDAR=documented-fixture
export RIDEAUDIT_PLAY_INTEGRITY=fixture
export ASPNETCORE_URLS=http://127.0.0.1:8080
dotnet run --project src/RideAudit.Server.Admission
```

The process serves gRPC over HTTP/2. Clients need cleartext HTTP/2 support. A browser GET of `/` returns a plain-text authority notice.

`RIDEAUDIT_OTS_CALENDAR=documented-fixture` records chain id `fixture-btc-ots` and a `fixture:` transaction reference. That is not a live Bitcoin transaction. Omitting the variable fails closed and admits nothing.

`RIDEAUDIT_PLAY_INTEGRITY=fixture` accepts only `fixture.v1.` tokens. Any other configuration rejects attestation as unverifiable.

Public ingest accepts `RIDESEAL1` envelopes and untouched device `RAES` envelopes. A RAES body is stored as submitted. Admission does not re-encode it as `RIDESEAL1` and does not decrypt it. The RAES receipt core must carry the SHA-256 of the envelope bytes, the active policy, and the fixture attestation binding. Court working-copy decrypt opens `RIDESEAL1` and already-escrowed `RAES` after the same M-of-N release. Public ingest still does not decrypt.

## Container sketch

```bash
docker build -f deploy/containers/admission/Dockerfile -t rideaudit-admission:local .
```

The image sets `ASPNETCORE_ENVIRONMENT=Production` and `RIDEAUDIT_EDGE_TLS=true`. The edge must terminate TLS 1.2 or newer. The process refuses to start in Production if fixture calendars (OTS or L2) or fixture Play Integrity decoders are enabled, and it refuses Production HTTP without either a certificate path or the edge-TLS acknowledgement.

PAYTON-LEGION2 Docker Desktop returns HTTP 500, so images are built on PAYTON-OMARCHY. See [../../omarchy/README.md](../../omarchy/README.md). A lab image `rideaudit-admission:local` was tagged on Omarchy from commit `dcb31bf` and was not started. That is not a CD green.

## Counsel, ingest, and privacy

Those RPCs are on this same host. See [../counsel/README.md](../counsel/README.md). Documented L2 fixtures are opt-in via `RIDEAUDIT_L2_CALENDAR=documented-fixture` and are not live Base, Polygon, or Bitcoin transactions. Dev, Staging, and Prod CD receipts are `not-run`.
