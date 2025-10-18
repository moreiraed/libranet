namespace libranet.Models;

public class Libro
{
    public int LibroId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Autor { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public EstadoLibro Estado { get; set; }
}