using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProyectoFinal.Controllers.Patients
{
    public class PatientUIController : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
    }
}
