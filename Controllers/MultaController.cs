using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using libranet.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using libranet.Models;                  // Para encontrar MultaViewModel, EstadoMulta, etc.
using Microsoft.AspNetCore.Mvc.Rendering; // Para encontrar SelectListItem.
using System.Linq;                      // Para poder usar .Select().

namespace libranet.Controllers
{
    [Authorize] // Protege todo el controlador
    public class MultaController : Controller
    {
        // Inyectamos el contexto de la base de datos.
        private readonly LibranetContext _context;

        public MultaController(LibranetContext context)
        {
            _context = context;
        }

        // --- MÉTODO PARA LISTAR LAS MULTAS (GET) ---
        public async Task<IActionResult> Index()
        {
            // 1. Accedemos a la tabla 'Multas'.
            // 2. Usamos '.Include(m => m.Socio)' para cargar los datos del Socio relacionado.
            // 3. '.ToListAsync()' obtiene todos los registros.
            var multas = await _context.Multas.Include(m => m.Socio).ToListAsync();

            // 4. Enviamos la lista de multas a la vista.
            return View(multas);
        }

        // --- MÉTODO PARA MOSTRAR EL FORMULARIO DE MULTA (GET) ---
        // Añadimos un parámetro opcional 'int? socioId'.
        public async Task<IActionResult> Crear(int? socioId)
        {
            var socios = await _context.Socios
                                    .Select(s => new SelectListItem
                                    {
                                        Value = s.SocioId.ToString(),
                                        Text = s.Apellido + ", " + s.Nombre
                                    }).ToListAsync();

            var viewModel = new MultaViewModel
            {
                Socios = socios
            };

            // Si recibimos un socioId desde la redirección...
            if (socioId.HasValue)
            {
                // ...lo asignamos al modelo para que el dropdown aparezca preseleccionado.
                viewModel.Multa.SocioId = socioId.Value;
            }

            return View(viewModel);
        }

        // --- MÉTODO PARA GUARDAR LA NUEVA MULTA (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(MultaViewModel viewModel)
        {
            // Verificamos si los datos del formulario son válidos.
            if (ModelState.IsValid)
            {
                // Asignamos la fecha de creación y el estado por defecto.
                viewModel.Multa.FechaCreacion = DateTime.Now;
                viewModel.Multa.Estado = EstadoMulta.Pendiente;

                // Añadimos el nuevo objeto 'Multa' al contexto.
                _context.Add(viewModel.Multa);
                // Guardamos los cambios en la base de datos.
                await _context.SaveChangesAsync();
                // Redirigimos al usuario a la lista de multas.
                return RedirectToAction(nameof(Index));
            }

            // Si el modelo no es válido, volvemos a mostrar el formulario.
            // (Aquí necesitaríamos recargar la lista de socios, lo simplificamos por ahora)
            return View(viewModel);
        }

        // --- MÉTODO PARA MOSTRAR LA PÁGINA DE CONFIRMACIÓN DE PAGO (GET) ---
        public async Task<IActionResult> Pagar(int? id)
        {
            // Si no nos pasan un id de multa, no podemos hacer nada.
            if (id == null)
            {
                return NotFound();
            }

            // Buscamos la multa en la base de datos e incluimos los datos del socio
            // para poder mostrarlos en la página de confirmación.
            var multa = await _context.Multas
                .Include(m => m.Socio)
                .FirstOrDefaultAsync(m => m.MultaId == id);

            // Si no encontramos una multa con ese id, devolvemos un error.
            if (multa == null)
            {
                return NotFound();
            }

            // Enviamos la multa a la vista de confirmación.
            return View(multa);
        }

        // --- MÉTODO PARA EJECUTAR EL PAGO (POST) ---
        [HttpPost, ActionName("Pagar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PagarConfirmado(int id)
        {
            // Buscamos la multa que vamos a actualizar.
            var multa = await _context.Multas.FindAsync(id);

            if (multa != null)
            {
                // 1. Cambiamos el estado de la multa a "Pagada".
                multa.Estado = EstadoMulta.Pagada;
                _context.Update(multa);

                // 2. Guardamos el cambio en la base de datos.
                await _context.SaveChangesAsync();
            }

            // Redirigimos al usuario de vuelta a la lista de multas.
            return RedirectToAction(nameof(Index));
        }

    }
}