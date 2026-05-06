using System.Globalization;
using System.IO;
using System.Text;
using ChomikEngine.Models;

namespace ChomikEngine.Geometry;

public static class StlImporter
{
    public static Mesh Load(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("Plik STL nie istnieje.", path);

        using var stream = File.OpenRead(path);
        return IsBinaryStl(stream)
            ? LoadBinary(stream, path)
            : LoadAscii(path);
    }

    // Binary STL: first 80 bytes = header, then 4-byte triangle count.
    // ASCII STL starts with "solid" keyword.
    static bool IsBinaryStl(Stream stream)
    {
        if (stream.Length < 84) return false;

        var header = new byte[80];
        stream.Read(header, 0, 80);
        stream.Position = 0;

        // If the first 80 bytes contain "solid" as ASCII text, it's almost certainly ASCII.
        string headerStr = Encoding.ASCII.GetString(header).TrimStart();
        if (headerStr.StartsWith("solid", StringComparison.OrdinalIgnoreCase))
        {
            // Still check triangle count matches file size (some binary files start with "solid")
            stream.Position = 80;
            var countBytes = new byte[4];
            stream.Read(countBytes, 0, 4);
            stream.Position = 0;
            uint count = BitConverter.ToUInt32(countBytes, 0);
            long expectedSize = 84 + count * 50L;
            return stream.Length == expectedSize;
        }

        return true;
    }

    static Mesh LoadBinary(Stream stream, string path)
    {
        var mesh = new Mesh { Name = Path.GetFileNameWithoutExtension(path) };
        using var reader = new BinaryReader(stream, Encoding.ASCII, leaveOpen: false);

        reader.ReadBytes(80); // skip header
        uint triangleCount = reader.ReadUInt32();

        for (uint i = 0; i < triangleCount; i++)
        {
            // Normal vector (ignored — we store normals per triangle but recalculate them)
            reader.ReadSingle(); reader.ReadSingle(); reader.ReadSingle();

            var v1 = ReadVec3(reader);
            var v2 = ReadVec3(reader);
            var v3 = ReadVec3(reader);

            reader.ReadUInt16(); // attribute byte count
            mesh.Add(new Triangle(v1, v2, v3));
        }

        return mesh;
    }

    static Vector3 ReadVec3(BinaryReader r) =>
        new(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());

    static Mesh LoadAscii(string path)
    {
        var mesh = new Mesh { Name = Path.GetFileNameWithoutExtension(path) };
        var vertices = new Vector3[3];
        int idx = 0;

        foreach (var raw in File.ReadLines(path))
        {
            var line = raw.Trim();
            if (!line.StartsWith("vertex ", StringComparison.OrdinalIgnoreCase)) continue;

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4) continue;

            if (float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) &&
                float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var y) &&
                float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var z))
            {
                vertices[idx++] = new Vector3(x, y, z);
                if (idx == 3)
                {
                    mesh.Add(new Triangle(vertices[0], vertices[1], vertices[2]));
                    idx = 0;
                }
            }
        }

        return mesh;
    }
}
