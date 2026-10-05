using System;
using System.Text.RegularExpressions;

namespace GeoLocalCAD.CRS
{
    public sealed class CrsIdentifier
    {
        private static readonly Regex EpsgPattern = new Regex("^EPSG:[0-9]+$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        public string Authority { get; private set; }
        public int Code { get; private set; }
        public string CanonicalName { get { return Authority + ":" + Code; } }
        private CrsIdentifier(string authority, int code) { Authority = authority; Code = code; }
        public static CrsIdentifier Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("A CRS is required; no coordinate transformation may be inferred.", "value");
            var normalized = value.Trim().ToUpperInvariant();
            if (!EpsgPattern.IsMatch(normalized)) throw new FormatException("Unsupported CRS identifier '" + value + "'. Use an explicit identifier such as EPSG:4326 or EPSG:32638.");
            return new CrsIdentifier("EPSG", int.Parse(normalized.Substring(5), System.Globalization.CultureInfo.InvariantCulture));
        }
        public override string ToString() { return CanonicalName; }
    }
}
