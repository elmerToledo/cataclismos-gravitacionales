using System;
using System.Collections.Generic;

namespace Cataclismos.Motor.Granular
{
    /// <summary>
    /// Construye una luna tipo "rubble pile": N esferas iguales en una red cúbica
    /// recortada en forma de esfera, con una pequeña perturbación aleatoria (semilla fija)
    /// para que no quede un cristal perfecto. Empiezan en reposo, sin tocarse.
    /// </summary>
    public static class Agregado
    {
        /// <param name="n">número de partículas.</param>
        /// <param name="radioLuna">radio aproximado del agregado.</param>
        /// <param name="masaLuna">masa total.</param>
        /// <param name="semilla">semilla aleatoria (misma semilla → misma luna).</param>
        public static Sistema CrearEsfera(int n, double radioLuna, double masaLuna, int semilla)
        {
            // Espaciado de la red para que n celdas llenen la esfera: n s³ ≈ (4/3) π R³
            double s = radioLuna * Math.Pow(4.0 / 3.0 * Math.PI / n, 1.0 / 3.0);
            double radioParticula = 0.49 * s;          // 2% de separación entre vecinas
            double jitter = 0.005 * s;                 // perturbación < separación → no hay solapes

            var rng = new Random(semilla);
            int m = (int)Math.Ceiling(radioLuna / s) + 2;
            var puntos = new List<(double r2, Vec3 p)>();
            for (int ix = -m; ix <= m; ix++)
                for (int iy = -m; iy <= m; iy++)
                    for (int iz = -m; iz <= m; iz++)
                    {
                        var p = new Vec3(
                            ix * s + jitter * (2 * rng.NextDouble() - 1),
                            iy * s + jitter * (2 * rng.NextDouble() - 1),
                            iz * s + jitter * (2 * rng.NextDouble() - 1));
                        puntos.Add((p.Norma2, p));
                    }

            // Las n más cercanas al centro forman la esfera
            puntos.Sort((a, b) => a.r2.CompareTo(b.r2));

            var sis = new Sistema(n);
            for (int i = 0; i < n; i++)
            {
                sis.Pos[i] = puntos[i].p;
                sis.Masa[i] = masaLuna / n;
                sis.Radio[i] = radioParticula;
            }

            // Centro de masa exactamente en el origen
            Vec3 cm = Diagnostico.CentroDeMasa(sis);
            for (int i = 0; i < n; i++) sis.Pos[i] = sis.Pos[i] - cm;
            return sis;
        }

        /// <summary>Radio de giro respecto al centro de masa: sqrt(Σ m r² / M).</summary>
        public static double RadioDeGiro(Sistema s)
        {
            Vec3 cm = Diagnostico.CentroDeMasa(s);
            double suma = 0, m = 0;
            for (int i = 0; i < s.N; i++) { suma += s.Masa[i] * (s.Pos[i] - cm).Norma2; m += s.Masa[i]; }
            return Math.Sqrt(suma / m);
        }

        /// <summary>Radio "efectivo" de una esfera uniforme con el mismo radio de giro: R = sqrt(5/3) r_g.</summary>
        public static double RadioEfectivo(Sistema s) => Math.Sqrt(5.0 / 3.0) * RadioDeGiro(s);

        /// <summary>Distancia del centro de masa a la superficie de la partícula más lejana.</summary>
        public static double RadioExterno(Sistema s)
        {
            Vec3 cm = Diagnostico.CentroDeMasa(s);
            double max = 0;
            for (int i = 0; i < s.N; i++) max = Math.Max(max, (s.Pos[i] - cm).Norma + s.Radio[i]);
            return max;
        }
    }
}
