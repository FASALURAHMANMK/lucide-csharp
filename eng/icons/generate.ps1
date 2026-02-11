param(
    [string]$InputPath = (Join-Path $PSScriptRoot 'lucide'),
    [string]$OutputPath = (Resolve-Path (Join-Path $PSScriptRoot '..\..')),
    [string]$Compress = 'gzip'
)

$generatorProject = Join-Path $PSScriptRoot 'generator\Lucide.Generator\Lucide.Generator.csproj'
if (-not (Test-Path $generatorProject)) {
    throw "Generator project not found at $generatorProject"
}

if (-not (Test-Path $InputPath)) {
    throw "Input folder not found: $InputPath"
}

Write-Host "Running generator..." -ForegroundColor Cyan

dotnet run --project $generatorProject -- --input $InputPath --output $OutputPath --compress $Compress
if ($LASTEXITCODE -ne 0) {
    throw "Generator failed with exit code $LASTEXITCODE"
}

$expected = @(
    'src/Lucide.NET.Core/Generated/LucideIconKind.g.cs',
    'src/Lucide.NET.Core/Generated/LucideAliases.g.cs',
    'src/Lucide.NET.Core/Generated/LucideIconIndex.g.cs',
    'src/Lucide.NET.Core/Resources/lucide_icons.bin'
)

foreach ($relative in $expected) {
    $path = Join-Path $OutputPath $relative
    if (-not (Test-Path $path)) {
        throw "Expected generated file missing: $path"
    }
}

Write-Host "Generation complete." -ForegroundColor Green
