using System;
using System.Collections.Generic;
using System.IO;
using GeoLocalCAD.CRS;
using GeoLocalCAD.Vector;

namespace GeoLocalCAD.Core.Tests
{
    internal static class GeoPhase3SmokeTest
    {
        public static void Run()
        {
            var wgs84 = CrsIdentifier.Parse("epsg:4326");
            if (wgs84.CanonicalName != "EPSG:4326") throw new InvalidOperationException("CRS canonicalization failed.");
            var path = Path.Combine(Path.GetTempPath(), "geolocal-phase3-" + Guid.NewGuid().ToString("N") + ".geojson");
            var dataset = new GeoJsonDataset(wgs84);
            var point = new GeoFeature { GeometryType = "Point" }; point.Parts.Add(new List<Coordinate> { new Coordinate(44.2, 15.3, 0) }); point.Attributes["name"] = "control"; dataset.Features.Add(point);
            var line = new GeoFeature { GeometryType = "LineString" }; line.Parts.Add(new List<Coordinate> { new Coordinate(44.2, 15.3), new Coordinate(44.3, 15.4) }); dataset.Features.Add(line);
            new GeoJsonWriter().Write(path, dataset, wgs84, null);
            var loaded = new GeoJsonReader().Read(path, wgs84);
            if (loaded.Features.Count != 2 || loaded.Features[0].Attributes["name"].ToString() != "control") throw new InvalidOperationException("GeoJSON round-trip failed.");
            File.Delete(path);
            var proj = new ProjCliTransformer("/usr/bin/cs2cs", null);
            var utm = proj.Transform(new Coordinate(44.2, 15.3), wgs84, CrsIdentifier.Parse("EPSG:32638"));
            if (Math.Abs(utm.X - 414112.6900) > 0.1 || Math.Abs(utm.Y - 1691665.9910) > 0.1) throw new InvalidOperationException("PROJ UTM transformation regression failed: " + utm);
            Console.WriteLine("Phase 3 CRS/GeoJSON smoke test passed.");
        }
    }
}
