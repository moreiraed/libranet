using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using libranet.Models;
using libranet.Repositories;
using Microsoft.EntityFrameworkCore;

namespace libranet.Controllers
{
    [Authorize]
    public class LibroController : Controller
    {
        private readonly ILibroRepository _libroRepository;

        public LibroController(ILibroRepository libroRepository)
        {
            _libroRepository = libroRepository;
        }

        // --- INDEX (Leer Todos - CON BÚSQUEDA) ---
        public async Task<IActionResult> Index(string? searchString)
        {
            List<Libro> libros; // Variable para la lista final

            if (!String.IsNullOrEmpty(searchString))
            {
                // Si hay término de búsqueda, llamamos al nuevo método FindAsync del repositorio.
                libros = await _libroRepository.FindAsync(searchString);
            }
            else
            {
                // Si no hay búsqueda, obtenemos todos los libros.
                libros = await _libroRepository.GetAllAsync();
            }

            // Guardamos el término buscado para mostrarlo en la vista.
            ViewData["CurrentFilter"] = searchString;

            // Pasamos la lista (filtrada o completa) a la vista.
            return View(libros);
        }

        // --- CREAR (GET) ---
        public IActionResult Crear()
        {
            var libro = new Libro { Estado = EstadoLibro.Disponible };
            return View(libro);
        }

        // --- CREAR (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Libro libro)
        {
            if (ModelState.IsValid)
            {
                await _libroRepository.AddAsync(libro);
                TempData["SuccessMessage"] = $"Libro '{libro.Titulo}' creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(libro);
        }

        // --- EDITAR (GET) ---
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null) return NotFound();
            var libro = await _libroRepository.GetByIdAsync(id.Value);
            if (libro == null) return NotFound();
            return View(libro);
        }

        // --- EDITAR (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Libro libro)
        {
            if (id != libro.LibroId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _libroRepository.UpdateAsync(libro);
                    TempData["SuccessMessage"] = $"Libro '{libro.Titulo}' actualizado exitosamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    var exists = await _libroRepository.GetByIdAsync(id) != null;
                    if (!exists)
                    {
                        TempData["ErrorMessage"] = "Error: El libro que intentaba editar ya no existe.";
                        return RedirectToAction(nameof(Index)); // Redirigir si no existe
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Error de concurrencia al actualizar el libro.";
                        return View(libro); // Devolver vista con datos actuales y error
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(libro);
        }

        // --- ELIMINAR (GET) ---
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null) return NotFound();
            var libro = await _libroRepository.GetByIdAsync(id.Value);
            if (libro == null) return NotFound();
            return View(libro);
        }

        // --- ELIMINAR (POST) ---
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var libroParaEliminar = await _libroRepository.GetByIdAsync(id); // Obtener antes de borrar

            if (libroParaEliminar != null)
            {
                await _libroRepository.DeleteAsync(id);
                TempData["SuccessMessage"] = $"Libro '{libroParaEliminar.Titulo}' eliminado exitosamente.";
            }
            else
            {
                TempData["ErrorMessage"] = "Error: No se encontró el libro para eliminar.";
            }
            return RedirectToAction(nameof(Index));
        }

        // --- BUSCAR (GET API) ---
        [HttpGet]
        public async Task<IActionResult> Buscar(string term)
        {
            if (string.IsNullOrEmpty(term)) return Json(new List<object>());
            var libros = await _libroRepository.SearchAvailableAsync(term);
            var result = libros.Select(l => new { id = l.LibroId, label = $"{l.Titulo} ({l.Autor})" }).ToList();
            return Json(result);
        }

        // --- DETALLES (GET) ---
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null) return NotFound();
            var libro = await _libroRepository.GetByIdWithDetailsAsync(id.Value);
            if (libro == null) return NotFound();
            return View(libro);
        }
    }
}