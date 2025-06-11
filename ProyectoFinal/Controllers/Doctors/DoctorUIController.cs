using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Models.Base;
using System.Security.Claims;

namespace ProyectoFinal.Controllers.Doctors
{
    [Authorize(Policy = "CanAccessDoctors")]
    [Route("DoctorUI")]
    public class DoctorUIController : Controller
    {
        private readonly DbCitasMedicasContext _context;

        // Volvemos a inyectar el DbContext porque lo necesitamos para la verificación
        public DoctorUIController(DbCitasMedicasContext context)
        {
            _context = context;
        }

        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized("Sesión inválida.");
            }

            // Verificamos las dos condiciones que nos importan
            bool isAdmin = User.HasClaim("Permission", "can_manage_doctors");
            bool doctorProfileExists = true;

            if (isAdmin == false) 
            {
                doctorProfileExists = await _context.Doctors.AnyAsync(p => p.UserId == userId);
            }

            // Pasamos ambas banderas a la vista
            ViewBag.IsAdminView = isAdmin;
            ViewBag.DoctorProfileExists = doctorProfileExists;

            return View();
        }
    }
}
