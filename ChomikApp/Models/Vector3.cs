namespace ChomikEngine.Models;

public record struct Vector3(float X, float Y, float Z)
{
    public static Vector3 Zero => new(0, 0, 0);
    public static Vector3 operator +(Vector3 a, Vector3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vector3 operator -(Vector3 a, Vector3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Vector3 operator *(Vector3 a, float s) => new(a.X * s, a.Y * s, a.Z * s);
    public float Length => MathF.Sqrt(X * X + Y * Y + Z * Z);
    public Vector3 Normalized => Length < 1e-6f ? this : this * (1f / Length);

    public static Vector3 Cross(Vector3 a, Vector3 b)
        => new(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
}
