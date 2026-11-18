using System;

namespace Cataclismos.Motor
{
    /// <summary>Magnitudes conservadas para validar el integrador.</summary>
    public static class Diagnostico
    {
        public static double EnergiaCinetica(Sistema s)
        {
            double k = 0;
            for (int i = 0; i < s.N; i++) k += 0.5 * s.Masa[i] * s.Vel[i].Norma2;
            return k;
        }

        /// <summary>Energía potencial con el MISMO suavizado que la fuerza: -G m_i m_j / sqrt(r² + ε²).</summary>
        public static double EnergiaPotencial(Sistema s)
        {
            double u = 0;
            double eps2 = s.Suavizado * s.Suavizado;
            for (int i = 0; i < s.N; i++)
                for (int j = i + 1; j < s.N; j++)
                    u -= s.G * s.Masa[i] * s.Masa[j] / Math.Sqrt((s.Pos[j] - s.Pos[i]).Norma2 + eps2);
            return u;
        }

        public static double Energia(Sistema s) => EnergiaCinetica(s) + EnergiaPotencial(s);

        /// <summary>Energía de giro de las esferas: Σ ½ I ω².</summary>
        public static double EnergiaRotacion(Sistema s)
        {
            double k = 0;
            for (int i = 0; i < s.N; i++) k += 0.5 * s.Inercia(i) * s.Omega[i].Norma2;
            return k;
        }

        /// <summary>
        /// Energía mecánica total de un agregado granular:
        ///   E = K_traslación + K_rotación + U_gravitacional + U_elástica (resortes de contacto).
        /// Con choques inelásticos, lo que se conserva es E + W_disipado.
        /// </summary>
        public static double EnergiaGranular(Sistema s) =>
            EnergiaCinetica(s) + EnergiaRotacion(s) + EnergiaPotencial(s) + s.EnergiaElastica;

        public static Vec3 CentroDeMasa(Sistema s)
        {
            Vec3 c = Vec3.Cero; double m = 0;
            for (int i = 0; i < s.N; i++) { c = c + s.Masa[i] * s.Pos[i]; m += s.Masa[i]; }
            return c / m;
        }

        public static Vec3 VelocidadCentroDeMasa(Sistema s)
        {
            Vec3 c = Vec3.Cero; double m = 0;
            for (int i = 0; i < s.N; i++) { c = c + s.Masa[i] * s.Vel[i]; m += s.Masa[i]; }
            return c / m;
        }

        /// <summary>L = Σ m_i (r_i × v_i)  (solo órbita; el giro propio de las esferas se suma aparte)</summary>
        public static Vec3 MomentoAngular(Sistema s)
        {
            Vec3 l = Vec3.Cero;
            for (int i = 0; i < s.N; i++) l = l + s.Masa[i] * Vec3.Cruz(s.Pos[i], s.Vel[i]);
            return l;
        }
    }
}
