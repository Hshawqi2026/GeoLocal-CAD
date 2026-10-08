using System;
using System.Globalization;
using GeoLocalCAD.CRS;

namespace GeoLocalCAD.Raster
{
    public sealed class RasterGeoTransform
    {
        public double PixelWidth { get; private set; }
        public double RotationX { get; private set; }
        public double RotationY { get; private set; }
        public double PixelHeight { get; private set; }
        public double OriginX { get; private set; }
        public double OriginY { get; private set; }
        public RasterGeoTransform(double pixelWidth, double rotationX, double rotationY, double pixelHeight, double originX, double originY)
        {
            PixelWidth = pixelWidth; RotationX = rotationX; RotationY = rotationY; PixelHeight = pixelHeight; OriginX = originX; OriginY = originY;
        }
        public Coordinate PixelToWorld(double column, double row)
        {
            return new Coordinate(OriginX + column * PixelWidth + row * RotationX, OriginY + column * RotationY + row * PixelHeight, 0);
        }
        public Coordinate WorldToPixel(double x, double y)
        {
            var determinant = PixelWidth * PixelHeight - RotationX * RotationY;
            if (Math.Abs(determinant) < 1e-15) throw new InvalidOperationException("Raster geotransform is singular.");
            var dx = x - OriginX; var dy = y - OriginY;
            return new Coordinate((dx * PixelHeight - dy * RotationX) / determinant, (dy * PixelWidth - dx * RotationY) / determinant, 0);
        }
        public RasterExtent GetExtent(int width, int height)
        {
            var p0 = PixelToWorld(0, 0); var p1 = PixelToWorld(width, 0); var p2 = PixelToWorld(0, height); var p3 = PixelToWorld(width, height);
            return new RasterExtent(Math.Min(Math.Min(p0.X, p1.X), Math.Min(p2.X, p3.X)), Math.Min(Math.Min(p0.Y, p1.Y), Math.Min(p2.Y, p3.Y)), Math.Max(Math.Max(p0.X, p1.X), Math.Max(p2.X, p3.X)), Math.Max(Math.Max(p0.Y, p1.Y), Math.Max(p2.Y, p3.Y)));
        }
        public override string ToString() { return string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4},{5}", PixelWidth, RotationX, RotationY, PixelHeight, OriginX, OriginY); }
    }

    public sealed class RasterExtent
    {
        public double MinX { get; private set; } public double MinY { get; private set; } public double MaxX { get; private set; } public double MaxY { get; private set; }
        public RasterExtent(double minX, double minY, double maxX, double maxY) { MinX = minX; MinY = minY; MaxX = maxX; MaxY = maxY; }
    }

    public sealed class RasterMetadata
    {
        public string Path { get; internal set; } public string Format { get; internal set; } public int Width { get; internal set; } public int Height { get; internal set; }
        public int Bands { get; internal set; } public string DataType { get; internal set; } public double? NoData { get; internal set; }
        public CrsIdentifier Crs { get; internal set; } public RasterGeoTransform GeoTransform { get; internal set; } public RasterExtent Extent { get; internal set; }
        public bool IsGeoreferenced { get { return GeoTransform != null; } }
        public int ResolutionX { get { return Width; } } public int ResolutionY { get { return Height; } }
    }
}
