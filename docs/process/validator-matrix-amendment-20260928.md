# Validator matrix amendment — 2026-09-28

**Status:** Process amendment  
**Scope:** Product opposing-model HV for RideAudit  
**Does not:** Claim HV AGREE, Play Store, live HSM, a GHCR green, or a live Octopus CD green. Product CD is Octopus Deploy to LAB-OMARCHY (FR-RIDE-063). Do not use GHCR. License exhaustion means a new Octopus container on LAB-OMARCHY, not deferral.

`docs/process/hostile-validation.md` and `docs/process/code-generation.md` named `gpt-6-sol` at `xhigh` as the required opposing validator to Grok `grok-4.6-xhigh` generation.

On 2026-09-28 the operator requested product code HV with **`gpt-5.6-sol`** at reasoning **`xhigh`**. That is the currently available Sol-family model id in Cursor.

This amendment records:

1. `gpt-5.6-sol` at `xhigh` is an approved opposing validator to Grok `grok-4.6-xhigh`.
2. `gpt-6-sol` is the Sol-family name; `gpt-5.6-sol` is the dated concrete alias.
3. code-hv-sol-r1 (`gpt-5.6-sol` xhigh) was formally eligible and returned NOT-READY / DISAGREE@99. Eligibility is not a pass.
4. The next opposing Sol HV rerun after this remediation may use `gpt-5.6-sol` at xhigh without a further model-name exception.
