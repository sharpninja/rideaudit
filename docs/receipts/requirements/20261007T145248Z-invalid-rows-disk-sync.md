# RideAudit requirements disk sync - invalid rows

Written: 2026-10-07 09:52 CT (America/Chicago).
Machine: PAYTON-LEGION2
Workspace: F:\GitHub\rideaudit branch cursor/capture-operator-reqs-b19f

## Scope
Requirements-only. No application/feature code edited in this pass.
MCP SoT already returned not_found for the kill-list IDs (queried via mcpserver-grok-plugin repl-invoke).
This pass removes stale projections from docs/Project batch YAML so a future ingest cannot reintroduce them.
Do not mark requirements done. No replacement BDPv4 rows written.

## Kill list (HANDOFF already-authorized)
- FR-RIDE-014
- FR-RIDE-202
- FR-RIDE-203
- FR-RIDE-208
- TR-RIDE-PRIV-002
- TR-RIDE-SEC-002
- TR-RIDE-SEC-003
- TEST-RIDE-012
- TEST-RIDE-032

## Before (id: hits in docs/Project/*.yaml)
- FR-RIDE-014: F:\GitHub\rideaudit\docs\Project\Functional-Requirements-Batch.yaml:214; F:\GitHub\rideaudit\docs\Project\Requirements-Mappings-Batch.yaml:55; F:\GitHub\rideaudit\docs\Project\Use-Cases-Batch.yaml:581
- FR-RIDE-202: F:\GitHub\rideaudit\docs\Project\Requirements-Mappings-Batch.yaml:215; F:\GitHub\rideaudit\docs\Project\Use-Cases-Batch.yaml:191; F:\GitHub\rideaudit\docs\Project\Use-Cases-Batch.yaml:583
- FR-RIDE-203: F:\GitHub\rideaudit\docs\Project\Functional-Requirements-Batch.yaml:871; F:\GitHub\rideaudit\docs\Project\Requirements-Mappings-Batch.yaml:219; F:\GitHub\rideaudit\docs\Project\Use-Cases-Batch.yaml:193
- FR-RIDE-208: F:\GitHub\rideaudit\docs\Project\Functional-Requirements-Batch.yaml:940; F:\GitHub\rideaudit\docs\Project\Requirements-Mappings-Batch.yaml:239; F:\GitHub\rideaudit\docs\Project\Use-Cases-Batch.yaml:195
- TR-RIDE-PRIV-002: F:\GitHub\rideaudit\docs\Project\Technical-Requirements-Batch.yaml:759
- TR-RIDE-SEC-002: F:\GitHub\rideaudit\docs\Project\Technical-Requirements-Batch.yaml:807
- TR-RIDE-SEC-003: F:\GitHub\rideaudit\docs\Project\Technical-Requirements-Batch.yaml:823
- TEST-RIDE-012: F:\GitHub\rideaudit\docs\Project\Testing-Requirements-Batch.yaml:169
- TEST-RIDE-032: F:\GitHub\rideaudit\docs\Project\Testing-Requirements-Batch.yaml:472

## Edits
### F:\GitHub\rideaudit\docs\Project\Functional-Requirements-Batch.yaml
- removed FR-RIDE-014 lines 213-227
- removed FR-RIDE-203 lines 870-881
- removed FR-RIDE-208 lines 939-953
### F:\GitHub\rideaudit\docs\Project\Technical-Requirements-Batch.yaml
- removed TR-RIDE-PRIV-002 lines 758-773
- removed TR-RIDE-SEC-002 lines 806-821
- removed TR-RIDE-SEC-003 lines 822-834
### F:\GitHub\rideaudit\docs\Project\Testing-Requirements-Batch.yaml
- removed TEST-RIDE-012 lines 168-182
- removed TEST-RIDE-032 lines 471-485
### F:\GitHub\rideaudit\docs\Project\Requirements-Mappings-Batch.yaml
- removed FR-RIDE-014 mapping lines 55-58
- removed FR-RIDE-202 mapping lines 215-218
- removed FR-RIDE-203 mapping lines 219-222
- removed FR-RIDE-208 mapping lines 239-242
### F:\GitHub\rideaudit\docs\Project\Use-Cases-Batch.yaml
- stripped frId list items: 5

## After (id: hits in docs/Project/*.yaml)
- FR-RIDE-014: ABSENT
- FR-RIDE-202: ABSENT
- FR-RIDE-203: ABSENT
- FR-RIDE-208: ABSENT
- TR-RIDE-PRIV-002: ABSENT
- TR-RIDE-SEC-002: ABSENT
- TR-RIDE-SEC-003: ABSENT
- TEST-RIDE-012: ABSENT
- TEST-RIDE-032: ABSENT

## Not done here
- Wiki / plans / UC markdown still may cite kill-list IDs (stale). Prefer MCP generateDocument wiki next.
- PrivacyDesk GeoMask/MaskedLocation type names and retention timers: app code; freeze holds. Observation only.
- No commit.

## MCP observation (this session)
- FR-RIDE-014/202/203/208: not_found
- TR-RIDE-PRIV-002/SEC-002/SEC-003: not_found
- TEST-RIDE-012/032: not_found
- FR-RIDE-074: present, status pending, notes say Draft until Payton AGREE


## Wiki surgical strip retry (attrib -R)
MCP generateDocument wiki still blocked on wiki.yaml Storyboards duplicate. Manual section strip attempted.

