[CmdletBinding(SupportsShouldProcess=$true)]
param(
    [string]$AutodeskManagedApiPath = 'C:\Program Files\Autodesk\AutoCAD 2020',
    [string]$Civil3DManagedApiPath = 'C:\Program Files\Autodesk\AutoCAD 2020',
    [string]$GdalRoot = '',
    [string]$Configuration = 'Release',
    [string]$Version = '0.6.0-civil3d-candidate',
    [switch]$Install,
    [switch]$InstallBundle,
    [switch]$SkipCoreTests,
    [switch]$AllowMissingGdal
)
$ErrorActionPreference = 'Stop'; Set-StrictMode -Version Latest
$root = (Resolve-Path $PSScriptRoot).Path
$log = Join-Path $root "artifacts\final-build-$((Get-Date).ToString('yyyyMMdd-HHmmss')).log"
New-Item -ItemType Directory -Force (Join-Path $root 'artifacts') | Out-Null
function Say([string]$message) { $message | Tee-Object -FilePath $log -Append | Write-Host }
function Need([string]$path, [string]$description) { if (-not (Test-Path $path)) { throw "Missing $description: $path" } }
function Run([string]$exe, [string[]]$args) { Say "> $exe $($args -join ' ')"; & $exe @args 2>&1 | Tee-Object -FilePath $log -Append; if ($LASTEXITCODE -ne 0) { throw "$exe failed with exit code $LASTEXITCODE" } }

Say "GeoLocal CAD final Windows pipeline started."
Need (Join-Path $root 'GeoLocalCAD.sln') 'solution'
if (-not (Get-Command msbuild -ErrorAction SilentlyContinue)) { throw 'MSBuild was not found. Install Visual Studio .NET desktop workload and .NET Framework 4.8 Developer Pack.' }
Need (Join-Path $AutodeskManagedApiPath 'AcMgd.dll') 'AutoCAD AcMgd.dll'
Need (Join-Path $AutodeskManagedApiPath 'AcDbMgd.dll') 'AutoCAD AcDbMgd.dll'
Need (Join-Path $AutodeskManagedApiPath 'AcCoreMgd.dll') 'AutoCAD AcCoreMgd.dll'
Need (Join-Path $AutodeskManagedApiPath 'AdWindows.dll') 'AutoCAD AdWindows.dll'
Need (Join-Path $Civil3DManagedApiPath 'AeccDbMgd.dll') 'Civil 3D AeccDbMgd.dll'

$gdalTranslate = $null; $gdalInfo = $null
if ($GdalRoot) {
    $gdalTranslate = Join-Path $GdalRoot 'bin\gdal_translate.exe'; $gdalInfo = Join-Path $GdalRoot 'bin\gdalinfo.exe'
} else {
    $gdalTranslate = $env:GEOLOCAL_GDAL_TRANSLATE; $gdalInfo = $env:GEOLOCAL_GDALINFO
}
if (-not (Test-Path $gdalTranslate) -or -not (Test-Path $gdalInfo)) {
    if (-not $AllowMissingGdal) { throw 'GDAL Windows binaries were not found. Provide -GdalRoot C:\GeoLocalCAD\GDAL or use -AllowMissingGdal for a non-GEODEM candidate only.' }
    Say 'WARNING: GDAL is missing; this candidate cannot pass GEODEM acceptance.'
} else {
    $env:GEOLOCAL_GDAL_TRANSLATE = (Resolve-Path $gdalTranslate).Path
    $env:GEOLOCAL_GDALINFO = (Resolve-Path $gdalInfo).Path
    Run $env:GEOLOCAL_GDAL_TRANSLATE @('--version')
    Run $env:GEOLOCAL_GDALINFO @('--version')
}

Run 'msbuild' @((Join-Path $root 'GeoLocalCAD.sln'), '/t:Rebuild', "/p:Configuration=$Configuration", '/p:Platform=Any CPU', "/p:AutodeskManagedApiPath=$AutodeskManagedApiPath", "/p:Civil3DManagedApiPath=$Civil3DManagedApiPath", '/verbosity:minimal')
if (-not $SkipCoreTests) {
    $testProject = Join-Path $root 'tests\GeoLocalCAD.Core.Tests\GeoLocalCAD.Core.Tests.csproj'
    Run 'msbuild' @($testProject, "/p:Configuration=$Configuration", '/p:Platform=Any CPU', '/verbosity:minimal')
    $testExe = Join-Path $root "tests\GeoLocalCAD.Core.Tests\bin\$Configuration\GeoLocalCAD.Core.Tests.exe"
    if (Test-Path $testExe) { Run $testExe @() }
}

$packageScript = Join-Path $root 'scripts\build-release-package.ps1'
Need $packageScript 'release package builder'
Run 'powershell' @('-NoProfile','-ExecutionPolicy','Bypass','-File',$packageScript,'-AutodeskManagedApiPath',$AutodeskManagedApiPath,'-Civil3DManagedApiPath',$Civil3DManagedApiPath,'-GdalRoot',($GdalRoot),'-Configuration',($Configuration),'-Version',$Version)
$zip = Join-Path $root "artifacts\GeoLocalCAD_v$Version.zip"
Need $zip 'final release ZIP'
$extract = Join-Path $root "artifacts\install-test-$Version"
if (Test-Path $extract) { Remove-Item $extract -Recurse -Force }
Expand-Archive $zip $extract -Force
foreach ($name in @('GeoLocalCAD.dll','GeoLocalCAD.Core.dll','GeoLocalCAD.UI.dll','GeoLocalCAD.CRS.dll','GeoLocalCAD.Vector.dll','GeoLocalCAD.Raster.dll','GeoLocalCAD.DEM.dll','GeoLocalCAD.TestRunner.exe','install.ps1','PackageContents.xml')) { Need (Join-Path $extract $name) "release component $name" }
$forbidden = @('AcMgd.dll','AcDbMgd.dll','AcCoreMgd.dll','AdWindows.dll','AeccDbMgd.dll')
foreach ($name in $forbidden) { if (Get-ChildItem $extract -Recurse -Filter $name -ErrorAction SilentlyContinue) { throw "Forbidden Autodesk binary included in release: $name" } }
Say "Package validation passed: $zip"

if ($Install) {
    $installArgs = @('-NoProfile','-ExecutionPolicy','Bypass','-File',(Join-Path $extract 'install.ps1'))
    if ($GdalRoot) { $installArgs += @('-GdalRoot',$GdalRoot) }
    if ($InstallBundle) { $installArgs += '-InstallBundle' }
    Run 'powershell' $installArgs
}
Say "SUCCESS: final candidate created at $zip"
Say "Civil 3D acceptance still required: NETLOAD/ApplicationPlugins, GEOLOCAL, GEOIMPORTRASTER, GEODEM, save/reopen DWG."
