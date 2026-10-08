using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using GeoLocalCAD.Raster;

namespace GeoLocalCAD.DEM
{
    public sealed class GdalElevationSamplerOptions
    {
        public string GdalTranslatePath { get; set; }
        public string GdalInfoPath { get; set; }
        public int Band { get; set; }
        public TimeSpan Timeout { get; set; }
        public GdalElevationSamplerOptions()
        {
            GdalTranslatePath = Environment.GetEnvironmentVariable("GEOLOCAL_GDAL_TRANSLATE") ?? "gdal_translate";
            GdalInfoPath = Environment.GetEnvironmentVariable("GEOLOCAL_GDALINFO") ?? "gdalinfo";
            Band = 1; Timeout = TimeSpan.FromMinutes(5);
        }
    }

    public sealed class GdalElevationSampler
    {
        private readonly RasterMetadataReader metadataReader;
        private readonly GdalElevationSamplerOptions options;
        public GdalElevationSampler(RasterMetadataReader metadataReader = null, GdalElevationSamplerOptions options = null)
        {
            this.metadataReader = metadataReader ?? new RasterMetadataReader(); this.options = options ?? new GdalElevationSamplerOptions();
            if (this.options.Band < 1) throw new ArgumentOutOfRangeException("options", "GDAL band must be one-based.");
        }
        public DemGrid Read(string rasterPath)
        {
            var metadata = metadataReader.Read(rasterPath);
            if (!metadata.IsGeoreferenced) throw new InvalidDataException("DEM raster has no geotransform or World File; elevation samples cannot be placed safely.");
            if (metadata.Crs == null) throw new InvalidDataException("DEM raster CRS is missing; provide an authoritative CRS before creating a surface.");
            var noData = ReadNoData(rasterPath);
            var output = Run(options.GdalTranslatePath, "-of XYZ -b " + options.Band.ToString(CultureInfo.InvariantCulture) + " " + Quote(rasterPath) + " /vsistdout/", options.Timeout);
            var values = new double[metadata.Width, metadata.Height]; var count = 0;
            using (var reader = new StringReader(output))
            {
                string line; while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var fields = line.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries); if (fields.Length < 3) continue;
                    double x, y, z; if (!double.TryParse(fields[0], NumberStyles.Float, CultureInfo.InvariantCulture, out x) || !double.TryParse(fields[1], NumberStyles.Float, CultureInfo.InvariantCulture, out y) || !double.TryParse(fields[2], NumberStyles.Float, CultureInfo.InvariantCulture, out z)) throw new InvalidDataException("GDAL XYZ output contains a non-numeric sample: " + line);
                    var row = count / metadata.Width; var column = count % metadata.Width; if (row >= metadata.Height) throw new InvalidDataException("GDAL returned more samples than raster dimensions.");
                    values[column, row] = z; count++;
                }
            }
            var expected = metadata.Width * metadata.Height; if (count != expected) throw new InvalidDataException("GDAL returned " + count + " samples but metadata declares " + expected + ".");
            return new DemGrid(values, metadata.GeoTransform, metadata.Crs, noData);
        }
        private double? ReadNoData(string rasterPath)
        {
            try
            {
                var json = Run(options.GdalInfoPath, "-json " + Quote(rasterPath), options.Timeout);
                var match = Regex.Match(json, "\\\"noDataValue\\\"\\s*:\\s*(-?(?:\\d+\\.?\\d*|\\.\\d+)(?:[eE][+-]?\\d+)?)", RegexOptions.CultureInvariant);
                double value; if (match.Success && double.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out value)) return value;
            }
            catch (Exception) { /* metadata reader still reports the actionable GDAL failure below if sampling fails */ }
            return null;
        }
        private static string Run(string executable, string arguments, TimeSpan timeout)
        {
            var psi = new ProcessStartInfo { FileName = executable, Arguments = arguments, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
            using (var process = new Process { StartInfo = psi })
            {
                if (!process.Start()) throw new InvalidOperationException("Could not start GDAL executable: " + executable);
                var stdout = process.StandardOutput.ReadToEnd(); var stderr = process.StandardError.ReadToEnd();
                if (!process.WaitForExit((int)timeout.TotalMilliseconds)) { try { process.Kill(); } catch { } throw new TimeoutException("GDAL operation timed out after " + timeout + "."); }
                if (process.ExitCode != 0) throw new InvalidOperationException("GDAL command failed (" + executable + "): " + stderr.Trim());
                return stdout;
            }
        }
        private static string Quote(string value) { return "\"" + value.Replace("\"", "\\\"") + "\""; }
    }
}
