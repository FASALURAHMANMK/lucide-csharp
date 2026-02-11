param(
    [string]$Destination = (Join-Path $PSScriptRoot 'lucide'),
    [string]$SourcePath,
    [string]$Version = 'main'
)

function Copy-Icons($iconsPath, $destination) {
    if (-not (Test-Path $iconsPath)) {
        throw "Icons path not found: $iconsPath"
    }

    $svgFiles = Get-ChildItem -Path $iconsPath -Filter *.svg -Recurse
    if (-not $svgFiles) {
        throw "No .svg files found under $iconsPath"
    }

    if (Test-Path $destination) {
        Remove-Item -Path $destination -Recurse -Force
    }
    New-Item -ItemType Directory -Force -Path $destination | Out-Null

    foreach ($file in $svgFiles) {
        Copy-Item -Path $file.FullName -Destination $destination -Force
    }

    Write-Host "Copied $($svgFiles.Count) icons to $destination" -ForegroundColor Green
}

if ($SourcePath) {
    $resolvedSource = Resolve-Path $SourcePath
    $iconsPath = Join-Path $resolvedSource 'icons'
    if (Test-Path $iconsPath) {
        Copy-Icons $iconsPath $Destination
    } else {
        Copy-Icons $resolvedSource $Destination
    }
    exit 0
}

$zipUrl = "https://github.com/lucide-icons/lucide/archive/refs/heads/$Version.zip"
$tempRoot = Join-Path ([IO.Path]::GetTempPath()) ("lucide_" + [Guid]::NewGuid())
$zipPath = Join-Path $tempRoot 'lucide.zip'

New-Item -ItemType Directory -Force -Path $tempRoot | Out-Null

try {
    Write-Host "Downloading Lucide icons from $zipUrl" -ForegroundColor Cyan
    Invoke-WebRequest -Uri $zipUrl -OutFile $zipPath
    Expand-Archive -Path $zipPath -DestinationPath $tempRoot

    $repoDir = Get-ChildItem -Path $tempRoot -Directory | Where-Object { $_.Name -like 'lucide-*' } | Select-Object -First 1
    if (-not $repoDir) {
        throw 'Failed to locate extracted Lucide repository.'
    }

    $iconsPath = Join-Path $repoDir.FullName 'icons'
    Copy-Icons $iconsPath $Destination
} finally {
    if (Test-Path $tempRoot) {
        Remove-Item -Path $tempRoot -Recurse -Force
    }
}
