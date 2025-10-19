// --- COMENTARIOS SOBRE ESTE CÓDIGO ---
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace libranet.Models
{
    // Este ViewModel contendrá un objeto Multa y la lista de Socios para el dropdown.
    public class MultaViewModel
    {
        public Multa Multa { get; set; } = new();
        public List<SelectListItem> Socios { get; set; } = new();
    }
}