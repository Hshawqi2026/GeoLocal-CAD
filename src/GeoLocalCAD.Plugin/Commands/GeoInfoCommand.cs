using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using GeoLocalCAD.CRS;
using GeoLocalCAD.Core.Inspection;
using GeoLocalCAD.Core.Settings;
using GeoLocalCAD.UI;

namespace GeoLocalCAD.Plugin.Commands
{
    public sealed class GeoInfoCommand
    {
        [CommandMethod("GEOINFO", CommandFlags.Modal)]
        public void Execute()
        {
            var document = Application.DocumentManager.MdiActiveDocument;
            if (document == null) throw new InvalidOperationException("GEOINFO requires an active drawing.");
            try
            {
                var picked = document.Editor.GetEntity(new PromptEntityOptions("\nSelect a Point, Line, Polyline, or Circle: "));
                if (picked.Status != PromptStatus.OK) return;
                CoordinateObservation observation;
                using (var transaction = document.Database.TransactionManager.StartTransaction())
                {
                    observation = ReadObservation((Entity)transaction.GetObject(picked.ObjectId, OpenMode.ForRead));
                    transaction.Commit();
                }
                var result = BuildResult(observation);
                PluginEntryPoint.Logger?.Information("GEOINFO inspected " + observation.EntityType + ": " + observation.X + "," + observation.Y + "," + observation.Z + ". Status=" + result.Status);
                Application.ShowModalWindow(new CoordinateInspectorWindow(result, PluginEntryPoint.Logger));
            }
            catch (Exception exception)
            {
                PluginEntryPoint.Logger?.Error("GEOINFO command failed.", exception);
                document.Editor.WriteMessage("\nGEOINFO failed. What: coordinate inspection did not complete. Where: GEOINFO. Likely cause: unsupported entity, missing Drawing CRS, or invalid PROJ runtime. Action: select a supported entity, configure an explicit CRS, and review the log.\n");
                throw;
            }
        }

        private static CoordinateObservation ReadObservation(Entity entity)
        {
            var point = entity as DBPoint; if (point != null) return FromPoint("DBPoint", point.Position);
            var line = entity as Line; if (line != null) return FromPoint("Line.StartPoint", line.StartPoint);
            var polyline = entity as Polyline; if (polyline != null) return FromPoint("Polyline.Vertex0", polyline.GetPoint3dAt(0));
            var circle = entity as Circle; if (circle != null) return FromPoint("Circle.Center", circle.Center);
            throw new NotSupportedException("GEOINFO does not support entity type " + entity.GetType().Name + " in Phase 4. Select a DBPoint, Line, Polyline, or Circle.");
        }

        private static CoordinateObservation FromPoint(string type, Point3d point) { return new CoordinateObservation(type, point.X, point.Y, point.Z); }

        private static CoordinateInspectionResult BuildResult(CoordinateObservation observation)
        {
            var settings = new GeoSettingsStore().Load();
            if (string.IsNullOrWhiteSpace(settings.DrawingCrs)) return CoordinateInspectionResult.Create(observation, string.Empty, "EPSG:4326", null, null, null, "CRS not configured", "Set an explicit Drawing CRS in GEOSETTINGS. No geographic transformation was attempted.");
            CrsIdentifier drawing;
            try { drawing = CrsIdentifier.Parse(settings.DrawingCrs); }
            catch (Exception exception) { return CoordinateInspectionResult.Create(observation, settings.DrawingCrs, "EPSG:4326", null, null, null, "Invalid CRS", exception.Message); }
            var geographic = CrsIdentifier.Parse("EPSG:4326");
            if (drawing.CanonicalName == geographic.CanonicalName) return CoordinateInspectionResult.Create(observation, drawing.CanonicalName, geographic.CanonicalName, observation.X, observation.Y, observation.Z, "Available", string.Empty);
            var projPath = Environment.GetEnvironmentVariable("GEOLOCAL_PROJ_CS2CS");
            if (string.IsNullOrWhiteSpace(projPath)) return CoordinateInspectionResult.Create(observation, drawing.CanonicalName, geographic.CanonicalName, null, null, null, "Transformation unavailable", "GEOLOCAL_PROJ_CS2CS is not configured. Configure the offline PROJ cs2cs executable to calculate longitude/latitude.");
            try
            {
                var transformed = new ProjCliTransformer(projPath, PluginEntryPoint.Logger).Transform(new Coordinate(observation.X, observation.Y, observation.Z), drawing, geographic);
                return CoordinateInspectionResult.Create(observation, drawing.CanonicalName, geographic.CanonicalName, transformed.X, transformed.Y, transformed.Z, "Available", string.Empty);
            }
            catch (Exception exception) { return CoordinateInspectionResult.Create(observation, drawing.CanonicalName, geographic.CanonicalName, null, null, null, "Transformation failed", exception.Message); }
        }
    }
}
