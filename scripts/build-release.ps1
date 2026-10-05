param(
  [string]$AutodeskManagedApiPath = "C:\Program Files\Autodesk\AutoCAD 2020",
  [string]$Configuration = "Release"
)
$ErrorActionPreference = "Stop"
msbuild "$PSScriptRoot\..\GeoLocalCAD.sln" /t:Rebuild /p:Configuration=$Configuration /p:AutodeskManagedApiPath="$AutodeskManagedApiPath"
$release = Join-Path $PSScriptRoot "..\artifacts\GeoLocalCAD_v0.3.0"
New-Item -ItemType Directory -Force $release | Out-Null
Copy-Item "$PSScriptRoot\..\src\GeoLocalCAD.Plugin\bin\$Configuration\GeoLocalCAD.dll" $release
Copy-Item "$PSScriptRoot\..\src\GeoLocalCAD.Core\bin\$Configuration\GeoLocalCAD.Core.dll" $release
Copy-Item "$PSScriptRoot\..\src\GeoLocalCAD.UI\bin\$Configuration\GeoLocalCAD.UI.dll" $release
Copy-Item "$PSScriptRoot\..\src\GeoLocalCAD.CRS\bin\$Configuration\GeoLocalCAD.CRS.dll" $release
Copy-Item "$PSScriptRoot\..\src\GeoLocalCAD.Vector\bin\$Configuration\GeoLocalCAD.Vector.dll" $release
Copy-Item "$PSScriptRoot\..\README-INSTALL.md", "$PSScriptRoot\..\CHANGELOG.md", "$PSScriptRoot\..\LICENSE" $release
Compress-Archive -Path "$release\*" -DestinationPath "$PSScriptRoot\..\artifacts\GeoLocalCAD_v0.3.0.zip" -Force
Write-Host "Release created: artifacts\GeoLocalCAD_v0.3.0.zip"
