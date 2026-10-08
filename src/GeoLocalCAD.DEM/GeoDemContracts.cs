using System;
using System.Collections.Generic;
using GeoLocalCAD.CRS;
using GeoLocalCAD.Raster;

namespace GeoLocalCAD.DEM
{
    public sealed class DemSample
    {
        public Coordinate Location { get; private set; }
        public double Elevation { get; private set; }
        public DemSample(Coordinate location, double elevation) { if (double.IsNaN(elevation) || double.IsInfinity(elevation)) throw new ArgumentException("Elevation must be finite.", "elevation"); Location = location; Elevation = elevation; }
    }

    public sealed class DemGrid
    {
        private readonly double[,] values;
        public int Width { get; private set; } public int Height { get; private set; }
        public RasterGeoTransform GeoTransform { get; private set; }
        public CrsIdentifier Crs { get; private set; }
        public double? NoData { get; private set; }
        public DemGrid(double[,] values, RasterGeoTransform geoTransform, CrsIdentifier crs, double? noData)
        {
            if (values == null || values.GetLength(0) == 0 || values.GetLength(1) == 0) throw new ArgumentException("DEM grid values are required.", "values");
            if (geoTransform == null) throw new ArgumentNullException("geoTransform");
            if (crs == null) throw new ArgumentNullException("crs");
            this.values = values; Width = values.GetLength(0); Height = values.GetLength(1); GeoTransform = geoTransform; Crs = crs; NoData = noData;
        }
        public bool IsNoData(int column, int row) { var value = values[column, row]; return (NoData.HasValue && Math.Abs(value - NoData.Value) < 1e-12) || double.IsNaN(value) || double.IsInfinity(value); }
        public IEnumerable<DemSample> EnumerateValidSamples()
        {
            for (var row = 0; row < Height; row++) for (var column = 0; column < Width; column++) if (!IsNoData(column, row)) yield return new DemSample(GeoTransform.PixelToWorld(column + 0.5, row + 0.5), values[column, row]);
        }
    }

    public sealed class Civil3DSurfaceRequest
    {
        public string SurfaceName { get; private set; } public string LayerName { get; private set; } public double? MaximumRmse { get; private set; }
        public Civil3DSurfaceRequest(string surfaceName, string layerName, double? maximumRmse = null)
        {
            if (string.IsNullOrWhiteSpace(surfaceName)) throw new ArgumentException("Surface name is required.", "surfaceName");
            if (string.IsNullOrWhiteSpace(layerName)) throw new ArgumentException("Layer name is required.", "layerName");
            SurfaceName = surfaceName; LayerName = layerName; MaximumRmse = maximumRmse;
        }
    }

    public interface IDemSurfaceBuilder
    {
        int BuildTinSurface(DemGrid grid, Civil3DSurfaceRequest request);
    }
}
