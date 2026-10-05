using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using GeoLocalCAD.CRS;
using System.Web.Script.Serialization;

namespace GeoLocalCAD.Vector
{
    public sealed class GeoJsonWriter
    {
        public void Write(string path, GeoJsonDataset dataset, CrsIdentifier targetCrs, ICoordinateTransformer transformer)
        {
            if (dataset == null) throw new ArgumentNullException("dataset");
            if (targetCrs == null) throw new ArgumentNullException("targetCrs", "Target CRS is mandatory for export.");
            if (transformer == null && dataset.SourceCrs.CanonicalName != targetCrs.CanonicalName) throw new InvalidOperationException("Source and target CRS differ, but no transformation engine was supplied. Export was stopped to prevent a silent transformation.");
            var features = new List<object>();
            foreach (var item in dataset.Features)
            {
                var geometry = new Dictionary<string, object> { { "type", item.GeometryType }, { "coordinates", BuildCoordinates(item, dataset.SourceCrs, targetCrs, transformer) } };
                var properties = new Dictionary<string, object>(item.Attributes);
                features.Add(new Dictionary<string, object> { { "type", "Feature" }, { "geometry", geometry }, { "properties", properties } });
            }
            var root = new Dictionary<string, object> { { "type", "FeatureCollection" }, { "geolocalCrs", targetCrs.CanonicalName }, { "features", features } };
            var serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            File.WriteAllText(path, serializer.Serialize(root));
        }
        private static object BuildCoordinates(GeoFeature item, CrsIdentifier source, CrsIdentifier target, ICoordinateTransformer transformer)
        {
            var parts = new List<object>();
            foreach (var part in item.Parts)
            {
                var coords = new List<object>(); foreach (var coordinate in part) { var c = transformer == null ? coordinate : transformer.Transform(coordinate, source, target); coords.Add(new[] { c.X, c.Y, c.Z }); }
                if (string.Equals(item.GeometryType, "Point", StringComparison.OrdinalIgnoreCase)) return coords[0];
                parts.Add(coords);
            }
            return string.Equals(item.GeometryType, "Polygon", StringComparison.OrdinalIgnoreCase) ? (object)parts : parts[0];
        }
    }
}
