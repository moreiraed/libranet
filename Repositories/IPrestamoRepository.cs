// --- COMENTARIOS SOBRE ESTE CÓDIGO ---
using libranet.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace libranet.Repositories
{
    // Define el contrato para el repositorio de Préstamos.
    public interface IPrestamoRepository
    {
        // Obtiene todos los préstamos, incluyendo los datos del Socio y Libro asociados.
        Task<List<Prestamo>> GetAllWithDetailsAsync();

        // Obtiene un préstamo específico por su ID, incluyendo Socio y Libro.
        Task<Prestamo?> GetByIdWithDetailsAsync(int id);

        // Obtiene un préstamo específico por su ID (sin detalles).
        Task<Prestamo?> GetByIdAsync(int id);

        // Añade un nuevo préstamo a la base de datos.
        Task AddAsync(Prestamo prestamo);

        // Actualiza un préstamo existente en la base de datos.
        Task UpdateAsync(Prestamo prestamo);

        // (No incluimos DeleteAsync ya que los préstamos no se suelen eliminar, solo finalizar)
    }
}