<#
.SYNOPSIS
    Removes a named Dataverse auth profile from %USERPROFILE%\dataverse.auth.json.

.PARAMETER Name
    The profile name to delete.

.EXAMPLE
    .\Remove-DataverseAuthProfile.ps1 -Name "pcflab"
#>

[CmdletBinding(SupportsShouldProcess)]
param (
    [Parameter(Mandatory = $true)]
    [string]$Name
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$AUTH_STORE = Join-Path $env:USERPROFILE 'dataverse.auth.json'

if (-not (Test-Path $AUTH_STORE)) {
    throw "No auth store found at '$AUTH_STORE'."
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

# ---------------------------------------------------------------------------
# Validate profile exists
# ---------------------------------------------------------------------------
if ((@(Get-Member -InputObject $store -MemberType NoteProperty -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Name)) -notcontains $Name) {
    throw "Profile '$Name' not found in '$AUTH_STORE'."
}

# ---------------------------------------------------------------------------
# Remove the profile property
# ---------------------------------------------------------------------------
if ($PSCmdlet.ShouldProcess("profile '$Name'", 'Remove')) {
    $store.PSObject.Properties.Remove($Name)

    # Re-encrypt and persist
    $plainJson    = $store | ConvertTo-Json -Depth 3
    $secure       = ConvertTo-SecureString -String $plainJson -AsPlainText -Force
    $encryptedOut = ConvertFrom-SecureString -SecureString $secure
    Set-Content -Path $AUTH_STORE -Value $encryptedOut -Encoding UTF8

    Write-Host "Profile '$Name' removed from '$AUTH_STORE'."
}
