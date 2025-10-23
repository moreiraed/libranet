
using Microsoft.EntityFrameworkCore;
using libranet.Data;
using libranet.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace libranet.Repositories
{
    // Esta clase implementa la interfaz IMultaRepository.
    public class MultaRepository : IMultaRepository
    {
        // Referencia al DbContext.
        private readonly LibranetContext _context;

        // El constructor recibe el DbContext mediante inyección de dependencias.
        public MultaRepository(LibranetContext context)
        {
            _context = context;
        }

        // Implementación para obtener TODAS las multas CON detalles del Socio.
        public async Task<List<Multa>> GetAllWithDetailsAsync()
        {
            return await _context.Multas
                                 .Include(m => m.Socio) // Carga los datos del Socio relacionado
                                 .ToListAsync();
        }

        // Implementación para obtener UNA multa por ID (sin detalles).
        public async Task<Multa?> GetByIdAsync(int id)
        {
            // FindAsync es eficiente para búsquedas por clave primaria.
            return await _context.Multas.FindAsync(id);
        }

        // Implementación para añadir una nueva multa.
        public async Task AddAsync(Multa multa)
        {
            _context.Multas.Add(multa);
            await _context.SaveChangesAsync();
        }

        // Implementación para actualizar una multa existente (ej. marcar como pagada).
        public async Task UpdateAsync(Multa multa)
        {
            // Marcamos la entidad como modificada.
            _context.Entry(multa).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        // No implementamos DeleteAsync ya que las multas suelen guardarse como registro.
    }
}