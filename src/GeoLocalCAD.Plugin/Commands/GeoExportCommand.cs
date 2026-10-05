using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using GeoLocalCAD.CRS;
using GeoLocalCAD.Vector;

namespace GeoLocalCAD.Plugin.Commands
{
    public sealed class GeoExportCommand
    {
        [CommandMethod("GEOEXPORT", CommandFlags.Modal)]
        public void Execute()
        {
            var document = Application.DocumentManager.MdiActiveDocument;
            if (document == null) throw new InvalidOperationException("GEOEXPORT requires an active drawing.");
            var editor = document.Editor;
            try
            {
                var selection = editor.GetSelection(); if (selection.Status != PromptStatus.OK) return;
                var file = editor.GetString(new PromptStringOptions("\nOutput GeoJSON path: ") { AllowSpaces = true }); if (file.Status != PromptStatus.OK) return;
                var crsText = editor.GetString(new PromptStringOptions("\nDrawing CRS (required, e.g. EPSG:32638): ") { AllowSpaces = false }); if (crsText.Status != PromptStatus.OK) return;
                var target = CrsIdentifier.Parse(crsText.StringResult);
                var dataset = new GeoJsonDataset(target);
                using (var transaction = document.Database.TransactionManager.StartTransaction())
                {
                    foreach (SelectedObject selected in selection.Value)
                    {
                        var entity = transaction.GetObject(selected.ObjectId, OpenMode.ForRead) as Entity;
                        var feature = GeoEntityConverter.TryConvert(entity); if (feature != null) dataset.Features.Add(feature);
                    }
                    transaction.Commit();
                }
                new GeoJsonWriter().Write(file.StringResult, dataset, target, null);
                PluginEntryPoint.Logger?.Information("GEOEXPORT exported " + dataset.Features.Count + " features to " + file.StringResult + " with CRS " + target + ".");
                editor.WriteMessage("\nExported {0} feature(s) to GeoJSON. CRS: {1}.\n", dataset.Features.Count, target);
            }
            catch (Exception exception)
            {
                PluginEntryPoint.Logger?.Error("GEOEXPORT failed.", exception);
                editor.WriteMessage("\nGEOEXPORT failed. What: selected entities were not exported. Where: GEOEXPORT. Likely cause: unsupported entity type, invalid path, or missing CRS. Action: select Point/Polyline entities, provide an explicit EPSG CRS, and review the log.\n");
                throw;
            }
        }
    }

    internal static class GeoEntityConverter
    {
        public static GeoFeature TryConvert(Entity entity)
        {
            var point = entity as Autodesk.AutoCAD.DatabaseServices.DBPoint;
            if (point != null) { var p = point.Position; var f = new GeoFeature { GeometryType = "Point" }; f.Parts.Add(new System.Collections.Generic.List<Coordinate> { new Coordinate(p.X, p.Y, p.Z) }); return f; }
            var polyline = entity as Autodesk.AutoCAD.DatabaseServices.Polyline;
            if (polyline == null) return null;
            var feature = new GeoFeature { GeometryType = polyline.Closed ? "Polygon" : "LineString" };
            var part = new System.Collections.Generic.List<Coordinate>(); for (var i = 0; i < polyline.NumberOfVertices; i++) { var p = polyline.GetPoint3dAt(i); part.Add(new Coordinate(p.X, p.Y, p.Z)); }
            if (polyline.Closed && part.Count > 0 && (part[0].X != part[part.Count - 1].X || part[0].Y != part[part.Count - 1].Y)) part.Add(part[0]);
            feature.Parts.Add(part); return feature;
        }
    }
}
