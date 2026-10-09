# Octopus REST helpers for RideAudit (FR-RIDE-063).
# Loads API keys from DPAPI cred files under ~/.creds. Never prints secrets.
# SPDX-License-Identifier: GPL-2.0-only

Set-StrictMode -Version Latest

function ConvertFrom-RideAuditSecurePlain {
    param([Parameter(Mandatory = $true)][System.Security.SecureString]$Secure)
    $bstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($Secure)
    try {
        return [Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr)
    }
    finally {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)
    }
}

function Get-RideAuditOctopusConnection {
    [CmdletBinding()]
    param(
        [string]$CredPath = (Join-Path $env:USERPROFILE ".creds\octopus-lab-omarchy.cred.xml"),
        [string]$ApiBase = ""
    )
    if (-not (Test-Path -LiteralPath $CredPath)) {
        throw "missing Octopus cred file $CredPath"
    }
    $stored = Import-Clixml -LiteralPath $CredPath
    $apiKey = ConvertFrom-RideAuditSecurePlain -Secure $stored.ApiKey
    if ([string]::IsNullOrWhiteSpace($apiKey)) {
        throw "Octopus API key is empty in $CredPath"
    }
    $base = $ApiBase
    if ([string]::IsNullOrWhiteSpace($base)) {
        # HTTPS portal on :8444 is the operator URL. HTTP :8066 is the
        # reachable API from PAYTON-LEGION2 (self-signed TLS on 8444 fails
        # hostname checks against the LAN IP).
        $base = "http://192.168.1.182:8066"
    }
    $portal = [string]$stored.ServerUrl
    if ([string]::IsNullOrWhiteSpace($portal)) { $portal = $base }
    return [pscustomobject]@{
        ApiBase    = $base.TrimEnd("/")
        PortalBase = $portal.TrimEnd("/")
        ApiKey     = $apiKey
        ServerUrl  = [string]$stored.ServerUrl
        CredPath   = $CredPath
    }
}

function Invoke-RideAuditOctopusApi {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]$Connection,
        [Parameter(Mandatory = $true)][string]$Method,
        [Parameter(Mandatory = $true)][string]$Path,
        $Body = $null,
        [int]$TimeoutSec = 60
    )
    $uri = $Connection.ApiBase + $Path
    $headers = @{
        "X-Octopus-ApiKey" = $Connection.ApiKey
        "Accept"           = "application/json"
    }
    $params = @{
        Uri             = $uri
        Method          = $Method
        Headers         = $headers
        TimeoutSec      = $TimeoutSec
        ContentType     = "application/json"
    }
    if ($PSVersionTable.PSVersion.Major -ge 7) {
        $params.SkipCertificateCheck = $true
    }
    if ($null -ne $Body) {
        if ($Body -is [string]) {
            $params.Body = $Body
        }
        else {
            $params.Body = ($Body | ConvertTo-Json -Depth 30 -Compress)
        }
    }
    try {
        return Invoke-RestMethod @params
    }
    catch {
        $detail = $_.Exception.Message
        try {
            $resp = $_.ErrorDetails.Message
            if ($resp) { $detail = "$detail :: $resp" }
        }
        catch { }
        throw "Octopus $Method $Path failed: $detail"
    }
}

function Get-RideAuditOctopusJson {
    param([Parameter(Mandatory = $true)]$Connection, [Parameter(Mandatory = $true)][string]$Path)
    Invoke-RideAuditOctopusApi -Connection $Connection -Method GET -Path $Path
}

function Get-RideAuditYamlMap {
    param([Parameter(Mandatory = $true)][string]$Path)
    $map = @{}
    foreach ($line in Get-Content -LiteralPath $Path) {
        if ($line -match '^\s*#' -or $line -notmatch ':') { continue }
        $parts = $line.Split(":", 2)
        if ($parts.Count -lt 2) { continue }
        $map[$parts[0].Trim()] = $parts[1].Trim().Trim("'").Trim('"')
    }
    return $map
}

function Get-RideAuditSshFingerprint {
    param([string]$SshHost = "LAB-OMARCHY")
    $here = Split-Path -Parent $PSCommandPath
    . (Join-Path $here "..\omarchy\OmarchySsh.ps1")
    $raw = & (Get-OmarchySshExe) -o BatchMode=yes -o ConnectTimeout=15 $SshHost "exec /usr/bin/bash --noprofile --norc -c 'ssh-keygen -E md5 -lf /etc/ssh/ssh_host_ed25519_key.pub'"
    if ($LASTEXITCODE -ne 0) { throw "failed to read SSH host fingerprint on $SshHost" }
    $line = (($raw | Out-String) -split "`n" | Where-Object { $_ -match "MD5:" } | Select-Object -Last 1)
    if ($line -notmatch "MD5:([0-9a-f:]+)") {
        throw "could not parse MD5 fingerprint from: $line"
    }
    return $Matches[1]
}
