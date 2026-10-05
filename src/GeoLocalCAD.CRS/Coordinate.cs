using System;

namespace GeoLocalCAD.CRS
{
    public struct Coordinate
    {
        public double X; public double Y; public double Z;
        public Coordinate(double x, double y, double z = 0) { X = x; Y = y; Z = z; }
        public override string ToString() { return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}, {1}, {2}", X, Y, Z); }
    }

    public interface ICoordinateTransformer
    {
        Coordinate Transform(Coordinate coordinate, CrsIdentifier source, CrsIdentifier target);
    }
}
