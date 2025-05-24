using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ProyectoFinal.Controllers.Base
{
    public class BaseController : Controller
    {
        protected int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return 0;
        }
    }
}
