namespace ChomikEngine.Models;

public record struct Triangle(Vector3 A, Vector3 B, Vector3 C)
{
    public Vector3 Normal
    {
        get
        {
            var ab = B - A;
            var ac = C - A;
            return Vector3.Cross(ab, ac).Normalized;
        }
    }
}

public class Mesh
{
    public string Name { get; set; } = "mesh";
    public List<Triangle> Triangles { get; } = new();

    public void Add(Triangle t) => Triangles.Add(t);
    public void AddRange(IEnumerable<Triangle> ts) => Triangles.AddRange(ts);
    public void Merge(Mesh other) => Triangles.AddRange(other.Triangles);

    public (Vector3 Min, Vector3 Max) Bounds()
    {
        if (Triangles.Count == 0) return (Vector3.Zero, Vector3.Zero);

        float x0 = float.MaxValue, y0 = float.MaxValue, z0 = float.MaxValue;
        float x1 = float.MinValue, y1 = float.MinValue, z1 = float.MinValue;

        foreach (var t in Triangles)
        foreach (var v in new[] { t.A, t.B, t.C })
        {
            x0 = MathF.Min(x0, v.X); y0 = MathF.Min(y0, v.Y); z0 = MathF.Min(z0, v.Z);
            x1 = MathF.Max(x1, v.X); y1 = MathF.Max(y1, v.Y); z1 = MathF.Max(z1, v.Z);
        }

        return (new Vector3(x0, y0, z0), new Vector3(x1, y1, z1));
    }

    public Vector3 Center()
    {
        var (mn, mx) = Bounds();
        return new((mn.X + mx.X) / 2f, (mn.Y + mx.Y) / 2f, (mn.Z + mx.Z) / 2f);
    }

    public static Mesh Combine(params Mesh?[] meshes)
    {
        var result = new Mesh();
        foreach (var mesh in meshes)
        {
            if (mesh is not null)
                result.Merge(mesh);
        }
        return result;
    }
}
