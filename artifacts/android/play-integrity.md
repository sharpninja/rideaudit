# Play Integrity (fail-closed)

License: GPL-2.0

## Policy

1. **Attestation before keygen.** Session or sample encryption keys MUST NOT be generated until a Play Integrity token is obtained and verified against the server allowlist (FR-RIDE-025, FR-RIDE-215).
2. **Fail-closed.** If attestation is missing, expired, device-compromised, app-unrecognized, or not on the authenticity allowlist, the client MUST refuse to seal or upload (FR-RIDE-026).
3. **Receipt binding.** Attestation summary (token hash, verdict fields, timestamp) MUST appear on the CustodyReceipt (FR-RIDE-027).
4. **No bypass.** Debug builds used for local development must still surface the gate; production builds must not ship a disable flag reachable by users.

## Client sequence

1. Start dual-phone session setup.
2. Call `PlayIntegrityGate.requestAttestation(nonce)`.
3. Send token to admission/attest endpoint (or include for server verification at submission).
4. On success: proceed to keygen and seal.
5. On failure: abort UI with clear error; log locally without leaking secrets.

## Server alignment

The public sealed-submission API rejects submissions with failed or missing Play Integrity as part of fail-closed admission (see `artifacts/server-api/`).
