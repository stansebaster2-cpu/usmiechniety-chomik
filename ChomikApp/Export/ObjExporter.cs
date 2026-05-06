using System.IO;
using ChomikEngine.Models;

namespace ChomikEngine.Export;

public static class ObjExporter
{
    public static void Export(Mesh mesh, string path)
    {
        using var writer = new StreamWriter(path);
        var index = 1;
        foreach (var tri in mesh.Triangles)
        {
            writer.WriteLine($"v {tri.A.X} {tri.A.Y} {tri.A.Z}");
            writer.WriteLine($"v {tri.B.X} {tri.B.Y} {tri.B.Z}");
            writer.WriteLine($"v {tri.C.X} {tri.C.Y} {tri.C.Z}");
            writer.WriteLine($"f {index} {index + 1} {index + 2}");
            index += 3;
        }
    }
}
