# Remedia receipt: PR #26 Codex findings on 173c439 (Octopus role, killed traits, FR-077 perms, UC-021)

- **When (UTC):** 20261007T190146Z
- **When (operator):** 2026-10-07 14:01:46 CT
- **Branch:** cursor/capture-operator-reqs-b19f
- **Base HEAD before:** 173c439b013e880671b17b9969180eeae9ec1cbc
- **Operator:** Grok Bot remedia (AnnoyingOrange) on PAYTON-LEGION2
- **No merge.**
- **OTS:** Invoke-LiveOtsSmoke.ps1 left alone (deferred reply only).

## Findings addressed

1. P1 discussion_r4210678269 - Octopus TargetRole shared `rideaudit-host` could select retained PAYTON target.
2. P2 discussion_r4210678257 - killed FR/AC IDs still published as test Traits.
3. P2 discussion_r4210678238 - artifacts/android/permissions.md still described FR-RIDE-202 masking.
4. P2 discussion_r4210678209 - UC-RIDE-021 still required legal-hold confirmation.
5. P2 discussion_r4210678195 - OTS second-digest POST: reply only, no code change.

## Fixes

### 1. Octopus unique LAB role + exact TargetName preference
- `deploy/octopus/Invoke-RideAuditOctopusRelease.ps1`
  - default `TargetRole` = `rideaudit-host-lab-omarchy`
  - `Wait-RoleTarget` prefers exact `TargetName`; refuses ambiguous multi-target role matches when TargetName is set; unique-role fallback only when exactly one role match
  - `Ensure-LinuxTarget` rewrites existing LAB-OMARCHY-LINUX machine Roles to the unique role
- `deploy/octopus/new-instance/compose.yaml` Tentacle `TargetRole` updated
- `deploy/octopus/README.md` role docs updated
- `NgrokDeploySecretsTests` asserts unique role default, ambiguous refuse text, and tentacle compose TargetRole

### 2. Killed traits removed from IngestAnalysisTests
- Removed `FR-RIDE-206` / `AC-RIDE-206-*` from TEST-002
- Removed `AC-RIDE-PRIV-003-002` from DSAR deletion test
- Removed `FR-RIDE-203-killed-invent` and `AC-RIDE-203-001` from access-log test; NOTE retained for history

### 3. Android permissions guidance
- `artifacts/android/permissions.md` location row now cites FR-RIDE-077 precise unmasked location (no FR-RIDE-202 mask). No Camera2 / H.264 invent in that doc.

### 4. UC-RIDE-021 legal-hold step
- `docs/Project/Use-Cases-Batch.yaml` step 4 rewritten to own-submissions privacy deletion without legal-hold
- No generated Use-Cases wiki page exists under `docs/Project/wiki` (azure/github exports have no Use-Cases.md / UC-RIDE-021 page), so no wiki page edit

### 5. OTS
- No change to `deploy/chain/Invoke-LiveOtsSmoke.ps1`

## Tests (Release, PAYTON-LEGION2)

`dotnet test tests/RideAudit.Server.Admission.Tests -c Release --filter FullyQualifiedName~NgrokDeploySecretsTests`
Passed: 3, Failed: 0

`dotnet test tests/RideAudit.Workflow.Tests -c Release --filter FullyQualifiedName~TestRide010012029031032|FullyQualifiedName~TestRide001Through006And011And030`
Passed: 12, Failed: 0

Combined: 15/15.

## Deliberately not changed
- Invoke-LiveOtsSmoke.ps1
- Camera2 / H.264 SEI / telematics invent
- Concierge / RBAC / legal-hold invent
- wiki azure/github .mcp-requirements-manifest.json dirty locals (unrelated)
- No merge / no master rebase