[CmdletBinding()]
param(
    [string]$AutodeskManagedApiPath = 'C:\Program Files\Autodesk\AutoCAD 2020',
    [string]$Civil3DManagedApiPath = 'C:\Program Files\Autodesk\AutoCAD 2020',
    [string]$GdalRoot = '',
    [string]$Configuration = 'Release',
    [switch]$SkipBuild,
    [string]$Version = '0.6.0'
)
$ErrorActionPreference = 'Stop'; Set-StrictMode -Version Latest
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$release = Join-Path $root "artifacts\GeoLocalCAD_v$Version"
if (-not $SkipBuild) {
    & msbuild (Join-Path $root 'GeoLocalCAD.sln') /t:Rebuild /p:Configuration=$Configuration /p:Platform='Any CPU' /p:AutodeskManagedApiPath="$AutodeskManagedApiPath" /p:Civil3DManagedApiPath="$Civil3DManagedApiPath"
    if ($LASTEXITCODE -ne 0) { throw 'Windows build failed.' }
}
New-Item -ItemType Directory -Force $release | Out-Null
$assemblies = @('GeoLocalCAD.dll','GeoLocalCAD.Core.dll','GeoLocalCAD.UI.dll','GeoLocalCAD.CRS.dll','GeoLocalCAD.Vector.dll','GeoLocalCAD.Raster.dll','GeoLocalCAD.DEM.dll')
$locations = @('Plugin','Core','UI','CRS','Vector','Raster','DEM')
for ($i=0; $i -lt $assemblies.Count; $i++) {
    $source = Join-Path $root "src\GeoLocalCAD.$($locations[$i])\bin\$Configuration\$($assemblies[$i])"
    if ($locations[$i] -eq 'Plugin') { $source = Join-Path $root "src\GeoLocalCAD.Plugin\bin\$Configuration\GeoLocalCAD.dll" }
    if (-not (Test-Path $source)) { throw "Missing build output: $source" }
    Copy-Item $source $release -Force
}
Copy-Item (Join-Path $root 'install.ps1'),(Join-Path $root 'PackageContents.xml'),(Join-Path $root 'release-manifest.json'),(Join-Path $root 'README-INSTALL.md'),(Join-Path $root 'CHANGELOG.md'),(Join-Path $root 'LICENSE') $release -Force
New-Item -ItemType Directory -Force (Join-Path $release 'GDAL') | Out-Null
Copy-Item (Join-Path $root 'GDAL\README-Windows.md') (Join-Path $release 'GDAL') -Force
if ($GdalRoot) {
    if (-not (Test-Path (Join-Path $GdalRoot 'bin\gdal_translate.exe'))) { throw 'GdalRoot does not contain bin\gdal_translate.exe.' }
    Copy-Item $GdalRoot (Join-Path $release 'GDAL') -Recurse -Force
}
$zip = Join-Path $root "artifacts\GeoLocalCAD_v$Version.zip"
if (Test-Path $zip) { Remove-Item $zip -Force }
Compress-Archive -Path (Join-Path $release '*') -DestinationPath $zip -Force
Write-Host "Release package created: $zip"
