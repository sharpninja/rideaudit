# CODE-HV READY — remediation-loop definition (2026-09-28)

**Dated:** 2026-09-28  
**Workspace:** `F:\GitHub\rideaudit` on PAYTON-LEGION2  
**Operator authorization:** iterate remediation until opposing CODE HV AGREE (Payton, 2026-09-28)

## What CODE-HV READY means for this loop

Opposing Sol HV (`gpt-5.6-sol` xhigh) may **AGREE** on **code readiness** when:

1. Closed code defects are proven by executable tests (this rem r2: D01 thread-safety, B02 executable capture path, B03 Canonical→`AdmissionGrpcService`, B01 rem-phase custody).
2. Deferred live tracks remain **fail-closed** and are **not overclaimed**.
3. Operator 2026-09-28 iterate-until-agree is the **active gate**. Historical section 8 / P0 / Payton AGREE boxes stay unchecked. They are not backdated.

CODE-HV READY is **not** product-complete, Play Store, live HSM, dual-phone media, or a live Octopus CD green. GHCR is not the CD path and is not an allowed fallback.

## Deferred live tracks (must stay fail-closed)

These remain out of scope for **this CODE-HV loop**. Missing them must not block CODE-HV AGREE if the code path refuses honestly. They are not a rewrite of product CD direction.

| Track | HV id | Honest state |
| --- | --- | --- |
| Physical Bluetooth media / H.264 composite | B04 | In-process source-payload container only. No two-device radio media. |
| Real Play decode, hardware HSM, OTS confirm, L2 signer | B07 | Adapters fail-closed or labeled fixture. No invented tokens or txids. |
| Production CD / edge TLS / Play publication | B08 | Lab Omarchy loopback plus ngrok is interim only. Product CD is Octopus Deploy to LAB-OMARCHY (FR-RIDE-063). Do not use GHCR. If the default Octopus container is out of licenses, create a new Octopus container on LAB-OMARCHY; that is not deferral and not out of scope. This loop still must not invent a live Octopus green. |

Do **not** weaken fail-closed behavior to manufacture greens.

## What this is not

- Not a historical P0 / section 8 / per-phase HV pass.
- Not a Sol HV verdict. Coordinator runs opposing HV after merge.
- Not permission to skip Unavailable* when hardware, Play, HSM, or admission config is missing.
