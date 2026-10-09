# Release Status

## Current state

The repository has a Windows-oriented installer and ApplicationPlugins bundle manifest. The package builder requires a licensed Civil 3D 2020 build machine to produce `GeoLocalCAD.dll` and the complete runnable ZIP.

The current Sandbox can build and test Core, CRS, Raster, DEM, and the GDAL sampler, but it cannot produce or validate the Autodesk plugin because `AcMgd.dll`, `AcDbMgd.dll`, `AcCoreMgd.dll`, `AdWindows.dll`, and `AeccDbMgd.dll` are not present. It also cannot generate or validate a CUIX through AutoCAD CUI Editor.

## Final-version decision

This is **not yet `v1.0.0`**. It is a release-preparation/source-validation state. `v1.0.0` requires all of the following evidence:

1. Windows Release build produces `GeoLocalCAD.dll` and all managed dependencies.
2. No Autodesk DLL is copied into the package.
3. `NETLOAD` or the ApplicationPlugins bundle loads inside Civil 3D 2020.
4. `GEOLOCAL`, `GEOIMPORTRASTER`, and `GEODEM` execute in Civil 3D.
5. GDAL Windows binaries are validated and `GEODEM` creates a real `TinSurface`.
6. The CUIX, if included, loads through `CUILOAD` and every macro reaches a real command.
7. The saved DWG is reopened and the surface/metadata remain valid.

The installer intentionally fails or warns instead of pretending that missing Windows/Civil 3D artifacts are present.
