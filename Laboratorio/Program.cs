using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Cataclismos.Laboratorio.Pruebas;

namespace Cataclismos.Laboratorio
{
    /// <summary>
    /// Uso:
    ///   dotnet run                 → todas las pruebas
    ///   dotnet run -- orbita       → Prueba 1: órbita circular (10 vueltas)
    ///   dotnet run -- largo        → Prueba 2: energía a largo plazo (1000 vueltas)
    ///   dotnet run -- agregado 500 → Prueba 3: luna de partículas aislada (N opcional, 500 por defecto)
    /// </summary>
    internal static class Program
    {
        private static void Main(string[] args)
        {
            // Punto decimal en los CSV aunque Windows esté en español
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            string prueba = args.Length > 0 ? args[0].ToLowerInvariant() : "todas";

            if (prueba == "orbita" || prueba == "todas") { PruebaOrbita.Ejecutar(); Console.WriteLine(); }
            if (prueba == "largo" || prueba == "todas") { PruebaLargoPlazo.Ejecutar(); Console.WriteLine(); }
            if (prueba == "agregado")
            {
                int n = args.Length > 1 ? int.Parse(args[1]) : 500;
                PruebaAgregado.Ejecutar(n);
            }
        }
    }

    internal static class Rutas
    {
        /// <summary>Carpeta resultados/ en la raíz del proyecto (sube desde bin/Debug/net10.0/).</summary>
        public static string Resultados
        {
            get
            {
                string c = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "resultados"));
                Directory.CreateDirectory(c);
                return c;
            }
        }
    }
}
