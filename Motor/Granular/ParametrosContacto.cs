using System;

namespace Cataclismos.Motor.Granular
{
    /// <summary>
    /// Modelo de esferas blandas (DEM):
    ///   Normal:      F_n = max(0, k δ − γ_n v_n) n
    ///   Tangencial:  |F_t| = min(γ_t |v_t|, μ |F_n|), opuesta al deslizamiento (Coulomb)
    /// Los coeficientes NO se eligen a ojo: se calculan desde la restitución e y el solape máximo.
    /// </summary>
    public sealed class ParametrosContacto
    {
        /// <summary>Coeficiente de restitución normal e (0 = se pegan, 1 = rebote perfecto).</summary>
        public double Restitucion { get; private set; }

        /// <summary>Coeficiente de fricción de Coulomb μ.</summary>
        public double Friccion { get; private set; }

        /// <summary>Rigidez del resorte normal k.</summary>
        public double K { get; private set; }

        /// <summary>Razón de amortiguamiento ζ (se obtiene de e). γ_n = 2 ζ sqrt(m_ef k) por par.</summary>
        public double Zeta { get; private set; }

        /// <summary>Coeficiente viscoso tangencial γ_t.</summary>
        public double GammaT { get; private set; }

        /// <summary>Duración de un choque entre dos partículas típicas.</summary>
        public double TiempoContacto { get; private set; }

        /// <summary>
        /// Construye los parámetros a partir de cantidades con significado físico.
        /// </summary>
        /// <param name="restitucion">e, típico 0.5 para roca.</param>
        /// <param name="friccion">μ, típico 0.5 para roca.</param>
        /// <param name="masaParticula">masa de una partícula típica.</param>
        /// <param name="radioParticula">radio de una partícula típica.</param>
        /// <param name="velocidadReferencia">velocidad de choque esperada (ej. velocidad de escape de la luna).</param>
        /// <param name="solapeMaximo">solape máximo permitido como fracción del radio (ej. 0.01 = 1%).</param>
        public static ParametrosContacto DesdeFisica(double restitucion, double friccion,
            double masaParticula, double radioParticula, double velocidadReferencia, double solapeMaximo)
        {
            double mEf = masaParticula / 2.0;                       // masa efectiva de dos partículas iguales
            double deltaMax = solapeMaximo * radioParticula;

            // Energía cinética del choque = energía del resorte al máximo solape:
            //   ½ m_ef v² = ½ k δ_max²   →   k = m_ef v² / δ_max²
            double k = mEf * velocidadReferencia * velocidadReferencia / (deltaMax * deltaMax);

            // Oscilador amortiguado: e = exp(−ζ π / sqrt(1 − ζ²))  →  ζ = −ln e / sqrt(π² + ln² e)
            double lnE = Math.Log(restitucion);
            double zeta = -lnE / Math.Sqrt(Math.PI * Math.PI + lnE * lnE);

            double gammaN = 2.0 * zeta * Math.Sqrt(mEf * k);
            double omega0 = Math.Sqrt(k / mEf);
            double tc = Math.PI / (omega0 * Math.Sqrt(1.0 - zeta * zeta));

            return new ParametrosContacto
            {
                Restitucion = restitucion,
                Friccion = friccion,
                K = k,
                Zeta = zeta,
                GammaT = gammaN,     // misma escala que el amortiguamiento normal
                TiempoContacto = tc,
            };
        }

        /// <summary>Paso de tiempo recomendado: ~25 pasos dentro de cada choque.</summary>
        public double PasoRecomendado(int pasosPorChoque = 25) => TiempoContacto / pasosPorChoque;
    }
}
