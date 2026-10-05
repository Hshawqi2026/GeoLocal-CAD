# Phase 2 — Ribbon, WPF, and Settings

## Implemented

- `GeoLocalCAD.UI` targets .NET Framework 4.8 and contains a real WPF settings window.
- `GEOSETTINGS` opens the WPF window from AutoCAD.
- Settings are persisted locally as XML under `%LOCALAPPDATA%\\GeoLocalCAD\\settings.xml`.
- Offline mode defaults to enabled.
- Drawing CRS is an explicit user-entered value; no geographic or Yemen-based assumption is made.
- The `GeoLocal` Ribbon tab is created through `Autodesk.Windows` after AutoCAD initializes the Ribbon.
- Ribbon sections: Maps, CRS, Import/Export, Georeference, DEM, Survey, Civil 3D, Tools, Settings.
- Ribbon buttons dispatch real AutoCAD commands; only `GEOLOCAL` and `GEOSETTINGS` are implemented in this phase. Other buttons deliberately route to the existing command until their dedicated feature phase exists and are not represented as completed GIS functionality.
- `AdWindows.dll` is referenced from the local licensed Autodesk installation and is never copied to the repository or release package.

## Validation status

The source and project structure are implemented. A full build and runtime test still requires a Windows machine with licensed Civil 3D 2020, AutoCAD managed APIs, `AdWindows.dll`, and .NET Framework 4.8. The Linux sandbox cannot truthfully validate Ribbon rendering or WPF hosting inside Civil 3D.

## Civil 3D checklist

- [ ] Build with `AutodeskManagedApiPath` pointing to the Civil 3D 2020 install folder.
- [ ] NETLOAD `GeoLocalCAD.dll`.
- [ ] Confirm the `GeoLocal` Ribbon tab appears.
- [ ] Click `Settings` and confirm the WPF dialog opens.
- [ ] Change Offline mode/CRS, save, reopen `GEOSETTINGS`, and confirm persistence.
- [ ] Confirm `%LOCALAPPDATA%\\GeoLocalCAD\\settings.xml` exists.
- [ ] Confirm the log contains Ribbon creation and settings events.
