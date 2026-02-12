param(
  [Parameter(Mandatory = $true)]
  [string]$PfxPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if (-not (Test-Path $PfxPath)) {
  throw "PFX not found: $PfxPath"
}

$bytes = [System.IO.File]::ReadAllBytes((Resolve-Path $PfxPath))
$b64 = [System.Convert]::ToBase64String($bytes)

Write-Host ""
Write-Host "Use this value as a GitHub Secret (e.g. SIGNING_CERT_PFX_B64):"
Write-Host $b64

