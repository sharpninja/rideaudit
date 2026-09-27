# SB-04 Seal and receipt

**Artifact:** ART-RIDE-UX-001  
**FR links:** FR-RIDE-015, FR-RIDE-016, FR-RIDE-017, FR-RIDE-018, FR-RIDE-027, FR-RIDE-045  
**Screens:** [WF-06](../assets/wireframes/WF-06-seal-progress.svg), [WF-08](../assets/wireframes/WF-08-fail-closed-errors.svg)
**Chain default:** Bitcoin OpenTimestamps (btc-ots)

## Goal

At collection boundary, encrypt and seal each evidence package (including the passenger composite) and write a custody receipt whose primary public anchor is Bitcoin via OpenTimestamps (OTS). Attestation metadata from Play Integrity is bound into the receipt.

## Actors

- Driver phone (orchestrates seal completion for the session)
- Passenger phone (seals local streams and composite)
- OTS calendar / Bitcoin headers (public verify path)
- Optional L2 fast-confirm (only if dual-anchor config enabled)

## Beats

1. **Enter seal**  
   After STOP, both phones show seal progress (WF-06): hashing payload, encrypting with session/sample keys, assembling custody receipt fields (hashes, public key id, collector_id, attestation digest, SyncClockOffset, role).

   ![WF-06 Seal progress](../assets/wireframes/WF-06-seal-progress.svg)

   [Open WF-06-seal-progress.svg](../assets/wireframes/WF-06-seal-progress.svg)

2. **Seal-at-collect**  
   Encryption completes before durable cloud handoff. Unsealed buffers are not uploaded. Failure at any seal step is fail-closed: package marked not admissible.

   ![WF-06 Seal progress](../assets/wireframes/WF-06-seal-progress.svg)

   [Open WF-06-seal-progress.svg](../assets/wireframes/WF-06-seal-progress.svg)

   Fail-closed branch:

   ![WF-08 Fail-closed errors](../assets/wireframes/WF-08-fail-closed-errors.svg)

   [Open WF-08-fail-closed-errors.svg](../assets/wireframes/WF-08-fail-closed-errors.svg)

3. **Receipt pending**  
   Local receipt is created with chain profile `btc-ots`. UI shows "OTS pending" until stamp/upgrade succeeds. Optional eth-L2 interim confirmation may appear only when dual-anchor is configured (see architecture note).

   ![WF-06 Seal progress](../assets/wireframes/WF-06-seal-progress.svg)

   [Open WF-06-seal-progress.svg](../assets/wireframes/WF-06-seal-progress.svg)

4. **Receipt confirmed**  
   OTS proof path available (`.ots` portable). Driver UI shows receipt id, chain profile, and verify-later hint. Session is ready for public sealed submit (SB-05).

   ![WF-06 Seal progress](../assets/wireframes/WF-06-seal-progress.svg)

   [Open WF-06-seal-progress.svg](../assets/wireframes/WF-06-seal-progress.svg)

   Ready for sealed submit:

   ![WF-07 Submit status](../assets/wireframes/WF-07-submit-status.svg)

   [Open WF-07-submit-status.svg](../assets/wireframes/WF-07-submit-status.svg)

## Success criteria

- Seal and encrypt occur at collection, before upload.
- Custody receipt includes attestation and public key material needed for later verification.
- Primary chain note: Bitcoin OpenTimestamps per `docs/architecture/blockchain-custody-receipts.md`.

## Notes

Offline policy: local pending receipt is allowed; package is not admitted on the public server until configured confirmation succeeds.
