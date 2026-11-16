# Remediation receipt: PR #29 fix for Codex security review 5474291979 on ad08d1e

- **When (UTC):** 20261009T191158Z
- **When (operator):** 2026-10-09 14:11:58 CT
- **Branch:** claude/eager-pascal-wsb5at (PR #29).
- **Operator:** Claude Code (Anthropic) cloud session.
- **No force-push. No rebase. No amend.**

| Codex finding | Verified | Change |
| --- | --- | --- |
| P1 (High) `remote-stop.sh:111`: with no unit file, a failed `MainPID` query plus a failed stop is discarded | Yes. Reproduced with the ad08d1e script: no unit file, user bus down, stdout-logging agent running. It printed `NGROK_STOPPED=1` and exited 0 with the agent alive. | `stop_loaded_unit_without_file` records whether the query succeeded. If the query and the stop both fail (stop rc other than 0 or 5), it sets `SYSTEMD_FAIL=ngrok_systemd_unverified query_failed stop_rc=N`, and the stop reports `FAIL_CLOSED` and exits 1. |

## Evidence (this session, fakes only)

| Case | ad08d1e | Fixed |
| --- | --- | --- |
| no unit file, bus down, stdout agent running | `NGROK_STOPPED=1`, rc 0, agent alive | `FAIL_CLOSED=ngrok_systemd_unverified query_failed stop_rc=1`, rc 1. The agent is still alive and reported, not killed. |
| no unit file, bus down, nohup agent only | `NGROK_STOPPED=1` | `FAIL_CLOSED=ngrok_systemd_unverified ...`, nohup agent stopped (behavior change: fails closed) |
| all earlier systemd, loaded-unit-without-file, supervisor, custom-address, foreign-tunnel, legacy-agent and stale-PID cases | as receipted | unchanged |

## Why the unidentified agent is not killed

Its command line is `<ngrok> http <addr> --config <config> --log stdout --log-format logfmt`. Without a `MainPID`, a PID file or this tree's `--log` path, it cannot be told apart from another tunnel started on the same ngrok config. The default config is the user's general `~/.config/ngrok/ngrok.yml`. Killing on that match would break the rule that foreign tunnels are never signaled, so the stop fails closed and leaves the decision to the operator.

## Not claimed

No HV AGREE. No FR or AC is marked satisfied. The scripts were run only against fakes, not live ngrok or real systemd.
