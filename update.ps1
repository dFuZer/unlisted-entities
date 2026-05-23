# THIS SCRIPT COMPILES THE MOD AND UPDATES THE ASSET BUNDLES INSIDE THE WORKSHOP FOLDER FOR LOCAL TESTING.

# Before you can run this script, you need to build the mod in Debug (dotnet build), and update the config.json file with the correct paths.
# You also need to publish your mod on the workshop, subscribe to it, and update the config.json file with the correct workshop folder path.
# You also need to build the asset bundles and update the config.json file with the correct asset bundle paths.
# Then, rename EXAMPLE.config.json to config.json

# Check if config file exists
$configPath = Join-Path $PSScriptRoot "config.json"
if (!(Test-Path $configPath)) {
    Write-Error "Config file not found ! Please rename EXAMPLE.config.json to config.json and update the paths !"
    exit 1
}
$config = Get-Content $configPath | ConvertFrom-Json

# Get the paths from the config file
$workshopFolder = $config.WorkshopModFolder
$assetBundlePath = $config.AssetBundlePath
$debugDllPath = $config.DebugDllPath

# Build the mod (Debug)
dotnet build

# Check if the assetbundles, debug dll and workshop folder exist
if (!(Test-Path $workshopFolder)) {
    Write-Error "Workshop mod folder not found: $workshopFolder"
    exit 1
}
if (!(Test-Path $assetBundlePath)) {
    Write-Error "Asset bundle not found: $assetBundlePath"
    exit 1
}
if (!(Test-Path $debugDllPath)) {
    Write-Error "Debug DLL not found: $debugDllPath"
    exit 1
}

# Copy the debug dll and assetbundles to the workshop folder for local testing
Copy-Item $debugDllPath (Join-Path $workshopFolder (Split-Path $debugDllPath -Leaf)) -Force
Copy-Item $assetBundlePath (Join-Path $workshopFolder (Split-Path $assetBundlePath -Leaf)) -Force
