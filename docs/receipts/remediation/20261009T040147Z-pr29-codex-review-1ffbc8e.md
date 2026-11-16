# Remediation receipt: PR #29 fixes for Codex review 5465551807 on 1ffbc8e

- **When (UTC):** 20261009T040147Z
- **When (operator):** 2026-10-08 23:01:47 CT
- **Branch:** claude/eager-pascal-wsb5at (PR #29).
- **Operator:** Claude Code (Anthropic) cloud session.
- **No force-push. No rebase. No amend.**

Each finding was checked against the code before it was fixed.

| Codex finding | Verified | Change |
| --- | --- | --- |
| P2 `WindowsBleDiscoveryBus.cs:103`: when the publisher cannot add the service UUID, `Advertise` still reports success, but every scanner now drops the name-only packet | Yes. Since 18d1514 the scan handler requires the UUID, so a name-only advertise is never discovered and peers only time out. | `Advertise` now fails closed with `BT_DISABLED` when the UUID cannot be added. The outer catch no longer rewraps that exception. The Client test asserts both. `WindowsHardwareSeamTests` already accepts `BT_DISABLED` from `Advertise`. |
| P2 `IngestAnalysisTests.cs:459`: the DSAR export test seeds no other subject before the export | Yes. The second driver was registered after the export and had no data. | Another driver now enrolls, submits and ingests a different export before the export. The test asserts that no ZIP entry contains that driver's id, import id, import hash or submission id. |
| P2 `remote-stop.sh:53-54`: unconditional `pkill -f` bypasses `is_our_ngrok` | Yes | The sweep now lists `pgrep -f "ngrok http <backend> "` for the two backends and signals only the PIDs that pass `is_our_ngrok`. The README states this. The interim `:18080` agent was started with the same `--config` and `--log` (697e159), so it is still swept. Agents started by the systemd unit use `--log stdout` and are stopped by `systemctl --user stop`. |
| P1 candidate: UC-RIDE-021 is missing from the actor migration | Yes. The root cause: r1 and r2 covered only five use cases, but ten more name Admin, Auditor or Counsel (001, 002, 004, 013, 018, 019, 021, 026, 027, 031). | r3 adds a table with exact proposed actors and flow text for all ten. UC-RIDE-021 export needs `package-export`. |
| P1 candidate: no grantor-only issuance or revocation tests | Yes | TR-RIDE-SEC-004 AC 2 now rejects ids the grantor does not own and requires no state change on refusal. TEST-RIDE-058 item 5 adds the grantee, other-driver and foreign-id issuance cases and the grantee and other-driver revocation cases. |

## Evidence (this session)

- DSAR test passes. With the driver filter removed from `PrivacyDesk.Export` (local mutation, reverted), the test fails on `Assert.DoesNotContain`.
- ngrok stop, with fake agents under a temporary HOME:
  - stopped: our `:28080` agent and the interim `:18080` agent (both with this config and log);
  - still running: a `:28080` agent with another config, and a `:18080` agent with another log.
- `bash -n remote-stop.sh` passes.
- Tests: Chain 17/17, Client 111/111, Escrow 4/4, Protos 5/5, Seal 8/8, Server.Admission 32/32, Workflow 28/28.
- `RideAudit.Host.Windows` and `RideAudit.Host.Windows.Tests` build with `EnableWindowsTargeting` (0 warnings, 0 errors). They were not run, because they need Windows. The BLE change was not exercised on Windows hardware.
- The SDK pin in `global.json` was relaxed locally for these runs only and is not committed.

## Not claimed

No HV AGREE. No FR or AC marked satisfied. The case grant is still a candidate awaiting Payton AGREE.
