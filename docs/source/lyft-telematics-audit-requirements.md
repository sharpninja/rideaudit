# Lyft Driver Telematics Audit Application — Requirements

**Document purpose:** Requirements for an application Payton Byrd will use to audit Lyft driver telematics.  
**Research date:** 2026-09-27 (America/Chicago).  
**Scope:** US-focused unless noted. Unverified items are marked **Unverified**.

---

## 1. Executive summary

Lyft collects rich driver telematics from the driver’s smartphone (precise GPS, IMU/gyroscope, speed, acceleration, deceleration, direction, and related signals) and uses that data internally for Smooth Cruiser safety scoring, real-time speeding alerts, claims investigation, insurance, and driver ranking. Drivers see *derived* metrics in-app (Smooth Cruiser components, Feedback & Insights hub, online-hours limits), not raw millisecond sensor streams. Official Lyft APIs that third parties can obtain today are gated Lyft Business / Concierge ride-booking APIs; they expose ride status and coarse real-time driver lat/lng for booked rides—not braking, acceleration, Smooth Cruiser scores, hours-online history, or vehicle diagnostics. There is **no public driver telematics API**. Driver privacy exports (`account.lyft.com/privacy/data/download`) provide personal data packages but are **Unverified** to include raw telematics or Smooth Cruiser event logs. An audit app must therefore plan for (a) consented driver exports / manual capture of in-app scores, (b) optional Lyft Business partnership for ride-level location only, and/or (c) independent third-party telematics (phone GPS apps, OBD devices, dashcams) as parallel evidence—not as Lyft-native feeds. Compliance centers on precise geolocation as sensitive personal information under CCPA/CPRA and similar state laws, Lyft’s own retention (≥7 years for transactional ride data), and limited FMCSA HOS applicability to typical TNCs. For court-admissible audit evidence, every audit datum must be sealed and encrypted at the exact moment of collection, before durable storage or queue handoff, and its custody receipt—including the public encryption key used—must be written synchronously or immediately to a configurable public immutable blockchain. The system must also document the court-review decryption path, including private-key custody, legal-process release of plaintext, escrow or dual-control, and what the custody receipt proves.

The fully open-source GPL-2.0 application includes a public server that any driver can use to download the client, configure it for their own vehicle, and submit sealed encrypted audit data for their own routes and vehicles; it is designed as a crowdsourced evidence network rather than a single-driver repository. Court provenance remains multi-device and per-submission: each record carries its seal, on-chain custody receipt, Google Play attestation, and escrow-backed decryption path.

---

## 2. Telematics data landscape

### 2.1 Data Lyft is known to collect

| Signal / category | Source of truth | Notes |
| --- | --- | --- |
| Precise location (GPS, Wi‑Fi, Bluetooth, IP) | Lyft Privacy Policy (updated Jul 1, 2026) | Drivers: collected while app open/in background; also for a limited time after exiting driver mode for incident detection; optional Lyft dashboard/tablet devices may collect location when on. |
| Mobile sensor data: speed, direction, height/altitude, acceleration, deceleration, other technical data | Privacy Policy § Device Information (Drivers) | Phone-based, not vehicle OBD. |
| Millisecond-level IMU, gyroscope, GPS | Lyft Road Safety & Telematics engineering materials (job/product descriptions, ~2025–2026) | Internally processed via DAG/ML pipelines into harsh-event and risk features; petabyte-scale. |
| Ride usage: date/time, destination, distance, route, payment | Privacy Policy § Usage Information | Transactional trip records. |
| Inferences / safety risk features | Privacy Policy + Telematics team descriptions | Used for ranking, rewards, off-boarding unsafe drivers, claims tools, insurance. |
| Dashboard recording device registration | Privacy Policy | If driver registers a dashcam/recording device, Lyft may receive device metadata (e.g., serial number)—not necessarily continuous video via API. |
| Vehicle diagnostics (OBD, engine codes, VIN live telemetry) | — | **Not documented** as collected via the Lyft Driver app. Vehicle info is mostly registration/insurance documents. Treat OBD as **not Lyft-native**. |

**Internal products powered by telematics (collected → processed, not necessarily exported):**

- **Smooth Cruiser** components: gentle braking, smooth steering/turning, phone mount use while moving, vehicle speed vs area average.
- **Real-time speeding alerts** (optional): alert when exceeding speed limit by ≥5 mph with a rider.
- **Vision 360** safety reminders (e.g., hand-held phone use campaigns).
- Claims / insurance reconstruction tools (internal; legal discovery often required for third parties).

### 2.2 Data exposed to drivers / in products

| Exposed to driver | Where | Granularity |
| --- | --- | --- |
| Smooth Cruiser score (≥75 typically required for Lyft Rewards) | Driver app → Feedback & Rewards / Lyft Rewards | Aggregate score across four habits; not raw event timelines publicly documented. |
| Feedback & Insights hub metrics (e.g., safety, cleanliness, star rating over last 100 rides) | Driver app | Rolling window summaries. |
| Rating, acceptance rate, cancellation rate, safety flags | Lyft Rewards / Insights | Last ~100 rides for several stats. |
| Online / driver-mode time limits | Earnings tab (regions without local override); system blocks after 12h driver mode until 6h uninterrupted break | Policy-level hours, not FMCSA ELD logs. Airport queue time (up to 8h) often excluded from the 12h clock. Local regs may tighten limits. |
| Real-time speeding alerts | Optional in-app | Event alerts; exportability **Unverified**. |
| Trip / earnings history, tax docs | Driver dashboard / Tax Center | Financial and trip summaries; not full telematics. |
| Privacy data download package | `https://account.lyft.com/privacy/data/download` | ZIP with DataDictionary (**Unverified** whether Smooth Cruiser events / high-rate GPS are included). |

### 2.3 Official APIs and partner access (availability and gaps)

**Critical finding:** Lyft does **not** publicly document or offer a third-party API that returns driver telematics (harsh braking, accel, Smooth Cruiser, IMU streams, hours-online history, or vehicle diagnostics).

| Access path | What it provides | Telemetry relevance | Access gate |
| --- | --- | --- | --- |
| **Lyft Concierge / Business APIs** (`api.lyft.com`, OAuth client credentials + Business Portal program link) | Create/list/cancel rides; ride detail; **GET `/concierge/rides/{id}/status`** with `driver_location.lat/lng`, `eta_seconds`, status enum | Coarse real-time location for *that booked ride only*—no speed/brake/accel scores | Requires Lyft Business relationship; `www.lyft.com/developers` is sign-in gated; legacy `developer.lyft.com` no longer resolves |
| **Legacy public Rides / Drivers / ETA / Cost APIs** | Historical SDKs (e.g., lyft-node-sdk) listed nearby drivers, ETAs, user rides | Deprecated / unsupported; not a telematics audit feed | Do not rely on for new builds |
| **GBFS micromobility feeds** (`gbfs.lyft.com`) | Bike/scooter fleet status | Irrelevant to car-driver telematics | Public |
| **Business Portal / SFTP transaction reports** | Ride billing/transaction reports for enterprise programs | Financial/trip metadata, not sensor telematics | Enterprise customers |
| **Driver privacy export** | Account data ZIP | Possible trip/account fields; raw telematics inclusion **Unverified** | Driver-authenticated self-service |
| **Legal process / claims** | Detailed GPS/telematics for incidents | Often via subpoena / preservation letters; not an app integration | Counsel / courts |

**Explicit gaps (do not invent endpoints):**

1. No public Smooth Cruiser / safety-score API.  
2. No public harsh-event or IMU stream API.  
3. No public driver hours-of-service / online-time history API.  
4. No public vehicle OBD/diagnostics API from Lyft.  
5. Concierge status location is for rides *your organization booked*, not arbitrary drivers’ fleets.

### 2.4 Third-party / alternative data sources

| Source | Role for an audit app | Caveat |
| --- | --- | --- |
| **Driver-consented phone GPS / mileage apps** (e.g., Everlance—Lyft Rewards partner for mileage discounts) | Independent mileage/route/time evidence; CSV/Excel/PDF exports | Not Lyft’s Smooth Cruiser; no official Everlance public REST API for third-party apps (**Unverified** partner sync for auditors) |
| **Hardware telematics** (Geotab, Samsara, Motive, OBD-II dongles, dashcams such as Lytx) | Full fleet-style telematics if the driver/vehicle installs them | **No documented native Lyft↔Geotab/Samsara/Motive rideshare telematics partnership** for pulling Lyft’s internal scores; parallel evidence only |
| **Flexdrive / AV fleet partners** | Fleet ops for Lyft-related vehicles / AV programs | Enterprise/partner only; not a consumer driver audit API |
| **Manual / screenshot capture of in-app Smooth Cruiser & Insights** | Practical short-term audit input | Fragile; needs consent, authenticity controls, retention rules |
| **Legal discovery from Lyft** | Gold-standard for crash reconstruction | Out of band for a continuous audit product |

---

## 3. Audit application capabilities needed

### 3.1 Ingestion

- **Court-admissibility collection boundary:** At the exact moment each audit datum is collected, seal and encrypt it before any durable store, queue handoff, normalization, or analysis. Generate the custody receipt and write it synchronously with, or immediately after, sealing within the same collection transaction boundary; do not mark collection complete or admit the record without a confirmed receipt.
- **Primary (driver-owned):** Authenticated upload of Lyft privacy-export ZIP; structured parse of DataDictionary-described files; optional OCR/manual entry of Smooth Cruiser and Insights screenshots with provenance metadata.
- **Secondary (enterprise):** Optional Concierge/Business API client for organizations that book rides—ingest ride IDs, statuses, and driver_location polls during active rides only.
- **Parallel telematics:** Connectors or file imports for third-party GPS/OBD/dashcam CSV/API exports when the subject vehicle uses them.
- **Consent & identity:** Bind every dataset to a verified driver identity, consent record, jurisdiction, and collection purpose (audit).
- **Honesty layer:** Tag each field with provenance enum: `lyft_privacy_export` | `lyft_concierge_api` | `in_app_manual` | `third_party_telematics` | `unverified`.

### 3.2 Storage

- Store original raw imports (ZIP, CSV, screenshots) only as sealed, immutable ciphertext blobs plus their custody receipts; normalized relational/time-series tables (trips, location samples, score snapshots, online-hours intervals) must not replace or rewrite the sealed originals.
- Store only sealed ciphertext and receipts for original evidence; keep private-key custody separate from the application database and record access logging for key use and evidence access.
- Never rewrite a sealed blob. Corrections, reprocessing, or new metadata create a new version/event while preserving the original ciphertext, hash, and receipt.
- Retention policies configurable by jurisdiction (default align with audit need, not indiscriminate copy of Lyft’s ≥7-year transactional retention).
- Support deletion/export to honor CCPA/CPRA and driver requests, subject to legal holds and the documented custody record.

### 3.3 Analysis / audit workflows

- Analysis operates only on authorized decrypted working copies under an audit trail; originals remain sealed and immutable, and working copies have explicit expiry and access records.
- Reconstruct trip timelines from available location/route data.
- Compare driver-reported Smooth Cruiser components over time; flag score drops, safety flags, speeding-alert periods (**where data exists**).
- Hours-online audit: detect patterns relative to Lyft’s 12h / 6h break rule and region overrides (from policy + any exported online intervals).
- Gap analysis report: explicitly list signals Lyft collects but that the audit package **lacks**.
- Incident package builder: assemble available GPS/route + scores + third-party telematics for a time window (for counsel handoff—not a substitute for Lyft’s internal claims tools).
- Diff / integrity checks: hash raw imports; detect re-uploads and tampering.

### 3.4 Public crowdsourced server architecture

- **Public, multi-tenant network:** Operate a public server that any driver can use after self-registration. Each tenant's account, vehicle registrations, configuration profiles, submissions, keys/escrow references, and access logs are isolated from every other driver.
- **Driver self-service:** A driver downloads the GPL-2.0 client from the Google Play Store or the published source repository, creates an account, records consent and purpose, and manages their own registered vehicles.
- **Vehicle registry and configuration:** Register VIN, plate, make/model, and other vehicle attributes only as appropriate and with consent. A versioned configuration profile is selected and validated for the registered vehicle before collection is admitted.
- **Sealed-only submission API:** The public server accepts only an already-sealed encrypted package plus custody-receipt metadata, including the content hash, collector identity, driver and vehicle references, chain transaction, and attestation evidence/reference. It never sees plaintext at ingest, never decrypts or re-encodes evidence, and never accepts a raw sensor/video payload in place of a sealed package.
- **Admission verification:** Before indexing a submission in the network, the server verifies required receipt fields, payload hash, chain receipt/confirmation, app package/signing or Play Integrity attestation, nonce/key binding, driver authorization, registered vehicle, and configuration profile. Failed verification leaves the package rejected or quarantined, not admitted.
- **Abuse controls:** Apply authenticated rate limits, quotas, size limits, duplicate/replay detection, idempotency keys, proof-of-work or other anti-spam controls as appropriate, moderation/reporting hooks, and operational abuse response. Reject duplicate or replayed packages, tampered attestations, unregistered vehicles, and failed Integrity verdicts.
- **Privacy and isolation:** Keep sealed ciphertext and minimum admission metadata separate from account/profile data; enforce tenant-scoped authorization, encryption in transit, append-only admission logs, legal holds, deletion/retention policy, and no cross-driver queries except explicitly authorized aggregate or counsel-bundle workflows.
- **Counsel aggregation:** The network may assemble many drivers' independently admitted submissions into a counsel package, but every record retains its own receipt, attestation, collector ID, vehicle ID, and verification report. Aggregation is an index and presentation function, not a replacement for per-record custody.

### 3.5 Dual-phone video and composited evidence stream

- **Dual capture:** Two approved phones collect synchronized video evidence for the same authorized route/vehicle session, with each phone retaining its own device, camera, app-attestation, and clock metadata.
- **Clock synchronization:** Establish and record a shared session clock and per-device offset/drift estimates; align frames and telematics samples to the synchronized timeline and expose uncertainty or dropped-frame intervals.
- **On-device/edge compositing:** Before submission, an on-device or trusted edge pipeline combines the two streams into one time-synchronized composite. The composite places each corresponding video view together and displays telematics and accelerometer values in a real-time spider graph below the corresponding video stream, aligned frame-by-frame with the camera imagery.
- **Sealed output:** Seal and encrypt the composited output at the collection-device boundary and issue its blockchain custody receipt exactly like raw sensor data. The public server receives only the sealed composite package; it must not receive plaintext for server-side re-encoding or compositing. Raw source streams may also be sealed as separate first-class evidence when storage, consent, and bandwidth budgets permit.
- **Playback:** Counsel playback must verify the composite's sealed-record hash, chain receipt, app attestations, source-stream links, clock offsets, overlay timeline, and any raw-stream relationship before rendering an expiring authorized working copy.
- **Capacity impact:** Dual-camera video plus compositing creates substantially higher storage, ingress bandwidth, egress, processing, and battery demand than sensor-only records. The public server therefore needs per-driver/session quotas, resumable chunked uploads, compression profiles, admission backpressure, retention tiers, and explicit limits for composite and optional raw-stream assets.

### 3.6 Open-source desktop court viewer

- **Cross-platform viewer:** Provide a GPL-2.0 desktop court viewer runnable on Windows, Linux, and macOS. It is governed and auditable under the same license as the mobile client and server components in scope.
- **Court-authorized decryption only:** The viewer decrypts a sealed ride bundle only through the escrowed private-key or wrapped-DEK court-authorized release path; it must not bypass escrow, quorum, legal-process, or release logging controls.
- **Independent synchronized review:** Display all collected data on one synchronized timeline, including the dual-video composite, telematics, accelerometer spider graph, GPS, and OBD2 where collected. The viewer must operate independently of the original collection device.
- **Fail-closed verification:** Before decrypting or displaying data, independently verify the blockchain custody receipt, payload hashes, and Google Play signing-certificate/Play Integrity attestation and key binding. Any failed, missing, stale, or inconsistent verification must prevent decryption/display and produce an auditable error.
- **Auditable rendering:** Keep decryption, verification, timeline synchronization, overlay, and rendering logic versioned, reproducible, and GPL2-published; create a ViewerSession and VerificationReport for every court review.

---

## 4. Compliance and privacy considerations

| Topic | Implication for the audit app |
| --- | --- |
| **CCPA/CPRA (CA) & similar state privacy laws** | Precise geolocation is **sensitive personal information**. Limit use to disclosed audit purposes; support access, deletion, correction, portability; do not sell/share for ads. Lyft’s own addendum (updated Feb 9, 2026) lists geolocation and sensitive PI categories. |
| **Other US state privacy laws** | CO, CT, VA, TX, OR, etc.—Lyft documents portability/opt-out style rights; mirror those for audit-held copies. |
| **Illinois BIPA (and similar biometric laws)** | Relevant if the app stores face geometry from selfie/ID checks or AI dashcam biometrics. Lyft may process biometric-style imagery for identity; **avoid collecting biometrics** unless strictly necessary and consented. |
| **Lyft Privacy Policy retention** | Profile while account open; transactional rides/payments **≥7 years**; legal/claims holds. Audit app should not assume Lyft will delete telematics on driver request if claims/legal holds apply. |
| **Driver consent & ToS** | Scraping the Driver app or reverse-engineering private endpoints risks ToS and CFAA issues. Prefer official export, Business API, or driver-provided screenshots with consent. |
| **FMCSA Hours of Service / ELD** | Typical Lyft passenger TNCs are **not** generally under federal HOS/ELD the way interstate CMVs are. Lyft’s **12h driver-mode / 6h break** (plus local rules) is the operational control to model—not FMCSA ELD schemas—unless the use case is a regulated commercial fleet (**Unverified** edge cases). |
| **NHTSA** | Safety research interest; no public Lyft telematics feed for NHTSA-style reporting found. |
| **Insurance / discovery** | Detailed post-crash telematics often obtained via legal process; product should support export for counsel, not unauthorized access to Lyft systems. |
| **Data minimization** | Collect only signals needed for the stated audit; drop high-rate GPS when aggregate scores suffice. |

### 4.1 Open-source governance and licensing

The application, custody-receipt schema, verification tooling, and crowdsourced evidence-network components must use **GPL-2.0 (GPL2)**. GPL2 copyleft means any derivative works or forks must also be open-sourced under GPL2; this supports keeping the crowdsourced evidence network transparent and auditable. Do not recommend or substitute Apache-2.0 or MIT for these components. Licensing the software and schemas does not authorize publishing plaintext personal or sensitive evidence: sealed ciphertext, access controls, consent, legal holds, and privacy obligations still apply.

---

## 5. Functional requirements

- **FR-1** Ingest Lyft privacy-export ZIP files and parse files according to the included DataDictionary; surface unknown file types as Unverified.
- **FR-2** Record Smooth Cruiser score and component values (gentle braking, smooth steering, phone mount use, vehicle speed relative to area) from structured export fields **or** consented manual/screenshot entry with timestamp and source tag.
- **FR-3** Ingest trip-level records (start/end time, origin/destination or route summary, distance, earnings metadata) when present in exports or Business reports.
- **FR-4** Optionally integrate Lyft Concierge/Business API: OAuth client credentials, program linkage, poll `/concierge/rides/{id}/status` for `driver_location` during active organizational rides only.
- **FR-5** Import third-party telematics files (CSV/JSON) for GPS track, speed, harsh brake/accel events from devices the driver/fleet controls.
- **FR-6** Maintain a provenance and consent ledger for every dataset (who consented, when, jurisdiction, purpose).
- **FR-7** Produce a **coverage matrix** per audit subject listing: collected-by-Lyft vs available-to-auditor vs missing, per signal type.
- **FR-8** Analyze online-hours against Lyft’s 12h / 6h break policy and configurable regional overrides; flag apparent violations only when hours data is present.
- **FR-9** Time-window incident report: export GPS/route samples, scores, and third-party events for a selectable interval.
- **FR-10** Support data subject access and deletion workflows for data the audit app stores.
- **FR-11** Do **not** implement undocumented Lyft private APIs, credential stuffing, or app traffic interception as product features.
- **FR-12** Admin UI to mark partnerships (Business API approved / denied) and disable Concierge features when ungated access is unavailable.
- **FR-13** Hash and version raw imports; show integrity status on audit reports.
- **FR-14** Role-based access: auditor, subject (driver), admin; least privilege for precise location views.
- **FR-15** Seal and encrypt every audit datum at the exact collection moment, before any durable store, queue handoff, normalization, or analysis; record the sealing algorithm/version and content hash.
- **FR-16** Generate encryption keys per session or per sample as configured for the evidence type; do not use one long-lived shared key for all records, and bind each key to its declared scope.
- **FR-17** Create a custody receipt for every sealed record containing, at minimum, the sealed-payload content hash, the public encryption key used (or key ID plus the public key material), collector identity, collection timestamp, provenance tag, and the blockchain transaction reference once confirmed.
- **FR-18** Write each receipt synchronously with, or immediately after, sealing to a configurable public immutable blockchain within the same collection transaction boundary; record chain ID, transaction hash, block height, and write time when confirmed.
- **FR-19** Apply an explicit failure policy: if the blockchain write or confirmation fails, do not mark collection complete, do not place the sealed record in an admitted state, and quarantine/retry the sealed record and pending receipt without rewriting the ciphertext; surface the failure for operator action and preserve the local failure audit trail.
- **FR-20** Document and implement the court-review decryption path: identify private-key custodians, legal-process requirements for counsel to obtain plaintext, escrow jurisdiction and controls, dual-control release and attestations, authorized working-copy scope/expiry, and exactly what the chain-of-custody receipt proves (and does not prove).
- **FR-21** Provide a verification UI/report that recomputes the sealed-payload hash, compares it with the custody receipt, checks the on-chain receipt and transaction reference, and displays any mismatch, missing confirmation, or chain status clearly.
- **FR-22** Escrow every private key or wrapped data-encryption key (DEK) needed for court decryption using dual control and a configurable M-of-N quorum, so sealed data remains decryptable if the original device or operator is unavailable; preserve the original sealed blob and receipt unchanged.
- **FR-23** Keep escrow material separate from the collection device, operator account, and application database. Document per key whether the escrow package contains quorum-protected private-key shares or a wrapped DEK, while the device retains only the public key/key ID and transient or encrypted recovery material needed for collection; no plaintext private key may persist on-device after sealing.
- **FR-24** Implement a court-authorized escrow-release procedure that verifies the case and legal process, obtains the M-of-N dual-control approvals, releases only the minimum key or wrapped DEK needed, creates an append-only release log with approvers, shares/use, timestamps, and purpose, and never changes the sealed ciphertext, content hash, or on-chain receipt.
- **FR-25** Use Google Play Store app signing and/or Play Integrity API attestation as part of the encryption-key authentication chain. At collection, bind the generated key to the approved package identity and signing-certificate digest or a nonce-bound Integrity attestation, and retain evidence sufficient for later court verification.
- **FR-26** Verify the Play Integrity/signing-certificate result before key generation and sealing; reject collection when the device is compromised, the binary is modified or unofficial/sideloaded, the package or certificate is not approved, the attestation is stale or nonce-mismatched, or the verdict cannot be validated. Do not admit a record from a failed-attestation attempt.
- **FR-27** Include the app-attestation evidence or a cryptographic hash of it, plus the package identity, signing-certificate digest or attestation reference, in the custody receipt written on-chain and link it to the `KeyMaterial` and `SealedRecord`.
- **FR-28** Provide counsel-verification steps that validate the public-chain receipt and payload hash, verify the Play Integrity evidence/signing certificate against the approved Google Play app identity and collection nonce/key binding, confirm the attestation timestamp and verdict, verify escrow quorum and release logs, and then document authorized decryption of an expiring working copy.
- **FR-29** License the application, custody/verification schemas and tooling, and crowdsourced evidence-network code under **GPL-2.0 (GPL2)**; publish the corresponding source and license notices so derivative works and forks are also open-sourced under GPL2.
- **FR-30** Attach a GPL2 license/version and source-commit notice to shared software, schema, receipt, attestation, and verification artifacts, while ensuring that crowdsourced evidence payloads remain sealed and subject to consent, privacy controls, access policy, and legal holds rather than being exposed merely because the code is open source.
- **FR-31** Publish a downloadable GPL-2.0 client through Google Play Store distribution and a public source repository, with reproducible/versioned build and signing metadata.
- **FR-32** Provide a public-server driver account identity with authentication, consent records, jurisdiction/purpose, account recovery, and tenant-scoped authorization.
- **FR-33** Register each vehicle to an authorized driver account using VIN, plate, make/model, or the minimum appropriate attributes, with driver consent and an auditable change history.
- **FR-34** Require a valid configuration profile for the registered vehicle and verify that configuration before admitting a collection session or submission.
- **FR-35** Make the public submission API accept only already-sealed ciphertext packages plus custody-receipt metadata; reject plaintext, unsealed sensor/video data, missing receipts, or packages that cannot be tied to an authorized driver and vehicle.
- **FR-36** Integrate public-server admission with the existing seal, blockchain custody receipt, escrow, and Play Integrity/app-signing chain; verify receipt fields, payload hash, chain confirmation, app attestation, nonce/key binding, and approved package identity before adding a submission to the network index.
- **FR-37** Preserve court multi-driver provenance: each submission must be independently verifiable through device attestation, `collector_id`, `driver_id`, `vehicle_id`, and its on-chain receipt; aggregation must not weaken per-record custody.
- **FR-38** Generate a counsel bundle containing submissions from many drivers while providing a per-record verification report for each sealed record, receipt, attestation, vehicle registration, and escrow/decryption path.
- **FR-39** Enforce public-server abuse controls: authenticated rate limits and quotas, idempotency, duplicate/replay rejection, tampered-attestation rejection, unregistered-vehicle rejection, failed-Integrity rejection, size limits, quarantine, and abuse/audit logs.
- **FR-40** Enforce strict multi-tenant isolation for account, vehicle, configuration, submission, admission, and counsel data; expose only authorized per-driver records and explicitly approved aggregate indexes.
- **FR-41** Support two approved phones collecting video for the same authorized vehicle/session, retaining each source stream's device identity, app attestation, camera metadata, and capture timestamps.
- **FR-42** Establish a shared session clock between phones, record `SyncClockOffset` and drift/uncertainty, and align video frames, telematics, and accelerometer samples to that clock, including dropped-frame or unsynchronized intervals.
- **FR-43** Run an on-device or trusted-edge compositing pipeline that produces one time-synchronized combined video stream from both source streams before public submission.
- **FR-44** Overlay telematics and accelerometer data in real time as a spider graph below the corresponding video stream, with frame-aligned timestamps and a versioned overlay/timeline manifest.
- **FR-45** Seal and encrypt the composited artifact at the collection-device boundary as a first-class `SealedRecord`, generate its custody receipt, and write the receipt to the configured public immutable blockchain under the same admission policy as raw sensor data.
- **FR-46** Optionally seal each raw source video stream as its own first-class sealed artifact, linked to the composite, when the driver consents and storage/bandwidth/retention budgets allow; never replace a sealed source or composite by re-encoding plaintext on the public server.
- **FR-47** Provide counsel playback of an authorized expiring working copy of the sealed composite only after verifying its payload hash, blockchain receipt, source-stream links, clock offsets, overlay timeline, and app-attestation evidence; clearly report any missing or inconsistent component.
- **FR-48** Include composite, source-stream, synchronization, overlay, codec, compression, dropped-frame, and device metadata in the custody and verification package without exposing plaintext to the public submission server at ingest.
- **FR-49** Provide a GPL-2.0 desktop court viewer for Windows, Linux, and macOS that decrypts sealed ride bundles only through the court-authorized escrow release path and never bypasses escrow controls.
- **FR-50** Have the desktop viewer independently verify the blockchain custody receipt, payload hashes, Google Play signing certificate/Play Integrity attestation, nonce/key binding, and escrow release authorization before decrypting or displaying any data; fail closed on any verification failure.
- **FR-51** Display all collected data on a synchronized timeline, including composite video, telematics, accelerometer spider graph, GPS, and OBD2 where available, independently of the original collection device.
- **FR-52** Create an auditable `ViewerSession` and `VerificationReport` for every review, and publish versioned decryption, verification, synchronization, overlay, and rendering logic under GPL-2.0 so judge or opposing counsel can independently inspect the process.

---

## 6. Non-functional requirements

- **NFR-1** Security: encryption in transit (TLS 1.2+) and at rest; secrets in a vault; no plaintext API tokens in logs.
- **NFR-3** Auditability: immutable append-only access logs for views/exports of sensitive location and identity data.
- **NFR-4** Reliability: Concierge ingestion must tolerate API rate limits and partial outages without corrupting stored rides.
- **NFR-5** Scalability: support multi-year trip histories and multi-Hz GPS tracks from third-party devices without UI freezes (paginate / downsample for display).
- **NFR-6** Accuracy: never present inferred speed/brake events from sparse Concierge lat/lng polls as Lyft Smooth Cruiser equivalents—label derived metrics clearly.
- **NFR-7** Portability: export audit packages as ZIP (CSV + PDF summary + provenance JSON).
- **NFR-8** Compliance config: per-state retention and deletion timers; California sensitive-PI handling as default strict profile.
- **NFR-9** Documentation: in-product “API gap” notice stating Lyft does not offer a public driver telematics API (as of research date).
- **NFR-10** Legal hold: ability to suspend deletion for designated cases under counsel instruction.
- **NFR-11** Cryptographic agility: use versioned, configurable algorithms, key formats, and receipt schemas so cryptographic suites can be rotated or replaced without rewriting sealed records; preserve the algorithm identifier with each record.
- **NFR-12** Chain choice configurability: support a configurable public immutable chain (or approved set of chains) by environment and jurisdiction; do not make any specific blockchain brand or mainnet mandatory, and preserve chain ID/transaction metadata for verification.
- **NFR-13** Latency budget: define, measure, and monitor a configurable per-connector budget for sealing, receipt creation, and blockchain write/confirmation; collection completion and admitted state remain blocked until the configured receipt confirmation policy is satisfied.
- **NFR-14** Key custody: private keys must be generated, stored, and used through an HSM/KMS or equivalent controlled service, never in the application database; support separation of duties, dual control, rotation, escrow, access logging, and documented legal-release procedures.
- **NFR-15** App authenticity: enforce a versioned allowlist of Google Play package identities and signing-certificate digests, or an equivalent Play Integrity verification policy; reject failed, tampered, unofficial, sideloaded, stale, or unverifiable attestations before collection is admitted, and support auditable certificate/key rotation.
- **NFR-16** Escrow resilience: maintain encrypted M-of-N escrow shares or wrapped-DEK recovery packages independently of collection devices and operators, with tested recovery, quorum enforcement, separate custodians, and append-only release logging that cannot alter sealed evidence.
- **NFR-17** Open-source governance: publish the application, custody/verification schemas, and crowdsourced evidence-network implementation under GPL-2.0 (GPL2), including source, notices, and build/version provenance; do not introduce Apache-2.0 or MIT as an alternative license for these components.
- **NFR-18** Public-server admission: enforce tenant isolation, authenticated rate limits, quotas, resumable/idempotent uploads, duplicate/replay detection, abuse throttling, and backpressure; admission capacity must not bypass receipt, attestation, or seal verification.
- **NFR-19** Video storage and bandwidth: define per-driver/session quotas and maximum composite/raw-stream sizes; use documented lossless or approved lossy compression, resumable chunked transfer, integrity-checked chunks, retention tiers, lifecycle deletion/legal holds, and separate capacity budgets for composite output and optional raw streams.
- **NFR-20** Video performance: measure device/edge CPU, memory, battery, thermal load, encoding latency, clock-sync error, frame drops, public-server ingress/egress, and counsel playback bandwidth; do not admit a composite whose synchronization or sealing quality falls below configured thresholds.
- **NFR-21** Composite integrity and playback: preserve codec/compression and overlay-manifest versions, source-to-output hashes, sync offsets, frame/timestamp mapping, and verification status so counsel can reproduce the integrity check before playback.
- **NFR-22** Desktop portability and fail-closed behavior: test signed/reproducible GPL2 builds on Windows, Linux, and macOS; a viewer must not decrypt or render when receipt, attestation, escrow-release, hash, or bundle verification fails.

---

## 7. Data model

The court-admissibility model preserves the original sealed evidence while making its custody and any court-authorized plaintext release explicit:

- **`CollectionEvent`** — one collection action, including collector identity, provenance tag, collection timestamp, consent/purpose context, and the session/sample scope used for key generation.
- **`SealedRecord`** — immutable evidence record containing a ciphertext blob reference, sealing/encryption algorithm identifier, `content_hash`, and `collected_at`; links to its `CollectionEvent` and custody receipt.
- **`KeyMaterial`** — `key_id`, `public_key`, scope (`session` or `sample`), and `created_at`; references the attestation used to authorize key generation. The private key is **never** stored in the application database and remains under HSM/KMS or separately controlled custody.
- **`KeyEscrowShare`** — `escrow_id`, `key_id` or wrapped-DEK reference, `share_id`, M-of-N policy, custodian identity, encrypted share/package reference, `created_at`, and release status; shares are held separately from the collection device and operator, and are unusable without the required quorum.
- **`AppAttestation`** — `attestation_id`, package identity, Google Play signing-certificate digest and/or Play Integrity evidence reference, nonce/challenge, verdict, device/app integrity results, attested-at time, and evidence hash; binds the approved genuine app binary to key generation and the sealed record.
- **`CustodyReceipt`** — `sealed_record_id`, `public_key`, `content_hash`, `collected_at`, `collector_id`, `provenance_tag`, `app_attestation_id`, attestation evidence/hash, `chain_id`, `tx_hash`, `block_height`, and `written_at`; a pending local receipt must not make a record admitted until the required public-chain write is confirmed.
- **`CourtRelease`** — who authorized the release, `case_id`, dual-control attestations, `decrypted_working_copy_ref`, and `expires_at`, plus the legal-process and access audit details needed to explain the release.
- **`EscrowRelease`** — release ID, case/legal-process reference, authorized requester, approving custodians and quorum, released key/wrapped-DEK scope, release timestamp, recipient/working-copy reference, expiry, and an append-only audit hash; logging the release must not alter the sealed record or its receipt.
- **`LicenseMetadata`** — `license_id` (fixed to `GPL-2.0`/`GPL2` for application, schema, receipt, attestation, verification, and evidence-network software), source commit/version, notice reference, and derivative/fork disclosure; attach to shared software and schema artifacts, not as a substitute for access controls on sealed evidence.
- **`DriverAccount`** — `driver_id`, authenticated identity/account status, consent records, jurisdiction, purposes, recovery metadata, and tenant-scoped authorization; account identity is not proof that any submission is truthful.
- **`VehicleRegistration`** — `vehicle_id`, `driver_id`, consented VIN/plate/make/model or minimized vehicle attributes, registration status, configuration-profile version, and change history.
- **`Submission`** — public-network admission object linking `SealedRecord`, `CustodyReceipt`, `AppAttestation`, `driver_id`, `vehicle_id`, configuration version, admission status, and server receipt/admission timestamp; it contains no plaintext evidence.
- **`ServerAdmissionLog`** — append-only admission/rejection event, verifier version, receipt/attestation checks, duplicate/replay result, rate-limit/quota decision, actor/service identity, and timestamp.
- **`VideoStream`** — one phone's sealed source video reference, device/phone identity, camera metadata, codec/compression, frame/timestamp metadata, session ID, and optional raw-stream `SealedRecord`/receipt link.
- **`SyncClockOffset`** — session ID, source phone IDs, offset/drift estimates, synchronization method, uncertainty, calibration timestamps, and validity interval.
- **`SpiderGraphFrame`** — frame/timestamp reference, telematics and accelerometer values, graph schema/version, overlay position, and source/sample linkage; may instead be represented by a signed/hashed overlay timeline embedded in the composite manifest.
- **`CompositeEvidenceAsset`** — composite ID, two source `VideoStream` links, `SyncClockOffset`, overlay timeline/`SpiderGraphFrame` manifest, codec/compression and output hashes, and the output `SealedRecord` plus `CustodyReceipt`; links optional raw-stream records without replacing them.
- **`RideBundle`** — counsel-review package containing one or many admitted submissions and their sealed records, composite/raw video links, telematics, GPS, accelerometer, OBD2 references, receipts, attestations, escrow-release authorization, and per-record verification requirements.
- **`ViewerSession`** — viewer build/version, reviewer role, case/bundle ID, authorized escrow release reference, start/end times, displayed timeline assets, and append-only access/rendering events.
- **`VerificationReport`** — independent results for payload hashes, blockchain receipts, Play signing/Integrity attestations, key binding, vehicle/driver links, escrow authorization, synchronization/overlay metadata, and fail-closed errors; one report is retained per record and viewer session.

**Open-source data-model note:** Publishing the model, schemas, verification rules, and source under GPL2 keeps the crowdsourced evidence network transparent and auditable. It does not publish plaintext evidence or waive consent, privacy, legal-hold, key-custody, or court-release controls.

---

## 8. Build plan

- **Phase 0 — Crypto/custody and license design:** Define key scopes, receipt schema, chain configuration, HSM/KMS custody, M-of-N escrow and separated custodians, what is escrowed versus retained transiently/on-device, Google Play signing/Play Integrity trust policy, GPL2 governance and contribution rules, legal-release controls, failure states, and latency SLOs.
- **Phase 1 — Seal at collect + local receipt:** Implement the collection boundary, per-session/per-sample keys, nonce-bound Play Integrity/signing-certificate verification before key generation, immutable ciphertext, local receipt ledger with attestation evidence/hash, escrow packaging, GPL2 license metadata, and admitted-state gating.
- **Phase 2 — Blockchain writer + verify:** Add the configurable public-chain writer, confirmation/retry policy, on-chain receipt including attestation evidence/hash, counsel-facing chain/hash/attestation verification, and operator/reporting UI; publish the relevant source and schemas under GPL2.
- **Phase 3 — Court decryption and escrow workflow:** Implement counsel intake, legal-process checks, M-of-N dual-control escrow release when the device/operator is unavailable, append-only release logging, expiring decrypted working copies, and documented counsel verification of app authenticity and custody.
- **Phase 4 — Ingest connectors behind the seal boundary:** Bring in privacy exports, Business API data, screenshots, and third-party telematics only through the established seal, attestation, escrow, receipt, and GPL2-governed verification boundary.
- **Phase 5 — Public multi-driver server:** Build the GPL2-governed public client distribution and source publication, driver identity/consent service, vehicle registry and configuration profiles, sealed-only submission/admission service, tenant isolation, rate limits/abuse controls, network index, `ServerAdmissionLog`, and counsel multi-driver verification bundle. Keep all in-scope server components, schemas, verification tooling, and network services under GPL-2.0; never decrypt or plaintext-composite at public-server ingest.
- **Phase 6 — Dual-phone video evidence:** Implement two-phone capture, clock calibration/offset tracking, on-device or trusted-edge compositing, frame-aligned spider-graph overlays, compression and quota controls, first-class sealing and chain receipts for the composite, optional raw-stream sealing, sealed-package upload, and counsel playback/verification. Prefer sealing the composite at the collection-device boundary and submit only sealed composite packages to the public server.
- **Phase 7 — Desktop court viewer:** Build and publish the GPL-2.0 Windows/Linux/macOS viewer, court-authorized escrow-release integration, independent receipt and Play attestation verification, fail-closed decryption, synchronized RideBundle timeline for composite/video, telematics, spider graph, GPS, and OBD2, plus auditable GPL2 decryption/rendering logic and `VerificationReport` output.

---

## 9. Open questions / unknowns

1. Exact file inventory of the Lyft privacy-export ZIP for **drivers** (does it include Smooth Cruiser event logs, high-rate GPS, or only account/trip summaries?) — **Unverified**; needs a consented sample export.
2. Whether Lyft Business offers any non-public partner feed for fleet telematics beyond Concierge status location — **Unverified**; would require partnership inquiry via Lyft API Developer Program request.
3. Regional variations of driver-mode time limits beyond the national 12h/6h rule — partially documented; full matrix not exhaustively captured here.
4. Whether optional Lyft dashboard tablets expose additional sensor streams to the driver or only to Lyft — **Unverified**.
5. Retention period specifically for high-rate IMU/GPS telematics vs transactional ride records — Privacy Policy states ≥7 years for rides/payments; sensor-stream retention not separately published.
6. Current status of any Lyft↔third-party telematics (Geotab/Samsara/Motive) commercial integration for rideshare cars — none found in public docs (**Unverified** private deals).
7. Colorado exception: Smooth Cruiser / acceptance rate not in Rewards goals—confirm other jurisdictions with scoring differences.
8. Whether authorized-agent CCPA requests can obtain telematics detail beyond the self-serve ZIP — process exists; content completeness **Unverified**.
9. Which public chain or chains should be configured for production custody receipts, by jurisdiction and evidence class?
10. Who sponsors gas or other transaction fees, and what fee-management policy applies during congestion?
11. What is the policy for offline collection or collection when public-chain connectivity is unavailable: queue locally, reject collection, or use another explicitly non-admitted state?
12. In which jurisdiction(s) may private-key escrow be held, and what legal, access, and cross-border controls apply?
13. Do public-chain fields such as public keys, hashes, timestamps, or provenance tags create additional disclosure or re-identification risk?
14. Which Google Play Integrity verdicts, package identities, and signing-certificate digests should be accepted, and what evidence must be retained for court verification?
15. How should Google Play signing-certificate rotation, key upgrade, app version changes, and historical attestation verification be handled?
16. Can Play Integrity attestation be obtained at every collection boundary, including offline or degraded-connectivity conditions, and what is the reject policy when it cannot?
17. What M-of-N quorum, custodian roles, escrow jurisdictions/providers, disaster-recovery tests, and separation-of-duty controls are required for production key escrow?
18. Should production escrow retain private-key shares, wrapped DEKs, or both for each evidence class, and what recovery material may remain transiently on the collection device?
19. What privacy or disclosure risks arise from publishing app package identifiers, signing-certificate digests, attestation hashes, or device-integrity evidence on a public chain?
20. Which GPL2 contribution, source-publication, dependency-compliance, and fork-notice processes are required for the crowdsourced evidence network?
21. Which software, schemas, verification rules, and network services are in scope for GPL2, and how are sealed evidence payloads kept private while the code remains auditable?
22. Who operates the public server, under what governance, jurisdiction, funding model, and incident-response obligations?
23. What identity-proofing strength is required for driver accounts (phone, email, government ID, or another level), and what data-minimization tradeoff applies?
24. What anti-spam and anti-Sybil controls are acceptable without excluding legitimate drivers, and who adjudicates abuse or false submissions?
25. Does parallel telematics/video collection conflict with Lyft Terms of Service, local recording/privacy law, or other contractual restrictions?
26. What liability and correction process applies to false, misleading, or malicious submissions and public indexes?
27. Who pays chain transaction fees/gas for many drivers' receipts, and what batching, sponsorship, or fee-management policy applies during congestion?
28. What minimum video quality, clock-sync accuracy, overlay timing tolerance, codec/compression, quota, retention tier, and raw-stream opt-in policy are required for court use?
29. What desktop viewer signing, package distribution, OS permission, hardware-acceleration, and long-term reproducibility policy is required for independent court review?

---

## 10. Source list (URLs)

1. https://www.lyft.com/privacy — Lyft Privacy Policy (Last Updated: July 1, 2026)  
2. https://www.lyft.com/privacy/addendum — Local Information and Rights / CCPA (Last updated: February 9, 2026)  
3. https://cdn.lyft.com/static/privacy.html — Privacy Policy CDN mirror  
4. https://help.lyft.com/hc/en-us/all/articles/360035885974-Lyft-Rewards — Smooth Cruiser & Rewards metrics  
5. https://www.lyft.com/blog/posts/lyfts-impact-on-road-safety — Smooth Cruiser Program, speeding alerts, Vision 360 (Feb 7, 2023)  
6. https://help.lyft.com/hc/en-us/all/articles/115012926787-Taking-breaks-and-time-limits-in-driver-mode — 12h / 6h break policy  
7. https://help.lyft.com/business/hc/en-us/articles/360001599667-Concierge-API-overview — Concierge API overview  
8. https://help.lyft.com/business/hc/en-us/articles/8587470351891-Managing-your-API-client-and-program-connections — API client ↔ Business Portal programs  
9. https://www.lyft.com/developers — Developer portal (sign-in gated)  
10. https://go.lyftbusiness.com/api-dev-request — Lyft API Developer Program request  
11. https://raw.githubusercontent.com/api-evangelist/lyft/refs/heads/main/openapi/lyft-concierge-rides-api-openapi.yml — Community OpenAPI for Concierge (status includes driver_location lat/lng)  
12. https://apis.io/providers/lyft/ — Summary of gated ride-hailing APIs vs public GBFS  
13. https://account.lyft.com/privacy/data/download — Driver/rider data download (referenced by Princeton rideshare study guide)  
14. https://rideshare-study.cs.princeton.edu/resources/request_data_guide_all.pdf — Steps to request Lyft data export  
15. https://hirocareers.com/jobs/66745e35-6925-4231-853a-17aa9b3eebc1 — Telematics team description (IMU/gyro/GPS → harsh events / risk features)  
16. https://www.lyft.com/blog/posts/lyft-mobility-expertise-powers-av-partners — Telemetry use for AV partners / Flexdrive (enterprise context)  
17. https://github.com/lyft/lyft-node-sdk — Deprecated legacy public API SDK  
18. https://help.lyft.com/business/hc/en-us/articles/360001818127-Accessing-transaction-reports-via-SFTP — Business SFTP transaction reports  
19. https://fmcsa.dot.gov/regulations/hours-of-service — FMCSA HOS (baseline; TNC applicability generally limited)  
20. https://curatedprivacy.com/privacy/geolocation-data-and-the-cpra-what-u-s-businesses-must-know-to-stay-compliant/ — CPRA precise geolocation as sensitive PI (secondary explainer)

---

*End of requirements document. No undocumented Lyft API endpoints were asserted. Gaps are intentional findings for product scoping.*
