using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using libranet.Models;
using libranet.Repositories; // ¡Importante! Añadimos el using para nuestros repositorios.
using Microsoft.EntityFrameworkCore; // Todavía necesario para DbUpdateConcurrencyException
using System.Threading.Tasks; // Necesario para Task<>
using System.Linq; // Necesario para Select() en Buscar()
using System.Collections.Generic; // Necesario para List<> en Buscar()

namespace libranet.Controllers
{
    [Authorize]
    public class SocioController : Controller
    {
        private readonly ISocioRepository _socioRepository;

        public SocioController(ISocioRepository socioRepository)
        {
            _socioRepository = socioRepository;
        }

        // --- INDEX (Leer Todos) ---
        public async Task<IActionResult> Index()
        {
            var socios = await _socioRepository.GetAllAsync();
            return View(socios);
        }

        // --- CREAR (Mostrar Formulario GET) ---
        public IActionResult Crear()
        {
            return View();
        }

        // --- CREAR (Guardar POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Socio socio)
        {
            if (ModelState.IsValid)
            {
                // Usamos el repositorio para obtener el último socio.
                var ultimoSocio = await _socioRepository.GetLastAsync();
                int nuevoNumero = 1;

                if (ultimoSocio != null && !string.IsNullOrEmpty(ultimoSocio.NumeroSocio))
                {
                    if (int.TryParse(ultimoSocio.NumeroSocio, out int ultimoNumero))
                    {
                        nuevoNumero = ultimoNumero + 1;
                    }
                }
                socio.NumeroSocio = nuevoNumero.ToString("D5");
                socio.FechaDeAlta = DateTime.Now;

                // Usamos el repositorio para añadir el nuevo socio.
                await _socioRepository.AddAsync(socio);
                return RedirectToAction(nameof(Index));
            }
            return View(socio);
        }

        // --- EDITAR (Mostrar Formulario GET) ---
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null) return NotFound();

            // Usamos el repositorio para obtener el socio por ID.
            var socio = await _socioRepository.GetByIdAsync(id.Value);
            if (socio == null) return NotFound();

            return View(socio);
        }

        // --- EDITAR (Guardar POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Socio socioFormulario)
        {
            if (id != socioFormulario.SocioId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Buscamos el socio ORIGINAL en la base de datos usando el ID.
                    var socioOriginal = await _socioRepository.GetByIdAsync(id);
                    if (socioOriginal == null)
                    {
                        return NotFound();
                    }

                    // Copiamos solo los valores que vienen del formulario
                    // al objeto original que recuperamos de la base de datos.
                    socioOriginal.Apellido = socioFormulario.Apellido;
                    socioOriginal.Nombre = socioFormulario.Nombre;
                    socioOriginal.DNI = socioFormulario.DNI;
                    socioOriginal.Email = socioFormulario.Email;
                    socioOriginal.Telefono = socioFormulario.Telefono;
                    socioOriginal.Direccion = socioFormulario.Direccion;
                    // Los campos NumeroSocio y FechaDeAlta del socioOriginal NO se tocan.

                    // Le decimos al repositorio que actualice el objeto ORIGINAL (modificado).
                    await _socioRepository.UpdateAsync(socioOriginal);
                }
                catch (DbUpdateConcurrencyException)
                {
                    return NotFound();
                }
                return RedirectToAction(nameof(Index));
            }
            return View(socioFormulario);
        }

        // --- ELIMINAR (Mostrar Confirmación GET) ---
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null) return NotFound();

            // Usamos el repositorio para obtener el socio por ID.
            var socio = await _socioRepository.GetByIdAsync(id.Value);
            if (socio == null) return NotFound();

            return View(socio);
        }

        // --- ELIMINAR (Confirmar POST) ---
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            // Usamos el repositorio para eliminar el socio.
            await _socioRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // --- BUSCAR (API Autocompletado GET) ---
        [HttpGet]
        public async Task<IActionResult> Buscar(string term)
        {
            if (string.IsNullOrEmpty(term)) return Json(new List<object>());

            // Usamos el repositorio para buscar socios.
            var socios = await _socioRepository.SearchAsync(term);

            // Transformación a formato {id, label}
            var result = socios.Select(s => new {
                id = s.SocioId,
                label = $"{s.NumeroSocio} - {s.Apellido}, {s.Nombre} (DNI: {s.DNI})"
            }).ToList();

            return Json(result);
        }

        // --- DETALLES (Mostrar GET) ---
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null) return NotFound();

            // Usamos el método específico del repositorio que carga los detalles.
            var socio = await _socioRepository.GetByIdWithDetailsAsync(id.Value);

            if (socio == null) return NotFound();

            return View(socio);
        }
    }
}