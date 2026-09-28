# Swapping interim stubs for src/RideAudit.Protos

| Keep | Replace |
| --- | --- |
| `ISealedAdmissionClient` | `interim/rideaudit/v1/custody.proto` include path |
| UI views and view models | Generated `RideAudit.V1` types only if field numbers change; update `SubmissionMapper` |
| `ContractProvenance.SwapTarget` | Set source note to the shared proto revision |

Do not copy server admission, counsel, chain, escrow, or ingest implementations into this project.
