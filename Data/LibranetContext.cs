using Microsoft.EntityFrameworkCore;
using Libranet.Models;

namespace Libranet.Data
{
    public class LibranetContext : DbContext
    {
        public LibranetContext(DbContextOptions<LibranetContext> options) : base(options)
        {
        }

        public DbSet<Admin> Admins { get; set; }
        public DbSet<Socio> Socios { get; set; }
        public DbSet<Libro> Libros { get; set; }
        public DbSet<Prestamo> Prestamos { get; set; }
        public DbSet<Multa> Multas { get; set; }
    }
}