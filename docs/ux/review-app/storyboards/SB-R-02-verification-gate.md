# SB-R-02 Verification gate (fail-closed)

**Artifact:** ART-RIDE-UX-REVIEW-001  
**FR links:** FR-RIDE-017, FR-RIDE-018, FR-RIDE-025, FR-RIDE-028, FR-RIDE-050  
**NFR:** NFR-22  
**Screens:** WF-R-03, WF-R-04, WF-R-07  
**UI:** Avalonia UI 12 desktop  
**Chain default:** Bitcoin OpenTimestamps (`btc-ots`)

## Goal

Independently verify Bitcoin OTS custody receipts, payload hashes, Play Integrity / signing certificate + nonce/key binding, and escrow authorization state **before** any decrypt or display. Fail closed on any failure.

## Actors

- Counsel / Auditor
- Avalonia review app verification module
- Bitcoin headers / OTS verify path
- Attestation policy (approved Play identity)

## Beats

1. **Run verification**  
   For each sealed record, app checks OTS proof against Bitcoin headers, content hashes, Play Integrity / signing cert and nonce/key binding, and whether escrow release authorization is present and structurally valid.

2. **Verification report (WF-R-03)**  
   Pass/fail detail rows populate `VerificationReport`. Provenance panel can expand OTS fields (WF-R-07). Incomplete proofs are not marked court-ready.

3. **Fail-closed (WF-R-04)**  
   Any failed, missing, stale, or inconsistent check blocks decrypt and playback. Auditable error is written to ViewerSession and VerificationReport.

4. **Pass gate**  
   Full pass unlocks escrow-request UI only. Still no plaintext.

## Success criteria

- Verify-before-decrypt/display is enforced (FR-RIDE-050).
- Bitcoin OTS is the primary public custody check path.
- Failures never soft-continue into decrypt.

## Notes

Optional eth-L2 dual-anchor, if configured, is interim only; Bitcoin OTS remains the long-term anchor unless jurisdiction config says otherwise.
