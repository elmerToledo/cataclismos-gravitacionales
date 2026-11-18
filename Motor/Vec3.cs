using System;

namespace Cataclismos.Motor
{
    /// <summary>Vector 3D de doble precisión (Unity usa float, que no alcanza para órbitas largas).</summary>
    public readonly struct Vec3
    {
        public readonly double X, Y, Z;

        public Vec3(double x, double y, double z) { X = x; Y = y; Z = z; }

        public static readonly Vec3 Cero = new Vec3(0, 0, 0);

        public static Vec3 operator +(Vec3 a, Vec3 b) => new Vec3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Vec3 operator -(Vec3 a, Vec3 b) => new Vec3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Vec3 operator -(Vec3 a) => new Vec3(-a.X, -a.Y, -a.Z);
        public static Vec3 operator *(Vec3 a, double k) => new Vec3(a.X * k, a.Y * k, a.Z * k);
        public static Vec3 operator *(double k, Vec3 a) => new Vec3(a.X * k, a.Y * k, a.Z * k);
        public static Vec3 operator /(Vec3 a, double k) => new Vec3(a.X / k, a.Y / k, a.Z / k);

        public double Norma2 => X * X + Y * Y + Z * Z;
        public double Norma => Math.Sqrt(Norma2);

        public static double Punto(Vec3 a, Vec3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

        public static Vec3 Cruz(Vec3 a, Vec3 b) => new Vec3(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X);

        public override string ToString() => $"({X:G6}, {Y:G6}, {Z:G6})";
    }
}
