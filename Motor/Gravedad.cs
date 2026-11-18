using System;

namespace Cataclismos.Motor
{
    /// <summary>
    /// Fuerza gravitacional directa O(N²) con suavizado de Plummer:
    ///   a_i = G Σ_j m_j (r_j - r_i) / (|r_j - r_i|² + ε²)^(3/2)
    /// </summary>
    public static class Gravedad
    {
        public static void CalcularAceleraciones(Sistema s, Vec3[] pos, Vec3[] acc)
        {
            int n = s.N;
            double g = s.G;
            double eps2 = s.Suavizado * s.Suavizado;

            for (int i = 0; i < n; i++) acc[i] = Vec3.Cero;

            // Cada par se calcula una sola vez (3ª ley de Newton)
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    Vec3 d = pos[j] - pos[i];
                    double r2 = d.Norma2 + eps2;
                    double inv = 1.0 / (r2 * Math.Sqrt(r2));
                    acc[i] = acc[i] + d * (g * s.Masa[j] * inv);
                    acc[j] = acc[j] - d * (g * s.Masa[i] * inv);
                }
            }
        }

        public static void CalcularAceleraciones(Sistema s) =>
            CalcularAceleraciones(s, s.Pos, s.Acc);
    }
}
