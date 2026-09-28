# Coordinator-only cutover after merge. Default is a dry print of the steps.
# This agent does not pass -ConfirmCutover. Not a CD green.
# SPDX-License-Identifier: GPL-2.0-only

[CmdletBinding()]
param(
    [string]$SshHost = "PAYTON-OMARCHY",
    [string]$RemoteAbs = "/home/sharpninja/github/rideaudit",
    [switch]$ConfirmCutover
)

$ErrorActionPreference = "Stop"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
. (Join-Path $here "OmarchySsh.ps1")

$steps = @"
1. Confirm the Omarchy checkout is the merged commit.
2. pwsh -NoProfile -File deploy/omarchy/Publish-Admission.ps1
3. pwsh -NoProfile -File deploy/omarchy/Sync-FromLegion2.ps1
4. pwsh -NoProfile -File deploy/omarchy/Sync-Publish.ps1
5. ssh $SshHost `"exec /usr/bin/bash --noprofile --norc -c 'bash $RemoteAbs/deploy/omarchy/remote-runtime-build.sh'`"
   (or remote-build.sh if you prefer a full SDK rebuild from source)
6. Copy $RemoteAbs/deploy/omarchy/env.example to $RemoteAbs/deploy/omarchy/.env
   Leave calendars unset to fail closed. Production refuses documented-fixture
   and RIDEAUDIT_PLAY_INTEGRITY=fixture.
7. docker compose -f $RemoteAbs/deploy/omarchy/compose.yaml --env-file $RemoteAbs/deploy/omarchy/.env up -d
8. Probe 127.0.0.1:18080 on Omarchy. Do not docker compose down Octopus/SQL/Caddy.
9. Put TLS 1.2+ on the existing Caddy edge if this should leave loopback.
"@

Write-Host $steps
if (-not $ConfirmCutover) {
    Write-Host "Dry run only. Coordinator after merge: re-run with -ConfirmCutover."
    return
}

Write-Host "Starting loopback admission on $SshHost. Octopus/SQL/Caddy are not touched."
Invoke-OmarchyBash -SshHost $SshHost -Command "test -f $RemoteAbs/deploy/omarchy/env.example"
Invoke-OmarchyBash -SshHost $SshHost -Command "if [ ! -f $RemoteAbs/deploy/omarchy/.env ]; then cp $RemoteAbs/deploy/omarchy/env.example $RemoteAbs/deploy/omarchy/.env; fi"
Invoke-OmarchyBash -SshHost $SshHost -Command "cd $RemoteAbs && docker compose -f deploy/omarchy/compose.yaml --env-file deploy/omarchy/.env up -d"
Write-Host "Compose up issued. This is still not a CD green until the coordinator records a receipt."
