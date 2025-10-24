using libranet.Models;
using libranet.BusinessLogic.Strategies;
using System; // Para DateTime

namespace libranet.BusinessLogic.Factories
{
    // Implementa la interfaz para crear multas por retraso.
    public class MultaPorRetrasoFactory : IMultaFactory
    {
        public Multa? CrearMulta(int socioId, string motivo, Prestamo? prestamo = null)
        {
            // Verificación: Necesitamos el préstamo para calcular el retraso.
            if (prestamo == null || !prestamo.FechaDevolucionReal.HasValue || prestamo.FechaDevolucionReal.Value.Date <= prestamo.FechaDevolucionPrevista.Date)
            {
                // Si no hay préstamo o no hay retraso real, no se crea la multa (o podrías lanzar una excepción).
                // Devolvemos null para indicar que no se creó la multa en este caso.
                return null; 
            }

            // Usamos la estrategia de cálculo por retraso.
            ICalculoMultaStrategy estrategia = new CalculoMultaPorRetrasoStrategy();
            decimal monto = estrategia.CalcularMonto(prestamo);

            // Calculamos los días para el motivo.
             var diasDeRetraso = (prestamo.FechaDevolucionReal.Value.Date - prestamo.FechaDevolucionPrevista.Date).Days;
             string motivoCompleto = $"Devolución tardía de {diasDeRetraso} día(s).";


            // Creamos y devolvemos el objeto Multa completo.
            var multa = new Multa
            {
                SocioId = socioId,
                Motivo = motivoCompleto, // Usamos el motivo calculado
                Monto = monto,
                FechaCreacion = DateTime.Now,
                Estado = EstadoMulta.Pendiente
            };

            return multa;
        }
    }
}