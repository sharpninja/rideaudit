# RideAudit Server API Artifact

**Author:** Sharp Ninja
**Artifact ID:** ART-RIDE-API-001
**Kind:** grpc-api
**Version:** 0.3.0
**License:** GPL-2.0-only

> **Authoritative contract:** `src/RideAudit.Protos/` (proto3, contract version 0.3.0).  
> **OpenAPI role:** non-authoritative companion (`openapi.yaml`). FR-RIDE-062. When this companion and the protos disagree, conformance binds to grpc-protobuf.

> **Target stack:** gRPC on .NET 10 containers. The checked-in `openapi.yaml` is an interim human-readable companion and is not the wire contract.

## Purpose

Target API for the RideAudit public sealed-submission service. It runs as gRPC services in .NET 10 containers. Drivers, through the coordinating driver phone, register accounts and vehicles, open sessions, and upload **sealed** evidence with CustodyReceipt metadata. The ingest path never accepts plaintext video or sensor payloads and does not decrypt at rest on ingest.

## Contract documentation

- **Authoritative contract:** [src/RideAudit.Protos/](../../src/RideAudit.Protos/) — gRPC protobuf on .NET 10 containers
- [openapi.yaml](openapi.yaml) - non-authoritative OpenAPI 3.0+ human-readable companion for reviewers
- [error-codes.md](error-codes.md)
- [security.md](security.md)
- [ARTIFACT.yaml](ARTIFACT.yaml)

The OpenAPI document may help reviewers understand the interim public surface. It does not change the target gRPC and .NET 10 container decision.

## Fail-closed admission

Submissions are rejected when any of the following hold:

- Missing or invalid CustodyReceipt
- Failed or missing Play Integrity / app attestation
- Unregistered vehicle (or vehicle not bound to the driver)
- Plaintext video/sensor payloads (or content-type indicating unsealed media)

See FR-RIDE-035, FR-RIDE-036, FR-RIDE-026, FR-RIDE-033.

## Interim OpenAPI endpoints (human-readable summary)

The following REST paths describe the interim OpenAPI companion only. They are not the target API wire contract.

| Method | Path | Notes |
|--------|------|-------|
| POST | `/v1/drivers/register` | Create driver account |
| POST | `/v1/vehicles` | Register vehicle |
| GET | `/v1/vehicles` | List vehicles for caller |
| POST | `/v1/sessions` | Open audit session |
| POST | `/v1/submissions` | Sealed blob ref + CustodyReceipt |
| GET | `/v1/submissions/{id}/admission-status` | Admission result |
| POST | `/v1/submissions/{id}/chunks` | Resumable sealed upload |
| GET | `/v1/health` | Liveness |

Auth: bearer token for the driver account (placeholder scheme in the interim OpenAPI document).

## License (GPL-2.0)

```
RideAudit server API artifact
Copyright (C) 2026 RideAudit contributors

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version
2 of the License, or (at your option) any later version.
```
