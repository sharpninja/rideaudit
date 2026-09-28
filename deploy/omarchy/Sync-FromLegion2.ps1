# Sync the current HEAD to PAYTON-OMARCHY as a git bundle.
# Does not wipe remote Docker volumes or other repositories.
# SPDX-License-Identifier: GPL-2.0-only

[CmdletBinding()]
param(
    [string]$SshHost = "PAYTON-OMARCHY",
    [string]$RemoteAbs = "/home/sharpninja/github/rideaudit"
)

$ErrorActionPreference = "Stop"
$root = git rev-parse --show-toplevel
if (-not $root) { throw "Not inside a git repository." }

$bundle = Join-Path ([System.IO.Path]::GetTempPath()) "rideaudit.bundle"
if (Test-Path -LiteralPath $bundle) { Remove-Item -LiteralPath $bundle -Force }

Write-Host "Bundling HEAD from $root"
git -C $root bundle create $bundle HEAD
if ($LASTEXITCODE -ne 0) { throw "git bundle create failed." }

$ssh = @("-o", "BatchMode=yes", "-o", "ConnectTimeout=15", $SshHost)
Write-Host "Ensuring $RemoteAbs on $SshHost"
& ssh.exe @ssh "bash --noprofile --norc -c 'mkdir -p $RemoteAbs'"
if ($LASTEXITCODE -ne 0) { throw "remote mkdir failed." }

Write-Host "Copying bundle"
& scp.exe -o BatchMode=yes $bundle "${SshHost}:$RemoteAbs/rideaudit.bundle"
if ($LASTEXITCODE -ne 0) { throw "scp bundle failed." }

$remoteSh = "set -eu; cd $RemoteAbs; if [ -d .git ]; then git fetch ./rideaudit.bundle HEAD; git checkout --force FETCH_HEAD; else git clone ./rideaudit.bundle .; fi; git rev-parse --short HEAD; git status -sb"
& ssh.exe @ssh "bash --noprofile --norc -c '$remoteSh'"
if ($LASTEXITCODE -ne 0) { throw "remote git apply failed." }

Write-Host "Sync complete. Images are not built and containers are not started."
