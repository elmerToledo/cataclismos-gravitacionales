namespace Cataclismos.Motor
{
    /// <summary>
    /// Estado de un sistema de N partículas: 6N variables de primer orden (posición y velocidad 3D),
    /// más 3N de velocidad angular cuando las partículas son esferas con contacto.
    /// Unidades: G = 1.
    /// </summary>
    public sealed class Sistema
    {
        public int N { get; }
        public Vec3[] Pos { get; }
        public Vec3[] Vel { get; }
        public Vec3[] Acc { get; }
        public double[] Masa { get; }

        /// <summary>Radio físico de cada esfera (solo lo usan los contactos granulares).</summary>
        public double[] Radio { get; }

        /// <summary>Velocidad angular de cada esfera (giro sobre sí misma).</summary>
        public Vec3[] Omega { get; }

        /// <summary>Aceleración angular de cada esfera.</summary>
        public Vec3[] AlfaAng { get; }

        /// <summary>Constante gravitacional (1 en unidades del modelo).</summary>
        public double G { get; set; } = 1.0;

        /// <summary>Longitud de suavizado ε. 0 = gravedad newtoniana exacta.</summary>
        public double Suavizado { get; set; } = 0.0;

        public double Tiempo { get; set; }

        /// <summary>Indica si Acc/AlfaAng corresponden al estado actual (lo usan los Leapfrog).</summary>
        public bool AceleracionesValidas { get; set; }

        // --- Contabilidad de energía de los contactos ---

        /// <summary>Energía acumulada que los contactos han convertido en calor (W_disipado).</summary>
        public double EnergiaDisipada { get; set; }

        /// <summary>Energía guardada en los "resortes" de los contactos en este instante: Σ ½ k δ².</summary>
        public double EnergiaElastica { get; set; }

        /// <summary>Número de pares de partículas en contacto en este instante.</summary>
        public int Contactos { get; set; }

        public Sistema(int n)
        {
            N = n;
            Pos = new Vec3[n];
            Vel = new Vec3[n];
            Acc = new Vec3[n];
            Masa = new double[n];
            Radio = new double[n];
            Omega = new Vec3[n];
            AlfaAng = new Vec3[n];
        }

        /// <summary>Momento de inercia de una esfera sólida: I = 2/5 m R².</summary>
        public double Inercia(int i) => 0.4 * Masa[i] * Radio[i] * Radio[i];
    }
}
