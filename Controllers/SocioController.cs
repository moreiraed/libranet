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

        // Este es el método que se ejecuta cuando el formulario es enviado.
        // [HttpPost] indica que solo responde a peticiones de tipo POST (envíos de formulario).
        // [ValidateAntiForgeryToken] es una medida de seguridad para prevenir ataques.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Socio socio)
        {
            // 'ModelState.IsValid' comprueba si los datos recibidos cumplen con las reglas
            // del modelo (por ejemplo, si un campo requerido está lleno).
            if (ModelState.IsValid)
            {
                // --- LÓGICA PARA CREAR EL SOCIO ---

                // 1. Asignamos la fecha de alta al momento actual.
                socio.FechaDeAlta = DateTime.Now;

                // 2. Generamos un número de socio único.
                //    Usamos los primeros 8 caracteres de un GUID para crear un código aleatorio.
                socio.NumeroSocio = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

                // 3. Añadimos el nuevo objeto 'socio' al contexto de la base de datos.
                //    Esto lo prepara para ser guardado.
                _context.Add(socio);

                // 4. Guardamos los cambios en la base de datos de forma asíncrona.
                await _context.SaveChangesAsync();

                // 5. Redirigimos al usuario a la lista de socios (la crearemos en el siguiente paso).
                return RedirectToAction(nameof(Index));
            }

            // Si el modelo no es válido (por ejemplo, faltó un dato),
            // volvemos a mostrar el formulario con los datos que el usuario ya había ingresado.
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

    }
}