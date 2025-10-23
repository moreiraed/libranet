using libranet.Models;
using System.Threading.Tasks;

namespace libranet.Repositories
{
    // Define el contrato para el repositorio de Admins.
    public interface IAdminRepository
    {
        // Busca un admin por su nombre de usuario.
        Task<Admin?> GetByUsernameAsync(string username);

        // (No necesitamos Add, Update, Delete para este proyecto,
        // ya que el admin se crea al inicio y no se gestiona).
    }
}