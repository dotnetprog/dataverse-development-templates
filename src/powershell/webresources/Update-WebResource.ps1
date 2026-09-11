<#
.SYNOPSIS
    Updates a Dataverse web resource from a local file.

.PARAMETER WebResourceUniqueName
    The unique name of the web resource in Dataverse (e.g. "boost_/js/myScript.js").

.PARAMETER LocalFilePath
    Relative or absolute path to the local file whose content will be uploaded.

.PARAMETER EnvironmentUrl
    The Dataverse environment URL (e.g. "https://yourorg.crm.dynamics.com").

.PARAMETER ClientId
    The Azure AD application (client) ID used for authentication.

.PARAMETER ClientSecret
    The Azure AD client secret.

.PARAMETER TenantId
    The Azure AD tenant ID.

.PARAMETER ProfileName
    Name of a profile stored in %USERPROFILE%\dataverse.auth.json
    (created by Set-DataverseAuthProfile.ps1).  When supplied, ClientId,
    ClientSecret and TenantId are loaded from the encrypted store and must
    NOT be passed explicitly.

.EXAMPLE
    # With explicit credentials
    .\Update-WebResource.ps1 `
        -WebResourceUniqueName "boost_/js/myScript.js" `
        -LocalFilePath "src\WebResources\js\myScript.js" `
        -EnvironmentUrl "https://yourorg.crm.dynamics.com" `
        -ClientId "00000000-0000-0000-0000-000000000000" `
        -ClientSecret "your-secret" `
        -TenantId "00000000-0000-0000-0000-000000000000"

.EXAMPLE
    # With a saved auth profile
    .\Update-WebResource.ps1 `
        -WebResourceUniqueName "boost_/js/myScript.js" `
        -LocalFilePath "src\WebResources\js\myScript.js" `
        -EnvironmentUrl "https://yourorg.crm.dynamics.com" `
        -ProfileName "prod"
#>

[CmdletBinding(DefaultParameterSetName = 'Explicit')]
param (
    [Parameter(Mandatory = $true)]
    [string]$WebResourceUniqueName,

    [Parameter(Mandatory = $true)]
    [string]$LocalFilePath,

    [Parameter(Mandatory = $true)]
    [string]$EnvironmentUrl,

    # --- Explicit credentials ---
    [Parameter(Mandatory = $true, ParameterSetName = 'Explicit')]
    [string]$ClientId,

    [Parameter(Mandatory = $true, ParameterSetName = 'Explicit')]
    [string]$ClientSecret,

    [Parameter(Mandatory = $true, ParameterSetName = 'Explicit')]
    [string]$TenantId,

    # --- Auth profile (from encrypted store) ---
    [Parameter(Mandatory = $true, ParameterSetName = 'Profile')]
    [string]$ProfileName
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ---------------------------------------------------------------------------
# 0. Load credentials from encrypted auth store when -ProfileName is used
# ---------------------------------------------------------------------------
if ($PSCmdlet.ParameterSetName -eq 'Profile') {
    $AUTH_STORE = Join-Path $env:USERPROFILE 'dataverse.auth.json'
    if (-not (Test-Path $AUTH_STORE)) {
        throw "Auth store not found at '$AUTH_STORE'. Run Set-DataverseAuthProfile.ps1 first."
    }
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
    if (-not ($store.PSObject.Properties.Name -contains $ProfileName)) {
        throw "Profile '$ProfileName' not found in '$AUTH_STORE'."
    }
    $prof         = $store.$ProfileName
    $ClientId     = $prof.clientId
    $ClientSecret = $prof.clientSecret
    $TenantId     = $prof.tenantId
    Write-Host "Loaded credentials from profile '$ProfileName'."
}

# ---------------------------------------------------------------------------
# 1. Resolve and validate the local file
# ---------------------------------------------------------------------------
$resolvedPath = Resolve-Path -Path $LocalFilePath -ErrorAction SilentlyContinue
if (-not $resolvedPath) {
    throw "Local file not found: '$LocalFilePath'"
}
$localFile = $resolvedPath.Path
Write-Verbose "Local file resolved to: $localFile"

# ---------------------------------------------------------------------------
# 2. Acquire an OAuth 2.0 access token via client credentials flow
# ---------------------------------------------------------------------------
$environmentUrl = $EnvironmentUrl.TrimEnd('/')
$tokenEndpoint  = "https://login.microsoftonline.com/$TenantId/oauth2/v2.0/token"
$scope          = "$environmentUrl/.default"

Write-Host "Acquiring access token from Azure AD..."

$tokenBody = [System.Collections.Generic.Dictionary[string,string]]::new()
$tokenBody.Add('grant_type',    'client_credentials')
$tokenBody.Add('client_id',     $ClientId)
$tokenBody.Add('client_secret', $ClientSecret)
$tokenBody.Add('scope',         $scope)

$tokenResponse = Invoke-RestMethod `
    -Uri         $tokenEndpoint `
    -Method      Post `
    -ContentType 'application/x-www-form-urlencoded' `
    -Body        $tokenBody

$accessToken = $tokenResponse.access_token
if ([string]::IsNullOrWhiteSpace($accessToken)) {
    throw "Failed to acquire access token. Check ClientId, ClientSecret and TenantId."
}
Write-Host "Access token acquired successfully."

# Common headers for all Dataverse calls
$headers = @{
    'Authorization'    = "Bearer $accessToken"
    'Accept'           = 'application/json'
    'OData-MaxVersion' = '4.0'
    'OData-Version'    = '4.0'
}

# ---------------------------------------------------------------------------
# 3. Look up the web resource by unique name
# ---------------------------------------------------------------------------
$encodedName  = [Uri]::EscapeDataString($WebResourceUniqueName)
$queryUrl     = "$environmentUrl/api/data/v9.2/webresourceset" +
                "?`$filter=name eq '$encodedName'" +
                "&`$select=webresourceid,name,displayname"

Write-Host "Looking up web resource '$WebResourceUniqueName'..."

$queryResponse = Invoke-RestMethod `
    -Uri     $queryUrl `
    -Method  Get `
    -Headers $headers

if ($null -eq $queryResponse.value -or $queryResponse.value.Count -eq 0) {
    throw "Web resource with unique name '$WebResourceUniqueName' was not found in '$environmentUrl'."
}

$webResource   = $queryResponse.value[0]
$webResourceId = $webResource.webresourceid
Write-Host "Found web resource: id=$webResourceId  name=$($webResource.name)"

# ---------------------------------------------------------------------------
# 4. Read local file and base64-encode its content
# ---------------------------------------------------------------------------
$fileBytes      = [System.IO.File]::ReadAllBytes($localFile)
$base64Content  = [Convert]::ToBase64String($fileBytes)
Write-Host "File read and encoded ($($fileBytes.Length) bytes)."

# ---------------------------------------------------------------------------
# 5. PATCH the web resource with the new content
# ---------------------------------------------------------------------------
$patchUrl  = "$environmentUrl/api/data/v9.2/webresourceset($webResourceId)"
$patchBody = @{ content = $base64Content } | ConvertTo-Json -Compress

$patchHeaders = $headers.Clone()
$patchHeaders['Content-Type'] = 'application/json'
$patchHeaders['If-Match']     = '*'   # prevent accidental create

Write-Host "Updating web resource content..."

Invoke-RestMethod `
    -Uri     $patchUrl `
    -Method  Patch `
    -Headers $patchHeaders `
    -Body    $patchBody | Out-Null

Write-Host "Web resource content updated successfully."

# ---------------------------------------------------------------------------
# 6. Publish the web resource so the change takes effect
# ---------------------------------------------------------------------------
$publishUrl  = "$environmentUrl/api/data/v9.2/PublishXml"
$publishXml  = "<importexportxml><webresources><webresource>$webResourceId</webresource></webresources></importexportxml>"
$publishBody = @{ ParameterXml = $publishXml } | ConvertTo-Json -Compress

$publishHeaders = $headers.Clone()
$publishHeaders['Content-Type'] = 'application/json'

Write-Host "Publishing web resource..."

Invoke-RestMethod `
    -Uri     $publishUrl `
    -Method  Post `
    -Headers $publishHeaders `
    -Body    $publishBody | Out-Null

Write-Host "Web resource '$WebResourceUniqueName' published successfully."
