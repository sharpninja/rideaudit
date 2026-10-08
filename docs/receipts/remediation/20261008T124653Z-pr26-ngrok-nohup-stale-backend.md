# Remedia receipt: PR #26 restart a stale nohup ngrok when the backend changes

- **When (UTC):** 20261008T124653Z
- **When (operator):** 2026-10-08 07:46:53 CT
- **Branch:** cursor/capture-operator-reqs-b19f
- **Operator:** Claude Code (Anthropic) cloud session.
- **Authority:** Payton 2026-10-08 takeover of PR #26. Answers the Codex P2 on `deploy/omarchy/ngrok/remote-start.sh` (review of `c956f06`).
- **No force-push. No rebase. No amend.**

## Finding (verified)

On a host without a usable user-systemd session, `start_nohup` reused any live PID in `ngrok.pid` without checking which backend it served. During the retarget to `192.168.1.182:28080`, a fallback agent started for an older backend stayed attached, and the script could still report a public URL.

## Change

- `remote-start.sh`: the nohup path writes the backend it started for to `$STATE_DIR/ngrok.addr`. A live agent is reused only when that file matches `ADDR`. Otherwise the script prints `NGROK_STALE_BACKEND=restart`, stops the old PID (waiting up to 10 s, then `FAIL_CLOSED=stale_ngrok_still_running`), and starts a new agent. An agent left by the earlier script has no `ngrok.addr` file, so it is restarted once.
- `remote-stop.sh`: also removes `ngrok.addr`.

The systemd path is unchanged, because it already restarts the unit with the current `ADDR`.

## Evidence (this session)

- `bash -n` passes for both scripts.
- `start_nohup` was run in isolation with a fake ngrok (`sleep`) under a temporary HOME:
  - Fresh start gives `NGROK_MODE=nohup`.
  - The same backend gives `nohup-existing` with the same PID.
  - A new backend gives `NGROK_STALE_BACKEND=restart`, the old PID is stopped, and a new PID is recorded with the new address.
  - A legacy PID with no address file is restarted.
- `RideAudit.Server.Admission.Tests` 32/32 (includes the ngrok deploy tests).
- Not run against a live LAB-OMARCHY host.
