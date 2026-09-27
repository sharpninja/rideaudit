# RideAudit Project requirements (MCP Server YAML entities)

**Author:** Sharp Ninja

BDPv4 batch files for Functional, Technical, Testing, Use-Case, and Mapping entities.

## Source of truth

- `docs/source/lyft-telematics-audit-requirements.md`
- Stack decision: `docs/architecture/stack.md` (Avalonia UI 12 clients; gRPC on .NET 10 containers)

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
   - `Additive-Bluetooth-Pairing-Batch.yaml`
   - `Additive-Avalonia-Grpc-Stack-Batch.yaml`
4. Apply `Requirements-Mappings-Batch.yaml` after FR/TR/TEST/UC records exist (use-case localIds map to numeric MCP IDs at runtime).
5. Verify counts: base 74 FRs (52 functional + 22 NFR-as-FR), plus additive FR-RIDE-053..055 (Bluetooth) and FR-RIDE-056..062 (Avalonia/gRPC stack), plus related TR/TEST/UC records and one mapping per FR.

## ID conventions

- FR: `FR-RIDE-001`..`FR-RIDE-052`, NFRs as `FR-RIDE-201`..`FR-RIDE-222`, additive `FR-RIDE-053`+
- TR: `TR-RIDE-<SUBAREA>-NNN` with SUBAREA in INGEST, STORE, ANAL, SEAL, CHAIN, ESCROW, PLAY, GPL, SERVER, VIDEO, VIEW, PRIV, SEC, PERF
- TEST: `TEST-RIDE-NNN`
- Use cases: local `UC-RIDE-NNN` (MCP assigns numeric IDs at ingest)

## Additive batches

| Batch | FR | TR | TEST | UC |
| --- | --- | --- | --- | --- |
| Bluetooth pairing | FR-RIDE-053..055 | TR-RIDE-VIDEO-010..011 | TEST-RIDE-034 | UC-RIDE-022..024 |
| Avalonia/gRPC stack | FR-RIDE-056..062 | TR-RIDE-VIDEO-012, VIEW-005, GPL-004..005, SERVER-008..010 | TEST-RIDE-035..037 | UC-RIDE-025..031 |

## Notes

- No em dashes in YAML text.
- Do not invent Lyft APIs; Unverified caveats from the source doc are preserved in FR notes where relevant.
- Seal-at-collect, blockchain receipts, escrow, Play Integrity, GPL-2.0, public multi-driver server, dual-phone video composite, and desktop court viewer are covered.
- Stack: Avalonia UI 12 for Android capture and desktop court viewer; backend gRPC on .NET 10 containers; protos GPL-2.0; OpenAPI companion non-authoritative. See `docs/architecture/stack.md`.