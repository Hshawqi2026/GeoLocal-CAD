# GeoLocal CAD

**GeoLocal CAD** is an offline-first, modular GIS/Survey/CRS plugin foundation for AutoCAD Civil 3D 2020.

## Current status — v0.5.0 / Phase 5

Implemented and documented:

- Modular solution boundaries.
- `.NET Framework 4.8` project targeting.
- Core logging independent of Autodesk APIs.
- Autodesk adapter with `IExtensionApplication`.
- Real `GEOLOCAL` command registered with `CommandMethod`.
- Build-time validation that AutoCAD 2020 managed API DLLs exist locally.
- No Autodesk DLLs included in source or release packages.
- GeoLocal Ribbon tab with Phase 2 sections.
- WPF settings window through `GEOSETTINGS`.
- Local XML settings persistence with explicit Offline mode and drawing CRS.
- Explicit EPSG CRS identifiers and PROJ-backed coordinate transformation.
- Offline GeoJSON import/export for Point, LineString, and Polygon features.
- `GEOIMPORT` and `GEOEXPORT` commands with CRS validation.
- `GEOINFO` Coordinate Inspector for DBPoint, Line, Polyline, and Circle.
- WPF coordinate inspection window with CRS status and clipboard copy.
- Raster metadata engine for PNG, JPEG, BMP, TIFF/GeoTIFF, and World Files.
- `GEOIMPORTRASTER` with geotransform validation and real RasterImage placement.
- Phase 6 DEM bridge contracts for converting georeferenced Raster elevation samples into a Civil 3D TIN Surface adapter.

Not claimed complete: Shapefile/GeoPackage, GDAL pixel sampling, Civil 3D TinSurface adapter, georeferencing, contours, advanced DEM analysis, or offline tile rendering. The Phase 6 bridge contracts are implemented, while runtime Surface creation still requires Windows Civil 3D 2020 validation.

## Build on a licensed Civil 3D 2020 machine

From a Developer Command Prompt or MSBuild-capable shell:

```powershell
msbuild GeoLocalCAD.sln /p:Configuration=Release /p:AutodeskManagedApiPath="C:\Program Files\Autodesk\AutoCAD 2020"
```

The output is `src\\GeoLocalCAD.Plugin\\bin\\Release\\GeoLocalCAD.dll` plus `GeoLocalCAD.Core.dll` and `GeoLocalCAD.UI.dll`.

## Install and test

See [README-INSTALL.md](README-INSTALL.md) for NETLOAD instructions and the exact validation checklist.

Phase 2 validation steps are in [docs/phase-2.md](docs/phase-2.md), Phase 3 steps are in [docs/phase-3.md](docs/phase-3.md), Phase 4 implementation/validation is in [docs/phase-4.md](docs/phase-4.md), and Phase 5 is in [docs/phase-5.md](docs/phase-5.md).
The Phase 6 Raster-to-DEM and Civil 3D Surface plan is in [docs/phase-6.md](docs/phase-6.md).

The proposed Phase 4 plan is in [docs/phase-4-plan.md](docs/phase-4-plan.md). Windows build and Civil 3D validation steps are in [docs/windows-test-guide.md](docs/windows-test-guide.md).

The complete production roadmap for `v1.0.0` is in [docs/final-production-roadmap.md](docs/final-production-roadmap.md). It covers Raster, Vector, Georeferencing, DEM, Civil 3D integration, Offline Maps, testing, and production release gates.

## License and third-party policy

The project is MIT-licensed. Autodesk assemblies are proprietary and must be supplied by the installed licensed product; they are not redistributed. Future PROJ/GDAL/GEOS/SQLite dependencies will be documented with their upstream licenses before inclusion.
