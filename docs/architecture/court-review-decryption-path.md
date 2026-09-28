# Court-review decryption path

Status: server path for PLAN-RIDEAUDIT-001-SERVER S4.  
License: GPL-2.0-only.  
FR-RIDE-020, FR-RIDE-024, FR-RIDE-028.

This document describes the implemented server path. The counsel verification report repeats these proves / does-not-prove statements and does not decrypt. A desktop viewer UI is outside this server plan. This document is not legal advice.

## Custodians

Private keys and wrapped data-encryption keys are escrowed inside the HSM/KMS boundary (`RideAudit.Escrow.HsmKeyCustody`), not in the application database, the driver account, or the collection device. The default test quorum is 2-of-3. Each share is tagged with a distinct custodian id. Production must map those ids onto separate custodians and HSMs. The requester of a release cannot be one of the approving custodians.

## Legal process

A release request requires a case id, a legal-process reference (for example a subpoena or court order identifier), a purpose, and the single key id that counsel needs. The server does not infer a broader release.

## Escrow jurisdiction

Jurisdiction is the configuration-profile jurisdiction recorded for the vehicle (driver registration also stores jurisdiction and purpose). This build does not add a second jurisdiction ledger. Counsel must confirm that the case authority matches that recorded jurisdiction before requesting release.

## Dual control

Release is M-of-N dual control. One custodian is not enough when M is at least 2. Duplicate approvals from the same custodian do not count twice. Approvals and the open event are appended to a release log. The log type exposes no removal API.

## Expiring working copy

After quorum, the HSM reconstructs the secret, checks its integrity hash, and places only the data-encryption key into an expiring authorized working copy (default 15 minutes). The working copy decrypts `RIDESEAL1` or an already-escrowed device `RAES` envelope. When the copy expires, further reads fail and the buffer is wiped. The sealed ciphertext, content hash, and receipt core are not rewritten. Public ingest still does not decrypt.

## What the receipt proves

An upgraded custody anchor proves that the receipt-core digest was committed under the stated proof source. The digest covers the sealed-payload content hash, public key material, collector identity, collection time, provenance tag, and attestation evidence hash.

## What the receipt does not prove

The receipt does not prove the plaintext contents, the truth of any sensor reading, the identity of a person in a recording, or that a documented fixture calendar is a live Bitcoin transaction. `live_bitcoin_metadata` stays false unless a real OpenTimestamps upgrade parser has verified Bitcoin header linkage. This build does not include that parser. Decryption still requires the court-authorized escrow release above.

## Admission boundary

Public gRPC ingest never calls the working-copy opener. Failed chain confirmation, failed Play Integrity, missing escrow, or a plaintext payload leaves the submission unadmitted.
