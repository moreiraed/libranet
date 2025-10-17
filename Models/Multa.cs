namespace Libranet.Models;

public class Multa
{
    public int MultaId { get; set; }
    public int SocioId { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime FechaCreacion { get; set; }
    public EstadoMulta Estado { get; set; }
}