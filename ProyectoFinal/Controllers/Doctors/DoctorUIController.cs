using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProyectoFinal.Controllers.Doctors
{
    public class DoctorUIController : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
    }
}
