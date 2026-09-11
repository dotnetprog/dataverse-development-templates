<#
.SYNOPSIS
    Creates or updates a named Dataverse authentication profile stored at
    %USERPROFILE%\dataverse.auth.json.

.DESCRIPTION
    Validates the supplied credentials by obtaining an OAuth 2.0 access token
    from Azure AD (client-credentials flow, scope: Dataverse Global Discovery).
    If the token request succeeds the profile is written to (or updated in)
    the JSON auth store.  The store is a flat JSON object whose keys are
    profile names, making it easy to manage multiple environments.

.PARAMETER Name
    Friendly name / key for this profile (e.g. "prod", "dev", "test").

.PARAMETER ClientId
    Azure AD application (client) ID.

.PARAMETER ClientSecret
    Azure AD client secret.

.PARAMETER TenantId
    Azure AD tenant ID (GUID or domain, e.g. "contoso.onmicrosoft.com").

.EXAMPLE
    .\Set-DataverseAuthProfile.ps1 `
        -Name       "prod" `
        -ClientId   "00000000-0000-0000-0000-000000000000" `
        -ClientSecret "your-secret" `
        -TenantId   "00000000-0000-0000-0000-000000000000"
#>

[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [string]$Name,

    [Parameter(Mandatory = $true)]
    [string]$ClientId,

    [Parameter(Mandatory = $true)]
    [string]$ClientSecret,

    [Parameter(Mandatory = $true)]
    [string]$TenantId
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ---------------------------------------------------------------------------
# Helpers — DPAPI encrypt / decrypt (Windows user-scoped, no external modules)
# ---------------------------------------------------------------------------
function ConvertTo-EncryptedString ([string]$PlainText) {
    $secure = ConvertTo-SecureString -String $PlainText -AsPlainText -Force
    return ConvertFrom-SecureString -SecureString $secure
}

function ConvertFrom-EncryptedString ([string]$EncryptedString) {
    $secure = ConvertTo-SecureString -String $EncryptedString
    $bstr   = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
    try {
        return [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($bstr)
    } finally {
        [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)
    }
}

# ---------------------------------------------------------------------------
# Constants
# ---------------------------------------------------------------------------
# The Dataverse Global Discovery Service is used purely as a validation scope.
# It is available to every tenant and requires no environment-specific URL.
$AUTH_STORE   = Join-Path $env:USERPROFILE 'dataverse.auth.json'
$TEST_SCOPE   = 'https://globaldisco.crm.dynamics.com/.default'
$TOKEN_URL    = "https://login.microsoftonline.com/$TenantId/oauth2/v2.0/token"

# ---------------------------------------------------------------------------
# 1. Test credentials by requesting an access token
# ---------------------------------------------------------------------------
Write-Host "Testing credentials for profile '$Name'..."

$tokenBody = [System.Collections.Generic.Dictionary[string,string]]::new()
$tokenBody.Add('grant_type',    'client_credentials')
$tokenBody.Add('client_id',     $ClientId)
$tokenBody.Add('client_secret', $ClientSecret)
$tokenBody.Add('scope',         $TEST_SCOPE)

try {
    $tokenResponse = Invoke-RestMethod `
        -Uri         $TOKEN_URL `
        -Method      Post `
        -ContentType 'application/x-www-form-urlencoded' `
        -Body        $tokenBody
}
catch {
    # Surface the Azure AD error description when available
    $detail = $_.ErrorDetails.Message
    if ($detail) {
        try {
            $errObj = $detail | ConvertFrom-Json
            if ($errObj.error_description) { $detail = $errObj.error_description }
        } catch { <# leave $detail as-is #> }
    }
    throw "Credential test failed. Azure AD response: $detail"
}

if ([string]::IsNullOrWhiteSpace($tokenResponse.access_token)) {
    throw "Credential test failed: access token was empty."
}

Write-Host "Credential test passed."

# ---------------------------------------------------------------------------
# 2. Load existing auth store (or start with an empty object)
# ---------------------------------------------------------------------------
if (Test-Path $AUTH_STORE) {
    $encrypted = Get-Content -Path $AUTH_STORE -Raw -Encoding UTF8
    try {
        $plainJson = ConvertFrom-EncryptedString -EncryptedString $encrypted.Trim()
    } catch {
        throw "Failed to decrypt '$AUTH_STORE'. The file may have been created by a different Windows user or is corrupt."
    }
    $profiles = $plainJson | ConvertFrom-Json
    
} else {
    $profiles = [PSCustomObject]@{}
}

# ---------------------------------------------------------------------------
# 3. Upsert the profile
# ---------------------------------------------------------------------------
$existingNames = @(Get-Member -InputObject $profiles -MemberType NoteProperty -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Name)

$action        = if ($existingNames -contains $Name) { 'Updated' } else { 'Created' }

$profileData = [PSCustomObject]@{
    clientId     = $ClientId
    clientSecret = $ClientSecret
    tenantId     = $TenantId
}

if ($existingNames -contains $Name) {
    $profiles.$Name = $profileData
} else {
    $profiles | Add-Member -NotePropertyName $Name -NotePropertyValue $profileData
}

# ---------------------------------------------------------------------------
# 4. Persist the auth store (DPAPI-encrypted, current Windows user only)
# ---------------------------------------------------------------------------
$plainJson    = $profiles | ConvertTo-Json -Depth 3
$encryptedOut = ConvertTo-EncryptedString -PlainText $plainJson
Set-Content -Path $AUTH_STORE -Value $encryptedOut -Encoding UTF8

Write-Host "$action profile '$Name' in $AUTH_STORE (DPAPI-encrypted)."
