# Changelog

## [Unreleased] - Phase 6 planning bridge

### Added
- `GeoLocalCAD.DEM` contracts for geolocated elevation grids, NoData filtering, and Civil 3D surface requests.
- Phase 6 plan for Raster-to-DEM-to-TIN Surface integration.

### Not yet claimed complete
- GDAL-backed elevation pixel sampling.
- Civil 3D 2020 `TinSurface` adapter and `GEODEM` runtime command.

## [0.5.0] - 2026-10-08

### Added
- `GeoLocalCAD.Raster` metadata and geotransform engine.
- PNG, JPEG, BMP, TIFF/GeoTIFF dimension and metadata reading.
- PGW, JGW, TFW, WLD, and WORLD file support.
- Pixel-to-world, world-to-pixel, rotated extent, and georeferencing validation.
- `GEOIMPORTRASTER` AutoCAD RasterImage insertion command.
- Phase 5 Raster smoke tests and Civil 3D acceptance checklist.

### Limitations
- Full pixel-band/NoData sampling through GDAL remains scheduled for Raster/DEM expansion.
- Civil 3D runtime validation requires a licensed Windows Civil 3D 2020 environment.

## [0.4.0] - 2026-10-05

### Added
- Coordinate inspection Core contracts and validation.
- `GEOINFO` command for DBPoint, Line, Polyline, and Circle representative coordinates.
- WPF Coordinate Inspector with CRS status, geographic conversion status, warnings, and clipboard copy.
- Phase 4 Civil 3D validation documentation.

## [0.3.0] - 2026-10-05

### Added
- Explicit EPSG CRS model and PROJ `cs2cs` transformation adapter.
- GeoJSON FeatureCollection reader/writer for Point, LineString, and Polygon geometries.
- `GEOIMPORT` and `GEOEXPORT` AutoCAD commands with explicit CRS prompts.
- Phase 3 smoke tests for CRS parsing, UTM transformation, GeoJSON round-trip, attributes, and Z values.

### Not included
- Autodesk DLLs or a Windows PROJ runtime.
- Shapefile, GeoPackage, raster, DEM, or Civil 3D surface integration.

## [0.2.0] - 2026-10-05

### Added
- GeoLocal Ribbon tab with Phase 2 sections.
- WPF settings window and `GEOSETTINGS` command.
- Local XML settings persistence with explicit drawing CRS and Offline mode.
- Autodesk `AdWindows.dll` build-time reference without redistribution.

## [0.1.0] - 2026-10-05

### Added
- Initial Phase 0 architecture and GitHub-ready solution.
- Core file logger with startup/command/error events.
- Civil 3D 2020 adapter entry point.
- `GEOLOCAL` command with actionable error reporting.
- Build validation for required Autodesk managed API references.
- Installation and runtime validation documentation.

### Not included
- Autodesk proprietary assemblies.
- CRS, raster, vector, DEM, georeferencing, ribbon, or Civil 3D data features.
