# Remedia receipt: PR #26 wiki deploy target LAB-OMARCHY (Codex P2)

- **When (UTC):** 20261007T183526Z
- **When (operator):** 2026-10-07 13:35:26 CT
- **Branch:** cursor/capture-operator-reqs-b19f
- **Base HEAD before:** c8b981ef99227522acf6323e2021270f080795fa
- **Operator:** Grok Bot remedia on PAYTON-LEGION2
- **Thread:** https://github.com/sharpninja/rideaudit/pull/26#discussion_r4210513967
- **No merge.**

## Finding

Authoritative batch `Additive-PostPlanning-Deploy-Ngrok-Batch.yaml` targets **LAB-OMARCHY** / `192.168.1.182:28080`, but azure/ and github/ wiki exports still named **PAYTON-DESKTOP** / `192.168.0.149` for FR-RIDE-063, FR-RIDE-064, TR-RIDE-DEPLOY-002, and TEST-RIDE-038.

## Fix

Section-scoped retarget only (no blanket replace; no wiki regen — `docs/wiki.yaml` is export config only; batch YAML is SoT):

- `docs/Project/wiki/azure/Functional-Requirements.md` — FR-063, FR-064
- `docs/Project/wiki/azure/Technical-Requirements.md` — TR-DEPLOY-002
- `docs/Project/wiki/azure/Testing-Requirements.md` — TEST-038
- `docs/Project/wiki/github/` — same three files / same sections

Replacements inside those sections only (matched batch wording):

- PAYTON-DESKTOP -> LAB-OMARCHY
- PAYTON-OMARCHY -> LAB-OMARCHY (compose-cutover ACs)
- 192.168.0.149 -> 192.168.1.182 (FR-064 tunnel `:28080`)

Left untouched on purpose:

- FR-RIDE-072 / TEST-RIDE-052 go-by-default hosts (PAYTON-DESKTOP, PAYTON-LEGION2) — required by TestRide052LabConductTests
- FR-RIDE-065 Caddy wording still naming PAYTON-DESKTOP loopback (outside Codex four-id scope)
- Invoke-LiveOtsSmoke (Payton: already Bitcoin-verified)
- 47 unnamed AC deferrals
- No Camera2 / H.264 SEI invent
- No rebase/merge of master

## Tests (Release, PAYTON-LEGION2)

`dotnet test tests/RideAudit.Server.Admission.Tests -c Release --filter FullyQualifiedName~TestRide052LabConductTests|FullyQualifiedName~NgrokDeploySecretsTests`

Passed: 5, Failed: 0

## Deliberately not changed

- Batch YAML already correct (SoT)
- deploy/omarchy/ngrok already LAB-OMARCHY
- Distribution receipt filename `octopus-payton-desktop.md` left as historical receipt path
