using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoFinal.Models.Base;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using ProyectoFinal.Models; // Aseg�rate de que tu ErrorViewModel est� en este namespace o en el correcto.
                            // Si ErrorViewModel est� en Models.Base, no necesitas este using adicional.

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
            // 1. Valida si el usuario tiene una sesi�n abierta (est� autenticado)
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // 2. Si es as�, lo redirige a la pantalla principal, que es el Chat.
                return RedirectToAction("Index", "Chat");
            }

            // 3. Si no, lo redirige a la p�gina para que inicie sesi�n.
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

        // --- �NUEVA ACCI�N: AccessDenied! ---
        [AllowAnonymous] // Es crucial que esta acci�n permita el acceso an�nimo
        public IActionResult AccessDenied()
        {
            return View(); // Esto renderizar� Views/Home/AccessDenied.cshtml
        }
        // --- FIN DE NUEVA ACCI�N ---


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Aseg�rate de que ErrorViewModel sea accesible.
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}