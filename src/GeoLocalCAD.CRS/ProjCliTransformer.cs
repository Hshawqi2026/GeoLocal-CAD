using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using GeoLocalCAD.Core.Logging;

namespace GeoLocalCAD.CRS
{
    public sealed class ProjCliTransformer : ICoordinateTransformer
    {
        private readonly string _cs2csPath;
        private readonly IGeoLogger _logger;
        public ProjCliTransformer(string cs2csPath, IGeoLogger logger)
        {
            if (string.IsNullOrWhiteSpace(cs2csPath)) throw new ArgumentException("A PROJ cs2cs executable path is required.", "cs2csPath");
            _cs2csPath = cs2csPath; _logger = logger;
        }
        public Coordinate Transform(Coordinate coordinate, CrsIdentifier source, CrsIdentifier target)
        {
            if (source == null || target == null) throw new ArgumentNullException("source", "Source and target CRS are mandatory.");
            if (source.CanonicalName == target.CanonicalName) return coordinate;
            if (!File.Exists(_cs2csPath)) throw new FileNotFoundException("PROJ cs2cs was not found. Install or configure the offline PROJ runtime before transforming coordinates.", _cs2csPath);
            var input = string.Format(CultureInfo.InvariantCulture, "{0:R} {1:R} {2:R}\n", coordinate.X, coordinate.Y, coordinate.Z);
            var sourceGeographic = IsGeographic(source);
            var targetGeographic = IsGeographic(target);
            var axisSwitch = sourceGeographic ? "-r " : string.Empty;
            var psi = new ProcessStartInfo { FileName = _cs2csPath, Arguments = axisSwitch + "-f %.17g " + source.CanonicalName + " " + target.CanonicalName, UseShellExecute = false, RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
            using (var process = new Process { StartInfo = psi })
            {
                process.Start(); process.StandardInput.Write(input); process.StandardInput.Close();
                var output = process.StandardOutput.ReadToEnd(); var error = process.StandardError.ReadToEnd(); process.WaitForExit();
                if (process.ExitCode != 0) { _logger?.Error("PROJ coordinate transformation failed: " + source + " to " + target + ". " + error); throw new InvalidOperationException("PROJ could not transform from " + source + " to " + target + ". Verify the EPSG definitions and coordinate order. Details: " + error); }
                var parts = output.Trim().Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) throw new InvalidDataException("PROJ returned an invalid coordinate result: " + output);
                double x, y, z = 0; if (!double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out x) || !double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out y)) throw new InvalidDataException("PROJ returned non-numeric coordinates: " + output);
                if (parts.Length > 2) double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out z);
                if (targetGeographic && !sourceGeographic) { var swap = x; x = y; y = swap; }
                _logger?.Information("CRS transformation completed: " + source + " to " + target);
                return new Coordinate(x, y, z);
            }
        }

        private static bool IsGeographic(CrsIdentifier crs)
        {
            // PROJ still performs the actual CRS operation; this flag only adapts
            // traditional XY application coordinates to EPSG axis conventions.
            return crs.Code == 4326 || crs.Code == 4258 || crs.Code == 4269 || crs.Code == 4277;
        }
    }
}
