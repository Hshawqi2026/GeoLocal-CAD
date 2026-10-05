using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using GeoLocalCAD.CRS;
using GeoLocalCAD.Vector;

namespace GeoLocalCAD.Plugin.Commands
{
    public sealed class GeoImportCommand
    {
        [CommandMethod("GEOIMPORT", CommandFlags.Modal)]
        public void Execute()
        {
            var document = Application.DocumentManager.MdiActiveDocument;
            if (document == null) throw new InvalidOperationException("GEOIMPORT requires an active drawing.");
            var editor = document.Editor;
            try
            {
                var file = editor.GetString(new PromptStringOptions("\nGeoJSON file path: ") { AllowSpaces = true });
                if (file.Status != PromptStatus.OK) return;
                var crsText = editor.GetString(new PromptStringOptions("\nSource CRS (required, e.g. EPSG:32638): ") { AllowSpaces = false });
                if (crsText.Status != PromptStatus.OK) return;
                var source = CrsIdentifier.Parse(crsText.StringResult);
                var drawingCrsText = editor.GetString(new PromptStringOptions("\nDrawing CRS (required, e.g. EPSG:32638): ") { AllowSpaces = false });
                if (drawingCrsText.Status != PromptStatus.OK) return;
                var drawingCrs = CrsIdentifier.Parse(drawingCrsText.StringResult);
                var dataset = new GeoJsonReader().Read(file.StringResult, source);
                if (source.CanonicalName != drawingCrs.CanonicalName)
                {
                    var projPath = Environment.GetEnvironmentVariable("GEOLOCAL_PROJ_CS2CS");
                    if (string.IsNullOrWhiteSpace(projPath)) throw new InvalidOperationException("Source CRS differs from drawing CRS, but GEOLOCAL_PROJ_CS2CS is not configured. Set it to the offline PROJ cs2cs executable; no silent transformation was performed.");
                    TransformDataset(dataset, drawingCrs, new ProjCliTransformer(projPath, PluginEntryPoint.Logger));
                }
                using (var transaction = document.Database.TransactionManager.StartTransaction())
                {
                    foreach (var feature in dataset.Features) AddFeature(document.Database, transaction, feature);
                    transaction.Commit();
                }
                PluginEntryPoint.Logger?.Information("GEOIMPORT imported " + dataset.Features.Count + " features from " + file.StringResult + " with source CRS " + source + ".");
                editor.WriteMessage("\nImported {0} feature(s) from GeoJSON. Source CRS: {1}; drawing CRS: {2}.\n", dataset.Features.Count, source, drawingCrs);
            }
            catch (Exception exception)
            {
                PluginEntryPoint.Logger?.Error("GEOIMPORT failed.", exception);
                editor.WriteMessage("\nGEOIMPORT failed. What: GeoJSON was not imported. Where: GEOIMPORT. Likely cause: invalid file, unsupported geometry, or missing CRS. Action: provide a valid FeatureCollection and explicit EPSG CRS; review the log.\n");
                throw;
            }
        }

        private static void TransformDataset(GeoJsonDataset dataset, CrsIdentifier target, ICoordinateTransformer transformer)
        {
            foreach (var feature in dataset.Features) foreach (var part in feature.Parts) for (var i = 0; i < part.Count; i++) part[i] = transformer.Transform(part[i], dataset.SourceCrs, target);
        }

        private static void AddFeature(Database database, Transaction transaction, GeoFeature feature)
        {
            var space = (BlockTableRecord)transaction.GetObject(database.CurrentSpaceId, OpenMode.ForWrite);
            if (feature.GeometryType.Equals("Point", StringComparison.OrdinalIgnoreCase))
            {
                var c = feature.Parts[0][0]; var dbPoint = new DBPoint(new Point3d(c.X, c.Y, c.Z)); space.AppendEntity(dbPoint); transaction.AddNewlyCreatedDBObject(dbPoint, true); return;
            }
            foreach (var part in feature.Parts)
            {
                var polyline = new Polyline(); for (var i = 0; i < part.Count; i++) polyline.AddVertexAt(i, new Point2d(part[i].X, part[i].Y), 0, 0, 0);
                if (feature.GeometryType.Equals("Polygon", StringComparison.OrdinalIgnoreCase) && part.Count > 2 && part[0].X == part[part.Count - 1].X && part[0].Y == part[part.Count - 1].Y) polyline.Closed = true;
                space.AppendEntity(polyline); transaction.AddNewlyCreatedDBObject(polyline, true);
            }
        }
    }
}
