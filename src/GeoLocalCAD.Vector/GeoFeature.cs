using System.Collections.Generic;
using GeoLocalCAD.CRS;

namespace GeoLocalCAD.Vector
{
    public sealed class GeoFeature
    {
        public string GeometryType { get; set; }
        public List<List<Coordinate>> Parts { get; private set; }
        public Dictionary<string, object> Attributes { get; private set; }
        public GeoFeature() { Parts = new List<List<Coordinate>>(); Attributes = new Dictionary<string, object>(); }
    }
    public sealed class GeoJsonDataset
    {
        public CrsIdentifier SourceCrs { get; private set; }
        public List<GeoFeature> Features { get; private set; }
        public GeoJsonDataset(CrsIdentifier sourceCrs) { SourceCrs = sourceCrs; Features = new List<GeoFeature>(); }
    }
}
