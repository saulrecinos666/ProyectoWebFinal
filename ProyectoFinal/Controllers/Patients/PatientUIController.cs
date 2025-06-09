using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoFinal.Models.Base;
using System.Security.Claims;

namespace ProyectoFinal.Controllers.Patients
{
    // 1. Autorización general a nivel de controlador (todos deben estar logueados)
    [Authorize]
    [Route("PatientUI")]
    public class PatientUIController : Controller
    {
        // El constructor no necesita cambios
        private readonly DbCitasMedicasContext _context;

        public PatientUIController(DbCitasMedicasContext context)
        {
            _context = context;
        }

        // GET: /PatientUI o /PatientUI/Index
        // Esta es la ÚNICA acción que necesita este controlador para la interfaz.
        [HttpGet]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            // Verificamos si el usuario tiene el permiso para gestionar TODOS los pacientes.
            // Si lo tiene, es un administrador. Si no, asumimos que es un usuario normal
            // que necesita crear o ver su propio perfil.
            bool isAdminView = User.HasClaim("Permission", "can_manage_patients");

            // Pasamos esta información a la vista para que pueda mostrar u ocultar elementos.
            ViewBag.IsAdminView = isAdminView;

            return View();
        }

        // Las acciones [HttpGet("Create")] y [HttpPost("Create")] se han eliminado
        // porque su lógica ahora es manejada por la vista Index y el API Controller (PatientsController).
    }
}
