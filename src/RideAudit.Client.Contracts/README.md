# RideAudit.Client.Contracts

Interim gRPC client stubs for the Avalonia clients.

`src/RideAudit.Protos/` is not on `origin/master`. This project compiles `interim/rideaudit/v1/custody.proto` with `Grpc.Tools` (`GrpcServices=Client`) into namespace `RideAudit.V1`.

The checked-in OpenAPI companion under `artifacts/server-api/` is non-authoritative. When the documents disagree, client code binds to this proto.

## Swap to shared protos later

1. Point the `Protobuf` item at the shared `src/RideAudit.Protos/` file that keeps package `rideaudit.v1`, `csharp_namespace = RideAudit.V1`, and the same field numbers.
2. Keep `ISealedAdmissionClient`. UI projects do not reference generated client classes directly.
3. Add a gRPC adapter beside `InterimInProcessAdmissionClient` when a real server endpoint exists.

`InterimInProcessAdmissionClient` is a client-side preflight. It does not admit evidence and it does not decrypt.

See [SWAP.md](SWAP.md).

License: GPL-2.0-or-later. See the repository `LICENSE`.
