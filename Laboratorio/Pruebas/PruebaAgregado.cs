using System;
using System.Diagnostics;
using System.IO;
using Cataclismos.Motor;
using Cataclismos.Motor.Granular;

namespace Cataclismos.Laboratorio.Pruebas
{
    /// <summary>
    /// PRUEBA 3 — Luna de partículas aislada (sin planeta).
    /// Unidades del módulo Roche: radio del planeta = 1, masa del planeta = 1, G = 1.
    /// La luna: radio ≈ 0.1, masa 0.001 → misma densidad que el planeta antes de compactarse.
    ///
    /// Qué se comprueba:
    ///   1. La luna se mantiene unida por su propia gravedad y se "asienta" (la energía cinética cae).
    ///   2. El balance E + W_disipado se mantiene constante (no hay energía que aparezca o se pierda sin control).
    ///   3. Se mide la densidad final de la luna → con ella se calcula su distancia de Roche teórica.
    /// Al final guarda la luna relajada para usarla en los experimentos con el planeta.
    /// </summary>
    internal static class PruebaAgregado
    {
        public static void Ejecutar(int n)
        {
            const double radioLuna = 0.1, masaLuna = 1e-3;
            const int semilla = 12345;
            const double duracion = 15.0;      // en tiempos dinámicos de la luna
            const double cadaSalida = 0.05;

            Sistema s = Agregado.CrearEsfera(n, radioLuna, masaLuna, semilla);
            double radioParticula = s.Radio[0];
            double masaParticula = s.Masa[0];

            double vEscape = Math.Sqrt(2.0 * masaLuna / radioLuna);
            double tDin = Math.Sqrt(radioLuna * radioLuna * radioLuna / masaLuna);

            var p = ParametrosContacto.DesdeFisica(
                restitucion: 0.5, friccion: 0.5,
                masaParticula: masaParticula, radioParticula: radioParticula,
                velocidadReferencia: vEscape, solapeMaximo: 0.01);

            double h = p.PasoRecomendado(25);
            int pasosTotales = (int)Math.Ceiling(duracion * tDin / h);
            int pasosPorSalida = Math.Max(1, (int)Math.Round(cadaSalida * tDin / h));
            var integrador = new LeapfrogGranular(p);

            Console.WriteLine($"PRUEBA 3 — Luna de partículas aislada (N = {n})");
            Console.WriteLine($"  radio partícula = {radioParticula:G4}   masa partícula = {masaParticula:G4}");
            Console.WriteLine($"  v_escape = {vEscape:G4}   tiempo dinámico = {tDin:G4}");
            Console.WriteLine($"  k = {p.K:G4}   ζ = {p.Zeta:F4}   t_contacto = {p.TiempoContacto:G4}   h = {h:G4}");
            Console.WriteLine($"  {pasosTotales} pasos para {duracion} tiempos dinámicos");
            Console.WriteLine();

            // Estado inicial
            FuerzasGranulares.Calcular(s, p, s.Pos, s.Vel, s.Omega, s.Acc, s.AlfaAng);
            s.AceleracionesValidas = true;
            double e0 = Diagnostico.EnergiaGranular(s);

            string archivo = Path.Combine(Rutas.Resultados, $"agregado_relajacion_N{n}.csv");
            var reloj = Stopwatch.StartNew();
            double siguienteReporte = 1.0;

            Console.WriteLine("   t   | E cinética  | E giro      | contactos | R externo | R efectivo | error balance | tiempo");
            Console.WriteLine("-------+-------------+-------------+-----------+-----------+------------+---------------+-------");

            using (var w = new StreamWriter(archivo))
            {
                w.WriteLine("t,e_cinetica,e_giro,e_potencial,e_elastica,w_disipado,error_balance,contactos,radio_externo,radio_efectivo");
                Escribir(w, s, e0, n);

                for (int paso = 1; paso <= pasosTotales; paso++)
                {
                    integrador.Paso(s, h);
                    if (paso % pasosPorSalida != 0) continue;

                    double err = Escribir(w, s, e0, n);
                    double t = s.Tiempo / tDin;
                    if (t >= siguienteReporte - 1e-9)
                    {
                        siguienteReporte += 1.0;
                        Console.WriteLine($"{t,6:F2} | {Diagnostico.EnergiaCinetica(s),11:E3} | {Diagnostico.EnergiaRotacion(s),11:E3} | {s.Contactos,9} | {Agregado.RadioExterno(s),9:F5} | {Agregado.RadioEfectivo(s),10:F5} | {err,13:E2} | {reloj.Elapsed.TotalSeconds,4:F0} s");
                    }
                }
            }

            // --- Resultado: densidad de la luna y su distancia de Roche teórica ---
            double rEf = Agregado.RadioEfectivo(s);
            double razonDensidad = masaLuna / (rEf * rEf * rEf);     // ρ_luna/ρ_planeta (planeta: M = 1, R = 1)
            double volSolido = 0;
            for (int i = 0; i < n; i++) volSolido += 4.0 / 3.0 * Math.PI * Math.Pow(s.Radio[i], 3);
            double porosidad = 1.0 - volSolido / (4.0 / 3.0 * Math.PI * rEf * rEf * rEf);
            double dFluido = 2.44 * Math.Pow(1.0 / razonDensidad, 1.0 / 3.0);
            double dRigido = 1.26 * Math.Pow(1.0 / razonDensidad, 1.0 / 3.0);

            Console.WriteLine();
            Console.WriteLine("RESULTADO");
            Console.WriteLine($"  Radio efectivo de la luna        = {rEf:F5}  (radios del planeta)");
            Console.WriteLine($"  Porosidad (espacio vacío)        = {porosidad * 100:F1} %");
            Console.WriteLine($"  Densidad luna / densidad planeta = {razonDensidad:F3}");
            Console.WriteLine($"  Límite de Roche fluido  (2.44)   = {dFluido:F3} radios del planeta");
            Console.WriteLine($"  Límite de Roche rígido  (1.26)   = {dRigido:F3} radios del planeta");

            // --- Guardar la luna relajada (centro de masa en reposo en el origen) ---
            Vec3 cm = Diagnostico.CentroDeMasa(s), vcm = Diagnostico.VelocidadCentroDeMasa(s);
            string archivoLuna = Path.Combine(Rutas.Resultados, $"luna_relajada_N{n}.csv");
            using (var w = new StreamWriter(archivoLuna))
            {
                w.WriteLine("x,y,z,vx,vy,vz,wx,wy,wz,masa,radio");
                for (int i = 0; i < n; i++)
                {
                    Vec3 r = s.Pos[i] - cm, v = s.Vel[i] - vcm, o = s.Omega[i];
                    w.WriteLine($"{r.X:R},{r.Y:R},{r.Z:R},{v.X:R},{v.Y:R},{v.Z:R},{o.X:R},{o.Y:R},{o.Z:R},{s.Masa[i]:R},{s.Radio[i]:R}");
                }
            }

            Console.WriteLine();
            Console.WriteLine($"Evolución guardada en: {archivo}");
            Console.WriteLine($"Luna relajada en:      {archivoLuna}");
            Console.WriteLine($"Gráficas:  python graficos/agregado_relajacion.py {n}");
        }

        private static double Escribir(StreamWriter w, Sistema s, double e0, int n)
        {
            double ec = Diagnostico.EnergiaCinetica(s);
            double eg = Diagnostico.EnergiaRotacion(s);
            double ep = Diagnostico.EnergiaPotencial(s);
            double ee = s.EnergiaElastica;
            double err = Math.Abs((ec + eg + ep + ee + s.EnergiaDisipada - e0) / e0);

            w.WriteLine($"{s.Tiempo:G8},{ec:E6},{eg:E6},{ep:E8},{ee:E6},{s.EnergiaDisipada:E6},{err:E4},{s.Contactos},{Agregado.RadioExterno(s):G8},{Agregado.RadioEfectivo(s):G8}");
            return err;
        }
    }
}
