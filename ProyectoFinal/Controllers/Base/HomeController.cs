using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoFinal.Models.Base;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using ProyectoFinal.Models; // Asegúrate de que tu ErrorViewModel esté en este namespace o en el correcto.
                            // Si ErrorViewModel está en Models.Base, no necesitas este using adicional.

namespace ProyectoFinal.Controllers.Base // Tu namespace actual
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DbCitasMedicasContext _context;

        public HomeController(ILogger<HomeController> logger, DbCitasMedicasContext context)
        {
            _logger = logger;
            _context = context;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            // 1. Valida si el usuario tiene una sesión abierta (está autenticado)
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // 2. Si es así, lo redirige a la pantalla principal, que es el Chat.
                return RedirectToAction("Index", "Chat");
            }

            // 3. Si no, lo redirige a la página para que inicie sesión.
            return RedirectToAction("Login", "Home");
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        // --- ¡NUEVA ACCIÓN: AccessDenied! ---
        [AllowAnonymous] // Es crucial que esta acción permita el acceso anónimo
        public IActionResult AccessDenied()
        {
            return View(); // Esto renderizará Views/Home/AccessDenied.cshtml
        }
        // --- FIN DE NUEVA ACCIÓN ---


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Asegúrate de que ErrorViewModel sea accesible.
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}