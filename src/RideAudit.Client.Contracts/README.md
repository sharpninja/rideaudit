# RideAudit.Client.Contracts

Client admission abstraction over the authoritative protos in `src/RideAudit.Protos/`.

Generated types live in `RideAudit.Protos` (`RideAudit.Protos.Admission.V1`, `RideAudit.Protos.Custody.V1`). This project does not compile a second proto. UI projects depend on `ISealedAdmissionClient`, not on a generated gRPC channel.

`InterimInProcessAdmissionClient` checks sealed content type, the device RAES magic, receipt core, vehicle, session, and attestation, then returns `admitted=false` with custody state `local-sealed-pending`. That is not server admission and not an OpenTimestamps confirmation.

The attestation token sent on `AttestationSubmission.token` is the SHA-256 hex retained on the device receipt. Raw Play tokens are not stored or logged. Fixture and stub providers are not Google Play Integrity JWTs. This client does not emit server `fixture.v1.` tokens and does not claim a live Play or Bitcoin receipt.

The checked-in OpenAPI companion under `artifacts/server-api/` is non-authoritative. When the documents disagree, conformance binds to `src/RideAudit.Protos/`.

See [SWAP.md](SWAP.md).

License: GPL-2.0-only for the shared package expression; client source headers remain GPL-2.0-or-later. See the repository `LICENSE` and `NOTICE`.
