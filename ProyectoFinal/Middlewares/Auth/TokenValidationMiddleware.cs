using StackExchange.Redis;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication; // Necesario para HttpContext.AuthenticateAsync
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer; // Necesario para CookieAuthenticationDefaults

namespace ProyectoFinal.Middlewares.Auth
{
    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IConnectionMultiplexer redis)
        {
            var path = context.Request.Path.Value;

            // 1. Excluir rutas que no requieren validación de token (login, registro, swagger, archivos estáticos)
            // Estas rutas serán manejadas por el middleware de Cookies o son públicas.
            if (path != null && (
                path.StartsWith("/api/auth/login") ||       // API de Login
                path.StartsWith("/api/auth/register") ||     // API de Registro
                path.StartsWith("/swagger") ||               // Rutas de Swagger
                path.StartsWith("/Home/Login") ||            // Vista de Login
                path.StartsWith("/Home/Register") ||         // Vista de Registro
                path.StartsWith("/css/") ||                  // Archivos CSS
                path.StartsWith("/js/") ||                   // Archivos JavaScript
                path.StartsWith("/lib/") ||                  // Librerías (Bootstrap, jQuery, etc.)
                path.StartsWith("/favicon.ico") ||           // Icono de la página
                path.StartsWith("/_framework/") ||           // Para Blazor (si lo usas)
                path.StartsWith("/_content/")   ||             // Para CSS aislado o librerías estáticas
                path.Equals("/") ||                 // Permite la ruta raíz
                path.Equals("/Home") ||             // Permite /Home
                path.Equals("/Home/Index")
                ))
            {
                await _next(context); // Permitir que estas rutas continúen sin validación de token/redis
                return;
            }

            // AHORA: Si el usuario ya está autenticado por cookies, no necesitamos hacer nada más aquí
            // para las vistas, ya que el middleware de Cookies ya lo manejó.
            // Este middleware se centrará en las solicitudes que esperan un JWT (APIs y SignalR).
            if (context.User.Identity != null && context.User.Identity.IsAuthenticated &&
                context.User.Identity.AuthenticationType == CookieAuthenticationDefaults.AuthenticationScheme)
            {
                await _next(context); // El usuario ya está autenticado por cookie, continuar.
                return;
            }


            // --- Lógica de validación de JWT y Redis para APIs y SignalR ---
            string token = null;
            if (context.Request.Headers.ContainsKey("Authorization"))
            {
                token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            }
            else if (path != null && path.StartsWith("/chathub") && context.Request.Query.ContainsKey("access_token"))
            {
                token = context.Request.Query["access_token"].ToString();
            }

            // Si no hay token en una solicitud que lo esperaría (API/Hub)
            if (string.IsNullOrEmpty(token))
            {
                // Si la solicitud es una navegación de página y no hay token (y no está autenticado por cookie)
                // el middleware de Cookies ya debería haber redirigido a /Home/Login.
                // Si llegamos aquí para una API/Hub sin token, es un 401.
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized: Token is missing for API/Hub request.");
                return;
            }

            // Si hay un token, intenta autenticarlo con JWT Bearer (si no se hizo ya)
            // Esto es crucial si el middleware se ejecuta antes de UseAuthentication para algunas rutas,
            // o si quieres re-autenticar. Sin embargo, con el orden propuesto, UseAuthentication ya lo hizo.
            // Aquí, nos enfocamos en la validación de Redis después de que JWT Bearer haya validado el token.

            // Asegúrate de que el usuario esté autenticado por JWT Bearer antes de consultar Redis.
            // El `context.User` ya debería estar poblado por `app.UseAuthentication()` si el token es válido.
            if (context.User.Identity == null || !context.User.Identity.IsAuthenticated ||
                context.User.Identity.AuthenticationType != JwtBearerDefaults.AuthenticationScheme)
            {
                // Si no está autenticado por JWT Bearer (o no es un esquema JWT),
                // significa que el token es inválido o no fue procesado por JWT Bearer.
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized: Invalid JWT token or not authenticated by JWT.");
                return;
            }

            // Si el usuario está autenticado por JWT Bearer, procede a validar en Redis.
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Token inválido: Usuario no identificado en claims.");
                return;
            }

            var db = redis.GetDatabase();
            var storedToken = await db.StringGetAsync($"JWT_{userId}");

            if (string.IsNullOrEmpty(storedToken) || storedToken != token)
            {
                // Token JWT válido, pero revocado o no coincide en Redis.
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized: Session ended or token revoked in Redis.");
                return;
            }

            // Si todo es válido, continuar con la solicitud.
            await _next(context);
        }
    }
}