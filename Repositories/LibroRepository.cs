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

        public async Task<List<Libro>> GetAllAsync()
        {
            return await _context.Libros.ToListAsync();
        }

        public async Task<Libro?> GetByIdAsync(int id)
        {
            return await _context.Libros.FindAsync(id);
        }

        public async Task AddAsync(Libro libro)
        {
            _context.Libros.Add(libro);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Libro libro)
        {
            // Asegurarse de que EF Core rastree la entidad antes de marcarla como modificada.
            // Una forma segura es adjuntarla si no está siendo rastreada.
             _context.Attach(libro).State = EntityState.Modified;
            // _context.Entry(libro).State = EntityState.Modified; // Alternativa si ya está rastreada.
            await _context.SaveChangesAsync();
        }


        public async Task DeleteAsync(int id)
        {
            var libro = await GetByIdAsync(id);
            if (libro != null)
            {
                _context.Libros.Remove(libro);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Libro>> SearchAvailableAsync(string term)
        {
            return await _context.Libros
                .Where(l => l.Estado == EstadoLibro.Disponible &&
                            (l.Titulo.Contains(term) || l.Autor.Contains(term) || l.ISBN.Contains(term)))
                .Take(10)
                .ToListAsync();
        }

        public async Task<Libro?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Libros
                .Include(l => l.Prestamos)       // Carga la lista de préstamos del libro
                    .ThenInclude(p => p.Socio) // Por cada préstamo, carga el socio asociado
                .FirstOrDefaultAsync(m => m.LibroId == id); // Busca el libro por ID
        }
    }
}