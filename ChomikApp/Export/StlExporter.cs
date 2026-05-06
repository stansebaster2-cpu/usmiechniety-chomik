using System.Globalization;
using System.IO;
using ChomikEngine.Models;

namespace ChomikEngine.Export;

public static class StlExporter
{
    public static void Export(Mesh mesh, string path, bool ascii)
    {
        if (ascii)
        {
            using var writer = new StreamWriter(path);
            writer.WriteLine($"solid {mesh.Name}");
            foreach (var tri in mesh.Triangles)
            {
                writer.WriteLine("  facet normal 0 0 0");
                writer.WriteLine("    outer loop");
                writer.WriteLine($"      vertex {tri.A.X.ToString(CultureInfo.InvariantCulture)} {tri.A.Y.ToString(CultureInfo.InvariantCulture)} {tri.A.Z.ToString(CultureInfo.InvariantCulture)}");
                writer.WriteLine($"      vertex {tri.B.X.ToString(CultureInfo.InvariantCulture)} {tri.B.Y.ToString(CultureInfo.InvariantCulture)} {tri.B.Z.ToString(CultureInfo.InvariantCulture)}");
                writer.WriteLine($"      vertex {tri.C.X.ToString(CultureInfo.InvariantCulture)} {tri.C.Y.ToString(CultureInfo.InvariantCulture)} {tri.C.Z.ToString(CultureInfo.InvariantCulture)}");
                writer.WriteLine("    endloop");
                writer.WriteLine("  endfacet");
            }
            writer.WriteLine($"endsolid {mesh.Name}");
        }
        else
        {
            using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
            using var writer = new BinaryWriter(stream);
            writer.Write(new byte[80]);
            writer.Write(mesh.Triangles.Count);
            foreach (var tri in mesh.Triangles)
            {
                writer.Write(tri.Normal.X);
                writer.Write(tri.Normal.Y);
                writer.Write(tri.Normal.Z);
                writer.Write(tri.A.X);
                writer.Write(tri.A.Y);
                writer.Write(tri.A.Z);
                writer.Write(tri.B.X);
                writer.Write(tri.B.Y);
                writer.Write(tri.B.Z);
                writer.Write(tri.C.X);
                writer.Write(tri.C.Y);
                writer.Write(tri.C.Z);
                writer.Write((ushort)0);
            }
        }
    }
}
