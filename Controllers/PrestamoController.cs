using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using libranet.Data;
using libranet.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering; // Necesario para SelectListItem
using System.Linq; // Necesario para .Select()
using System.Threading.Tasks; // Necesario para Task<>

namespace libranet.Controllers
{
    [Authorize]
    public class PrestamoController : Controller
    {
        private readonly LibranetContext _context;

        public PrestamoController(LibranetContext context)
        {
            _context = context;
        }

        // --- MÉTODO PARA LISTAR LOS PRÉSTAMOS (GET) ---
        public async Task<IActionResult> Index()
        {
            // 1. Usamos el _context para acceder a la tabla 'Prestamos'.
            // 2. '.Include(p => p.Socio)' le dice a EF que también cargue los datos del Socio relacionado.
            // 3. '.Include(p => p.Libro)' le dice a EF que también cargue los datos del Libro relacionado.
            // 4. '.ToListAsync()' obtiene todos los registros.
            var prestamos = await _context.Prestamos
                                        .Include(p => p.Socio)
                                        .Include(p => p.Libro)
                                        .ToListAsync();

            // 5. Enviamos la lista completa de préstamos a la vista.
            return View(prestamos);
        }

        // --- MÉTODO PARA MOSTRAR EL FORMULARIO DE PRÉSTAMO (GET) ---
        public async Task<IActionResult> Crear()
        {
            // 1. Obtenemos la lista de todos los socios.
            var socios = await _context.Socios
                                       .Select(s => new SelectListItem
                                       {
                                           Value = s.SocioId.ToString(),
                                           Text = s.Apellido + ", " + s.Nombre + " (DNI: " + s.DNI + ")"
                                       }).ToListAsync();

            // 2. Obtenemos la lista de libros que están en estado "Disponible".
            var librosDisponibles = await _context.Libros
                                                  .Where(l => l.Estado == EstadoLibro.Disponible)
                                                  .Select(l => new SelectListItem
                                                  {
                                                      Value = l.LibroId.ToString(),
                                                      Text = l.Titulo + " (" + l.Autor + ")"
                                                  }).ToListAsync();

            // 3. Creamos el ViewModel y le pasamos las listas que acabamos de crear.
            var viewModel = new PrestamoViewModel
            {
                Prestamo = new Prestamo(), // Un objeto Prestamo vacío para el formulario.
                Socios = socios,
                LibrosDisponibles = librosDisponibles
            };

            // 4. Enviamos el ViewModel a la vista.
            return View(viewModel);
        }

        // --- MÉTODO PARA GUARDAR EL NUEVO PRÉSTAMO (POST) ---
        // Hemos cambiado el parámetro para que solo reciba los IDs que envía el formulario.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(int socioId, int libroId)
        {
            // Verificación básica: nos aseguramos de que el usuario haya seleccionado una opción válida.
            if (socioId <= 0 || libroId <= 0)
            {
                // Si no, mostramos un error.
                ModelState.AddModelError("", "Debe seleccionar un socio y un libro válidos.");
                // (Aquí tendríamos que volver a cargar el ViewModel, lo haremos después)
                return RedirectToAction("Crear"); // Redirigimos al formulario de creación por ahora.
            }

            // --- LÓGICA PARA CREAR EL PRÉSTAMO ---

            // 1. Buscamos el libro seleccionado en la base de datos.
            var libro = await _context.Libros.FindAsync(libroId);

            // 2. Verificación de seguridad: nos aseguramos de que el libro exista y esté disponible.
            if (libro == null || libro.Estado != EstadoLibro.Disponible)
            {
                ModelState.AddModelError("", "El libro seleccionado no está disponible o no existe.");
                return RedirectToAction("Crear");
            }

            // 3. Creamos un NUEVO objeto Prestamo.
            var nuevoPrestamo = new Prestamo
            {
                SocioId = socioId,
                LibroId = libroId,
                FechaPrestamo = DateTime.Now,
                // Por defecto, damos 15 días para la devolución.
                FechaDevolucionPrevista = DateTime.Now.AddDays(15),
                FechaDevolucionReal = null // Aún no ha sido devuelto.
            };

            // 4. Actualizamos el estado del libro a "Prestado".
            libro.Estado = EstadoLibro.Prestado;
            _context.Update(libro);

            // 5. Añadimos el nuevo objeto 'Prestamo' al contexto.
            _context.Add(nuevoPrestamo);

            // 6. Guardamos TODOS los cambios en la base de datos.
            await _context.SaveChangesAsync();

            // 7. Redirigimos al usuario a la futura lista de préstamos.
            return RedirectToAction("Index");
        }

        // --- MÉTODO PARA MOSTRAR LA PÁGINA DE CONFIRMACIÓN DE DEVOLUCIÓN (GET) ---
        public async Task<IActionResult> Devolucion(int? id)
        {
            // Si no nos pasan un id de préstamo, no podemos hacer nada.
            if (id == null)
            {
                return NotFound();
            }

            // Buscamos el préstamo en la base de datos e incluimos los datos del libro y del socio
            // para poder mostrarlos en la página de confirmación.
            var prestamo = await _context.Prestamos
                .Include(p => p.Socio)
                .Include(p => p.Libro)
                .FirstOrDefaultAsync(p => p.PrestamoId == id);

            // Si no encontramos un préstamo con ese id, devolvemos un error.
            if (prestamo == null)
            {
                return NotFound();
            }

            // Enviamos el préstamo a la vista de confirmación.
            return View(prestamo);
        }

        // --- MÉTODO PARA EJECUTAR LA DEVOLUCIÓN (POST) ---
        [HttpPost, ActionName("Devolucion")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DevolucionConfirmada(int id)
        {
            // Buscamos el préstamo que vamos a actualizar.
            var prestamo = await _context.Prestamos.FindAsync(id);

            if (prestamo != null)
            {
                // 1. Marcamos la fecha de devolución real con el momento actual.
                prestamo.FechaDevolucionReal = DateTime.Now;
                _context.Update(prestamo);

                // 2. Buscamos el libro asociado a este préstamo.
                var libro = await _context.Libros.FindAsync(prestamo.LibroId);
                if (libro != null)
                {
                    // 3. Cambiamos su estado de vuelta a "Disponible".
                    libro.Estado = EstadoLibro.Disponible;
                    _context.Update(libro);
                }

                // 4. Guardamos TODOS los cambios en la base de datos (la actualización del préstamo Y del libro).
                await _context.SaveChangesAsync();
            }

            // Redirigimos al usuario de vuelta a la lista de préstamos.
            return RedirectToAction(nameof(Index));
        }   


    }
}