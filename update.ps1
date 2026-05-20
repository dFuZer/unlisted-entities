$configPath = Join-Path $PSScriptRoot "config.json"
$config = Get-Content $configPath | ConvertFrom-Json

$workshopFolder = $config.WorkshopModFolder
$assetBundlePath = $config.AssetBundlePath
$buildDllPath = $config.BuildDllPath

dotnet build

if (!(Test-Path $workshopFolder)) {
    Write-Error "Workshop mod folder not found: $workshopFolder"
    exit 1
}
if (!(Test-Path $assetBundlePath)) {
    Write-Error "Asset bundle not found: $assetBundlePath"
    exit 1
}
if (!(Test-Path $buildDllPath)) {
    Write-Error "Build DLL not found: $buildDllPath"
    exit 1
}

Copy-Item $buildDllPath (Join-Path $workshopFolder (Split-Path $buildDllPath -Leaf)) -Force
Copy-Item $assetBundlePath (Join-Path $workshopFolder (Split-Path $assetBundlePath -Leaf)) -Force
