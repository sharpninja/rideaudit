# RideAudit Project requirements (MCP Server YAML entities)

BDPv4 batch files for Functional, Technical, Testing, Use-Case, and Mapping entities.

## Source of truth

- `docs/source/lyft-telematics-audit-requirements.md`

## MCP status

`MCP_UNTRUSTED` is set at the repo root (`MCP_UNTRUSTED.yaml`). Do not treat the MCP workspace as registered until McpServer health recovers. Re-run workspace init via `mcpserver-grok-plugin` when healthy.

## How to ingest (when MCP is healthy)

1. Confirm McpServer health and clear or update `MCP_UNTRUSTED` per plugin guidance.
2. Ensure the RideAudit workspace is registered (workspacePath `F:\GitHub\rideaudit`).
3. Use `mcpserver-grok-plugin` `createBatch` (or equivalent batch create) against each file under `docs/Project/`:
   - `Functional-Requirements-Batch.yaml`
   - `Technical-Requirements-Batch.yaml`
   - `Testing-Requirements-Batch.yaml`
   - `Use-Cases-Batch.yaml`
4. Apply `Requirements-Mappings-Batch.yaml` after FR/TR/TEST/UC records exist (use-case localIds map to numeric MCP IDs at runtime).
5. Verify counts: 74 FRs (52 functional + 22 NFR-as-FR), plus TR/TEST/UC records and one mapping per FR.

## ID conventions

- FR: `FR-RIDE-001`..`FR-RIDE-052`, NFRs as `FR-RIDE-201`..`FR-RIDE-222`
- TR: `TR-RIDE-<SUBAREA>-NNN` with SUBAREA in INGEST, STORE, ANAL, SEAL, CHAIN, ESCROW, PLAY, GPL, SERVER, VIDEO, VIEW, PRIV, SEC, PERF
- TEST: `TEST-RIDE-NNN`
- Use cases: local `UC-RIDE-NNN` (MCP assigns numeric IDs at ingest)

## Notes

- No em dashes in YAML text.
- Do not invent Lyft APIs; Unverified caveats from the source doc are preserved in FR notes where relevant.
- Seal-at-collect, blockchain receipts, escrow, Play Integrity, GPL-2.0, public multi-driver server, dual-phone video composite, and desktop court viewer are covered.
