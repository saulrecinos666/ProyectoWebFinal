using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProyectoFinal.Controllers.Users
{
    public class UserUIController : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
    }
}
