# Remediation-phase checklist — code-hv-sol-r2 rem r2

**Operator:** Payton  
**Authorization date:** 2026-09-28  
**Order:** iterate remediation until opposing CODE HV AGREE  
**Active gate:** this authorized rem loop, not backdated P0 / section 8 / per-phase HV boxes

## Authorization (do not treat as historical P0)

- [x] Operator authorized iterate-until-agree after code-hv-sol-r1 DISAGREE@99
- [x] HV r2 custody squash-merged as `bf8f6ac` (PR #10) — NOT-READY / DISAGREE@99
- [x] Historical section 8 / P0 / Payton AGREE boxes left **unchecked**
- [x] Live tracks recorded as deferred (B04 / B07 / B08)

## Code closures required in rem r2

- [x] D01 concurrent admission/chunk/abuse/rate/metadata atomicity + adversarial tests
- [x] B02 UI start/stop invoke camera, Play, seal, escrow, authenticated admission (Unavailable* when missing)
- [x] B03 CanonicalAdmissionRequestFactory → AdmissionGrpcService (in-process server)
- [x] B01 rem-phase HV custody note + this checklist
- [x] B06 AC ledger coverage raised; remaining live ACs deferred-with-reason
- [x] CODE-HV READY scope note dated 2026-09-28

## Explicitly not claimed

- [ ] Historical P0 complete (do not check; not fabricated)
- [ ] Historical Payton AGREE on Astra R7 before PRs #3–#7 (do not check)
- [ ] Per-phase HV for P1–P11b / A1–A4 / S1–S9 (still required before claiming those phases complete)
- [ ] B04 physical BT media / H.264 composite
- [ ] B07 real Play / hardware HSM / OTS confirm / L2 signer
- [ ] B08 GHCR / production CD / edge TLS / Play publication
- [ ] MCP full-result session-log persistence (`MCP_PLUGIN_UNAVAILABLE:GrokCode`)
- [ ] Opposing Sol HV AGREE (coordinator after merge)
