# Lab self-signed desktop RID publish (PAYTON-LEGION2)

Date: 20260929T125539Z (host clock 2026-09-29T07:55:39-05:00). Operator host: PAYTON-LEGION2. SPDX: GPL-2.0-only.

Operator direction after the signing inventory: **self-sign for now**. **Real certs later.**

This receipt closes the **lab self-signed Authenticode** path for framework-dependent `win-x64` only. It does not close Public Trust, SmartScreen reputation, Class C plan acceptance, commercial OV/IV, or the full P11b exit. **full P11b is not closed.** Class C section 9 boxes stay unchecked.

Prior unsigned publish: `docs/receipts/distribution/20260929T033731Z-unsigned-desktop-rid-publish.md`.
Prior inventory (included from open PR #21): `docs/receipts/distribution/20260929T124653Z-p11b-signing-inventory.md`.

## Subject chosen

`CN=RideAudit Lab Self-Signed`

The lab certificate uses this subject so it stays distinct from a later commercial certificate for Payton Byrd and from unrelated store certificates. It is not an IV or OV identity.

| Field | Value |
| --- | --- |
| Store | `Cert:\CurrentUser\My` |
| SIGNING_THUMBPRINT | `98B8942B143D2D788F635530531C1B2DF0EC3C79` |
| NotBefore | 2026-09-29 |
| NotAfter | 2029-09-29 |
| Key | RSA 3072, SHA-256, code-signing EKU `1.3.6.1.5.5.7.3.3` |
| Export policy | `None` (created with `-KeyExportPolicy NonExportable`) |
| PFX | not exported |

Private key remains in the CurrentUser store. No `.pfx` with a password was written, and none is committed.

## Recreate

If this certificate is still in `CurrentUser\My`, reuse it. Do not mint a second one.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File deploy\desktop\New-RideAuditLabCodeSigningCert.ps1
```

If it was deleted, the same script calls:

```powershell
New-SelfSignedCertificate -Type CodeSigningCert -Subject 'CN=RideAudit Lab Self-Signed' -CertStoreLocation 'Cert:\CurrentUser\My' -KeyAlgorithm RSA -KeyLength 3072 -HashAlgorithm SHA256 -KeyExportPolicy NonExportable -KeyUsage DigitalSignature -FriendlyName 'RideAudit Lab Self-Signed Code Signing' -NotAfter (Get-Date).AddYears(3)
```

A recreated certificate has a **new thumbprint**. Update this receipt if that happens. Do not export a `.pfx` into the repo.

## Commands

Framework-dependent publish (`--self-contained false`). macOS was not published. Outputs are gitignored under `artifacts/desktop-publish/`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File deploy\desktop\Publish-RideAuditDesktopLab.ps1
```

That script publishes `win-x64` and `linux-x64`, then signs only `artifacts/desktop-publish/win-x64/RideAudit.Client.Desktop.exe`.

SignTool is not on PATH. The script used:

`C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\signtool.exe`

Override with `RIDEAUDIT_WIN_SIGNTOOL` if that kit build is absent. `RIDEAUDIT_WIN_SIGN_THUMBPRINT`, when set, must match the lab certificate. The script refuses these unrelated thumbprints:

- `CN=McpServerManager Dev` `FD1AC65B183E708D229E3D7A16C0D021CA3EB3C4`: were not used
- `CN=ClaudeMigrator` `50ACCEC97BFD3A50A6C2EB7E34F454B2994D1919`: were not used

## Publish result

```text
RID=win-x64 EXIT=0
RID=linux-x64 EXIT=0
```

| RID | Path | Signed | SHA256 |
| --- | --- | --- | --- |
| win-x64 | `F:\GitHub\rideaudit\artifacts\desktop-publish\win-x64\RideAudit.Client.Desktop.exe` | lab Authenticode | `DFCF5EDEDF8B51521855E56AB5EABFAC421E2E34758AEE7EA480AE2AFAFD5221` |
| win-x64 | `F:\GitHub\rideaudit\artifacts\desktop-publish\win-x64\RideAudit.Client.Desktop.dll` | no (managed dll; exe is the canary) | `2C1F09297C27DC95B3C39A25558D1FDAB6ADD27D2B2816D08E6F7D087C8DAA46` |
| linux-x64 | `F:\GitHub\rideaudit\artifacts\desktop-publish\linux-x64\RideAudit.Client.Desktop.dll` | no (Authenticode does not apply) | `37D088687B777480F6E8192F5B9EF0A1FE91F55C955BC835D61EEE92A4A41781` |

LINUX_AUTHENTICODE=not-applicable. MACOS=not-published.

## Timestamp

Attempted `http://timestamp.digicert.com` with `/tr` and `/td SHA256`.

```text
TIMESTAMP_EXIT=0
TIMESTAMP_RESULT=applied
```

The signature is timestamped `Tue Sep 29 07:54:43 2026` (host local, UTC-5). The TSA accepted this self-signed signature on this run. The publish script still signs again without a timestamp if a later TSA call fails, and that fallback must be recorded if it happens. A failed timestamp would not create Public Trust.

## Verify

signtool sees the signature. `signtool verify /pa` does **not** succeed. Outcome: **Signed but not Public Trust.** **SmartScreen will warn.**

```text
SIGNING_THUMBPRINT=98B8942B143D2D788F635530531C1B2DF0EC3C79
AUTHENTICODE_win-x64 Status=UnknownError Signer=CN=RideAudit Lab Self-Signed Thumbprint=98B8942B143D2D788F635530531C1B2DF0EC3C79
AUTHENTICODE_STATUS_MESSAGE=A certificate chain processed, but terminated in a root certificate which is not trusted by the trust provider
SIGNTOOL_VERIFY_PA_EXIT=1
```

`Status=UnknownError` here means a signature is present and the chain ends in an untrusted self-signed root. It is not `NotSigned`.

`signtool verify /pa /v` on the win-x64 exe:

```text
Verifying: F:\GitHub\rideaudit\artifacts\desktop-publish\win-x64\RideAudit.Client.Desktop.exe

Signature Index: 0 (Primary Signature)
Hash of file (sha256): 084F2D6DE636D82AC5397EF5D16AA86E99273CB4339168101D9EFD49D7D95118

Signing Certificate Chain:
    Issued to: RideAudit Lab Self-Signed
    Issued by: RideAudit Lab Self-Signed
    Expires:   Sat Sep 29 07:51:51 2029
    SHA1 hash: 98B8942B143D2D788F635530531C1B2DF0EC3C79

The signature is timestamped: Tue Sep 29 07:54:43 2026

SignTool Error: A certificate chain processed, but terminated in a root
	certificate which is not trusted by the trust provider.

Number of files successfully Verified: 0
Number of warnings: 0
Number of errors: 1
```

The Authenticode content hash above is not the whole-file SHA256. The whole-file hash includes the embedded signature.

Nothing was purchased. Commercial OV/IV + cloud HSM stays deferred. The client distribution manifest still has Windows `not-produced` and `reproducibleSignedClaim: false`. AC-RIDE-222-001 stays deferred. This is not a Class C closure and not a Public Trust closure.

**real certs later**
