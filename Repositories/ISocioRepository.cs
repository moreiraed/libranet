using libranet.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace libranet.Repositories
{
    // Esta interfaz define el contrato para cualquier clase que quiera actuar
    // como un repositorio de Socios.
    public interface ISocioRepository
    {
        // --- OPERACIONES CRUD BÁSICAS ---

        // Obtiene todos los socios.
        Task<List<Socio>> GetAllAsync();

        // Obtiene un socio específico por su ID.
        Task<Socio?> GetByIdAsync(int id); // El '?' indica que podría no encontrarlo (devuelve null).

        // Añade un nuevo socio a la base de datos.
        Task AddAsync(Socio socio);

        // Actualiza un socio existente en la base de datos.
        Task UpdateAsync(Socio socio);

        // Elimina un socio de la base de datos por su ID.
        Task DeleteAsync(int id);

        // --- OPERACIONES ESPECÍFICAS ---

        // Busca socios por número de socio o DNI (para el autocompletado).
        Task<List<Socio>> SearchAsync(string term);

        // Obtiene el último socio registrado (para generar el nuevo número secuencial).
        Task<Socio?> GetLastAsync();

        // Obtiene un socio por ID, incluyendo sus préstamos y los libros de esos préstamos.
        Task<Socio?> GetByIdWithDetailsAsync(int id);

        // Verifica si ya existe un socio con el DNI especificado.
        Task<bool> DniExistsAsync(string dni);

        // Verifica si el DNI existe para OTRO socio diferente al ID proporcionado.
        Task<bool> DniExistsForAnotherSocioAsync(string dni, int socioIdToExclude);

        // Busca socios por Apellido, Nombre, DNI o NumeroSocio para la lista Index.
        Task<List<Socio>> FindAsync(string searchTerm);
    }
}