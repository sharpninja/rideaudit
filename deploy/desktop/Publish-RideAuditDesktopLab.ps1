# SPDX-License-Identifier: GPL-2.0-only
# Framework-dependent desktop publish plus lab Authenticode sign for win-x64.
# linux-x64 is published unsigned. macOS is out of scope.
# Signs only with CN=RideAudit Lab Self-Signed from Cert:\CurrentUser\My.
# Refuses McpServerManager Dev and ClaudeMigrator. Never exports a .pfx.
# signtool verify /pa is expected to fail Public Trust for this self-signed cert.
# Timestamp is attempted and may fail; the exe is then signed without a timestamp.
# This is Signed but not Public Trust. SmartScreen will warn. Full P11b is not closed.

[CmdletBinding()]
param(
    [string]$TimestampUrl = $(if ($env:RIDEAUDIT_WIN_TIMESTAMP_URL) { $env:RIDEAUDIT_WIN_TIMESTAMP_URL } else { 'http://timestamp.digicert.com' })
)

$ErrorActionPreference = 'Stop'

$Subject = 'CN=RideAudit Lab Self-Signed'
$Store = 'Cert:\CurrentUser\My'
$BlockedThumbprints = @(
    'FD1AC65B183E708D229E3D7A16C0D021CA3EB3C4', # CN=McpServerManager Dev
    '50ACCEC97BFD3A50A6C2EB7E34F454B2994D1919'  # CN=ClaudeMigrator
)

function Test-BlockedThumbprint {
    param([string]$Thumbprint)
    $normalized = ($Thumbprint -replace '\s', '').ToUpperInvariant()
    return $BlockedThumbprints -contains $normalized
}

function Invoke-NativeCapture {
    param(
        [Parameter(Mandatory = $true)][string]$FilePath,
        [Parameter(Mandatory = $true)][string[]]$ArgumentList
    )
    $ErrorActionPreference = 'Continue'
    $lines = & $FilePath @ArgumentList 2>&1 | ForEach-Object { "$_" }
    return [pscustomobject]@{
        ExitCode = $LASTEXITCODE
        Text     = ($lines -join "`n")
    }
}

function Get-SignToolPath {
    if ($env:RIDEAUDIT_WIN_SIGNTOOL -and (Test-Path -LiteralPath $env:RIDEAUDIT_WIN_SIGNTOOL)) {
        return (Resolve-Path -LiteralPath $env:RIDEAUDIT_WIN_SIGNTOOL).Path
    }
    $preferred = 'C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\signtool.exe'
    if (Test-Path -LiteralPath $preferred) {
        return $preferred
    }
    $kits = 'C:\Program Files (x86)\Windows Kits\10\bin'
    if (-not (Test-Path -LiteralPath $kits)) {
        throw "signtool.exe was not found. Set RIDEAUDIT_WIN_SIGNTOOL to the full path."
    }
    $candidate = Get-ChildItem -LiteralPath $kits -Filter signtool.exe -Recurse -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -match '\\x64\\signtool.exe$' } |
        Sort-Object { $_.FullName } -Descending |
        Select-Object -First 1
    if (-not $candidate) {
        throw "signtool.exe was not found under $kits. Set RIDEAUDIT_WIN_SIGNTOOL to the full path."
    }
    return $candidate.FullName
}

$root = (& git rev-parse --show-toplevel).Trim()
if (-not $root) { throw 'Not inside a git repository.' }
Set-Location -LiteralPath $root

$project = Join-Path $root 'src\RideAudit.Client.Desktop\RideAudit.Client.Desktop.csproj'
$outRoot = Join-Path $root 'artifacts\desktop-publish'
New-Item -ItemType Directory -Force -Path $outRoot | Out-Null

$cert = Get-ChildItem -LiteralPath $Store | Where-Object {
    $_.Subject -eq $Subject -and
    $_.HasPrivateKey -and
    ($_.EnhancedKeyUsageList.ObjectId -contains '1.3.6.1.5.5.7.3.3')
}
$certList = @($cert)
if ($certList.Count -ne 1) {
    throw "Expected exactly one $Subject code-signing certificate in $Store. Run deploy/desktop/New-RideAuditLabCodeSigningCert.ps1. Found $($certList.Count)."
}
$cert = $certList[0]
if (Test-BlockedThumbprint $cert.Thumbprint) {
    throw "Refusing unrelated certificate thumbprint $($cert.Thumbprint)."
}
if ($env:RIDEAUDIT_WIN_SIGN_THUMBPRINT) {
    $pinned = ($env:RIDEAUDIT_WIN_SIGN_THUMBPRINT -replace '\s', '').ToUpperInvariant()
    if ($pinned -ne $cert.Thumbprint) {
        throw "RIDEAUDIT_WIN_SIGN_THUMBPRINT=$pinned does not match $Subject ($($cert.Thumbprint)). Refusing to sign with another certificate."
    }
}

$signTool = Get-SignToolPath
Write-Host "SIGNTOOL=$signTool"
Write-Host "SIGNING_THUMBPRINT=$($cert.Thumbprint)"
Write-Host "SIGNING_SUBJECT=$($cert.Subject)"
Write-Host "PUBLIC_TRUST=false"
Write-Host "FULL_P11B_CLOSED=false"

function Publish-Rid {
    param([Parameter(Mandatory = $true)][string]$Rid)
    $outDir = Join-Path $outRoot $Rid
    if (Test-Path -LiteralPath $outDir) {
        Remove-Item -LiteralPath $outDir -Recurse -Force
    }
    New-Item -ItemType Directory -Force -Path $outDir | Out-Null
    Write-Host "PUBLISH rid=$Rid --self-contained false -> $outDir"
    $published = Invoke-NativeCapture -FilePath 'dotnet' -ArgumentList @(
        'publish', $project, '-c', 'Release', '-r', $Rid, '--self-contained', 'false', '-o', $outDir
    )
    Write-Host $published.Text
    if ($published.ExitCode -ne 0) { throw "dotnet publish $Rid failed with exit $($published.ExitCode)" }
    Write-Host "RID=$Rid EXIT=0"
    return $outDir
}

$winDir = Publish-Rid -Rid 'win-x64'
$linuxDir = Publish-Rid -Rid 'linux-x64'

$exe = Join-Path $winDir 'RideAudit.Client.Desktop.exe'
if (-not (Test-Path -LiteralPath $exe)) { throw "Missing $exe" }
$linuxDll = Join-Path $linuxDir 'RideAudit.Client.Desktop.dll'
if (-not (Test-Path -LiteralPath $linuxDll)) { throw "Missing $linuxDll" }
$linuxExe = Join-Path $linuxDir 'RideAudit.Client.Desktop.exe'
if (Test-Path -LiteralPath $linuxExe) { throw 'linux-x64 produced a Windows exe; refusing to Authenticode-sign it.' }

$timestampResult = 'not-attempted'
if ($TimestampUrl) {
    Write-Host "TIMESTAMP_ATTEMPT url=$TimestampUrl"
    $stamped = Invoke-NativeCapture -FilePath $signTool -ArgumentList @(
        'sign', '/fd', 'SHA256', '/sha1', $cert.Thumbprint, '/tr', $TimestampUrl, '/td', 'SHA256', $exe
    )
    Write-Host $stamped.Text
    Write-Host "TIMESTAMP_EXIT=$($stamped.ExitCode)"
    if ($stamped.ExitCode -eq 0) {
        $timestampResult = 'applied'
    }
    else {
        $timestampResult = 'failed'
        Write-Host 'TIMESTAMP_FAILED retrying sign without timestamp. Self-signed timestamp may fail; signature without a timestamp is the lab outcome.'
    }
}

if ($timestampResult -ne 'applied') {
    $signed = Invoke-NativeCapture -FilePath $signTool -ArgumentList @(
        'sign', '/fd', 'SHA256', '/sha1', $cert.Thumbprint, $exe
    )
    Write-Host $signed.Text
    Write-Host "SIGN_EXIT=$($signed.ExitCode)"
    if ($signed.ExitCode -ne 0) { throw "signtool sign failed with exit $($signed.ExitCode)" }
    if ($timestampResult -eq 'failed') {
        $timestampResult = 'failed-then-signed-without-timestamp'
    }
    else {
        $timestampResult = 'skipped'
    }
}

Write-Host "TIMESTAMP_RESULT=$timestampResult"

$auth = Get-AuthenticodeSignature -FilePath $exe
$signerSubject = ''
$signerThumb = ''
if ($auth.SignerCertificate) {
    $signerSubject = $auth.SignerCertificate.Subject
    $signerThumb = $auth.SignerCertificate.Thumbprint
}
Write-Host "AUTHENTICODE_win-x64 Status=$($auth.Status) Signer=$signerSubject Thumbprint=$signerThumb"
Write-Host "AUTHENTICODE_STATUS_MESSAGE=$($auth.StatusMessage)"
if ($auth.Status -eq 'NotSigned' -or -not $signerThumb) {
    throw 'signtool did not leave a signature on the win-x64 exe.'
}
if (Test-BlockedThumbprint $signerThumb) {
    throw "Signature uses a blocked thumbprint $signerThumb."
}
if ($signerThumb -ne $cert.Thumbprint) {
    throw "Signer thumbprint $signerThumb does not match lab certificate $($cert.Thumbprint)."
}

Write-Host 'VERIFY_PA_COMMAND=signtool verify /pa /v'
$verify = Invoke-NativeCapture -FilePath $signTool -ArgumentList @('verify', '/pa', '/v', $exe)
Write-Host $verify.Text
Write-Host "SIGNTOOL_VERIFY_PA_EXIT=$($verify.ExitCode)"
Write-Host 'Signed but not Public Trust. SmartScreen will warn. verify /pa is not a Public Trust pass for this self-signed certificate.'

$winDll = Join-Path $winDir 'RideAudit.Client.Desktop.dll'
Write-Host ("SHA256 win-x64 exe={0}" -f (Get-FileHash -Algorithm SHA256 -LiteralPath $exe).Hash)
Write-Host ("SHA256 win-x64 dll={0}" -f (Get-FileHash -Algorithm SHA256 -LiteralPath $winDll).Hash)
Write-Host ("SHA256 linux-x64 dll={0}" -f (Get-FileHash -Algorithm SHA256 -LiteralPath $linuxDll).Hash)
Write-Host 'LINUX_AUTHENTICODE=not-applicable'
Write-Host 'MACOS=not-published'
Write-Host 'PFX_EXPORTED=false'
Write-Host 'CLASS_C_CLOSED=false'
Write-Host 'real certs later'
