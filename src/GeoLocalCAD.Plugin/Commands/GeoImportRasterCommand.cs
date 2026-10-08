using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using GeoLocalCAD.Raster;

namespace GeoLocalCAD.Plugin.Commands
{
    public sealed class GeoImportRasterCommand
    {
        [CommandMethod("GEOIMPORTRASTER", CommandFlags.Modal)]
        public void Execute()
        {
            var document = Application.DocumentManager.MdiActiveDocument;
            if (document == null) throw new InvalidOperationException("GEOIMPORTRASTER requires an active drawing.");
            var editor = document.Editor;
            try
            {
                var file = editor.GetString(new PromptStringOptions("\nRaster path (GeoTIFF/TIFF/JPEG/PNG/BMP): ") { AllowSpaces = true });
                if (file.Status != PromptStatus.OK) return;
                var metadata = new RasterMetadataReader().Read(file.StringResult);
                if (!metadata.IsGeoreferenced) throw new InvalidOperationException("Raster has no GeoTIFF geotransform or World File. Add a valid georeferencing source before import; the image was not placed at (0,0).");
                var extent = metadata.Extent;
                using (var transaction = document.Database.TransactionManager.StartTransaction())
                {
                    var definitionId = RasterImageDef.CreateImageDefinition(file.StringResult);
                    var image = new RasterImage { ImageDefId = definitionId, ShowImage = true };
                    var origin = new Point3d(extent.MinX, extent.MinY, 0);
                    var u = new Vector3d(extent.MaxX - extent.MinX, 0, 0);
                    var v = new Vector3d(0, extent.MaxY - extent.MinY, 0);
                    image.Orientation = new CoordinateSystem3d(origin, u, v);
                    var space = (BlockTableRecord)transaction.GetObject(document.Database.CurrentSpaceId, OpenMode.ForWrite);
                    space.AppendEntity(image); transaction.AddNewlyCreatedDBObject(image, true);
                    transaction.Commit();
                }
                editor.WriteMessage("\nRaster imported: {0}x{1}, bands={2}, format={3}, extent=({4},{5})-({6},{7}), CRS={8}.\n", metadata.Width, metadata.Height, metadata.Bands, metadata.Format, extent.MinX, extent.MinY, extent.MaxX, extent.MaxY, metadata.Crs == null ? "not embedded" : metadata.Crs.CanonicalName);
                PluginEntryPoint.Logger?.Information("GEOIMPORTRASTER imported " + file.StringResult + " using geotransform " + metadata.GeoTransform + ".");
            }
            catch (Exception exception)
            {
                PluginEntryPoint.Logger?.Error("GEOIMPORTRASTER failed.", exception);
                editor.WriteMessage("\nGEOIMPORTRASTER failed. What: raster was not inserted. Where: GEOIMPORTRASTER. Likely cause: unsupported format, missing georeferencing, invalid image, or missing RasterImageDef support. Action: provide a valid GeoTIFF/World File and review the log.\n");
                throw;
            }
        }
    }
}
