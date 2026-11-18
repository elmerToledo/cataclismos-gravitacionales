namespace Cataclismos.Motor.Integradores
{
    /// <summary>
    /// Runge-Kutta clásico de orden 4 sobre y = (r, v), f(y) = (v, a(r)).
    /// Muy preciso por paso, pero no simpléctico: la energía deriva lentamente.
    /// Cuatro evaluaciones de fuerzas por paso.
    /// </summary>
    public sealed class RungeKutta4 : IIntegrador
    {
        private Vec3[] _r0, _v0, _rT, _vT, _kr, _kv, _a;

        public string Nombre => "RK4";

        private void Preparar(int n)
        {
            if (_r0 != null && _r0.Length == n) return;
            _r0 = new Vec3[n]; _v0 = new Vec3[n];
            _rT = new Vec3[n]; _vT = new Vec3[n];
            _kr = new Vec3[n]; _kv = new Vec3[n];
            _a = new Vec3[n];
        }

        public void Paso(Sistema s, double h)
        {
            int n = s.N;
            Preparar(n);

            for (int i = 0; i < n; i++) { _r0[i] = s.Pos[i]; _v0[i] = s.Vel[i]; _kr[i] = Vec3.Cero; _kv[i] = Vec3.Cero; }

            // Etapa 1: k1 en (r0, v0)
            Gravedad.CalcularAceleraciones(s, _r0, _a);
            for (int i = 0; i < n; i++)
            {
                Vec3 k1r = _v0[i], k1v = _a[i];
                _kr[i] = _kr[i] + k1r; _kv[i] = _kv[i] + k1v;
                _rT[i] = _r0[i] + (0.5 * h) * k1r;
                _vT[i] = _v0[i] + (0.5 * h) * k1v;
            }

            // Etapa 2: k2 en y0 + h/2 k1
            Gravedad.CalcularAceleraciones(s, _rT, _a);
            for (int i = 0; i < n; i++)
            {
                Vec3 k2r = _vT[i], k2v = _a[i];
                _kr[i] = _kr[i] + 2.0 * k2r; _kv[i] = _kv[i] + 2.0 * k2v;
                _rT[i] = _r0[i] + (0.5 * h) * k2r;
                _vT[i] = _v0[i] + (0.5 * h) * k2v;
            }

            // Etapa 3: k3 en y0 + h/2 k2
            Gravedad.CalcularAceleraciones(s, _rT, _a);
            for (int i = 0; i < n; i++)
            {
                Vec3 k3r = _vT[i], k3v = _a[i];
                _kr[i] = _kr[i] + 2.0 * k3r; _kv[i] = _kv[i] + 2.0 * k3v;
                _rT[i] = _r0[i] + h * k3r;
                _vT[i] = _v0[i] + h * k3v;
            }

            // Etapa 4: k4 en y0 + h k3
            Gravedad.CalcularAceleraciones(s, _rT, _a);
            for (int i = 0; i < n; i++)
            {
                Vec3 k4r = _vT[i], k4v = _a[i];
                s.Pos[i] = _r0[i] + (h / 6.0) * (_kr[i] + k4r);
                s.Vel[i] = _v0[i] + (h / 6.0) * (_kv[i] + k4v);
            }

            s.AceleracionesValidas = false;
            s.Tiempo += h;
        }
    }
}
