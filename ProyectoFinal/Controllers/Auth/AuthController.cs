using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ProyectoFinal.Models.Base;
using ProyectoFinal.Models.Users;
using ProyectoFinal.Models.Roles; // ¡NUEVO! Para acceder a Roles, UserRoles, RolePermissions
using ProyectoFinal.Models.Permissions; // ¡NUEVO! Para acceder a Permissions
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore; // ¡NUEVO! Para usar .Include() y .ThenInclude()

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
            // Cargar usuario incluyendo sus roles y los permisos de esos roles
            var user = await _context.Users
                .Include(u => u.UserRoles) // Incluye las asignaciones de roles del usuario
                    .ThenInclude(ur => ur.Role) // Luego incluye el objeto Role para cada asignación
                        .ThenInclude(r => r.RolePermissions) // Luego incluye las asignaciones de permisos para cada rol
                            .ThenInclude(rp => rp.Permission) // Finalmente, incluye el objeto Permission para cada asignación
                .Include(u => u.UserPermissions) // También incluye permisos directos por si los usas
                    .ThenInclude(up => up.Permission)
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
                return Unauthorized(new
                {
                    Message = "Credenciales incorrectas."
                });

            // --- Construir Claims para la cookie de autenticación ---
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()), // ID del usuario
                new Claim(ClaimTypes.Name, user.Username), // Nombre de usuario (username)
                new Claim(ClaimTypes.Email, user.Email) // Email del usuario
            };

            // Añadir roles como Claims
            foreach (var userRole in user.UserRoles)
            {
                if (userRole.Role != null)
                {
                    claims.Add(new Claim(ClaimTypes.Role, userRole.Role.RoleName));
                }
            }

            // Añadir permisos como Claims (tanto de roles como directos si UserPermissions se usa)
            var grantedPermissions = new HashSet<string>(); // Usar HashSet para evitar duplicados

            // Permisos a través de roles
            foreach (var userRole in user.UserRoles)
            {
                if (userRole.Role != null)
                {
                    foreach (var rolePermission in userRole.Role.RolePermissions)
                    {
                        if (rolePermission.Permission != null && rolePermission.Permission.IsActive == true)
                        {
                            grantedPermissions.Add(rolePermission.Permission.PermissionName);
                        }
                    }
                }
            }

            // Permisos directos al usuario (si la tabla UserPermissions se usa para esto)
            foreach (var userPermission in user.UserPermissions)
            {
                if (userPermission.Permission != null && userPermission.Permission.IsActive == true)
                {
                    grantedPermissions.Add(userPermission.Permission.PermissionName);
                }
            }

            // Añadir cada permiso como un Claim separado. Usaremos un tipo de claim personalizado, por ejemplo "Permission".
            foreach (var permissionName in grantedPermissions)
            {
                claims.Add(new Claim("Permission", permissionName)); // Tipo de claim "Permission"
            }


            // Generar el JWT (esto es independiente de la cookie, pero lo sigues usando para APIs/SignalR)
            var token = GenerateJwtToken(user, claims); // Pasa los claims al generador de JWT

            // Almacenar el JWT en Redis
            var redisDb = _redis.GetDatabase();
            await redisDb.StringSetAsync($"JWT_{user.UserId}", token, TimeSpan.FromMinutes(int.Parse(_configuration["JwtSettings:ExpiresInMinutes"])));

            // Iniciar sesión basada en Cookies
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Si quieres que la cookie persista (para "Recordarme")
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:ExpiresInMinutes"]))
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

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
            // No es un error si el token de Redis ya no existe, solo significa que ya no estaba en la caché
            // if (!deleted) return NotFound(new { Message = "No se encontró sesión activa para este usuario en Redis." });

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Ok(new
            {
                Message = "Sesión cerrada correctamente."
            });
        }

        // Modificado para aceptar una lista de Claims
        private string GenerateJwtToken(User user, List<Claim> userClaims)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            // Añade los claims adicionales (roles y permisos) al JWT también
            claims.AddRange(userClaims.Where(c =>
                c.Type != ClaimTypes.NameIdentifier &&
                c.Type != ClaimTypes.Name &&
                c.Type != ClaimTypes.Email
            ));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims, // Usa los claims actualizados aquí
                expires: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:ExpiresInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}