using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using libranet.Models;
using libranet.Repositories; // Añadimos using para los repositorios
using Microsoft.EntityFrameworkCore; // Aún necesario para DbUpdateConcurrencyException
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace libranet.Controllers
{
    [Authorize]
    public class LibroController : Controller
    {
        // --- CAMBIO 1: Inyectamos la interfaz del repositorio, no el DbContext ---
        private readonly ILibroRepository _libroRepository;

        // El constructor ahora recibe ILibroRepository
        public LibroController(ILibroRepository libroRepository)
        {
            _libroRepository = libroRepository;
        }

        // --- INDEX (Leer Todos) ---
        public async Task<IActionResult> Index()
        {
            // CAMBIO 2: Llamamos al método del repositorio
            var libros = await _libroRepository.GetAllAsync();
            return View(libros);
        }

        // --- CREAR (Mostrar Formulario GET) ---
        public IActionResult Crear()
        {
            var libro = new Libro { Estado = EstadoLibro.Disponible };
            return View(libro);
        }

        // --- CREAR (Guardar POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Libro libro)
        {
            if (ModelState.IsValid)
            {
                // CAMBIO 3: Usamos el repositorio para añadir el nuevo libro
                await _libroRepository.AddAsync(libro);
                return RedirectToAction(nameof(Index));
            }
            return View(libro);
        }

        // --- EDITAR (Mostrar Formulario GET) ---
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null) return NotFound();

            // CAMBIO 4: Usamos el repositorio para obtener el libro por ID
            var libro = await _libroRepository.GetByIdAsync(id.Value);
            if (libro == null) return NotFound();

            return View(libro);
        }

        // --- EDITAR (Guardar POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Libro libro)
        {
            if (id != libro.LibroId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // CAMBIO 5: Usamos el repositorio para actualizar el libro
                    await _libroRepository.UpdateAsync(libro);
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Verificar si el libro todavía existe antes de NotFound
                    var exists = await _libroRepository.GetByIdAsync(id) != null;
                    if (!exists)
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw; // Relanzar la excepción si es otro problema de concurrencia
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(libro);
        }

        // --- ELIMINAR (Mostrar Confirmación GET) ---
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null) return NotFound();

            // CAMBIO 6: Usamos el repositorio para obtener el libro por ID
            var libro = await _libroRepository.GetByIdAsync(id.Value);
            if (libro == null) return NotFound();

            return View(libro);
        }

        // --- ELIMINAR (Confirmar POST) ---
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            // CAMBIO 7: Usamos el repositorio para eliminar el libro
            await _libroRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // --- BUSCAR (API Autocompletado GET) ---
        [HttpGet]
        public async Task<IActionResult> Buscar(string term)
        {
            if (string.IsNullOrEmpty(term)) return Json(new List<object>());

            // CAMBIO 8: Usamos el repositorio para buscar libros disponibles
            var libros = await _libroRepository.SearchAvailableAsync(term);

            // La transformación a formato {id, label} se mantiene en el controlador
            var result = libros.Select(l => new {
                id = l.LibroId,
                label = $"{l.Titulo} ({l.Autor})"
            }).ToList();

            return Json(result);
        }

        // --- DETALLES (Mostrar GET) ---
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null) return NotFound();

            // CAMBIO 9: Usamos el método específico del repositorio que incluye detalles
            var libro = await _libroRepository.GetByIdWithDetailsAsync(id.Value);

            if (libro == null) return NotFound();

            return View(libro);
        }
    }
}