# PR #26 safe remedia + own-submissions auth (PAYTON-LEGION2)

Date: 20261007T161645Z. Operator host: PAYTON-LEGION2. Branch: cursor/capture-operator-reqs-b19f.
SPDX: GPL-2.0-only. Not a merge. Not reviewer AGREE.

## Auth (Payton: own-submissions only / no roles)

- CounselDesk.Build(actorId, ...): rejects non-own submissions (TenantIsolation), matching Verify.
- Counsel gRPC BuildMultiDriverBundle passes caller.DriverId.
- AnalysisService: kept self-service Authorize (already own-only); added negative tests.
- SetPartnership: STOP — PartnershipGate is a global singleton bool with no per-driver identity. Cannot invent admin roles (Payton forbid) and cannot invent per-driver partnership without a new SoT model. Left authenticated-only; needs Payton product decision (disable mutation RPC, make gate config-only, or redesign per-driver).

## Safe remedia addressed

1. AC ledger regenerated: 394 covered / 175 deferred / 0 missing / **569** total (was 424). PLAN r3.10 notes updated.
2. Mappings dedupe: Additive-Operator-Capture mappings keep new FR-065..074 only.
3. README: document UC approval restore after createBatch; leave UC-044 Draft.
4. Plan ownership: Unassigned FR count corrected to 10 (065-067,069-074); no invented phase homes.
5. Android visual AC text scoped off docs/ux/review-app to mobile wireframes/storyboards.
6. TEST-052 / FR-072 lab tree: STOP — FR ACs say whole toolchain/committed lab text; TEST ACs say "lab change". Ambiguous; not fix-forwarded.
7. DistributionReceipts: retargeted to committed 20260929T015822Z-octopus-payton-desktop.md (LAB-OMARCHY.md never existed on disk).
8. Orphaned linkType keys removed under FR-RIDE-010 / UC-RIDE-020.
9. Deploy/ngrok/octopus executable scripts retargeted to LAB-OMARCHY 192.168.1.182 (batch AGREE). Receipt path refs kept on payton-desktop artifact.
10. Azure wiki Storyboards/: moved Mobile Dual-Phone + Review App children under Storyboards/.
11. UiLicense attributions: Avalonia MIT, Avalonia.Fonts.Inter SIL OFL 1.1, Grpc.Net.Client Apache-2.0, Google.Protobuf BSD-3; About tests strengthened.
12. privacy.proto: cites FR-010/077/207; Contract-Version 0.3.0; reserved note for removed PlaceLegalHold.
13. PrivacyDesk: removed dead precise-geo Expired branch; comment Locations not swept; caseId discarded; RetentionPolicy.For(jurisdiction).
14. Precise = sample.Precise in PlatformGrpcServices.
15. BOM stripped from Shared.Ui views + mappings; PrivacySlice trailing newline; MainView.OnAbout redundant check simplified.

## Tests (LEGION2)

- Workflow own-submissions / Build_rejects / Analysis_rejects / privacy / verify: 5 passed
- Client About + AcCoverageLedger + shell: 17 passed
- NgrokDeploySecrets: 3 passed

## Not claimed

- Reviewer AGREE / "all agree"
- SetPartnership product decision
- TEST-052 whole-tree vs change-scope decision
- Semantic AC closure beyond name-or-defer ledger