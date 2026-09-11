<#
.SYNOPSIS
    Lists all Dataverse auth profiles stored in %USERPROFILE%\dataverse.auth.json.

.DESCRIPTION
    Decrypts the auth store (DPAPI, current Windows user) and displays every
    profile with all properties except clientSecret.

.EXAMPLE
    .\Get-DataverseAuthProfiles.ps1
#>

[CmdletBinding()]
param ()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$AUTH_STORE = Join-Path $env:USERPROFILE 'dataverse.auth.json'

if (-not (Test-Path $AUTH_STORE)) {
    Write-Host "No auth store found at '$AUTH_STORE'. Run Set-DataverseAuthProfile.ps1 to create one."
    return
}

# ---------------------------------------------------------------------------
# Decrypt
# ---------------------------------------------------------------------------
$encrypted = Get-Content -Path $AUTH_STORE -Raw -Encoding UTF8
try {
    $secure  = ConvertTo-SecureString -String $encrypted.Trim()
    $bstr    = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
    $rawJson = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($bstr)
    [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)
} catch {
    throw "Failed to decrypt '$AUTH_STORE'. The file may have been created by a different Windows user or is corrupt."
}

$store = $rawJson | ConvertFrom-Json
$profileNames = @(Get-Member -InputObject $store -MemberType NoteProperty -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Name)
if ($profileNames.Count -eq 0) {
    Write-Host "Auth store is empty."
    return
}

# ---------------------------------------------------------------------------
# Display — all properties except clientSecret
# ---------------------------------------------------------------------------
Write-Host ""
Write-Host "Stored Dataverse auth profiles ($($profileNames.Count)):"
Write-Host ("-" * 50)

foreach ($name in $profileNames) {
    $p = $store.$name
    [PSCustomObject]@{
        Profile  = $name
        ClientId = $p.clientId
        TenantId = $p.tenantId
    }
}
