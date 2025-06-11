using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ProyectoFinal.Models.Base;
using ProyectoFinal.Models.Users;
using ProyectoFinal.Models.Roles;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Models.Users.Dto;
using Microsoft.AspNetCore.Identity;

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
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Username);

            if (user == null || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
                return Unauthorized(new { Message = "Credenciales incorrectas." });

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var grantedPermissions = new HashSet<string>();
            foreach (var userRole in user.UserRoles)
            {
                if (userRole.Role != null)
                {
                    claims.Add(new Claim(ClaimTypes.Role, userRole.Role.RoleName));
                    foreach (var rolePermission in userRole.Role.RolePermissions)
                    {
                        if (rolePermission.Permission != null && rolePermission.Permission.IsActive == true)
                        {
                            grantedPermissions.Add(rolePermission.Permission.PermissionName);
                        }
                    }
                }
            }

            foreach (var permissionName in grantedPermissions)
            {
                claims.Add(new Claim("Permission", permissionName));
            }

            var token = GenerateJwtToken(claims);
            var redisDb = _redis.GetDatabase();
            await redisDb.StringSetAsync($"JWT_{user.UserId}", token, TimeSpan.FromMinutes(int.Parse(_configuration["JwtSettings:ExpiresInMinutes"])));

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:ExpiresInMinutes"]))
            };
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            return Ok(new { Token = token, Message = "Inicio de sesión con éxito" });
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateUserDto createUserDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _context.Users.AnyAsync(u => u.Username == createUserDto.Username))
            {
                return Conflict(new { message = "El nombre de usuario ya está en uso." });
            }
            if (await _context.Users.AnyAsync(u => u.Email == createUserDto.Email))
            {
                return Conflict(new { message = "El correo electrónico ya está registrado." });
            }

            var user = new User
            {
                Username = createUserDto.Username,
                Email = createUserDto.Email,
                PasswordHash = _passwordHasher.HashPassword(null!, createUserDto.Password),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = 0
            };

            _context.Users.Add(user);

            var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Usuario Estándar");
            if (defaultRole != null)
            {
                var userRole = new UserRole
                {
                    User = user,
                    Role = defaultRole,
                    AssignedAt = DateTime.UtcNow,
                    AssignedBy = "System-Registration"
                };
                _context.UserRoles.Add(userRole);
            }
            else
            {
                Console.WriteLine("ADVERTENCIA: No se encontró el rol 'Usuario Estándar'. El usuario se creará sin rol.");
            }

            await _context.SaveChangesAsync();

            var responseDto = new CreatedUserDto
            {
                Username = user.Username,
                Email = user.Email
            };

            return Created($"/api/user/{user.UserId}", responseDto);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var redisDb = _redis.GetDatabase();
            await redisDb.KeyDeleteAsync($"JWT_{userIdClaim}");

            return Ok(new { Message = "Sesión cerrada correctamente." });
        }

        private string GenerateJwtToken(IEnumerable<Claim> claims)
        {
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