using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ProyectoFinal.Models.Base;
using ProyectoFinal.Models.Users;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ProyectoFinal.Controllers.Auth
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : Controller
    {
        private readonly DbCitasMedicasContext _context;
        private readonly IConnectionMultiplexer _redis;
        private readonly IConfiguration _configuration;

        public AuthController(DbCitasMedicasContext context, IConnectionMultiplexer redis, IConfiguration configuration)
        {
            _context = context;
            _redis = redis;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == request.Username);
            if (user == null || user.PasswordHash != request.Password)
                return Unauthorized(new
                {
                    Messagge = "Credenciales incorrectas."
                });

            var token = GenerateJwtToken(user);

            var redisDb = _redis.GetDatabase();
            await redisDb.StringSetAsync($"JWT_{user.UserId}", token, TimeSpan.FromMinutes(int.Parse(_configuration["JwtSettings:ExpiresInMinutes"])));

            return Ok(new
            {
                Token = token,
                Message = "Inicio de sesión con éxito"
            });
        }

        [HttpPost("logout")]
        [Authorize]
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
                    Message = "No se encontró sesión activa para este usuario."
                });

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
