using System;
using System.IO;
using Cataclismos.Motor;
using Cataclismos.Motor.Integradores;

namespace Cataclismos.Laboratorio.Pruebas
{
    /// <summary>
    /// PRUEBA 1 — Validación con dos cuerpos.
    /// Un satélite de masa despreciable en órbita circular alrededor de M = 1, a r = 1 (G = 1).
    /// Teoría:  v_c = sqrt(GM/r) = 1     T = 2π sqrt(r³/GM) = 2π ≈ 6.283185
    /// Se compara Euler, RK4 y Leapfrog con el mismo paso de tiempo.
    /// </summary>
    internal static class PruebaOrbita
    {
        public static void Ejecutar()
        {
            const double M = 1.0, m = 1e-10, r0 = 1.0;
            const int pasosPorOrbita = 200;
            const int orbitas = 10;

            double vc = Math.Sqrt(M / r0);
            double T = 2.0 * Math.PI * Math.Sqrt(r0 * r0 * r0 / M);
            double h = T / pasosPorOrbita;
            int pasos = pasosPorOrbita * orbitas;

            string carpeta = Rutas.Resultados;

            Console.WriteLine("PRUEBA 1 — Órbita circular de dos cuerpos (G = M = r = 1)");
            Console.WriteLine($"Período teórico T = {T:F6}   paso h = T/{pasosPorOrbita}   {orbitas} órbitas");
            Console.WriteLine();
            Console.WriteLine("Método     | Período medido | Error período | Error radio final | Error energía final | Error energía máx");
            Console.WriteLine("-----------+----------------+---------------+-------------------+---------------------+------------------");

            IIntegrador[] metodos = { new Euler(), new RungeKutta4(), new Leapfrog() };

            foreach (var metodo in metodos)
            {
                Sistema s = CrearOrbitaCircular(M, m, r0, vc);
                double e0 = Diagnostico.Energia(s);
                double l0 = Diagnostico.MomentoAngular(s).Norma;

                double errEMax = 0, errE = 0, errR = 0;
                double periodo = double.NaN;
                double yAnterior = (s.Pos[1] - s.Pos[0]).Y;

                string archivo = Path.Combine(carpeta, $"orbita_{metodo.Nombre}.csv");
                using (var w = new StreamWriter(archivo))
                {
                    w.WriteLine("t,x,y,z,r,error_energia,error_momento_angular");
                    for (int k = 1; k <= pasos; k++)
                    {
                        metodo.Paso(s, h);

                        Vec3 rel = s.Pos[1] - s.Pos[0];
                        double r = rel.Norma;
                        errE = Math.Abs((Diagnostico.Energia(s) - e0) / e0);
                        double errL = Math.Abs((Diagnostico.MomentoAngular(s).Norma - l0) / l0);
                        errR = Math.Abs(r - r0) / r0;
                        if (errE > errEMax) errEMax = errE;

                        // Período: primer cruce de y = 0 subiendo, interpolado linealmente
                        if (double.IsNaN(periodo) && yAnterior < 0 && rel.Y >= 0)
                            periodo = (s.Tiempo - h) + h * (0 - yAnterior) / (rel.Y - yAnterior);
                        yAnterior = rel.Y;

                        w.WriteLine($"{s.Tiempo:G10},{rel.X:G10},{rel.Y:G10},{rel.Z:G10},{r:G10},{errE:E4},{errL:E4}");
                    }
                }

                double errT = Math.Abs(periodo - T) / T;
                Console.WriteLine($"{metodo.Nombre,-10} | {periodo,14:F6} | {errT,13:E2} | {errR,17:E2} | {errE,19:E2} | {errEMax,16:E2}");
            }

            Console.WriteLine();
            Console.WriteLine($"Trayectorias guardadas en: {carpeta}");
        }

        /// <summary>Planeta en el origen + satélite en (r0,0,0) con velocidad (0,v,0), en el marco del centro de masa.</summary>
        internal static Sistema CrearOrbitaCircular(double M, double m, double r0, double v)
        {
            var s = new Sistema(2);
            s.Masa[0] = M;
            s.Masa[1] = m;
            s.Pos[0] = Vec3.Cero;
            s.Pos[1] = new Vec3(r0, 0, 0);
            s.Vel[1] = new Vec3(0, v, 0);
            s.Vel[0] = new Vec3(0, -v * m / M, 0); // momento lineal total = 0
            return s;
        }
    }
}
