# API error codes

License: GPL-2.0-only

Machine-readable `code` values. The gRPC status mapping in `RideAudit.Server.Admission` is authoritative if this companion list drifts (FR-RIDE-062). Admission failures are fail-closed and must not be softened to a partial accept.

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
| `NOT_IMPLEMENTED` | 501 | Reserved. Counsel, ingest, and privacy RPCs in this tree are implemented. |
| `CONSENT_REQUIRED` | 422 | Import refused because consent was not granted |
| `IMPORT_REJECTED` | 422 | Raw import could not be parsed |
| `CONFIG_PROFILE_INVALID` | 422 | Missing or invalid vehicle configuration profile |
| `CHAIN_UNCONFIRMED` | 422 | Anchor pending or otherwise not confirmed |
| `CHAIN_FAILED` | 422 | Chain write or confirmation failed; record not admitted |
| `CHAIN_PROFILE_UNSUPPORTED` | 422 | Selected public-chain profile is not implemented |
| `ESCROW_UNAVAILABLE` | 422 | Collection key is not escrowed for the tenant |
| `ESCROW_QUORUM` | 422 | M-of-N approvals are incomplete |
| `ESCROW_INTEGRITY` | 422 | Reconstructed escrow secret failed its check |
| `DUPLICATE_REPLAY` | 422 | Duplicate or replayed sealed submission |
| `IDEMPOTENCY_CONFLICT` | 409 | Idempotency key reused with a different body |
| `SIZE_LIMIT` | 429 | Payload exceeds the configured size limit |
| `QUOTA_EXCEEDED` | 429 | Tenant quota exceeded |
| `POLICY_MISMATCH` | 422 | Receipt policy version is not active |
| `KEY_SCOPE_REJECTED` | 422 | Key scope is not session or sample |
| `LATENCY_BUDGET_EXCEEDED` | 422 | Seal/receipt latency budget blocked admission |
| `WORKING_COPY_EXPIRED` | 422 | Authorized working copy expired |

Admission failures (`RECEIPT_*`, `ATTESTATION_FAILED`, `VEHICLE_UNREGISTERED`, `PLAINTEXT_REJECTED`) are intentional fail-closed outcomes and MUST NOT be softened to partial accept.
