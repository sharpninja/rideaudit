# Start the ngrok tunnel to LAB-OMARCHY admission 192.168.1.182:28080 from PAYTON-LEGION2.
# Token is copied from ~/.creds/ngrok.yml over SSH stdin. Never printed. Not GHCR.
# SPDX-License-Identifier: GPL-2.0-only

[CmdletBinding()]
param(
    [string]$SshHost = "LAB-OMARCHY",
    [string]$CredsPath = "",
    [string]$Addr = "192.168.1.182:28080",
    [switch]$KeepOnFailure,
    [switch]$KeepConflictingLocal
)

$ErrorActionPreference = "Stop"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
. (Join-Path (Split-Path -Parent $here) "OmarchySsh.ps1")

if (-not $CredsPath) {
    $CredsPath = Join-Path $env:USERPROFILE ".creds\ngrok.yml"
}

function Test-NgrokCredsFile {
    param([string]$Path)
    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Missing ngrok creds at $Path. Expected keys: version, authtoken."
    }
    $hasVersion = $false
    $hasToken = $false
    foreach ($line in Get-Content -LiteralPath $Path) {
        if ($line -match '^\s*version\s*:') { $hasVersion = $true }
        if ($line -match '^\s*authtoken\s*:') { $hasToken = $true }
    }
    if (-not $hasVersion -or -not $hasToken) {
        throw "ngrok creds file is missing version or authtoken key. Token sourced from ~/.creds/ngrok.yml"
    }
    Write-Host "Local ngrok creds: version and authtoken keys present. Token sourced from ~/.creds/ngrok.yml"
}

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

function Get-RemoteText {
    param([string]$Command)
    $out = Invoke-OmarchyBash -SshHost $SshHost -Command $Command
    if ($null -eq $out) { return "" }
    return ($out | Out-String).Trim()
}

function Invoke-FailClosedStop {
    param([string]$Reason)
    Write-Host "FAIL_CLOSED=$Reason"
    if (-not $KeepOnFailure) {
        try {
            Invoke-OmarchyBash -SshHost $SshHost -Command "bash /tmp/rideaudit-ngrok/remote-stop.sh"
        }
        catch {
            Write-Host "stop-after-failure also failed; tunnel is not advertised."
        }
    }
    throw "ngrok fail-closed: $Reason. No public URL is advertised."
}

function Invoke-OmarchyBashAllowFail {
    param([string]$Command)
    $sshExe = Get-OmarchySshExe
    $out = & $sshExe -o BatchMode=yes -o ConnectTimeout=15 $SshHost "exec /usr/bin/bash --noprofile --norc -c '$Command'"
    return [pscustomobject]@{
        ExitCode = $LASTEXITCODE
        Text = ($(if ($null -eq $out) { "" } else { $out | Out-String })).Trim()
    }
}

function Stop-ConflictingLocalNgrok {
    $api = $null
    try {
        $api = Invoke-RestMethod -Uri "http://127.0.0.1:4040/api/tunnels" -TimeoutSec 3
    }
    catch {
        return
    }
    $conflicts = @()
    foreach ($t in @($api.tunnels)) {
        $addr = [string]$t.config.addr
        $pub = [string]$t.public_url
        if ($addr -and $addr -notmatch '28080') {
            $conflicts += [pscustomobject]@{ Addr = $addr; Public = $pub }
        }
    }
    if ($conflicts.Count -eq 0) { return }
    foreach ($c in $conflicts) {
        Write-Host ("CONFLICTING_LOCAL_NGROK addr={0} public={1}" -f $c.Addr, $c.Public)
    }
    if ($KeepConflictingLocal) {
        throw "Local ngrok already holds the reserved domain for a non-admission address. Fail closed; not pooling mixed backends."
    }
    Write-Host "Stopping conflicting local ngrok so LAB-OMARCHY can terminate the RideAudit admission tunnel."
    foreach ($t in @($api.tunnels)) {
        $name = [string]$t.name
        if (-not $name) { continue }
        try {
            Invoke-RestMethod -Method Delete -Uri ("http://127.0.0.1:4040/api/tunnels/{0}" -f [uri]::EscapeDataString($name)) -TimeoutSec 5 | Out-Null
            Write-Host ("LOCAL_TUNNEL_DELETED={0}" -f $name)
        }
        catch {
            Write-Host ("LOCAL_TUNNEL_DELETE_FAILED={0}" -f $name)
        }
    }
    $proc = Get-Process -Name ngrok -ErrorAction SilentlyContinue
    if ($proc) {
        try {
            $proc | Stop-Process -Force -ErrorAction Stop
        }
        catch {
            Write-Host "Stop-Process access denied; trying taskkill."
            foreach ($p in @($proc)) {
                & taskkill.exe /F /PID $p.Id | Out-Null
            }
        }
    }
    Start-Sleep -Seconds 4
    $still = $false
    try {
        $again = Invoke-RestMethod -Uri "http://127.0.0.1:4040/api/tunnels" -TimeoutSec 3
        foreach ($t in @($again.tunnels)) {
            if ([string]$t.config.addr -notmatch '28080') { $still = $true }
        }
    }
    catch {
        $still = $false
    }
    if ($still) {
        throw "Could not displace the local ngrok that holds the reserved domain. Fail closed; not advertising a Swagger URL as admission."
    }
}

function Get-PublicProbe {
    param([string]$Url)
    $headers = @{ "ngrok-skip-browser-warning" = "1" }
    try {
        $resp = Invoke-WebRequest -Uri $Url -Headers $headers -Method GET -TimeoutSec 20 -UseBasicParsing
        $code = [int]$resp.StatusCode
        $body = [string]$resp.Content
        return [pscustomobject]@{ Code = $code; Body = $body; Error = "" }
    }
    catch {
        $code = 0
        $ex = $_.Exception
        if ($ex.Response -and $ex.Response.StatusCode) {
            $code = [int]$ex.Response.StatusCode
        }
        return [pscustomobject]@{ Code = $code; Body = ""; Error = $ex.Message }
    }
}

Test-NgrokCredsFile -Path $CredsPath
Stop-ConflictingLocalNgrok

Write-Host "Preflight: admission on $SshHost $Addr"
$admission = Get-RemoteText -Command "code=`$(curl -s -o /tmp/rideaudit-ngrok-admission-body.txt -w %{http_code} --max-time 10 http://$Addr/); echo ADMISSION_HTTP=`$code"
Write-Host $admission
if ($admission -notmatch "ADMISSION_HTTP=200") {
    throw "Admission is not HTTP 200 on $Addr. Fail closed; no public URL is advertised."
}

Write-Host "Preparing remote directories on $SshHost"
Invoke-OmarchyBash -SshHost $SshHost -Command "mkdir -p `$HOME/.config/ngrok `$HOME/.local/bin `$HOME/.local/state/rideaudit-ngrok /tmp/rideaudit-ngrok"

Write-Host "Copying helper scripts over SSH stdin (not scp, not git)"
Copy-OmarchyUnixFile -LocalPath (Join-Path $here "remote-install.sh") -RemotePath "/tmp/rideaudit-ngrok/remote-install.sh"
Copy-OmarchyUnixFile -LocalPath (Join-Path $here "remote-start.sh") -RemotePath "/tmp/rideaudit-ngrok/remote-start.sh"
Copy-OmarchyUnixFile -LocalPath (Join-Path $here "remote-stop.sh") -RemotePath "/tmp/rideaudit-ngrok/remote-stop.sh"
Copy-OmarchyUnixFile -LocalPath (Join-Path $here "rideaudit-ngrok.service") -RemotePath "/tmp/rideaudit-ngrok/rideaudit-ngrok.service"
Invoke-OmarchyBash -SshHost $SshHost -Command "chmod +x /tmp/rideaudit-ngrok/remote-install.sh /tmp/rideaudit-ngrok/remote-start.sh /tmp/rideaudit-ngrok/remote-stop.sh"

Write-Host "Copying ngrok config to non-git path ~/.config/ngrok/ngrok.yml"
$credsTmp = Join-Path ([System.IO.Path]::GetTempPath()) ("rideaudit-ngrok-creds-" + [guid]::NewGuid().ToString("N"))
$credsText = [System.IO.File]::ReadAllText($CredsPath).Replace("`r`n", "`n").Replace("`r", "`n")
[System.IO.File]::WriteAllText($credsTmp, $credsText, (New-Object System.Text.UTF8Encoding $false))
try {
    Copy-OmarchyStdinFile -SshHost $SshHost -LocalPath $credsTmp -RemotePath "/home/sharpninja/.config/ngrok/ngrok.yml"
}
finally {
    Remove-Item -LiteralPath $credsTmp -Force -ErrorAction SilentlyContinue
}
Invoke-OmarchyBash -SshHost $SshHost -Command "chmod 600 `$HOME/.config/ngrok/ngrok.yml && test -s `$HOME/.config/ngrok/ngrok.yml && echo CONFIG_PRESENT=1"

Write-Host "Installing or reusing ngrok on $SshHost"
$install = Get-RemoteText -Command "bash /tmp/rideaudit-ngrok/remote-install.sh"
Write-Host $install
if ($install -match "NGROK_NEED_TARBALL=1") {
    $tgz = Join-Path ([System.IO.Path]::GetTempPath()) "ngrok-v3-stable-linux-amd64.tgz"
    if (-not (Test-Path -LiteralPath $tgz)) {
        Write-Host "Downloading official linux amd64 ngrok tarball on LEGION2"
        Invoke-WebRequest -Uri "https://bin.equinox.io/c/bNyj1mQVY4c/ngrok-v3-stable-linux-amd64.tgz" -OutFile $tgz
    }
    Copy-OmarchyStdinFile -SshHost $SshHost -LocalPath $tgz -RemotePath "/tmp/rideaudit-ngrok/ngrok-v3-stable-linux-amd64.tgz"
    $install = Get-RemoteText -Command "bash /tmp/rideaudit-ngrok/remote-install.sh"
    Write-Host $install
}
if ($install -match "NGROK_NEED_TARBALL=1" -or $install -notmatch "NGROK_BIN=") {
    throw "ngrok binary was not installed on $SshHost. Fail closed; no public URL is advertised."
}

Write-Host "Starting tunnel to $Addr"
$started = Invoke-OmarchyBashAllowFail -Command "RIDEAUDIT_NGROK_ADDR=$Addr RIDEAUDIT_NGROK_CONFIG=`$HOME/.config/ngrok/ngrok.yml bash /tmp/rideaudit-ngrok/remote-start.sh"
$start = $started.Text
Write-Host $start
if ($started.ExitCode -ne 0 -and $start -notmatch "FAIL_CLOSED=") {
    Invoke-FailClosedStop -Reason "remote_start_failed"
}
if ($start -notmatch "ADMISSION_HTTP=200") {
    Invoke-FailClosedStop -Reason "admission_loopback_not_http_200"
}
if ($start -match "FAIL_CLOSED=([A-Za-z0-9_]+)") {
    Invoke-FailClosedStop -Reason $Matches[1]
}

$publicUrl = $null
if ($start -match "PUBLIC_URL=(https://\S+)") {
    $publicUrl = $Matches[1].Trim()
}
if (-not $publicUrl) {
    Invoke-FailClosedStop -Reason "no_public_url"
}

Write-Host "Probing public URL from PAYTON-LEGION2 (fail closed if this is not HTTP 200 + admission body)"
$probe = Get-PublicProbe -Url $publicUrl
Write-Host ("PROBE_HTTP={0}" -f $probe.Code)
$bodyOk = $probe.Body -match "RideAudit admission"
Write-Host ("PROBE_BODY_OK={0}" -f [int]$bodyOk)
if ($probe.Code -ne 200 -or -not $bodyOk) {
    if ($probe.Error) { Write-Host ("PROBE_ERROR={0}" -f $probe.Error) }
    Invoke-FailClosedStop -Reason "public_probe_failed"
}

Write-Host "PUBLIC_URL=$publicUrl"
Write-Host "TUNNEL_LIVE=1"
Write-Host "Restart: pwsh -NoProfile -File deploy/omarchy/ngrok/Start-Ngrok.ps1"
Write-Host "Stop:    pwsh -NoProfile -File deploy/omarchy/ngrok/Stop-Ngrok.ps1"
