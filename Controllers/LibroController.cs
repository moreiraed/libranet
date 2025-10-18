// Usamos 'using' para importar las herramientas que necesitaremos.
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // Para proteger el controlador.
using libranet.Data; // Para usar nuestro LibranetContext.
using Microsoft.EntityFrameworkCore; // Para usar .ToListAsync().
using libranet.Models;

namespace libranet.Controllers
{
    [Authorize] // Esta etiqueta asegura que solo usuarios logueados puedan acceder.
    public class LibroController : Controller
    {
        // Inyectamos el contexto de la base de datos.
        private readonly LibranetContext _context;

        public LibroController(LibranetContext context)
        {
            _context = context;
        }

        // --- MÉTODO PARA LISTAR LOS LIBROS (GET) ---
        // Esta acción obtiene todos los libros de la base de datos y los muestra en una tabla.
        public async Task<IActionResult> Index()
        {
            // 1. Usamos el _context para acceder a la tabla 'Libros'.
            // 2. '.ToListAsync()' obtiene todos los registros de la tabla.
            var libros = await _context.Libros.ToListAsync();

            // 3. Enviamos la lista de libros a la vista para que los muestre.
            return View(libros);
        }

        // --- MÉTODO PARA MOSTRAR EL FORMULARIO (GET) ---
        // Esta acción se ejecuta cuando el usuario quiere ver la página para crear un libro.
        public IActionResult Crear()
        {
            // Creamos un nuevo objeto Libro con el estado por defecto "Disponible".
            var libro = new Libro { Estado = EstadoLibro.Disponible };
            // Enviamos este objeto a la vista para que el estado ya esté preseleccionado.
            return View(libro);
        }

        // --- MÉTODO PARA GUARDAR EL NUEVO LIBRO (POST) ---
        // Este método se ejecuta cuando el formulario es enviado.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Libro libro)
        {
            // 'ModelState.IsValid' comprueba si los datos recibidos son válidos.
            if (ModelState.IsValid)
            {
                // Añadimos el nuevo objeto 'libro' al contexto, preparándolo para ser guardado.
                _context.Add(libro);
                // Guardamos los cambios en la base de datos.
                await _context.SaveChangesAsync();
                // Redirigimos al usuario a la lista de libros.
                return RedirectToAction(nameof(Index));
            }
            // Si el modelo no es válido, volvemos a mostrar el formulario.
            return View(libro);
        }

        // --- MÉTODO PARA MOSTRAR EL FORMULARIO DE EDICIÓN (GET) ---
        // Este método busca el libro por su 'id' y muestra sus datos en un formulario.
        public async Task<IActionResult> Editar(int? id)
        {
            // Si no nos pasan un id, no podemos editar nada.
            if (id == null)
            {
                return NotFound();
            }

            // Buscamos el libro en la base de datos usando el id.
            var libro = await _context.Libros.FindAsync(id);

            // Si no encontramos un libro con ese id, devolvemos un error.
            if (libro == null)
            {
                return NotFound();
            }

            // Si lo encontramos, lo enviamos a la vista para que muestre el formulario.
            return View(libro);
        }

        // --- MÉTODO PARA GUARDAR LOS CAMBIOS (POST) ---
        // Este método recibe los datos modificados del formulario y los guarda en la BD.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Libro libro)
        {
            // Verificamos que el id del libro que queremos editar coincida con el que nos llega.
            if (id != libro.LibroId)
            {
                return NotFound();
            }

            // Si los datos enviados son válidos...
            if (ModelState.IsValid)
            {
                try
                {
                    // Le decimos al contexto que este objeto 'libro' ha sido modificado.
                    _context.Update(libro);
                    // Guardamos los cambios en la base de datos.
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Este es un manejo de error por si algo inesperado ocurre al guardar.
                    return NotFound();
                }

                // Redirigimos al usuario a la lista de libros.
                return RedirectToAction(nameof(Index));
            }

            // Si los datos no son válidos, volvemos a mostrar el formulario con los datos ingresados.
            return View(libro);
        }

        // --- MÉTODO PARA MOSTRAR LA PÁGINA DE CONFIRMACIÓN (GET) ---
        // Este método busca el libro por su 'id' para preguntar si realmente queremos eliminarlo.
        public async Task<IActionResult> Eliminar(int? id)
        {
            // Si no nos pasan un id, no podemos hacer nada.
            if (id == null)
            {
                return NotFound();
            }

            // Buscamos el libro en la base de datos para mostrar sus detalles.
            var libro = await _context.Libros
                .FirstOrDefaultAsync(m => m.LibroId == id);

            // Si no encontramos un libro con ese id, devolvemos un error.
            if (libro == null)
            {
                return NotFound();
            }

            // Enviamos el libro a la vista de confirmación.
            return View(libro);
        }

        // --- MÉTODO PARA EJECUTAR LA ELIMINACIÓN (POST) ---
        // Este método se ejecuta cuando el usuario hace clic en el botón "Eliminar" en la página de confirmación.
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            // Buscamos el libro que vamos a eliminar.
            var libro = await _context.Libros.FindAsync(id);

            if (libro != null)
            {
                // Le decimos al contexto que este objeto debe ser eliminado.
                _context.Libros.Remove(libro);
            }

            // Guardamos los cambios en la base de datos.
            await _context.SaveChangesAsync();

            // Redirigimos al usuario a la lista de libros.
            return RedirectToAction(nameof(Index));
        }

    }
}