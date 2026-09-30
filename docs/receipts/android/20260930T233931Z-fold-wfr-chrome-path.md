# Fold WF-R chrome path probe; local green baseline restored

Host: PAYTON-LEGION2. Device: `RFCW7078MVZ` / `SM_F936U` only. Branch `cursor/dual-phone-fold-moto-8aa2`.

Moto not touched.

## Commits

| SHA | Summary |
| --- | --- |
| `c4cc7c3` | 2-col sidebar rail + icons + single-row ScrollViewer nav |
| `0b51be2` | drop nav ScrollViewer; per-row sidebar; Run verification action |
| `6b98aa6` | restore vertical packing + compact StackPanel nav (this tip) |

## Path stop (wireframes remain SoT)

Approved WF-R SVGs are **desktop** chrome (~1320x868, left `#173E66` sidebar + right form). Forcing that 2-col layout on Fold (~904x2316) moved frontier slightly (WF-R-01 `style=agree` once) but **regressed local gates** that were ALL PASS at `8d0fc59` / `d84600f`.

Do **not** invent SVG defects. Phone viewport vs desktop wireframe chrome is the open fidelity tension. Stopped inventing further 2-col packing; restored vertical body packing that previously kept CLS/contrast/overflow green, while retaining compact single-row nav without WrapPanel/ScrollViewer.

## Device evidence (`0b51be2` APK on Fold)

Filter: `FullyQualifiedName~WireframeDeviceTests&DisplayName~WF-R`. Log: `artifacts/aiunit-device/wfr-0b51be2.log`. xUnit Failed 8 / Total 8.

Local regressions vs prior ALL PASS: WF-R-01 text-overflow; WF-R-05 layout Y-order; WF-R-06 overflow + PART_LineDownButton overlap; WF-R-07 overlapping-controls. Remaining frames frontier-only fail-closed. WF-R-01 frontier reported `style=agree` once amid controls/layout disagree.

SB-R / SB-05 / SB-06 **not re-run** after path restore (would re-install tip APK first). Prior SB suite still fail-closed on frontier at tip `8d0fc59` receipt.

## Headless

- `ReviewCaptureHostTests`: Passed 2 on restore tip

## MCP

- Plugin path `F:\GitHub\mcpserver-grok-plugin` exists; not in Cursor MCP catalog.
- `lib\mcp-status.ps1` requires PowerShell 7+. With `MCP_PLUGIN_HOST=grok` + `MCP_AGENT_NAME=GrokCode` + `MCP_WORKSPACE_PATH=F:\GitHub\rideaudit`, status=available agent=GrokCode.
- `workflow.todo.query` for `PLAN-PR24FOLD-001` succeeded (done=false). No FR/AC greens claimed; no MCP TODO mutation.

## Blockers / open

- Frontier agree still open (phone column != desktop SVG chrome).
- Tip after restore needs Fold APK install + WF-R/SB re-score before claiming local greens again.
- No FR or AC satisfied. `isSatisfied` stays false. PLAN-PR24FOLD-001 stays open.
