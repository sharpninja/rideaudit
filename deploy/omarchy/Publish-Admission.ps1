# Publish Admission (counsel RPCs included) for linux-x64 on PAYTON-LEGION2.
# Used when Docker Desktop on this host returns HTTP 500. Not a CD green.
# SPDX-License-Identifier: GPL-2.0-only

[CmdletBinding()]
param(
    [string]$OutputDir = ""
)

$ErrorActionPreference = "Stop"
$root = git rev-parse --show-toplevel
if (-not $root) { throw "Not inside a git repository." }
if (-not $OutputDir) {
    $OutputDir = Join-Path $root "artifacts\omarchy-publish\admission"
}

if (Test-Path -LiteralPath $OutputDir) {
    Remove-Item -LiteralPath $OutputDir -Recurse -Force
}
New-Item -ItemType Directory -Path $OutputDir | Out-Null

$project = Join-Path $root "src\RideAudit.Server.Admission\RideAudit.Server.Admission.csproj"
Write-Host "Publishing $project -> $OutputDir (linux-x64, framework-dependent)"
dotnet publish $project -c Release -r linux-x64 --self-contained false -p:PublishTrimmed=false -o $OutputDir
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed." }

$dll = Join-Path $OutputDir "RideAudit.Server.Admission.dll"
if (-not (Test-Path -LiteralPath $dll)) { throw "publish output missing RideAudit.Server.Admission.dll" }

Write-Host "Publish complete. Counsel is the same host with RIDEAUDIT_SERVICE_ROLE=counsel."
Write-Host "This is not a Docker image and not a CD green."
