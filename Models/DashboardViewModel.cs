// Un ViewModel es una clase simple que solo contiene los datos que una vista necesita mostrar.
// No se guarda en la base de datos.
namespace Libranet.Models
{
    public class DashboardViewModel
    {
        public int LibrosPrestados { get; set; }
        public int PrestamosVencidos { get; set; }
        public int TotalSocios { get; set; }
        public int TotalLibros { get; set; }
    }
}