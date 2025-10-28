using Microsoft.EntityFrameworkCore;
using libranet.Data;
using libranet.Models;

namespace libranet.Repositories
{
    // Esta clase implementa la interfaz ISocioRepository.
    // Contiene la lógica real para interactuar con la base de datos para los Socios.
    public class SocioRepository : ISocioRepository
    {
        private readonly LibranetContext _context;

        // El constructor recibe el DbContext a través de inyección de dependencias.
        public SocioRepository(LibranetContext context)
        {
            _context = context;
        }

        // Implementación de los métodos definidos en la interfaz.

        // Obtener todos los socios.
        public async Task<List<Socio>> GetAllAsync()
        {
            return await _context.Socios.ToListAsync();
        }

        // Obtener un socio por su ID.
        public async Task<Socio?> GetByIdAsync(int id)
        {
            return await _context.Socios.FindAsync(id);
        }

        // Añadir un nuevo socio.
        public async Task AddAsync(Socio socio)
        {
            _context.Socios.Add(socio);
            await _context.SaveChangesAsync();
        }

        // Actualizar un socio existente.
        public async Task UpdateAsync(Socio socio)
        {
            // Le dice a EF que este objeto ha sido modificado.
            _context.Entry(socio).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        // Eliminar un socio por su ID.
        public async Task DeleteAsync(int id)
        {
            var socio = await GetByIdAsync(id);
            if (socio != null)
            {
                // Si lo encuentra, lo marca para eliminar.
                _context.Socios.Remove(socio);
                // Ejecuta la eliminación en la base de datos.
                await _context.SaveChangesAsync();
            }
        }

        // Buscar socios (autocompletado).
        public async Task<List<Socio>> SearchAsync(string term)
        {
            // Realiza la búsqueda por NumeroSocio o DNI, toma los primeros 10.
            return await _context.Socios
                .Where(s => s.NumeroSocio.Contains(term) || s.DNI.Contains(term))
                .Take(10)
                .ToListAsync();
        }

        // Obtener el último socio (para generar el número secuencial).
        public async Task<Socio?> GetLastAsync()
        {
            return await _context.Socios.OrderByDescending(s => s.SocioId).FirstOrDefaultAsync();
        }

        // Obtener un socio con sus detalles (préstamos y libros).
        public async Task<Socio?> GetByIdWithDetailsAsync(int id)
        {
            // Usamos Include y ThenInclude para cargar los datos relacionados.
            return await _context.Socios
                .Include(s => s.Prestamos)       // Carga la lista de préstamos del socio
                    .ThenInclude(p => p.Libro) // Por cada préstamo, carga el libro asociado
                .FirstOrDefaultAsync(m => m.SocioId == id); // Busca el socio por ID
        }

        // Verificar si un DNI ya existe.
        public async Task<bool> DniExistsAsync(string dni)
        {
            // Busca si existe algún socio cuyo DNI (ignorando mayúsculas/minúsculas)
            // coincida con el DNI proporcionado.
            // AnyAsync() es eficiente porque devuelve true tan pronto como encuentra una coincidencia.
            return await _context.Socios.AnyAsync(s => s.DNI.ToLower() == dni.ToLower());
        }

        public async Task<bool> DniExistsForAnotherSocioAsync(string dni, int socioIdToExclude)
        {
            // Busca si existe algún socio con el mismo DNI pero DIFERENTE SocioId.
            return await _context.Socios.AnyAsync(s => s.DNI.ToLower() == dni.ToLower() && s.SocioId != socioIdToExclude);
        }

    }
}