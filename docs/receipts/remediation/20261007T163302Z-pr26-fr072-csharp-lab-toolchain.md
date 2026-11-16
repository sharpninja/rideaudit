# PR #26 FR-072 / TEST-052: rewrite lab Python in C# (PAYTON-LEGION2)

Date: 20261007T163302Z. Operator host: PAYTON-LEGION2. Branch: cursor/capture-operator-reqs-b19f.
SPDX: GPL-2.0-only. Not a merge. Not reviewer AGREE.

## Scope clarification (Payton)

FR-072 / TEST-052 apply to **committed in-repo lab toolchain artifacts** (for example `artifacts/hardware/headrest-phone-mount` README invoking a verifier, and em/en dashes in that committed lab text). They do **not** constrain languages installed on the physical LAB-OMARCHY machine. Payton asked what the dev lab has to do with any FR/AC; answer: the FR/AC name "lab" for the in-repo lab tree, not the host.

## Done

1. Sole committed Python lab script `artifacts/hardware/headrest-phone-mount/verify-geometry.py` rewritten as C# `tools/RideAudit.HeadrestGeometry` (`Program.cs` + csproj, net10.0 Exe). Same OpenSCAD/STL geometry checks; entrypoint `dotnet run --project tools/RideAudit.HeadrestGeometry -- --root <mount-dir>`.
2. README / dimensions / ARTIFACT.yaml / verification docs updated to call the C# entrypoint; `python3` / `verify-geometry.py` removed from those paths; `.py` deleted.
3. Em/en dashes (U+2013 / U+2014) stripped to ASCII `-` in committed headrest-phone-mount lab text (AC-RIDE-072-004 still requires this).
4. Wiki disk mirrors (github + azure Functional-Requirements + Testing-Requirements) aligned: FR-072 clarifies in-repo toolchain; TEST-052 ACs scan `artifacts/hardware` (whole applicable lab tree), not invent broader scope.
5. `TestRide052LabConductTests` (2 facts) added under Admission.Tests: no `.py` / no python invokes / no dashes under `artifacts/hardware`; C# tool present; FR-072/TEST-052 wiki sections name PAYTON-DESKTOP + PAYTON-LEGION2 go-by-default and `artifacts/hardware`.
6. Tool project added to `RideAudit.sln`.

## Tests (LEGION2)

- `dotnet build tools/RideAudit.HeadrestGeometry` Release: 0 warnings / 0 errors
- `dotnet test` filter `TestRide052LabConductTests`: **2 passed**
- Full OpenSCAD geometry re-run not executed here (openscad not on PATH on LEGION2 this session); C# port is compile-verified and docs point at it.

## MCP / Additive sync still needed

- `docs/Project/Additive-Operator-Capture-20260929-Batch.yaml` still has TEST-052 "lab change" wording and LAB-OMARCHY in go-by-default text. Disk wiki updated to Payton intent; Additive MCP update not applied this turn (prefer AGREE on Additive wording / mcpserver-grok-plugin path). Note for follow-up.

## Left alone

- SetPartnership: still STOP (Payton has not answered).
- No RBAC / legal-hold / mask invent.
- No Camera2 / H.264 SEI / telematics.
- No merge.

## Not claimed

- Reviewer AGREE / semantic AC isSatisfied flips in MCP
- Additive YAML / MCP batch sync for TEST-052 / FR-072 host-name wording