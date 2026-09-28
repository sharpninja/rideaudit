# Post-planning Octopus + ngrok requirements capture

Date: 2026-09-28. Host: PAYTON-LEGION2. Workspace: `F:\GitHub\rideaudit`.
Branched from `origin/master` `858ce9d` (CODE-HV AGREE custody). SPDX: GPL-2.0-only.

This receipt records requirement ID allocation and MCP ingest. It is not an Octopus CD green, not an ngrok live probe, not GHCR, and not a Play Store claim.

## Operator deltas captured

1. No GHCR. Container build and deploy uses Octopus Deploy targeting PAYTON-DESKTOP. License exhaustion on the default Octopus container means provision a new Octopus container on PAYTON-DESKTOP.
2. Configure the RideAudit service to use ngrok. Fail closed if the tunnel is not live. Token via secret store or environment, never committed.

Omarchy loopback cutover remains existing deploy evidence (`docs/receipts/distribution/legion2-omarchy-20260928.md`). It is not a new FR. Live Play is not a new FR.

## Collision scan

Scanned `docs/Project/*.yaml` (excluding wiki). New FR/UC/TR/TEST IDs were unused before this batch. AC IDs `AC-RIDE-063-*`, `AC-RIDE-064-*`, `AC-UC-032-*`, `AC-UC-033-*`, `AC-RIDE-DEPLOY-*`, `AC-RIDE-EDGE-*`, `AC-TEST-038`..`040` appear only in the new additive file.

## IDs

| Kind | ID | Title |
| --- | --- | --- |
| FR | FR-RIDE-063 | Octopus Deploy CD to PAYTON-DESKTOP |
| FR | FR-RIDE-064 | ngrok ingress for RideAudit service |
| UC | UC-RIDE-032 | Operator releases RideAudit via Octopus to PAYTON-DESKTOP |
| UC | UC-RIDE-033 | Operator configures ngrok and probes admission health |
| TR | TR-RIDE-DEPLOY-001 | Octopus project and image build process |
| TR | TR-RIDE-DEPLOY-002 | Octopus agent and license fallback on PAYTON-DESKTOP |
| TR | TR-RIDE-EDGE-001 | ngrok tunnel config and host service wrapper |
| TEST | TEST-RIDE-038 | Octopus release receipt without GHCR |
| TEST | TEST-RIDE-039 | ngrok URL reaches admission health |
| TEST | TEST-RIDE-040 | Deploy secrets absent from git |

Mappings: `docs/Project/Additive-PostPlanning-Deploy-Ngrok-Mappings.yaml` (also embedded in the batch file).

## MCP ingest

Plugin: `F:\GitHub\mcpserver-grok-plugin`. Agent: GrokCode.
Workspace used for entities: `F:\GitHub\rideaudit` (signed marker + `MCP_TRUSTED.yaml`). Not `F:\github\mcpserver`.

`Invoke-FullBootstrap` succeeded. `workflow.requirements.createBatch` created 8 records (2 FR, 3 TR, 3 TEST), `errors: []`.
`client.UseCases.CreateAsync` created MCP useCaseId 159 (FR-RIDE-063) and 160 (FR-RIDE-064). YAML localIds remain UC-RIDE-032 and UC-RIDE-033.
`workflow.requirements.createMapping` stored:

- FR-RIDE-063 -> TR-RIDE-DEPLOY-001, TR-RIDE-DEPLOY-002, TEST-RIDE-038, TEST-RIDE-040
- FR-RIDE-064 -> TR-RIDE-EDGE-001, TEST-RIDE-039, TEST-RIDE-040

Repo YAML is still the reviewable source of truth. MCP numeric use-case IDs are runtime assignments.
