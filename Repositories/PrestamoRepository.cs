using Microsoft.EntityFrameworkCore;
using libranet.Data;
using libranet.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace libranet.Repositories
{
    public class PrestamoRepository : IPrestamoRepository
    {
        private readonly LibranetContext _context;

        public PrestamoRepository(LibranetContext context)
        {
            _context = context;
        }

        // Implementación para obtener TODOS los préstamos CON detalles (Socio y Libro).
        public async Task<List<Prestamo>> GetAllWithDetailsAsync()
        {
            return await _context.Prestamos
                                 .Include(p => p.Socio) // Carga el Socio relacionado
                                 .Include(p => p.Libro) // Carga el Libro relacionado
                                 .ToListAsync();
        }

        // Implementación para obtener UN préstamo por ID CON detalles.
        public async Task<Prestamo?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Prestamos
                                 .Include(p => p.Socio)
                                 .Include(p => p.Libro)
                                 .FirstOrDefaultAsync(p => p.PrestamoId == id);
        }

         // Implementación para obtener UN préstamo por ID SIN cargar detalles.
        public async Task<Prestamo?> GetByIdAsync(int id)
        {
             // FindAsync es eficiente para buscar por clave primaria cuando no necesitas relaciones.
            return await _context.Prestamos.FindAsync(id);
        }


        // Implementación para añadir un nuevo préstamo.
        public async Task AddAsync(Prestamo prestamo)
        {
            _context.Prestamos.Add(prestamo);
            await _context.SaveChangesAsync();
        }

        // Implementación para actualizar un préstamo existente.
        public async Task UpdateAsync(Prestamo prestamo)
        {
            // Marcamos la entidad como modificada para que EF Core sepa que debe actualizarla.
            _context.Entry(prestamo).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        // No implementamos DeleteAsync, ya que los préstamos se finalizan (actualizan), no se borran.
    }
}