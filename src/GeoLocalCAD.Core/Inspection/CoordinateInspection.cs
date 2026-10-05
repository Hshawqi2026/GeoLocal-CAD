using System;
using System.Globalization;

namespace GeoLocalCAD.Core.Inspection
{
    public sealed class CoordinateObservation
    {
        public string EntityType { get; private set; }
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Z { get; private set; }
        public CoordinateObservation(string entityType, double x, double y, double z)
        {
            if (string.IsNullOrWhiteSpace(entityType)) throw new ArgumentException("Entity type is required.", "entityType");
            Validate(x, "X"); Validate(y, "Y"); Validate(z, "Z");
            EntityType = entityType; X = x; Y = y; Z = z;
        }
        private static void Validate(double value, string name) { if (double.IsNaN(value) || double.IsInfinity(value)) throw new ArgumentOutOfRangeException(name, "Coordinate values must be finite numbers."); }
    }

    public sealed class CoordinateInspectionResult
    {
        public CoordinateObservation Observation { get; private set; }
        public string DrawingCrs { get; private set; }
        public string GeographicCrs { get; private set; }
        public double? Longitude { get; private set; }
        public double? Latitude { get; private set; }
        public double? GeographicElevation { get; private set; }
        public string Status { get; private set; }
        public string Warning { get; private set; }
        public bool HasGeographicCoordinate { get { return Longitude.HasValue && Latitude.HasValue; } }

        private CoordinateInspectionResult() { }

        public static CoordinateInspectionResult Create(CoordinateObservation observation, string drawingCrs, string geographicCrs, double? longitude, double? latitude, double? elevation, string status, string warning)
        {
            if (observation == null) throw new ArgumentNullException("observation");
            return new CoordinateInspectionResult { Observation = observation, DrawingCrs = drawingCrs ?? string.Empty, GeographicCrs = geographicCrs ?? string.Empty, Longitude = longitude, Latitude = latitude, GeographicElevation = elevation, Status = status ?? string.Empty, Warning = warning ?? string.Empty };
        }

        public string FormatDrawingCoordinate()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}: X={1:R}, Y={2:R}, Z={3:R} | CRS={4}", Observation.EntityType, Observation.X, Observation.Y, Observation.Z, DrawingCrs);
        }
        public string FormatGeographicCoordinate()
        {
            if (!HasGeographicCoordinate) return "Geographic coordinate unavailable";
            return string.Format(CultureInfo.InvariantCulture, "Longitude={0:R}, Latitude={1:R}, Elevation={2:R} | CRS={3}", Longitude.Value, Latitude.Value, GeographicElevation ?? Observation.Z, GeographicCrs);
        }
    }
}
