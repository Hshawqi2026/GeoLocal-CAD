#!/usr/bin/env bash
set -euo pipefail
if [[ -z "${AUTODESK_MANAGED_API_PATH:-}" ]]; then
  echo "Set AUTODESK_MANAGED_API_PATH to the licensed AutoCAD/Civil 3D 2020 install directory." >&2
  exit 2
fi
if ! command -v msbuild >/dev/null 2>&1; then
  echo "msbuild is required; run this on a Windows/.NET Framework build environment." >&2
  exit 2
fi
msbuild GeoLocalCAD.sln /t:Rebuild /p:Configuration=Release /p:AutodeskManagedApiPath="$AUTODESK_MANAGED_API_PATH"
mkdir -p artifacts/GeoLocalCAD_v0.4.0
cp src/GeoLocalCAD.Plugin/bin/Release/GeoLocalCAD.dll artifacts/GeoLocalCAD_v0.4.0/
cp src/GeoLocalCAD.Core/bin/Release/GeoLocalCAD.Core.dll artifacts/GeoLocalCAD_v0.4.0/
cp src/GeoLocalCAD.UI/bin/Release/GeoLocalCAD.UI.dll artifacts/GeoLocalCAD_v0.4.0/
cp src/GeoLocalCAD.CRS/bin/Release/GeoLocalCAD.CRS.dll artifacts/GeoLocalCAD_v0.4.0/
cp src/GeoLocalCAD.Vector/bin/Release/GeoLocalCAD.Vector.dll artifacts/GeoLocalCAD_v0.4.0/
cp README-INSTALL.md CHANGELOG.md LICENSE artifacts/GeoLocalCAD_v0.4.0/
(cd artifacts && zip -r GeoLocalCAD_v0.4.0.zip GeoLocalCAD_v0.4.0 >/dev/null)
echo "Release created: artifacts/GeoLocalCAD_v0.4.0.zip"
