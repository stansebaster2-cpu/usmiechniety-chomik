using System.IO;
using System.Text;
using ChomikEngine.Models;

namespace ChomikEngine.Export;

public static class PlyExporter
{
    public static void Export(Mesh mesh, string path)
    {
        using var writer = new StreamWriter(path, false, Encoding.ASCII);
        writer.WriteLine("ply");
        writer.WriteLine("format ascii 1.0");
        writer.WriteLine($"element vertex {mesh.Triangles.Count * 3}");
        writer.WriteLine("property float x");
        writer.WriteLine("property float y");
        writer.WriteLine("property float z");
        writer.WriteLine($"element face {mesh.Triangles.Count}");
        writer.WriteLine("property list uchar int vertex_indices");
        writer.WriteLine("end_header");

        foreach (var tri in mesh.Triangles)
        {
            writer.WriteLine($"{tri.A.X} {tri.A.Y} {tri.A.Z}");
            writer.WriteLine($"{tri.B.X} {tri.B.Y} {tri.B.Z}");
            writer.WriteLine($"{tri.C.X} {tri.C.Y} {tri.C.Z}");
        }

        for (var i = 0; i < mesh.Triangles.Count; i++)
        {
            writer.WriteLine($"3 {i * 3} {i * 3 + 1} {i * 3 + 2}");
        }
    }
}
