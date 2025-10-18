using Microsoft.AspNetCore.Mvc;
using Libranet.Models;
using Microsoft.AspNetCore.Authorization;
using Libranet.Data; // Para usar LibranetContext
using System.Linq; // Para usar .Count(), .Where(), etc.

namespace Libranet.Controllers
{
    [Authorize] // Esta etiqueta protege todo el controlador
    public class HomeController : Controller
    {
        // Inyectamos el contexto de la base de datos, igual que en AccountController.
        private readonly LibranetContext _context;

        public HomeController(LibranetContext context)
        {
            _context = context;
        }

        // --- LÓGICA DEL PANEL DE CONTROL ---
        public IActionResult Index()
        {
            // Creamos una nueva instancia de nuestro ViewModel.
            var viewModel = new DashboardViewModel
            {
                // Contamos cuántos libros tienen el estado "Prestado".
                LibrosPrestados = _context.Libros.Count(l => l.Estado == EstadoLibro.Prestado),

                // Contamos cuántos préstamos activos (sin fecha de devolución real)
                // tienen una fecha de devolución prevista que ya pasó.
                PrestamosVencidos = _context.Prestamos.Count(p => p.FechaDevolucionReal == null && p.FechaDevolucionPrevista < DateTime.Now),

                // Contamos el total de socios registrados.
                TotalSocios = _context.Socios.Count(),

                // Contamos el total de libros en el catálogo.
                TotalLibros = _context.Libros.Count()
            };

            // Enviamos el ViewModel (el "paquete" con todas nuestras estadísticas) a la vista.
            return View(viewModel);
        }

        // Mantenemos esta acción por si la necesitamos más adelante.
        public IActionResult Privacy()
        {
            return View();
        }
    }
}