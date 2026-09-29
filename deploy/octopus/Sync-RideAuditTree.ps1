# Sync RideAudit HEAD (and optional linux-x64 publish tree) to PAYTON-DESKTOP.
# Reuses the Omarchy stdin-copy path because the login shell is pwsh.
# FR-RIDE-063. Not a CD green by itself. SPDX-License-Identifier: GPL-2.0-only

[CmdletBinding()]
param(
    [string]$SshHost = "PAYTON-DESKTOP",
    [string]$RemoteAbs = "/home/sharpninja/github/rideaudit",
    [switch]$WithPublish
)

$ErrorActionPreference = "Stop"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$omarchy = Join-Path $here "..\omarchy"

& (Join-Path $omarchy "Sync-FromLegion2.ps1") -SshHost $SshHost -RemoteAbs $RemoteAbs

. (Join-Path $omarchy "OmarchySsh.ps1")
Invoke-OmarchyBash -SshHost $SshHost -Command "test -f $RemoteAbs/deploy/octopus/remote-build-and-run.sh && chmod +x $RemoteAbs/deploy/octopus/remote-build-and-run.sh && if [ ! -f $RemoteAbs/deploy/octopus/.env ]; then cp $RemoteAbs/deploy/octopus/env.example $RemoteAbs/deploy/octopus/.env; fi && echo SYNC_OCTOPUS_TREE_OK"

if ($WithPublish) {
    $root = git rev-parse --show-toplevel
    $dll = Join-Path $root "artifacts\omarchy-publish\admission\RideAudit.Server.Admission.dll"
    if (-not (Test-Path -LiteralPath $dll)) {
        & (Join-Path $omarchy "Publish-Admission.ps1")
    }
    & (Join-Path $omarchy "Sync-Publish.ps1") -SshHost $SshHost -RemoteAbs $RemoteAbs
}
