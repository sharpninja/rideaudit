# SPDX-License-Identifier: GPL-2.0-only
# Create or reuse the RideAudit lab self-signed Authenticode certificate.
# Subject is CN=RideAudit Lab Self-Signed (not a commercial Payton Byrd IV/OV identity).
# Private key stays in Cert:\CurrentUser\My and is NonExportable. This script never writes a .pfx.
# Recreating after deletion mints a new thumbprint. Real commercial certs come later.

[CmdletBinding()]
param()

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

function Get-LabCodeSigningCert {
    Get-ChildItem -LiteralPath $Store | Where-Object {
        $_.Subject -eq $Subject -and
        $_.HasPrivateKey -and
        ($_.EnhancedKeyUsageList.ObjectId -contains '1.3.6.1.5.5.7.3.3')
    }
}

function Get-KeyExportPolicyName {
    param($Certificate)
    $rsa = [System.Security.Cryptography.X509Certificates.RSACertificateExtensions]::GetRSAPrivateKey($Certificate)
    if ($rsa -isnot [System.Security.Cryptography.RSACng]) {
        return 'unknown-not-cng'
    }
    try {
        return [string]$rsa.Key.ExportPolicy
    }
    finally {
        $rsa.Dispose()
    }
}

$found = @(Get-LabCodeSigningCert)
if ($found.Count -gt 1) {
    throw "Expected one $Subject certificate in $Store; found $($found.Count). Refusing to guess."
}

if ($found.Count -eq 0) {
    $created = New-SelfSignedCertificate -Type CodeSigningCert -Subject $Subject -CertStoreLocation $Store -KeyAlgorithm RSA -KeyLength 3072 -HashAlgorithm SHA256 -KeyExportPolicy NonExportable -KeyUsage DigitalSignature -FriendlyName 'RideAudit Lab Self-Signed Code Signing' -NotAfter (Get-Date).AddYears(3)
    $found = @($created)
    Write-Host "CREATED subject=$Subject"
}
else {
    Write-Host "REUSED subject=$Subject"
}

$cert = $found[0]
if (Test-BlockedThumbprint $cert.Thumbprint) {
    throw "Refusing unrelated certificate thumbprint $($cert.Thumbprint)."
}

$policy = Get-KeyExportPolicyName $cert
Write-Host ("THUMBPRINT={0}" -f $cert.Thumbprint)
Write-Host ("SUBJECT={0}" -f $cert.Subject)
Write-Host ("STORE={0}" -f $Store)
Write-Host ("NOT_BEFORE={0:yyyy-MM-dd}" -f $cert.NotBefore)
Write-Host ("NOT_AFTER={0:yyyy-MM-dd}" -f $cert.NotAfter)
Write-Host ("HAS_PRIVATE_KEY={0}" -f $cert.HasPrivateKey)
Write-Host ("EXPORT_POLICY={0}" -f $policy)
Write-Host "PFX_EXPORTED=false"
Write-Host "PUBLIC_TRUST=false"
Write-Host "Real certs later. This certificate is lab self-signed only."
