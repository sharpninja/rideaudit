# API error codes

License: GPL-2.0

Machine-readable `code` values returned in the `Error` schema.

| Code | HTTP | Meaning |
|------|------|---------|
| `AUTH_REQUIRED` | 401 | Missing or invalid bearer token |
| `AUTH_FORBIDDEN` | 403 | Authenticated but not allowed for resource |
| `VALIDATION_FAILED` | 400 | Request body failed schema / field validation |
| `RECEIPT_MISSING` | 422 | CustodyReceipt absent or incomplete (fail-closed) |
| `RECEIPT_INVALID` | 422 | CustodyReceipt failed integrity or field checks |
| `ATTESTATION_FAILED` | 422 | Play Integrity / AppAttestation missing or failed |
| `VEHICLE_UNREGISTERED` | 422 | Vehicle not registered or not bound to driver |
| `PLAINTEXT_REJECTED` | 422 | Payload appears unsealed / plaintext media refused |
| `SESSION_INVALID` | 422 | Session unknown, closed, or not owned by caller |
| `SUBMISSION_NOT_FOUND` | 404 | Unknown submission id |
| `CHUNK_OUT_OF_ORDER` | 409 | Resumable chunk index conflict |
| `RATE_LIMITED` | 429 | Abuse control / rate limit |
| `TENANT_ISOLATION` | 403 | Cross-tenant access denied |
| `INTERNAL_ERROR` | 500 | Unexpected server fault (no sensitive detail) |
| `NOT_IMPLEMENTED` | 501 | Placeholder endpoint behavior in draft |

Admission failures (`RECEIPT_*`, `ATTESTATION_FAILED`, `VEHICLE_UNREGISTERED`, `PLAINTEXT_REJECTED`) are intentional fail-closed outcomes and MUST NOT be softened to partial accept.
