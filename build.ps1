param(
    [string]$Configuration = "Release",
    [string]$GameName = "GTFO",
    [string]$Profile = "Easy Mode DEV"
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

# The DLL/folder name. Thunderstore's manifest "name" can't contain a period, so that one differs.
$displayName = "GTFO.CrashTestDummies"
$manifest = Get-Content "$root\package\manifest.json" -Raw | ConvertFrom-Json
$version = $manifest.version_number

Write-Host "Building $displayName v$version (configuration: $Configuration, profile: $GameName - $Profile)..."
dotnet build "$root\src\$displayName\$displayName.csproj" -c $Configuration -p:GameName=$GameName -p:Profile=$Profile
if ($LASTEXITCODE -ne 0) { throw "Build failed." }

$bepInEx = Join-Path $env:AppData "Thunderstore Mod Manager\DataFolder\$GameName\profiles\$GameName - $Profile\BepInEx"
$builtDll = Join-Path $bepInEx "plugins\$displayName\$displayName.dll"
if (-not (Test-Path $builtDll)) { throw "Could not find built DLL at $builtDll. Check -Profile matches your mod manager profile." }

Copy-Item $builtDll "$root\package\$displayName.dll" -Force

# GTFO.CrashTestDummies.Core.dll is a runtime dependency of the plugin, so it has to ship in the same
# package or the mod throws a FileNotFoundException the moment it loads.
Get-ChildItem (Split-Path $builtDll) -Filter "*.dll" | Where-Object { $_.Name -ne "$displayName.dll" } | ForEach-Object {
    Copy-Item $_.FullName "$root\package\$($_.Name)" -Force
}

$distDir = Join-Path $root "dist"
New-Item -ItemType Directory -Path $distDir -Force | Out-Null
$zipPath = Join-Path $distDir "$displayName-$version.zip"
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }

Compress-Archive -Path "$root\package\*" -DestinationPath $zipPath
Write-Host "Packaged: $zipPath (Thunderstore package name: $($manifest.name))"
