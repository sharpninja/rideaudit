# Provision a new Octopus Server container on LAB-OMARCHY.
# Used when the default octopus-legion2 instance cannot run tasks.
# FR-RIDE-063 AC-RIDE-063-002. Secrets stay under ~/.creds. No GHCR.
# SPDX-License-Identifier: GPL-2.0-only

[CmdletBinding()]
param(
    [string]$SshHost = "LAB-OMARCHY",
    [string]$RemoteAbs = "/home/sharpninja/github/rideaudit",
    [string]$ListenAddress = "192.168.1.182",
    [int]$HttpPort = 18066,
    [string]$CredOut = (Join-Path $env:USERPROFILE ".creds\octopus-rideaudit.cred.xml"),
    [switch]$ResetData,
    [switch]$ReuseExistingEnv
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

function New-RideAuditMasterKey {
    $bytes = New-Object byte[] 16
    [System.Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
    return [Convert]::ToBase64String($bytes)
}

function Save-RideAuditOctopusCred {
    param(
        [string]$Path,
        [string]$ServerUrl,
        [string]$ApiKey,
        [string]$AdminPassword,
        [string]$MasterKey
    )
    $record = [pscustomobject]@{
        ServerUrl     = $ServerUrl
        ApiKey        = (ConvertTo-SecureString -String $ApiKey -AsPlainText -Force)
        AdminUser     = "admin"
        AdminPassword = (ConvertTo-SecureString -String $AdminPassword -AsPlainText -Force)
        MasterKey     = (ConvertTo-SecureString -String $MasterKey -AsPlainText -Force)
    }
    $record | Export-Clixml -Path $Path
    Write-Host "wrote DPAPI cred (no secrets printed) to $Path"
}

Write-Host "FR-RIDE-063 provisioning new Octopus container on $SshHost. Default instance stays up."

$remoteEnv = "$RemoteAbs/deploy/octopus/new-instance/.env"
$reuse = [bool]$ReuseExistingEnv
if ($ResetData -and -not $PSBoundParameters.ContainsKey("ReuseExistingEnv")) {
    $reuse = $true
}

$haveRemoteEnv = $false
if ($reuse) {
    try {
        Invoke-OmarchyBash -SshHost $SshHost -Command "test -f $remoteEnv"
        $haveRemoteEnv = $true
        Write-Host "reusing remote .env (not printed)"
    }
    catch {
        Write-Host "remote .env missing; generating new secrets"
        $haveRemoteEnv = $false
    }
}

$masterKey = New-RideAuditMasterKey
$apiKey = $null
$adminPw = $null

Write-Host "syncing new-instance compose (env copied over stdin, not printed)"
& (Join-Path $here "Sync-RideAuditTree.ps1") -SshHost $SshHost -RemoteAbs $RemoteAbs

if ($haveRemoteEnv) {
    $patch = Join-Path ([System.IO.Path]::GetTempPath()) "octopus-rideaudit-master.env"
    [IO.File]::WriteAllText($patch, ("MASTER_KEY={0}`n" -f $masterKey))
    Copy-OmarchyStdinFile -SshHost $SshHost -LocalPath $patch -RemotePath "/tmp/rideaudit-octopus-master.env"
    Remove-Item -LiteralPath $patch -Force
    Invoke-OmarchyBash -SshHost $SshHost -Command "grep -q ^MASTER_KEY= $remoteEnv && sed -i /^MASTER_KEY=/d $remoteEnv; cat /tmp/rideaudit-octopus-master.env >> $remoteEnv; rm -f /tmp/rideaudit-octopus-master.env; chmod 600 $remoteEnv"
    if (Test-Path -LiteralPath $CredOut) {
        $stored = Import-Clixml -LiteralPath $CredOut
        $apiKey = ConvertFrom-RideAuditSecurePlain -Secure $stored.ApiKey
        if ($stored.PSObject.Properties.Name -contains "AdminPassword" -and $stored.AdminPassword) {
            $adminPw = ConvertFrom-RideAuditSecurePlain -Secure $stored.AdminPassword
        }
        else {
            $adminPw = "unchanged-remote"
        }
    }
    if ([string]::IsNullOrWhiteSpace($apiKey)) {
        throw "reusing remote .env requires $CredOut with ApiKey"
    }
}
else {
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
        "DB_CONNECTION_STRING=$db",
        "MASTER_KEY=$masterKey",
        "DOCKER_GID=966"
    )
    $tmpEnv = Join-Path ([System.IO.Path]::GetTempPath()) "octopus-rideaudit.env"
    [IO.File]::WriteAllText($tmpEnv, (($envLines -join "`n") + "`n"))
    Copy-OmarchyStdinFile -SshHost $SshHost -LocalPath $tmpEnv -RemotePath $remoteEnv
    Remove-Item -LiteralPath $tmpEnv -Force
    Invoke-OmarchyBash -SshHost $SshHost -Command "chmod 600 $remoteEnv"
}

$remoteUp = "$RemoteAbs/deploy/octopus/new-instance/remote-up.sh"
Invoke-OmarchyBash -SshHost $SshHost -Command "chmod +x $remoteUp && sed -i s/\x0d`$// $remoteUp"

$upMode = "server"
if ($ResetData) { $upMode = "reset" }
Invoke-OmarchyBash -SshHost $SshHost -Command "/usr/bin/bash --noprofile --norc $remoteUp $upMode"

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

Save-RideAuditOctopusCred -Path $CredOut -ServerUrl $apiBase -ApiKey $apiKey -AdminPassword $adminPw -MasterKey $masterKey

try {
    & (Join-Path $here "Apply-RideAuditOctopusLicense.ps1") -DestApiBase $apiBase -DestCredPath $CredOut
}
catch {
    Write-Host ("license apply failed (tentacle start may still run): {0}" -f $_.Exception.Message)
}

$bootCx = Get-RideAuditOctopusConnection -ApiBase $apiBase -CredPath $CredOut
$envs = Get-RideAuditOctopusJson -Connection $bootCx -Path "/api/environments/all"
$envItems = @()
if ($envs -is [System.Array]) { $envItems = @($envs) }
elseif ($null -ne $envs -and $envs.PSObject.Properties.Name -contains "Items") { $envItems = @($envs.Items) }
elseif ($null -ne $envs) { $envItems = @($envs) }
$dev = @($envItems) | Where-Object { $_.Name -eq "Development" } | Select-Object -First 1
if (-not $dev) {
    $dev = Invoke-RideAuditOctopusApi -Connection $bootCx -Method POST -Path "/api/Spaces-1/environments" -Body @{
        Name             = "Development"
        Description      = "FR-RIDE-063 LAB-OMARCHY"
        SortOrder        = 1
        UseGuidedFailure = $false
    }
    Write-Host ("created environment {0} before Tentacle start" -f $dev.Id)
}
else {
    Write-Host ("environment {0} already exists" -f $dev.Id)
}

Write-Host "starting polling Tentacle (docker.sock worker)"
Invoke-OmarchyBash -SshHost $SshHost -Command "/usr/bin/bash --noprofile --norc $remoteUp tentacle"

Write-Host "NEW_OCTOPUS_API_BASE=$apiBase"
Write-Host "NEW_OCTOPUS_CONTAINER=octopus-rideaudit-octopus-1"
Write-Host "NEW_OCTOPUS_TENTACLE=octopus-rideaudit-tentacle-1"
Write-Host "NEW_OCTOPUS_RESET=$ResetData"
Write-Host "NEW_OCTOPUS_OK"
