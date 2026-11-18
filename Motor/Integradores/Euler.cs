namespace Cataclismos.Motor.Integradores
{
    /// <summary>
    /// Euler explícito (orden 1):
    ///   r_{n+1} = r_n + h v_n
    ///   v_{n+1} = v_n + h a_n
    /// Solo para comparar: su error de energía crece sin límite.
    /// </summary>
    public sealed class Euler : IIntegrador
    {
        public string Nombre => "Euler";

        public void Paso(Sistema s, double h)
        {
            Gravedad.CalcularAceleraciones(s);
            for (int i = 0; i < s.N; i++)
            {
                Vec3 vAnterior = s.Vel[i];
                s.Vel[i] = s.Vel[i] + h * s.Acc[i];
                s.Pos[i] = s.Pos[i] + h * vAnterior;
            }
            s.AceleracionesValidas = false;
            s.Tiempo += h;
        }
    }
}
