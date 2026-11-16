# Coordinator-only cutover after merge. Default is a dry print of the steps.
# This agent does not pass -ConfirmCutover. Not a CD green.
# SPDX-License-Identifier: GPL-2.0-only

[CmdletBinding()]
param(
    [string]$SshHost = "LAB-OMARCHY",
    [string]$RemoteAbs = "/home/sharpninja/github/rideaudit",
    [switch]$ConfirmCutover
)

$ErrorActionPreference = "Stop"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
. (Join-Path $here "OmarchySsh.ps1")

$recordedDigestPath = Join-Path $here "RECORDED-IMAGE-DIGEST"
$recordedDigest = (Get-Content -LiteralPath $recordedDigestPath -Raw).Trim()
if ($recordedDigest -notmatch '^sha256:[0-9a-f]{64}$') {
    throw "RECORDED-IMAGE-DIGEST is missing or not a sha256 hex digest."
}

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
7. docker compose -f $RemoteAbs/deploy/omarchy/compose.yaml --env-file $RemoteAbs/deploy/omarchy/.env up -d --no-build --pull never
8. Probe http://127.0.0.1:18080/ on Omarchy and require HTTP 200. Fail cutover otherwise.
9. Confirm the running image id equals $recordedDigest. Do not docker compose down Octopus/SQL/Caddy.
10. Put TLS 1.2+ on the existing Caddy edge if this should leave loopback.
"@

Write-Host $steps
if (-not $ConfirmCutover) {
    Write-Host "Dry run only. Coordinator after merge: re-run with -ConfirmCutover."
    return
}

Write-Host "Starting loopback admission on $SshHost. Octopus/SQL/Caddy are not touched."
Invoke-OmarchyBash -SshHost $SshHost -Command "test -f $RemoteAbs/deploy/omarchy/env.example"
Invoke-OmarchyBash -SshHost $SshHost -Command "if [ ! -f $RemoteAbs/deploy/omarchy/.env ]; then cp $RemoteAbs/deploy/omarchy/env.example $RemoteAbs/deploy/omarchy/.env; fi"
Invoke-OmarchyBash -SshHost $SshHost -Command "cd $RemoteAbs && docker compose -f deploy/omarchy/compose.yaml --env-file deploy/omarchy/.env up -d --no-build --pull never"

Write-Host "Probing 127.0.0.1:18080 and enforcing HTTP 200."
Invoke-OmarchyBash -SshHost $SshHost -Command "code=`$(curl -s -o /tmp/rideaudit-cutover-body.txt -w %{http_code} --max-time 10 http://127.0.0.1:18080/); echo CUTOVER_HTTP=`$code; test `$code = 200"

Write-Host "Enforcing recorded image digest $recordedDigest."
Invoke-OmarchyBash -SshHost $SshHost -Command "id=`$(docker image inspect rideaudit-admission:local --format {{.Id}}); echo CUTOVER_IMAGE=`$id; echo `$id | grep -q $recordedDigest"

Write-Host "Compose up issued, loopback returned HTTP 200, and the image digest matched. This is still not a CD green until the coordinator records a receipt."
