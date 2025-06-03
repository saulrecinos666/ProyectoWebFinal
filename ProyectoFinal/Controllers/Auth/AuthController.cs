using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ProyectoFinal.Models.Base;
using ProyectoFinal.Models.Users;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication; // ¡NUEVO USING!
using Microsoft.AspNetCore.Authentication.Cookies; // ¡NUEVO USING!

namespace ProyectoFinal.Controllers.Auth
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : Controller
    {
        private readonly DbCitasMedicasContext _context;
        private readonly IConnectionMultiplexer _redis;
        private readonly IConfiguration _configuration;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AuthController(
            DbCitasMedicasContext context,
            IConnectionMultiplexer redis,
            IConfiguration configuration,
            IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _redis = redis;
            _configuration = configuration;
            _passwordHasher = passwordHasher;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == request.Username);
            if (user == null || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
                return Unauthorized(new
                {
                    Message = "Credenciales incorrectas."
                });

            var token = GenerateJwtToken(user); // Genera el JWT

            var redisDb = _redis.GetDatabase();
            await redisDb.StringSetAsync($"JWT_{user.UserId}", token, TimeSpan.FromMinutes(int.Parse(_configuration["JwtSettings:ExpiresInMinutes"])));

            // *** AÑADIR ESTO: INICIAR SESIÓN BASADA EN COOKIES ***
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
                // Puedes añadir más claims aquí si los necesitas para la cookie
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Si quieres que la cookie persista entre sesiones del navegador (para "Recordarme")
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:ExpiresInMinutes"]))
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
            // **********************************************************

            return Ok(new
            {
                Token = token,
                Message = "Inicio de sesión con éxito"
            });
        }

        [HttpPost("logout")]
        [Authorize] // Esta acción requiere que el usuario esté autenticado (por cookie o JWT) para ejecutar el logout.
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized();

            int userId = int.Parse(userIdClaim);
            var redisDb = _redis.GetDatabase();

            bool deleted = await redisDb.KeyDeleteAsync($"JWT_{userId}");
            if (!deleted)
                return NotFound(new
                {
                    Message = "No se encontró sesión activa para este usuario en Redis."
                });

            // *** AÑADIR ESTO: CERRAR SESIÓN BASADA EN COOKIES ***
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // **************************************************

            return Ok(new
            {
                Message = "Sesión cerrada correctamente."
            });
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:ExpiresInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}