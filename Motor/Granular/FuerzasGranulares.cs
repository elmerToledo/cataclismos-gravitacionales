using System;
using System.Threading.Tasks;

namespace Cataclismos.Motor.Granular
{
    /// <summary>
    /// Fuerzas sobre cada partícula del agregado:
    ///   a_i = Σ_j gravedad(i,j) + Σ_{j en contacto} (F_n + F_t)/m_i
    ///   α_i = Σ_{j en contacto} τ_ij / I_i,     τ_ij = (−R_i n) × F_t
    /// donde n es el vector unitario de j hacia i.
    ///
    /// Se recorre, para cada i, todo j ≠ i (cada par se calcula dos veces) para poder
    /// repartir las partículas entre los núcleos del procesador sin conflictos.
    /// </summary>
    public static class FuerzasGranulares
    {
        private static double[] _potencia = new double[0];
        private static double[] _elastica = new double[0];
        private static int[] _contactos = new int[0];

        /// <summary>
        /// Calcula aceleraciones lineales y angulares con las posiciones/velocidades dadas.
        /// Devuelve la potencia disipada instantánea (energía convertida en calor por unidad de tiempo).
        /// También actualiza s.EnergiaElastica y s.Contactos.
        /// </summary>
        public static double Calcular(Sistema s, ParametrosContacto p,
            Vec3[] pos, Vec3[] vel, Vec3[] omega, Vec3[] acc, Vec3[] alfa)
        {
            int n = s.N;
            if (_potencia.Length != n)
            {
                _potencia = new double[n];
                _elastica = new double[n];
                _contactos = new int[n];
            }

            double g = s.G;
            double eps2 = s.Suavizado * s.Suavizado;
            double k = p.K, zeta = p.Zeta, gammaT = p.GammaT, mu = p.Friccion;
            double[] masa = s.Masa, radio = s.Radio;

            Parallel.For(0, n, i =>
            {
                Vec3 ri = pos[i], vi = vel[i], wi = omega[i];
                double mi = masa[i], Ri = radio[i];
                Vec3 a = Vec3.Cero, torque = Vec3.Cero;
                double potencia = 0, elastica = 0;
                int contactos = 0;

                for (int j = 0; j < n; j++)
                {
                    if (j == i) continue;
                    Vec3 d = pos[j] - ri;               // de i hacia j
                    double r2 = d.Norma2;

                    // --- Gravedad ---
                    double r2s = r2 + eps2;
                    a = a + d * (g * masa[j] / (r2s * Math.Sqrt(r2s)));

                    // --- Contacto ---
                    double suma = Ri + radio[j];
                    if (r2 >= suma * suma || r2 == 0) continue;

                    double r = Math.Sqrt(r2);
                    double delta = suma - r;            // solape
                    Vec3 nij = d * (-1.0 / r);          // normal unitaria de j hacia i
                    double mj = masa[j], Rj = radio[j];

                    // Velocidad relativa en el punto de contacto (incluye el giro de cada esfera)
                    Vec3 vc = (vi + Vec3.Cruz(wi, -Ri * nij)) - (vel[j] + Vec3.Cruz(omega[j], Rj * nij));
                    double vn = Vec3.Punto(vc, nij);    // > 0 se separan, < 0 se acercan

                    // Normal: resorte + amortiguador, nunca atractiva
                    double mEf = mi * mj / (mi + mj);
                    double gammaN = 2.0 * zeta * Math.Sqrt(mEf * k);
                    double fn = k * delta - gammaN * vn;
                    if (fn < 0) fn = 0;

                    // Tangencial: fricción viscosa limitada por Coulomb
                    Vec3 vt = vc - vn * nij;
                    double vtm = vt.Norma;
                    Vec3 ft = Vec3.Cero;
                    double ftm = 0;
                    if (vtm > 1e-300)
                    {
                        ftm = Math.Min(gammaT * vtm, mu * fn);
                        ft = vt * (-ftm / vtm);
                    }

                    a = a + (fn * nij + ft) / mi;
                    torque = torque + Vec3.Cruz(-Ri * nij, ft);

                    // Contabilidad (cada par se visita dos veces → mitad para cada uno)
                    //   disipación normal = v_n (k δ − f_n),   tangencial = |F_t| |v_t|
                    potencia += 0.5 * (vn * (k * delta - fn) + ftm * vtm);
                    elastica += 0.5 * (0.5 * k * delta * delta);
                    contactos++;
                }

                acc[i] = a;
                alfa[i] = torque / (0.4 * mi * Ri * Ri);
                _potencia[i] = potencia;
                _elastica[i] = elastica;
                _contactos[i] = contactos;
            });

            double potenciaTotal = 0, elasticaTotal = 0;
            int contactosTotal = 0;
            for (int i = 0; i < n; i++)
            {
                potenciaTotal += _potencia[i];
                elasticaTotal += _elastica[i];
                contactosTotal += _contactos[i];
            }
            s.EnergiaElastica = elasticaTotal;
            s.Contactos = contactosTotal / 2;
            return potenciaTotal;
        }
    }
}
