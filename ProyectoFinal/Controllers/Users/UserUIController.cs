using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ProyectoFinal.Controllers.Users
{
    public class UserUIController : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized("Sesión inválida.");
            }

            bool isAdmin = User.HasClaim("Permission", "can_manage_users");

            ViewBag.IsAdminView = isAdmin;

            return View();
        }
    }
}
