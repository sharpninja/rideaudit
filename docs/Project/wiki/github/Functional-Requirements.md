# Functional Requirements (MCP Server)

## FR-RIDE-001 Ingest Lyft privacy-export ZIP

Ingest Lyft privacy-export ZIP files and parse files according to the included DataDictionary; surface unknown file types as Unverified.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] ZIP upload is accepted for an authenticated consented driver.
- [ ] Files listed in DataDictionary are parsed into structured records.
- [ ] Unknown file types are tagged Unverified and retained without silent discard.

## FR-RIDE-002 Record Smooth Cruiser scores

Record Smooth Cruiser score and component values (gentle braking, smooth steering, phone mount use, vehicle speed relative to area) from structured export fields or consented manual/screenshot entry with timestamp and source tag.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Structured export fields for Smooth Cruiser are stored when present.
- [ ] Manual or screenshot entry requires consent, timestamp, and provenance source tag.
- [ ] Components are stored individually when available.

## FR-RIDE-003 Ingest trip-level records

Ingest trip-level records (start/end time, origin/destination or route summary, distance, earnings metadata) when present in exports or Business reports.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Trip records are created when export or Business report fields are present.
- [ ] Missing trip fields are left null and flagged, not invented.

## FR-RIDE-004 Optional Concierge/Business API integration

Optionally integrate Lyft Concierge/Business API: OAuth client credentials, program linkage, poll /concierge/rides/{id}/status for driver_location during active organizational rides only.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] OAuth client-credentials flow works when Business partnership is approved.
- [ ] Polling is limited to active organizational rides the org booked.
- [ ] No undocumented Lyft endpoints are called. Concierge location is coarse lat/lng only.

## FR-RIDE-005 Import third-party telematics

Import third-party telematics files (CSV/JSON) for GPS track, speed, harsh brake/accel events from devices the driver/fleet controls.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] CSV/JSON imports for supported schemas succeed.
- [ ] Imported events are tagged third_party_telematics.
- [ ] Parallel evidence is not labeled as Lyft-native.

## FR-RIDE-006 Provenance and consent ledger

Maintain a provenance and consent ledger for every dataset (who consented, when, jurisdiction, purpose).
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Every dataset links to consent actor, timestamp, jurisdiction, and purpose.
- [ ] Ledger entries are append-only and auditable.

## FR-RIDE-007 Coverage matrix

Produce a coverage matrix per audit subject listing: collected-by-Lyft vs available-to-auditor vs missing, per signal type.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Matrix lists signal types with collected / available / missing columns.
- [ ] Gaps such as no public Smooth Cruiser API are explicitly shown.

## FR-RIDE-008 Online-hours policy audit

Analyze online-hours against Lyft 12h / 6h break policy and configurable regional overrides; flag apparent violations only when hours data is present.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Policy rules are configurable including regional overrides.
- [ ] Violations are flagged only when hours data exists.
- [ ] FMCSA ELD schemas are not required for typical TNC use (Unverified edge cases).

## FR-RIDE-009 Time-window incident report

Time-window incident report: export GPS/route samples, scores, and third-party events for a selectable interval.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] User can select a time window and export available evidence for that interval.
- [ ] Export includes provenance for each included item.

## FR-RIDE-010 Data subject access and deletion

Support data subject access and deletion workflows for data the audit app stores.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Subject can request access export of audit-held data.
- [ ] Deletion honors legal holds and documents custody impact.

## FR-RIDE-011 No undocumented Lyft private APIs

Do not implement undocumented Lyft private APIs, credential stuffing, or app traffic interception as product features.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Product features list excludes private API scraping and traffic interception.
- [ ] Build and review gates reject such implementations.

## FR-RIDE-012 Admin partnership gates

Admin UI to mark partnerships (Business API approved / denied) and disable Concierge features when ungated access is unavailable.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Admin can set Business API partnership status.
- [ ] Concierge features are disabled when status is denied or unavailable.

## FR-RIDE-013 Hash and version raw imports

Hash and version raw imports; show integrity status on audit reports.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Raw imports receive content hashes and version identifiers.
- [ ] Audit reports display integrity status.

## FR-RIDE-014 Role-based access

Role-based access: auditor, subject (driver), admin; least privilege for precise location views.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Roles auditor, subject, and admin exist with least-privilege defaults.
- [ ] Precise location views require authorized roles.

## FR-RIDE-015 Seal and encrypt at collection

Seal and encrypt every audit datum at the exact collection moment, before any durable store, queue handoff, normalization, or analysis; record the sealing algorithm/version and content hash.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Sealing occurs before durable store, queue, normalization, or analysis.
- [ ] Algorithm/version and content hash are recorded on the sealed record.

## FR-RIDE-016 Per-session or per-sample keys

Generate encryption keys per session or per sample as configured for the evidence type; do not use one long-lived shared key for all records, and bind each key to its declared scope.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Key scope is session or sample per configuration.
- [ ] Long-lived shared keys for all records are rejected.
- [ ] Keys are bound to declared scope.

## FR-RIDE-017 Custody receipt content

Create a custody receipt for every sealed record containing, at minimum, the sealed-payload content hash, the public encryption key used (or key ID plus the public key material), collector identity, collection timestamp, provenance tag, and the blockchain transaction reference once confirmed.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Receipt includes content hash, public key or key ID plus material, collector identity, timestamp, provenance tag.
- [ ] Blockchain transaction reference is recorded when confirmed.

## FR-RIDE-018 Blockchain receipt write

Write each receipt synchronously with, or immediately after, sealing to a configurable public immutable blockchain within the same collection transaction boundary; record chain ID, transaction hash, block height, and write time when confirmed.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Receipt write is synchronous with or immediately after sealing in the same collection boundary.
- [ ] Chain ID, tx hash, block height, and write time are recorded on confirmation.
- [ ] Chain brand is configurable, not hard-coded to one mainnet.

## FR-RIDE-019 Blockchain write failure policy

Apply an explicit failure policy: if the blockchain write or confirmation fails, do not mark collection complete, do not place the sealed record in an admitted state, and quarantine/retry the sealed record and pending receipt without rewriting the ciphertext; surface the failure for operator action and preserve the local failure audit trail.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Failed chain write leaves record non-admitted and not collection-complete.
- [ ] Ciphertext is not rewritten on retry.
- [ ] Operator sees failure and local audit trail is preserved.

## FR-RIDE-020 Court-review decryption path docs

Document and implement the court-review decryption path: identify private-key custodians, legal-process requirements for counsel to obtain plaintext, escrow jurisdiction and controls, dual-control release and attestations, authorized working-copy scope/expiry, and exactly what the chain-of-custody receipt proves (and does not prove).
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Documentation identifies custodians, legal process, escrow jurisdiction, dual-control, working-copy scope/expiry.
- [ ] Receipt proof boundaries are explicit (what it proves and does not prove).
- [ ] Implementation matches documented path.

## FR-RIDE-021 Verification UI/report

Provide a verification UI/report that recomputes the sealed-payload hash, compares it with the custody receipt, checks the on-chain receipt and transaction reference, and displays any mismatch, missing confirmation, or chain status clearly.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] UI recomputes hash and compares to receipt.
- [ ] On-chain receipt and tx reference are checked.
- [ ] Mismatches and missing confirmations are clearly displayed.

## FR-RIDE-022 M-of-N key escrow

Escrow every private key or wrapped data-encryption key (DEK) needed for court decryption using dual control and a configurable M-of-N quorum, so sealed data remains decryptable if the original device or operator is unavailable; preserve the original sealed blob and receipt unchanged.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Every court-needed private key or wrapped DEK is escrowed under M-of-N dual control.
- [ ] Sealed blob and receipt remain unchanged by escrow packaging.

## FR-RIDE-023 Escrow separation from device

Keep escrow material separate from the collection device, operator account, and application database. Document per key whether the escrow package contains quorum-protected private-key shares or a wrapped DEK, while the device retains only the public key/key ID and transient or encrypted recovery material needed for collection; no plaintext private key may persist on-device after sealing.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Escrow material is not stored on device, operator account, or app DB.
- [ ] Device retains only public key/key ID and transient/encrypted recovery material.
- [ ] No plaintext private key persists on-device after sealing.

## FR-RIDE-024 Court-authorized escrow release

Implement a court-authorized escrow-release procedure that verifies the case and legal process, obtains the M-of-N dual-control approvals, releases only the minimum key or wrapped DEK needed, creates an append-only release log with approvers, shares/use, timestamps, and purpose, and never changes the sealed ciphertext, content hash, or on-chain receipt.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Release verifies case and legal process and obtains M-of-N approvals.
- [ ] Only minimum key/DEK scope is released.
- [ ] Append-only release log is created; sealed ciphertext/hash/receipt unchanged.

## FR-RIDE-025 Play Integrity key binding

Use Google Play Store app signing and/or Play Integrity API attestation as part of the encryption-key authentication chain. At collection, bind the generated key to the approved package identity and signing-certificate digest or a nonce-bound Integrity attestation, and retain evidence sufficient for later court verification.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Key generation binds to approved package identity and cert digest or nonce-bound Integrity attestation.
- [ ] Attestation evidence sufficient for court verification is retained.

## FR-RIDE-026 Reject failed Play Integrity

Verify the Play Integrity/signing-certificate result before key generation and sealing; reject collection when the device is compromised, the binary is modified or unofficial/sideloaded, the package or certificate is not approved, the attestation is stale or nonce-mismatched, or the verdict cannot be validated. Do not admit a record from a failed-attestation attempt.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Integrity/signing check runs before key generation and sealing.
- [ ] Failed, stale, sideloaded, or unverifiable attestations reject collection.
- [ ] Failed-attestation attempts are not admitted.

## FR-RIDE-027 Attestation on custody receipt

Include the app-attestation evidence or a cryptographic hash of it, plus the package identity, signing-certificate digest or attestation reference, in the custody receipt written on-chain and link it to the KeyMaterial and SealedRecord.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] On-chain receipt includes attestation evidence or hash plus package identity and cert digest/reference.
- [ ] Receipt links to KeyMaterial and SealedRecord.

## FR-RIDE-028 Counsel verification steps

Provide counsel-verification steps that validate the public-chain receipt and payload hash, verify the Play Integrity evidence/signing certificate against the approved Google Play app identity and collection nonce/key binding, confirm the attestation timestamp and verdict, verify escrow quorum and release logs, and then document authorized decryption of an expiring working copy.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Counsel flow validates chain receipt, payload hash, Play attestation, escrow quorum/release logs.
- [ ] Authorized decryption of expiring working copy is documented.

## FR-RIDE-029 GPL-2.0 licensing

License the application, custody/verification schemas and tooling, and crowdsourced evidence-network code under GPL-2.0 (GPL2); publish the corresponding source and license notices so derivative works and forks are also open-sourced under GPL2.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Application, schemas, tooling, and network code are GPL-2.0.
- [ ] Source and license notices are published.
- [ ] Apache-2.0 or MIT are not substituted for these components.

## FR-RIDE-030 GPL2 notices on artifacts

Attach a GPL2 license/version and source-commit notice to shared software, schema, receipt, attestation, and verification artifacts, while ensuring that crowdsourced evidence payloads remain sealed and subject to consent, privacy controls, access policy, and legal holds rather than being exposed merely because the code is open source.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Shared software/schema/receipt/attestation/verification artifacts carry GPL2 and commit notice.
- [ ] Sealed evidence payloads remain access-controlled despite open-source code.

## FR-RIDE-031 Publish client via Play and source repo

Publish a downloadable GPL-2.0 client through Google Play Store distribution and a public source repository, with reproducible/versioned build and signing metadata.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Client is available on Google Play and public source repository.
- [ ] Build and signing metadata are versioned and reproducible.

## FR-RIDE-032 Public-server driver account

Provide a public-server driver account identity with authentication, consent records, jurisdiction/purpose, account recovery, and tenant-scoped authorization.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Driver can create authenticated account with consent, jurisdiction, purpose.
- [ ] Authorization is tenant-scoped.
- [ ] Account recovery is supported.

## FR-RIDE-033 Vehicle registration

Register each vehicle to an authorized driver account using VIN, plate, make/model, or the minimum appropriate attributes, with driver consent and an auditable change history.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Vehicle attributes are registered with consent to a driver account.
- [ ] Change history is auditable.

## FR-RIDE-034 Configuration profile gate

Require a valid configuration profile for the registered vehicle and verify that configuration before admitting a collection session or submission.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Valid config profile is required before admitting collection or submission.
- [ ] Invalid or missing profile blocks admission.

## FR-RIDE-035 Sealed-only submission API

Make the public submission API accept only already-sealed ciphertext packages plus custody-receipt metadata; reject plaintext, unsealed sensor/video data, missing receipts, or packages that cannot be tied to an authorized driver and vehicle.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] API rejects plaintext and unsealed payloads.
- [ ] Missing receipts or unauthorized driver/vehicle packages are rejected.

## FR-RIDE-036 Admission verification chain

Integrate public-server admission with the existing seal, blockchain custody receipt, escrow, and Play Integrity/app-signing chain; verify receipt fields, payload hash, chain confirmation, app attestation, nonce/key binding, and approved package identity before adding a submission to the network index.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Admission verifies receipt, hash, chain confirmation, attestation, nonce/key binding, package identity.
- [ ] Failed verification leaves package rejected or quarantined, not admitted.

## FR-RIDE-037 Multi-driver per-record provenance

Preserve court multi-driver provenance: each submission must be independently verifiable through device attestation, collector_id, driver_id, vehicle_id, and its on-chain receipt; aggregation must not weaken per-record custody.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Each submission is independently verifiable via attestation, collector_id, driver_id, vehicle_id, and receipt.
- [ ] Aggregation does not replace per-record custody.

## FR-RIDE-038 Counsel multi-driver bundle

Generate a counsel bundle containing submissions from many drivers while providing a per-record verification report for each sealed record, receipt, attestation, vehicle registration, and escrow/decryption path.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Counsel bundle can include many drivers submissions.
- [ ] Each record has its own verification report covering receipt, attestation, vehicle, escrow path.

## FR-RIDE-039 Public-server abuse controls

Enforce public-server abuse controls: authenticated rate limits and quotas, idempotency, duplicate/replay rejection, tampered-attestation rejection, unregistered-vehicle rejection, failed-Integrity rejection, size limits, quarantine, and abuse/audit logs.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Rate limits, quotas, idempotency, and size limits are enforced.
- [ ] Duplicates, replays, tampered attestations, unregistered vehicles, failed Integrity are rejected.
- [ ] Abuse/audit logs are written.

## FR-RIDE-040 Multi-tenant isolation

Enforce strict multi-tenant isolation for account, vehicle, configuration, submission, admission, and counsel data; expose only authorized per-driver records and explicitly approved aggregate indexes.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Tenant data is isolated across account/vehicle/config/submission/admission/counsel scopes.
- [ ] Cross-driver queries require explicit authorization.

## FR-RIDE-041 Dual-phone video capture

Support two approved phones collecting video for the same authorized vehicle/session, retaining each source stream device identity, app attestation, camera metadata, and capture timestamps.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Two approved phones can collect for one authorized session.
- [ ] Each stream retains device identity, attestation, camera metadata, timestamps.

## FR-RIDE-042 Shared session clock sync

Establish a shared session clock between phones, record SyncClockOffset and drift/uncertainty, and align video frames, telematics, and accelerometer samples to that clock, including dropped-frame or unsynchronized intervals.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Shared session clock and SyncClockOffset/drift are recorded.
- [ ] Frames and samples are aligned; dropped or unsynced intervals are exposed.

## FR-RIDE-043 On-device/edge compositing

Run an on-device or trusted-edge compositing pipeline that produces one time-synchronized combined video stream from both source streams before public submission.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Composite is produced on-device or trusted edge before public submission.
- [ ] Public server does not re-encode or composite plaintext.

## FR-RIDE-044 Spider-graph overlay

Overlay telematics and accelerometer data in real time as a spider graph below the corresponding video stream, with frame-aligned timestamps and a versioned overlay/timeline manifest.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Spider graph overlays telematics/accel below corresponding video.
- [ ] Overlay/timeline manifest is versioned and frame-aligned.

## FR-RIDE-045 Seal composite as first-class evidence

Seal and encrypt the composited artifact at the collection-device boundary as a first-class SealedRecord, generate its custody receipt, and write the receipt to the configured public immutable blockchain under the same admission policy as raw sensor data.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Composite is sealed at collection-device boundary as SealedRecord.
- [ ] Custody receipt is written to configured chain under same admission policy as sensors.

## FR-RIDE-046 Optional raw stream sealing

Optionally seal each raw source video stream as its own first-class sealed artifact, linked to the composite, when the driver consents and storage/bandwidth/retention budgets allow; never replace a sealed source or composite by re-encoding plaintext on the public server.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Optional raw streams can be sealed and linked to composite with consent and budget checks.
- [ ] Public server never re-encodes plaintext to replace sealed artifacts.

## FR-RIDE-047 Counsel composite playback

Provide counsel playback of an authorized expiring working copy of the sealed composite only after verifying its payload hash, blockchain receipt, source-stream links, clock offsets, overlay timeline, and app-attestation evidence; clearly report any missing or inconsistent component.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Playback requires verification of hash, receipt, source links, clock offsets, overlay, attestation.
- [ ] Missing/inconsistent components are clearly reported; working copy expires.

## FR-RIDE-048 Composite metadata in custody package

Include composite, source-stream, synchronization, overlay, codec, compression, dropped-frame, and device metadata in the custody and verification package without exposing plaintext to the public submission server at ingest.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Custody/verification package includes listed metadata fields.
- [ ] Public ingest receives no plaintext evidence.

## FR-RIDE-049 GPL-2.0 desktop court viewer

Provide a GPL-2.0 desktop court viewer for Windows, Linux, and macOS that decrypts sealed ride bundles only through the court-authorized escrow release path and never bypasses escrow controls.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Viewer runs on Windows, Linux, and macOS under GPL-2.0.
- [ ] Decryption uses only court-authorized escrow release path.

## FR-RIDE-050 Viewer fail-closed verification

Have the desktop viewer independently verify the blockchain custody receipt, payload hashes, Google Play signing certificate/Play Integrity attestation, nonce/key binding, and escrow release authorization before decrypting or displaying any data; fail closed on any verification failure.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Viewer independently verifies receipt, hashes, Play attestation, binding, escrow authorization before decrypt/display.
- [ ] Any failure prevents decrypt/display and is auditable.

## FR-RIDE-051 Synchronized timeline display

Display all collected data on a synchronized timeline, including composite video, telematics, accelerometer spider graph, GPS, and OBD2 where available, independently of the original collection device.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Timeline shows composite, telematics, spider graph, GPS, and OBD2 when available.
- [ ] Viewer operates without the original collection device.

## FR-RIDE-052 ViewerSession and VerificationReport

Create an auditable ViewerSession and VerificationReport for every review, and publish versioned decryption, verification, synchronization, overlay, and rendering logic under GPL-2.0 so judge or opposing counsel can independently inspect the process.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Every review creates ViewerSession and VerificationReport.
- [ ] Decryption/verification/sync/overlay/rendering logic is versioned and GPL-2.0 published.

## FR-RIDE-053 Bluetooth driver-rider phone pairing

Approved phones discover each other over Bluetooth and establish an authenticated driver-rider pairing for one authorized vehicle/session before dual capture begins.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Phones discover peers via Bluetooth and only complete pairing when both sides confirm driver vs passenger role and shared session intent.
- [ ] Pairing fails closed if Bluetooth discovery, role confirmation, or session binding fails.

## FR-RIDE-054 Driver phone session coordination

The driver phone is the session coordinator. It owns session start/stop, shared clock mastership, admission readiness checks, and submission orchestration for the paired session.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Only the driver-role phone may start or stop an admitted dual-phone session after pairing.
- [ ] Driver phone publishes the session clock and coordination commands to the passenger phone over the paired link.

## FR-RIDE-055 Passenger phone video sync join and telematics overlay

The passenger phone performs video synchronization, joins/composites the dual streams, and adds realtime telematics (including accelerometer spider graph) aligned to the shared session clock.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Passenger phone syncs video to the driver-published session clock and records SyncClockOffset.
- [ ] Passenger phone joins streams into the composite and overlays realtime telematics before seal-at-collect on the composite path.

## FR-RIDE-056 Avalonia UI 12 Android dual-phone capture client

Implement the Android dual-phone capture client (driver coordinator and passenger compositor) with Avalonia UI 12 as the sole UI framework for capture surfaces.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Android capture client builds and runs with Avalonia UI 12 for driver and passenger roles.
- [ ] Capture UX does not depend on a non-Avalonia UI framework for primary screens.

## FR-RIDE-057 Avalonia UI 12 desktop court viewer

Implement the desktop court and counsel review app with Avalonia UI 12 on Windows, Linux, and macOS, preserving escrow-only decrypt and fail-closed verification behavior.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Desktop court viewer builds with Avalonia UI 12 on Windows, Linux, and macOS.
- [ ] Viewer keeps escrow-only decrypt and fail-closed verification (FR-RIDE-049, FR-RIDE-050).

## FR-RIDE-058 Shared Avalonia UI 12 constraints under GPL-2.0

Share Avalonia UI 12 controls, themes, and client libraries across Android capture and desktop review under GPL-2.0. Do not introduce a conflicting UI license for in-scope RideAudit application UI code.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Shared Avalonia UI 12 client libraries are licensed and published under GPL-2.0 with notices.
- [ ] Android and desktop clients consume the shared Avalonia stack without substituting a non-GPL-2.0 UI license for in-scope app UI.

## FR-RIDE-059 Backend gRPC on .NET 10 containers

Expose the public RideAudit backend as gRPC services hosted on .NET 10 and deployed as containers. Accept only sealed ciphertext packages plus custody-receipt metadata at ingest. Never decrypt sealed payloads at public-server ingest.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Public backend services are gRPC on .NET 10 and ship as container images.
- [ ] Ingest accepts only sealed ciphertext plus custody-receipt metadata and never decrypts at ingest.

## FR-RIDE-060 Proto and schema publication under GPL-2.0

Publish authoritative gRPC protobuf contracts and related custody or verification schemas under GPL-2.0 with versioned source and license notices so clients and forks can interoperate without inventing private Lyft APIs.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Proto files and related schemas are published under GPL-2.0 with version and license notices.
- [ ] Published contracts do not invent undocumented Lyft private APIs.

## FR-RIDE-061 Fail-closed admission over gRPC

Apply existing fail-closed admission verification (receipt, hash, chain, attestation, binding, authorized driver or vehicle) to all gRPC submit and admission RPCs. Reject and do not store on any verification failure.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] gRPC submit and admission RPCs run the same fail-closed admission checks as FR-RIDE-036.
- [ ] Any admission failure rejects the RPC without storing an admitted sealed package.

## FR-RIDE-062 Interim OpenAPI companion non-authoritative

Treat any interim OpenAPI description as a non-authoritative human-readable companion. gRPC protobuf contracts are the sole authoritative service API for clients and admission behavior.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Documentation and ARTIFACT metadata state that OpenAPI is non-authoritative versus gRPC protos.
- [ ] Client and server conformance tests bind to gRPC contracts, not OpenAPI, when they disagree.

## FR-RIDE-201 TLS and secrets vault

Security: encryption in transit (TLS 1.2+) and at rest; secrets in a vault; no plaintext API tokens in logs.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] TLS 1.2+ enforced for network traffic.
- [ ] Secrets stored in vault; no plaintext API tokens in logs.

## FR-RIDE-202 Geolocation sensitive masking

Privacy-by-design: precise geolocation fields classified as sensitive; default UI masks exact coordinates except for authorized roles.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Precise geolocation classified as sensitive PI.
- [ ] Default UI masks exact coordinates except authorized roles.

## FR-RIDE-203 Append-only access logs

Auditability: immutable append-only access logs for views/exports of sensitive location and identity data.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Views/exports of sensitive location and identity write append-only access logs.

## FR-RIDE-204 Concierge ingestion resilience

Reliability: Concierge ingestion must tolerate API rate limits and partial outages without corrupting stored rides.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Rate-limit and outage conditions do not corrupt stored rides.
- [ ] Retries are idempotent.

## FR-RIDE-205 Scale multi-year histories

Scalability: support multi-year trip histories and multi-Hz GPS tracks from third-party devices without UI freezes (paginate / downsample for display).
Scope: layer-1+
**Acceptance Criteria:**
- [ ] UI paginates or downsamples large GPS tracks.
- [ ] Multi-year histories load without UI freezes.

## FR-RIDE-206 No false Smooth Cruiser labeling

Accuracy: never present inferred speed/brake events from sparse Concierge lat/lng polls as Lyft Smooth Cruiser equivalents; label derived metrics clearly.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Derived metrics from Concierge lat/lng are labeled as derived, not Smooth Cruiser.
- [ ] No false equivalence claims.

## FR-RIDE-207 Portable audit ZIP export

Portability: export audit packages as ZIP (CSV + PDF summary + provenance JSON).
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Export produces ZIP with CSV, PDF summary, and provenance JSON.

## FR-RIDE-208 Per-state retention config

Compliance config: per-state retention and deletion timers; California sensitive-PI handling as default strict profile.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Retention/deletion timers are per-state configurable.
- [ ] CA sensitive-PI strict profile is default.

## FR-RIDE-209 In-product API gap notice

Documentation: in-product API gap notice stating Lyft does not offer a public driver telematics API (as of research date).
Scope: layer-1+
**Acceptance Criteria:**
- [ ] In-product notice states no public driver telematics API as of research date 2026-09-27.

## FR-RIDE-210 Legal hold suspends deletion

Legal hold: ability to suspend deletion for designated cases under counsel instruction.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Counsel can designate legal hold that suspends deletion for a case.

## FR-RIDE-211 Cryptographic agility

Cryptographic agility: use versioned, configurable algorithms, key formats, and receipt schemas so cryptographic suites can be rotated or replaced without rewriting sealed records; preserve the algorithm identifier with each record.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Algorithms/key formats/receipt schemas are versioned and configurable.
- [ ] Sealed records retain algorithm identifier; rotation does not rewrite ciphertext.

## FR-RIDE-212 Configurable public chain

Chain choice configurability: support a configurable public immutable chain (or approved set of chains) by environment and jurisdiction; do not make any specific blockchain brand or mainnet mandatory, and preserve chain ID/transaction metadata for verification.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Chain is configurable by environment/jurisdiction.
- [ ] No single chain brand is mandatory.
- [ ] Chain ID and tx metadata preserved.

## FR-RIDE-213 Seal/receipt latency budget

Latency budget: define, measure, and monitor a configurable per-connector budget for sealing, receipt creation, and blockchain write/confirmation; collection completion and admitted state remain blocked until the configured receipt confirmation policy is satisfied.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Per-connector latency budgets are defined and monitored.
- [ ] Admission blocked until confirmation policy satisfied.

## FR-RIDE-214 HSM/KMS key custody

Key custody: private keys must be generated, stored, and used through an HSM/KMS or equivalent controlled service, never in the application database; support separation of duties, dual control, rotation, escrow, access logging, and documented legal-release procedures.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Private keys never stored in application database.
- [ ] HSM/KMS or equivalent used with dual control, rotation, escrow, access logging.

## FR-RIDE-215 Play authenticity allowlist

App authenticity: enforce a versioned allowlist of Google Play package identities and signing-certificate digests, or an equivalent Play Integrity verification policy; reject failed, tampered, unofficial, sideloaded, stale, or unverifiable attestations before collection is admitted, and support auditable certificate/key rotation.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Versioned allowlist of package identities and cert digests enforced.
- [ ] Failed/tampered/sideloaded/stale attestations rejected before admission.
- [ ] Cert/key rotation is auditable.

## FR-RIDE-216 Escrow resilience

Escrow resilience: maintain encrypted M-of-N escrow shares or wrapped-DEK recovery packages independently of collection devices and operators, with tested recovery, quorum enforcement, separate custodians, and append-only release logging that cannot alter sealed evidence.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Escrow packages independent of devices/operators.
- [ ] Recovery tested; quorum enforced; release logging append-only and non-mutating of evidence.

## FR-RIDE-217 GPL-2.0 governance NFR

Open-source governance: publish the application, custody/verification schemas, and crowdsourced evidence-network implementation under GPL-2.0 (GPL2), including source, notices, and build/version provenance; do not introduce Apache-2.0 or MIT as an alternative license for these components.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] GPL-2.0 published with source, notices, build/version provenance.
- [ ] Apache-2.0/MIT not used as alternative for these components.

## FR-RIDE-218 Public-server admission capacity

Public-server admission: enforce tenant isolation, authenticated rate limits, quotas, resumable/idempotent uploads, duplicate/replay detection, abuse throttling, and backpressure; admission capacity must not bypass receipt, attestation, or seal verification.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Isolation, rate limits, quotas, resumable/idempotent uploads, replay detection, backpressure enforced.
- [ ] Capacity controls never bypass receipt/attestation/seal verification.

## FR-RIDE-219 Video storage and bandwidth quotas

Video storage and bandwidth: define per-driver/session quotas and maximum composite/raw-stream sizes; use documented lossless or approved lossy compression, resumable chunked transfer, integrity-checked chunks, retention tiers, lifecycle deletion/legal holds, and separate capacity budgets for composite output and optional raw streams.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Per-driver/session quotas and max sizes defined.
- [ ] Resumable chunked transfer with integrity-checked chunks.
- [ ] Separate budgets for composite and raw streams.

## FR-RIDE-220 Video performance thresholds

Video performance: measure device/edge CPU, memory, battery, thermal load, encoding latency, clock-sync error, frame drops, public-server ingress/egress, and counsel playback bandwidth; do not admit a composite whose synchronization or sealing quality falls below configured thresholds.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Listed performance metrics are measured.
- [ ] Composites below sync/seal quality thresholds are not admitted.

## FR-RIDE-221 Composite integrity for playback

Composite integrity and playback: preserve codec/compression and overlay-manifest versions, source-to-output hashes, sync offsets, frame/timestamp mapping, and verification status so counsel can reproduce the integrity check before playback.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Codec, overlay versions, hashes, sync offsets, frame mapping, verification status preserved.
- [ ] Counsel can reproduce integrity check before playback.

## FR-RIDE-222 Desktop portability fail-closed

Desktop portability and fail-closed behavior: test signed/reproducible GPL2 builds on Windows, Linux, and macOS; a viewer must not decrypt or render when receipt, attestation, escrow-release, hash, or bundle verification fails.
Scope: layer-1+
**Acceptance Criteria:**
- [ ] Signed/reproducible GPL2 builds tested on Windows, Linux, macOS.
- [ ] Viewer does not decrypt/render on any verification failure.

