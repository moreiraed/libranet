using System.ComponentModel.DataAnnotations;

namespace libranet.Models;

public class Socio
{
    public int SocioId { get; set; }
    public string NumeroSocio { get; set; } = string.Empty;
    [Required(ErrorMessage = "El Nombre es obligatorio.")]
    public string Nombre { get; set; } = string.Empty;
    [Required(ErrorMessage = "El Apellido es obligatorio.")]
    public string Apellido { get; set; } = string.Empty;
    [Required(ErrorMessage = "El DNI es obligatorio.")]
    [RegularExpression(@"^\d{7,8}$", ErrorMessage = "El DNI debe contener 7 u 8 dígitos numéricos.")]
    public string DNI { get; set; } = string.Empty;
    [Required(ErrorMessage = "El Email es obligatorio.")]
    [EmailAddress(ErrorMessage = "Por favor, ingrese una dirección de correo válida.")]
    public string? Email { get; set; }
    [Required(ErrorMessage = "El Teléfono es obligatorio.")]
    [RegularExpression(@"^[\d\s\+\-\(\)]{7,15}$", ErrorMessage = "Ingrese un número de teléfono válido.")]
    public string? Telefono { get; set; }
    [Required(ErrorMessage = "La Dirección es obligatoria.")]
    public string? Direccion { get; set; }
    public DateTime FechaDeAlta { get; set; }
    // Propiedad de navegación para acceder a la lista de préstamos de este socio.
    public List<Prestamo> Prestamos { get; set; } = new();
}