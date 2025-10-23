using Microsoft.EntityFrameworkCore;
using libranet.Data;
using libranet.Models;
using System.Threading.Tasks;

namespace libranet.Repositories
{
    // Implementa la interfaz IAdminRepository.
    public class AdminRepository : IAdminRepository
    {
        private readonly LibranetContext _context;

        public AdminRepository(LibranetContext context)
        {
            _context = context;
        }

        // Implementación para buscar un admin por nombre de usuario.
        public async Task<Admin?> GetByUsernameAsync(string username)
        {
            // Busca el primer admin cuyo Username coincida (ignorando mayúsculas/minúsculas).
            // Es importante usar ToLower() para asegurar la comparación correcta.
            return await _context.Admins
                                 .FirstOrDefaultAsync(a => a.Username.ToLower() == username.ToLower());
        }
    }
}