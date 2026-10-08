using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using GeoLocalCAD.DEM;
using GeoLocalCAD.Plugin.Civil3D;

namespace GeoLocalCAD.Plugin.Commands
{
    public sealed class GeoDemCommand
    {
        [CommandMethod("GEODEM", CommandFlags.Modal)]
        public void Execute()
        {
            var document = Application.DocumentManager.MdiActiveDocument;
            if (document == null) throw new InvalidOperationException("GEODEM requires an active drawing.");
            var editor = document.Editor;
            try
            {
                var file = editor.GetString(new PromptStringOptions("\nGeoTIFF DEM path: ") { AllowSpaces = true }); if (file.Status != PromptStatus.OK) return;
                var surfaceName = editor.GetString(new PromptStringOptions("\nCivil 3D surface name: ") { AllowSpaces = false }); if (surfaceName.Status != PromptStatus.OK) return;
                var layerName = editor.GetString(new PromptStringOptions("\nSurface layer name: ") { AllowSpaces = false }); if (layerName.Status != PromptStatus.OK) return;
                var grid = new GdalElevationSampler().Read(file.StringResult);
                var request = new Civil3DSurfaceRequest(surfaceName.StringResult, layerName.StringResult);
                var count = new Civil3DTinSurfaceBuilder(document.Database).BuildTinSurface(grid, request);
                editor.WriteMessage("\nGEODEM created Civil 3D TIN surface '{0}' from {1} valid elevation samples; CRS={2}.\n", request.SurfaceName, count, grid.Crs.CanonicalName);
                PluginEntryPoint.Logger?.Information("GEODEM created TinSurface " + request.SurfaceName + " from " + file.StringResult + "; valid samples=" + count + ".");
            }
            catch (Exception exception)
            {
                PluginEntryPoint.Logger?.Error("GEODEM failed; no successful surface result was reported.", exception);
                editor.WriteMessage("\nGEODEM failed. What: no DEM surface was created successfully. Where: GEODEM. Likely cause: GDAL is unavailable, DEM CRS/geotransform is missing, the raster contains insufficient valid samples, or Civil 3D rejected the surface operation. Action: configure GEOLOCAL_GDAL_TRANSLATE/GEOLOCAL_GDALINFO, provide a georeferenced single-band DEM, and review the log.\n");
                throw;
            }
        }
    }
}
