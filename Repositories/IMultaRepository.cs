using libranet.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace libranet.Repositories
{
    // Define el contrato para el repositorio de Multas.
    public interface IMultaRepository
    {
        // Obtiene todas las multas, incluyendo los datos del Socio asociado.
        Task<List<Multa>> GetAllWithDetailsAsync();

        // Obtiene una multa específica por su ID.
        Task<Multa?> GetByIdAsync(int id);

        // Añade una nueva multa a la base de datos.
        Task AddAsync(Multa multa);

        // Actualiza una multa existente en la base de datos (ej. para marcarla como pagada).
        Task UpdateAsync(Multa multa);

        // (No incluimos DeleteAsync ya que las multas suelen mantenerse como registro)
    }
}