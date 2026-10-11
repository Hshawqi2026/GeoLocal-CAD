using System;
using System.IO;
using System.Linq;
using System.Text;
using GeoLocalCAD.CRS;
using GeoLocalCAD.DEM;
using GeoLocalCAD.Raster;

namespace GeoLocalCAD.TestRunner
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                string dem = GetOption(args, "--dem"); string report = GetOption(args, "--report");
                var result = dem == null ? RunSelfTest() : RunDemTest(dem);
                Console.WriteLine(result);
                if (report != null) { var directory = Path.GetDirectoryName(Path.GetFullPath(report)); if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory); File.WriteAllText(report, result + Environment.NewLine, Encoding.UTF8); }
                return 0;
            }
            catch (Exception ex)
            {
                var result = "GeoLocalCAD TestRunner: FAIL\n" + ex.GetType().FullName + ": " + ex.Message;
                Console.Error.WriteLine(result); return 1;
            }
        }

        private static string RunSelfTest()
        {
            var values = new double[2, 2]; values[0, 0] = 10; values[1, 0] = 20; values[0, 1] = -9999; values[1, 1] = 40;
            var grid = new DemGrid(values, new RasterGeoTransform(5, 0, 0, -5, 100, 200), CrsIdentifier.Parse("EPSG:32638"), -9999);
            var samples = grid.EnumerateValidSamples().ToArray();
            if (samples.Length != 3) throw new InvalidOperationException("NoData filtering failed.");
            if (Math.Abs(samples[0].Location.X - 102.5) > 1e-9 || Math.Abs(samples[0].Location.Y - 197.5) > 1e-9) throw new InvalidOperationException("Pixel-to-world validation failed.");
            return "GeoLocalCAD TestRunner: PASS\nContract tests: PASS\nCivil 3D TinSurface: NOT TESTED (requires Civil 3D)";
        }

        private static string RunDemTest(string path)
        {
            var grid = new GdalElevationSampler().Read(path);
            var samples = grid.EnumerateValidSamples().ToArray();
            if (samples.Length < 3) throw new InvalidOperationException("DEM has fewer than three valid samples.");
            return string.Format("GeoLocalCAD TestRunner: PASS\nDEM: {0}\nCRS: {1}\nDimensions: {2}x{3}\nValid samples: {4}\nCivil 3D TinSurface: NOT TESTED (requires Civil 3D)", path, grid.Crs.CanonicalName, grid.Width, grid.Height, samples.Length);
        }

        private static string GetOption(string[] args, string name)
        {
            for (var i = 0; i < args.Length - 1; i++) if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase)) return args[i + 1];
            return null;
        }
    }
}
