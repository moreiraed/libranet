namespace libranet.Models;

public class Prestamo
{
    public int PrestamoId { get; set; }
    public int SocioId { get; set; }
    public int LibroId { get; set; }
    public DateTime FechaPrestamo { get; set; }
    public DateTime FechaDevolucionPrevista { get; set; }
    public DateTime? FechaDevolucionReal { get; set; }
    // Estas son las propiedades de navegación.
    // Le dicen a EF que un Préstamo está relacionado con un Socio y un Libro.
    public Socio? Socio { get; set; }
    public Libro? Libro { get; set; }
}