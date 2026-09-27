# Security notes (public sealed-submission API)

License: GPL-2.0

## Transport

- TLS 1.2+ required on all public endpoints (FR-RIDE-201).
- HSTS recommended at the edge.
- No mixed-content or plaintext HTTP fallback for API hosts.

## Authentication

- Bearer token representing a **driver account** (placeholder scheme in OpenAPI).
- Tokens MUST be revoked on credential reset and MUST NOT be logged in full.
- Passenger phones do not hold independent public-API driver credentials for admission; the driver phone coordinates submission.

## Tenant isolation

- Every admission and read path scopes by tenant / driver account (FR-RIDE-040).
- Cross-tenant submission or vehicle access returns `TENANT_ISOLATION` / 403.
- Object storage keys for sealed blobs MUST be tenant-prefixed and not guessable.

## Rate limits and abuse controls

- Per-account and per-IP limits on register, submit, and chunk upload (FR-RIDE-039).
- Exceeded limits return `RATE_LIMITED` (429).
- Registration may require additional bot checks in production (out of scope for this draft).

## No decrypt at ingest

- Ingest stores sealed ciphertext and verifies custody metadata only.
- Server admission MUST NOT decrypt video or sensor payloads to "inspect" them.
- Plaintext media content-types or envelopes without seal markers are rejected (`PLAINTEXT_REJECTED`).
- Court decryption is a separate authorized path (escrow / viewer), not part of this public API.

## Fail-closed admission checklist

Reject when:

1. CustodyReceipt missing or invalid
2. Play Integrity / AppAttestation failed or missing
3. Vehicle unregistered or unbound
4. Payload is plaintext or otherwise unsealed

Record outcomes in `ServerAdmissionLog` for audit without storing plaintext evidence.
