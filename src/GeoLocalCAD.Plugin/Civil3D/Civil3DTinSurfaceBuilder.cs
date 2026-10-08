using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.DatabaseServices;
using GeoLocalCAD.DEM;

namespace GeoLocalCAD.Plugin.Civil3D
{
    public sealed class Civil3DTinSurfaceBuilder : IDemSurfaceBuilder
    {
        private readonly Database database;
        public Civil3DTinSurfaceBuilder(Database database) { this.database = database ?? throw new ArgumentNullException("database"); }

        public int BuildTinSurface(DemGrid grid, Civil3DSurfaceRequest request)
        {
            if (grid == null) throw new ArgumentNullException("grid");
            if (request == null) throw new ArgumentNullException("request");
            var points = new Point3dCollection();
            foreach (var sample in grid.EnumerateValidSamples()) points.Add(new Point3d(sample.Location.X, sample.Location.Y, sample.Elevation));
            if (points.Count < 3) throw new InvalidOperationException("DEM contains fewer than three valid elevation samples; a TIN surface cannot be created.");

            using (var transaction = database.TransactionManager.StartTransaction())
            {
                var layerId = EnsureLayer(transaction, request.LayerName);
                var surfaceId = TinSurface.Create(database, request.SurfaceName);
                var surface = (TinSurface)transaction.GetObject(surfaceId, OpenMode.ForWrite);
                surface.LayerId = layerId;
                surface.AddVertices(points);
                transaction.Commit();
            }
            return points.Count;
        }

        private ObjectId EnsureLayer(Transaction transaction, string layerName)
        {
            var layers = (LayerTable)transaction.GetObject(database.LayerTableId, OpenMode.ForRead);
            if (layers.Has(layerName)) return layers[layerName];
            layers.UpgradeOpen();
            var record = new LayerTableRecord { Name = layerName };
            var id = layers.Add(record); transaction.AddNewlyCreatedDBObject(record, true); return id;
        }
    }
}
