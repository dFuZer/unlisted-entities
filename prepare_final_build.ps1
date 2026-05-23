# THIS SCRIPT COMPILES THE MOD IN RELEASE MODE AND STAGES THE BUILD FOR THE WORKSHOP.
# Requirements: The SDK must be built in Release mode (dotnet build -c Release) and the Asset bundles must be built.
# Paths must be updated in config.json

$configPath = Join-Path $PSScriptRoot "config.json"
if (!(Test-Path $configPath)) {
    Write-Error "Config file not found ! Please rename EXAMPLE.config.json to config.json and update the paths !"
    exit 1
}
$config = Get-Content $configPath | ConvertFrom-Json

$releaseDllPath = $config.ReleaseDllPath
$assetBundlePath = $config.AssetBundlePath

dotnet build -c Release
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

if (!(Test-Path $releaseDllPath)) {
    Write-Error "Release DLL not found: $releaseDllPath"
    exit 1
}
if (!(Test-Path $assetBundlePath)) {
    Write-Error "Asset bundle not found: $assetBundlePath"
    exit 1
}

$steamBuildFolder = Join-Path $PSScriptRoot "steam-build\build"
if (!(Test-Path $steamBuildFolder)) {
    New-Item -ItemType Directory -Path $steamBuildFolder | Out-Null
}
Remove-Item (Join-Path $steamBuildFolder "*") -Recurse -Force -ErrorAction SilentlyContinue

Copy-Item $releaseDllPath (Join-Path $steamBuildFolder (Split-Path $releaseDllPath -Leaf)) -Force
Copy-Item $assetBundlePath (Join-Path $steamBuildFolder (Split-Path $assetBundlePath -Leaf)) -Force
