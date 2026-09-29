# Copy the default Octopus subscription license onto the RideAudit instance.
# License XML never printed. FR-RIDE-063 AC-RIDE-063-002.
# SPDX-License-Identifier: GPL-2.0-only

[CmdletBinding()]
param(
    [string]$SourceApiBase = "http://192.168.0.149:8066",
    [string]$DestApiBase = "http://192.168.0.149:18066",
    [string]$SourceCredPath = "",
    [string]$DestCredPath = (Join-Path $env:USERPROFILE ".creds\octopus-rideaudit.cred.xml")
)

$ErrorActionPreference = "Stop"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
. (Join-Path $here "OctopusApi.ps1")

if ([string]::IsNullOrWhiteSpace($SourceCredPath)) {
    $src = Get-RideAuditOctopusConnection -ApiBase $SourceApiBase
}
else {
    $src = Get-RideAuditOctopusConnection -ApiBase $SourceApiBase -CredPath $SourceCredPath
}
$dst = Get-RideAuditOctopusConnection -ApiBase $DestApiBase -CredPath $DestCredPath

$current = Get-RideAuditOctopusJson -Connection $src -Path "/api/licenses/licenses-current"
if ([string]::IsNullOrWhiteSpace($current.LicenseText)) {
    throw "default instance license text missing"
}
Write-Host ("source serial-present={0} text-len={1}" -f (-not [string]::IsNullOrWhiteSpace([string]$current.SerialNumber)), $current.LicenseText.Length)

$applied = Invoke-RideAuditOctopusApi -Connection $dst -Method PUT -Path "/api/licenses/licenses-current" -Body @{
    LicenseText = $current.LicenseText
}
Write-Host ("applied serial-present={0}" -f (-not [string]::IsNullOrWhiteSpace([string]$applied.SerialNumber)))

$verify = Get-RideAuditOctopusJson -Connection $dst -Path "/api/licenses/licenses-current"
Write-Host ("verify serial-present={0} text-len={1}" -f (-not [string]::IsNullOrWhiteSpace([string]$verify.SerialNumber)), $verify.LicenseText.Length)
if ([string]::IsNullOrWhiteSpace([string]$verify.SerialNumber)) {
    throw "RideAudit Octopus still has no subscription serial after license apply"
}
Write-Host "RIDEAUDIT_OCTOPUS_LICENSE_OK"
