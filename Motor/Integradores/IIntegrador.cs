namespace Cataclismos.Motor.Integradores
{
    public interface IIntegrador
    {
        string Nombre { get; }

        /// <summary>Avanza el sistema un paso de tiempo h.</summary>
        void Paso(Sistema s, double h);
    }
}
