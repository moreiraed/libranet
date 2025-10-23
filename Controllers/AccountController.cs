using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using libranet.Models; // Necesario para Admin
using libranet.Repositories; // ¡Importante! Añadimos using para los repositorios
using System.Threading.Tasks; // Necesario para Task<>
using System.Collections.Generic; // Necesario para List<>
using Microsoft.AspNetCore.Authorization; // Necesario para [Authorize]

namespace libranet.Controllers
{
    public class AccountController : Controller
    {
        // --- CAMBIO 1: Inyectamos IAdminRepository ---
        private readonly IAdminRepository _adminRepository;

        // El constructor ahora recibe IAdminRepository
        public AccountController(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        // --- VISTA DEL LOGIN (GET) ---
        public IActionResult Login()
        {
            // Si el usuario ya está autenticado, lo redirigimos al Home.
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }


        // --- LÓGICA DEL LOGIN (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            // CAMBIO 2: Usamos el repositorio para buscar al admin
            var admin = await _adminRepository.GetByUsernameAsync(username);

            // Verificamos si encontramos un admin y si la contraseña es correcta
            // Usamos BCrypt.Verify para comparar la contraseña ingresada con el hash guardado.
            if (admin != null && BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, admin.Username),
                    // Podríamos añadir otros claims si fueran necesarios (ej. roles)
                };

                var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // Creamos la cookie de sesión
                await HttpContext.SignInAsync("CookieAuth", claimsPrincipal);

                return RedirectToAction("Index", "Home"); // Redirige al panel de control
            }

            // Si el login falla, mostramos mensaje de error
            ViewData["Error"] = "Usuario o contraseña incorrectos.";
            return View(); // Vuelve a mostrar el formulario de login
        }

        // --- LÓGICA DEL LOGOUT ---
        [Authorize] // Solo usuarios logueados pueden hacer logout
        [HttpPost] // Acción que cambia estado debe ser POST
        [ValidateAntiForgeryToken] // Seguridad
        public async Task<IActionResult> Logout()
        {
            // Eliminamos la cookie de sesión
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Login", "Account"); // Redirigimos al login
        }
    }
}