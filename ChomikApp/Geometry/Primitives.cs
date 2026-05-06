using ChomikEngine.Models;

namespace ChomikEngine.Geometry;

public static class Primitives
{
    public static Mesh Transform(Mesh mesh, float dx, float dy, float dz)
    {
        var result = new Mesh { Name = mesh.Name };
        foreach (var tri in mesh.Triangles)
        {
            result.Add(new Triangle(
                new Vector3(tri.A.X + dx, tri.A.Y + dy, tri.A.Z + dz),
                new Vector3(tri.B.X + dx, tri.B.Y + dy, tri.B.Z + dz),
                new Vector3(tri.C.X + dx, tri.C.Y + dy, tri.C.Z + dz)));
        }
        return result;
    }
}
