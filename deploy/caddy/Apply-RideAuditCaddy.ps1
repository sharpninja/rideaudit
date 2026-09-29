# Copy the lab Caddyfile to PAYTON-OMARCHY and start rideaudit-caddy.
# Does not compose down Octopus, SQL, the Octopus API Caddy, or rideaudit-omarchy.
# SPDX-License-Identifier: GPL-2.0-only

[CmdletBinding()]
param(
    [string]$SshHost = "PAYTON-OMARCHY",
    [string]$RemoteDir = "/home/sharpninja/rideaudit-caddy"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
. (Join-Path (Split-Path -Parent $here) "omarchy\OmarchySsh.ps1")

function Copy-UnixFile {
    param(
        [Parameter(Mandatory = $true)][string]$LocalPath,
        [Parameter(Mandatory = $true)][string]$RemotePath
    )
    $tmp = Join-Path ([System.IO.Path]::GetTempPath()) ("rideaudit-caddy-" + [guid]::NewGuid().ToString("N"))
    $text = [System.IO.File]::ReadAllText($LocalPath)
    $unix = $text.Replace("`r`n", "`n").Replace("`r", "`n")
    $utf8 = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($tmp, $unix, $utf8)
    try {
        Copy-OmarchyStdinFile -SshHost $SshHost -LocalPath $tmp -RemotePath $RemotePath
    }
    finally {
        Remove-Item -LiteralPath $tmp -Force -ErrorAction SilentlyContinue
    }
}

Invoke-OmarchyBash -SshHost $SshHost -Command "mkdir -p $RemoteDir"
Copy-UnixFile -LocalPath (Join-Path $here "Caddyfile") -RemotePath "$RemoteDir/Caddyfile"
Copy-UnixFile -LocalPath (Join-Path $here "compose.yaml") -RemotePath "$RemoteDir/compose.yaml"
Copy-UnixFile -LocalPath (Join-Path $here "remote-up.sh") -RemotePath "$RemoteDir/remote-up.sh"
Copy-UnixFile -LocalPath (Join-Path $here "remote-probe.sh") -RemotePath "$RemoteDir/remote-probe.sh"
Invoke-OmarchyBash -SshHost $SshHost -Command "chmod +x $RemoteDir/remote-up.sh && /usr/bin/bash --noprofile --norc $RemoteDir/remote-up.sh"
