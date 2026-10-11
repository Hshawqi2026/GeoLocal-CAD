[CmdletBinding()]
param(
  [string]$Configuration = 'Release',
  [string]$Version = '0.6.0',
  [string]$AutodeskManagedApiPath = 'C:\Program Files\Autodesk\AutoCAD 2020',
  [string]$Civil3DManagedApiPath = 'C:\Program Files\Autodesk\AutoCAD 2020',
  [string]$GdalRoot = ''
)
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$bundleStage = Join-Path $root 'artifacts\GeoLocalCAD.bundle'
$resourceDir = Join-Path $root 'src\GeoLocalCAD.Installer\Resources'
$installerOut = Join-Path $root 'artifacts\installer'
Remove-Item $bundleStage,$installerOut -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force $bundleStage,$resourceDir,$installerOut | Out-Null
& msbuild (Join-Path $root 'GeoLocalCAD.sln') /t:Rebuild /p:Configuration=$Configuration /p:Platform='Any CPU' /p:AutodeskManagedApiPath="$AutodeskManagedApiPath" /p:Civil3DManagedApiPath="$Civil3DManagedApiPath"
if ($LASTEXITCODE -ne 0) { throw 'GeoLocalCAD solution build failed.' }
Copy-Item (Join-Path $root 'PackageContents.xml') $bundleStage -Force
foreach ($name in @('GeoLocalCAD.dll','GeoLocalCAD.Core.dll','GeoLocalCAD.UI.dll','GeoLocalCAD.CRS.dll','GeoLocalCAD.Vector.dll','GeoLocalCAD.Raster.dll','GeoLocalCAD.DEM.dll')) {
  $source = if ($name -eq 'GeoLocalCAD.dll') { Join-Path $root "src\GeoLocalCAD.Plugin\bin\$Configuration\$name" } else { Join-Path $root "src\GeoLocalCAD.$($name -replace '^GeoLocalCAD\.','' -replace '\.dll$','')\bin\$Configuration\$name" }
  if (-not (Test-Path $source)) { throw "Missing build output: $source" }
  Copy-Item $source $bundleStage -Force
}
$bundleZip = Join-Path $resourceDir 'GeoLocalCAD.bundle.zip'
Remove-Item $bundleZip -Force -ErrorAction SilentlyContinue
Compress-Archive -Path $bundleStage -DestinationPath $bundleZip -Force
& dotnet publish (Join-Path $root 'src\GeoLocalCAD.Installer\GeoLocalCAD.Installer.csproj') -c $Configuration -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o $installerOut
if ($LASTEXITCODE -ne 0) { throw 'Installer publish failed.' }
$final = Join-Path $root "artifacts\GeoLocalCAD-Installer-v$Version.exe"
Copy-Item (Join-Path $installerOut 'GeoLocalCADInstaller.exe') $final -Force
Write-Host "Installer created: $final"
