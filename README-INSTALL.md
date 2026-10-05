# GeoLocal CAD v0.2.0 — Installation and Phase 2 Validation

## Requirements

- AutoCAD Civil 3D 2020, 64-bit, fully installed and licensed.
- The release ZIP extracted to a writable local folder.
- No Autodesk DLLs are included; Civil 3D provides them at runtime.

## Install with NETLOAD

1. Start Civil 3D 2020.
2. Open a drawing or create a new drawing.
3. Enter `NETLOAD` in the command line.
4. Browse to `GeoLocalCAD.dll` from the extracted release folder.
5. Select **Load**. If a security warning appears, use a trusted local folder configured in Civil 3D's trusted locations; do not disable security globally.
6. Enter `GEOLOCAL` and press Enter.
7. Confirm the command reports `GeoLocal CAD v0.2.0 loaded successfully` and displays the log path.
8. Confirm the `GeoLocal` Ribbon tab appears.
9. Enter `GEOSETTINGS`, change Offline mode or the explicit Drawing CRS, save, and reopen the command to verify persistence.

## Expected log

The plugin writes to `%LOCALAPPDATA%\\GeoLocalCAD\\Logs\\GeoLocalCAD.log` and records startup, shutdown, command execution, and exceptions with UTC timestamps.

## Validation checklist

- [ ] NETLOAD completes without an assembly-load error.
- [ ] `GEOLOCAL` is recognized as a command.
- [ ] The command reports the loaded version.
- [ ] A log file is created and contains startup and command entries.
- [ ] Unloading/reloading does not copy or require Autodesk DLLs from the release folder.
- [ ] The `GeoLocal` Ribbon tab appears with its Phase 2 sections.
- [ ] `GEOSETTINGS` opens the WPF settings window.
- [ ] Settings persist at `%LOCALAPPDATA%\\GeoLocalCAD\\settings.xml`.

## Troubleshooting

- **AutoCAD managed API not found during build:** set `AutodeskManagedApiPath` to the installed AutoCAD/Civil 3D 2020 folder.
- **NETLOAD cannot load the DLL:** verify the DLL was built for .NET Framework 4.8 and that it is not blocked by Windows file security.
- **GEOLOCAL is unknown:** unload/reload the exact `GeoLocalCAD.dll` from the release folder and inspect the log.
- **Runtime exception:** collect the command-line text and the log file; do not claim Phase 2 passed until the evidence is reviewed.

## Important limitation

The repository was prepared without redistributing Autodesk binaries. A build and runtime test inside Civil 3D 2020 requires a licensed Windows/Civil 3D environment. This package does not claim that the current Linux sandbox executed NETLOAD.
