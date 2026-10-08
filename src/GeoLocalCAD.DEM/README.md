# GeoLocalCAD.DEM

Phase 6 Raster-to-DEM contracts. `DemGrid` converts valid raster elevation cells to geolocated `DemSample` values using the source `RasterGeoTransform`, preserves CRS and NoData policy, and exposes `Civil3DSurfaceRequest`/`IDemSurfaceBuilder` for the Autodesk-only Civil 3D adapter.

This module does not infer elevations from image colors and does not claim to create a Civil 3D surface until a GDAL-backed pixel sampler and a Civil 3D 2020 `TinSurface` adapter are implemented and tested on Windows.
