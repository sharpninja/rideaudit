# Testing Requirements (MCP Server)

## TEST-RIDE

### TEST-RIDE-001

Parse sample ZIP with DataDictionary; assert known files mapped and unknown tagged Unverified.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-001 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-002

Import structured fields and manual entry with consent/source tag.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-002 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-003

Map trip fields when present; leave missing null.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-003 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-004

With partnership approved, poll documented status only; when denied, features disabled; no private APIs.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-004 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-005

Import CSV/JSON and assert third_party_telematics provenance.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-005 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-006

Every dataset links to consent actor, time, jurisdiction, purpose.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-006 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-007

Matrix lists collected/available/missing; API-gap notice present.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-007 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-008

Flag violations only when hours data present; apply regional overrides.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-008 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-009

Export GPS/scores/third-party events for selected interval with provenance.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-009 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-010

Access export works; deletion blocked under legal hold and allowed when clear.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-010 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-011

Raw imports hashed/versioned; integrity status shown.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-011 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-012

Roles enforced; precise coordinates masked by default.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-012 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-013

Seal before durable store; scoped keys; no shared long-lived all-record key.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-013 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-014

Receipt fields complete; chain write records chain ID/tx/block/time.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-014 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-015

On chain failure, non-admitted, ciphertext unchanged, operator alerted.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-015 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-016

Docs present; verify UI recomputes hash and shows chain mismatches.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-016 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-017

M-of-N escrow off-device; no plaintext private key on-device after seal.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-017 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-018

Quorum release logs append-only; sealed evidence unchanged; counsel verification steps pass.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-018 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-019

Failed attestation rejects collection; success binds key and includes attestation on receipt.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-019 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-020

LICENSE GPL-2.0; notices on artifacts; Play/source publish metadata present.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-020 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-021

Account/consent/vehicle/config required before admit collection.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-021 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-022

Plaintext rejected; admission verifies receipt/hash/chain/attestation/binding.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-022 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-023

Per-record verification in multi-driver bundle; aggregation does not weaken custody.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-023 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-024

Rate limits, replay rejection, quarantine; no unauthorized cross-tenant access; capacity does not bypass verify.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-024 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-025

Two-phone session syncs clocks, composites on-device with spider-graph overlay.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-025 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-026

Composite sealed with chain receipt; optional raw sealed with consent; metadata in custody package; no server plaintext re-encode.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-026 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-027

Playback only after full verification; inconsistencies reported; integrity reproducible.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-027 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-028

Viewer on Win/Linux/macOS verifies then shows timeline; creates ViewerSession/VerificationReport; fails closed on verify errors.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-028 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-029

TLS 1.2+; secrets in vault; no plaintext tokens in logs; private keys not in app DB; sensitive access logged.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-029 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-030

Rate-limit/outage does not corrupt rides; Concierge-derived metrics not labeled Smooth Cruiser.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-030 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-031

Large histories paginate; export ZIP has CSV+PDF+provenance JSON.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-031 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-032

CA-strict retention default; algorithm IDs preserved on rotation; latency budgets gate admission.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-032 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-033

Quotas/chunked uploads enforced; below-threshold composites not admitted.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-033 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-034

Verify Bluetooth pairing assigns driver coordinator vs passenger compositor roles and fail-closed paths.


### TEST-RIDE-035

Verify Android capture and desktop court viewer build on Avalonia UI 12 and shared client UI is GPL-2.0.


### TEST-RIDE-036

Verify gRPC on .NET 10 containers accepts sealed-only submit, never decrypts at ingest, and fails closed on admission errors.


### TEST-RIDE-037

Verify protos and schemas publish under GPL-2.0 and that OpenAPI is marked non-authoritative when it disagrees with gRPC.
