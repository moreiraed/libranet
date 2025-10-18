using Microsoft.AspNetCore.Mvc;
using Libranet.Data;
using System.Linq;
using System.Security.Claims; // Necesario para crear la "identidad" del usuario.
using Microsoft.AspNetCore.Authentication; // Necesario para el SignIn y SignOut.

namespace Libranet.Controllers
{
    public class AccountController : Controller
    {
        private readonly LibranetContext _context;

        public AccountController(LibranetContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var admin = _context.Admins.FirstOrDefault(a => a.Username == username);

            if (admin != null && BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash))
            {
                // --- CREACIÓN DE LA SESIÓN ---
                // 1. Creamos una lista de "claims". Un claim es una pieza de información
                //    sobre el usuario (como su nombre).
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, admin.Username)
                };

                // 2. Creamos la "identidad" del usuario con estos claims.
                var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");

                // 3. Creamos el "principal" que representa al usuario autenticado.
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // 4. Usamos el método SignInAsync para crear la cookie de sesión en el navegador del usuario.
                await HttpContext.SignInAsync("CookieAuth", claimsPrincipal);

                return RedirectToAction("Index", "Home");
            }

            ViewData["Error"] = "Usuario o contraseña incorrectos.";
            return View();
        }

        // --- LÓGICA DEL LOGOUT ---
        public async Task<IActionResult> Logout()
        {
            // Usamos el método SignOutAsync para eliminar la cookie de sesión.
            await HttpContext.SignOutAsync("CookieAuth");

            // Redirigimos al usuario a la página de login.
            return RedirectToAction("Login", "Account");
        }
    }
}