using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using libranet.Models;
using libranet.Repositories; // ¡Importante! Añadimos el using para los repositorios
using Microsoft.EntityFrameworkCore; // Para DbUpdateConcurrencyException y SelectListItem (temporalmente)
using Microsoft.AspNetCore.Mvc.Rendering; // Necesario para SelectListItem
using System.Linq;
using System.Threading.Tasks;
using libranet.BusinessLogic.Strategies;
using libranet.BusinessLogic.Factories;

namespace libranet.Controllers
{
    [Authorize]
    public class PrestamoController : Controller
    {
        // --- CAMBIO 1: Inyectamos los repositorios necesarios ---
        private readonly IPrestamoRepository _prestamoRepository;
        private readonly ILibroRepository _libroRepository; // Necesario para Crear (GET y POST) y DevolucionConfirmada
        private readonly ISocioRepository _socioRepository; // Necesario para Crear (GET)
         private readonly IMultaRepository _multaRepository; // Necesario para DevolucionConfirmada (multa automática)

        // El constructor ahora recibe los repositorios
        public PrestamoController(
            IPrestamoRepository prestamoRepository,
            ILibroRepository libroRepository,
            ISocioRepository socioRepository,
            IMultaRepository multaRepository) // Añadimos IMultaRepository
        {
            _prestamoRepository = prestamoRepository;
            _libroRepository = libroRepository;
            _socioRepository = socioRepository;
            _multaRepository = multaRepository; // Guardamos la referencia
        }


        // --- INDEX (Leer Todos) ---
        public async Task<IActionResult> Index()
        {
            // CAMBIO 2: Usamos el repositorio de préstamos
            var prestamos = await _prestamoRepository.GetAllWithDetailsAsync();
            return View(prestamos);
        }

        // --- CREAR (Mostrar Formulario GET) ---
        public async Task<IActionResult> Crear()
        {
            // CAMBIO 3: Usamos los repositorios de Socio y Libro para las listas
            var socios = await _socioRepository.GetAllAsync();
            var librosDisponibles = await _libroRepository.SearchAvailableAsync(""); // Busca todos los disponibles

            // Convertimos las listas para el ViewModel
            var sociosSelectList = socios.Select(s => new SelectListItem
            {
                Value = s.SocioId.ToString(),
                Text = $"{s.NumeroSocio} - {s.Apellido}, {s.Nombre}"
            }).ToList();

            var librosSelectList = librosDisponibles.Select(l => new SelectListItem
            {
                Value = l.LibroId.ToString(),
                Text = $"{l.Titulo} ({l.Autor})"
            }).ToList();

            var viewModel = new PrestamoViewModel
            {
                Prestamo = new Prestamo(),
                Socios = sociosSelectList,
                LibrosDisponibles = librosSelectList
            };

            return View(viewModel);
        }

        // --- CREAR (Guardar POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(int socioId, int libroId)
        {
            if (socioId <= 0 || libroId <= 0)
            {
                ModelState.AddModelError("", "Debe seleccionar un socio y un libro válidos.");
                // Recargar ViewModel si hay error (Mejora pendiente)
                return RedirectToAction("Crear");
            }

            // CAMBIO 4: Usamos el repositorio de libros para buscar y verificar
            var libro = await _libroRepository.GetByIdAsync(libroId);

            if (libro == null || libro.Estado != EstadoLibro.Disponible)
            {
                ModelState.AddModelError("", "El libro seleccionado no está disponible o no existe.");
                // Recargar ViewModel si hay error (Mejora pendiente)
                return RedirectToAction("Crear");
            }

            var nuevoPrestamo = new Prestamo
            {
                SocioId = socioId,
                LibroId = libroId,
                FechaPrestamo = DateTime.Now,
                FechaDevolucionPrevista = DateTime.Now.AddDays(15),
                FechaDevolucionReal = null
            };

            // CAMBIO 5: Usamos el repositorio de libros para actualizar estado
            libro.Estado = EstadoLibro.Prestado;
            await _libroRepository.UpdateAsync(libro);

            // CAMBIO 6: Usamos el repositorio de préstamos para añadir el nuevo
            await _prestamoRepository.AddAsync(nuevoPrestamo);

            // IMPORTANTE: SaveChangesAsync se llama dentro de cada método del repositorio.
            // Ya no necesitamos llamarlo aquí explícitamente.

            return RedirectToAction(nameof(Index));
        }

        // --- DEVOLUCION (Mostrar Confirmación GET) ---
        public async Task<IActionResult> Devolucion(int? id)
        {
            if (id == null) return NotFound();

            // CAMBIO 7: Usamos el repositorio de préstamos con detalles
            var prestamo = await _prestamoRepository.GetByIdWithDetailsAsync(id.Value);

            if (prestamo == null) return NotFound();

            return View(prestamo);
        }

        // --- DEVOLUCION (Confirmar POST) ---
        [HttpPost, ActionName("Devolucion")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DevolucionConfirmada(int id, bool estaDanado)
        {
            var prestamo = await _prestamoRepository.GetByIdAsync(id);

            if (prestamo != null)
            {
                prestamo.FechaDevolucionReal = DateTime.Now;

                // --- USO DE LA FÁBRICA Y ESTRATEGIA POR RETRASO ---
                if (prestamo.FechaDevolucionReal > prestamo.FechaDevolucionPrevista)
                {
                    // 1. Creamos una instancia de la fábrica específica para retraso.
                    IMultaFactory fabricaMultaRetraso = new MultaPorRetrasoFactory();

                    // 2. Usamos la fábrica para crear el objeto Multa.
                    //    Le pasamos el socioId, un motivo base (la fábrica lo completará) y el préstamo.
                    Multa? nuevaMulta = fabricaMultaRetraso.CrearMulta(prestamo.SocioId, "Devolución tardía", prestamo);

                    // 3. Verificamos si la fábrica devolvió una multa (podría ser null si no hubo retraso real)
                    if (nuevaMulta != null)
                    {
                        // 4. Si se creó la multa, la añadimos usando el repositorio.
                        await _multaRepository.AddAsync(nuevaMulta);
                    }
                }
                // --- FIN DEL USO DE LA FÁBRICA ---

                // Actualizamos el préstamo
                await _prestamoRepository.UpdateAsync(prestamo);

                // Actualizamos el libro
                var libro = await _libroRepository.GetByIdAsync(prestamo.LibroId);
                if (libro != null)
                {
                    libro.Estado = EstadoLibro.Disponible;
                    await _libroRepository.UpdateAsync(libro);
                }

                if (estaDanado)
                {
                    // Pasamos indicador para que MultaController sepa que es por daño
                    return RedirectToAction("Crear", "Multa", new { socioId = prestamo.SocioId, motivoDanado = true });
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // --- MÉTODO PARA MOSTRAR LOS DETALLES DE UN PRÉSTAMO (GET) ---
        public async Task<IActionResult> Detalles(int? id)
        {
            // Si no nos pasan un id, no podemos mostrar nada.
            if (id == null)
            {
                return NotFound();
            }

            // Usamos el repositorio para buscar el préstamo por ID, incluyendo los detalles del Socio y del Libro.
            var prestamo = await _prestamoRepository.GetByIdWithDetailsAsync(id.Value);

            // Si no encontramos un préstamo con ese id, devolvemos un error.
            if (prestamo == null)
            {
                return NotFound();
            }

            // Enviamos el objeto 'prestamo' (con toda su información) a la vista.
            return View(prestamo);
        }

    }
}