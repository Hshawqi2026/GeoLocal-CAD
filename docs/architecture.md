# Architecture

```text
GeoLocalCAD.Plugin (AutoCAD/Civil 3D adapter)
        |
GeoLocalCAD.Core (logging, shared contracts; no Autodesk dependency)
        |
Future modules: CRS / Raster / Vector / DEM / Civil3D / UI / Storage
```

The adapter boundary prevents Autodesk API types from leaking into the core engine. Future modules should expose testable services and leave command/UI code thin.
