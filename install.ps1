[CmdletBinding(SupportsShouldProcess=$true)]
param(
    [string]$InstallRoot = "$env:ProgramData\GeoLocalCAD",
    [string]$GdalRoot = "",
    [switch]$InstallBundle,
    [switch]$InstallCuiX,
    [string]$CuiXPath = ""
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$packageRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$manifestPath = Join-Path $packageRoot 'release-manifest.json'
if (-not (Test-Path $manifestPath)) { throw "release-manifest.json is missing from the package." }
$manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
$required = @('GeoLocalCAD.Core.dll','GeoLocalCAD.CRS.dll','GeoLocalCAD.Raster.dll','GeoLocalCAD.DEM.dll','GeoLocalCAD.Vector.dll')
foreach ($name in $required) { if (-not (Test-Path (Join-Path $packageRoot $name))) { throw "Required managed assembly is missing: $name" } }

$destination = [IO.Path]::GetFullPath($InstallRoot)
if ($PSCmdlet.ShouldProcess($destination, 'Install GeoLocal CAD files')) {
    New-Item -ItemType Directory -Force -Path $destination | Out-Null
    Copy-Item (Join-Path $packageRoot '*.dll') $destination -Force
    Copy-Item (Join-Path $packageRoot 'README-INSTALL.md') $destination -Force -ErrorAction SilentlyContinue
    Copy-Item (Join-Path $packageRoot 'CHANGELOG.md') $destination -Force -ErrorAction SilentlyContinue
    Copy-Item $manifestPath $destination -Force
}

$gdalBin = $null
if ($GdalRoot) { $gdalBin = Join-Path ([IO.Path]::GetFullPath($GdalRoot)) 'bin' }
elseif (Test-Path (Join-Path $packageRoot 'GDAL\bin')) { $gdalBin = Join-Path $packageRoot 'GDAL\bin' }
if ($gdalBin) {
    $translate = Join-Path $gdalBin 'gdal_translate.exe'; $info = Join-Path $gdalBin 'gdalinfo.exe'
    if (-not (Test-Path $translate) -or -not (Test-Path $info)) { throw "GDAL root must contain bin\gdal_translate.exe and bin\gdalinfo.exe." }
    [Environment]::SetEnvironmentVariable('GEOLOCAL_GDAL_TRANSLATE', $translate, 'User')
    [Environment]::SetEnvironmentVariable('GEOLOCAL_GDALINFO', $info, 'User')
    Write-Host "GDAL configured: $gdalBin"
} else {
    Write-Warning 'GDAL Windows binaries were not found. GEODEM will remain unavailable until GEOLOCAL_GDAL_TRANSLATE and GEOLOCAL_GDALINFO are configured.'
}

$bundleRoot = Join-Path $env:ProgramData 'Autodesk\ApplicationPlugins\GeoLocalCAD.bundle'
if ($InstallBundle) {
    if (-not (Test-Path (Join-Path $packageRoot 'GeoLocalCAD.dll'))) { throw 'GeoLocalCAD.dll is missing; run the Windows build before installing the bundle.' }
    if ($PSCmdlet.ShouldProcess($bundleRoot, 'Install Autodesk ApplicationPlugins bundle')) {
        New-Item -ItemType Directory -Force -Path $bundleRoot | Out-Null
        Copy-Item (Join-Path $packageRoot '*.dll') $bundleRoot -Force
        Copy-Item (Join-Path $packageRoot 'PackageContents.xml') $bundleRoot -Force
        if (Test-Path (Join-Path $packageRoot 'GeoLocalCAD.cuix')) { Copy-Item (Join-Path $packageRoot 'GeoLocalCAD.cuix') $bundleRoot -Force }
        Write-Host "Autodesk bundle installed: $bundleRoot"
    }
}
if ($InstallCuiX) {
    if (-not $CuiXPath) { $CuiXPath = Join-Path $packageRoot 'GeoLocalCAD.cuix' }
    if (-not (Test-Path $CuiXPath)) { throw "CUIX file not found: $CuiXPath" }
    Copy-Item $CuiXPath $destination -Force
    Write-Host "CUIX copied to: $destination"
}

Write-Host "GeoLocal CAD package version $($manifest.version) installed at $destination"
if (-not (Test-Path (Join-Path $packageRoot 'GeoLocalCAD.dll'))) { Write-Warning 'This is a source-validation package: GeoLocalCAD.dll was not built in the current environment. NETLOAD/GEODEM require a Windows Civil 3D build.' }
