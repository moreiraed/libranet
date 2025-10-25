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
    public class MultaController : Controller
    {
        private readonly IMultaRepository _multaRepository;
        private readonly ISocioRepository _socioRepository;
        private readonly IEnumerable<ICalculoMultaStrategy> _calculoMultaStrategies;

        public MultaController(
            IMultaRepository multaRepository,
            ISocioRepository socioRepository,
            IEnumerable<ICalculoMultaStrategy> calculoMultaStrategies)
        {
            _multaRepository = multaRepository;
            _socioRepository = socioRepository;
            _calculoMultaStrategies = calculoMultaStrategies;
        }

        // --- INDEX ---
        public async Task<IActionResult> Index()
        {
            var multas = await _multaRepository.GetAllWithDetailsAsync();
            return View(multas);
        }

        // --- CREAR (GET) ---
        public async Task<IActionResult> Crear(int? socioId, bool? motivoDanado)
        {
            var sociosList = await _socioRepository.GetAllAsync();
            var sociosSelectList = sociosList.Select(s => new SelectListItem
            {
                Value = s.SocioId.ToString(),
                Text = $"{s.NumeroSocio} - {s.Apellido}, {s.Nombre}"
            }).ToList();

            var viewModel = new MultaViewModel { Socios = sociosSelectList };

            if (socioId.HasValue) viewModel.Multa.SocioId = socioId.Value;

            if (motivoDanado == true && socioId.HasValue)
            {
                var estrategiaDano = _calculoMultaStrategies.OfType<CalculoMultaPorDanoStrategy>().FirstOrDefault();
                if (estrategiaDano != null)
                {
                    IMultaFactory fabricaMultaDano = new MultaPorDanoFactory();
                    // Usamos la fábrica para obtener los valores predeterminados
                    Multa? multaPorDanoTemp = fabricaMultaDano.CrearMulta(socioId.Value, "Libro devuelto con daños."); // Guardamos como Multa?

                    if (multaPorDanoTemp != null)
                    {
                        viewModel.Multa.Motivo = multaPorDanoTemp.Motivo;
                        viewModel.Multa.Monto = multaPorDanoTemp.Monto;
                    }
                }
            }
            return View(viewModel);
        }

        // --- CREAR (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(MultaViewModel viewModel)
        {
            // Verificamos explícitamente SocioId y Monto
            if (viewModel.Multa.SocioId <= 0) ModelState.AddModelError("Multa.SocioId", "Debe seleccionar un socio.");
            if (viewModel.Multa.Monto < 0) ModelState.AddModelError("Multa.Monto", "El monto no puede ser negativo.");
            if (string.IsNullOrWhiteSpace(viewModel.Multa.Motivo)) ModelState.AddModelError("Multa.Motivo", "El motivo es requerido.");


            if (ModelState.IsValid)
            {
                viewModel.Multa.FechaCreacion = DateTime.Now;
                viewModel.Multa.Estado = EstadoMulta.Pendiente;
                await _multaRepository.AddAsync(viewModel.Multa);

                TempData["SuccessMessage"] = $"Multa registrada exitosamente.";

                return RedirectToAction(nameof(Index));
            }

            // Recargamos socios si hay error
            var sociosList = await _socioRepository.GetAllAsync();
            viewModel.Socios = sociosList.Select(s => new SelectListItem
            {
                Value = s.SocioId.ToString(),
                Text = $"{s.NumeroSocio} - {s.Apellido}, {s.Nombre}"
            }).ToList();

            return View(viewModel);
        }

        // --- PAGAR (GET) ---
        public async Task<IActionResult> Pagar(int? id)
        {
            if (id == null) return NotFound();
            var multa = await _multaRepository.GetByIdAsync(id.Value);
            if (multa == null) return NotFound();
            multa.Socio = await _socioRepository.GetByIdAsync(multa.SocioId);
            if (multa.Socio == null) return NotFound(); // Seguridad extra
            return View(multa);
        }

        // --- PAGAR (POST) ---
        [HttpPost, ActionName("Pagar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PagarConfirmado(int id)
        {
            var multa = await _multaRepository.GetByIdAsync(id);
            if (multa != null)
            {
                multa.Estado = EstadoMulta.Pagada;
                await _multaRepository.UpdateAsync(multa);
                TempData["SuccessMessage"] = "Pago de multa registrado exitosamente.";
            }
            else
            {
                TempData["ErrorMessage"] = "Error: No se encontró la multa para registrar el pago.";
            }
            return RedirectToAction(nameof(Index));
        }

        // --- DETALLES (GET) ---
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null) return NotFound();
            var multa = (await _multaRepository.GetAllWithDetailsAsync()).FirstOrDefault(m => m.MultaId == id);
            if (multa == null) return NotFound();
            return View(multa);
        }
    }
}