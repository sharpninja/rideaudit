# RideAudit Server API Artifact

**Artifact ID:** ART-RIDE-API-001  
**Kind:** openapi  
**Version:** 0.1.0  
**License:** GPL-2.0

## Purpose

Public sealed-submission API for RideAudit. Drivers (via the coordinating driver phone) register accounts and vehicles, open sessions, and upload **sealed** evidence with CustodyReceipt metadata. The ingest path never accepts plaintext video or sensor payloads and does not decrypt at rest on ingest.

## Spec

- [openapi.yaml](openapi.yaml)  -  OpenAPI 3.0+ for the public surface
- [error-codes.md](error-codes.md)
- [security.md](security.md)
- [ARTIFACT.yaml](ARTIFACT.yaml)

## Fail-closed admission

Submissions are rejected when any of the following hold:

- Missing or invalid CustodyReceipt
- Failed or missing Play Integrity / app attestation
- Unregistered vehicle (or vehicle not bound to the driver)
- Plaintext video/sensor payloads (or content-type indicating unsealed media)

See FR-RIDE-035, FR-RIDE-036, FR-RIDE-026, FR-RIDE-033.

## Endpoints (summary)

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

Auth: bearer token for the driver account (placeholder scheme in the OpenAPI doc).

## License (GPL-2.0)

```
RideAudit server API artifact
Copyright (C) 2026 RideAudit contributors

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.
```
