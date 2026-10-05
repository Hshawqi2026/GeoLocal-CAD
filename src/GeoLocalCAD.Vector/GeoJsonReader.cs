using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using GeoLocalCAD.CRS;

namespace GeoLocalCAD.Vector
{
    public sealed class GeoJsonReader
    {
        public GeoJsonDataset Read(string path, CrsIdentifier sourceCrs)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("A GeoJSON path is required.", "path");
            if (sourceCrs == null) throw new ArgumentNullException("sourceCrs", "GeoJSON has no universally reliable CRS member; provide the source CRS explicitly.");
            var text = File.ReadAllText(path);
            var serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            var root = serializer.DeserializeObject(text) as IDictionary<string, object>;
            if (root == null || !string.Equals(Convert.ToString(root["type"]), "FeatureCollection", StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("GeoJSON must be a FeatureCollection.");
            var dataset = new GeoJsonDataset(sourceCrs);
            foreach (var raw in (IEnumerable)root["features"])
            {
                var feature = raw as IDictionary<string, object>; if (feature == null) throw new InvalidDataException("GeoJSON contains an invalid feature.");
                var geometry = feature["geometry"] as IDictionary<string, object>; if (geometry == null) continue;
                var item = new GeoFeature { GeometryType = Convert.ToString(geometry["type"]) };
                ReadGeometry(item, geometry["coordinates"]);
                var properties = feature["properties"] as IDictionary<string, object>;
                if (properties != null) foreach (var pair in properties) item.Attributes[pair.Key] = pair.Value;
                dataset.Features.Add(item);
            }
            return dataset;
        }

        private static void ReadGeometry(GeoFeature feature, object value)
        {
            if (string.Equals(feature.GeometryType, "Point", StringComparison.OrdinalIgnoreCase)) feature.Parts.Add(new List<Coordinate> { ReadCoordinate(value as IList) });
            else if (string.Equals(feature.GeometryType, "LineString", StringComparison.OrdinalIgnoreCase)) feature.Parts.Add(ReadLine(value as IList));
            else if (string.Equals(feature.GeometryType, "Polygon", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var ring in (IEnumerable)value) feature.Parts.Add(ReadLine(ring as IList));
            }
            else throw new NotSupportedException("GeoJSON geometry type '" + feature.GeometryType + "' is not supported in Phase 3. Supported types: Point, LineString, Polygon.");
        }
        private static List<Coordinate> ReadLine(IList values)
        {
            if (values == null || values.Count < 2) throw new InvalidDataException("GeoJSON line geometry must contain at least two coordinates.");
            var result = new List<Coordinate>(); foreach (var value in values) result.Add(ReadCoordinate(value as IList)); return result;
        }
        private static Coordinate ReadCoordinate(IList values)
        {
            if (values == null || values.Count < 2) throw new InvalidDataException("GeoJSON coordinate must contain at least X and Y.");
            return new Coordinate(Convert.ToDouble(values[0], System.Globalization.CultureInfo.InvariantCulture), Convert.ToDouble(values[1], System.Globalization.CultureInfo.InvariantCulture), values.Count > 2 ? Convert.ToDouble(values[2], System.Globalization.CultureInfo.InvariantCulture) : 0);
        }
    }
}
