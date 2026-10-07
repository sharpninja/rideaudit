# Remedia receipt: PR #26 review findings (post-29c3af3)

- **When (UTC):** 20261007T182104Z
- **When (operator):** 2026-10-07 13:21 CT
- **Branch:** cursor/capture-operator-reqs-b19f
- **Base HEAD before:** 29c3af3b27772069a16096e99b3f9cb3f124cbb9
- **Operator:** AnnoyingOrange / Grok Bot remedia on PAYTON-LEGION2
- **No merge.**

## Fixed

1. **TEST-052 / geometry-report.md (P1)**
   - Replaced `verify-geometry.py` mention with `pre-C# geometry verifier (removed in 5c2178f)`.
   - Diameter nits: `crest ~ 8.0 mm` -> `crest dia. 8.0 mm`; `head ~ 22 mm` -> `head dia. 22 mm`.
2. **Companion contract docs aligned to ContractAuthority 0.3.0**
   - artifacts/server-api/openapi.yaml, README.md, ARTIFACT.yaml
   - src/RideAudit.Client.Contracts/SWAP.md
   - error-codes.md: removed PARTNERSHIP_DISABLED and LEGAL_HOLD_ACTIVE (product removed). No new codes invented.
3. **Duplicate mappings removed from Additive-Operator-Capture-20260929-Batch.yaml**
   - Entire `mappings:` block removed (FR-065..074 + FR-222/018/056/041/063).
   - Canonical homes kept: Additive-Operator-Capture-20260929-Mappings.yaml (065..074); Requirements-Mappings-Batch.yaml / Additive-PostPlanning-Deploy-Ngrok-Mappings.yaml (existing FRs).
4. **Stale counts**
   - PLAN-RIDEAUDIT-001-implementation.md: 569 -> 550 (386 covered / 164 deferred) on r3.10 notes.
   - docs/Project/README.md Step 6 + UML line: UC-RIDE-001..031 contiguous claim corrected to 29 live UCs (003/020 removed); base FR count corrected to 67 in Functional-Requirements-Batch (verified on disk).
5. **Wiki storyboard wireframe links**
   - Copied existing SVGs from docs/ux/assets/wireframes/*.svg into docs/Project/wiki/azure/assets/wireframes/ (no invent).
   - Mobile Dual-Phone pages: `../assets/` -> `../../assets/`. Review App already used `../../assets/`. Zero broken relative targets after fix.
6. **IngestAnalysisTests.cs**: cleared 2 whitespace-only lines (L104, L199).

## Deliberately not changed

- Invoke-LiveOtsSmoke.ps1 (OTS smoke findings deferred).
- No new product requirements / broad new tests for the 47 deferred ACs.
- No Camera2 / H.264 SEI / telematics invent.
- FR-RIDE-011 / FR-RIDE-078 retention product code unchanged.
- Optional `third_party_telematics` clarifying note: skipped (no single clear FR-078/provenance home found without inventing).

## Tests run (PAYTON-LEGION2)

```
dotnet test tests/RideAudit.Server.Admission.Tests/RideAudit.Server.Admission.Tests.csproj --filter "FullyQualifiedName~TestRide052LabConductTests"
# Passed: 2

dotnet test tests/RideAudit.Client.Tests/RideAudit.Client.Tests.csproj --filter "FullyQualifiedName~AcCoverageLedgerTests"
# Passed: 1

dotnet test tests/RideAudit.Client.Tests/RideAudit.Client.Tests.csproj --filter "FullyQualifiedName~TestRide035|FullyQualifiedName~ContractVersion|FullyQualifiedName~0.3.0"
# Passed: 11

ConvertFrom-Yaml smoke: Additive-Operator-Capture Batch + Mappings, Requirements-Mappings, Use-Cases-Batch: OK
```

## GitHub thread replies

Reply after push on: 4210180559, 4210293039, 4210173309, 4210173315, 4210071728, 4210293050, 4210293043.