# Technical Requirements (MCP Server)

## TR-RIDE-ANAL-001

**Authorized working-copy analysis** — Analysis operates only on authorized decrypted working copies with expiry and access records; originals remain sealed.
**Covered by:** FR: FR-RIDE-020, FR-RIDE-021; TEST: TEST-RIDE-016
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Working copies require authorization and have expiry.
- [ ] Originals remain sealed.

## TR-RIDE-ANAL-002

**Coverage matrix generator** — Generate collected-by-Lyft vs available vs missing matrix per signal type.
**Covered by:** FR: FR-RIDE-007, FR-RIDE-209; TEST: TEST-RIDE-007
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Matrix produced per audit subject.
- [ ] API gaps explicitly listed.

## TR-RIDE-ANAL-003

**Online-hours policy engine** — Evaluate online intervals against 12h/6h and regional overrides when data present.
**Covered by:** FR: FR-RIDE-008; TEST: TEST-RIDE-008
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Flags only when hours data present.
- [ ] Regional overrides configurable.

## TR-RIDE-ANAL-004

**Incident package builder** — Assemble GPS/route, scores, and third-party events for a selectable time window for counsel handoff.
**Covered by:** FR: FR-RIDE-009; TEST: TEST-RIDE-009
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Time window selectable.
- [ ] Package includes provenance.

## TR-RIDE-CHAIN-001

**Custody receipt builder** — Build custody receipt with hash, public key/key ID, collector, timestamp, provenance, attestation refs, and tx reference when confirmed.
**Covered by:** FR: FR-RIDE-017, FR-RIDE-027; TEST: TEST-RIDE-014, TEST-RIDE-019
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Required receipt fields present.
- [ ] Tx reference filled on confirmation.

## TR-RIDE-CHAIN-002

**Configurable chain writer** — Write receipts to configurable public immutable chain; record chain ID, tx hash, block height, write time.
**Covered by:** FR: FR-RIDE-018, FR-RIDE-212; TEST: TEST-RIDE-014
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Chain configurable by env/jurisdiction.
- [ ] Confirmation metadata recorded.

## TR-RIDE-CHAIN-003

**Non-admission on chain failure** — On write/confirmation failure, quarantine/retry without rewriting ciphertext; keep non-admitted and surface operator alert.
**Covered by:** FR: FR-RIDE-019; TEST: TEST-RIDE-015
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Non-admitted on failure.
- [ ] Ciphertext unchanged; audit trail preserved.

## TR-RIDE-CHAIN-004

**Receipt verification service** — Recompute hash, compare receipt, check on-chain status; surface mismatches clearly.
**Covered by:** FR: FR-RIDE-021, FR-RIDE-028; TEST: TEST-RIDE-016, TEST-RIDE-018
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Hash recompute and chain check implemented.
- [ ] Mismatch UI/report clear.

## TR-RIDE-ESCROW-001

**M-of-N escrow packaging** — Escrow private keys or wrapped DEKs under configurable M-of-N dual control separate from device and app DB.
**Covered by:** FR: FR-RIDE-022, FR-RIDE-023, FR-RIDE-216; TEST: TEST-RIDE-017
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] M-of-N packaging created per court-needed key/DEK.
- [ ] Escrow separate from device/operator/app DB.

## TR-RIDE-ESCROW-002

**Court escrow-release workflow** — Verify case/legal process, obtain quorum approvals, release minimum scope, append-only release log, never mutate sealed evidence.
**Covered by:** FR: FR-RIDE-024, FR-RIDE-028; TEST: TEST-RIDE-018
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Quorum and legal-process checks enforced.
- [ ] Release log append-only; sealed evidence unchanged.

## TR-RIDE-ESCROW-003

**HSM/KMS private-key custody** — Generate/store/use private keys via HSM/KMS or equivalent; never in application database.
**Covered by:** FR: FR-RIDE-023, FR-RIDE-214; TEST: TEST-RIDE-017, TEST-RIDE-029
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] No private keys in app DB.
- [ ] Access logging and separation of duties supported.

## TR-RIDE-GPL-001

**GPL-2.0 project licensing** — License app, schemas, tooling, and evidence-network under GPL-2.0; publish source and notices; do not substitute Apache-2.0 or MIT.
**Covered by:** FR: FR-RIDE-029, FR-RIDE-217; TEST: TEST-RIDE-020
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] GPL-2.0 LICENSE and notices present.
- [ ] No Apache/MIT substitution for in-scope components.

## TR-RIDE-GPL-002

**Artifact license metadata** — Attach GPL2 license/version and source-commit notice to shared software, schema, receipt, attestation, and verification artifacts without exposing sealed payloads.
**Covered by:** FR: FR-RIDE-030; TEST: TEST-RIDE-020
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] LicenseMetadata attached to shared artifacts.
- [ ] Sealed payloads remain access-controlled.

## TR-RIDE-GPL-003

**Play and source distribution** — Publish client via Google Play and public source repo with reproducible/versioned build and signing metadata.
**Covered by:** FR: FR-RIDE-031; TEST: TEST-RIDE-020
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Play listing and source repo available.
- [ ] Build/signing metadata reproducible.

## TR-RIDE-GPL-004

**Shared Avalonia UI under GPL-2.0** — Package shared Avalonia UI 12 client libraries, themes, and controls under GPL-2.0 for Android and desktop reuse.
**Covered by:** FR: FR-RIDE-058; TEST: TEST-RIDE-035
**Status:** pending
Scope: layer-1+

## TR-RIDE-GPL-005

**Publish gRPC protos under GPL-2.0** — Version and publish protobuf contracts and related schemas under GPL-2.0 with license notices.
**Covered by:** FR: FR-RIDE-060; TEST: TEST-RIDE-037
**Status:** pending
Scope: layer-1+

## TR-RIDE-INGEST-001

**Privacy-export ZIP parser** — Implement DataDictionary-driven ZIP parser with Unverified tagging for unknown types.
**Covered by:** FR: FR-RIDE-001, FR-RIDE-011; TEST: TEST-RIDE-001, TEST-RIDE-004
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Parser reads DataDictionary and maps known files.
- [ ] Unknown types tagged Unverified.

## TR-RIDE-INGEST-002

**Smooth Cruiser structured and manual ingest** — Support structured field ingest and consented screenshot/manual entry with provenance tags.
**Covered by:** FR: FR-RIDE-002, FR-RIDE-011; TEST: TEST-RIDE-002, TEST-RIDE-004
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Structured fields ingested when present.
- [ ] Manual entry requires consent and source tag.

## TR-RIDE-INGEST-003

**Trip and Business report ingest** — Parse trip-level records from exports and optional Business reports.
**Covered by:** FR: FR-RIDE-003; TEST: TEST-RIDE-003
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Trip fields mapped when present.
- [ ] Missing fields not invented.

## TR-RIDE-INGEST-004

**Concierge OAuth and status poller** — Optional Concierge client using documented OAuth and /concierge/rides/{id}/status only for org-booked active rides.
**Covered by:** FR: FR-RIDE-004, FR-RIDE-012; TEST: TEST-RIDE-004
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Uses documented Concierge status endpoint only.
- [ ] Disabled when partnership denied.

## TR-RIDE-INGEST-005

**Third-party telematics importers** — CSV/JSON importers for GPS, speed, harsh events with third_party_telematics provenance.
**Covered by:** FR: FR-RIDE-005; TEST: TEST-RIDE-005
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Supported schemas import successfully.
- [ ] Provenance enum set correctly.

## TR-RIDE-INGEST-006

**Honesty provenance tagging** — Tag each field with provenance enum: lyft_privacy_export | lyft_concierge_api | in_app_manual | third_party_telematics | unverified.
**Covered by:** FR: FR-RIDE-001, FR-RIDE-005, FR-RIDE-006; TEST: TEST-RIDE-001, TEST-RIDE-005, TEST-RIDE-006
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Every ingested field carries a provenance enum value.

## TR-RIDE-PERF-001

**Seal/receipt latency monitoring** — Define, measure, monitor per-connector budgets for seal, receipt, chain confirmation; block admission until policy satisfied.
**Covered by:** FR: FR-RIDE-213; TEST: TEST-RIDE-032
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Budgets configurable and monitored.
- [ ] Admission gated on confirmation policy.

## TR-RIDE-PERF-002

**Large history UI pagination** — Paginate/downsample multi-year and multi-Hz tracks to avoid UI freezes.
**Covered by:** FR: FR-RIDE-205; TEST: TEST-RIDE-031
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Large tracks display without freeze via pagination/downsample.

## TR-RIDE-PERF-003

**Video performance gates** — Measure device/edge and server video metrics; refuse admission below sync/seal quality thresholds.
**Covered by:** FR: FR-RIDE-220; TEST: TEST-RIDE-033
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Metrics collected.
- [ ] Below-threshold composites not admitted.

## TR-RIDE-PLAY-001

**Play Integrity before seal** — Verify Play Integrity / signing-certificate before key generation and sealing; reject compromised or unapproved binaries.
**Covered by:** FR: FR-RIDE-025, FR-RIDE-026, FR-RIDE-215; TEST: TEST-RIDE-019
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Check precedes keygen/seal.
- [ ] Failed verdicts reject collection.

## TR-RIDE-PLAY-002

**Attestation evidence on receipt** — Include attestation evidence or hash, package identity, and cert digest/reference on on-chain receipt linked to KeyMaterial and SealedRecord.
**Covered by:** FR: FR-RIDE-027; TEST: TEST-RIDE-019
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Receipt carries attestation fields.
- [ ] Links to KeyMaterial and SealedRecord.

## TR-RIDE-PLAY-003

**Package allowlist and rotation** — Versioned allowlist of package identities and signing-certificate digests with auditable rotation.
**Covered by:** FR: FR-RIDE-026, FR-RIDE-215; TEST: TEST-RIDE-019
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Allowlist versioned and enforced.
- [ ] Rotation audited.

## TR-RIDE-PRIV-001

**Consent and purpose binding** — Bind every dataset to verified driver identity, consent record, jurisdiction, and audit purpose.
**Covered by:** FR: FR-RIDE-006, FR-RIDE-010; TEST: TEST-RIDE-006, TEST-RIDE-010
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Consent ledger fields required for ingest admission.

## TR-RIDE-PRIV-002

**Sensitive geolocation masking** — Classify precise geolocation as sensitive; mask exact coordinates in default UI except authorized roles.
**Covered by:** FR: FR-RIDE-014, FR-RIDE-202; TEST: TEST-RIDE-012
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Default UI masks coordinates.
- [ ] Authorized roles can unmask with access log.

## TR-RIDE-PRIV-003

**DSAR access and deletion** — Support subject access export and deletion subject to legal holds and custody documentation.
**Covered by:** FR: FR-RIDE-010, FR-RIDE-210; TEST: TEST-RIDE-010
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Access export available.
- [ ] Deletion respects legal holds.

## TR-RIDE-SEAL-001

**Collection-boundary sealer** — Seal and encrypt at exact collection moment before durable store, queue, normalization, or analysis.
**Covered by:** FR: FR-RIDE-015; TEST: TEST-RIDE-013
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Sealing precedes all durable handoffs.
- [ ] Algorithm/version and content hash recorded.

## TR-RIDE-SEAL-002

**Scoped key generation** — Generate per-session or per-sample keys bound to declared scope; reject long-lived shared keys for all records.
**Covered by:** FR: FR-RIDE-016; TEST: TEST-RIDE-013
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Scope binding enforced.
- [ ] Shared long-lived all-record keys rejected.

## TR-RIDE-SEAL-003

**Cryptographic agility layer** — Versioned algorithms, key formats, and receipt schemas rotatable without rewriting sealed records.
**Covered by:** FR: FR-RIDE-211; TEST: TEST-RIDE-032
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Algorithm identifier preserved per record.
- [ ] Suite rotation does not rewrite ciphertext.

## TR-RIDE-SEC-001

**TLS and vault secrets** — Enforce TLS 1.2+, encryption at rest, vault-stored secrets, no plaintext tokens in logs.
**Covered by:** FR: FR-RIDE-201; TEST: TEST-RIDE-029
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] TLS 1.2+ required.
- [ ] Secret scan of logs finds no plaintext tokens.

## TR-RIDE-SEC-002

**RBAC least privilege** — Roles auditor, subject (driver), admin with least privilege for precise location views.
**Covered by:** FR: FR-RIDE-014; TEST: TEST-RIDE-012
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Role matrix enforced.
- [ ] Location views least-privilege.

## TR-RIDE-SEC-003

**Append-only sensitive access logs** — Immutable append-only logs for views/exports of sensitive location and identity data.
**Covered by:** FR: FR-RIDE-203; TEST: TEST-RIDE-029
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Access events appended immutably.

## TR-RIDE-SERVER-001

**Driver account and consent service** — Public-server driver identity with auth, consent, jurisdiction/purpose, recovery, tenant-scoped authorization.
**Covered by:** FR: FR-RIDE-032; TEST: TEST-RIDE-021
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Account creation with consent and tenant scope.
- [ ] Recovery supported.

## TR-RIDE-SERVER-002

**Vehicle registry and config profiles** — Register vehicles with consent and auditable history; require valid config profile before admitting collection/submission.
**Covered by:** FR: FR-RIDE-033, FR-RIDE-034; TEST: TEST-RIDE-021
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Vehicle registration with change history.
- [ ] Config profile verified before admission.

## TR-RIDE-SERVER-003

**Sealed-only submission API** — Accept only sealed ciphertext plus receipt metadata; reject plaintext and unlinked packages.
**Covered by:** FR: FR-RIDE-035; TEST: TEST-RIDE-022
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Plaintext rejected.
- [ ] Unauthorized driver/vehicle rejected.

## TR-RIDE-SERVER-004

**Admission verifier** — Verify receipt, hash, chain confirmation, attestation, nonce/key binding, package identity before network index admission.
**Covered by:** FR: FR-RIDE-036, FR-RIDE-039; TEST: TEST-RIDE-022, TEST-RIDE-024
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] All listed checks required for admission.
- [ ] Failures quarantine/reject.

## TR-RIDE-SERVER-005

**Abuse controls and backpressure** — Authenticated rate limits, quotas, idempotency, duplicate/replay detection, size limits, quarantine, abuse logs; capacity must not bypass verification.
**Covered by:** FR: FR-RIDE-039, FR-RIDE-218; TEST: TEST-RIDE-024
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Controls enforced under load.
- [ ] Verification never bypassed for capacity.

## TR-RIDE-SERVER-006

**Multi-tenant isolation** — Isolate account, vehicle, config, submission, admission, counsel data; no unauthorized cross-driver queries.
**Covered by:** FR: FR-RIDE-040; TEST: TEST-RIDE-024
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Tenant isolation tests pass.
- [ ] Aggregate indexes require explicit authorization.

## TR-RIDE-SERVER-007

**Counsel multi-driver bundle builder** — Assemble many drivers independently admitted submissions with per-record verification reports.
**Covered by:** FR: FR-RIDE-037, FR-RIDE-038; TEST: TEST-RIDE-023
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Bundle preserves per-record custody.
- [ ] Per-record VerificationReport included.

## TR-RIDE-SERVER-008

**gRPC services on .NET 10 containers** — Host sealed-submit and related public services as gRPC on .NET 10 container images with no decrypt at ingest.
**Covered by:** FR: FR-RIDE-059; TEST: TEST-RIDE-036
**Status:** pending
Scope: layer-1+

## TR-RIDE-SERVER-009

**Fail-closed gRPC admission** — Enforce fail-closed admission verification on gRPC submit and admission RPCs before any store of an admitted package.
**Covered by:** FR: FR-RIDE-061; TEST: TEST-RIDE-036
**Status:** pending
Scope: layer-1+

## TR-RIDE-SERVER-010

**OpenAPI companion non-authoritative** — Keep interim OpenAPI as a human-readable companion only. Bind conformance and codegen to gRPC protos when documents disagree.
**Covered by:** FR: FR-RIDE-062; TEST: TEST-RIDE-037
**Status:** pending
Scope: layer-1+

## TR-RIDE-STORE-001

**Sealed immutable blob store** — Store original imports only as sealed immutable ciphertext blobs plus custody receipts; normalized tables must not replace sealed originals.
**Covered by:** FR: FR-RIDE-013, FR-RIDE-015, FR-RIDE-204, FR-RIDE-206, FR-RIDE-207; TEST: TEST-RIDE-011, TEST-RIDE-013, TEST-RIDE-030, TEST-RIDE-031
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Sealed originals retained immutable.
- [ ] Normalized tables reference but do not rewrite sealed blobs.

## TR-RIDE-STORE-002

**Versioned correction events** — Never rewrite a sealed blob; corrections create new version/event preserving original ciphertext, hash, and receipt.
**Covered by:** FR: FR-RIDE-013, FR-RIDE-015, FR-RIDE-204, FR-RIDE-206, FR-RIDE-207; TEST: TEST-RIDE-011, TEST-RIDE-013, TEST-RIDE-030, TEST-RIDE-031
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Correction creates new version/event.
- [ ] Original ciphertext/hash/receipt preserved.

## TR-RIDE-STORE-003

**Jurisdiction retention engine** — Configurable retention by jurisdiction with legal-hold suspension of deletion.
**Covered by:** FR: FR-RIDE-010, FR-RIDE-208, FR-RIDE-210; TEST: TEST-RIDE-010, TEST-RIDE-032
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Per-jurisdiction retention timers apply.
- [ ] Legal hold suspends deletion.

## TR-RIDE-VIDEO-001

**Dual-phone capture session** — Coordinate two approved phones for one authorized vehicle/session with per-device identity, attestation, camera metadata, timestamps.
**Covered by:** FR: FR-RIDE-041, FR-RIDE-221; TEST: TEST-RIDE-025, TEST-RIDE-027
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Two-phone session established.
- [ ] Per-device metadata retained.

## TR-RIDE-VIDEO-002

**SyncClockOffset service** — Establish shared session clock, record offset/drift/uncertainty, align frames and samples, expose dropped/unsynced intervals.
**Covered by:** FR: FR-RIDE-042, FR-RIDE-221; TEST: TEST-RIDE-025, TEST-RIDE-027
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] SyncClockOffset recorded.
- [ ] Alignment and uncertainty exposed.

## TR-RIDE-VIDEO-003

**On-device composite pipeline** — Produce time-synchronized composite on-device or trusted edge before public submission; no server-side plaintext compositing.
**Covered by:** FR: FR-RIDE-043, FR-RIDE-046; TEST: TEST-RIDE-025, TEST-RIDE-026
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Composite produced before submit.
- [ ] Server never receives plaintext for re-encode/composite.

## TR-RIDE-VIDEO-004

**Spider-graph overlay manifest** — Overlay telematics/accel as spider graph with frame-aligned timestamps and versioned overlay/timeline manifest.
**Covered by:** FR: FR-RIDE-044, FR-RIDE-048; TEST: TEST-RIDE-025, TEST-RIDE-026
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Spider graph frame-aligned.
- [ ] Manifest versioned.

## TR-RIDE-VIDEO-005

**Composite seal and chain receipt** — Seal composite as first-class SealedRecord with custody receipt under same admission policy as sensors.
**Covered by:** FR: FR-RIDE-045; TEST: TEST-RIDE-026
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Composite sealed at device boundary.
- [ ] Chain receipt written under same policy.

## TR-RIDE-VIDEO-006

**Video quota and chunked upload** — Per-driver/session quotas, max sizes, resumable integrity-checked chunked transfer, separate budgets for composite and raw streams.
**Covered by:** FR: FR-RIDE-219, FR-RIDE-220; TEST: TEST-RIDE-033
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Quotas and size limits enforced.
- [ ] Chunk integrity verified; resumable uploads work.

## TR-RIDE-VIDEO-010

**Bluetooth pairing and role protocol** — Implement Bluetooth discovery, driver-rider role negotiation, and secure session binding for DualPhoneSession.
**Covered by:** FR: FR-RIDE-053; TEST: TEST-RIDE-034
**Status:** pending
Scope: layer-1+

## TR-RIDE-VIDEO-011

**Driver coordinator and passenger compositor split** — Driver phone hosts SessionCoordinator; passenger phone hosts VideoSyncJoiner and RealtimeTelematicsOverlay.
**Covered by:** FR: FR-RIDE-054, FR-RIDE-055; TEST: TEST-RIDE-034
**Status:** pending
Scope: layer-1+

## TR-RIDE-VIDEO-012

**Avalonia UI 12 Android capture client** — Build the Android dual-phone capture client on Avalonia UI 12 for driver coordinator and passenger compositor surfaces.
**Covered by:** FR: FR-RIDE-056; TEST: TEST-RIDE-035
**Status:** pending
Scope: layer-1+

## TR-RIDE-VIEW-001

**Cross-platform GPL2 viewer** — Desktop court viewer for Windows, Linux, macOS under GPL-2.0 using only court-authorized escrow release for decryption.
**Covered by:** FR: FR-RIDE-049, FR-RIDE-222; TEST: TEST-RIDE-028
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Builds on Win/Linux/macOS.
- [ ] No escrow bypass.

## TR-RIDE-VIEW-002

**Fail-closed independent verification** — Independently verify receipt, hashes, Play attestation, binding, escrow authorization before decrypt/display; fail closed on any failure.
**Covered by:** FR: FR-RIDE-047, FR-RIDE-050; TEST: TEST-RIDE-027, TEST-RIDE-028
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] All checks run before decrypt/display.
- [ ] Failure blocks render and is auditable.

## TR-RIDE-VIEW-003

**Synchronized RideBundle timeline** — Display composite, telematics, spider graph, GPS, OBD2 on one synchronized timeline independent of collection device.
**Covered by:** FR: FR-RIDE-051; TEST: TEST-RIDE-028
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Timeline includes available assets.
- [ ] Works without original device.

## TR-RIDE-VIEW-004

**ViewerSession and VerificationReport** — Create ViewerSession and VerificationReport per review; publish versioned viewer logic under GPL-2.0.
**Covered by:** FR: FR-RIDE-052; TEST: TEST-RIDE-028
**Status:** pending
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Session and report created every review.
- [ ] Logic versioned and GPL2-published.

## TR-RIDE-VIEW-005

**Avalonia UI 12 desktop court viewer** — Build the cross-platform desktop court viewer on Avalonia UI 12 while retaining escrow-only decrypt and fail-closed verification.
**Covered by:** FR: FR-RIDE-057; TEST: TEST-RIDE-035
**Status:** pending
Scope: layer-1+

