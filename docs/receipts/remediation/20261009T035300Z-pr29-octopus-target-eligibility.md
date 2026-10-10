# Remediation receipt: Octopus named target must be in the deploy environment and role

- **When (UTC):** 20261009T035300Z
- **When (operator):** 2026-10-08 22:53 CT
- **Branch:** claude/eager-pascal-wsb5at (PR #29).
- **Operator:** Claude Code (Anthropic) cloud session.
- **Source:** Codex review 5465487068 on PR #29, P2 on `deploy/octopus/Invoke-RideAuditOctopusRelease.ps1` `Wait-RoleTarget`.
- **No force-push. No rebase. No amend.**

| Codex finding | Verified | Change |
| --- | --- | --- |
| P2: `Wait-RoleTarget` returns an exact-name match without checking its environment or `$TargetRole`, and the deploy then pins `SpecificMachineIds` to it | Yes. A machine named `LAB-OMARCHY-DOCKER` in another environment, or without the role, was returned as is. | `Wait-RoleTarget` takes `-EnvironmentId` (the caller passes `$environment.Id`) and refuses an empty id. A single named match outside the environment or without the role throws `refuse to pin a deploy to it`. The unique-role fallback counts only targets in the environment with the role. |
| Same class, SSH path (found while checking) | Yes. `Ensure-LinuxTarget` reused `LAB-OMARCHY-LINUX` and repaired its role but not its environments. | A reused target missing the deploy environment gets it added in the same PUT. |

## Evidence (this session)

- PowerShell 7.6.6, function extracted from the script by AST with a mocked machine list:
  - named target in env with role: returned;
  - named target in another env: throws, `in Development=False`;
  - named target without role: throws, `role ...=False`;
  - two targets with the name: throws ambiguous;
  - name absent, one roled target in env plus one in another env: returns the in-env one;
  - name absent, roled target only in another env: throws `no Octopus target ... in Development`;
  - name absent, two roled targets in env: throws ambiguous role;
  - empty TargetName, unique roled target: returned;
  - empty environment id: throws.
- `Ensure-LinuxTarget` mock: a target in `Environments-2` gains `Environments-1` with one PUT; a target already in it makes no PUT.
- The script parses with 0 errors.
- `RideAudit.Server.Admission.Tests` 32/32 (the TEST-RIDE-038 source assertions now include the environment guard).
- Not run against a live Octopus server.

## Also in this commit

- HANDOFF: PR #26 merge time corrected from 22:33 to 22:32 CT (squash commit 2ee9c4d at 22:32:04 CT).

## Not claimed

No HV AGREE. No FR or AC marked satisfied.
