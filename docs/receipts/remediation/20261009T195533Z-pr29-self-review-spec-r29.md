# Remediation receipt: PR #29 holistic self-review, spec half (candidate r29)

- **When (UTC):** 20261009T195533Z
- **When (operator):** 2026-10-09 14:55:33 CT
- **Branch:** claude/eager-pascal-wsb5at (PR #29).
- **Operator:** Claude Code (Anthropic) cloud session, at Payton's request ("Before next commit, do another holistic self-review").
- **No force-push. No rebase. No amend.** The candidate is a spec only: no MCP writes and no implementation until Payton AGREEs.
- **File:** docs/receipts/requirements/20261009T033424Z-bdpv4-candidate-case-grant.md (r29 "Revised" line).

A read-through of r28 found 21 internal inconsistencies or gaps. Each was checked against the spec text, and the code claims against the source (`HsmKeyCustody.RequestRelease` refuses a requester only when it is in `package.CustodianIds` and requires a matching tenant; `EscrowGrpcService.VerifyForCounsel` only calls `Require(context)`). All 21 were confirmed and fixed in 34 exact-match edits.

| Area | Inconsistency in r28 | r29 change |
| --- | --- | --- |
| Problem | Said the server was own-submissions only, but the escrow RPCs check only authentication; counted five use cases against fifteen elsewhere | Names the escrow-RPC exception; fifteen use cases (five tabled, ten in the r3 table) |
| FR-079 item 2 | Omitted expiry and revocation from the success conditions; the fail-closed rule read as covering non-grant calls | Adds both; the rule is scoped to grant-scoped calls, with AC 7 calls following their own rules |
| Export | Manifest, provenance and `account/` scope disagreed between two sections | Every `account/` entry, vehicle versions included; manifest, provenance and receipts list only granted items |
| Grantee mutations | Listed in part | Every grantee mutation listed |
| `ApproveCourtRelease` | Grouped with grantee operations in one place, custodian-only in another | Custodian-only throughout |
| Operation table | Two `verify` rows | Merged into one |
| Grant RPCs | `ListMyGrants` result fields, `ListGrants`, `ListInvitations` and `GetPendingRelease` were referenced but not specified | Specified, with import and submission ids |
| `GrantContext` | Field set given differently in two places | Defined once, with the registration-retry rule |
| Idempotency | Replay with a different key or after 24 h unspecified | Refused; negative tests added |
| Invitations and accounts | Entropy sentence detached from the single-use rule; retry and unique-email check order unclear; `account close` command named but not defined; custodian recovery fields unspecified | Fixed in place; `rideaudit-admin account close --principal-id` defined; `RegisterCustodian` follows the no-credentials rule; recovery has empty driver fields |
| `IssueGrant` | Did not require an accepted relationship | Requires an accepted, non-disabled relationship; restoring a disabled one is a new owner choice |
| Court release | No requester and approver separation; `RequestCourtRelease` principal kinds and tenant rule unspecified; Driver path of `VerifyForCounsel` had no owner check | Separation, principal kinds, tenant rule and owner check specified; custodian allow-list stated; negative tests added to item 13 |
| Wording | Effective-holder phrasing, two punctuation slips, heading revision | Fixed; heading reads "revised through r29" |

## Deferred

- Moving the custodian test items from item 12 into item 13 would be tidier but is a reorganization, not a correctness fix. Left for after the AGREE decision.

## Checks

- Each edit matched exactly once before it was applied (script aborts on a miss).
- No en or em dash characters in the spec, HANDOFF.md or this receipt.
- Code unchanged in this commit; the last full test run (1acd9a4) passed: Chain 17, Client 111, Escrow 4, Protos 5, Seal 8, Server.Admission 32, Workflow 29.
