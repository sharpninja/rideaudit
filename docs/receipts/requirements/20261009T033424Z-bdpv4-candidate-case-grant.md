# BDPv4 candidate for Payton AGREE: driver-issued case grant (not in MCP)

- **Written:** 2026-10-08 22:34 CT (America/Chicago), 20261009T033424Z.
- **Revised:** r2, 2026-10-08 22:50 CT (20261009T035027Z). Answers Codex's review of r1 on PR #29 (HV pair `docs/reviews/hv-pairs/20261009T034331Z-pr29-codex-hv.json`, DISAGREE 96/92, ineligible model).
- **Revised:** r3, 2026-10-08 22:59 CT (20261009T035928Z). Answers Codex review 5465551807 (UC-RIDE-021 left out; no grantor-only issuance or revocation tests). r3 lists every use case in `docs/Project/Use-Cases-Batch.yaml` that still names Admin, Auditor or Counsel, not only the five r1 named.
- **Revised:** r4, 2026-10-08 23:08 CT (20261009T040851Z). Answers Codex review 5465599386: desktop viewer flows now need an online grant check (TR-RIDE-SEC-004 AC 5, TEST-RIDE-058 item 6), and refused grant management is logged and tested.
- **Revised:** r5, 2026-10-08 23:15 CT (20261009T041513Z). Answers Codex review 5465636315: the server binds viewer records to submissions by content hash, the offline viewer keeps a durable refusal outbox, and the tests add a successful grant lifecycle and a grant-cannot-decrypt negative.
- **Author:** Claude Code (Anthropic) cloud session.
- **Status:** awaiting Payton AGREE. Not in MCP, not in the disk batches, no code.
- **Direction chosen by Payton (2026-10-08):** a per-case permission the driver grants, not a role.

## Problem

The server is own-submissions only: `PlatformAuth` yields a driver principal, and `CounselDesk.Verify` plus the three `AnalysisService` RPCs accept only the record owner. But five AGREEd use cases name other actors who act on a driver's records:

| Use case | Title | Actors today | Realizes |
| --- | --- | --- | --- |
| UC-RIDE-005 | Generate coverage matrix | Auditor, Counsel | FR-RIDE-007 Coverage matrix |
| UC-RIDE-006 | Online-hours policy check | Auditor | FR-RIDE-008 Online-hours policy audit |
| UC-RIDE-007 | Build incident time-window package | Auditor, Counsel | FR-RIDE-009 Time-window incident report, FR-RIDE-207 Portable audit ZIP export |
| UC-RIDE-010 | Counsel verification and decrypt path | Counsel, Admin | FR-RIDE-020, FR-RIDE-021 Verification UI/report, FR-RIDE-028, FR-RIDE-214 |
| UC-RIDE-011 | Escrow key and court release (included by UC-RIDE-010) | Admin, Counsel | FR-RIDE-022, 023, 024, 214, 216 |

Codex raised two P1s on PR #26 about this mismatch.

## Candidate C: driver-issued case grant

### Exact candidate sentence (BDPv4)

A driver may grant a named, authenticated counsel or auditor read-only access to specific imports and submissions of their own for one case, through a revocable, expiring case grant that lists the allowed operations. Without a valid grant, only the driver can verify, analyze or export that data. A grant is not a role, never covers another driver's data, and never decrypts a sealed record. Decryption stays on the escrow and HSM legal-process path, whose custodians act under court process, not under a case grant.

### Proposed ids (PROPOSED, not created)

| Kind | Proposed id | Role |
| --- | --- | --- |
| FR | **FR-RIDE-079** | Functional rule: driver-issued, case-scoped, revocable, expiring read-only grant to a named, authenticated grantee |
| TR | **TR-RIDE-SEC-004** | Technical: grantee principal, grant model, issuance, scope enforcement at every data sink, expiry, revocation and use logging. SEC-002/003 were killed and are not reused. |
| TEST | **TEST-RIDE-058** | Grant happy paths and fail-closed negatives, including scope binding and logging |

### Proposed acceptance criteria (for AGREE with the sentence)

FR-RIDE-079:

1. A grant names one grantor (the driver), one grantee principal, one case id, an explicit list of the grantor's own **import ids** and **submission ids**, the allowed operations, and an expiry. The allowed operations are drawn from: `verify`, `coverage`, `online-hours`, `incident-window` and `package-export`.
2. A grantee call succeeds only when all of these hold: the authenticated caller is the named grantee, the case id matches, the operation is allowed, and every row read is in the granted import or submission list. Every other call fails closed with `TENANT_ISOLATION` and returns no data.
3. Analysis and export results contain only rows whose import id is granted (coverage dictionary, online-hours, trips, scores, locations) and only granted submissions. A wide time window never pulls in an ungranted import.
4. The driver can revoke a grant at any time. Revoked or expired grants fail closed.
5. A grant never authorizes decryption, a working copy, escrow release, or any record of another driver.
6. Each successful grant use is recorded once with grant id, grantee principal, case id, operation, and the import and submission ids returned. Each refused grantee call, and each refused grant issuance or revocation, is recorded with grant id (if any), caller, operation and refusal reason.
7. The driver's own access needs no grant. A driver principal acting on their own data keeps today's own-submissions authorization.

TR-RIDE-SEC-004:

1. Grantees authenticate as their own principal kind (grantee account). Grants reference that principal. A grant secret alone never authorizes a call.
2. Grants are stored with the grantor's driver id. Issuance and revocation accept only that driver's principal. Issuance rejects any import or submission id the grantor does not own. A refused issuance or revocation changes no grant state.
3. Scope is checked server-side on every call, before any data is read: grantee principal, case id, operation, expiry, revocation, and the import and submission lists. Each sink (coverage dictionary, online-hours, trip index, scores, location rows, access-export ZIP, verification report) filters to the granted ids.
4. Use and refusal records are append-only and include the fields in FR-RIDE-079 AC 6.
5. Desktop viewer (UC-RIDE-018, 019 and 026). The viewer identifies the caller by an authenticated principal (driver or grantee), not by a free-text reviewer role as `CourtViewer` takes today. On the Grantee path it shows a record of a bundle only after an online check with the server confirms that the grant is valid (not expired, not revoked), names this grantee and case, allows `verify`, and lists that record's submission. Records the grant does not list are withheld, even when they are in the same bundle. The server, not the bundle, decides which submission a record is: the viewer sends the content hash it recomputes from the sealed ciphertext plus the record's receipt, and the server resolves the submission id from its custody journal. Any submission id or label carried in the bundle is ignored, and a record whose hash matches no submission of the grantor fails closed. If the server cannot be reached, the Grantee path fails closed. Each viewer check writes the use or refusal record in AC 4. When the server is unreachable, the viewer writes the refusal to a durable, append-only local outbox and uploads it to the server's refusal log on the next successful connection. Limit: the grant controls what RideAudit software shows. It cannot stop someone who already holds a copied bundle from recomputing ciphertext hashes with other tools. Plaintext stays behind the escrow release in every case.

TEST-RIDE-058:

1. Happy paths: a granted `verify`, `coverage`, `online-hours`, `incident-window` and `package-export` each succeed for granted data and write exactly one correctly attributed use record (grant id, grantee, case, operation, returned ids).
2. Scope binding: with granted import I1 and ungranted import I2 from the same driver, a wide incident window, online-hours and coverage return only I1 rows. `package-export` contains only I1 and granted submissions.
3. Failure cases, each asserting `TENANT_ISOLATION`, no data returned, and a refusal record:
   - ungranted submission;
   - another driver's data;
   - operation not granted (for example `package-export` on a grant without it);
   - wrong grantee (another authenticated principal presenting the grant id);
   - wrong case id;
   - expired grant;
   - revoked grant;
   - no grant.
4. The driver path: the driver verifies, analyzes and exports their own data with no grant.
5. Grant management, each refused with `TENANT_ISOLATION`, asserting that the grant store is unchanged (no grant created, the target grant still active) and that exactly one refusal record names the caller, operation (`issue` or `revoke`) and reason:
   - a grantee issues a grant over the grantor's ids;
   - another driver issues a grant over the grantor's ids;
   - the grantor issues a grant listing another driver's import or submission id;
   - a grantee revokes the grantor's grant;
   - another driver revokes the grantor's grant.
6. Desktop viewer, Grantee path, with a bundle that mixes granted submission S1 and ungranted submission S2:
   - a valid grant shows S1 and withholds S2, and writes one use record;
   - an expired grant, a revoked grant, and another grantee's principal each show nothing and write one refusal record;
   - a relabeled record (S2's ciphertext and receipt presented under S1's id or label) is withheld, because the server resolves it to S2, and writes one refusal record;
   - with the server unreachable, the viewer shows nothing, reports the fail-closed reason, and writes one refusal record to the local outbox; after the server is reachable again, that record appears once in the server's refusal log;
   - the Driver path shows the driver's own records with no grant.
7. Grant lifecycle through the real management calls (no fixture-seeded grant):
   - the grantor issues a grant, and the grant store shows it active with the listed ids, operations and expiry;
   - the grantee's call in scope then succeeds;
   - the grantor revokes it, and the grant store shows it revoked with the revocation time;
   - the same grantee call then fails with `TENANT_ISOLATION` and writes one refusal record.
8. Decryption boundary: a valid grant that allows every operation, with no court legal-process authorization. Attempts at escrow release and at creating a working copy each fail closed. No key material, plaintext or working copy is returned, escrow release state is unchanged, and each attempt writes one refusal record.

### Use-case changes that go with it (for AGREE)

Actors: **Driver** and **Grantee** (an authenticated counsel or auditor holding a case grant). Auditor, Counsel and Admin are removed as role actors. Preconditions apply to the Grantee path only; the Driver path keeps own-data authorization.

| Use case | Actors | Precondition (Grantee path only) | Basic flow, exact text |
| --- | --- | --- | --- |
| UC-RIDE-005 | Driver, Grantee | Grant allows `coverage` for the imports shown. | 1. Driver, or Grantee under a case grant, opens the coverage view for the driver's own imports. 2. System lists Lyft-collected signal categories vs audit-available data, limited to granted imports for a Grantee. 3. System marks missing signals and shows API-gap notice. |
| UC-RIDE-006 | Driver, Grantee | Grant allows `online-hours` for the imports read. | 1. Driver, or Grantee under a case grant, selects the policy profile for the driver's own data. 2. System loads available online intervals, limited to granted imports for a Grantee. 3. If data present, system evaluates 12h/6h and overrides. 4. System flags apparent violations only when hours data exists. |
| UC-RIDE-007 | Driver, Grantee | Grant allows `incident-window` and, for step 3, `package-export`. | 1. Driver, or Grantee under a case grant, selects the interval for the driver's own data. 2. System assembles available sealed-record references and metadata, limited to granted imports and submissions for a Grantee. 3. System exports the portable package (CSV, PDF summary, provenance JSON; FR-RIDE-207) with provenance and verification stubs. |
| UC-RIDE-010 | Driver, Grantee; steps 4-5 also Escrow custodians | Steps 1-3: grant allows `verify` for the submission. Steps 4-5: a court legal-process release under UC-RIDE-011, independent of any case grant. | 1. Driver, or Grantee under a case grant, opens the verification report for a sealed record. 2. System recomputes hash and checks on-chain receipt. 3. System verifies Play attestation and key binding. 4. Requesting party follows the documented legal process; escrow custodians release under UC-RIDE-011. 5. System issues an expiring authorized working copy and logs access. |
| UC-RIDE-011 | Escrow custodians (M-of-N key holders), Requesting party under court process | Court process for the release. A case grant neither grants nor is required for release authority. | Flow text unchanged except step 3: "Escrow custodians provide M-of-N approvals after the legal-process check." |

#### Other use cases that still name Admin, Auditor or Counsel (r3)

r1 and r2 covered only the five use cases above. These also name a role actor today. Applying the candidate without them would leave a role authority in place.

| Use case | Title | Actors today | Proposed actors | Flow change, exact text |
| --- | --- | --- | --- | --- |
| UC-RIDE-001 | Ingest privacy-export ZIP | Driver, Auditor | Driver | "Actor" becomes "Driver" in steps 1-2. A grant is read-only, so no grantee ingests on the driver's behalf. |
| UC-RIDE-002 | Record Smooth Cruiser evidence | Driver, Auditor | Driver | "Actor" becomes "Driver" in steps 1 and 3. Same reason. |
| UC-RIDE-004 | Import third-party telematics | Driver, Auditor | Driver | Step 1: "Driver uploads third-party export." Same reason. |
| UC-RIDE-013 | GPL-2.0 publish and notice | Admin | Maintainer | None (the flow names no actor). The maintainer publishes source and notices and has no access to driver data. |
| UC-RIDE-018 | Counsel composite playback | Counsel | Driver, Grantee; step 3 also Escrow custodians | 1. Driver, or Grantee under a case grant that allows `verify`, opens the composite record. 2. (unchanged) 3. On success, and only after a court legal-process release under UC-RIDE-011, system decrypts an expiring working copy for playback. 4. (unchanged) |
| UC-RIDE-019 | Desktop court viewer review | Counsel, Auditor | Driver, Grantee; step 3 also Escrow custodians | 1. Driver, or Grantee under a case grant that allows `verify`, opens the RideBundle in the desktop viewer on Win/Linux/macOS. Steps 2-5 unchanged. Step 3 already decrypts only via escrow release (UC-RIDE-011). |
| UC-RIDE-021 | Cross-cutting compliance and quality gates | Admin, Auditor, Counsel | Maintainer, Driver, Grantee | 1. Maintainer reviews compliance configuration. 2. Driver, or Grantee under a case grant that allows `package-export`, exports the portable audit ZIP (FR-RIDE-207) when needed, limited to granted imports and submissions for a Grantee. Steps 3-4 unchanged. |
| UC-RIDE-026 | Review sealed bundle with Avalonia desktop viewer | Counsel, Auditor | Driver, Grantee; step 2 decrypt also Escrow custodians | 1. Driver, or Grantee under a case grant that allows `verify`, launches the Avalonia UI 12 desktop viewer on Win, Linux, or macOS. 2. Viewer verifies, then decrypts only via escrow release under UC-RIDE-011. 3. Driver or Grantee inspects the synchronized timeline. |
| UC-RIDE-027 | Reuse shared Avalonia UI under GPL-2.0 | Developer, Auditor | Developer | None (the flow names only Developer). |
| UC-RIDE-031 | Prefer gRPC over interim OpenAPI companion | Developer, Auditor | Developer | None (the flow names only Developer). |

After this, no use case names Admin, Auditor or Counsel. "Maintainer" and "Developer" have no access to driver data.

TR-RIDE-SERVER-006 is unchanged. The grant is the explicit authorization its isolation rule already allows.

## Grantee authentication

r1 offered a bearer-secret option. Codex showed it contradicts a *named* grantee: possession of a secret cannot prove who is calling, and the use log would attribute a forwarded secret's use to the wrong person. r2 therefore specifies **grantee accounts** (TR-RIDE-SEC-004 AC 1).

If you prefer the bearer route anyway, the sentence and ACs must change so that the capability holder, not a named person, is the grantee, and logs must attribute use to the grant, not a person. That is a different AGREE.

## Not done

No MCP write, no batch edit, no proto or code change, no tests. Once the sentence, ACs and use-case changes are AGREEd, the next step is the MCP write (on LEGION2) plus the disk batch sync, then implementation and TEST-RIDE-058.
