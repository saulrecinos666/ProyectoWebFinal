using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ProyectoFinal.Hubs
{
    public class ChatHub : Hub
    {

        public async Task SendMessage(string message)
        {
            try
            {
                var userIdClaim = Context.User.FindFirst(ClaimTypes.NameIdentifier); 
                var usernameClaim = Context.User.FindFirst(ClaimTypes.Name); 
                var emailClaim = Context.User.FindFirst(ClaimTypes.Email); 

                string authenticatedUserId = userIdClaim?.Value ?? "Desconocido";
                string authenticatedUsername = usernameClaim?.Value ?? "Anónimo";
                string authenticatedEmail = emailClaim?.Value ?? "sinemail@ejemplo.com";

                await Clients.All.SendAsync("ReceiveMessage", authenticatedUsername, message);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}