# MANUS.md — GeoLocal CAD Project Rules

## Architecture
- `GeoLocalCAD.Core` has no Autodesk references and remains independently testable.
- `GeoLocalCAD.Plugin` is the Autodesk adapter and owns commands/entry point.
- Future GIS modules remain isolated behind explicit interfaces.

## Coding Rules
- C#/.NET Framework 4.8 for the Civil 3D 2020 target.
- No hardcoded coordinates, CRS assumptions, dummy commands, or silent transformations.
- User-visible errors must explain what happened, where, likely cause, and next action.

## Build Rules
- Build with the licensed AutoCAD/Civil 3D 2020 managed API installed locally.
- Set `AutodeskManagedApiPath`; never copy Autodesk DLLs into Git or releases.
- A release requires a successful build and a review of the produced assembly dependencies.

## Testing Rules
- Each completed module must include focused tests and regression tests for fixed bugs.
- Civil 3D integration is only marked passed with evidence from Civil 3D 2020.
- Linux source validation is not a substitute for Civil 3D runtime validation.

## Git Rules
- Small focused commits using conventional messages (`feat:`, `fix:`, `test:`, `docs:`).
- Stable releases use annotated semantic-version tags and a ZIP artifact.

## Autodesk dependency rules
Autodesk assemblies are referenced from the user's licensed installation only and are excluded from source and release packages.

## CRS rules
No CRS is inferred from geography. Source/target CRS must be explicit or read from authoritative data, and transformations must never be silent.

## Offline rules
The baseline must not call web map APIs or download tiles. Local data sources only.

## Error handling
Log startup, shutdown, commands, imports/exports, transformations, performance warnings, exceptions, and failures with UTC timestamps.

## Release rules
Each release includes `README-INSTALL.md`, `CHANGELOG.md`, `LICENSE`, configuration where applicable, and only redistributable managed/native dependencies.
