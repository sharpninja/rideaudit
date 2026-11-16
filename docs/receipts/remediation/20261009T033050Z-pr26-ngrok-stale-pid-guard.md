# Remedia receipt: PR #26 guard ngrok stale-PID handling against PID reuse

- **When (UTC):** 20261009T033050Z
- **When (operator):** 2026-10-08 22:30:50 CT
- **Branch:** cursor/capture-operator-reqs-b19f
- **Operator:** Claude Code (Anthropic) cloud session.
- **Authority:** Payton 2026-10-08 takeover of PR #26. Answers the Codex P1 on `deploy/omarchy/ngrok/remote-start.sh:108` (review of `dfb6356`). The bug was introduced by 66b7891.
- **No force-push. No rebase. No amend.**

## Finding (verified)

After a crash or reboot, `ngrok.pid` can name a PID that now belongs to another process. `kill -0` only shows that some process exists. With a missing or mismatched `ngrok.addr`, 66b7891 then killed that unrelated process. `remote-stop.sh` had the same exposure from before this PR: it killed the recorded PID without checking it, escalating to `kill -9`.

## Change

- Both scripts now have `is_our_ngrok PID`, which reads `/proc/PID/cmdline` and accepts the PID only when the process is an `ngrok http ...` tunnel.
- `remote-start.sh`: a live PID that is not an ngrok tunnel is never signaled. The script prints `NGROK_STALE_PID=discarded`, removes `ngrok.pid` and `ngrok.addr`, and starts a fresh agent.
- `remote-stop.sh`: signals the recorded PID only when it passes the same check. It still removes the PID file. The existing `pkill -f 'ngrok http ...'` lines are unchanged.

## Evidence (this session)

- `bash -n` passes for both scripts, and the guard function is identical in both.
- `start_nohup` was run with a fake `ngrok` script under a temporary HOME:
  - Fresh start gives `nohup`.
  - The same backend gives `nohup-existing`.
  - A changed backend gives `NGROK_STALE_BACKEND=restart`.
  - A PID file pointing at an unrelated `sleep` process gives `NGROK_STALE_PID=discarded`, a new agent starts, and the unrelated process is still alive afterwards.
- `RideAudit.Server.Admission.Tests` 32/32.
- Not run on the live LAB-OMARCHY host.
