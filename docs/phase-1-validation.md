# Phase 1 Validation Record

## Source validation

- Solution contains `GeoLocalCAD.Core` and `GeoLocalCAD.Plugin`.
- `GeoLocalCAD.Core` builds successfully with Mono/xbuild in the Linux authoring environment (warning: Mono 6.8 does not provide a v4.8 targeting pack).
- Core has no Autodesk references.
- Plugin references Autodesk APIs through an explicit local path property.
- `GEOLOCAL` is implemented with `CommandMethod` and calls the active document editor.
- Logging is persisted under the user's local application data directory.

## Environment limitation

The authoring environment is Linux and does not contain AutoCAD/Civil 3D 2020 or its licensed managed API assemblies. Therefore this environment cannot truthfully mark the following as passed:

- Windows/.NET Framework 4.8 plugin build with Autodesk references.
- NETLOAD inside Civil 3D 2020.
- Runtime execution of `GEOLOCAL` inside Civil 3D 2020.

The project deliberately fails early with a clear build error when the local Autodesk API path is missing. The checklist in `README-INSTALL.md` is the required next validation step on a licensed Civil 3D 2020 machine.
