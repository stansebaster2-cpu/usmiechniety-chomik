using ChomikEngine.Data;
using ChomikEngine.Models;
using ChomikEngine.Parameters;

namespace ChomikEngine.Geometry;

/// <summary>
/// Generates a printable hamster wheel drum.
///
/// Coordinate system:
///   Z = 0      → open front (chomik enters here) — flat on print bed
///   Z = Width  → closed back (disc + hub side)
///   Hub extends from Z = Width outward to Z = Width + HubLength
///
/// DiameterMm = INNER diameter (running surface, what matters for spine health).
/// </summary>
public static class WheelGenerator
{
    // ── Hub geometry constants ─────────────────────────────────────────────
    const float HubOuterR    = 11f;   // hub outer radius [mm]
    const float HubLength    = 24f;   // hub protrusion length beyond back disc [mm]
    const int   HubSeg       = 48;

    // Bearing seats (624ZZ: OD=13mm → radius 6.5mm, width=5mm, bore=4mm)
    const float BearingSeatedR  = 6.6f;  // hub inner radius at bearing seat (OD/2 + 0.1 tolerance)
    const float BearingWidth    = 5f;
    const float BearingSep      = 3f;    // gap between two bearing seats

    // Thread (custom 3D-print-friendly, nominal ≈M11)
    const float ThreadNominalR  = 5.5f;  // nominal inner thread radius of hub
    const float ThreadDepth     = 0.9f;  // ridge height
    const float ThreadPitch     = 3f;    // mm per revolution
    const int   ThreadSegR      = 48;    // angular steps per revolution

    // ─────────────────────────────────────────────────────────────────────
    public static Mesh CreateWheel(WheelParameters p)
    {
        var mesh = new Mesh { Name = "wheel" };
        float rI = p.DiameterMm / 2f;
        float rO = rI + p.WallThicknessMm;
        float w  = p.TrackWidthMm;
        int   seg = p.Segments;

        // 1 ── Outer cylindrical shell (smooth, structural)
        CylOuter(mesh, rO, 0f, w, seg);

        // 2 ── Inner running surface — circumferential step ridges
        InnerWithSteps(mesh, p, rI, w, seg);

        // 3 ── Front rim at Z=0 (open end, thin annular ring)
        Annulus(mesh, rI, rO, 0f, seg, facePosZ: false);

        // 4 ── Back disc at Z=w
        switch (p.Mount)
        {
            case MountType.Hanging:
                BackDiscHanging(mesh, p, rI, w, seg);
                AddHub(mesh, w);
                break;
            default:
                // Bearing-track: solid back disc (no hub), refinement later
                SolidDisc(mesh, rI, w, seg, facePosZ: true);
                break;
        }

        return mesh;
    }

    // ── 1. Outer smooth cylinder ──────────────────────────────────────────
    static void CylOuter(Mesh mesh, float r, float z0, float z1, int seg)
    {
        for (int i = 0; i < seg; i++)
        {
            float a = Ang(i, seg), b = Ang(i + 1, seg);
            // Normals outward → CCW from outside
            mesh.Add(T(P(r,a,z0), P(r,a,z1), P(r,b,z0)));
            mesh.Add(T(P(r,b,z0), P(r,a,z1), P(r,b,z1)));
        }
    }

    // ── 2. Inner running surface with circumferential step ridges ─────────
    static void InnerWithSteps(Mesh mesh, WheelParameters p, float rI, float w, int seg)
    {
        float stepH  = p.StepHeightMm;   // ridge height (inward = smaller radius)
        float stepW  = 1.5f;             // ridge width along Z [mm]
        float spacing = p.StepSpacingMm;

        // Build a list of (z, r) pairs describing the profile
        var profile = new List<(float z, float r)>();
        profile.Add((0f, rI));

        float pos = spacing;
        while (pos + stepW < w)
        {
            float z0 = pos - stepW / 2f;
            float z1 = pos + stepW / 2f;
            if (z0 > profile[^1].z + 0.01f) profile.Add((z0, rI));
            profile.Add((z0, rI - stepH)); // step up (smaller r = ridge protrudes inward)
            profile.Add((z1, rI - stepH));
            profile.Add((z1, rI));
            pos += spacing;
        }
        profile.Add((w, rI));

        // Generate inner cylinder sections + radial faces at r-transitions
        for (int k = 0; k < profile.Count - 1; k++)
        {
            var (za, ra) = profile[k];
            var (zb, rb) = profile[k + 1];

            if (MathF.Abs(zb - za) > 0.001f)
            {
                // Cylinder section — normals inward → reversed winding
                for (int i = 0; i < seg; i++)
                {
                    float a = Ang(i, seg), b = Ang(i + 1, seg);
                    mesh.Add(T(P(ra,a,za), P(rb,b,za), P(ra,b,za))); // wrong, needs correction
                    // Correct inner winding (normals toward center):
                    mesh.Add(T(P(ra,a,za), P(rb,a,zb), P(rb,b,zb)));
                    mesh.Add(T(P(ra,a,za), P(rb,b,zb), P(ra,b,za)));
                }
            }
            else if (MathF.Abs(rb - ra) > 0.001f)
            {
                // Radial transition face (step wall) at z=za
                // This annular disc caps the step edge
                bool ridgeRising = rb < ra; // going to smaller radius = ridge appearing
                Annulus(mesh, MathF.Min(ra, rb), MathF.Max(ra, rb), za, seg, facePosZ: ridgeRising);
            }
        }
    }

    // ── 3. Front rim (annular disc at z=0) — already called as Annulus ────

    // ── 4a. Back disc — hanging type ─────────────────────────────────────
    static void BackDiscHanging(Mesh mesh, WheelParameters p, float rI, float w, int seg)
    {
        float rHub = HubOuterR;

        switch (p.DiscPattern)
        {
            case DiscPattern.Full:
                Annulus(mesh, rHub, rI, w, seg, facePosZ: true);
                break;
            case DiscPattern.Spoked:
                SpokedDisc(mesh, p, rHub, rI, w);
                break;
            case DiscPattern.Cross:
            {
                var crossP = new WheelParameters
                {
                    DiameterMm      = p.DiameterMm,
                    TrackWidthMm    = p.TrackWidthMm,
                    WallThicknessMm = p.WallThicknessMm,
                    Segments        = p.Segments,
                    DiscPattern     = p.DiscPattern,
                    SpokeCount      = 4,
                    SpokeWidthMm    = p.SpokeWidthMm * 1.5f
                };
                SpokedDisc(mesh, crossP, rHub, rI, w);
                break;
            }
        }
    }

    static void SpokedDisc(Mesh mesh, WheelParameters p, float rHub, float rI, float w)
    {
        int n    = p.SpokeCount;
        float sw = p.SpokeWidthMm;
        float thick = 3f; // spoke/disc thickness [mm]
        float z0 = w - thick, z1 = w;

        // Thin disc ring at outer and inner rim
        Annulus(mesh, rHub,        rHub + 2f, w, n * 8, facePosZ: true);
        Annulus(mesh, rI - 2f,     rI,        w, n * 8, facePosZ: true);

        // Spokes as rectangular boxes
        for (int k = 0; k < n; k++)
        {
            float angle = k * MathF.PI * 2f / n;
            float cos   = MathF.Cos(angle);
            float sin   = MathF.Sin(angle);
            float hw    = sw / 2f; // half spoke width

            // 4 corner points per end face × 2 faces = spoke box
            // Spoke runs from rHub to rI along the angle direction
            // Width hw perpendicular to angle
            float px   = cos, py = sin;   // radial direction
            float qx   = -sin, qy = cos;  // tangential direction

            foreach (float z in new[] { z0, z1 })
            {
                var c1 = new Vector3(rHub * px + hw * qx, rHub * py + hw * qy, z);
                var c2 = new Vector3(rHub * px - hw * qx, rHub * py - hw * qy, z);
                var c3 = new Vector3(rI   * px + hw * qx, rI   * py + hw * qy, z);
                var c4 = new Vector3(rI   * px - hw * qx, rI   * py - hw * qy, z);

                if (z > w - 0.5f) { mesh.Add(T(c1, c3, c2)); mesh.Add(T(c2, c3, c4)); }
                else               { mesh.Add(T(c1, c2, c3)); mesh.Add(T(c2, c4, c3)); }
            }

            // Spoke side faces (4 long sides of the box)
            var e1a = new Vector3(rHub * px + hw * qx, rHub * py + hw * qy, z0);
            var e1b = new Vector3(rHub * px + hw * qx, rHub * py + hw * qy, z1);
            var e2a = new Vector3(rHub * px - hw * qx, rHub * py - hw * qy, z0);
            var e2b = new Vector3(rHub * px - hw * qx, rHub * py - hw * qy, z1);
            var e3a = new Vector3(rI   * px + hw * qx, rI   * py + hw * qy, z0);
            var e3b = new Vector3(rI   * px + hw * qx, rI   * py + hw * qy, z1);
            var e4a = new Vector3(rI   * px - hw * qx, rI   * py - hw * qy, z0);
            var e4b = new Vector3(rI   * px - hw * qx, rI   * py - hw * qy, z1);

            // Side 1 (positive Q)
            mesh.Add(T(e1a, e1b, e3a)); mesh.Add(T(e3a, e1b, e3b));
            // Side 2 (negative Q)
            mesh.Add(T(e2a, e4a, e2b)); mesh.Add(T(e4a, e4b, e2b));
            // Inner end face
            mesh.Add(T(e1a, e2a, e1b)); mesh.Add(T(e2a, e2b, e1b));
            // Outer end face
            mesh.Add(T(e3a, e3b, e4a)); mesh.Add(T(e4a, e3b, e4b));
        }
    }

    // ── 4b. Hub with thread and bearing recesses ──────────────────────────
    static void AddHub(Mesh mesh, float w)
    {
        float rO     = HubOuterR;
        float zStart = w;
        float zEnd   = w + HubLength;
        int   seg    = HubSeg;

        // Hub outer wall (smooth cylinder)
        CylOuter(mesh, rO, zStart, zEnd, seg);

        // Hub end cap (annulus at zEnd)
        Annulus(mesh, ThreadNominalR, rO, zEnd, seg, facePosZ: true);

        // Hub inner surface: bearing seats + thread zone
        // Zones along z (from zStart to zEnd):
        // [zStart, zStart+BearingWidth]          : bearing seat 1
        // [zStart+BW, zStart+BW+BearingSep]      : transition (tight: ThreadNominalR)
        // [zStart+BW+Sep, zEnd-BW-Sep]            : thread zone
        // [zEnd-BW-Sep, zEnd-Sep]                 : transition
        // [zEnd-Sep, zEnd]                        : bearing seat 2
        float bw  = BearingWidth;
        float sep = BearingSep;
        float br  = BearingSeatedR;
        float tr  = ThreadNominalR;

        var innerProfile = new List<(float z, float r)>
        {
            (zStart,               br),               // bearing seat 1 start
            (zStart + bw,          br),               // bearing seat 1 end
            (zStart + bw,          tr),               // step down to thread zone
            (zEnd   - bw - sep,    tr),               // thread zone end
            (zEnd   - bw - sep,    br),               // step up to bearing seat 2
            (zEnd   - sep,         br),               // bearing seat 2 end
            (zEnd   - sep,         tr),               // step down for end
            (zEnd,                 tr),
        };

        // Render bearing-seat sections as plain cylinders (inward normals)
        RenderInnerProfile(mesh, innerProfile, seg);

        // Thread on inner surface in thread zone
        float tZStart = zStart + bw;
        float tZEnd   = zEnd   - bw - sep;
        AddInternalThread(mesh, tr, ThreadDepth, ThreadPitch, tZStart, tZEnd, seg);
    }

    static void RenderInnerProfile(Mesh mesh, List<(float z, float r)> profile, int seg)
    {
        for (int k = 0; k < profile.Count - 1; k++)
        {
            var (za, ra) = profile[k];
            var (zb, rb) = profile[k + 1];

            if (MathF.Abs(zb - za) > 0.001f)
            {
                // Inner cylinder section (normals toward center)
                for (int i = 0; i < seg; i++)
                {
                    float a = Ang(i, seg), b = Ang(i + 1, seg);
                    mesh.Add(T(P(ra,a,za), P(rb,b,zb), P(rb,a,zb)));
                    mesh.Add(T(P(ra,a,za), P(ra,b,za), P(rb,b,zb)));
                }
            }
            else if (MathF.Abs(rb - ra) > 0.001f)
            {
                // Radial step face
                bool facePos = rb > ra; // going outward = face toward +z? no...
                // The step wall faces along z based on which direction we came from
                // At bearing-seat→thread transition: face toward -z (inward into hub)
                Annulus(mesh, MathF.Min(ra, rb), MathF.Max(ra, rb), za, seg,
                        facePosZ: rb < ra); // bearing seat ends: step goes inward → face +z
            }
        }
    }

    // ── Internal helix thread ─────────────────────────────────────────────
    /// <summary>
    /// Adds a triangular-profile internal (female) thread to a cylinder inner surface.
    /// The thread ridge extends OUTWARD from rBase (increasing radius = into hub wall).
    /// </summary>
    static void AddInternalThread(Mesh mesh, float rBase, float depth,
                                   float pitch, float zStart, float zEnd, int _)
    {
        if (zEnd - zStart < pitch) return;

        float revolutions = (zEnd - zStart) / pitch;
        int   stepsPerRev = ThreadSegR;
        int   totalSteps  = (int)(revolutions * stepsPerRev);
        float dA   = MathF.PI * 2f / stepsPerRev;
        float dZ   = pitch / stepsPerRev;

        for (int k = 0; k < totalSteps; k++)
        {
            float a0 = k       * dA;
            float a1 = (k + 1) * dA;
            float z0 = zStart + k       * dZ;
            float z1 = zStart + (k + 1) * dZ;

            // Thread profile: triangular (mid-point at peak)
            float am  = (a0 + a1) / 2f;
            float zm  = (z0 + z1) / 2f;

            float rA0 = rBase + depth * ThreadProfile(a0, k, stepsPerRev);
            float rA1 = rBase + depth * ThreadProfile(a1, k + 1, stepsPerRev);
            float rM  = rBase + depth; // peak

            // Two triangles per step on the inner surface
            var v0 = P(rA0, a0, z0);
            var v1 = P(rM,  am, zm);
            var v2 = P(rA1, a1, z1);

            // Back-face (flat base of thread groove on the cylinder surface)
            var v3 = P(rBase, a0, z0);
            var v4 = P(rBase, a1, z1);

            // Thread ridge (inward normals)
            mesh.Add(T(v0, v2, v1));      // ridge surface
            mesh.Add(T(v3, v0, v4));      // base flat face part 1
            mesh.Add(T(v4, v0, v2));      // base flat face part 2
        }
    }

    // Triangle profile function: 0 at valley, 1 at ridge peak
    static float ThreadProfile(float angle, int step, int stepsPerRev)
    {
        float frac = (step % stepsPerRev) / (float)stepsPerRev;
        return MathF.Max(0f, 1f - MathF.Abs(frac - 0.5f) * 4f); // triangle wave 0..1..0
    }

    // ── Solid disc (full filled circle) ───────────────────────────────────
    static void SolidDisc(Mesh mesh, float r, float z, int seg, bool facePosZ)
    {
        var center = new Vector3(0, 0, z);
        for (int i = 0; i < seg; i++)
        {
            float a = Ang(i, seg), b = Ang(i + 1, seg);
            var pa = P(r, a, z);
            var pb = P(r, b, z);
            if (facePosZ) mesh.Add(T(center, pa, pb));
            else          mesh.Add(T(center, pb, pa));
        }
    }

    // ── Annular disc (ring from r0 to r1 at given z) ──────────────────────
    static void Annulus(Mesh mesh, float r0, float r1, float z, int seg, bool facePosZ)
    {
        for (int i = 0; i < seg; i++)
        {
            float a = Ang(i, seg), b = Ang(i + 1, seg);
            var ia = P(r0, a, z); var ib = P(r0, b, z);
            var oa = P(r1, a, z); var ob = P(r1, b, z);
            if (facePosZ) { mesh.Add(T(ia, oa, ib)); mesh.Add(T(ib, oa, ob)); }
            else          { mesh.Add(T(ia, ib, oa)); mesh.Add(T(ib, ob, oa)); }
        }
    }

    // ── Primitives helpers ────────────────────────────────────────────────
    static Vector3 P(float r, float a, float z) =>
        new(r * MathF.Cos(a), r * MathF.Sin(a), z);

    static float Ang(int i, int total) => i * MathF.PI * 2f / total;

    static Triangle T(Vector3 a, Vector3 b, Vector3 c) => new(a, b, c);
}
