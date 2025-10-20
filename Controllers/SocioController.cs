// Usamos 'using' para importar las herramientas que necesitaremos.
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // Para proteger el controlador.
using libranet.Data; // Para usar nuestro LibranetContext.
using libranet.Models; // Para usar la clase Socio.
using Microsoft.EntityFrameworkCore;

namespace Lzibranet.Controllers
{
    [Authorize] // Esta etiqueta asegura que solo usuarios logueados puedan acceder.
    public class SocioController : Controller
    {
        // Inyectamos el contexto de la base de datos, igual que en HomeController.
        private readonly LibranetContext _context;

        public SocioController(LibranetContext context)
        {
            _context = context;
        }

        // Este método se encarga de mostrar la lista de todos los socios.
        // Es 'async' porque la consulta a la base de datos puede tomar un momento.
        public async Task<IActionResult> Index()
        {
            // 1. Usamos el _context para acceder a la tabla 'Socios'.
            // 2. '.ToListAsync()' obtiene todos los registros de la tabla de forma asíncrona.
            // 3. Guardamos la lista de socios en la variable 'socios'.
            var socios = await _context.Socios.ToListAsync();

            // 4. Enviamos la lista de socios a la vista para que pueda mostrarla.
            return View(socios);
        }

        // --- MÉTODO PARA MOSTRAR EL FORMULARIO (GET) ---
        // Esta acción se ejecuta cuando el usuario quiere ver la página para crear un socio.
        public IActionResult Crear()
        {
            // Simplemente devuelve la vista con el formulario vacío.
            return View();
        }

        // --- MÉTODO PARA GUARDAR EL NUEVO SOCIO (ACTUALIZADO CON NÚMERO SECUENCIAL) ---
        // Se ejecuta cuando el formulario es enviado.
        [HttpPost] // [HttpPost] indica que solo responde a peticiones de tipo POST (envíos de formulario).
        [ValidateAntiForgeryToken] // [ValidateAntiForgeryToken] es una medida de seguridad para prevenir ataques.
        public async Task<IActionResult> Crear(Socio socio)
        {
            // 'ModelState.IsValid' comprueba si los datos recibidos son válidos.
            if (ModelState.IsValid)
            {
                // --- LÓGICA PARA GENERAR EL NÚMERO DE SOCIO SECUENCIAL ---

                // 1. Buscamos el último socio registrado, ordenando por SocioId de forma descendente.
                var ultimoSocio = await _context.Socios.OrderByDescending(s => s.SocioId).FirstOrDefaultAsync();
                
                // 2. Definimos el número inicial por si es el primer socio.
                int nuevoNumero = 1; 

                if (ultimoSocio != null && !string.IsNullOrEmpty(ultimoSocio.NumeroSocio))
                {
                    // 3. Si ya existen socios, intentamos convertir su NumeroSocio a un entero.
                    if (int.TryParse(ultimoSocio.NumeroSocio, out int ultimoNumero))
                    {
                        // 4. Si la conversión es exitosa, le sumamos 1 para obtener el nuevo número.
                        nuevoNumero = ultimoNumero + 1;
                    }
                }

                // 5. Formateamos el número como un string de 5 dígitos, rellenando con ceros a la izquierda (ej: "00001").
                socio.NumeroSocio = nuevoNumero.ToString("D5");

                // --- FIN DE LA LÓGICA DE GENERACIÓN ---

                // Asignamos la fecha de alta al momento actual.
                socio.FechaDeAlta = DateTime.Now;

                // Añadimos el nuevo objeto 'socio' al contexto para prepararlo.
                _context.Add(socio);
                // Guardamos los cambios en la base de datos.
                await _context.SaveChangesAsync();

                // Redirigimos al usuario a la lista de socios.
                return RedirectToAction(nameof(Index));
            }

            // Si el modelo no es válido, volvemos a mostrar el formulario.
            return View(socio);
        }

        // --- MÉTODO PARA MOSTRAR EL FORMULARIO DE EDICIÓN (GET) ---
        // Este método busca al socio por su 'id' y muestra sus datos en un formulario.
        public async Task<IActionResult> Editar(int? id)
        {
            // Si no nos pasan un id, no podemos editar nada.
            if (id == null)
            {
                return NotFound();
            }

            // Buscamos el socio en la base de datos usando el id.
            var socio = await _context.Socios.FindAsync(id);

            // Si no encontramos un socio con ese id, devolvemos un error.
            if (socio == null)
            {
                return NotFound();
            }

            // Si lo encontramos, lo enviamos a la vista para que muestre el formulario.
            return View(socio);
        }

        // --- MÉTODO PARA GUARDAR LOS CAMBIOS (POST) ---
        // Este método recibe los datos modificados del formulario y los guarda en la BD.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Socio socio)
        {
            // Verificamos que el id del socio que queremos editar coincida con el que nos llega del formulario.
            if (id != socio.SocioId)
            {
                return NotFound();
            }

            // Si los datos enviados son válidos...
            if (ModelState.IsValid)
            {
                try
                {
                    // Le decimos al contexto que este objeto 'socio' ha sido modificado.
                    _context.Update(socio);
                    // Guardamos los cambios en la base de datos.
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // (Este es un manejo de error avanzado por si dos personas intentan editar lo mismo a la vez)
                    // Simplemente redirigimos si algo sale mal.
                    return NotFound();
                }

                // Redirigimos al usuario a la lista de socios.
                return RedirectToAction(nameof(Index));
            }

            // Si los datos no son válidos, volvemos a mostrar el formulario.
            return View(socio);
        }

        // --- MÉTODO PARA MOSTRAR LA PÁGINA DE CONFIRMACIÓN (GET) ---
        // Este método busca al socio por su 'id' para preguntar si realmente queremos eliminarlo.
        public async Task<IActionResult> Eliminar(int? id)
        {
            // Si no nos pasan un id, no podemos hacer nada.
            if (id == null)
            {
                return NotFound();
            }

            // Buscamos el socio en la base de datos para mostrar sus detalles.
            var socio = await _context.Socios
                .FirstOrDefaultAsync(m => m.SocioId == id);

            // Si no encontramos un socio con ese id, devolvemos un error.
            if (socio == null)
            {
                return NotFound();
            }

            // Enviamos el socio a la vista de confirmación.
            return View(socio);
        }

        // --- MÉTODO PARA EJECUTAR LA ELIMINACIÓN (POST) ---
        // Este método se ejecuta cuando el usuario hace clic en el botón "Eliminar" en la página de confirmación.
        [HttpPost, ActionName("Eliminar")] // ActionName("Eliminar") permite que el formulario apunte a /Socio/Eliminar
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            // Buscamos el socio que vamos a eliminar.
            var socio = await _context.Socios.FindAsync(id);

            if (socio != null)
            {
                // Le decimos al contexto que este objeto debe ser eliminado.
                _context.Socios.Remove(socio);
            }

            // Guardamos los cambios en la base de datos.
            await _context.SaveChangesAsync();

            // Redirigimos al usuario a la lista de socios.
            return RedirectToAction(nameof(Index));
        }

        // --- ENDPOINT DE BÚSQUEDA DE SOCIOS (API) ---
        // Este método está diseñado para ser llamado por JavaScript.
        // Recibe un 'term' (el texto que el usuario está escribiendo).
        [HttpGet]
        public async Task<IActionResult> Buscar(string term)
        {
            if (string.IsNullOrEmpty(term))
            {
                return Json(new List<object>());
            }

            // Buscamos por Número de Socio O por DNI.
            var socios = await _context.Socios
                .Where(s => s.NumeroSocio.Contains(term) || s.DNI.Contains(term))
                .Select(s => new
                {
                    id = s.SocioId,
                    label = $"{s.NumeroSocio} - {s.Apellido}, {s.Nombre} (DNI: {s.DNI})"
                })
                .Take(10)
                .ToListAsync();

            return Json(socios);
        }
        
        // --- MÉTODO PARA MOSTRAR LOS DETALLES DE UN SOCIO (GET) ---
        public async Task<IActionResult> Detalles(int? id)
        {
            // Si no nos pasan un id, no podemos mostrar nada.
            if (id == null)
            {
                return NotFound();
            }

            // Buscamos el socio en la base de datos.
            // Usamos '.Include()' dos veces para cargar la información relacionada:
            // 1. Incluimos la lista de Préstamos de este socio.
            // 2. Con '.ThenInclude()', le decimos que por cada Préstamo, también cargue los datos del Libro asociado.
            var socio = await _context.Socios
                .Include(s => s.Prestamos)
                    .ThenInclude(p => p.Libro)
                .FirstOrDefaultAsync(m => m.SocioId == id);

            // Si no encontramos un socio con ese id, devolvemos un error.
            if (socio == null)
            {
                return NotFound();
            }

            // Enviamos el objeto 'socio' (con toda su información y préstamos) a la vista.
            return View(socio);
        }

    }
}