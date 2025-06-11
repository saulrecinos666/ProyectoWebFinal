using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoFinal.Models.Base;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdStr, out var userId))
                {
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    return RedirectToAction("Login", "Home");
                }

                if (User.HasClaim("Permission", "can_manage_patients"))
                {
                    // Usamos IgnoreQueryFilters() como la prueba definitiva
                    var patientExists = await _context.Patients.AnyAsync(p => p.UserId == userId);

                    if (!patientExists)
                    {
                        // Si NO existe, se va a crear el perfil
                        TempData["InfoMessage"] = "Para usar el sistema, primero debe completar su perfil de paciente.";
                        return RedirectToAction("Index", "PatientUI");
                    }
                }

                // Si SÍ existe, se va al chat
                return RedirectToAction("Index", "Chat");
            }

            // Si no está autenticado, va al login
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