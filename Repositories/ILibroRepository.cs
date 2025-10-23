using libranet.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace libranet.Repositories
{
    public interface ILibroRepository
    {
        // Obtiene todos los libros.
        Task<List<Libro>> GetAllAsync();

        // Obtiene un libro por su ID.
        Task<Libro?> GetByIdAsync(int id);

        // Añade un nuevo libro.
        Task AddAsync(Libro libro);

        // Actualiza un libro existente.
        Task UpdateAsync(Libro libro);

        // Elimina un libro por su ID.
        Task DeleteAsync(int id);

        // Busca libros disponibles por término (para autocompletado).
        Task<List<Libro>> SearchAvailableAsync(string term);

        // Obtiene un libro por ID, incluyendo sus préstamos y socios.
        Task<Libro?> GetByIdWithDetailsAsync(int id);
    }
}