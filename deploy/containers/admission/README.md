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

PAYTON-LEGION2 Docker Desktop returns HTTP 500. The preferred lab path is `dotnet publish` linux-x64 on LEGION2 and `deploy/omarchy/Dockerfile.runtime` on PAYTON-OMARCHY. See [../../omarchy/README.md](../../omarchy/README.md). A lab runtime image is not a CD green.

Product CD is Octopus Deploy to PAYTON-DESKTOP (FR-RIDE-063). Use Octopus Deploy. Build containers and deploy to PAYTON-DESKTOP. If you are out of licenses on the default container, create a new Octopus container on PAYTON-DESKTOP. Do not use GHCR. The receipt on file is `docs/receipts/distribution/20260929T015822Z-octopus-payton-desktop.md`. This README is not that probe and is not Play publication.

## Counsel, ingest, and privacy

Those RPCs are on this same host. See [../counsel/README.md](../counsel/README.md). Documented L2 fixtures are opt-in via `RIDEAUDIT_L2_CALENDAR=documented-fixture` and are not live Base, Polygon, or Bitcoin transactions. Octopus to PAYTON-DESKTOP receipts are `not-run`. Historical Dev/Staging/Prod placeholders in `docs/receipts/distribution/cd-receipts.md` are also `not-run` and are not GHCR.
