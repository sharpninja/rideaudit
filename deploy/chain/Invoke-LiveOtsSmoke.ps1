# SPDX-License-Identifier: GPL-2.0-only
# Live OpenTimestamps smoke against public calendars. No API key. No purchase.
# POSTs a 32-byte SHA-256 digest as application/octet-stream to /digest.
# Saves the calendar body. A local .ots wrapper is magic + version + SHA-256
# op + digest + that body (python-opentimestamps DetachedTimestampFile).
# A pending attestation is not a Bitcoin txid. BitcoinBlockHeaderAttestation
# records a block height only. This script never invents a txid.
# Exit 0: at least one calendar returned a recognized attestation.
# Exit 2: every calendar failed or returned an unrecognized body.

[CmdletBinding()]
param(
    [int]$UpgradeWaitSeconds = 60,
    [string]$RepoRoot = ""
)

$ErrorActionPreference = "Stop"

if (-not $RepoRoot) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}

Add-Type -AssemblyName System.Net.Http

$PendingTag = [byte[]](0x83, 0xdf, 0xe3, 0x0d, 0x2e, 0xf9, 0x0c, 0x8e)
$BitcoinTag = [byte[]](0x05, 0x88, 0x96, 0x0d, 0x73, 0xd7, 0x19, 0x01)
$OtsMagic = New-Object System.Collections.Generic.List[byte]
$magicAscii = [Text.Encoding]::ASCII.GetBytes([string]([char]0) + "OpenTimestamps" + [string]([char]0) + [string]([char]0) + "Proof" + [string]([char]0))
$OtsMagic.AddRange($magicAscii)
$OtsMagic.AddRange([byte[]](0xbf, 0x89, 0xe2, 0xe8, 0x84, 0xe8, 0x92, 0x94))

$Calendars = @(
    @{ Name = "a.pool"; Url = "https://a.pool.opentimestamps.org" },
    @{ Name = "b.pool"; Url = "https://b.pool.opentimestamps.org" },
    @{ Name = "alice"; Url = "https://alice.btc.calendar.opentimestamps.org" },
    @{ Name = "bob"; Url = "https://bob.btc.calendar.opentimestamps.org" }
)

function Format-HexBytes {
    param([byte[]]$Bytes, [int]$Count = -1)
    if ($null -eq $Bytes -or $Bytes.Length -eq 0) { return "" }
    $take = $Bytes.Length
    if ($Count -ge 0 -and $Count -lt $take) { $take = $Count }
    $sb = New-Object System.Text.StringBuilder
    for ($i = 0; $i -lt $take; $i++) {
        [void]$sb.Append($Bytes[$i].ToString("x2"))
    }
    return $sb.ToString()
}

function Test-TagAt {
    param([byte[]]$Haystack, [int]$Index, [byte[]]$Needle)
    if ($Index -lt 0 -or ($Index + $Needle.Length) -gt $Haystack.Length) { return $false }
    for ($j = 0; $j -lt $Needle.Length; $j++) {
        if ($Haystack[$Index + $j] -ne $Needle[$j]) { return $false }
    }
    return $true
}

function Find-Tag {
    param([byte[]]$Haystack, [byte[]]$Needle)
    if ($null -eq $Haystack) { return -1 }
    $last = $Haystack.Length - $Needle.Length
    for ($i = 0; $i -le $last; $i++) {
        if (Test-TagAt -Haystack $Haystack -Index $i -Needle $Needle) { return $i }
    }
    return -1
}

function Get-ErrorChain {
    param($ErrorRecord)
    $parts = @()
    $ex = $ErrorRecord.Exception
    while ($null -ne $ex) {
        $parts += ($ex.GetType().FullName + ": " + $ex.Message)
        $ex = $ex.InnerException
    }
    return ($parts -join " | ")
}

function New-ByteCursor {
    param([byte[]]$Bytes)
    return [pscustomobject]@{ Bytes = $Bytes; Pos = 0 }
}

function Read-CursorByte {
    param($Cursor)
    if ($Cursor.Pos -ge $Cursor.Bytes.Length) { throw "timestamp walker eof" }
    $b = $Cursor.Bytes[$Cursor.Pos]
    $Cursor.Pos = $Cursor.Pos + 1
    return [int]$b
}

function Read-CursorBytes {
    param($Cursor, [int]$Count)
    if ($Count -lt 0) { throw "negative read" }
    if (($Cursor.Pos + $Count) -gt $Cursor.Bytes.Length) { throw "timestamp walker eof" }
    $slice = New-Object byte[] $Count
    if ($Count -gt 0) {
        [Array]::Copy($Cursor.Bytes, $Cursor.Pos, $slice, 0, $Count)
    }
    $Cursor.Pos = $Cursor.Pos + $Count
    return $slice
}

function Read-Varuint {
    param($Cursor)
    $first = Read-CursorByte $Cursor
    if ($first -lt 0xfd) { return [int64]$first }
    if ($first -eq 0xfd) {
        $raw = Read-CursorBytes $Cursor 2
        return [int64][BitConverter]::ToUInt16($raw, 0)
    }
    if ($first -eq 0xfe) {
        $raw = Read-CursorBytes $Cursor 4
        return [int64][BitConverter]::ToUInt32($raw, 0)
    }
    $raw8 = Read-CursorBytes $Cursor 8
    return [int64][BitConverter]::ToUInt64($raw8, 0)
}

function Read-Varbytes {
    param($Cursor)
    $len = Read-Varuint $Cursor
    if ($len -gt 8192) { throw "varbytes too long" }
    return (Read-CursorBytes $Cursor ([int]$len))
}

function Get-Sha256 {
    param([byte[]]$Data)
    $sha = [Security.Cryptography.SHA256]::Create()
    try { return $sha.ComputeHash($Data) }
    finally { $sha.Dispose() }
}

function Join-Bytes {
    param([byte[]]$Left, [byte[]]$Right)
    $out = New-Object byte[] ($Left.Length + $Right.Length)
    if ($Left.Length -gt 0) { [Array]::Copy($Left, 0, $out, 0, $Left.Length) }
    if ($Right.Length -gt 0) { [Array]::Copy($Right, 0, $out, $Left.Length, $Right.Length) }
    return $out
}

function Read-Attestation {
    param($Cursor, $Found)
    $tag = Read-CursorBytes $Cursor 8
    $payload = Read-Varbytes $Cursor
    $tagHex = Format-HexBytes $tag
    if (Test-TagAt -Haystack $tag -Index 0 -Needle $PendingTag) {
        $inner = New-ByteCursor $payload
        $uriBytes = Read-Varbytes $inner
        if ($inner.Pos -ne $inner.Bytes.Length) { throw "pending payload trailing bytes" }
        $uri = [Text.Encoding]::ASCII.GetString($uriBytes)
        [void]$Found.Pending.Add([pscustomobject]@{
            Uri = $uri
            CommitmentHex = (Format-HexBytes $Found.Msg)
        })
    }
    elseif (Test-TagAt -Haystack $tag -Index 0 -Needle $BitcoinTag) {
        $inner = New-ByteCursor $payload
        $height = Read-Varuint $inner
        if ($inner.Pos -ne $inner.Bytes.Length) { throw "bitcoin payload trailing bytes" }
        [void]$Found.BitcoinHeights.Add($height)
    }
    else {
        [void]$Found.UnknownTags.Add($tagHex)
    }
}

function Walk-Timestamp {
    param($Cursor, [byte[]]$Msg, $Found, [int]$Depth)
    if ($Depth -gt 64) { throw "timestamp recursion limit" }

    function Apply-One {
        param([int]$TagByte, [byte[]]$Current)
        if ($TagByte -eq 0x00) {
            $Found.Msg = $Current
            Read-Attestation $Cursor $Found
            return
        }
        if ($TagByte -eq 0x08) {
            $next = Get-Sha256 $Current
            Walk-Timestamp $Cursor $next $Found ($Depth + 1)
            return
        }
        if ($TagByte -eq 0xf0) {
            $suffix = Read-Varbytes $Cursor
            $next = Join-Bytes $Current $suffix
            Walk-Timestamp $Cursor $next $Found ($Depth + 1)
            return
        }
        if ($TagByte -eq 0xf1) {
            $prefix = Read-Varbytes $Cursor
            $next = Join-Bytes $prefix $Current
            Walk-Timestamp $Cursor $next $Found ($Depth + 1)
            return
        }
        throw ("unsupported ots op 0x" + $TagByte.ToString("x2"))
    }

    $tag = Read-CursorByte $Cursor
    while ($tag -eq 0xff) {
        $fork = Read-CursorByte $Cursor
        Apply-One $fork $Msg
        $tag = Read-CursorByte $Cursor
    }
    Apply-One $tag $Msg
}

function Invoke-Walk {
    param([byte[]]$Body, [byte[]]$Digest)
    $found = [pscustomobject]@{
        Msg = $Digest
        Pending = (New-Object System.Collections.Generic.List[object])
        BitcoinHeights = (New-Object System.Collections.Generic.List[object])
        UnknownTags = (New-Object System.Collections.Generic.List[string])
        WalkerError = ""
        Consumed = 0
    }
    if ($null -eq $Body -or $Body.Length -eq 0) {
        $found.WalkerError = "empty body"
        return $found
    }
    $cursor = New-ByteCursor $Body
    try {
        Walk-Timestamp $cursor $Digest $found 0
        $found.Consumed = $cursor.Pos
        if ($cursor.Pos -ne $Body.Length) {
            $found.WalkerError = "trailing $($Body.Length - $cursor.Pos) bytes"
        }
    }
    catch {
        $found.WalkerError = $_.Exception.Message
        $found.Consumed = $cursor.Pos
    }
    return $found
}

function New-OtsWrapper {
    param([byte[]]$Digest, [byte[]]$CalendarBody)
    $list = New-Object System.Collections.Generic.List[byte]
    $list.AddRange($OtsMagic)
    $list.Add(1)
    $list.Add(8)
    $list.AddRange($Digest)
    $list.AddRange($CalendarBody)
    return $list.ToArray()
}

function Invoke-HttpBytes {
    param($Client, [string]$Method, [string]$Url, [byte[]]$Body)
    $sw = [Diagnostics.Stopwatch]::StartNew()
    try {
        if ($Method -eq "POST") {
            $content = [System.Net.Http.ByteArrayContent]::new($Body)
            $content.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::new("application/octet-stream")
            $task = $Client.PostAsync($Url, $content)
        }
        else {
            $task = $Client.GetAsync($Url)
        }
        $resp = $task.GetAwaiter().GetResult()
        $bytes = $resp.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult()
        $sw.Stop()
        $ctype = ""
        if ($resp.Content.Headers.ContentType) { $ctype = $resp.Content.Headers.ContentType.ToString() }
        return [pscustomobject]@{
            HttpStatus = [int]$resp.StatusCode
            Reason = [string]$resp.ReasonPhrase
            ContentType = $ctype
            Body = $bytes
            ElapsedMs = $sw.ElapsedMilliseconds
            Error = ""
        }
    }
    catch {
        $sw.Stop()
        return [pscustomobject]@{
            HttpStatus = 0
            Reason = ""
            ContentType = ""
            Body = (New-Object byte[] 0)
            ElapsedMs = $sw.ElapsedMilliseconds
            Error = (Get-ErrorChain $_)
        }
    }
}

function Convert-HexToBytes {
    param([string]$Hex)
    if ($Hex.Length % 2 -ne 0) { throw "odd hex" }
    $bytes = New-Object byte[] ($Hex.Length / 2)
    for ($i = 0; $i -lt $bytes.Length; $i++) {
        $bytes[$i] = [Convert]::ToByte($Hex.Substring($i * 2, 2), 16)
    }
    return $bytes
}

$stamp = (Get-Date).ToUniversalTime().ToString("yyyyMMddTHHmmssZ")
$outDir = Join-Path $RepoRoot ("docs\receipts\chain\" + $stamp + "-live-ots-smoke")
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

$payloadText = @"
RideAudit live OpenTimestamps smoke payload
Label: LIVE-OTS-SMOKE
Host class: PAYTON-LEGION2 lab
This file is the preimage for a public calendar digest submit.
It is not a custody receipt.
It is not a documented fixture calendar.
It is not a Bitcoin transaction.
Git base: a5a649179a7e9ab6d098ec4517720c3d35a39001
"@.Replace("`r`n", "`n")
if (-not $payloadText.EndsWith("`n")) { $payloadText += "`n" }

$payloadPath = Join-Path $outDir "smoke-payload.txt"
$utf8 = New-Object System.Text.UTF8Encoding $false
[IO.File]::WriteAllText($payloadPath, $payloadText, $utf8)
$payloadBytes = [IO.File]::ReadAllBytes($payloadPath)
$digest = Get-Sha256 $payloadBytes
$digestHex = Format-HexBytes $digest
$fileHash = (Get-FileHash -Algorithm SHA256 -Path $payloadPath).Hash.ToLowerInvariant()
if ($fileHash -ne $digestHex) { throw "SHA-256 mismatch between Get-FileHash and SHA256.Create" }

$handler = [System.Net.Http.HttpClientHandler]::new()
$http = [System.Net.Http.HttpClient]::new($handler)
$http.Timeout = [TimeSpan]::FromSeconds(25)
[void]$http.DefaultRequestHeaders.UserAgent.ParseAdd("RideAudit-LiveOtsSmoke/1.0")
[void]$http.DefaultRequestHeaders.Accept.Add(
    [System.Net.Http.Headers.MediaTypeWithQualityHeaderValue]::new("application/vnd.opentimestamps.v1"))

$submits = @()
try {
    foreach ($cal in $Calendars) {
        $url = $cal.Url.TrimEnd("/") + "/digest"
        Write-Output ("SUBMIT " + $cal.Name + " " + $url)
        $hit = Invoke-HttpBytes -Client $http -Method POST -Url $url -Body $digest
        $body = New-Object byte[] 0
        if ($null -ne $hit.Body) { $body = [byte[]]@($hit.Body) }
        $walk = Invoke-Walk -Body $body -Digest $digest
        $safe = [string]$cal.Name
        $rawName = $safe + ".submit.bin"
        $otsName = $safe + ".submit.ots"
        [IO.File]::WriteAllBytes((Join-Path $outDir $rawName), $body)
        $otsFile = ""
        $recognized = ($walk.Pending.Count -gt 0 -or $walk.BitcoinHeights.Count -gt 0) -and [string]::IsNullOrEmpty([string]$walk.WalkerError)
        if ($recognized) {
            $wrapped = New-OtsWrapper -Digest $digest -CalendarBody $body
            [IO.File]::WriteAllBytes((Join-Path $outDir $otsName), $wrapped)
            $otsFile = $otsName
        }
        $pendingRows = New-Object System.Collections.Generic.List[object]
        foreach ($p in $walk.Pending) {
            $pendingRows.Add([pscustomobject]@{
                Uri = [string]$p.Uri
                CommitmentHex = [string]$p.CommitmentHex
            }) | Out-Null
        }
        $heights = New-Object System.Collections.Generic.List[object]
        foreach ($h in $walk.BitcoinHeights) { $heights.Add($h) | Out-Null }
        $unknown = New-Object System.Collections.Generic.List[string]
        foreach ($t in $walk.UnknownTags) { $unknown.Add([string]$t) | Out-Null }
        $row = [pscustomobject]@{
            Name = $safe
            CalendarUrl = [string]$cal.Url
            RequestUrl = $url
            HttpMethod = "POST"
            ContentType = "application/octet-stream"
            RequestBodyLength = 32
            HttpStatus = $hit.HttpStatus
            Reason = [string]$hit.Reason
            ResponseContentType = [string]$hit.ContentType
            ElapsedMs = $hit.ElapsedMs
            Error = [string]$hit.Error
            BodyLength = $body.Length
            BodyPrefixHex = (Format-HexBytes $body 48)
            RawFile = $rawName
            OtsFile = $otsFile
            Pending = $pendingRows.ToArray()
            BitcoinHeights = $heights.ToArray()
            UnknownTags = $unknown.ToArray()
            WalkerError = [string]$walk.WalkerError
            WalkerConsumed = $walk.Consumed
            PendingTagPresent = ((Find-Tag $body $PendingTag) -ge 0)
            BitcoinTagPresent = ((Find-Tag $body $BitcoinTag) -ge 0)
            Recognized = [bool]$recognized
        }
        $submits += $row
        Write-Output ("SUBMIT_DONE " + $safe + " http=" + $hit.HttpStatus + " bytes=" + $body.Length + " recognized=" + $recognized)
    }

    Write-Output ("WAIT_SECONDS " + $UpgradeWaitSeconds)
    if ($UpgradeWaitSeconds -gt 0) { Start-Sleep -Seconds $UpgradeWaitSeconds }

    $upgrades = @()
    foreach ($sub in $submits) {
        $url = [string]$sub.RequestUrl
        Write-Output ("REQUERY " + $sub.Name + " " + $url)
        $hit = Invoke-HttpBytes -Client $http -Method POST -Url $url -Body $digest
        $body = New-Object byte[] 0
        if ($null -ne $hit.Body) { $body = [byte[]]@($hit.Body) }
        $walk = Invoke-Walk -Body $body -Digest $digest
        $rawName = [string]$sub.Name + ".upgrade.bin"
        [IO.File]::WriteAllBytes((Join-Path $outDir $rawName), $body)
        $gets = @()
        $pendingList = @($sub.Pending)
        foreach ($pend in $pendingList) {
            $commitment = [string]$pend.CommitmentHex
            $getUrl = ([string]$pend.Uri).TrimEnd("/") + "/timestamp/" + $commitment
            Write-Output ("GET " + $getUrl)
            $got = Invoke-HttpBytes -Client $http -Method GET -Url $getUrl -Body (New-Object byte[] 0)
            $gbody = New-Object byte[] 0
            if ($null -ne $got.Body) { $gbody = [byte[]]@($got.Body) }
            $gwalk = Invoke-Walk -Body $gbody -Digest (Convert-HexToBytes $commitment)
            $gname = [string]$sub.Name + ".get." + ($gets.Count) + ".bin"
            [IO.File]::WriteAllBytes((Join-Path $outDir $gname), $gbody)
            $gHeights = New-Object System.Collections.Generic.List[object]
            foreach ($h in $gwalk.BitcoinHeights) { $gHeights.Add($h) | Out-Null }
            $gets += [pscustomobject]@{
                Url = $getUrl
                HttpStatus = $got.HttpStatus
                Reason = [string]$got.Reason
                Error = [string]$got.Error
                BodyLength = $gbody.Length
                BodyPrefixHex = (Format-HexBytes $gbody 48)
                RawFile = $gname
                BitcoinHeights = $gHeights.ToArray()
                PendingCount = $gwalk.Pending.Count
                WalkerError = [string]$gwalk.WalkerError
                BitcoinTagPresent = ((Find-Tag $gbody $BitcoinTag) -ge 0)
            }
        }
        $uHeights = New-Object System.Collections.Generic.List[object]
        foreach ($h in $walk.BitcoinHeights) { $uHeights.Add($h) | Out-Null }
        $upgrades += [pscustomobject]@{
            Name = [string]$sub.Name
            RequestUrl = $url
            HttpStatus = $hit.HttpStatus
            Reason = [string]$hit.Reason
            Error = [string]$hit.Error
            ElapsedMs = $hit.ElapsedMs
            BodyLength = $body.Length
            BodyPrefixHex = (Format-HexBytes $body 48)
            RawFile = $rawName
            PendingCount = $walk.Pending.Count
            BitcoinHeights = $uHeights.ToArray()
            WalkerError = [string]$walk.WalkerError
            BitcoinTagPresent = ((Find-Tag $body $BitcoinTag) -ge 0)
            PendingTagPresent = ((Find-Tag $body $PendingTag) -ge 0)
            Gets = $gets
        }
        Write-Output ("REQUERY_DONE " + $sub.Name + " http=" + $hit.HttpStatus + " bitcoin=" + ((Find-Tag $body $BitcoinTag) -ge 0))
    }
}
finally {
    $http.Dispose()
    $handler.Dispose()
}

$anyRecognized = $false
foreach ($sub in $submits) {
    if ($sub.Recognized -and $sub.HttpStatus -ge 200 -and $sub.HttpStatus -lt 300) { $anyRecognized = $true }
}

$bitcoinSeen = $false
foreach ($up in $upgrades) {
    if ($up.BitcoinTagPresent) { $bitcoinSeen = $true }
    foreach ($g in @($up.Gets)) {
        if ($g.BitcoinTagPresent) { $bitcoinSeen = $true }
    }
}
foreach ($sub in $submits) {
    if ($sub.BitcoinTagPresent) { $bitcoinSeen = $true }
}

$result = [pscustomobject]@{
    Stamp = $stamp
    Host = $env:COMPUTERNAME
    RepoRoot = $RepoRoot
    GitBase = "a5a649179a7e9ab6d098ec4517720c3d35a39001"
    PayloadFile = "smoke-payload.txt"
    PayloadLength = $payloadBytes.Length
    DigestHex = $digestHex
    Algorithm = "SHA-256"
    UpgradeWaitSeconds = $UpgradeWaitSeconds
    UserAgent = "RideAudit-LiveOtsSmoke/1.0"
    Accept = "application/vnd.opentimestamps.v1"
    InTreeClient = "src/RideAudit.Chain.OpenTimestamps/PublicOtsCalendarClient.cs"
    InTreeCalendarConstant = "https://alice.btc.calendar.opentimestamps.org"
    AnyRecognizedSubmit = $anyRecognized
    BitcoinAttestationSeen = $bitcoinSeen
    TransactionId = $null
    TransactionIdNote = "No txid is parsed or invented. BitcoinBlockHeaderAttestation carries a block height only. Pending proofs have no txid."
    Submits = $submits
    Upgrades = $upgrades
}
$jsonPath = Join-Path $outDir "result.json"
$json = $result | ConvertTo-Json -Depth 8
[IO.File]::WriteAllText($jsonPath, ($json + "`n"), $utf8)

Write-Output ("STAMP " + $stamp)
Write-Output ("DIGEST " + $digestHex)
Write-Output ("OUT " + $outDir)
Write-Output ("RECOGNIZED " + $anyRecognized)
Write-Output ("BITCOIN_TAG " + $bitcoinSeen)
Write-Output ("TXID none")

if (-not $anyRecognized) { exit 2 }
exit 0
