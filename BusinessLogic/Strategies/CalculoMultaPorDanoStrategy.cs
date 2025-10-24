using libranet.Models;

namespace libranet.BusinessLogic.Strategies
{
    // Implementa la interfaz para calcular multas por daño.
    public class CalculoMultaPorDanoStrategy : ICalculoMultaStrategy
    {
        // Define el monto fijo para multas por daño.
        private const decimal MontoFijoPorDano = 500m;

        public decimal CalcularMonto(Prestamo prestamo)
        {
            // Para la multa por daño, simplemente devolvemos el monto fijo.
            // El parámetro 'prestamo' no se usa en este cálculo, pero la interfaz lo requiere.
            return MontoFijoPorDano;
        }
    }
}