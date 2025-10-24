using libranet.Models;
using libranet.BusinessLogic.Strategies;
using System; // Para DateTime

namespace libranet.BusinessLogic.Factories
{
    // Implementa la interfaz para crear multas por daño.
    public class MultaPorDanoFactory : IMultaFactory
    {
        public Multa CrearMulta(int socioId, string motivo, Prestamo? prestamo = null)
        {
            // Usamos la estrategia de cálculo por daño para obtener el monto fijo.
            ICalculoMultaStrategy estrategia = new CalculoMultaPorDanoStrategy();
            // Pasamos un Prestamo vacío ya que la estrategia de daño no lo necesita.
            decimal monto = estrategia.CalcularMonto(new Prestamo());

            // Creamos y devolvemos el objeto Multa.
            var multa = new Multa
            {
                SocioId = socioId,
                Motivo = motivo, // Usamos el motivo que nos pasen (ej. "Libro devuelto con daños.")
                Monto = monto,
                FechaCreacion = DateTime.Now,
                Estado = EstadoMulta.Pendiente
            };

            return multa;
        }
    }
}