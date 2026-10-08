using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using GeoLocalCAD.CRS;

namespace GeoLocalCAD.Raster
{
    public sealed class RasterMetadataReader
    {
        public RasterMetadata Read(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) throw new FileNotFoundException("Raster file was not found.", path);
            var extension = Path.GetExtension(path).ToLowerInvariant(); var metadata = new RasterMetadata { Path = path, Format = extension.TrimStart('.').ToUpperInvariant(), Bands = 1, DataType = "Unknown" };
            if (extension == ".tif" || extension == ".tiff") ReadTiff(path, metadata);
            else if (extension == ".png") ReadPng(path, metadata);
            else if (extension == ".jpg" || extension == ".jpeg") ReadJpeg(path, metadata);
            else if (extension == ".bmp") ReadBmp(path, metadata);
            else throw new NotSupportedException("Raster format is not supported by the managed metadata reader: " + extension);
            var world = FindWorldFile(path); if (world != null) metadata.GeoTransform = ReadWorldFile(world, metadata.GeoTransform);
            var prj = Path.ChangeExtension(path, ".prj"); if (File.Exists(prj)) metadata.Crs = ReadPrj(prj);
            if (metadata.GeoTransform != null) metadata.Extent = metadata.GeoTransform.GetExtent(metadata.Width, metadata.Height);
            return metadata;
        }
        private static string FindWorldFile(string path)
        {
            var dir = Path.GetDirectoryName(path); var stem = Path.GetFileNameWithoutExtension(path); var ext = Path.GetExtension(path).ToLowerInvariant(); var candidates = new List<string>();
            if (ext == ".jpg" || ext == ".jpeg") candidates.Add(Path.Combine(dir, stem + ".jgw"));
            if (ext == ".tif" || ext == ".tiff") candidates.Add(Path.Combine(dir, stem + ".tfw"));
            if (ext == ".png") candidates.Add(Path.Combine(dir, stem + ".pgw"));
            candidates.Add(Path.Combine(dir, stem + ".wld")); candidates.Add(Path.Combine(dir, stem + ".world"));
            foreach (var candidate in candidates) if (File.Exists(candidate)) return candidate; return null;
        }
        private static RasterGeoTransform ReadWorldFile(string path, RasterGeoTransform existing)
        {
            var values = new List<double>(); foreach (var line in File.ReadAllLines(path)) { var s = line.Trim(); if (s.Length > 0) values.Add(double.Parse(s, CultureInfo.InvariantCulture)); }
            if (values.Count < 6) throw new InvalidDataException("World file must contain six numeric lines: A,D,B,E,C,F.");
            var a = values[0]; var d = values[1]; var b = values[2]; var e = values[3]; var c = values[4]; var f = values[5];
            // World-file C/F identify the centre of the upper-left pixel; convert to corner origin.
            return new RasterGeoTransform(a, b, d, e, c - (a + b) / 2.0, f - (d + e) / 2.0);
        }
        private static void ReadPng(string path, RasterMetadata m) { using (var b = new BinaryReader(File.OpenRead(path))) { var signature = b.ReadBytes(8); if (signature.Length != 8 || signature[0] != 137 || signature[1] != 80 || signature[2] != 78 || signature[3] != 71) throw new InvalidDataException("Invalid PNG signature."); b.ReadBytes(4); if (ReadUInt32(b.ReadBytes(4), 0, false) != 0x49484452) throw new InvalidDataException("PNG IHDR is missing."); m.Width = ReadBigEndianInt32(b); m.Height = ReadBigEndianInt32(b); b.ReadByte(); var colorType = b.ReadByte(); m.Bands = colorType == 6 ? 4 : (colorType == 2 ? 3 : 1); m.DataType = "PNG"; } }
        private static void ReadBmp(string path, RasterMetadata m) { using (var b = new BinaryReader(File.OpenRead(path))) { if (b.ReadUInt16() != 0x4D42) throw new InvalidDataException("Invalid BMP signature."); b.ReadBytes(16); m.Width = b.ReadInt32(); m.Height = Math.Abs(b.ReadInt32()); b.ReadBytes(2); m.Bands = b.ReadUInt16() / 8; m.DataType = "BMP"; } }
        private static void ReadJpeg(string path, RasterMetadata m)
        {
            using (var b = new BinaryReader(File.OpenRead(path))) { if (b.ReadByte() != 0xFF || b.ReadByte() != 0xD8) throw new InvalidDataException("Invalid JPEG signature."); while (b.BaseStream.Position < b.BaseStream.Length) { if (b.ReadByte() != 0xFF) continue; var marker = b.ReadByte(); while (marker == 0xFF) marker = b.ReadByte(); if (marker == 0xD9 || marker == 0xDA) break; var length = ReadBigEndianUInt16(b); if (marker >= 0xC0 && marker <= 0xC3) { b.ReadByte(); m.Height = ReadBigEndianUInt16(b); m.Width = ReadBigEndianUInt16(b); m.Bands = b.ReadByte(); m.DataType = "JPEG"; return; } b.BaseStream.Seek(length - 2, SeekOrigin.Current); } } throw new InvalidDataException("JPEG dimensions were not found.");
        }
        private static void ReadTiff(string path, RasterMetadata m)
        {
            using (var b = new BinaryReader(File.OpenRead(path))) { var little = b.ReadUInt16() == 0x4949; if (!little && b.BaseStream.Position != 2) throw new InvalidDataException("Unsupported TIFF byte order."); var magic = ReadU16(b, little); if (magic != 42) throw new InvalidDataException("Invalid TIFF signature."); var ifd = ReadU32(b, little); b.BaseStream.Position = ifd; var count = ReadU16(b, little); var tags = new Dictionary<ushort, byte[]>(); for (var i = 0; i < count; i++) { var tag = ReadU16(b, little); var type = ReadU16(b, little); var n = ReadU32(b, little); var size = TypeSize(type) * n; var inline = size <= 4; var pos = b.BaseStream.Position; byte[] data; if (inline) { data = b.ReadBytes(4); } else { var offset = ReadU32(b, little); var save = b.BaseStream.Position; b.BaseStream.Position = offset; data = b.ReadBytes((int)size); b.BaseStream.Position = save; } tags[tag] = data; if (!inline) { } else { } }
                m.Width = (int)ReadTagNumber(tags, 256, little); m.Height = (int)ReadTagNumber(tags, 257, little); m.Bands = (int)Math.Max(1, ReadTagNumber(tags, 277, little)); m.DataType = "TIFF";
                var scale = ReadDoubleArray(tags, 33550, little); var tie = ReadDoubleArray(tags, 33922, little); if (scale != null && scale.Length >= 2 && tie != null && tie.Length >= 6) { var ox = tie[3] - tie[0] * scale[0] - tie[1] * 0; var oy = tie[4] - tie[1] * scale[1] - tie[0] * 0; m.GeoTransform = new RasterGeoTransform(scale[0], 0, 0, -scale[1], ox, oy); }
                var keys = ReadU16Array(tags, 34735, little); if (keys != null && keys.Length >= 4) { for (var i = 4; i + 3 < keys.Length; i += 4) { var key = keys[i]; var value = keys[i + 3]; if ((key == 2048 || key == 3072) && value > 0) { try { m.Crs = CrsIdentifier.Parse("EPSG:" + value); } catch { } } } }
            }
        }
        private static CrsIdentifier ReadPrj(string path) { var text = File.ReadAllText(path); var marker = text.IndexOf("AUTHORITY[\"EPSG\"", StringComparison.OrdinalIgnoreCase); if (marker >= 0) { var start = text.IndexOf(',', marker); var end = text.IndexOf(']', start); if (start > 0 && end > start) { var code = text.Substring(start + 1, end - start - 1).Trim(' ', '"'); int n; if (int.TryParse(code, out n)) return CrsIdentifier.Parse("EPSG:" + n); } } throw new InvalidDataException("PRJ file does not expose an EPSG authority code; CRS must be supplied explicitly."); }
        private static int TypeSize(ushort type) { return type == 3 ? 2 : type == 4 ? 4 : type == 5 ? 8 : type == 12 ? 8 : 1; }
        private static double ReadTagNumber(Dictionary<ushort, byte[]> tags, ushort tag, bool little) { byte[] d; if (!tags.TryGetValue(tag, out d)) return 0; if (d.Length >= 8) return ReadDouble(d, 0, little); if (d.Length >= 4) return ReadUInt32(d, 0, little); if (d.Length >= 2) return ReadUInt16(d, 0, little); return d[0]; }
        private static double[] ReadDoubleArray(Dictionary<ushort, byte[]> tags, ushort tag, bool little) { byte[] d; if (!tags.TryGetValue(tag, out d) || d.Length < 8) return null; var result = new double[d.Length / 8]; for (var i = 0; i < result.Length; i++) result[i] = ReadDouble(d, i * 8, little); return result; }
        private static ushort[] ReadU16Array(Dictionary<ushort, byte[]> tags, ushort tag, bool little) { byte[] d; if (!tags.TryGetValue(tag, out d) || d.Length < 2) return null; var result = new ushort[d.Length / 2]; for (var i = 0; i < result.Length; i++) result[i] = ReadUInt16(d, i * 2, little); return result; }
        private static ushort ReadU16(BinaryReader b, bool little) { var x = b.ReadBytes(2); return ReadUInt16(x, 0, little); }
        private static uint ReadU32(BinaryReader b, bool little) { var x = b.ReadBytes(4); return ReadUInt32(x, 0, little); }
        private static ushort ReadBigEndianUInt16(BinaryReader b) { var x = b.ReadBytes(2); return ReadUInt16(x, 0, false); }
        private static int ReadBigEndianInt32(BinaryReader b) { return unchecked((int)ReadUInt32(b.ReadBytes(4), 0, false)); }
        private static ushort ReadUInt16(byte[] x, int p, bool little) { return little ? (ushort)(x[p] | x[p + 1] << 8) : (ushort)(x[p] << 8 | x[p + 1]); }
        private static uint ReadUInt32(byte[] x, int p, bool little) { return little ? (uint)(x[p] | x[p + 1] << 8 | x[p + 2] << 16 | x[p + 3] << 24) : (uint)(x[p] << 24 | x[p + 1] << 16 | x[p + 2] << 8 | x[p + 3]); }
        private static double ReadDouble(byte[] x, int p, bool little) { var b = new byte[8]; Buffer.BlockCopy(x, p, b, 0, 8); if (BitConverter.IsLittleEndian != little) Array.Reverse(b); return BitConverter.ToDouble(b, 0); }
    }
}
