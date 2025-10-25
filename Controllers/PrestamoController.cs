using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using libranet.Models;
using libranet.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;
using libranet.BusinessLogic.Strategies;
using libranet.BusinessLogic.Factories;

namespace libranet.Controllers
{
    [Authorize]
    public class PrestamoController : Controller
    {
        private readonly IPrestamoRepository _prestamoRepository;
        private readonly ILibroRepository _libroRepository;
        private readonly ISocioRepository _socioRepository;
        private readonly IMultaRepository _multaRepository;
        private readonly IEnumerable<ICalculoMultaStrategy> _calculoMultaStrategies;

        public PrestamoController(
            IPrestamoRepository prestamoRepository,
            ILibroRepository libroRepository,
            ISocioRepository socioRepository,
            IMultaRepository multaRepository,
            IEnumerable<ICalculoMultaStrategy> calculoMultaStrategies)
        {
            _prestamoRepository = prestamoRepository;
            _libroRepository = libroRepository;
            _socioRepository = socioRepository;
            _multaRepository = multaRepository;
            _calculoMultaStrategies = calculoMultaStrategies;
        }

        // --- INDEX ---
        public async Task<IActionResult> Index()
        {
            var prestamos = await _prestamoRepository.GetAllWithDetailsAsync();
            return View(prestamos);
        }

        // --- CREAR (GET) ---
        public async Task<IActionResult> Crear()
        {
            var socios = await _socioRepository.GetAllAsync();
            var librosDisponibles = (await _libroRepository.GetAllAsync()).Where(l => l.Estado == EstadoLibro.Disponible).ToList();

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

        // --- CREAR (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(int socioId, int libroId)
        {
            if (socioId <= 0 || libroId <= 0)
            {
                TempData["ErrorMessage"] = "Debe seleccionar un socio y un libro válidos.";
                return RedirectToAction("Crear"); // Podrías recargar el ViewModel aquí si prefieres
            }

            var libro = await _libroRepository.GetByIdAsync(libroId);

            if (libro == null || libro.Estado != EstadoLibro.Disponible)
            {
                TempData["ErrorMessage"] = "El libro seleccionado no está disponible o no existe.";
                return RedirectToAction("Crear"); // Podrías recargar el ViewModel aquí si prefieres
            }

            var nuevoPrestamo = new Prestamo
            {
                SocioId = socioId,
                LibroId = libroId,
                FechaPrestamo = DateTime.Now,
                FechaDevolucionPrevista = DateTime.Now.AddDays(15),
                FechaDevolucionReal = null
            };

            libro.Estado = EstadoLibro.Prestado;
            await _libroRepository.UpdateAsync(libro); // Guarda el cambio del libro
            await _prestamoRepository.AddAsync(nuevoPrestamo); // Guarda el nuevo préstamo

            TempData["SuccessMessage"] = $"Préstamo del libro '{libro.Titulo}' registrado exitosamente.";

            return RedirectToAction(nameof(Index));
        }

        // --- DEVOLUCION (GET) ---
        public async Task<IActionResult> Devolucion(int? id)
        {
            if (id == null) return NotFound();
            var prestamo = await _prestamoRepository.GetByIdWithDetailsAsync(id.Value);
            if (prestamo == null) return NotFound();
            return View(prestamo);
        }

        // --- DEVOLUCION (POST) ---
        [HttpPost, ActionName("Devolucion")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DevolucionConfirmada(int id, bool estaDanado)
        {
            var prestamo = await _prestamoRepository.GetByIdAsync(id);
            string? tituloLibro = "desconocido";

            if (prestamo != null)
            {
                var libroTemp = await _libroRepository.GetByIdAsync(prestamo.LibroId);
                if (libroTemp != null) tituloLibro = libroTemp.Titulo;

                prestamo.FechaDevolucionReal = DateTime.Now;

                // Lógica multa por retraso usando Strategy y Factory
                if (prestamo.FechaDevolucionReal > prestamo.FechaDevolucionPrevista)
                {
                    IMultaFactory fabricaMultaRetraso = new MultaPorRetrasoFactory();
                    Multa? nuevaMulta = fabricaMultaRetraso.CrearMulta(prestamo.SocioId, "Devolución tardía", prestamo);
                    if (nuevaMulta != null)
                    {
                        await _multaRepository.AddAsync(nuevaMulta); // Guarda la multa
                    }
                }

                await _prestamoRepository.UpdateAsync(prestamo); // Guarda el préstamo

                var libro = await _libroRepository.GetByIdAsync(prestamo.LibroId);
                if (libro != null)
                {
                    libro.Estado = EstadoLibro.Disponible;
                    await _libroRepository.UpdateAsync(libro); // Guarda el libro
                }

                TempData["SuccessMessage"] = $"Devolución del libro '{tituloLibro}' registrada exitosamente.";

                if (estaDanado)
                {
                    // Si está dañado, se redirige a Crear Multa (que mostrará su propio mensaje)
                    return RedirectToAction("Crear", "Multa", new { socioId = prestamo.SocioId, motivoDanado = true });
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Error: No se encontró el préstamo para registrar la devolución.";
            }

            return RedirectToAction(nameof(Index));
        }

        // --- DETALLES (GET) ---
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null) return NotFound();
            var prestamo = await _prestamoRepository.GetByIdWithDetailsAsync(id.Value);
            if (prestamo == null) return NotFound();
            return View(prestamo);
        }
    }
}