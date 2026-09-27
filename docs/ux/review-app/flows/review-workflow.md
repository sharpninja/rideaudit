# Review workflow (prose + acceptance criteria)

**Artifact:** ART-RIDE-UX-REVIEW-001  
**UI:** Avalonia UI 12  
**Backend:** gRPC on .NET 10 containers  
**Chain default:** btc-ots (Bitcoin OpenTimestamps)  
**Process:** Byrd Dev Process friendly (goal, actors, AC per step)

Overall goal: counsel reviews admitted sealed dual-phone evidence with independent provenance, court-authorized decrypt only, synchronized playback, and disclosable verification artifacts.

Actors (workflow-wide): Counsel / Auditor; Avalonia review app; Escrow custodians (M-of-N); gRPC backend services; opposing counsel (export consumer).

---

## Step 1. Open admitted submission / RideBundle

**Goal:** Bind a case to an admitted `RideBundle` or submission id and open a `ViewerSession`.

**Actors:** Counsel; Avalonia app; gRPC admission/index service.

**Acceptance criteria:**

- AC-1.1 App opens only admitted sealed submissions (reject unknown / non-admitted ids with auditable error).
- AC-1.2 `ViewerSession` records viewer build/version, reviewer role, case/bundle id, start time (FR-RIDE-052).
- AC-1.3 UI shows case id, vehicle, dual-phone roles summary without decrypting (WF-R-01).

---

## Step 2. Load sealed packages (no decrypt yet)

**Goal:** Enumerate sealed records and composite links as ciphertext references only.

**Actors:** Counsel; Avalonia app.

**Acceptance criteria:**

- AC-2.1 Package list shows sealed status, role (driver coordinator / passenger compositor), vehicle id, content hash ids, receipt ids (WF-R-02).
- AC-2.2 Decrypt / playback controls remain disabled until Steps 3 to 5 succeed.
- AC-2.3 Original sealed blobs are not mutated.

---

## Step 3. Independent verification gate

**Goal:** Independently verify custody and authenticity **before** any decrypt or display of evidence payloads.

**Actors:** Avalonia app verification module; Bitcoin OTS / headers; attestation policy.

**Checks (all required):**

1. Bitcoin OpenTimestamps (primary) custody receipt vs public Bitcoin headers
2. Payload / content hashes
3. Play Integrity / Google Play signing certificate + nonce/key binding
4. Escrow release authorization readiness (or explicit "not yet authorized" state that still blocks decrypt)

**Acceptance criteria:**

- AC-3.1 Each check produces a pass/fail/detail row in `VerificationReport` (WF-R-03, FR-RIDE-028, FR-RIDE-050).
- AC-3.2 OTS verification does not require trusting RideAudit servers alone (FR-RIDE-018; architecture note).
- AC-3.3 Incomplete, stale, missing, or inconsistent proofs are not labeled court-ready.

---

## Step 4. Fail-closed error path vs pass

**Goal:** On any failure, block decrypt/display; on full pass, unlock escrow request path.

**Actors:** Avalonia app; Counsel.

**Acceptance criteria:**

- AC-4.1 Any failed check routes to WF-R-04 blocking screen; decrypt and playback stay unavailable (NFR-22).
- AC-4.2 Fail path persists `VerificationReport` with fail-closed errors and ViewerSession audit event.
- AC-4.3 Pass path enables Step 5 only; still no plaintext until escrow completes.

---

## Step 5. Escrow release / CourtRelease attach (M-of-N)

**Goal:** Attach court-authorized `CourtRelease` / `EscrowRelease` under dual-control M-of-N quorum.

**Actors:** Counsel; Escrow custodians; Avalonia app; gRPC escrow service.

**Acceptance criteria:**

- AC-5.1 Quorum status (M of N) is visible; release cannot complete below policy (WF-R-05, FR-RIDE-022).
- AC-5.2 `CourtRelease` records authorizer, case_id, dual-control attestations, working-copy ref, `expires_at` (FR-RIDE-020).
- AC-5.3 Sealed blob and custody receipt remain unchanged by release logging.

---

## Step 6. Decrypt to expiring working copy

**Goal:** Decrypt only into an authorized working copy that expires; never casual plaintext.

**Actors:** Avalonia app; Escrow-released key/wrapped DEK.

**Acceptance criteria:**

- AC-6.1 Decrypt occurs only after Steps 3 to 5 succeed (FR-RIDE-049, FR-RIDE-050).
- AC-6.2 Working copy is scoped and shows `expires_at`; expired copy cannot play (FR-RIDE-047).
- AC-6.3 Viewer never presents a path labeled as bypassing escrow.

---

## Step 7. Synchronized timeline playback

**Goal:** Play composite + spider-graph telematics (and GPS/OBD if present) on one timeline using `SyncClockOffset`.

**Actors:** Counsel; Avalonia playback UI.

**Acceptance criteria:**

- AC-7.1 Timeline includes composite video and spider overlay when present (WF-R-06, FR-RIDE-051, NFR-21).
- AC-7.2 Scrubbing keeps overlay and video aligned via recorded `SyncClockOffset`.
- AC-7.3 Missing GPS/OBD/raw streams are reported as absent, not fabricated.

---

## Step 8. Coverage / gaps panel (Unverified Lyft-native caveats)

**Goal:** Show evidence coverage matrix and explicitly label Unverified gaps (no invented Lyft APIs).

**Actors:** Counsel; Avalonia app.

**Acceptance criteria:**

- AC-8.1 Gaps where Lyft-native signals were not collected are labeled **Unverified**.
- AC-8.2 UI copy never claims Lyft private API success.
- AC-8.3 Coverage panel remains visible alongside playback.

---

## Step 9. Export disclosure pack

**Goal:** Export sealed packages + portable `.ots` + attestation summary + `VerificationReport` for independent check.

**Actors:** Counsel; opposing counsel (consumer).

**Acceptance criteria:**

- AC-9.1 Export includes sealed ciphertext, `.ots` proofs, attestation summary, and VerificationReport (WF-R-08, FR-RIDE-028).
- AC-9.2 Export does not require RideAudit servers for OTS verify path explanation (WF-R-07).
- AC-9.3 Multi-driver exports retain **per-record** reports (FR-RIDE-038).

---

## Step 10. Close ViewerSession / audit log

**Goal:** End review with append-only access/render log and working-copy hygiene.

**Actors:** Counsel; Avalonia app.

**Acceptance criteria:**

- AC-10.1 Closing writes ViewerSession end time and append-only events (FR-RIDE-052).
- AC-10.2 Expired or closed-session working copies are purged or rendered unusable.
- AC-10.3 Final VerificationReport is retained per record and session.
