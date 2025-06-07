using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProyectoFinal.Controllers.Appointments
{
    // Aplicar la política a nivel de controlador para que todas sus acciones estén protegidas
    [Authorize(Policy = "CanManageAppointments")] // Requiere el permiso 'can_manage_appointments'
    public class AppointmentUIController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        // Si tuvieras otras acciones aquí, también estarían protegidas por esta política.
        // Por ejemplo, una acción para crear una cita:
        // public IActionResult Create() { return View(); }
    }
}