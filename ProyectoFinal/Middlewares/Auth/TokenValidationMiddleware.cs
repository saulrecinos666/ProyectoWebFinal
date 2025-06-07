using StackExchange.Redis;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer; // Necesario para JwtBearerDefaults
using Microsoft.AspNetCore.Http; // Para StatusCodes y WriteAsync

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

            // 1. Excluir rutas que no requieren ninguna validación de token/sesión específica de este middleware.
            // Las rutas de login/registro (API y UI) deben ser accesibles.
            // Los archivos estáticos (CSS, JS, imágenes) también.
            if (path != null && (
                path.StartsWith("/api/auth/login") ||
                path.StartsWith("/api/auth/register") ||
                path.StartsWith("/swagger") ||
                path.StartsWith("/Home/Login") ||
                path.StartsWith("/Home/Register") ||
                path.StartsWith("/css/") ||
                path.StartsWith("/js/") ||
                path.StartsWith("/lib/") ||
                path.StartsWith("/favicon.ico") ||
                path.StartsWith("/_framework/") ||
                path.StartsWith("/_content/") ||
                path.Equals("/") ||
                path.Equals("/Home") ||
                path.Equals("/Home/Index") ||
                // Añade aquí cualquier otra ruta pública que no deba ser interceptada
                context.Response.StatusCode == StatusCodes.Status401Unauthorized // Si UseAuthentication ya falló y estableció 401
                ))
            {
                await _next(context); // Permitir que estas rutas continúen
                return;
            }

            // Si la solicitud ya fue autenticada por Cookies (ej. navegación de UI), continuar.
            // Para las APIs, esperamos JWT.
            if (context.User.Identity != null && context.User.Identity.IsAuthenticated &&
                context.User.Identity.AuthenticationType == CookieAuthenticationDefaults.AuthenticationScheme)
            {
                await _next(context);
                return;
            }

            // --- Lógica específica para validar JWTs en Redis para APIs y SignalR ---
            // Solo se ejecuta si la solicitud no fue autenticada por cookies y no es una ruta excluida.
            // Esto es principalmente para solicitudes a APIs y SignalR que esperan un JWT.

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
                // Si la solicitud es a una API o SignalR Hub y no hay token, es un 401.
                // Si es una ruta MVC, el middleware de Cookies ya debió redirigir si se requería Auth.
                if (path != null && (path.StartsWith("/api/") || path.StartsWith("/chathub")))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized: Token is missing for API/Hub request.");
                    return;
                }
                // Para rutas MVC no autenticadas, simplemente pasamos al siguiente middleware,
                // esperando que UseAuthorization o el controlador maneje la redirección/denegación.
                await _next(context);
                return;
            }

            // Si hay un token, y no fue autenticado por cookies, intentamos validar JWT y Redis.
            // El middleware AddJwtBearer ya autenticó el token si era válido.
            // Aquí solo nos aseguramos de que el token JWT (si se usó) no haya sido revocado en Redis.
            if (context.User.Identity != null && context.User.Identity.IsAuthenticated &&
                context.User.Identity.AuthenticationType == JwtBearerDefaults.AuthenticationScheme)
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized: Invalid JWT token claims.");
                    return;
                }

                var db = redis.GetDatabase();
                var storedToken = await db.StringGetAsync($"JWT_{userId}");

                if (string.IsNullOrEmpty(storedToken) || storedToken != token)
                {
                    // Token JWT es válido sintácticamente pero revocado o no coincide en Redis.
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized: Session ended or token revoked in Redis.");
                    return;
                }
            }
            // Si el token no es JWT Bearer autenticado (ej. es un JWT inválido que el UseAuthentication no autenticó)
            // o si la solicitud es una API/Hub y no se autenticó ni por Cookies ni por JWT Bearer válido.
            else if (path != null && (path.StartsWith("/api/") || path.StartsWith("/chathub")))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized: Invalid or unauthenticated JWT token for API/Hub request.");
                return;
            }

            // Si todo es válido o la ruta no requiere validación específica aquí, continuar.
            await _next(context);
        }
    }
}