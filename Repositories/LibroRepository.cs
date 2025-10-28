using Microsoft.EntityFrameworkCore;
using libranet.Data;
using libranet.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace libranet.Repositories
{
    public class LibroRepository : ILibroRepository
    {
        private readonly LibranetContext _context;

        public LibroRepository(LibranetContext context)
        {
            _context = context;
        }

        // Obtener todos los libros.
        public async Task<List<Libro>> GetAllAsync()
        {
            return await _context.Libros.ToListAsync();
        }

        // Obtener un libro por su ID.
        public async Task<Libro?> GetByIdAsync(int id)
        {
            return await _context.Libros.FindAsync(id);
        }

        // Añadir un nuevo libro.
        public async Task AddAsync(Libro libro)
        {
            _context.Libros.Add(libro);
            await _context.SaveChangesAsync();
        }

        // Actualizar un libro existente.
        public async Task UpdateAsync(Libro libro)
        {
            // Asegurarse de que EF Core rastree la entidad antes de marcarla como modificada.
            // Una forma segura es adjuntarla si no está siendo rastreada.
             _context.Attach(libro).State = EntityState.Modified;
            // _context.Entry(libro).State = EntityState.Modified; // Alternativa si ya está rastreada.
            await _context.SaveChangesAsync();
        }

        // Eliminar un libro por su ID.
        public async Task DeleteAsync(int id)
        {
            var libro = await GetByIdAsync(id);
            if (libro != null)
            {
                _context.Libros.Remove(libro);
                await _context.SaveChangesAsync();
            }
        }

        // Buscar libros disponibles (para autocompletado).
        public async Task<List<Libro>> SearchAvailableAsync(string term)
        {
            // Convertimos a minúsculas una vez
            var lowerTerm = term.ToLower();

            return await _context.Libros
                .Where(l => l.Estado == EstadoLibro.Disponible &&
                            (
                                (l.Titulo != null && l.Titulo.ToLower().Contains(lowerTerm)) || // <-- Usa ToLower()
                                (l.Autor != null && l.Autor.ToLower().Contains(lowerTerm)) ||  // <-- Usa ToLower()
                                (l.ISBN != null && l.ISBN.Contains(term)) // ISBN se mantiene
                            )
                       )
                .Take(10)
                .ToListAsync();
        }

        // Obtener un libro con sus detalles (préstamos y socios).
        public async Task<Libro?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Libros
                .Include(l => l.Prestamos)       // Carga la lista de préstamos del libro
                    .ThenInclude(p => p.Socio) // Por cada préstamo, carga el socio asociado
                .FirstOrDefaultAsync(m => m.LibroId == id); // Busca el libro por ID
        }

        // Implementación para la búsqueda en la página Index de Libros.
        public async Task<List<Libro>> FindAsync(string searchTerm)
        {
            // Convertimos a minúsculas una vez. Usamos ToLower().
            var lowerCaseSearchTerm = searchTerm.ToLower();

            // La consulta busca en los tres campos relevantes.
            // No usamos .Take(), queremos todos los resultados.
            return await _context.Libros
                .Where(l =>
                    (l.Titulo != null && l.Titulo.ToLower().Contains(lowerCaseSearchTerm)) || 
                    (l.Autor != null && l.Autor.ToLower().Contains(lowerCaseSearchTerm)) || 
                    (l.ISBN != null && l.ISBN.Contains(searchTerm))
                ).ToListAsync();
        }

    }
}