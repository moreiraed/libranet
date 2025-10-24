using libranet.Models;
using System;

namespace libranet.BusinessLogic.Strategies
{
    // Implementa la interfaz para calcular multas por retraso.
    public class CalculoMultaPorRetrasoStrategy : ICalculoMultaStrategy
    {
        // Define la tarifa por día de retraso
        private const decimal TarifaPorDia = 100m;

        public decimal CalcularMonto(Prestamo prestamo)
        {
            // Verifica si el préstamo realmente fue devuelto y si fue devuelto tarde.
            if (prestamo.FechaDevolucionReal.HasValue && prestamo.FechaDevolucionReal.Value.Date > prestamo.FechaDevolucionPrevista.Date)
            {
                // Calcula la diferencia de días entre la devolución real y la prevista.
                var diasDeRetraso = (prestamo.FechaDevolucionReal.Value.Date - prestamo.FechaDevolucionPrevista.Date).Days;

                if (diasDeRetraso > 0)
                {
                    return diasDeRetraso * TarifaPorDia;
                }
            }
            
            return 0m;
        }
    }
}