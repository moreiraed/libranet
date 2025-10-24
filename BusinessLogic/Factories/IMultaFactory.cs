using libranet.Models;
using libranet.BusinessLogic.Strategies; // Necesitamos acceso a las estrategias

namespace libranet.BusinessLogic.Factories
{
    // Interfaz para las fábricas de Multas.
    public interface IMultaFactory
    {
        // Define el método para crear una Multa.
        // Recibe el socio, el motivo y el préstamo asociado (si aplica).
        Multa? CrearMulta(int socioId, string motivo, Prestamo? prestamo = null);
    }
}