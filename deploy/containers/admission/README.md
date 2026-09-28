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

## Container sketch

```bash
docker build -f deploy/containers/admission/Dockerfile -t rideaudit-admission:local .
```

The image sets `ASPNETCORE_ENVIRONMENT=Production` and `RIDEAUDIT_EDGE_TLS=true`. The edge must terminate TLS 1.2 or newer. The process refuses to start in Production if fixture calendars or fixture Play Integrity decoders are enabled, and it refuses Production HTTP without either a certificate path or the edge-TLS acknowledgement.

## Deferred slices

S1 through S4 are implemented in this tree (protos, admission, identity, seal store, documented OTS fixture, M-of-N escrow). These remain deferred:

- S5 counsel bundle and analysis UI: `src/RideAudit.Server.Counsel`, `src/RideAudit.Anal`. The counsel gRPC method returns UNIMPLEMENTED.
- S6 ingest pipelines: `src/RideAudit.Ingest`. No undocumented Lyft private API is called. The ingest RPC returns UNIMPLEMENTED.
- S7 privacy, DSAR, retention, legal hold: `src/RideAudit.Privacy`. Deletion is not performed.
- S8 live Base, Polygon, and dual-anchor profiles: selecting them fails closed and writes no transaction metadata. `src/RideAudit.Chain.EthL2`.
- S9 integrated acceptance and distribution receipts.

Access logs for vehicle list and admission status are append-only. Full RBAC and geolocation masking remain in S7.
