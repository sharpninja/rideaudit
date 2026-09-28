# Sync the current HEAD to PAYTON-OMARCHY as a git bundle.
# scp/sftp cannot be used: the remote login shell is pwsh and prints profile
# banners, which breaks the SFTP handshake. Files move over stdin to
# `exec /usr/bin/bash --noprofile --norc`. Does not wipe remote volumes.
# SPDX-License-Identifier: GPL-2.0-only

[CmdletBinding()]
param(
    [string]$SshHost = "PAYTON-OMARCHY",
    [string]$RemoteAbs = "/home/sharpninja/github/rideaudit"
)

$ErrorActionPreference = "Stop"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
. (Join-Path $here "OmarchySsh.ps1")

$root = git rev-parse --show-toplevel
if (-not $root) { throw "Not inside a git repository." }

$bundle = Join-Path ([System.IO.Path]::GetTempPath()) "rideaudit.bundle"
if (Test-Path -LiteralPath $bundle) { Remove-Item -LiteralPath $bundle -Force }

Write-Host "Bundling HEAD from $root"
git -C $root bundle create $bundle HEAD
if ($LASTEXITCODE -ne 0) { throw "git bundle create failed." }

Write-Host "Ensuring $RemoteAbs on $SshHost"
Invoke-OmarchyBash -SshHost $SshHost -Command "mkdir -p $RemoteAbs"

Write-Host "Copying bundle over stdin (not scp)"
Copy-OmarchyStdinFile -SshHost $SshHost -LocalPath $bundle -RemotePath "$RemoteAbs/rideaudit.bundle"
Invoke-OmarchyBash -SshHost $SshHost -Command "test -s $RemoteAbs/rideaudit.bundle"

Write-Host "Applying bundle"
Invoke-OmarchyBash -SshHost $SshHost -Command "cd $RemoteAbs && if [ ! -d .git ]; then git init; fi && git fetch ./rideaudit.bundle HEAD && git checkout --force FETCH_HEAD && git rev-parse --short HEAD && git status -sb"

Write-Host "Sync complete. Images are not built and containers are not started."
