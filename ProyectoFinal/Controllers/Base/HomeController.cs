using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoFinal.Models.Base;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using ProyectoFinal.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore; // Asegúrate de que tu ErrorViewModel esté en este namespace o en el correcto.
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
        public async Task<IActionResult> Index()
        {
            // 1. Valida si el usuario tiene una sesión abierta (está autenticado)
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // --- INICIO: NUEVA LÓGICA DE VERIFICACIÓN DE PERFIL ---

                // a. Obtenemos el ID del usuario que ha iniciado sesión.
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdStr, out var userId))
                {
                    // Si hay un problema con el token/cookie, lo mejor es forzar un nuevo login.
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    return RedirectToAction("Login", "Home");
                }

                // b. Verificamos si existe un paciente asociado a este usuario.
                var patientExists = await _context.Patients.AnyAsync(p => p.UserId == userId);

                // c. Si NO tiene un perfil de paciente, lo redirigimos para que lo cree.
                if (!patientExists)
                {
                    // Opcional: Ponemos un mensaje para que el usuario sepa por qué lo redirigimos.
                    TempData["InfoMessage"] = "Para continuar, por favor completa tu perfil de paciente.";

                    // Asumimos que tendrás un PatientUIController con una acción Create.
                    return RedirectToAction("Index", "PatientUI");
                }

                // --- FIN: NUEVA LÓGICA ---

                // d. Si el perfil SÍ existe, lo redirige a la pantalla principal, que es el Chat.
                return RedirectToAction("Index", "Chat");
            }

            // 3. Si no está autenticado, lo redirige a la página para que inicie sesión.
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