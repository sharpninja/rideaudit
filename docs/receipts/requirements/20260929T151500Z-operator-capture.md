# Operator requirement capture after plan approval

Date: 2026-09-29T15:15:00Z. Host: PAYTON-LEGION2. Workspace: `F:\GitHub\rideaudit`.
SPDX: GPL-2.0-only.

This receipt lists draft BDPv4 entities captured from operator direction after PLAN-RIDEAUDIT-001 approval. Every row is pending Payton AGREE. None of these rows close plan section 9 Class C boxes. None mark AC-RIDE-222-001, AC-UC-025-001, live OpenTimestamps confirmation, or Caddy edge TLS satisfied.

## Sources folded in

- Operator inventory in the capture request (signing, Octopus, Caddy, OTS, Fold 4, Motorola edge 2024, app font, RemoteControl, cradle, LEGION2 private worker).
- PR #22 (merged): lab self-signed Authenticode, `CN=RideAudit Lab Self-Signed`, unrelated store certs refused, no pfx committed, win-x64 signed and not Public Trust, linux-x64 unsigned, macOS not published. https://github.com/sharpninja/rideaudit/pull/22
- PR #23 (open): live public OpenTimestamps calendar submit, pending attestation, no Bitcoin txid. https://github.com/sharpninja/rideaudit/pull/23
- Plan section 11 Class C rows for edge TLS, live OTS confirmation, commercial signing, and macOS codesign. Section 9 boxes stay unchecked.

## MCP

Plugin: `F:\github\mcpserver-grok-plugin`. Agent: GrokCode. Wrapper: `lib\repl-invoke.ps1`.
`skills/*/scripts/invoke.ps1` is not present in that plugin.

The capture prompt named `MCP_WORKSPACE_PATH=F:\github\mcpserver`. RideAudit `MCP_TRUSTED.yaml` binds this product to `F:\GitHub\rideaudit`. Ingest used `F:\GitHub\rideaudit`. `getFr` returned `workspaceId: F:\GitHub\rideaudit`. RideAudit entities were not written into the mcpserver product workspace.

`workflow.requirements.createBatch` created 22 records (5 FR, 8 TR, 9 TEST), `errors: []`.
`workflow.requirements.updateBatch` updated 10 records (FR-RIDE-018, 041, 056, 057, 063, 064, 201, 222, TR-RIDE-CHAIN-002, TR-RIDE-VIEW-001), `errors: []`.
Mappings for the rows below stored with `errors` absent and `success: true` on the sampled create. `listMappings` for FR-RIDE-222 shows TR-RIDE-VIEW-001, TR-RIDE-VIEW-006, TEST-RIDE-028, TEST-RIDE-041.

New MCP use cases (approvalStatus Draft, not Approved):

| YAML localId | MCP useCaseId | FR |
| --- | --- | --- |
| UC-RIDE-034 | 161 | FR-RIDE-222 |
| UC-RIDE-035 | 162 | FR-RIDE-065 |
| UC-RIDE-036 | 163 | FR-RIDE-066 |
| UC-RIDE-037 | 164 | FR-RIDE-067 |
| UC-RIDE-038 | 165 | FR-RIDE-068 |
| UC-RIDE-039 | 166 | FR-RIDE-069 |

Updated MCP use case briefs only: 136 (UC-RIDE-009), 144 (UC-RIDE-017), 152 (UC-RIDE-025). A `GetAsync` of use case 152 before this capture showed no acceptance-criteria array on the MCP use case object. AC text for use cases is in the repo YAML. MCP briefs point at those AC ids.

## ID list

| Change | ID | Title | Approval |
| --- | --- | --- | --- |
| update | FR-RIDE-222 | Desktop portability fail-closed. Added lab CN, deferred commercial IV+eSigner or OV+cloud HSM for individual Payton Byrd, deferred macOS codesign. AC-RIDE-222-001 stays unsatisfied. | pending Payton AGREE |
| update | TR-RIDE-VIEW-001 | Notes only: macOS leg stays unsatisfied. | pending Payton AGREE |
| new | TR-RIDE-VIEW-006 | Lab Authenticode interim and deferred public trust | pending Payton AGREE |
| new | TEST-RIDE-041 | Lab Authenticode is not Public Trust | pending Payton AGREE |
| new | UC-RIDE-034 | Operator publishes a lab-signed Windows desktop build | pending Payton AGREE |
| update | FR-RIDE-063 | Notes only. Octopus to PAYTON-DESKTOP, no GHCR, already present. Names octopus-rideaudit receipt without satisfying ACs. | pending Payton AGREE |
| update | FR-RIDE-064 | Notes only. ngrok HTTPS is not Caddy edge TLS. | pending Payton AGREE |
| update | FR-RIDE-201 | Notes only. ngrok HTTPS and loopback HTTP do not satisfy TLS AC. | pending Payton AGREE |
| new | FR-RIDE-065 | Caddy edge TLS distinct from ngrok | pending Payton AGREE |
| new | TR-RIDE-EDGE-002 | Caddy edge TLS separate from the ngrok tunnel | pending Payton AGREE |
| new | TEST-RIDE-045 | Caddy TLS receipt excludes ngrok | pending Payton AGREE |
| new | UC-RIDE-035 | Operator proves Caddy edge TLS apart from ngrok | pending Payton AGREE |
| update | FR-RIDE-018 | Live public OTS calendar submit may be pending. Confirmation and txid stay Class C. | pending Payton AGREE |
| update | TR-RIDE-CHAIN-002 | Pending calendar submit is not txid or live_bitcoin_metadata. | pending Payton AGREE |
| update | UC-RIDE-009 | Added AC-UC-009-003 | pending Payton AGREE |
| new | TEST-RIDE-042 | Live OTS calendar submit without txid | pending Payton AGREE |
| update | FR-RIDE-056 | Fold 4 USB is primary device proof. Emulator-only does not pass. AC-UC-025-001 stays unsatisfied. | pending Payton AGREE |
| update | UC-RIDE-025 | Added AC-UC-025-002 | pending Payton AGREE |
| new | TR-RIDE-VIDEO-015 | Fold 4 USB primary device proof | pending Payton AGREE |
| new | TEST-RIDE-043 | Fold 4 USB primary, not emulator-only | pending Payton AGREE |
| update | FR-RIDE-041 | Motorola edge 2024 over wireless adb is the lab secondary. | pending Payton AGREE |
| update | UC-RIDE-017 | Added AC-UC-017-003 | pending Payton AGREE |
| new | TR-RIDE-VIDEO-016 | Motorola edge 2024 wireless adb secondary | pending Payton AGREE |
| new | TEST-RIDE-044 | Motorola edge 2024 wireless secondary | pending Payton AGREE |
| new | FR-RIDE-066 | Larger durable default Android UI font | pending Payton AGREE |
| new | TR-RIDE-VIDEO-013 | Application style default font | pending Payton AGREE |
| new | TEST-RIDE-046 | App style font larger than FontSize 20 | pending Payton AGREE |
| new | UC-RIDE-036 | Driver reads capture UI at the app default font | pending Payton AGREE |
| new | FR-RIDE-067 | Avalonia RemoteControl visual-tree debugging | pending Payton AGREE |
| new | TR-RIDE-VIDEO-014 | SharpNinja.Avalonia.RemoteControl debug attach | pending Payton AGREE |
| new | TEST-RIDE-047 | RemoteControl visual tree without ADB taps | pending Payton AGREE |
| new | UC-RIDE-037 | Operator inspects the Android visual tree with RemoteControl | pending Payton AGREE |
| new | FR-RIDE-068 | Fold 4 closed landscape forward camera tray | pending Payton AGREE |
| new | TR-RIDE-HW-001 | Fold 4 closed landscape tray geometry | pending Payton AGREE |
| new | TEST-RIDE-048 | Fold 4 closed landscape tray fit | pending Payton AGREE |
| new | UC-RIDE-038 | Operator seats a closed Fold 4 in the forward camera tray | pending Payton AGREE |
| new | FR-RIDE-069 | Cursor agents on PAYTON-LEGION2 private worker | pending Payton AGREE |
| new | TR-RIDE-LAB-001 | Private worker hosts Cursor cloud agents | pending Payton AGREE |
| new | TEST-RIDE-049 | LEGION2 private worker agent path | pending Payton AGREE |
| new | UC-RIDE-039 | Operator runs Cursor cloud agents on the LEGION2 private worker | pending Payton AGREE |
| update | FR-RIDE-057 | Notes only. macOS publish stays deferred. AC-RIDE-057-001 stays unsatisfied for macOS. | pending Payton AGREE |

## Not claimed

Plan section 9 is not closed. Public Trust is not closed. Caddy edge TLS is not closed. OpenTimestamps confirmation and txid are not closed. AC-UC-025-001 is not satisfied. AC-RIDE-222-001 is not satisfied. This capture is not a Payton AGREE.
