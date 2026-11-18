using System;
using System.Diagnostics;
using System.IO;
using Cataclismos.Motor;
using Cataclismos.Motor.Integradores;

namespace Cataclismos.Laboratorio.Pruebas
{
    /// <summary>
    /// PRUEBA 2 — Conservación de energía a largo plazo (experimento "Numérico A").
    /// Misma órbita circular que la Prueba 1, pero 1000 vueltas.
    /// Se registra, para cada vuelta, el error relativo de energía MÁXIMO dentro de esa vuelta.
    ///
    /// Comparación justa de costo: RK4 evalúa las fuerzas 4 veces por paso y Leapfrog 1 vez.
    /// Por eso se agrega "Leapfrog h/4": mismo número de evaluaciones de fuerza que RK4.
    /// </summary>
    internal static class PruebaLargoPlazo
    {
        private sealed class Corrida
        {
            public string Nombre;
            public IIntegrador Integrador;
            public int PasosPorOrbita;
            public int EvaluacionesPorPaso;
        }

        public static void Ejecutar()
        {
            const double M = 1.0, m = 1e-10, r0 = 1.0;
            const int orbitas = 1000;
            double T = 2.0 * Math.PI;
            double vc = Math.Sqrt(M / r0);

            var corridas = new[]
            {
                new Corrida { Nombre = "Euler",       Integrador = new Euler(),       PasosPorOrbita = 200, EvaluacionesPorPaso = 1 },
                new Corrida { Nombre = "RK4",         Integrador = new RungeKutta4(), PasosPorOrbita = 200, EvaluacionesPorPaso = 4 },
                new Corrida { Nombre = "Leapfrog",    Integrador = new Leapfrog(),    PasosPorOrbita = 200, EvaluacionesPorPaso = 1 },
                new Corrida { Nombre = "Leapfrog_h4", Integrador = new Leapfrog(),    PasosPorOrbita = 800, EvaluacionesPorPaso = 1 },
            };

            Console.WriteLine($"PRUEBA 2 — Energía a largo plazo ({orbitas} órbitas)");
            Console.WriteLine();
            Console.WriteLine("Método       | Pasos/órbita | Fuerzas/órbita | Error E (vuelta 10) | Error E (vuelta 100) | Error E (vuelta 1000) | Tiempo");
            Console.WriteLine("-------------+--------------+----------------+---------------------+----------------------+-----------------------+--------");

            string archivo = Path.Combine(Rutas.Resultados, "energia_largo_plazo.csv");
            using (var w = new StreamWriter(archivo))
            {
                w.WriteLine("metodo,orbita,error_energia_max,pasos_por_orbita,fuerzas_por_orbita");

                foreach (var c in corridas)
                {
                    Sistema s = PruebaOrbita.CrearOrbitaCircular(M, m, r0, vc);
                    double e0 = Diagnostico.Energia(s);
                    double h = T / c.PasosPorOrbita;
                    int fuerzasPorOrbita = c.PasosPorOrbita * c.EvaluacionesPorPaso;

                    double e10 = 0, e100 = 0, e1000 = 0;
                    var reloj = Stopwatch.StartNew();

                    for (int o = 1; o <= orbitas; o++)
                    {
                        double maxVuelta = 0;
                        for (int k = 0; k < c.PasosPorOrbita; k++)
                        {
                            c.Integrador.Paso(s, h);
                            double err = Math.Abs((Diagnostico.Energia(s) - e0) / e0);
                            if (err > maxVuelta) maxVuelta = err;
                        }
                        w.WriteLine($"{c.Nombre},{o},{maxVuelta:E6},{c.PasosPorOrbita},{fuerzasPorOrbita}");
                        if (o == 10) e10 = maxVuelta;
                        if (o == 100) e100 = maxVuelta;
                        if (o == 1000) e1000 = maxVuelta;
                    }

                    reloj.Stop();
                    Console.WriteLine($"{c.Nombre,-12} | {c.PasosPorOrbita,12} | {fuerzasPorOrbita,14} | {e10,19:E2} | {e100,20:E2} | {e1000,21:E2} | {reloj.Elapsed.TotalSeconds,5:F1} s");
                }
            }

            Console.WriteLine();
            Console.WriteLine($"Datos guardados en: {archivo}");
            Console.WriteLine("Gráfica:  python graficos/energia_largo_plazo.py");
        }
    }
}
