using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProyectoFinal.Controllers.Institutions
{
    public class InstitutionUIController : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
    }
}
