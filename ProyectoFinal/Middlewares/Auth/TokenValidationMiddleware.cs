using StackExchange.Redis;
using System.Security.Claims;

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

            if (path != null && (
                path.StartsWith("/api/auth/login") ||
                path.StartsWith("/api/auth/register") ||
                path.StartsWith("/swagger")))
            {
                await _next(context);
                return;
            }

            var user = context.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                var db = redis.GetDatabase();
                var storedToken = await db.StringGetAsync($"JWT_{userId}");

                if (string.IsNullOrEmpty(storedToken) || storedToken != token)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Token inválido o sesión finalizada.");
                    return;
                }
            }

            await _next(context);
        }
    }
}
