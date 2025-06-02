using Microsoft.AspNetCore.Mvc;
using ProyectoFinal.Controllers.Base;

namespace ProyectoFinal.Controllers.Chats
{
    public class ChatController : BaseController 
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
