# Provision a new Octopus Server container on PAYTON-DESKTOP.
# Used when the default octopus-legion2 instance cannot run tasks.
# FR-RIDE-063 AC-RIDE-063-002. Secrets stay under ~/.creds. No GHCR.
# SPDX-License-Identifier: GPL-2.0-only

[CmdletBinding()]
param(
    [string]$SshHost = "PAYTON-DESKTOP",
    [string]$RemoteAbs = "/home/sharpninja/github/rideaudit",
    [string]$ListenAddress = "192.168.0.149",
    [int]$HttpPort = 18066,
    [string]$CredOut = (Join-Path $env:USERPROFILE ".creds\octopus-rideaudit.cred.xml")
)

$ErrorActionPreference = "Stop"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
. (Join-Path $here "OctopusApi.ps1")
. (Join-Path $here "..\omarchy\OmarchySsh.ps1")

function New-RideAuditSecretText {
    param([int]$Length = 20)
    $chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789"
    $bytes = New-Object byte[] $Length
    [System.Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
    return -join ($bytes | ForEach-Object { $chars[$_ % $chars.Length] })
}

Write-Host "FR-RIDE-063 provisioning new Octopus container on $SshHost. Default instance stays up."

$sa = "Ra!" + (New-RideAuditSecretText -Length 16) + "9aA"
$adminPw = "Ra!" + (New-RideAuditSecretText -Length 16) + "9aA"
$apiKey = "API-" + (([guid]::NewGuid().ToString("N") + [guid]::NewGuid().ToString("N")).Substring(0, 27).ToUpperInvariant())
$db = "Server=db,1433;Database=OctopusRideAudit;User=sa;Password=$sa;TrustServerCertificate=True"

$envLines = @(
    "SA_PASSWORD=$sa",
    "ADMIN_USERNAME=admin",
    "ADMIN_PASSWORD=$adminPw",
    "ADMIN_EMAIL=rideaudit@localhost",
    "ADMIN_API_KEY=$apiKey",
    "DB_CONNECTION_STRING=$db"
)
$tmpEnv = Join-Path ([System.IO.Path]::GetTempPath()) "octopus-rideaudit.env"
[IO.File]::WriteAllText($tmpEnv, (($envLines -join "`n") + "`n"))

Write-Host "syncing new-instance compose (env copied over stdin, not printed)"
& (Join-Path $here "Sync-RideAuditTree.ps1") -SshHost $SshHost -RemoteAbs $RemoteAbs
$remoteEnv = "$RemoteAbs/deploy/octopus/new-instance/.env"
Copy-OmarchyStdinFile -SshHost $SshHost -LocalPath $tmpEnv -RemotePath $remoteEnv
Remove-Item -LiteralPath $tmpEnv -Force
Invoke-OmarchyBash -SshHost $SshHost -Command "chmod 600 $remoteEnv && chmod +x $RemoteAbs/deploy/octopus/new-instance/remote-up.sh && /usr/bin/bash --noprofile --norc $RemoteAbs/deploy/octopus/new-instance/remote-up.sh"

$apiBase = "http://${ListenAddress}:${HttpPort}"
Write-Host "waiting for $apiBase/api"
$ok = $false
for ($i = 1; $i -le 60; $i++) {
    try {
        $resp = Invoke-WebRequest -Uri "$apiBase/api" -Headers @{ "X-Octopus-ApiKey" = $apiKey } -TimeoutSec 10
        if ([int]$resp.StatusCode -eq 200) {
            Write-Host "new octopus API HTTP 200 on attempt $i"
            $ok = $true
            break
        }
    }
    catch {
        Write-Host ("attempt {0}: {1}" -f $i, $_.Exception.Message)
    }
    Start-Sleep -Seconds 10
}
if (-not $ok) { throw "new Octopus API did not become ready on $apiBase" }

$record = [pscustomobject]@{
    ServerUrl    = $apiBase
    ApiKey       = (ConvertTo-SecureString -String $apiKey -AsPlainText -Force)
    AdminUser    = "admin"
    AdminPassword = (ConvertTo-SecureString -String $adminPw -AsPlainText -Force)
}
$record | Export-Clixml -Path $CredOut
Write-Host "wrote DPAPI cred (no secrets printed) to $CredOut"
Write-Host "NEW_OCTOPUS_API_BASE=$apiBase"
Write-Host "NEW_OCTOPUS_CONTAINER=octopus-rideaudit-octopus-1"
Write-Host "NEW_OCTOPUS_OK"
