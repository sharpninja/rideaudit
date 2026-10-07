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

Access export works.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-010 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-011

Raw imports hashed/versioned; integrity status shown.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-011 passes for happy path of covered requirements.
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
- [ ] Public admission does not decrypt an RAES or RIDESEAL1 envelope. Decrypt evidence is an expiring counsel working copy after HSM escrow release.

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

Two-phone session syncs clocks, composites on-device with spider-graph overlay. The ride video codec is H.264.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-025 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-026

Composite sealed with chain receipt; optional raw sealed with consent; metadata in custody package; no server plaintext re-encode. The ride video codec is H.264.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-026 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-027

Playback only after full verification; inconsistencies reported; integrity reproducible. The ride video codec is H.264.

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

### TEST-RIDE-031

Large histories paginate; export ZIP has CSV+PDF+provenance JSON.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-031 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-033

Quotas/chunked uploads enforced; below-threshold composites not admitted. The ride video codec is H.264.

**Acceptance Criteria:**
- [ ] Test TEST-RIDE-033 passes for happy path of covered requirements.
- [ ] Failure cases assert non-admission or clear error without inventing Lyft APIs.

### TEST-RIDE-034

Verify Bluetooth pairing assigns driver coordinator vs passenger compositor roles and fail-closed paths. The ride video codec is H.264.

**Acceptance Criteria:**
- [ ] Pairing evidence uses the RideAudit Bluetooth session and does not call a Lyft Bluetooth API.
- [ ] Driver phone coordinates the session. Passenger phone performs video sync and telematics overlay.

### TEST-RIDE-035

Verify Android capture and desktop court viewer build on Avalonia UI 12 and shared client UI is GPL-2.0.


### TEST-RIDE-036

Verify gRPC on .NET 10 containers accepts sealed-only submit, never decrypts at ingest, and fails closed on admission errors.


### TEST-RIDE-037

Verify protos and schemas publish under GPL-2.0 and that OpenAPI is marked non-authoritative when it disagrees with gRPC.


### TEST-RIDE-038

A documented Octopus dry-run or release receipt proves RideAudit images were built and deployed to LAB-OMARCHY through Octopus, names the instance or container and target machine, and does not claim GHCR.

**Acceptance Criteria:**
- [ ] Receipt or dry-run shows Octopus built and deployed admission/counsel (or related) images to LAB-OMARCHY.
- [ ] Receipt names the Octopus instance or container and target machine and contains no GHCR green claim.
- [ ] A LAB-OMARCHY compose cutover receipt is not scored as the Octopus CD green.

### TEST-RIDE-039

An external probe of the configured ngrok public URL returns the expected admission health response.

**Acceptance Criteria:**
- [ ] The live ngrok URL returns the expected admission health or documented companion front-door response.
- [ ] When ngrok is down or misconfigured, no public URL is advertised as live.

### TEST-RIDE-040

Repository scan confirms ngrok tokens and Octopus API keys are not committed.

**Acceptance Criteria:**
- [ ] No ngrok auth token or Octopus API key is present in committed files.
- [ ] Checked-in deploy docs and scripts use environment or secret-store placeholders for those secrets.

### TEST-RIDE-041

Verify a lab win-x64 signature with subject CN=RideAudit Lab Self-Signed is recorded as signed and not Public Trust, that unrelated store certificates are refused, that no pfx is committed, and that commercial signing for individual Payton Byrd using IV plus eSigner is not purchased yet. An organization OV certificate is not the publisher identity. macOS codesign stays deferred.

**Acceptance Criteria:**
- [ ] Evidence shows CN=RideAudit Lab Self-Signed on win-x64, signtool sees a signature, and verify /pa does not establish Public Trust.
- [ ] Evidence shows commercial signing for individual Payton Byrd using IV plus eSigner is not purchased yet, an organization OV certificate is not the publisher identity, macOS codesign remains deferred, and AC-RIDE-222-001 stays unsatisfied.

### TEST-RIDE-042

Verify a live OpenTimestamps public calendar submit can be receipted as pending, and that confirmation, Bitcoin txid, block height, admission, and live_bitcoin_metadata stay false until upgrade succeeds.

**Acceptance Criteria:**
- [ ] A live public calendar submit receipt shows HTTP success and a pending attestation, and it is not a documented fixture calendar.
- [ ] The same evidence has no Bitcoin txid or block height, does not set live_bitcoin_metadata, and does not admit a record.

### TEST-RIDE-043

Verify AC-UC-025 class runtime proof names a Samsung Galaxy Z Fold 4 attached by USB. Emulator-only runs fail this test. The test does not mark AC-UC-025-001 satisfied.

**Acceptance Criteria:**
- [ ] Runtime evidence identifies a Galaxy Z Fold 4 on USB as the primary device.
- [ ] Emulator-only evidence is a failure for this runtime class, and AC-UC-025-001 remains unsatisfied.

### TEST-RIDE-044

Verify the lab secondary phone for dual-phone roles is a Motorola edge 2024 over wireless adb, and that it is not substituted for the Fold 4 primary.

**Acceptance Criteria:**
- [ ] Evidence shows a Motorola edge 2024 connected by wireless adb in the secondary role.
- [ ] The secondary attachment is not scored as Fold 4 primary proof or as AC-UC-025-001 closure.

### TEST-RIDE-045

Verify edge TLS evidence names Caddy and does not use ngrok HTTPS or loopback HTTP as the pass condition.

**Acceptance Criteria:**
- [ ] The receipt names Caddy as the TLS terminator.
- [ ] ngrok HTTPS and loopback or LAN HTTP are recorded as non-proof for this test.

### TEST-RIDE-046

Verify application styles set a capture UI default font larger than FontSize 20 and that OS accessibility scaling is not the only control.

**Acceptance Criteria:**
- [ ] A style or test asserts the default UI font is larger than FontSize 20.
- [ ] The assertion targets application styles, not only OS accessibility settings.

### TEST-RIDE-047

Verify SharpNinja.Avalonia.RemoteControl is integrated and that visual-tree debug evidence does not depend on ADB tapping.

**Acceptance Criteria:**
- [ ] The capture project references SharpNinja.Avalonia.RemoteControl.
- [ ] Debug evidence for the visual tree does not require ADB taps.

### TEST-RIDE-048

Verify tray dimensions or a fit receipt hold a Galaxy Z Fold 4 CLOSED, LANDSCAPE, with primary cameras facing FORWARD.

**Acceptance Criteria:**
- [ ] Fit evidence states CLOSED, LANDSCAPE, and primary cameras FORWARD for a Galaxy Z Fold 4.
- [ ] The phone remains secure in that pose. A generic cradle note without those three conditions fails.

### TEST-RIDE-049

Verify Cursor Desktop is installed on PAYTON-LEGION2, Cursor cloud coding agents for this repo are targeted at the PAYTON-LEGION2 private worker as ninja@thesharp.ninja, and mcpserver-grok-plugin is available there.

**Acceptance Criteria:**
- [ ] Agent target evidence names the PAYTON-LEGION2 private worker.
- [ ] Plugin availability on that worker is shown. A host that cannot load the plugin fails.
- [ ] Evidence shows Cursor Desktop on PAYTON-LEGION2 and agent identity ninja@thesharp.ninja.

### TEST-RIDE-050

Verify a hostile-validation receipt names an opposing model, includes committed request and response JSONL, and withholds AGREE below accuracy or completeness 98.

**Acceptance Criteria:**
- [ ] Evidence names an opposing model and points at committed request and response JSONL.
- [ ] An AGREE below accuracy or completeness 98 fails.

### TEST-RIDE-051

Verify the Class A ledger names or defers every AC, that an explicit deferral wins for live third-party ACs, and that 401/23/0/424 is not scored as P11b done.

**Acceptance Criteria:**
- [ ] A fixture deferred live third-party row remains deferred even when a test-source name is present.
- [ ] The test fails if a ledger total is treated as whole-AC closure or P11b done.

### TEST-RIDE-052

Verify the committed in-repo lab toolchain under artifacts/hardware includes a receipt reference, does not silently substitute paths, does not use Python lab tooling, and does not contain em or en dashes. Confirm go-by-default is limited to PAYTON-DESKTOP and PAYTON-LEGION2. Scope is in-repo lab artifacts (for example headrest-phone-mount), not languages on a physical LAB-OMARCHY host.

**Acceptance Criteria:**
- [ ] A scan of the committed lab tree under artifacts/hardware finds a receipt reference and no em or en dash characters.
- [ ] The committed lab toolchain under artifacts/hardware does not use Python lab tooling and does not describe a silent path substitution. Go-by-default is named only for PAYTON-DESKTOP and PAYTON-LEGION2.

### TEST-RIDE-053

Verify the Android Avalonia client references SharpNinja.aiUnit and that a PAYTON-LEGION2 run against a connected Android device compares one screen to every wireframe unless that wireframe specifies otherwise. For every storyboard, SharpNinja.Avalonia.RemoteControl (AvaloniaRemote) walks the step sequence and compares a screenshot at each frame. A static single-shot screenshot does not pass a storyboard. The threshold is documented. A mismatch fails closed. The run is receipted. A baseline is not silently skipped.

**Acceptance Criteria:**
- [ ] The Android client project file references SharpNinja.aiUnit. A missing reference fails.
- [ ] Evidence lists one single-screen device comparison for each wireframe in the repo, unless that wireframe specifies otherwise. A missing wireframe without a documented reason fails.
- [ ] Evidence shows SharpNinja.Avalonia.RemoteControl (AvaloniaRemote) walked each storyboard step sequence on the running app and compared a screenshot at each frame. A storyboard covered only by a static single-shot screenshot fails.
- [ ] Evidence shows the run host is PAYTON-LEGION2 and the screenshot source is a connected Android device. Emulator-only evidence fails.
- [ ] A run receipt names host, device, documented threshold, and per-baseline pass or fail. A mismatch is a failed test.
- [ ] A wireframe, storyboard, or storyboard frame omitted from the receipt with no documented reason fails closed.

### TEST-RIDE-054

Verify screenshot validation checks usability in addition to baseline comparison. A pixel match does not pass when the screenshot or the AvaloniaRemote visual tree shows cut-off, truncated, or clipped text, a missing icon, overlapping controls, text overflow, or another layout defect.

**Acceptance Criteria:**
- [ ] Evidence shows a usability check, from the screenshot or the AvaloniaRemote visual tree, in addition to the baseline pixel comparison.
- [ ] Cut-off, truncated, or clipped text, missing icons, overlapping controls, text overflow, and other detectable layout defects fail the run.
- [ ] A case that matches the baseline pixels and still has a usability defect is a failure. Pixel match alone does not satisfy the AC.
- [ ] Evidence shows SharpNinja.aiUnit is configured with the codex-subscription profile for the Android visual and usability run. A different profile fails.

### TEST-RIDE-055

Verify the bottom panel About control opens the About view, the About view shows the UI copyright and third-party attributions (licenses and credits), and the top title bar does not show that copyright. Copyright alone fails.

**Acceptance Criteria:**
- [x] Activating the bottom-panel About control opens the About view.
- [x] The About view shows the RideAudit UI copyright.
- [x] The top title bar does not show the copyright notice. A copyright string left on that chrome fails.
- [x] The About view includes third-party attributions (licenses and credits). An About view that shows copyright only fails.

### TEST-RIDE-056

Verify approved capture wireframe assets and the Avalonia capture app both use authorized slate colors #394656 / #3D4A5A and meet WCAG 2.x AA contrast (normal text >=4.5:1; large text/UI components >=3:1 as applicable). Fail when wireframe and app colors diverge or when either side fails contrast.

**Acceptance Criteria:**
- [ ] Wireframe capture chrome slate colors match authorized #394656 / #3D4A5A.
- [ ] App capture chrome/SVG slate colors match the same authorized values as the wireframes.
- [ ] Normal text contrast on those slate backgrounds is at least 4.5:1 for wireframes and app.
- [ ] Large text and UI component contrast on those slate backgrounds is at least 3:1 as applicable for wireframes and app.
- [ ] A wireframe-vs-app slate mismatch fails the test.

### TEST-RIDE-057

Verify visual verification treats controls/layout/style fidelity to approved wireframes as the primary verdict and treats pixel-by-pixel comparison as advisory only. Confirm a fidelity defect fails closed even when an advisory pixel metric would pass, and that receipts name the primary fidelity verdict separately.

**Acceptance Criteria:**
- [ ] The visual verification procedure or harness documents fidelity to approved wireframes as the primary verdict.
- [ ] Pixel-by-pixel comparison is advisory only and cannot alone produce a pass when fidelity fails.
- [ ] Injecting or observing a controls/layout/style divergence from the approved wireframe fails closed despite advisory pixel pass.
- [ ] The receipt records the primary fidelity verdict separately from any advisory pixel metric.
