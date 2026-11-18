using Cataclismos.Motor.Integradores;

namespace Cataclismos.Motor.Granular
{
    /// <summary>
    /// Leapfrog "patada-deriva-patada" para esferas con contacto y giro:
    ///   v_{1/2}  = v_n + (h/2) a_n                    ω_{1/2} = ω_n + (h/2) α_n
    ///   r_{n+1}  = r_n + h v_{1/2}
    ///   v*_{n+1} = v_{1/2} + (h/2) a_n                (velocidad predicha, para la fuerza de contacto)
    ///   a_{n+1}  = a(r_{n+1}, v*_{n+1}, ω*_{n+1})
    ///   v_{n+1}  = v_{1/2} + (h/2) a_{n+1}            (igual para ω)
    ///
    /// La fuerza de contacto depende de la velocidad. Evaluarla con la velocidad PREDICHA al final
    /// del paso (y no con la de medio paso) hace que el balance de energía sea de segundo orden
    /// en h: al reducir h a la mitad, el error baja ~4 veces (comprobado).
    /// La energía disipada se acumula con la regla del trapecio: W += h/2 (P_n + P_{n+1}).
    /// Con amortiguamiento ya no es simpléctico: se valida con el balance E + W_disipado.
    /// </summary>
    public sealed class LeapfrogGranular : IIntegrador
    {
        private readonly ParametrosContacto _p;
        private Vec3[] _vPred = new Vec3[0];
        private Vec3[] _wPred = new Vec3[0];
        private double _potenciaAnterior;

        public string Nombre => "LeapfrogGranular";

        public LeapfrogGranular(ParametrosContacto p) { _p = p; }

        public void Paso(Sistema s, double h)
        {
            int n = s.N;
            if (_vPred.Length != n) { _vPred = new Vec3[n]; _wPred = new Vec3[n]; }

            if (!s.AceleracionesValidas)
            {
                _potenciaAnterior = FuerzasGranulares.Calcular(s, _p, s.Pos, s.Vel, s.Omega, s.Acc, s.AlfaAng);
                s.AceleracionesValidas = true;
            }

            // Patada (medio paso) + deriva, y velocidad predicha al final del paso
            for (int i = 0; i < n; i++)
            {
                s.Vel[i] = s.Vel[i] + (0.5 * h) * s.Acc[i];
                s.Omega[i] = s.Omega[i] + (0.5 * h) * s.AlfaAng[i];
                s.Pos[i] = s.Pos[i] + h * s.Vel[i];
                _vPred[i] = s.Vel[i] + (0.5 * h) * s.Acc[i];
                _wPred[i] = s.Omega[i] + (0.5 * h) * s.AlfaAng[i];
            }

            double potencia = FuerzasGranulares.Calcular(s, _p, s.Pos, _vPred, _wPred, s.Acc, s.AlfaAng);

            // Segunda patada (medio paso) con las fuerzas nuevas
            for (int i = 0; i < n; i++)
            {
                s.Vel[i] = s.Vel[i] + (0.5 * h) * s.Acc[i];
                s.Omega[i] = s.Omega[i] + (0.5 * h) * s.AlfaAng[i];
            }

            s.EnergiaDisipada += 0.5 * h * (_potenciaAnterior + potencia);
            _potenciaAnterior = potencia;
            s.Tiempo += h;
        }
    }
}
