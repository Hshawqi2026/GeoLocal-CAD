using System;
using System.Linq;
using GeoLocalCAD.CRS;
using GeoLocalCAD.DEM;
using GeoLocalCAD.Raster;

namespace GeoLocalCAD.Core.Tests
{
    internal static class GeoDemSmokeTest
    {
        public static void Run()
        {
            var values = new double[2, 2]; values[0, 0] = 10; values[1, 0] = 20; values[0, 1] = -9999; values[1, 1] = 40;
            var transform = new RasterGeoTransform(5, 0, 0, -5, 100, 200);
            var grid = new DemGrid(values, transform, CrsIdentifier.Parse("EPSG:32638"), -9999);
            var samples = grid.EnumerateValidSamples().ToArray();
            if (samples.Length != 3) throw new InvalidOperationException("DEM NoData filtering failed.");
            if (Math.Abs(samples[0].Location.X - 102.5) > 1e-9 || Math.Abs(samples[0].Location.Y - 197.5) > 1e-9) throw new InvalidOperationException("DEM sample geolocation failed.");
            var request = new Civil3DSurfaceRequest("DEM_Test", "GEO_DEM", 0.01);
            if (request.SurfaceName != "DEM_Test" || request.LayerName != "GEO_DEM") throw new InvalidOperationException("Civil 3D surface request failed.");
            Console.WriteLine("Phase 6 Raster-to-DEM contract smoke test passed.");
        }
    }
}
