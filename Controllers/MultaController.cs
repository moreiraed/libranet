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
    public class MultaController : Controller
    {
        // --- CAMBIO 1: Inyectamos los repositorios necesarios ---
        private readonly IMultaRepository _multaRepository;
        private readonly ISocioRepository _socioRepository; // Necesario para Crear (GET)

        // El constructor ahora recibe los repositorios
        public MultaController(IMultaRepository multaRepository, ISocioRepository socioRepository)
        {
            _multaRepository = multaRepository;
            _socioRepository = socioRepository;
        }

        // --- INDEX (Leer Todas) ---
        public async Task<IActionResult> Index()
        {
            // CAMBIO 2: Usamos el repositorio de multas con detalles
            var multas = await _multaRepository.GetAllWithDetailsAsync();
            return View(multas);
        }

        // --- CREAR (Mostrar Formulario GET) ---
        public async Task<IActionResult> Crear(int? socioId)
        {
            // CAMBIO 3: Usamos el repositorio de socios para obtener la lista
            var sociosList = await _socioRepository.GetAllAsync();

            // Convertimos la lista para el ViewModel
            var sociosSelectList = sociosList.Select(s => new SelectListItem
            {
                Value = s.SocioId.ToString(),
                Text = $"{s.NumeroSocio} - {s.Apellido}, {s.Nombre}"
            }).ToList();

            var viewModel = new MultaViewModel
            {
                Socios = sociosSelectList
            };

            if (socioId.HasValue)
            {
                viewModel.Multa.SocioId = socioId.Value;
            }

            return View(viewModel);
        }

        // --- CREAR (Guardar POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(MultaViewModel viewModel)
        {
            // Nota: ModelState.IsValid verifica el objeto viewModel.Multa
            if (ModelState.IsValid)
            {
                viewModel.Multa.FechaCreacion = DateTime.Now;
                viewModel.Multa.Estado = EstadoMulta.Pendiente;

                // CAMBIO 4: Usamos el repositorio de multas para añadir
                await _multaRepository.AddAsync(viewModel.Multa);
                return RedirectToAction(nameof(Index));
            }

            // Si no es válido, necesitamos recargar la lista de socios para el dropdown
            var sociosList = await _socioRepository.GetAllAsync();
            viewModel.Socios = sociosList.Select(s => new SelectListItem
            {
                Value = s.SocioId.ToString(),
                Text = $"{s.NumeroSocio} - {s.Apellido}, {s.Nombre}"
            }).ToList();

            return View(viewModel); // Devuelve la vista con el modelo y los errores
        }

        // --- PAGAR (Mostrar Confirmación GET) ---
        public async Task<IActionResult> Pagar(int? id)
        {
            if (id == null) return NotFound();

            // CAMBIO 5: Usamos el repositorio de multas. Necesitamos cargar el Socio manualmente
            // o crear un GetByIdWithDetailsAsync en IMultaRepository. Por simplicidad, lo cargaremos aquí.
            var multa = await _multaRepository.GetByIdAsync(id.Value);
            if (multa == null) return NotFound();

            // Cargamos el socio asociado manualmente (alternativa a Include)
            multa.Socio = await _socioRepository.GetByIdAsync(multa.SocioId);

            if (multa.Socio == null) return NotFound(); // Seguridad extra

            return View(multa);
        }

        // --- PAGAR (Confirmar POST) ---
        [HttpPost, ActionName("Pagar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PagarConfirmado(int id)
        {
            // CAMBIO 6: Usamos el repositorio para obtener la multa
            var multa = await _multaRepository.GetByIdAsync(id);

            if (multa != null)
            {
                multa.Estado = EstadoMulta.Pagada;
                // CAMBIO 7: Usamos el repositorio para actualizar
                await _multaRepository.UpdateAsync(multa);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}