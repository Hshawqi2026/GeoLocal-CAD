# GDAL Windows dependency

The production Windows package must contain a redistributable GDAL Windows build under:

```text
GDAL\bin\gdal_translate.exe
GDAL\bin\gdalinfo.exe
```

The current Sandbox cannot produce Windows GDAL binaries. Do not copy Linux `/usr/bin/gdal_translate` or `/usr/bin/gdalinfo` into a Windows release. Obtain and validate a Windows GDAL distribution compatible with the project's redistribution license, then run:

```powershell
.\install.ps1 -GdalRoot .\GDAL
```

The installer sets user-level environment variables:

```text
GEOLOCAL_GDAL_TRANSLATE
GEOLOCAL_GDALINFO
```

The installer does not download from the internet and does not enable online map APIs. Verify `gdal_translate.exe --version` and `gdalinfo.exe --version` before testing `GEODEM`.
