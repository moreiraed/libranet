using libranet.Models;

namespace libranet.BusinessLogic.Strategies
{
    // Interfaz para todas las estrategias de cálculo de multas.
    public interface ICalculoMultaStrategy
    {
        // Recibe el préstamo relacionado y devuelve el monto de la multa.
        decimal CalcularMonto(Prestamo prestamo);
    }
}