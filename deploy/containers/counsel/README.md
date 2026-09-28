# Counsel container

GPL-2.0-only. gRPC on .NET 10.

The counsel image is a deployment sketch of the same host as admission. `Counsel`, `Ingest`, and `Privacy` RPCs are registered beside sealed admission. The authoritative contracts are the protos under `src/RideAudit.Protos/`. OpenAPI does not define these methods.

## Run locally

Development fixtures do not contact Bitcoin, an L2, or Google Play. Production refuses the fixture flags.

```bash
export PATH="$HOME/.dotnet:$PATH"
export ASPNETCORE_ENVIRONMENT=Development
export RIDEAUDIT_ALLOW_INSECURE_DEV_HTTP=true
export RIDEAUDIT_OTS_CALENDAR=documented-fixture
export RIDEAUDIT_L2_CALENDAR=documented-fixture
export RIDEAUDIT_PLAY_INTEGRITY=fixture
export ASPNETCORE_URLS=http://127.0.0.1:8080
dotnet run --project src/RideAudit.Server.Admission
```

Callers send `authorization: Bearer <token>`. Elevated counsel, auditor, and admin calls also send `x-rideaudit-role`. Role grants are not self-service; `RoleDirectory.GrantBootstrap` is the operator hook used by tests. There is no public role-elevation RPC.

`RIDEAUDIT_L2_CALENDAR=documented-fixture` is required only when a vehicle profile selects `eth-l2-base`, `eth-l2-polygon`, or `dual-btc-ots+l2`. Those references use the `fixture:` prefix and are not live chain transactions. Omitting the variable fails closed.

Privacy-export ingest accepts a driver-provided ZIP (`DataDictionary.csv`, `trips.csv`, optional `scores.csv` and `online_hours.csv`). Unknown members are unverified. The connector for organizational ride status stays off until an admin sets the partnership gate, and this build does not call a network API. Coarse location from that connector is not stored as a Smooth Cruiser score.

## Container sketch

```bash
docker build -f deploy/containers/counsel/Dockerfile -t rideaudit-counsel:local .
```

The image sets Production and `RIDEAUDIT_EDGE_TLS=true`. This environment did not build or run the image.

## Distribution

Dev, Staging, and Prod continuous-delivery receipts are recorded as `not-run`. See `docs/receipts/distribution/cd-receipts.md`. Play publication is not claimed.
