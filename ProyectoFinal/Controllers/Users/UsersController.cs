using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Controllers.Base;
using ProyectoFinal.Models.Base;
using ProyectoFinal.Models.Roles;
using ProyectoFinal.Models.Users;
using ProyectoFinal.Models.Users.Dto;
using System.Security.Claims;

namespace ProyectoFinal.Controllers.Users
{
    [ApiController]
    [Route("api/user")]
    [Authorize]
    public class UsersController : BaseController
    {
        private readonly DbCitasMedicasContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UsersController(DbCitasMedicasContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized("Sesión inválida.");
            }

            var query = _context.Users.AsQueryable();

            // Si el usuario NO tiene permiso para gestionar todos los usurios...
            if (!User.HasClaim("Permission", "can_manage_users"))
            {
                query = query.Where(a => a.UserId == userId);
            }

            var users = await query
                .Select(user => new ResponseUserDto
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    IsActive = user.IsActive,
                    CreatedBy = user.CreatedBy,
                    CreatedAt = user.CreatedAt,
                    ModifiedBy = user.ModifiedBy, 
                    ModifiedAt = user.ModifiedAt
                })
                .ToListAsync();

            if (users is null) 
            {
                return NotFound("No hay registros");
            }

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null) return NotFound("No hay registros");

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out var currentUserId);

            // Verificamos si es admin O es el usuario.
            if (!User.HasClaim("Permission", "can_manage_users") && user.UserId != currentUserId)
            {
                return Forbid();
            }

            var userDto = new ResponseUserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                IsActive = user.IsActive,
                CreatedBy = user.CreatedBy,
                CreatedAt = user.CreatedAt,
                ModifiedBy = user.ModifiedBy,
                ModifiedAt = user.ModifiedAt
            };

            return Ok(userDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _context.Users.AnyAsync(u => u.Username == createUserDto.Username))
            {
                return Conflict(new { Message = "El nombre de usuario ya está en uso." });
            }
            if (await _context.Users.AnyAsync(u => u.Email == createUserDto.Email))
            {
                return Conflict(new { Message = "El correo electrónico ya está registrado." });
            }

            var user = new User
            {
                Username = createUserDto.Username,
                Email = createUserDto.Email,
                PasswordHash = _passwordHasher.HashPassword(null!, createUserDto.Password),
                IsActive = true,
                CreatedBy = GetUserId(),
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            try
            {
                var defaultRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.RoleName == "Usuario Estándar");

                if (defaultRole != null)
                {
                    var userRole = new UserRole
                    {
                        UserId = user.UserId, 
                        RoleId = defaultRole.RoleId, 
                        AssignedAt = DateTime.UtcNow,
                        AssignedBy = user.UserId.ToString() 
                    };

                    _context.UserRoles.Add(userRole);
                    await _context.SaveChangesAsync(); 
                }
                else
                {
                    Console.WriteLine("ADVERTENCIA: No se encontró el rol por defecto 'Usuario Estándar'. El nuevo usuario fue creado sin rol.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR al asignar rol por defecto al usuario {user.Username}: {ex.Message}");
            }

            var responseDto = new CreatedUserDto
            {
                Username = user.Username,
                Email = user.Email
            };

            return CreatedAtAction(nameof(GetUserById), new { id = user.UserId }, responseDto);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (existingUser == null) return NotFound("Usuario no encontrado");

            var currentUserId = GetUserId();
            if (!User.HasClaim("Permission", "can_manage_users") && existingUser.UserId != currentUserId)
            {
                return Forbid();
            }

            if (!string.IsNullOrWhiteSpace(updateUserDto.Password))
            {
                existingUser.PasswordHash = _passwordHasher.HashPassword(existingUser, updateUserDto.Password);
            }

            if (!string.IsNullOrWhiteSpace(updateUserDto.Username))
            {
                existingUser.Username = updateUserDto.Username;
            }

            if (!string.IsNullOrWhiteSpace(updateUserDto.Email))
            {
                existingUser.Email = updateUserDto.Email;
            }

            existingUser.ModifiedAt = DateTime.Now;
            existingUser.ModifiedBy = GetUserId();

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id}/desactivate")]
        public async Task<IActionResult> DesactivateUser(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null) return NotFound();

            var currentUserId = GetUserId();
            if (!User.HasClaim("Permission", "can_manage_users") && user.UserId != currentUserId)
            {
                return Forbid();
            }

            user.IsActive = false;
            user.DeletedBy = GetUserId();
            user.DeletedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("byUsername")]
        public async Task<IActionResult> GetUserByUsername([FromQuery] string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return BadRequest(new { message = "El nombre de usuario no puede estar vacío." });
            }

            var user = await _context.Users
                                     .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return NotFound(new { message = $"Usuario '{username}' no encontrado." });
            }

            return Ok(new { UserId = user.UserId, Username = user.Username });
        }

        [HttpGet("without-patient")]
        public async Task<IActionResult> GetUsersWithoutPatientProfile()
        {
            var usersWithPatient = _context.Patients.Select(p => p.UserId);

            var usersWithoutPatient = await _context.Users
                .Where(u => u.IsActive && !usersWithPatient.Contains(u.UserId))
                .Select(u => new { u.UserId, u.Username, u.Email })
                .ToListAsync();

            return Ok(usersWithoutPatient);
        }

    }
}
