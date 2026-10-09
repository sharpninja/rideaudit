# BDPv4 candidate for Payton AGREE: driver-issued case grant (not in MCP)

- **Written:** 2026-10-08 22:34 CT (America/Chicago), 20261009T033424Z.
- **Author:** Claude Code (Anthropic) cloud session.
- **Status:** awaiting Payton AGREE. Not in MCP, not in the disk batches, no code.
- **Direction chosen by Payton (2026-10-08):** a per-case permission the driver grants, not a role.

## Problem

The server is own-submissions only: `PlatformAuth` yields a driver principal, and `CounselDesk.Verify` plus the three `AnalysisService` RPCs accept only the record owner. But four AGREEd use cases name other actors who act on a driver's records:

| Use case | Title | Actors today | Realizes |
| --- | --- | --- | --- |
| UC-RIDE-005 | Generate coverage matrix | Auditor, Counsel | FR-RIDE-007 Coverage matrix |
| UC-RIDE-006 | Online-hours policy check | Auditor | FR-RIDE-008 Online-hours policy audit |
| UC-RIDE-007 | Build incident time-window package | Auditor, Counsel | FR-RIDE-009 Time-window incident report, FR-RIDE-207 |
| UC-RIDE-010 | Counsel verification and decrypt path | Counsel, Admin | FR-RIDE-020, FR-RIDE-021 Verification UI/report, FR-RIDE-028, FR-RIDE-214 |

Codex raised two P1s on PR #26 about this mismatch.

## Candidate C: driver-issued case grant

### Exact candidate sentence (BDPv4)

A driver may grant a named counsel or auditor read-only access to specific submissions of their own for one case, through a revocable, expiring case grant. Without a valid grant, only the driver can verify or analyze those submissions. A grant is not a role, never covers another driver's data, and never decrypts a sealed record. Decryption stays on the escrow and HSM legal-process path.

### Proposed ids (PROPOSED, not created)

| Kind | Proposed id | Role |
| --- | --- | --- |
| FR | **FR-RIDE-079** | Functional rule: driver-issued, case-scoped, revocable, expiring read-only grant for verification and analysis |
| TR | **TR-RIDE-SEC-004** | Technical: grant model, issuance, presentation, scope enforcement, expiry, revocation and access logging. SEC-002/003 were killed and are not reused. |
| TEST | **TEST-RIDE-058** | Grant happy path and fail-closed negatives |

### Proposed acceptance criteria (for AGREE with the sentence)

FR-RIDE-079:
1. A grant names one grantor (the driver), one grantee, one case id, an explicit list of the grantor's own submission ids, the allowed operations (verify, coverage, online-hours, incident window), and an expiry.
2. A grantee can call only the allowed operations, only for the listed submissions. Every other call fails closed with `TENANT_ISOLATION`.
3. The driver can revoke a grant at any time, and revoked or expired grants fail closed.
4. A grant never authorizes decryption, a working copy, or any record of another driver.
5. Each use of a grant is recorded with grant id, grantee, operation and submission id.

TR-RIDE-SEC-004:
1. Grants are stored with the grantor's driver id. Issuance and revocation accept only that driver's principal.
2. Scope is checked server-side on every call: grant id, grantee, case id, submission id, operation, expiry and revocation.
3. A grant secret, if used, is stored only as a hash and shown once at issuance.

TEST-RIDE-058:
1. The happy path passes for a granted verify and each granted analysis operation.
2. Failure cases (ungranted submission, other driver's submission, wrong operation, expired, revoked, missing grant) assert a clear `TENANT_ISOLATION` error and no data returned.

### Use-case changes that go with it (for AGREE)

- UC-RIDE-005, 006, 007 and 010: replace the Auditor, Counsel and Admin actors with **Driver** plus **Grantee (counsel or auditor holding a case grant)**. Admin is removed (no roles). Add a precondition: "the driver has issued a case grant covering these submissions and this operation."
- UC-RIDE-010 steps 4-5 (escrow release and working copy) stay on the escrow/HSM legal-process path (FR-RIDE-028, FR-RIDE-214). A case grant does not unlock them.
- TR-RIDE-SERVER-006 is unchanged. The grant is the explicit authorization its isolation rule already allows.

## Open choice before implementation: how the grantee authenticates

Identity issues driver principals only. Pick one:

1. **Bearer capability (smallest change):** issuance returns a one-time grant secret that the driver hands to the grantee. The grantee presents it with each call. There are no new accounts, but anyone holding the secret can use it until it expires or is revoked.
2. **Grantee account:** counsel or auditor enroll as their own principal (a new non-driver principal kind), and the grant names that principal. This is stronger binding, but it adds an identity flow.

## Not done

No MCP write, no batch edit, no proto or code change, no tests. Once the sentence, ACs, use-case changes and the authentication choice are AGREEd, the next step is the MCP write (on LEGION2) plus the disk batch sync, then implementation and TEST-RIDE-058.
