# Copy the LEGION2 linux-x64 publish tree to PAYTON-OMARCHY over SSH stdin.
# scp/sftp cannot be used (remote pwsh profile banners). Does not compose up.
# SPDX-License-Identifier: GPL-2.0-only

[CmdletBinding()]
param(
    [string]$SshHost = "PAYTON-OMARCHY",
    [string]$RemoteAbs = "/home/sharpninja/github/rideaudit",
    [string]$PublishDir = ""
)

$ErrorActionPreference = "Stop"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
. (Join-Path $here "OmarchySsh.ps1")

$root = git rev-parse --show-toplevel
if (-not $root) { throw "Not inside a git repository." }
if (-not $PublishDir) {
    $PublishDir = Join-Path $root "artifacts\omarchy-publish\admission"
}
if (-not (Test-Path -LiteralPath (Join-Path $PublishDir "RideAudit.Server.Admission.dll"))) {
    throw "Run deploy/omarchy/Publish-Admission.ps1 first."
}

$tar = Join-Path ([System.IO.Path]::GetTempPath()) "rideaudit-admission-linux-x64.tar"
if (Test-Path -LiteralPath $tar) { Remove-Item -LiteralPath $tar -Force }

Write-Host "Archiving $PublishDir"
Push-Location $PublishDir
try {
    tar -cf $tar *
    if ($LASTEXITCODE -ne 0) { throw "tar create failed." }
}
finally {
    Pop-Location
}

$remoteTar = "$RemoteAbs/rideaudit-admission-linux-x64.tar"
$remoteOut = "$RemoteAbs/artifacts/omarchy-publish/admission"
Write-Host "Copying archive to $SshHost`:$remoteTar"
Invoke-OmarchyBash -SshHost $SshHost -Command "mkdir -p $remoteOut"
Copy-OmarchyStdinFile -SshHost $SshHost -LocalPath $tar -RemotePath $remoteTar
Invoke-OmarchyBash -SshHost $SshHost -Command "rm -rf $remoteOut && mkdir -p $remoteOut && tar -xf $remoteTar -C $remoteOut && test -f $remoteOut/RideAudit.Server.Admission.dll && echo PUBLISH_OK"

Write-Host "Publish tree is on Omarchy. Images are not rebuilt and containers are not started."
