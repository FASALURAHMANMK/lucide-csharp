param(
  [Parameter(Mandatory = $false)]
  [string]$Subject = "CN=Lucide.NET Package Signing (TEST)",

  [Parameter(Mandatory = $false)]
  [string]$OutDir = (Join-Path $PSScriptRoot "out"),

  [Parameter(Mandatory = $false)]
  [string]$BaseName = "lucide-net-signing",

  [Parameter(Mandatory = $false)]
  [securestring]$Password
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if (-not (Test-Path $OutDir)) {
  New-Item -ItemType Directory -Path $OutDir | Out-Null
}

$pfxPath = Join-Path $OutDir ($BaseName + ".pfx")
$cerPath = Join-Path $OutDir ($BaseName + ".cer")

if ($null -eq $Password) {
  $Password = Read-Host -Prompt "Enter PFX password" -AsSecureString
}

Write-Host "Creating test code-signing certificate in CurrentUser\\My..."

$cert = New-SelfSignedCertificate `
  -Subject $Subject `
  -FriendlyName "Lucide.NET Test Package Signing" `
  -Type CodeSigningCert `
  -KeyUsage DigitalSignature `
  -KeyAlgorithm RSA `
  -KeyLength 2048 `
  -HashAlgorithm SHA256 `
  -CertStoreLocation "Cert:\\CurrentUser\\My"

Write-Host "Exporting PFX to: $pfxPath"
Export-PfxCertificate -Cert $cert -FilePath $pfxPath -Password $Password | Out-Null

Write-Host "Exporting CER to: $cerPath"
Export-Certificate -Cert $cert -FilePath $cerPath | Out-Null

Write-Host ""
Write-Host "Done."
Write-Host "PFX (keep secret): $pfxPath"
Write-Host "CER (upload to NuGet): $cerPath"
Write-Host ""
Write-Warning "NuGet.org requires a CA-issued code-signing certificate. This self-signed cert is only for local/testing or private feeds you control."

