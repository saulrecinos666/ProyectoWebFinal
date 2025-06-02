using StackExchange.Redis;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

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

            string token = null;
            if (context.Request.Headers.ContainsKey("Authorization"))
            {
                token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            }
            else if (path != null && path.StartsWith("/chathub") && context.Request.Query.ContainsKey("access_token"))
            {
                token = context.Request.Query["access_token"].ToString();
            }

            if (string.IsNullOrEmpty(token))
            {
                await _next(context);
                return;
            }

            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Token inválido o usuario no identificado.");
                return;
            }

            var db = redis.GetDatabase();
            var storedToken = await db.StringGetAsync($"JWT_{userId}");

            if (string.IsNullOrEmpty(storedToken) || storedToken != token)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Token inválido o sesión finalizada.");
                return;
            }

            await _next(context);
        }
    }
}