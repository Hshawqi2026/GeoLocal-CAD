# Phase 4 — Coordinate Inspector (`GEOINFO`)

## Implemented

Phase 4 adds Core inspection contracts, a WPF Coordinate Inspector window, and the AutoCAD command `GEOINFO`. The command reads a representative coordinate from DBPoint, Line, Polyline, or Circle without modifying the drawing.

The window displays the entity type, drawing X/Y/Z, Drawing CRS, geographic longitude/latitude when a valid conversion to EPSG:4326 is available, status, warnings, and a copy-to-clipboard action. Drawing CRS is loaded from the explicit Phase 2 settings file. No geographic CRS is inferred when settings are empty or invalid.

For a projected Drawing CRS, `GEOINFO` uses the Phase 3 PROJ adapter and `GEOLOCAL_PROJ_CS2CS`. When PROJ is unavailable, the raw drawing coordinate remains visible while geographic output is marked unavailable; the command does not invent latitude/longitude values.

## Civil 3D validation checklist

- [ ] Build on Windows against the licensed Civil 3D 2020 managed API.
- [ ] NETLOAD `GeoLocalCAD.dll`.
- [ ] Configure `EPSG:4326` in `GEOSETTINGS`; select a DBPoint and run `GEOINFO`.
- [ ] Confirm X/Y/Z and longitude/latitude are shown without changing the drawing.
- [ ] Configure `EPSG:32638`, set `GEOLOCAL_PROJ_CS2CS`, select a UTM point, and confirm geographic values are shown.
- [ ] Remove the PROJ variable and confirm `GEOINFO` visibly reports transformation unavailable.
- [ ] Test Line, Polyline, and Circle; verify the displayed entity type and representative coordinate.
- [ ] Click Copy coordinates and paste into a text editor.
- [ ] Confirm the log records the entity type, status, and any transformation error.

## Automated validation

The Core test executable covers finite coordinate validation, formatting, CRS parsing, PROJ WGS84/UTM conversion, and GeoJSON round-trip. WPF hosting and Autodesk entity integration still require Civil 3D 2020 runtime validation.
