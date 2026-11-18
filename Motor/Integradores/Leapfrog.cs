namespace Cataclismos.Motor.Integradores
{
    /// <summary>
    /// Leapfrog / Velocity Verlet (orden 2, simpléctico con paso fijo):
    ///   r_{n+1} = r_n + h v_n + (h²/2) a_n
    ///   v_{n+1} = v_n + (h/2)(a_n + a_{n+1})
    /// Una sola evaluación de fuerzas por paso. Integrador principal del proyecto.
    /// </summary>
    public sealed class Leapfrog : IIntegrador
    {
        private Vec3[] _accNueva = new Vec3[0];

        public string Nombre => "Leapfrog";

        public void Paso(Sistema s, double h)
        {
            if (_accNueva.Length != s.N) _accNueva = new Vec3[s.N];
            if (!s.AceleracionesValidas) Gravedad.CalcularAceleraciones(s);

            for (int i = 0; i < s.N; i++)
                s.Pos[i] = s.Pos[i] + h * s.Vel[i] + (0.5 * h * h) * s.Acc[i];

            Gravedad.CalcularAceleraciones(s, s.Pos, _accNueva);

            for (int i = 0; i < s.N; i++)
            {
                s.Vel[i] = s.Vel[i] + (0.5 * h) * (s.Acc[i] + _accNueva[i]);
                s.Acc[i] = _accNueva[i];
            }

            s.AceleracionesValidas = true;
            s.Tiempo += h;
        }
    }
}
