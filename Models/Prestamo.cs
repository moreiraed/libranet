namespace Libranet.Models;

public class Prestamo
{
    public int PrestamoId { get; set; }
    public int SocioId { get; set; }
    public int LibroId { get; set; }
    public DateTime FechaPrestamo { get; set; }
    public DateTime FechaDevolucionPrevista { get; set; }
    public DateTime? FechaDevolucionReal { get; set; }
}