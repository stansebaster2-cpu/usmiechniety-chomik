using ChomikEngine.Data;
using ChomikEngine.Models;
using ChomikEngine.Parameters;

namespace ChomikEngine.Geometry;

public static class BaseGenerator
{
    const int BearingSegments = 32;
    const float PillarWallMm = 3f;

    public static Mesh CreateBase(BaseParameters parameters, WheelParameters wheel)
    {
        var mesh = new Mesh { Name = "base" };

        float width = wheel.DiameterMm + 40f;
        float depth = wheel.TrackWidthMm + 20f;
        float plateHeight = 10f;

        AddBox(mesh, width, depth, plateHeight);

        var bearing = parameters.GetBearing();
        if (bearing is null) return mesh;

        float socketOuterR = bearing.OuterDiamMm / 2f + PillarWallMm;
        float socketInnerR = bearing.OuterDiamMm / 2f + parameters.SocketToleranceMm;
        float pillarHeight = bearing.WidthMm + 2f;
        float pillarBaseZ = plateHeight;

        for (int k = 0; k < parameters.BearingCount; k++)
        {
            float angle = k * MathF.PI * 2f / parameters.BearingCount;
            float cx = parameters.BearingRadiusMm * MathF.Cos(angle);
            float cy = parameters.BearingRadiusMm * MathF.Sin(angle);

            AddHollowCylinder(mesh, cx, cy, pillarBaseZ, pillarBaseZ + pillarHeight,
                               socketOuterR, socketInnerR, BearingSegments);
        }

        return mesh;
    }

    public static Mesh CreateTestSocket(BaseParameters parameters)
    {
        var mesh = new Mesh { Name = "test_socket" };
        var bearing = parameters.GetBearing();

        float outerR = bearing is not null
            ? bearing.OuterDiamMm / 2f + PillarWallMm
            : 10f;
        float innerR = bearing is not null
            ? bearing.OuterDiamMm / 2f + parameters.SocketToleranceMm
            : 7f;
        float height = bearing is not null ? bearing.WidthMm + 2f : 10f;

        AddHollowCylinder(mesh, 0f, 0f, 0f, height, outerR, innerR, BearingSegments);
        return mesh;
    }

    // ---- Helpers ----

    static void AddBox(Mesh mesh, float width, float depth, float height)
    {
        float x0 = -width / 2, x1 = width / 2;
        float y0 = -depth / 2, y1 = depth / 2;

        var p1 = new Vector3(x0, y0, 0);
        var p2 = new Vector3(x1, y0, 0);
        var p3 = new Vector3(x1, y1, 0);
        var p4 = new Vector3(x0, y1, 0);
        var p5 = p1 with { Z = height };
        var p6 = p2 with { Z = height };
        var p7 = p3 with { Z = height };
        var p8 = p4 with { Z = height };

        // Sides
        mesh.Add(new Triangle(p1, p2, p5)); mesh.Add(new Triangle(p2, p6, p5));
        mesh.Add(new Triangle(p2, p3, p6)); mesh.Add(new Triangle(p3, p7, p6));
        mesh.Add(new Triangle(p3, p4, p7)); mesh.Add(new Triangle(p4, p8, p7));
        mesh.Add(new Triangle(p4, p1, p8)); mesh.Add(new Triangle(p1, p5, p8));
        // Bottom
        mesh.Add(new Triangle(p1, p4, p2)); mesh.Add(new Triangle(p2, p4, p3));
        // Top
        mesh.Add(new Triangle(p5, p6, p8)); mesh.Add(new Triangle(p6, p7, p8));
    }

    static void AddHollowCylinder(Mesh mesh, float cx, float cy, float zBot, float zTop,
                                   float outerR, float innerR, int segments)
    {
        for (int i = 0; i < segments; i++)
        {
            float a = i * MathF.PI * 2f / segments;
            float b = (i + 1) % segments * MathF.PI * 2f / segments;

            float oAx = cx + outerR * MathF.Cos(a), oAy = cy + outerR * MathF.Sin(a);
            float oBx = cx + outerR * MathF.Cos(b), oBy = cy + outerR * MathF.Sin(b);
            float iAx = cx + innerR * MathF.Cos(a), iAy = cy + innerR * MathF.Sin(a);
            float iBx = cx + innerR * MathF.Cos(b), iBy = cy + innerR * MathF.Sin(b);

            var oAb = new Vector3(oAx, oAy, zBot);
            var oBb = new Vector3(oBx, oBy, zBot);
            var oAt = new Vector3(oAx, oAy, zTop);
            var oBt = new Vector3(oBx, oBy, zTop);
            var iAb = new Vector3(iAx, iAy, zBot);
            var iBb = new Vector3(iBx, iBy, zBot);
            var iAt = new Vector3(iAx, iAy, zTop);
            var iBt = new Vector3(iBx, iBy, zTop);

            // Outer wall (faces outward)
            mesh.Add(new Triangle(oAb, oBb, oAt));
            mesh.Add(new Triangle(oBb, oBt, oAt));

            // Inner wall (faces inward — reversed winding)
            mesh.Add(new Triangle(iAb, iAt, iBb));
            mesh.Add(new Triangle(iBb, iAt, iBt));

            // Top ring (annulus)
            mesh.Add(new Triangle(oAt, oBt, iAt));
            mesh.Add(new Triangle(iBt, iAt, oBt));

            // Bottom ring (annulus, faces down)
            mesh.Add(new Triangle(oAb, iAb, oBb));
            mesh.Add(new Triangle(iBb, oBb, iAb));
        }
    }
}
