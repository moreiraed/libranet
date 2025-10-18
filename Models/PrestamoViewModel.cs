// --- COMENTARIOS SOBRE ESTE CÓDIGO ---
// Usamos 'using' para poder usar las clases SelectListItem y List.
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace libranet.Models
{
    // Este ViewModel contendrá un objeto Prestamo y las listas para los dropdowns.
    public class PrestamoViewModel
    {
        // Este es el objeto Prestamo que se llenará con los datos del formulario.
        public Prestamo Prestamo { get; set; } = new();

        // Esta es la lista de todos los socios para el dropdown.
        public List<SelectListItem> Socios { get; set; } = new();

        // Esta es la lista de todos los libros DISPONIBLES para el dropdown.
        public List<SelectListItem> LibrosDisponibles { get; set; } = new();
    }
}