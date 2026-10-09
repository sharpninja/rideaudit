# BDPv4 candidate for Payton AGREE: driver-issued case grant (not in MCP)

- **Written:** 2026-10-08 22:34 CT (America/Chicago), 20261009T033424Z.
- **Revised:** r2, 2026-10-08 22:50 CT (20261009T035027Z). Answers Codex's review of r1 on PR #29 (HV pair `docs/reviews/hv-pairs/20261009T034331Z-pr29-codex-hv.json`, DISAGREE 96/92, ineligible model).
- **Revised:** r3, 2026-10-08 22:59 CT (20261009T035928Z). Answers Codex review 5465551807 (UC-RIDE-021 left out; no grantor-only issuance or revocation tests). r3 lists every use case in `docs/Project/Use-Cases-Batch.yaml` that still names Admin, Auditor or Counsel, not only the five r1 named.
- **Revised:** r4, 2026-10-08 23:08 CT (20261009T040851Z). Answers Codex review 5465599386: desktop viewer flows now need an online grant check (TR-RIDE-SEC-004 AC 5, TEST-RIDE-058 item 6), and refused grant management is logged and tested.
- **Revised:** r5, 2026-10-08 23:15 CT (20261009T041513Z). Answers Codex review 5465636315: the server binds viewer records to submissions by content hash, the offline viewer keeps a durable refusal outbox, and the tests add a successful grant lifecycle and a grant-cannot-decrypt negative.
- **Revised:** r6, 2026-10-08 23:21 CT (20261009T042124Z). Answers Codex review 5465667258: every field the viewer displays must be bound to the resolved submission (sidecar grafting), and the mixed-bundle test asserts the refusal for the withheld record.
- **Revised:** r7, 2026-10-09 10:22 CT (20261009T152241Z). Answers Codex review 5471995829: the viewer's Driver path also resolves every record server-side and accepts only the driver's own submissions; cross-driver bundle negative added.
- **Revised:** r8, 2026-10-09 10:29 CT (20261009T152952Z). Answers Codex review 5472099862: exact title, brief-description and AC-text replacements; Driver-path viewer use record; grantees refused at every mutation endpoint.
- **Revised:** r9, 2026-10-09 10:36 CT (20261009T153644Z). Answers Codex review 5472175684: an explicit operation-to-RPC map, with every other read RPC refusing a grantee (TR-RIDE-SEC-004 AC 7, TEST-RIDE-058 item 10); the grafting negative includes the OTS proof.
- **Revised:** r10, 2026-10-09 10:50 CT (20261009T155020Z). Answers Codex review 5472262525: grant context transport on every mapped RPC (TR-RIDE-SEC-004 AC 8, TEST-RIDE-058 item 11); interval-bound package export; Driver-path grafting and offline negatives.
- **Revised:** r11, 2026-10-09 11:00 CT (20261009T160019Z). Answers Codex review 5472419763: the interval is a public request field and its test uses the public RPC; grantee packages exclude the driver's identity datasets.
- **Revised:** r12, 2026-10-09 11:16 CT (20261009T161621Z). Answers Codex review 5472589691: public contracts for grantee accounts, grant management and viewer authorization; escrow binding; duplicate case id; interval only for UC-RIDE-007 packages (TEST-RIDE-058 items 12 and 13).
- **Revised:** r13, 2026-10-09 11:24 CT (20261009T162405Z). Answers Codex review 5472671546: viewer RPCs are in the operation map (refusal upload is an authenticated exception); DisableGrantee is per-grantor; the viewer uses the server-authoritative case, not the bundle's.
- **Revised:** r14, 2026-10-09 11:31 CT (20261009T163117Z). Answers Codex review 5472741454: AC 7 exceptions for ListMyGrants, CloseAccount and the court path (with a positive court-authorized grantee test); Identity.CloseAccount defined.
- **Revised:** r15, 2026-10-09 11:40 CT (20261009T164004Z). Answers Codex review 5472826317: an inactive grantee keeps the independently authorized calls; grantee registration takes its identity from the invitation.
- **Revised:** r16, 2026-10-09 11:47 CT (20261009T164717Z). Answers Codex review 5472895040: the server sends the invite code to the invited address and never returns it to the driver; registration takes no credentials, and an existing grantee accepts a later invitation only for its own registered email; use records carry the `email-verified` assurance level and claim no more than that.
- **Revised:** r17, 2026-10-09 12:00 CT (20261009T170036Z). Answers Codex review 5472981248: `RegisterGrantee` has no identity fields and its codes are single-use, with a replay test; the driver learns the accepting grantee id through `Grants.ListInvitations`; `RecoverAccountResponse` is principal-aware; the disable test checks that the disabled driver cannot issue a new grant.
- **Revised:** r18, 2026-10-09 12:11 CT (20261009T171121Z). Answers Codex review 5473119678: a stored sidecar commitment, with canonical cue encoding and a positive test that genuine sidecars stay visible (an ingest dependency); custodian principals for the court path; invite codes of at least 128 bits, stored hashed, with rate limits and uniform refusals.
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
3. Analysis and export results contain only rows whose import id is granted (coverage dictionary, online-hours, trips, scores, locations) and only granted submissions. A wide time window never pulls in an ungranted import. A `package-export` for UC-RIDE-007 carries the selected interval, and rows outside it are excluded. A grantee package never contains the identity datasets of the driver's own access export (`account/` entries: account, vehicles, profiles, sessions), because a grant cannot name them.
4. The driver can revoke a grant at any time. Revoked or expired grants fail closed.
5. A grant never authorizes decryption, a working copy, escrow release, or any record of another driver.
6. Each successful grant use is recorded once with grant id, grantee principal, case id, operation, and the import and submission ids returned. Each refused grantee call, and each refused grant issuance or revocation, is recorded with grant id (if any), caller, operation and refusal reason.
7. The driver's own access needs no grant. A driver principal acting on their own data keeps today's own-submissions authorization.

TR-RIDE-SEC-004:

1. Grantees authenticate as their own principal kind (grantee account). Grants reference that principal. A grant secret alone never authorizes a call.
2. Grants are stored with the grantor's driver id. Issuance and revocation accept only that driver's principal. Issuance rejects any import or submission id the grantor does not own. A refused issuance or revocation changes no grant state.
3. Scope is checked server-side on every call, before any data is read: grantee principal, case id, operation, expiry, revocation, and the import and submission lists. Each sink (coverage dictionary, online-hours, trip index, scores, location rows, access-export ZIP, verification report) filters to the granted ids.
4. Use and refusal records are append-only and include the fields in FR-RIDE-079 AC 6.
5. Desktop viewer (UC-RIDE-018, 019 and 026). The viewer identifies the caller by an authenticated principal (driver or grantee), not by a free-text reviewer role as `CourtViewer` takes today. On the Grantee path it shows a record of a bundle only after an online check with the server confirms that the grant is valid (not expired, not revoked), names this grantee and case, allows `verify`, and lists that record's submission. Records the grant does not list are withheld, even when they are in the same bundle. The server, not the bundle, decides which submission a record is: the viewer sends the content hash it recomputes from the sealed ciphertext plus the record's receipt, and the server resolves the submission id from its custody journal. Any submission id or label carried in the bundle is ignored, and a record whose hash matches no submission of the grantor fails closed. Every field the viewer displays for a record must be bound to that resolved submission: telematics cues, GPS and OBD2 presence, admission state and the OTS proof (today `ReviewRecord.Telematics`, `GpsPresent`, `Obd2Present`, `Admitted` and `Ots`, which `TimelineBuilder` renders straight from the bundle). A field is bound when the content hash covers it, when it matches a digest the server returns for that submission, or, for admission state, when the server supplies it. The OTS proof is bound only after it verifies against the resolved content hash. An unbound field is not displayed, and its withholding writes a refusal record. The Driver path uses the same server resolution: each record must resolve to a submission the authenticated driver owns, or it is withheld and a refusal is recorded. Holding a copied bundle never makes another driver's record "own data". The field-binding rules above apply on both paths. The case shown, logged and carried into any working copy is the server-authoritative case from the grant context, never `bundle.CaseId`. If the bundle names another case, the record is withheld and a refusal is recorded. If the server cannot be reached, the Grantee path fails closed, and so does the Driver path for any record it cannot resolve. Each viewer check writes the use or refusal record in AC 4. When the server is unreachable, the viewer writes the refusal to a durable, append-only local outbox and uploads it to the server's refusal log on the next successful connection. Limit: the grant controls what RideAudit software shows. It cannot stop someone who already holds a copied bundle from recomputing ciphertext hashes with other tools. Plaintext stays behind the escrow release in every case.
6. A grantee principal has no write path. These mutating RPCs refuse it with `TENANT_ISOLATION` before any state change, and each refusal writes a refusal record:
   - `Identity.RegisterVehicle`, `UpdateVehicle` and `PutConfigurationProfile`;
   - `Ingest.IngestPrivacyExport`, `IngestThirdParty` and `RecordManualScore`;
   - `Privacy.RequestDeletion`;
   - `Admission.OpenSession`, `SubmitSealed` and `UploadSealedChunk`;
   - grant issuance and revocation (AC 2).

   `Escrow.RequestCourtRelease`, `ApproveCourtRelease` and `OpenExpiringWorkingCopy` are authorized only by the court legal-process path, never by a grant (FR-RIDE-079 AC 5; see the exceptions under AC 7).

7. Every RPC refuses a grantee unless it is mapped to an operation the grant allows. The mapping is:

   | Operation | RPC |
   | --- | --- |
   | `verify` | `Counsel.BuildVerificationReport`, `Escrow.VerifyForCounsel` |
   | `coverage` | `Counsel.AnalyzeCoverage` |
   | `online-hours` | `Counsel.AnalyzeOnlineHours` |
   | `incident-window` | `Counsel.AnalyzeIncidentWindow` |
   | `package-export` | `Privacy.RequestAccessExport`, filtered per AC 3 |
   | `verify` | `Grants.ResolveViewerRecords` (Grantee path; the Driver path needs no grant) |

   A grant authorizes only the mapped calls above. These calls are authorized on their own terms, independent of any grant:
   - `Grants.UploadViewerRefusals` needs no currently valid grant, because it reports refusals that often arise from expired, revoked or missing grants. It accepts only refusal entries about the caller's own attempts, and it returns no data.
   - `Grants.ListMyGrants` lists only grants naming the caller (metadata, no data), so a grantee can learn the grant ids it must present in `GrantContext`.
   - `Identity.CloseAccount` lets an authenticated principal close their own account (see "Public contracts").
   - `Grants.AcceptInvitation` lets an existing grantee accept an invitation addressed to its own registered email (see "Public contracts"). It returns no data.
   - `Escrow.RequestCourtRelease`, `ApproveCourtRelease` and `OpenExpiringWorkingCopy` follow the court legal-process path (UC-RIDE-011, with the escrow binding in "Public contracts"). A grantee who is also the authenticated requesting party under court process may use them on that authority, never on a grant's.

   All other read RPCs refuse a grantee with `TENANT_ISOLATION`, return no data, and write a refusal record. These include `Identity.ListVehicles`, `GetConfigurationProfile`, `Admission.GetAdmissionStatus`, `Escrow.GetEscrowStatus` and `Privacy.ViewLocations`.

8. Grant transport. Every grantee call carries an authoritative grant context, `{grant_id, case_id}`, as a request field on each mapped RPC. `Privacy.RequestAccessExport` also gains an optional `interval {start_unix_millis, end_unix_millis}` field. A UC-RIDE-007 incident package sets it, and rows outside it are excluded. A UC-RIDE-021 portable export leaves it unset and covers the whole granted scope. `Counsel.VerificationReportRequest.case_id` is deprecated: if it is set, it must equal `GrantContext.case_id`, or the call is refused. This is a contract change: a `GrantContext` message is added to each request in the AC 7 map, and the minor contract version is bumped. The server authorizes against that one grant only. It never infers a grant from the caller and never unions several grants. A grantee call without a grant context, or whose grant id is not the caller's, is refused with `TENANT_ISOLATION`. The use or refusal record names the presented grant id.

TEST-RIDE-058:

1. Happy paths: a granted `verify`, `coverage`, `online-hours`, `incident-window` and `package-export` each succeed for granted data and write exactly one correctly attributed use record (grant id, grantee, case, operation, returned ids).
2. Scope binding: with granted import I1 and ungranted import I2 from the same driver, a wide incident window, online-hours and coverage return only I1 rows. `package-export` contains only I1 and granted submissions. An interval-bound `package-export` (UC-RIDE-007 step 3), requested through the public `Privacy.RequestAccessExport` gRPC call with the interval field, contains no row outside the interval, even from a granted import, and contains no `account/` entry.
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
   - a valid grant shows S1 and withholds S2, and writes exactly one use record for S1 and one refusal record for S2, each naming the grant, grantee, case and resolved submission;
   - sidecar grafting: S2's telematics, GPS and OBD2 flags, admission state and OTS proof attached to S1's sealed record. S1's verified core is shown, but none of the grafted fields are (the grafted OTS proof fails verification against S1's content hash and is withheld), and one refusal record names S1 and the withheld fields;
   - an expired grant, a revoked grant, and another grantee's principal each show nothing and write one refusal record;
   - a relabeled record (S2's ciphertext and receipt presented under S1's id or label) is withheld, because the server resolves it to S2, and writes one refusal record;
   - with the server unreachable, the viewer shows nothing, reports the fail-closed reason, and writes one refusal record to the local outbox; after the server is reachable again, that record appears once in the server's refusal log;
   - the Driver path shows the driver's own records with no grant, after the server resolves each one to a submission that driver owns, and writes exactly one use record per record naming the driver and the resolved submission;
   - cross-driver bundle: an authenticated driver opens a bundle holding another driver's record. The server resolves it to the other driver's submission, so the viewer withholds it with no grant fallback and writes one refusal record.
   - Driver-path sidecar grafting: another driver's telematics, GPS and OBD2 flags, admission state and OTS proof attached to the driver's own sealed record S1. S1's verified core is shown, the grafted fields are not, and one refusal record names S1 and the withheld fields;
   - Driver path offline: with the server unreachable, a driver opening a bundle that holds another driver's record sees nothing for any record, gets the fail-closed reason, and one refusal record goes to the local outbox and reaches the server's refusal log once after reconnect;
7. Grant lifecycle through the real management calls (no fixture-seeded grant):
   - the grantor issues a grant, and the grant store shows it active with the listed ids, operations and expiry;
   - the grantee's call in scope then succeeds;
   - the grantor revokes it, and the grant store shows it revoked with the revocation time;
   - the same grantee call then fails with `TENANT_ISOLATION` and writes one refusal record.
8. Decryption boundary: a valid grant that allows every operation, with no court legal-process authorization. Attempts at escrow release and at creating a working copy each fail closed. No key material, plaintext or working copy is returned, escrow release state is unchanged, and each attempt writes one refusal record.
9. Read-only grant: with a valid grant that allows every operation, the grantee calls each mutating RPC in TR-RIDE-SEC-004 AC 6 against the grantor's data. Each call returns `TENANT_ISOLATION` and no data. The import, normalized, custody, vehicle, profile and key stores are unchanged, and each call writes one refusal record.
10. Unmapped reads: with a valid grant that allows every operation, the grantee calls each read RPC outside the AC 7 mapping (`Identity.ListVehicles`, `GetConfigurationProfile`, `Admission.GetAdmissionStatus`, `Escrow.GetEscrowStatus`, `Privacy.ViewLocations`). Each call returns `TENANT_ISOLATION` and no data, and writes one refusal record.
11. Grant transport, through the public gRPC contracts: the same grantee holds grant A (case CA, import IA) and grant B (case CB, import IB). A call presenting grant A returns only IA rows and logs grant A. A call presenting grant A's id with case CB is refused. A call with no grant context is refused. Each refusal writes one refusal record naming the presented grant id, if any.
12. Public contracts, through gRPC (see "Public contracts" below):
    - a grantee invited by the driver registers with the invite code, authenticates, and is the grantee used by item 7; the registered identity is the invitation's email and name; the `RegisterGrantee` request has no email or name field, and the registered email and display name equal the invitation's; replaying a redeemed or accepted code, through `RegisterGrantee` or `AcceptInvitation`, is refused and creates no second account, token or recovery code; the driver obtains the grantee id used by item 7 only from its own `ListInvitations` (status `accepted`), and another driver's `ListInvitations` does not show that invitation; `RecoverAccount` for that grantee returns `principal_kind` `GRANTEE`, its grantee id, empty `driver_id` and `tenant_id`, and a token that works, while a driver's recovery is unchanged apart from the new fields; the `InviteGrantee` response, the driver's other responses and the server logs never contain the code, and it reaches only the invitation address through a test mail sink; a redemption made with the inviting driver's or another principal's credentials is refused, as is an expired code; a second driver's invitation to the same address is accepted through `AcceptInvitation` by that grantee and refused for a different grantee; the grant-use records name the grantee principal with assurance `email-verified`;
    - after driver A calls `DisableGrantee` on a grantee who also holds a grant from driver B, A's grants are refused and B's still work (two-grantor isolation); A's next `IssueGrant` naming that grantee is refused and creates no grant, while B's `IssueGrant` naming it still succeeds; a grantee who calls `Identity.CloseAccount` is then refused everywhere, including with the old token and recovery code;
    - `Grants.ListMyGrants` returns only the caller's grants, and works with no `GrantContext`;
    - a grantee who is also the authenticated requesting party under a court process obtains a working copy after M-of-N custodian approval, through the court-path RPCs and with no grant involved;
    - viewer resolution and refusal upload run through `Grants.ResolveViewerRecords` and `Grants.UploadViewerRefusals`, and replaying the same outbox entry records it once;
    - a `VerificationReportRequest` whose `case_id` differs from `GrantContext.case_id` is refused;
    - a UC-RIDE-021 portable export with no interval returns the whole granted scope, and nothing outside it;
    - a bundle relabeled with case CB, opened under a grant for case CA, is withheld with one refusal record, and nothing is shown or exported under CB;
    - a genuine record whose bundle sidecars match its commitment shows its telematics cues and GPS and OBD2 flags on both paths, with no refusal; reordered or edited cues, or a flipped flag, are withheld with one refusal record; a submission with no commitment shows its verified core with the sidecars withheld as `SIDECAR_UNCOMMITTED`;
    - repeated invalid `RegisterGrantee` calls from one source are throttled; the refusal for an unknown, expired, consumed or throttled code is identical; the real invitation still redeems afterward from another source; the issued code carries at least 128 bits of entropy, and the invitation store holds only its hash;
    - a custodian provisioned and registered through gRPC approves a release for a key that names it, and the approval counts toward quorum;
    - `ResolveViewerRecords` succeeds under a grant allowing `verify` and is refused under one without it; `UploadViewerRefusals` is accepted after the grant has expired or been revoked, but not for another caller's attempts.
13. Escrow binding, each failing closed with no key, plaintext or working copy, and one refusal record:
    - `VerifyForCounsel` with a real release id that belongs to another submission, grantor or case reports no quorum and no `WorkingCopyAuthorized`;
    - `RequestCourtRelease` with a `requester_id` other than the authenticated caller is refused;
    - `ApproveCourtRelease` by a driver or grantee token, by a custodian not named on that key, or with a request `custodian_id` that differs from the authenticated custodian is refused, and no approval is recorded;
    - `OpenExpiringWorkingCopy` by anyone but the authenticated requester of that release is refused.


### Public contracts (proposed, r12)

These come with the same minor contract-version bump as `GrantContext` (TR-RIDE-SEC-004 AC 8).

- **Grantee accounts.** The driver invites a grantee with `Grants.InviteGrantee {email, display_name}`. The server returns only an invitation id and status to the driver, never the code. It sends the one-time invite code to the invitation email address itself, so the code reaches only whoever controls that mailbox. This makes outbound mail a new server dependency. The grantee redeems the code with `Identity.RegisterGrantee {invite_code}`. The request has no identity fields: the registered email and display name are taken solely from the invitation. The code is single-use. Once redeemed or accepted it is consumed, and a replay creates no second account, token or recovery code. It is a random token of at least 128 bits from a cryptographic generator, and the server stores only its hash. Because `RegisterGrantee` is unauthenticated, it is rate-limited per source address and globally. Every failed redemption gets the same refusal, whether the code is unknown, expired, consumed or throttled, so no response shows whether a code exists. A failed attempt never consumes or locks a real invitation. `RegisterGrantee` takes no credentials; a call that presents any principal's credentials, including the inviting driver's, is refused. A grantee who already has an account accepts a later invitation, from the same or another driver, with `Grants.AcceptInvitation {invite_code}` under its own grantee credentials. That call succeeds only when the invitation's email equals the grantee's registered email; for anyone else it is refused and the code stays unused. The inviting driver learns who accepted through `Grants.ListInvitations` (below). The code expires, and it never appears in logs or in any response to the driver. Grant-use and refusal records name the grantee principal and its assurance level, `email-verified`. That level proves control of the invited address, not a real-world identity, and any attribution of a log to a named person is limited to that. The grantee receives a grantee principal, an access token and a recovery code, under the same credential rules as drivers. `Identity.RecoverAccount` works for grantees. `RecoverAccountResponse` gains `principal_kind` (`DRIVER`, `GRANTEE` or `CUSTODIAN`) and `principal_id`, which are additive fields under the same minor bump. For a driver, `principal_id` equals `driver_id`, and the existing fields are unchanged. For a grantee, `driver_id` and `tenant_id` are empty, so no grantee id is ever carried in a driver field. The inviting driver can call `Grants.DisableGrantee {grantee_id}`. This disables only the relationship between that driver and that grantee: it revokes that driver's grants and blocks new ones from that driver, and grants from other drivers are unaffected. Disabling the grantee account itself is reserved for the account owner, through `Identity.CloseAccount {}` (authenticated; it revokes the caller's tokens and recovery code, ends every grant naming the caller, and cannot be undone), or for an authorized operator. A grantee with no live grant has no grant-scoped data access. The calls TR-RIDE-SEC-004 AC 7 authorizes independently of a grant still work for it. *Choice for Payton: driver-invited grantee accounts with server-sent email codes, as written here, or operator-provisioned ones where an operator verifies the identity. Either way, a code returned to the driver is not enough, because the driver could redeem it themselves (Codex review 5472895040).*
- **Grant management.** These calls take the driver principal only:
  - `Grants.IssueGrant {grantee_id, case_id, import_ids[], submission_ids[], operations[], expires_unix_millis}` returns `Grant`;
  - `Grants.RevokeGrant {grant_id}` returns `Grant`;
  - `Grants.ListGrants {}` lists the caller's issued grants;
  - `Grants.ListInvitations {}` lists the caller's own invitations: invitation id, email, display name, status (`pending`, `accepted` or `expired`) and, once accepted, the accepting `grantee_id`. It never returns the code. This is how the driver gets the `grantee_id` that `IssueGrant` needs.

  A grantee can call `Grants.ListMyGrants {}`. It returns grant id, grantor, case, operations and expiry, but no data.
- **Viewer authorization.**
  - `Grants.ResolveViewerRecords {grant_context?, records[{content_hash, receipt_core}]}` returns, per record, either a refusal reason, or the resolved submission id, its admission state and its stored sidecar commitment: `gps_present`, `obd2_present` and `telematics_digest`. The viewer re-encodes the bundle's telematics cues canonically (SHA-256 of the deterministic protobuf encoding of `ReviewSidecarCues {repeated TelematicsCue cues}`, in recorded order) and shows them only when the digest matches. It shows the GPS and OBD2 presence flags only when they equal the server's values. `grant_context` is absent on the Driver path.
  - **Sidecar commitment (ingest dependency).** Today the server never receives the telematics cues or the GPS and OBD2 flags. They exist only in the bundle (`ReviewRecord`), so the server has nothing to bind them to. The device's submission therefore adds `sidecar_commitment {telematics_digest, gps_present, obd2_present}`, which is stored with the submission in the custody journal and never rewritten. A submission without one, including every submission made before this change, has unbound sidecars. The viewer still shows its verified core and withholds the sidecars with reason `SIDECAR_UNCOMMITTED`.
  - `Grants.UploadViewerRefusals {entries[{client_event_id, at_unix_millis, grant_id?, record_hash, reason}]}` is idempotent by `client_event_id`.
- **Escrow binding.** This is a dependency of FR-RIDE-079 AC 5, on the court path:
  - `VerifyForCounsel` counts quorum only for a release whose key belongs to the requested submission's sealed record, grantor tenant and case;
  - `RequestCourtRelease` binds `requester_id` to the authenticated caller;
  - `ApproveCourtRelease` accepts only an authenticated custodian principal named on that key (see "Custodian principals");
  - `OpenExpiringWorkingCopy` opens only for the authenticated requester of that release.
- **Custodian principals (court-path dependency).** Today escrow custodians are bare strings on the key. The server authenticates only `DriverPrincipal`, and `ApproveCourtRelease` trusts the request's `custodian_id`. This adds a third principal kind, custodian:
  - an authorized operator provisions one with `Identity.ProvisionCustodian {email, display_name}`. The server sends a single-use code to that address under the same rules as grantee codes, and returns only a custodian id and status;
  - the custodian redeems it with `Identity.RegisterCustodian {invite_code}`, which issues a custodian principal, an access token and a recovery code. `RecoverAccount` reports `principal_kind` `CUSTODIAN`;
  - the custodian ids recorded on an escrow key are custodian principal ids;
  - `ApproveCourtRelease` takes the custodian from the authenticated token. A request `custodian_id` that differs from it is refused. The approval counts only when that principal is named on the key;
  - a custodian principal has no driver or grantee access.

  *Choice for Payton, tied to the escrow defect fix (PR comment of 2026-10-09 16:17Z): operator-provisioned custodian principals as written here, or another custodian model.*

  The current master code does none of these (`GrpcServices.cs`, `EscrowGrpcService`). That is a pre-existing defect, reported separately on PR #29.

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


#### Titles, brief descriptions and AC text that still name a role (r8)

The actor and flow edits above leave these role names behind. Exact replacements:

| Use case | Field | Today | Proposed |
| --- | --- | --- | --- |
| UC-RIDE-001 | briefDescription | Driver or auditor uploads a consented Lyft privacy-export ZIP; system parses DataDictionary files and tags unknowns Unverified. | Driver uploads a consented Lyft privacy-export ZIP; system parses DataDictionary files and tags unknowns Unverified. |
| UC-RIDE-007 | briefDescription | Export available GPS, scores, and third-party events for a counsel time window. | Export available GPS, scores, and third-party events for a case time window, for the Driver or a Grantee under a case grant. |
| UC-RIDE-010 | title | Counsel verification and decrypt path | Verification report and legal-process decrypt path |
| UC-RIDE-010 | briefDescription | Counsel verifies hash, chain, attestation, escrow logs, then obtains an expiring RAES or RIDESEAL1 working copy only through the counsel and HSM path. Public admission does not decrypt. approval: Payton AGREE 2026-09-29 for the RAES AC. | Driver, or Grantee under a case grant, verifies hash, chain, attestation and escrow logs. An expiring RAES or RIDESEAL1 working copy is issued only through the escrow and HSM legal-process path (UC-RIDE-011). Public admission does not decrypt. approval: Payton AGREE 2026-09-29 for the RAES AC. |
| UC-RIDE-010 | AC-UC-010-003 | RAES or RIDESEAL1 decrypt happens only as an expiring counsel working copy after HSM escrow release. Public admission stays fail-closed. | RAES or RIDESEAL1 decrypt happens only as an expiring working copy after HSM escrow release under court legal process. Public admission stays fail-closed. |
| UC-RIDE-018 | title | Counsel composite playback | Composite playback |
| UC-RIDE-019 | briefDescription | Reviewer uses GPL2 desktop viewer to verify and display RideBundle via escrow release path. | Driver, or Grantee under a case grant, uses the GPL2 desktop viewer to verify and display a RideBundle; decryption only via the escrow release path. |
| UC-RIDE-026 | briefDescription | Counsel or auditor opens a RideBundle in the Avalonia UI 12 desktop court viewer. | Driver, or Grantee under a case grant, opens a RideBundle in the Avalonia UI 12 desktop court viewer. |

AC-UC-010-003 is an AGREEd AC (RAES, 2026-09-29). Its rewording drops only the role word; the decrypt rule is unchanged.

After this, no use case names Admin, Auditor or Counsel in its actors, title, brief description, basic flow or AC text. "Maintainer" and "Developer" have no access to driver data.

TR-RIDE-SERVER-006 is unchanged. The grant is the explicit authorization its isolation rule already allows.

## Grantee authentication

r1 offered a bearer-secret option. Codex showed it contradicts a *named* grantee: possession of a secret cannot prove who is calling, and the use log would attribute a forwarded secret's use to the wrong person. r2 therefore specifies **grantee accounts** (TR-RIDE-SEC-004 AC 1).

If you prefer the bearer route anyway, the sentence and ACs must change so that the capability holder, not a named person, is the grantee, and logs must attribute use to the grant, not a person. That is a different AGREE.

## Not done

No MCP write, no batch edit, no proto or code change, no tests. Once the sentence, ACs and use-case changes are AGREEd, the next step is the MCP write (on LEGION2) plus the disk batch sync, then implementation and TEST-RIDE-058.
