# PR #26 remedia (accuracy + HeadrestGeometry plumbing)

Written: 2026-10-07 12:41:18 CT (America/Chicago) / 20261007T174118Z UTC.
Author: Grok Bot on PAYTON-LEGION2.
PR: https://github.com/sharpninja/rideaudit/pull/26
Base head before: `759cea2`.
No merge.

## Fixed (this remedia)

1. Requirements-Mappings-Batch.yaml: deleted orphaned FR-RIDE-210 keys under FR-RIDE-209 (duplicate trIds/testIds/useCaseLocalIds). Strict YAML parse OK.
2. AcCoverageLedgerTests: hard-coded unique AC count 569 -> 550. Regenerated `docs/receipts/ac-coverage/20260928-ledger.md`.
3. FR-RIDE-011 guard test: `RideAudit_Ingest_path_has_no_HttpClient_and_no_lyft_com_private_api_scrape` names AC-RIDE-011-001/002. Concierge not restored.
4. PLAN scrub: PLAN-RIDEAUDIT-001-implementation.md and PLAN-RIDEAUDIT-001-SERVER.md mark killed Concierge rows (FR-004/012/204/206, UC-003/020, TR-INGEST-004, TEST-004/030) as KILLED.
5. IngestAnalysisTests Octopus env expects `Octopus-PAYTON-DESKTOP` (matches DistributionReceipts).
6. Restored UTF-8 BOM on AboutView.axaml and MainView.axaml only.
7. PlatformGrpcServices.Command indentation fixed (4 spaces).
8. ErrorCodes.PartnershipDisabled removed (unused).
9. NgrokDeploySecretsTests: removed duplicated LAB-OMARCHY assertion.
10-14. HeadrestGeometry: concurrent stderr drain; TrimEnd CR; +0.0 negative-zero keys; LocateMountRoot inside Fail try; added to RideAudit.slnx.
15. geometry-report.md: honest provenance (Python prior output; not regenerated; openscad not on PATH); ASCII symbol normalize.
16. ContractAuthority + shared proto Contract-Version headers bumped 0.2.0 -> 0.3.0 together; TestRide035 assert updated.
17. Azure Home.md storyboard links point under Storyboards/Mobile Dual-Phone/ and Storyboards/Review App/; restored Storyboards tree; removed misplaced root copies.
18. Additive-Operator-Capture mappings already new-IDs-only (0 overlap with Batch); no change.
19. UiLicense: Xamarin.AndroidX.Core.SplashScreen attribution added.
PrivacySlice.State: implemented -> partial (FR-077 without Camera2/H.264 SEI invent).

## Deferred / STOP (Payton)

- **STOP UC-RIDE-008**: Use-Cases-Batch.yaml still says "Subject or authorized agent requests access or deletion." Own-submissions only; do not invent RBAC/authorized-agent DSAR. Needs Payton wording decision.
- **FR-077**: left `partial` (SEI missing). Do not mark implemented.
- **Invoke-LiveOtsSmoke P1s** (varuint LEB128, calendar URL allowlist, fresh digest): deferred; not clearly in-scope for this remedia pass.
- **PrivacyDesk Locations / Codex retention P1**: prefer AGREED FR-078 (driver LyftAuditTools data is not third-party person data). No re-sweep invent. Provenance rename note only if Payton asks.
- No Camera2/H.264/SEI invent. No Concierge restore. No merge.

## Tests (Release)

- AcCoverageLedgerTests: 1 passed
- RideAudit_Ingest_path (FR-011) + Octopus_desktop_pointer: 2 passed
- NgrokDeploySecretsTests: 3 passed
- TestRide055AboutTests: 5 passed
- TestRide035ShellTests: 11 passed
- TestRide037CompanionAuthority: 3 passed
- RideAudit.HeadrestGeometry: build 0 warnings / 0 errors

## Do not

Merge PR. Restore Concierge. Invent roles / Camera2 SEI.