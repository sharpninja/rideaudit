# Remedia receipt: PR #29 fixes for Codex's review of the PR #26 squash (2ee9c4d)

- **When (UTC):** 20261009T034622Z
- **When (operator):** 2026-10-08 22:46:22 CT
- **Branch:** claude/eager-pascal-wsb5at (PR #29). Payton 2026-10-08: "Use PR29 for reviews."
- **Operator:** Claude Code (Anthropic) cloud session.
- **Source:** Codex review 5465478433 on `2ee9c4d`, posted in reply to the HV request on PR #26. The HV pair is `docs/reviews/hv-pairs/20261009T034145Z-pr26-squash-codex-hv.json`, status partial: no model, verdict or scores were reported.
- **No force-push. No rebase. No amend.**

Each finding was checked against the code before it was fixed.

| Codex finding | Verified | Change |
| --- | --- | --- |
| P1 `remote-start.sh:104`: the stale-PID guard accepts any `ngrok http` process | Yes. A reused PID held by another ngrok tunnel under the shared account would be signaled, and `remote-stop.sh` escalates to `kill -9`. | `is_our_ngrok` now also requires this script's own ` --config $CONFIG ` and ` --log $LOG_FILE ` in the live command line. `remote-stop.sh` defines the same `CONFIG` and `LOG_FILE` defaults. |
| P2 `Additive-PostPlanning-Deploy-Ngrok-Batch.yaml:34`: the Octopus receipt is attributed to LAB-OMARCHY | Yes. The AGREEd note (b7dfca1) said PAYTON-DESKTOP. A PR #26 find-and-replace changed it to LAB-OMARCHY, and the receipt itself records the PAYTON-DESKTOP host. | Restored `PAYTON-DESKTOP` in that note. |
| P2 `TestRide052LabConductTests.cs:77`: AC-RIDE-072-005 trait on a PAYTON-DESKTOP assertion | Yes. The AC text names LAB-OMARCHY, and the AC is explicitly deferred for that host conflict. | Removed the AC-RIDE-072-005 trait. The ledger is unchanged because the explicit deferral already wins. |
| P2 `IngestAnalysisTests.cs:440`: the TEST-RIDE-010 trait sits on a deletion-only test | Yes. TEST-RIDE-010 is "DSAR access export", and that test never calls `Privacy.Export`. | The deletion test keeps FR-RIDE-010 and AC-UC-008 traits plus a NOTE. A new test, `Dsar_access_export_returns_the_subjects_own_data_and_refuses_other_callers`, carries TEST-RIDE-010, AC-RIDE-010-001 and AC-TEST-010-001/002. It covers the driver's own export (manifest names the driver, import hash and submission, no sealed plaintext), another caller refused with `TENANT_ISOLATION`, and an empty subject returning `no-personal-imports`. |
| P2 `PLAN-RIDEAUDIT-001-implementation.md:337`: the plan still describes the retired bundle | Yes | The live TEST-RIDE-023 and FR-RIDE-037 rows say "Per-record provenance". P8 is retitled "Per-record counsel verification + analysis working copy" and its goal is rewritten. The counsel rows in both plans and the SERVER plan summary say per-record verification. |
| P2 `BluetoothRideAuditOnlyTests.cs:59`: the Windows BLE fallback scans unfiltered and labels every peer RideAudit | Yes. When `ServiceUuids.Add` throws, the watcher runs unfiltered and every named advertisement becomes `RideAuditBluetooth`. | Added `ApiBoundary.AdvertisesRideAuditService(serviceUuids)`. The Windows `Received` handler now drops any advertisement without the RideAudit service UUID, so the fallback fails closed. The test covers the predicate (match, foreign, empty, null) and asserts that the Windows handler applies it first. |

## Evidence (this session)

- ngrok, with a fake agent under a temporary HOME:
  - fresh start gives `nohup`;
  - the same backend gives `nohup-existing`;
  - a new backend restarts the agent;
  - a PID reused by another ngrok tunnel (different config and log) is discarded and that tunnel survives;
  - a PID reused by an unrelated `sleep` is discarded and the process survives.
- `bash -n` passes for both scripts.
- Tests: Chain 17/17, Client 111/111, Escrow 4/4, Protos 5/5, Seal 8/8, Server.Admission 32/32, Workflow 28/28. `RideAudit.Host.Windows.Tests` builds with `EnableWindowsTargeting` (0 warnings, 0 errors) but did not run, because it needs Windows. The BLE change was not exercised on Windows hardware.
- The SDK pin in `global.json` was relaxed locally for these runs only and is not committed.

## Not claimed

No HV AGREE. No FR or AC marked satisfied.
