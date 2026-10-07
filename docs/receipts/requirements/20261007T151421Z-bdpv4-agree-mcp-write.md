# BDPv4 AGREE written to MCP SoT

Written: 2026-10-07 10:14 CT (America/Chicago) / 20261007T151421Z UTC.
Machine: PAYTON-LEGION2
Workspace: F:\GitHub\RideAudit
Author: Grok Bot executor
Plugin: F:\github\mcpserver-grok-plugin (MCP_AGENT_NAME=GrokCode)

## Authorization

- Payton AGREED BDPv4 Candidates A+B.
- Unblock: priority **high** for FR-RIDE-077, FR-RIDE-078, TR-RIDE-VIDEO-007, TR-RIDE-PRIV-004.
- Accuracy: descriptions from `docs/receipts/requirements/20261007-bdpv4-candidates-for-agree.md` (Exact candidate sentences on FRs; Role sentence bodies on TRs). No invented AC/mask/RBAC/legal-hold.

## MCP creates

| Id | Method | requestId | success |
| --- | --- | --- | --- |
| FR-RIDE-077 | createFr | req-20261007T151357Z-40a1 | true |
| TR-RIDE-VIDEO-007 | createTr | req-20261007T151358Z-7d34 | true |
| FR-RIDE-078 | createFr | req-20261007T151359Z-7d84 | true |
| TR-RIDE-PRIV-004 | createTr | req-20261007T151400Z-9bf1 | true |
| map FR-077→TR-VIDEO-007 | createMapping | req-20261007T151401Z-44ac | true |
| map FR-078→TR-PRIV-004 | createMapping | req-20261007T151402Z-9a32 | true |

status: pending on all four. priority: high. notes include `approval: Payton AGREE 2026-10-07`. acceptanceCriteria: [] (none invented).

## Stored bodies (get after write)

### FR-RIDE-077 (getFr req-20261007T151403Z-bd2b)

- title: Functional rule: unmasked capture; accel + precise location in H.264 SEI per picture
- description: Precise location and other capture data stay unmasked; accelerometer and precise location are embedded in the H.264 stream as real-time per-picture SEI for certifiable legal data.
- status: pending | priority: high | area: RIDE
- notes: approval: Payton AGREE 2026-10-07. BDPv4 Candidate A. Replaces killed FR-RIDE-014/202, TR-RIDE-PRIV-002/SEC-002. Source: docs/receipts/requirements/20261007-bdpv4-candidates-for-agree.md. No mask/RBAC/legal-hold. No invented AC.
- acceptanceCriteria: []

### TR-RIDE-VIDEO-007 (getTr req-20261007T151404Z-fb3b)

- title: Technical: SEI NAL units carry real-time per-picture accelerometer and precise location matched to each picture
- description: SEI NAL units carry real-time per-picture accelerometer and precise location matched to each picture
- status: pending | priority: high | area: RIDE | subarea: VIDEO
- notes: approval: Payton AGREE 2026-10-07. BDPv4 Candidate A. Replaces killed FR-RIDE-014/202, TR-RIDE-PRIV-002/SEC-002. Source: docs/receipts/requirements/20261007-bdpv4-candidates-for-agree.md. No mask/RBAC/legal-hold. No invented AC.
- acceptanceCriteria: []

### FR-RIDE-078 (getFr req-20261007T151405Z-307c)

- title: Functional rule: driver-collected evidentiary retention is not third-party retention
- description: Retention does not treat the driver as a third party; do not apply California 30-day or 180-day location third-party deletion frames to driver-collected evidentiary data in their own vehicle.
- status: pending | priority: high | area: RIDE
- notes: approval: Payton AGREE 2026-10-07. BDPv4 Candidate B. Replaces killed FR-RIDE-208. Source: docs/receipts/requirements/20261007-bdpv4-candidates-for-agree.md. No third-party/legal-hold invent. No invented AC.
- acceptanceCriteria: []

### TR-RIDE-PRIV-004 (getTr req-20261007T151406Z-4c34)

- title: Technical: retention timers must not apply CA third-party 30-day / location 180-day deletion frames to driver-owned vehicle evidentiary capture
- description: retention timers must not apply CA third-party 30-day / location 180-day deletion frames to driver-owned vehicle evidentiary capture
- status: pending | priority: high | area: RIDE | subarea: PRIV
- notes: approval: Payton AGREE 2026-10-07. BDPv4 Candidate B. Replaces killed FR-RIDE-208. Source: docs/receipts/requirements/20261007-bdpv4-candidates-for-agree.md. No third-party/legal-hold invent. No invented AC.
- acceptanceCriteria: []

## Disk YAML synced

- `docs/Project/Functional-Requirements-Batch.yaml` — FR-RIDE-077 @ L1121; FR-RIDE-078 @ L1129
- `docs/Project/Technical-Requirements-Batch.yaml` — TR-RIDE-VIDEO-007 @ L831; TR-RIDE-PRIV-004 @ L840
- `docs/Project/Requirements-Mappings-Batch.yaml` — FR-077 map @ L325; FR-078 map @ L329

## Not done

- No Camera2 / H.264 / telematics / PrivacyDesk / About UI feature code.
- FR-RIDE-074 untouched.
- No commit.
