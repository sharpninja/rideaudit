# Remediation receipt: PR #29 Codex review of 9cbddb6 (review 5475893328)

- **When (UTC):** 20261009T222519Z
- **When (operator):** 2026-10-09 17:25:19 CT
- **Branch:** claude/eager-pascal-wsb5at (PR #29).
- **Operator:** Claude Code (Anthropic) cloud session.
- **Classification:** traceability drift introduced by my own r32 edit; mine to fix, no owner decision. Codex's security review of 9cbddb6 found nothing new.
- **No force-push. No rebase. No amend.** The candidate is a spec only.

| Finding | Verified | Change |
| --- | --- | --- |
| P2 spec: the custodian allow-list includes `RegisterCustodian`, and the r32 refusal test excludes it, while the registration contract says it takes no credentials and refuses any call presenting them | Yes. Spec line 225 states the no-credentials rule; line 254 (allow-list) and the r32 test contradicted it. | A custodian token is accepted only by `CloseAccount`, `GetPendingRelease` and `ApproveCourtRelease`. `RegisterCustodian` and `RecoverAccount` are stated as calls that take no token. The refusal test now covers every RPC except those three plus `RecoverAccount`, and includes `RegisterCustodian`. |

Sibling sweep: the grantee allow-list (AC 7) never listed `RegisterGrantee`, so it has no matching drift. Every remaining `RegisterCustodian` reference agrees with the no-credentials rule; an r-numbered revision line from an earlier round is history and is left as written.

## Validation

- Counted edits (each match exactly once before writing). No en or em dash characters.
- No code changed since 9cbddb6, whose full suite pass (all ngrok harnesses, .NET) was clean, so it was not re-run for this spec-only change.
