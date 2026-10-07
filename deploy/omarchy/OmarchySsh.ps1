# Shared SSH helper for LAB-OMARCHY. Remote login shell is pwsh and prints
# profile banners, so scp/sftp fail. Commands run under
# `exec /usr/bin/bash --noprofile --norc`. SPDX-License-Identifier: GPL-2.0-only

Set-StrictMode -Version Latest

function Get-OmarchySshExe {
    $candidate = Join-Path $env:WINDIR "System32\OpenSSH\ssh.exe"
    if (Test-Path -LiteralPath $candidate) { return $candidate }
    return "ssh.exe"
}

function Invoke-OmarchyBash {
    param(
        [Parameter(Mandatory = $true)][string]$Command,
        [string]$SshHost = "LAB-OMARCHY"
    )
    $sshExe = Get-OmarchySshExe
    & $sshExe -o BatchMode=yes -o ConnectTimeout=15 $SshHost "exec /usr/bin/bash --noprofile --norc -c '$Command'"
    if ($LASTEXITCODE -ne 0) { throw "remote bash failed: $Command" }
}

function Copy-OmarchyStdinFile {
    param(
        [Parameter(Mandatory = $true)][string]$LocalPath,
        [Parameter(Mandatory = $true)][string]$RemotePath,
        [string]$SshHost = "LAB-OMARCHY"
    )
    if (-not (Test-Path -LiteralPath $LocalPath)) { throw "missing $LocalPath" }
    $sshExe = Get-OmarchySshExe
    $remoteCat = "exec /usr/bin/bash --noprofile --norc -c 'cat > $RemotePath'"
    $proc = Start-Process -FilePath $sshExe -ArgumentList @("-o", "BatchMode=yes", "-o", "ConnectTimeout=60", $SshHost, $remoteCat) -RedirectStandardInput $LocalPath -NoNewWindow -Wait -PassThru
    if ($proc.ExitCode -ne 0) { throw "stdin copy to $RemotePath failed with exit $($proc.ExitCode)." }
}
