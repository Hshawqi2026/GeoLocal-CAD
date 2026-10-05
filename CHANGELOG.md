# Changelog

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
