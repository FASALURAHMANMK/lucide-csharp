param(
    [Parameter(Mandatory = $true)]
    [string]$Version
)

$semverPattern = '^\d+\.\d+\.\d+(-[0-9A-Za-z.-]+)?(\+[0-9A-Za-z.-]+)?$'
if ($Version -notmatch $semverPattern) {
    throw "Version '$Version' is not a valid semver (e.g., 1.2.3, 1.2.3-beta.1, 1.2.3+meta)."
}

$baseVersion = ($Version -split '[-+]')[0]
$parts = $baseVersion.Split('.')
$assemblyVersion = "{0}.{1}.{2}.0" -f $parts[0], $parts[1], $parts[2]

$versionPath = Join-Path $PSScriptRoot 'version.json'
if (-not (Test-Path $versionPath)) {
    throw "version.json not found at $versionPath"
}

$versionJson = Get-Content -Path $versionPath -Raw | ConvertFrom-Json
if (-not $versionJson) {
    $versionJson = [pscustomobject]@{}
}

$versionJson.version = $Version
$versionJson | ConvertTo-Json -Depth 10 | Set-Content -Path $versionPath

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$propsPath = Join-Path $repoRoot 'Directory.Version.props'

@"
<Project>
  <PropertyGroup>
    <PackageVersion>$Version</PackageVersion>
    <AssemblyVersion>$assemblyVersion</AssemblyVersion>
    <FileVersion>$assemblyVersion</FileVersion>
    <InformationalVersion>$Version</InformationalVersion>
    <InformationalVersion Condition="'`$(SourceRevisionId)' != ''">$Version+`$(SourceRevisionId)</InformationalVersion>
  </PropertyGroup>
</Project>
"@ | Set-Content -Path $propsPath

Write-Host "Updated version.json and Directory.Version.props to $Version" -ForegroundColor Green
