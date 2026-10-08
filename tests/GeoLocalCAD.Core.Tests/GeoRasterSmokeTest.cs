using System;
using System.IO;
using GeoLocalCAD.Raster;

namespace GeoLocalCAD.Core.Tests
{
    internal static class GeoRasterSmokeTest
    {
        public static void Run()
        {
            var dir = Path.Combine(Path.GetTempPath(), "geolocal-raster-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(dir);
            var png = Path.Combine(dir, "sample.png"); File.WriteAllBytes(png, Png1x1());
            File.WriteAllText(Path.Combine(dir, "sample.pgw"), "2\n0\n0\n-2\n101\n199\n");
            var metadata = new RasterMetadataReader().Read(png);
            if (metadata.Width != 1 || metadata.Height != 1 || !metadata.IsGeoreferenced) throw new InvalidOperationException("PNG/World File metadata failed.");
            var world = metadata.GeoTransform.PixelToWorld(0, 0); if (Math.Abs(world.X - 100) > 1e-9 || Math.Abs(world.Y - 200) > 1e-9) throw new InvalidOperationException("World File pixel-to-world conversion failed: " + world);
            var extent = metadata.Extent; if (Math.Abs(extent.MinX - 100) > 1e-9 || Math.Abs(extent.MaxY - 200) > 1e-9) throw new InvalidOperationException("Raster extent failed.");
            var pixel = metadata.GeoTransform.WorldToPixel(100, 200); if (Math.Abs(pixel.X) > 1e-9 || Math.Abs(pixel.Y) > 1e-9) throw new InvalidOperationException("World-to-pixel conversion failed.");
            var tif = Path.Combine(dir, "sample.tif"); WriteMinimalGeoTiff(tif);
            var tifMetadata = new RasterMetadataReader().Read(tif);
            if (tifMetadata.Width != 2 || tifMetadata.Height != 2 || tifMetadata.Crs == null || tifMetadata.Crs.CanonicalName != "EPSG:32638") throw new InvalidOperationException("GeoTIFF metadata/GeoKey parsing failed.");
            Directory.Delete(dir, true); Console.WriteLine("Phase 5 Raster smoke test passed.");
        }
        private static void WriteMinimalGeoTiff(string path)
        {
            using (var b = new BinaryWriter(File.Create(path)))
            {
                b.Write((byte)'I'); b.Write((byte)'I'); b.Write((ushort)42); b.Write((uint)8); b.Write((ushort)9);
                WriteShortEntry(b, 256, 2); WriteShortEntry(b, 257, 2); WriteShortEntry(b, 258, 8); WriteShortEntry(b, 259, 1); WriteShortEntry(b, 262, 1); WriteShortEntry(b, 277, 1);
                WriteOffsetEntry(b, 33550, 3, 122); WriteOffsetEntry(b, 33922, 6, 146); WriteOffsetEntry(b, 34735, 8, 194); b.Write((uint)0);
                b.Write(1.0); b.Write(1.0); b.Write(0.0); b.Write(0.0); b.Write(0.0); b.Write(0.0); b.Write(1000.0); b.Write(2000.0); b.Write(0.0);
                var keys = new ushort[] { 1, 1, 0, 1, 3072, 0, 1, 32638 }; foreach (var key in keys) b.Write(key);
            }
        }
        private static void WriteShortEntry(BinaryWriter b, ushort tag, ushort value) { b.Write(tag); b.Write((ushort)3); b.Write((uint)1); b.Write(value); b.Write((ushort)0); }
        private static void WriteOffsetEntry(BinaryWriter b, ushort tag, uint count, uint offset) { b.Write(tag); b.Write((ushort)(tag == 34735 ? 3 : 12)); b.Write(count); b.Write(offset); }
        private static byte[] Png1x1() { return new byte[] { 137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,1,0,0,0,1,8,2,0,0,0,144,119,83,222,0,0,0,0,73,68,65,84,8,215,99,248,207,192,240,31,0,5,0,1,255,137,153,61,29,0,0,0,0,73,69,78,68,174,66,96,130 }; }
    }
}
