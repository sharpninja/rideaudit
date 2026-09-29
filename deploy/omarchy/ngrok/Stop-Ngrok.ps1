# Stop the Omarchy ngrok tunnel. After this, no public URL is advertised.
# SPDX-License-Identifier: GPL-2.0-only

[CmdletBinding()]
param(
    [string]$SshHost = "PAYTON-OMARCHY"
)

$ErrorActionPreference = "Stop"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
. (Join-Path (Split-Path -Parent $here) "OmarchySsh.ps1")

function Copy-OmarchyUnixFile {
    param(
        [string]$LocalPath,
        [string]$RemotePath
    )
    $tmp = Join-Path ([System.IO.Path]::GetTempPath()) ("rideaudit-unix-" + [guid]::NewGuid().ToString("N"))
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

Invoke-OmarchyBash -SshHost $SshHost -Command "mkdir -p /tmp/rideaudit-ngrok"
Copy-OmarchyUnixFile -LocalPath (Join-Path $here "remote-stop.sh") -RemotePath "/tmp/rideaudit-ngrok/remote-stop.sh"
Invoke-OmarchyBash -SshHost $SshHost -Command "chmod +x /tmp/rideaudit-ngrok/remote-stop.sh && bash /tmp/rideaudit-ngrok/remote-stop.sh"
Write-Host "Tunnel stopped. No public URL is advertised as live."
