# P11b desktop signing inventory (PAYTON-LEGION2)

Date: 20260929T124653Z (host clock 2026-09-29T07:46:53-05:00). Operator host: PAYTON-LEGION2. SPDX: GPL-2.0-only.

Prior unsigned publish: `docs/receipts/distribution/20260929T033731Z-unsigned-desktop-rid-publish.md`.
Plan closeout (r3.6): signed reproducible desktop builds and full P11b remain **not closed**.

**P11b is not closed by this receipt.** Inventory + obtain path only. Nothing purchased. No private keys exported. Unrelated store certs not used.

## Operator decisions (binding for this increment)

| Decision | Value |
| --- | --- |
| Publisher | **Payton Byrd** (individual) |
| Platforms | **Windows only** (win-x64 Authenticode) |
| macOS / Apple codesign | **Deferred by operator** - do not plan |
| Provision path | **Traditional OV Authenticode + cloud HSM** |
| Azure Artifact Signing | **Stopped** - Azure subscription state is not usable for this path |
| Purchase this turn | **No** (agent must not buy) |

## Verdict

| Question | Answer |
| --- | --- |
| Usable RideAudit / sharpninja / Payton Byrd Authenticode material with private key on LEGION2? | **N** |
| Mac + codesign path? | **N** (and macOS deferred) |
| SignTool present? | **Y** (SDK; not on PATH) |
| Ready to sign win-x64 today? | **N** - provision OV + cloud HSM first |

## Tooling (LEGION2)

| Tool | Status |
| --- | --- |
| `signtool.exe` | `C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\signtool.exe` (also `10.0.19041.0`). Not on PATH. |
| `codesign` | Absent (Windows host). |
| Azure CLI `az` | Present v2.87.0. Account: subscription `Azure subscription 1` id `f52f8b2f-8faa-4207-9adb-67fd64da9b8a`, tenant `1d041a69-96e1-4f6e-b2b1-15acade5f35d`, user `ninja@thesharp.ninja`, **state=`Warned`** (operator: subscription disabled / not for Artifact Signing). `az keyvault list` was empty. **Do not pursue Azure Artifact Signing.** |
| DigiCert `smctl` / DigiCertUtil | Missing |
| SSL.com eSigner CLI / jsign | Missing |
| 1Password CLI `op` | Not on PATH |

Registered agents: PAYTON-LEGION2, PAYTON-DESKTOP, PAYTON-DESKTOP2 - all Windows. No Mac. SSH only to OMARCHY/PAYTON-DESKTOP `192.168.0.149` (Linux).

## Cert store (READ ONLY) - CurrentUser\My Code Signing EKU + HasPrivateKey

**Do not use** (unrelated self-signed):

| Subject | Thumbprint | NotAfter | HasPrivateKey |
| --- | --- | --- | --- |
| CN=McpServerManager Dev | FD1AC65B183E708D229E3D7A16C0D021CA3EB3C4 | 2031-04-01 | True |
| CN=ClaudeMigrator | 50ACCEC97BFD3A50A6C2EB7E34F454B2994D1919 | 2029-05-12 | True |

Also present but **not** Authenticode / not RideAudit: TruckMate PreProduction Identity Signing 2026 (no Code Signing EKU), TruckMate-issued PAYTON-* TLS client/server certs, Adobe content certs, localhost. LocalMachine\My: localhost only.

Filesystem: no RideAudit `.pfx`/`.p12` under repo or common secrets dirs. Unrelated: OrcaSlicer `printer.cer` / `slicer_base64.cer`; Starcraft `Sharp Ninja.spc`.

Env: no `SIGNING_*` / `CODE_SIGN*` / `CSC_*` / `APPLE_*`. Publish scripts not yet wired for signing. `origin/master` tip observed: `d1ce4d1`.

## Chosen path: OV Authenticode + cloud HSM (individual Payton Byrd)

For an **individual** publisher, CAs typically issue **IV** (Individual Validation) or OV when a registered business/DBA is used. Operator chose **traditional OV + cloud HSM** wording: enroll the product that matches Payton Byrd's legal identity (individual / sole prop / DBA as the CA accepts) with **cloud signing** so the private key stays in HSM.

### Recommended default order

1. **SSL.com - OV (or individual-equivalent) Code Signing + eSigner** - preferred for LEGION2 automation.
2. **DigiCert - KeyLocker / Software Trust Manager** - fallback if SSL.com KYC/pricing fails.

Do **not** fall back to Azure Artifact Signing while the subscription is disabled/Warned.

### Start enrollment URLs (operator opens; agent does not purchase)

- SSL.com code signing: https://www.ssl.com/code-signing-certificates/
- DigiCert code signing / KeyLocker: https://www.digicert.com/signing/code-signing-certificates

### Next steps - SSL.com OV + eSigner

1. Confirm legal display name for the cert subject: **Payton Byrd** (and DBA/org if any).
2. Create SSL.com account; select **Code Signing** with **eSigner** (cloud) - avoid file-based `.pfx` day-to-day if possible.
3. Complete identity validation (government ID, address, phone/email as CA requires for individual / OV).
4. After issuance: install eSigner credential on LEGION2; keep TOTP/API secrets in 1Password (not git).
5. Canary-sign `artifacts/desktop-publish/win-x64/RideAudit.Client.Desktop.exe` via eSigner + SignTool; verify `Get-AuthenticodeSignature` Valid and `signtool verify /pa` exit 0.
6. Wire publish env vars (below); re-receipt; still do not claim whole P11b closed.

### Next steps - DigiCert KeyLocker (fallback)

1. Same identity prep for **Payton Byrd**.
2. Enroll DigiCert Code Signing with **KeyLocker** / Software Trust Manager cloud key.
3. Install DigiCert Signing Manager Tools (`smctl`) on LEGION2; configure API key + client cert outside repo.
4. Same canary sign + verify + env wiring as above.

## Fields still needed from operator (for CA enrollment)

1. Exact subject name: **Payton Byrd** vs DBA/trade name
2. Whether sole prop / DBA registration exists (helps OV vs pure IV)
3. Legal address, phone, email for KYC
4. Government ID readiness
5. Billing method
6. Preference if SSL.com vs DigiCert after comparing quotes (default: **SSL.com eSigner**)

## Proposed publish-script env vars (not wired yet)

| Variable | Purpose |
| --- | --- |
| `RIDEAUDIT_WIN_SIGN_ENABLE` | `1` to sign after win-x64 publish |
| `RIDEAUDIT_WIN_SIGN_PROVIDER` | `sslesigner` or `digicert-keylocker` |
| `RIDEAUDIT_WIN_SIGN_THUMBPRINT` | Cert thumbprint metadata once available |
| `RIDEAUDIT_WIN_SIGNTOOL` | Optional full path to signtool |
| `RIDEAUDIT_WIN_TIMESTAMP_URL` | RFC3161 timestamp URL |
| `SSL_ESIGNER_CREDENTIAL_ID` / `SSL_ESIGNER_TOTP_SECRET` | SSL.com eSigner secrets (outside repo) |
| `SM_API_KEY` / `SM_CLIENT_CERT_FILE` / `SM_CLIENT_CERT_PASSWORD` / `SM_HOST` | DigiCert KeyLocker set (outside repo) |
| `RIDEAUDIT_WIN_SIGNED_OUT_DIR` | Optional signed output dir |

No `APPLE_*` / `CSC_*` this increment.

## Gaps

- No Payton Byrd / RideAudit OV cloud-HSM cert yet.
- eSigner / `smctl` not installed.
- Azure present but **not** the chosen path (subscription Warned/disabled).
- macOS deferred; Linux unsigned managed assemble remains out of scope for Authenticode.

## Minimal operator obtain list

1. Open https://www.ssl.com/code-signing-certificates/ → OV/individual code signing **+ eSigner**.
2. Complete KYC as **Payton Byrd**.
3. Return for LEGION2 canary sign + script wiring.

**Do not claim P11b closed. Do not buy from this agent turn. Do not use unrelated certs.**

## Addendum (20260929T125539Z): operator chose lab self-sign

Payton Byrd, after this inventory: self-sign for now. Real certs later.

The immediate path is no longer "purchase OV + cloud HSM before any signature." A dedicated lab certificate `CN=RideAudit Lab Self-Signed` was created in `CurrentUser\My` and used only for the lab canary. Commercial OV/IV + cloud HSM stays the later path and was not purchased. Unrelated store certificates were still not used.

See `docs/receipts/distribution/20260929T125539Z-self-signed-desktop-rid-publish.md`. That receipt does not close full P11b, Class C, or Public Trust.
