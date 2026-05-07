using ChomikEngine.Models;
using ChomikEngine.Parameters;

namespace ChomikEngine.Geometry;

/// <summary>
/// Generates a threaded bolt that screws into the WheelGenerator hub (female thread).
///
/// Thread parameters mirror WheelGenerator internal thread constants.
/// Bolt is oriented along Z axis, head at Z=0, thread end at Z = BoltLength.
/// </summary>
public static class BoltGenerator
{
    // Must match WheelGenerator constants
    const float ThreadNominalR = 5.5f;
    const float ThreadDepth    = 0.9f;
    const float ThreadPitch    = 3f;
    const int   ThreadSegR     = 48;

    const float HeadR      = 8f;    // hex-head outer radius
    const float HeadH      = 6f;    // head height
    const float ShaftR     = ThreadNominalR - 0.3f; // slight clearance fit
    const int   Seg        = 48;

    public static Mesh CreateBolt(float totalLengthMm, float threadLengthMm)
    {
        var mesh = new Mesh { Name = "bolt" };
        float shaftLen  = totalLengthMm - HeadH;
        float smoothLen = shaftLen - threadLengthMm;

        // 1. Bolt head (hexagonal prism at z = 0..HeadH)
        AddHexHead(mesh);

        // 2. Smooth shaft (z = HeadH .. HeadH + smoothLen)
        CylOuter(mesh, ShaftR, HeadH, HeadH + smoothLen, Seg);

        // 3. Chamfer head→shaft transition
        CylOuter(mesh, HeadR, HeadH - 0.5f, HeadH, 6); // rough chamfer

        // 4. End caps
        Annulus(mesh, 0f, ShaftR, HeadH + shaftLen, Seg, facePosZ: true);  // tip
        Annulus(mesh, 0f, HeadR, 0f, Seg, facePosZ: false);                // head bottom

        // 5. Threaded section (external thread)
        float tStart = HeadH + smoothLen;
        float tEnd   = HeadH + shaftLen;
        AddExternalThread(mesh, ShaftR, ThreadDepth, ThreadPitch, tStart, tEnd);

        return mesh;
    }

    // Bolt dimensions helper (for standard length derived from hub length)
    public static (float total, float threaded) GetDefaultLengths()
    {
        float hubLen   = 24f;           // matches WheelGenerator.HubLength
        float wallThk  = 3f;            // base plate thickness
        float extra    = 10f;           // for nut on far side
        float total    = HeadH + hubLen + wallThk + extra;
        float threaded = hubLen + 4f;
        return (total, threaded);
    }

    // ── Hex head ─────────────────────────────────────────────────────────
    static void AddHexHead(Mesh mesh)
    {
        int sides = 6;
        float z0 = 0f, z1 = HeadH;
        var centers = new Vector3(0, 0, z0);
        var centert = new Vector3(0, 0, z1);

        for (int i = 0; i < sides; i++)
        {
            float a = Ang(i, sides), b = Ang(i + 1, sides);
            var pa0 = P(HeadR, a, z0); var pb0 = P(HeadR, b, z0);
            var pa1 = P(HeadR, a, z1); var pb1 = P(HeadR, b, z1);
            // Side face
            mesh.Add(T(pa0, pa1, pb0)); mesh.Add(T(pb0, pa1, pb1));
            // Bottom
            mesh.Add(T(centers, pb0, pa0));
            // Top
            mesh.Add(T(centert, pa1, pb1));
        }
    }

    // ── External (male) thread ────────────────────────────────────────────
    static void AddExternalThread(Mesh mesh, float rBase, float depth,
                                   float pitch, float zStart, float zEnd)
    {
        if (zEnd - zStart < pitch) return;

        float revolutions = (zEnd - zStart) / pitch;
        int   totalSteps  = (int)(revolutions * ThreadSegR);
        float dA = MathF.PI * 2f / ThreadSegR;
        float dZ = pitch / ThreadSegR;

        // Smooth shaft surface under the thread
        CylOuter(mesh, rBase, zStart, zEnd, Seg);

        // Thread ridges on top of shaft
        for (int k = 0; k < totalSteps; k++)
        {
            float a0 = k       * dA;
            float a1 = (k + 1) * dA;
            float z0 = zStart + k       * dZ;
            float z1 = zStart + (k + 1) * dZ;
            float am = (a0 + a1) / 2f;
            float zm = (z0 + z1) / 2f;

            float rA0 = rBase + depth * ExternalThreadProfile(k, ThreadSegR);
            float rA1 = rBase + depth * ExternalThreadProfile(k + 1, ThreadSegR);
            float rM  = rBase + depth;

            var v0 = P(rA0, a0, z0);
            var v1 = P(rM,  am, zm);
            var v2 = P(rA1, a1, z1);
            var v3 = P(rBase, a0, z0);
            var v4 = P(rBase, a1, z1);

            mesh.Add(T(v0, v1, v2));
            mesh.Add(T(v3, v0, v4));
            mesh.Add(T(v4, v0, v2));
        }
    }

    static float ExternalThreadProfile(int step, int stepsPerRev)
    {
        float frac = (step % stepsPerRev) / (float)stepsPerRev;
        return MathF.Max(0f, 1f - MathF.Abs(frac - 0.5f) * 4f);
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    static void CylOuter(Mesh mesh, float r, float z0, float z1, int seg)
    {
        for (int i = 0; i < seg; i++)
        {
            float a = Ang(i, seg), b = Ang(i + 1, seg);
            mesh.Add(T(P(r,a,z0), P(r,a,z1), P(r,b,z0)));
            mesh.Add(T(P(r,b,z0), P(r,a,z1), P(r,b,z1)));
        }
    }

    static void Annulus(Mesh mesh, float r0, float r1, float z, int seg, bool facePosZ)
    {
        var center = new Vector3(0, 0, z);
        if (r0 < 0.01f)
        {
            for (int i = 0; i < seg; i++)
            {
                float a = Ang(i, seg), b = Ang(i + 1, seg);
                if (facePosZ) mesh.Add(T(center, P(r1,a,z), P(r1,b,z)));
                else          mesh.Add(T(center, P(r1,b,z), P(r1,a,z)));
            }
        }
        else
        {
            for (int i = 0; i < seg; i++)
            {
                float a = Ang(i, seg), b = Ang(i + 1, seg);
                var ia = P(r0,a,z); var ib = P(r0,b,z);
                var oa = P(r1,a,z); var ob = P(r1,b,z);
                if (facePosZ) { mesh.Add(T(ia,oa,ib)); mesh.Add(T(ib,oa,ob)); }
                else          { mesh.Add(T(ia,ib,oa)); mesh.Add(T(ib,ob,oa)); }
            }
        }
    }

    static Vector3 P(float r, float a, float z) =>
        new(r * MathF.Cos(a), r * MathF.Sin(a), z);

    static float Ang(int i, int total) => i * MathF.PI * 2f / total;

    static Triangle T(Vector3 a, Vector3 b, Vector3 c) => new(a, b, c);
}
