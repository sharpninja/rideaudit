# Unsigned framework-dependent desktop RID publish

Date: 2026-09-29T03:37:31Z (host clock 2026-09-28T22:37:31-05:00). Operator host: PAYTON-LEGION2. SPDX: GPL-2.0-only.

TR-RIDE-VIEW-001 / AC-RIDE-VIEW-001-001 records that a Release publish of `RideAudit.Client.Desktop` exited 0 for `win-x64`, `linux-x64`, and `osx-arm64`. This is not a signed release. P11b is not closed. The reproducible signed claim stays false. The client distribution manifest still says Windows and macOS `not-produced` for a signed release, and the desktop csproj does not embed a RID.

## Command

Framework-dependent publish (`--self-contained false`). Outputs are gitignored under `artifacts/desktop-publish/`.

```powershell
dotnet publish src/RideAudit.Client.Desktop/RideAudit.Client.Desktop.csproj -c Release -r win-x64 --self-contained false -o artifacts/desktop-publish/win-x64
dotnet publish src/RideAudit.Client.Desktop/RideAudit.Client.Desktop.csproj -c Release -r linux-x64 --self-contained false -o artifacts/desktop-publish/linux-x64
dotnet publish src/RideAudit.Client.Desktop/RideAudit.Client.Desktop.csproj -c Release -r osx-arm64 --self-contained false -o artifacts/desktop-publish/osx-arm64
```

`artifacts/desktop-publish/publish-log.txt`:

```text
start 2026-09-28T22:37:31.7688399-05:00
RID=win-x64 EXIT=0
RID=linux-x64 EXIT=0
RID=osx-arm64 EXIT=0
```

## Hashes (SHA256)

| RID | File | SHA256 |
| --- | --- | --- |
| win-x64 | RideAudit.Client.Desktop.dll | eaced988fe0c6ca08aef1951523e1d5b7ad44d3513d1a1f16a2c4ec68dca3b7a |
| win-x64 | RideAudit.Client.Desktop.exe | d7c62552d725b956280b71d5cad0beaed5d98ee2bd31a6aebaf7a68cd07834ba |
| linux-x64 | RideAudit.Client.Desktop.dll | 0e15f4c1ffcf0623c4687ab41fe9fcfe420f8f69e2862631aa41304c3eac1c87 |
| osx-arm64 | RideAudit.Client.Desktop.dll | a613f01406cb6760b131312a42aad2d43915ed2445a11be3159c211d344807c6 |

Linux and macOS publishes are framework-dependent managed assemblies. They do not include a native apphost exe.

## Signing blocker

signtool.exe is installed at `C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\signtool.exe`.

```text
AUTHENTICODE_win-x64 Status=NotSigned Signer=
SignTool Error: No signature found.
SIGNTOOL_EXIT=1
CODESIGN_ON_PATH=False
```

`codesign` is not on PATH, so the macOS publish was not signed on this Windows host.

Two CurrentUser\My certificates have the code-signing EKU and a private key. They are not RideAudit release identities and were not used:

- `CN=McpServerManager Dev` thumbprint `FD1AC65B183E708D229E3D7A16C0D021CA3EB3C4` NotAfter 2031-04-01
- `CN=ClaudeMigrator` thumbprint `50ACCEC97BFD3A50A6C2EB7E34F454B2994D1919` NotAfter 2029-05-12

Signed reproducible Win/Linux/macOS builds stay deferred until a RideAudit Authenticode identity and Apple codesign are available. Play publication is not claimed.
