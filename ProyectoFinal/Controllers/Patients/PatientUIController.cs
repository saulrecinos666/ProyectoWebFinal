using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Models.Base;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProyectoFinal.Controllers.Patients
{
    [Authorize]
    [Route("PatientUI")]
    public class PatientUIController : Controller
    {
        private readonly DbCitasMedicasContext _context;

        // Volvemos a inyectar el DbContext porque lo necesitamos para la verificación
        public PatientUIController(DbCitasMedicasContext context)
        {
            _context = context;
        }

        [HttpGet]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized("Sesión inválida.");
            }

            // Verificamos las dos condiciones que nos importan
            bool isAdmin = User.HasClaim("Permission", "can_manage_patients");
            bool patientProfileExists = await _context.Patients.AnyAsync(p => p.UserId == userId);

            // Pasamos ambas banderas a la vista
            ViewBag.IsAdminView = isAdmin;
            ViewBag.PatientProfileExists = patientProfileExists;

            return View();
        }
    }
}
