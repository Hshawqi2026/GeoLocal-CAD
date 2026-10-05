# Phase 3 — CRS Engine and Geospatial Import/Export

## Scope

Phase 3 adds an explicit CRS model, a PROJ-backed transformation adapter, and offline GeoJSON import/export. The implementation deliberately does not infer a CRS from the drawing location or from Yemen/project geography.

## Implemented

`GeoLocalCAD.CRS` validates explicit `EPSG:<code>` identifiers and exposes `Coordinate` and `ICoordinateTransformer`. `ProjCliTransformer` executes the local PROJ `cs2cs` runtime and fails with an actionable error when the executable or CRS operation is unavailable. Axis handling is explicit for common geographic EPSG definitions so application XY coordinates remain longitude/easting, latitude/northing.

`GeoLocalCAD.Vector` supports GeoJSON FeatureCollections with Point, LineString, and Polygon geometries, attributes, Z values, and an explicit `geolocalCrs` export metadata member. Reader and writer preserve real coordinates and stop when a required CRS or transformation engine is missing.

The AutoCAD adapter now provides `GEOIMPORT` and `GEOEXPORT`. Import asks for the file and source CRS, then creates Point/Polyline entities. Export asks for a selection, output path, and drawing CRS, then writes supported Point/Polyline entities to GeoJSON. Unsupported entity types are skipped and the command reports the count.

## PROJ runtime

The plugin does not download anything. On Windows, set the `GEOLOCAL_PROJ_CS2CS` environment variable to the bundled or installed offline `cs2cs.exe` path before using a transformation between different CRS definitions. If source and target CRS are equal, no external transformation is needed. A future release may package the matching Windows PROJ runtime after license and architecture review.

## Tests completed in the Linux authoring environment

The Core test executable builds and passes settings persistence, CRS identifier canonicalization, GeoJSON round-trip with attributes, and WGS84 (`EPSG:4326`) to UTM Zone 38N (`EPSG:32638`) using PROJ 9.4.0. This is not a substitute for AutoCAD Civil 3D runtime testing.

## Civil 3D validation checklist

- [ ] Build with licensed AutoCAD/Civil 3D 2020 managed APIs and the Phase 3 project references.
- [ ] Configure an offline Windows PROJ `cs2cs.exe` and set `GEOLOCAL_PROJ_CS2CS` when transformations are required.
- [ ] NETLOAD `GeoLocalCAD.dll`.
- [ ] Run `GEOIMPORT` with a GeoJSON FeatureCollection and explicit source EPSG.
- [ ] Confirm Point, LineString, and Polygon entities are created at their real coordinates.
- [ ] Run `GEOEXPORT` on supported entities with an explicit drawing EPSG.
- [ ] Open the output GeoJSON and verify `geolocalCrs`, geometry, Z values, and attributes.
- [ ] Test a source/target CRS mismatch and confirm the operation either uses PROJ or stops with a visible error; no silent transformation is acceptable.
