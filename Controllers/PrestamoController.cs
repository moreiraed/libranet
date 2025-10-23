using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using libranet.Models;
using libranet.Repositories; // ¡Importante! Añadimos el using para los repositorios
using Microsoft.EntityFrameworkCore; // Para DbUpdateConcurrencyException y SelectListItem (temporalmente)
using Microsoft.AspNetCore.Mvc.Rendering; // Necesario para SelectListItem
using System.Linq;
using System.Threading.Tasks;

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
             // CAMBIO 8: Usamos GetByIdAsync (sin detalles es suficiente aquí)
            var prestamo = await _prestamoRepository.GetByIdAsync(id);

            if (prestamo != null)
            {
                prestamo.FechaDevolucionReal = DateTime.Now;

                // Lógica de multa automática
                if (prestamo.FechaDevolucionReal > prestamo.FechaDevolucionPrevista)
                {
                    var diasDeRetraso = (prestamo.FechaDevolucionReal.Value.Date - prestamo.FechaDevolucionPrevista.Date).Days;
                     if (diasDeRetraso > 0) // Solo si hay al menos 1 día de retraso
                    {
                        var nuevaMulta = new Multa
                        {
                            SocioId = prestamo.SocioId,
                            Motivo = $"Devolución tardía de {diasDeRetraso} día(s).",
                            Monto = diasDeRetraso * 100, // Tarifa ejemplo
                            FechaCreacion = DateTime.Now,
                            Estado = EstadoMulta.Pendiente
                        };
                         // CAMBIO 9: Usamos el repositorio de multas para añadir
                        await _multaRepository.AddAsync(nuevaMulta);
                    }
                }

                // CAMBIO 10: Usamos el repositorio de préstamos para actualizar
                await _prestamoRepository.UpdateAsync(prestamo);

                // CAMBIO 11: Usamos el repositorio de libros para buscar y actualizar estado
                var libro = await _libroRepository.GetByIdAsync(prestamo.LibroId);
                if (libro != null)
                {
                    libro.Estado = EstadoLibro.Disponible;
                    await _libroRepository.UpdateAsync(libro);
                }

                // Redirección si está dañado (la lógica se mantiene)
                if (estaDanado)
                {
                    return RedirectToAction("Crear", "Multa", new { socioId = prestamo.SocioId });
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}