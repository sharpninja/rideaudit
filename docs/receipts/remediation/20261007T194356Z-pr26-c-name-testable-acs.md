# Remedia receipt: PR #26 step C, name the 12 testable-now ACs

- **When (UTC):** 20261007T194356Z
- **When (operator):** 2026-10-07 14:43:56 CT
- **Branch:** cursor/capture-operator-reqs-b19f
- **Base HEAD before:** 4278c166ca2f4cc3bd4b9474796e5ddfc7bb1973 (step B)
- **Operator:** Grok Bot remedia on PAYTON-LEGION2 (pwsh 7)
- **Authority:** Payton approved step C 2026-10-07.
- **No merge. No force-push. No amend.**

## Rule applied

A trait goes on an existing test only if that test already asserts the AC text. Otherwise a focused test was written. An AC that cannot be tested honestly stays deferred with a specific reason. Each newly named AC's line was removed from `explicit-deferrals.txt`.

## Per AC

| AC | How named | Ledger after | Note |
| --- | --- | --- | --- |
| AC-RIDE-SEC-001-001 "TLS 1.2+ required." | Trait added to existing `TestRide029Security.Tls_policy_vault_and_log_redaction_hold` (AdmissionTests.cs). It already asserts `EnsureTls12OrHigher` throws for TLS 1.1 and allows TLS 1.2 and 1.3. | deferred (keyword "TLS 1.2": "Omarchy loopback is not an edge TLS receipt.") | Named. Still deferred by the existing keyword rule, as AC-RIDE-201-001 is. |
| AC-RIDE-030-003 GPL notices on scad, README, BOM, ARTIFACT; CAD is not a road release | New `HeadrestGplNoticeTests` (Server.Admission.Tests): GPL-2.0 SPDX or license lines in all four files, GNU GPL text in scad and README, "not a road release" in README, "Not an on-vehicle" in ARTIFACT.yaml, no MIT or Apache SPDX. | deferred (keyword "road release": "hardware mount road-fit is operator checklist work.") | Named. The AC's own text says "not a road release", which trips the existing keyword rule. |
| AC-RIDE-053-003 / AC-UC-022-001 RideAudit Bluetooth only, no Lyft Bluetooth API | New `BluetoothRideAuditOnlyTests` (Client.Tests): a peer advertised under another service is not discovered and pairing fails `BT_DISCOVERY_FAILED`; the same peer on `ApiBoundary.RideAuditBluetooth` pairs with that transport; `src/RideAudit.Bt`, `AndroidDiscoveryBus.cs`, `WindowsBleDiscoveryBus.cs` contain no "lyft" (obj and bin excluded) and both radio adapters use `ApiBoundary.RideAuditBluetoothService`. | covered / covered | |
| AC-RIDE-063-004, AC-RIDE-DEPLOY-002-003, AC-TEST-038-003, AC-UC-032-002 compose cutover is prior interim, not the Octopus CD green | New `NgrokDeploySecretsTests.Omarchy_compose_cutover_is_prior_interim_and_not_the_octopus_cd_green`: the cutover receipt `legion2-omarchy-20260928.md` says "compose up on loopback only" and "Still not a CD green" and never names `octopus-rideaudit`; `deploy/omarchy/README.md` and `Confirm-Cutover.ps1` call it interim and not a CD green; the Octopus receipt TEST-RIDE-038 scores is compose project `rideaudit-octopus` (not `rideaudit-omarchy`), excludes the interim loopback, and never cites the cutover. | 063-004 deferred, DEPLOY-002-003 covered, TEST-038-003 deferred, UC-032-002 deferred | 063-004, TEST-038-003 and UC-032-002 contain "CD green" and stay deferred by the existing keyword rule ("lab loopback is not production CD."). DEPLOY-002-003 says "deployment green" and is covered. |
| AC-UC-009-003 public OTS submit pending only; no confirmation, txid, or admission until upgrade | New `LiveChainClientTests.Public_ots_submit_is_pending_only_and_does_not_confirm_until_upgrade` (hermetic handler, no network): `PublicOtsCalendarClient` returns pending with no txid, chain id, block height, or write time; `BtcOtsAnchor` is not confirmed, custody is Quarantined, `ChainUnconfirmed`, no tx reference; only an upgraded proof (documented fixture, `fixture:` reference, `LiveBitcoinMetadata` false) confirms. | deferred (keyword "txid": "public OTS remains pending-labeled; no invented txid.") | Named. Still deferred by the existing keyword rule. |
| AC-UC-021-002 Unverified caveats remain visible where applicable | New `PortableAuditCaveatTests` (Workflow.Tests): an import with an unknown file is `imported-with-gap` with the API gap notice; an import with no data dictionary is `unverified-package`; the portable audit ZIP (FR-RIDE-207, which UC-RIDE-021 realizes) keeps both statuses in `provenance.csv` and `manifest.json`. | covered | Interpretation: for UC-RIDE-021 the applicable caveats are the import gap and unverified statuses carried into the portable export. |
| AC-UC-041-001 named or deferred, deferred wins, totals do not close ACs | New `AcCoverageLedgerTests.Ledger_names_or_defers_each_ac_and_deferred_wins_without_closing_acs`: every ledger row is covered or deferred; every explicit deferral stays deferred with its own reason even when a test names it; AC-RIDE-201-001 is named in AdmissionTests and is still deferred; every covered row reads "Named in executable test source. Not semantic closure." Row building moved into `BuildLedgerRows` and is shared with the ledger writer. Behavior is unchanged. | covered | |
| AC-UC-042-001 lab step receipt, documented path, no Python or dashes, approval except on LAB-OMARCHY and PAYTON-LEGION2 | **Not named. Left deferred with a specific reason.** | deferred | `TestRide052LabConductTests` asserts receipt references, no dashes, no Python, the C# geometry entrypoint, and that the FR-RIDE-072 and TEST-RIDE-052 wiki sections name **PAYTON-DESKTOP** and PAYTON-LEGION2. This AC names **LAB-OMARCHY** and PAYTON-LEGION2. TR-RIDE-LAB-004 and both FR-RIDE-072 wikis say PAYTON-DESKTOP. AC-RIDE-072-005 in YAML says LAB-OMARCHY. No test asserts the approval rule itself. Tagging would misstate what the test checks, and writing a LAB-OMARCHY test would pick one side of a requirement conflict. That needs Payton's decision. |

## Keyword rule note (reported, not changed)

Six of the twelve (SEC-001-001, 030-003, 063-004, TEST-038-003, UC-032-002, UC-009-003) are now named in tests, but `ClassifyDeferred` still defers them because their own text contains a live-integration keyword ("TLS 1.2", "road release", "CD green", "txid"). For 030-003, 063-004, TEST-038-003 and UC-032-002 the keyword appears in a negative statement ("not a road release", "not the Octopus CD green"). An id-level exemption would change the approved "deferred wins" ledger rule (AC-UC-041-001 / FR-RIDE-071). It is left for Payton. No reason was invented: each of the six carries the existing keyword reason.

## Files

- `tests/RideAudit.Server.Admission.Tests/AdmissionTests.cs` (trait)
- `tests/RideAudit.Server.Admission.Tests/HeadrestGplNoticeTests.cs` (new)
- `tests/RideAudit.Server.Admission.Tests/NgrokDeploySecretsTests.cs` (new fact)
- `tests/RideAudit.Client.Tests/BluetoothRideAuditOnlyTests.cs` (new)
- `tests/RideAudit.Client.Tests/AcCoverageLedgerTests.cs` (new fact, shared row builder)
- `tests/RideAudit.Chain.Tests/LiveChainClientTests.cs` (new fact)
- `tests/RideAudit.Workflow.Tests/PortableAuditCaveatTests.cs` (new)
- `docs/receipts/ac-coverage/explicit-deferrals.txt`: 11 lines removed; AC-UC-042-001 reason replaced with the specific reason above
- `docs/receipts/ac-coverage/20260928-ledger.md`: regenerated (exactly these 12 rows changed)
- `docs/plans/PLAN-RIDEAUDIT-001-implementation.md` lines 8 and 15: 549 (392 covered / 157 deferred). README carries no ledger counts.

## Tests (Release, PAYTON-LEGION2)

- Client.Tests `AcCoverageLedgerTests|BluetoothRideAuditOnlyTests`: Passed 5, Failed 0.
- Server.Admission.Tests `HeadrestGplNoticeTests|NgrokDeploySecretsTests|TestRide029Security`: Passed 6, Failed 0.
- Chain.Tests `Public_ots_submit_is_pending_only`: Passed 1, Failed 0.
- Workflow.Tests `PortableAuditCaveatTests`: Passed 1, Failed 0.

Ledger after: total 549, covered 392, deferred 157, missing 0.
