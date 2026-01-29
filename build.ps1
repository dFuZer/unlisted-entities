$config = Get-Content "config.json" | ConvertFrom-Json

$sourceFolder = $config.SourceFolder
$destinationZip = $config.DestinationZip
$assetBundlePath = $config.AssetBundlePath
$buildDllPath = $config.BuildDllPath

dotnet build

# Check if source exists
if (!(Test-Path $sourceFolder)) { 
    Write-Error "Source folder not found."; exit 
}

# Temporary copies
$tempFileDest = Join-Path $sourceFolder (Split-Path $assetBundlePath -Leaf)
Copy-Item $assetBundlePath $tempFileDest -Force

$buildTempFileDest = Join-Path $sourceFolder (Split-Path $buildDllPath -Leaf)
Copy-Item $buildDllPath $buildTempFileDest -Force

# Create archive
Compress-Archive -Path "$sourceFolder\*" -DestinationPath $destinationZip -Force

# Clean up
Remove-Item $tempFileDest, $buildTempFileDest -Force