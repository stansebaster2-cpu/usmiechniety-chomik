using ChomikEngine.Data;
using ChomikEngine.Models;
using ChomikEngine.Parameters;

namespace ChomikEngine.Geometry;

public static class WheelGenerator
{
    public static Mesh CreateWheel(WheelParameters parameters)
    {
        var mesh = new Mesh { Name = "wheel" };
        int segments = parameters.Segments;
        float radius = parameters.DiameterMm / 2f;
        float halfWidth = parameters.TrackWidthMm / 2f;
        float wall = parameters.WallThicknessMm;
        float innerRadius = radius - wall;

        AddOuterSurface(mesh, parameters, segments, radius, halfWidth);
        AddInnerSurface(mesh, segments, innerRadius, halfWidth);
        AddSideRings(mesh, segments, radius, innerRadius, halfWidth);

        return mesh;
    }

    static void AddOuterSurface(Mesh mesh, WheelParameters p, int segments, float radius, float halfWidth)
    {
        switch (p.Surface)
        {
            case SurfaceType.Ribbed:
                AddRibbedSurface(mesh, p, segments, radius, halfWidth);
                break;
            case SurfaceType.GridTexture:
                AddGridSurface(mesh, p, segments, radius, halfWidth);
                break;
            default:
                AddSmoothSurface(mesh, segments, radius, halfWidth);
                break;
        }
    }

    static void AddSmoothSurface(Mesh mesh, int segments, float radius, float halfWidth)
    {
        for (int i = 0; i < segments; i++)
        {
            float a = i * MathF.PI * 2f / segments;
            float b = (i + 1) % segments * MathF.PI * 2f / segments;
            var p1 = new Vector3(radius * MathF.Cos(a), radius * MathF.Sin(a), -halfWidth);
            var p2 = new Vector3(radius * MathF.Cos(b), radius * MathF.Sin(b), -halfWidth);
            var p3 = new Vector3(radius * MathF.Cos(a), radius * MathF.Sin(a), halfWidth);
            var p4 = new Vector3(radius * MathF.Cos(b), radius * MathF.Sin(b), halfWidth);
            mesh.Add(new Triangle(p1, p2, p3));
            mesh.Add(new Triangle(p3, p2, p4));
        }
    }

    static void AddRibbedSurface(Mesh mesh, WheelParameters p, int segments, float radius, float halfWidth)
    {
        float ribHeight = p.RibHeightMm;
        float ribSpacing = p.RibSpacingMm;
        float circumference = 2f * MathF.PI * radius;
        int ribCount = Math.Max(1, (int)(circumference / ribSpacing));

        // Divide each segment into rib vs. valley based on angular position
        int subsPerSeg = 4;
        int totalSubs = segments * subsPerSeg;

        for (int i = 0; i < totalSubs; i++)
        {
            float a = i * MathF.PI * 2f / totalSubs;
            float b = (i + 1) * MathF.PI * 2f / totalSubs;

            // Height: raised at rib position, normal at valley
            float rA = radius + ribHeight * RibProfile(a, ribCount);
            float rB = radius + ribHeight * RibProfile(b, ribCount);

            var p1 = new Vector3(rA * MathF.Cos(a), rA * MathF.Sin(a), -halfWidth);
            var p2 = new Vector3(rB * MathF.Cos(b), rB * MathF.Sin(b), -halfWidth);
            var p3 = new Vector3(rA * MathF.Cos(a), rA * MathF.Sin(a), halfWidth);
            var p4 = new Vector3(rB * MathF.Cos(b), rB * MathF.Sin(b), halfWidth);
            mesh.Add(new Triangle(p1, p2, p3));
            mesh.Add(new Triangle(p3, p2, p4));
        }
    }

    // Returns 0..1 raised-cosine rib profile
    static float RibProfile(float angle, int ribCount)
    {
        float phase = (angle * ribCount) % (MathF.PI * 2f);
        return MathF.Max(0f, MathF.Cos(phase));
    }

    static void AddGridSurface(Mesh mesh, WheelParameters p, int segments, float radius, float halfWidth)
    {
        float ribHeight = p.RibHeightMm;
        float ribSpacing = p.RibSpacingMm;
        float circumference = 2f * MathF.PI * radius;
        int angularRibs = Math.Max(1, (int)(circumference / ribSpacing));
        float trackWidth = halfWidth * 2f;
        int axialRibs = Math.Max(1, (int)(trackWidth / ribSpacing));

        int subsPerSeg = 4;
        int totalSubs = segments * subsPerSeg;
        // Axial subdivisions to match rib resolution
        int axialSubs = axialRibs * 4;

        for (int i = 0; i < totalSubs; i++)
        {
            float a = i * MathF.PI * 2f / totalSubs;
            float b = (i + 1) * MathF.PI * 2f / totalSubs;

            for (int j = 0; j < axialSubs; j++)
            {
                float z1 = -halfWidth + j * (halfWidth * 2f) / axialSubs;
                float z2 = -halfWidth + (j + 1) * (halfWidth * 2f) / axialSubs;

                float angProfileA = RibProfile(a, angularRibs);
                float angProfileB = RibProfile(b, angularRibs);
                float axProfileZ1 = RibProfile(z1 / halfWidth * axialRibs, axialRibs);
                float axProfileZ2 = RibProfile(z2 / halfWidth * axialRibs, axialRibs);

                // Grid = product of angular and axial rib profiles
                float rA1 = radius + ribHeight * angProfileA * axProfileZ1;
                float rB1 = radius + ribHeight * angProfileB * axProfileZ1;
                float rA2 = radius + ribHeight * angProfileA * axProfileZ2;
                float rB2 = radius + ribHeight * angProfileB * axProfileZ2;

                var v1 = new Vector3(rA1 * MathF.Cos(a), rA1 * MathF.Sin(a), z1);
                var v2 = new Vector3(rB1 * MathF.Cos(b), rB1 * MathF.Sin(b), z1);
                var v3 = new Vector3(rA2 * MathF.Cos(a), rA2 * MathF.Sin(a), z2);
                var v4 = new Vector3(rB2 * MathF.Cos(b), rB2 * MathF.Sin(b), z2);

                mesh.Add(new Triangle(v1, v2, v3));
                mesh.Add(new Triangle(v3, v2, v4));
            }
        }
    }

    static void AddInnerSurface(Mesh mesh, int segments, float innerRadius, float halfWidth)
    {
        for (int i = 0; i < segments; i++)
        {
            float a = i * MathF.PI * 2f / segments;
            float b = (i + 1) % segments * MathF.PI * 2f / segments;
            // Inner surface faces inward — reversed winding
            var p1 = new Vector3(innerRadius * MathF.Cos(a), innerRadius * MathF.Sin(a), -halfWidth);
            var p2 = new Vector3(innerRadius * MathF.Cos(b), innerRadius * MathF.Sin(b), -halfWidth);
            var p3 = new Vector3(innerRadius * MathF.Cos(a), innerRadius * MathF.Sin(a), halfWidth);
            var p4 = new Vector3(innerRadius * MathF.Cos(b), innerRadius * MathF.Sin(b), halfWidth);
            mesh.Add(new Triangle(p3, p2, p1));
            mesh.Add(new Triangle(p4, p2, p3));
        }
    }

    static void AddSideRings(Mesh mesh, int segments, float outerR, float innerR, float halfWidth)
    {
        for (int i = 0; i < segments; i++)
        {
            float a = i * MathF.PI * 2f / segments;
            float b = (i + 1) % segments * MathF.PI * 2f / segments;

            foreach (float z in new[] { -halfWidth, halfWidth })
            {
                var oA = new Vector3(outerR * MathF.Cos(a), outerR * MathF.Sin(a), z);
                var oB = new Vector3(outerR * MathF.Cos(b), outerR * MathF.Sin(b), z);
                var iA = new Vector3(innerR * MathF.Cos(a), innerR * MathF.Sin(a), z);
                var iB = new Vector3(innerR * MathF.Cos(b), innerR * MathF.Sin(b), z);

                // Side ring faces outward along Z axis
                if (z < 0)
                {
                    mesh.Add(new Triangle(oA, iA, oB));
                    mesh.Add(new Triangle(oB, iA, iB));
                }
                else
                {
                    mesh.Add(new Triangle(oA, oB, iA));
                    mesh.Add(new Triangle(oB, iB, iA));
                }
            }
        }
    }
}
