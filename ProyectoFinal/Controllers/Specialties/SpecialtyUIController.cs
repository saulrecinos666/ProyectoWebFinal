using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProyectoFinal.Controllers.Specialties
{
    public class SpecialtyUIController : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
    }
}
