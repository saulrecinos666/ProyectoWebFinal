using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Controllers.Base;
using ProyectoFinal.Models.Base;
using ProyectoFinal.Models.Users;
using ProyectoFinal.Models.Users.Dto;

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
            var users = await _context.Users
                .Where(user => user.IsActive == true)
                .Select(user => new ResponseUserDto
                {
                    Username = user.Username,
                    Email = user.Email,
                    CreatedBy = user.CreatedBy,
                    CreatedAt = user.CreatedAt,
                    ModifiedBy = user.ModifiedBy, 
                    ModifiedAt = user.ModifiedAt
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) { return NotFound(); }

            var userDto = new ResponseUserDto
            {
                Username = user.Username,
                Email = user.Email,
                CreatedBy = user.CreatedBy,
                CreatedAt = user.CreatedAt,
                ModifiedBy = user.ModifiedBy,
                ModifiedAt = user.ModifiedAt
            };

            return Ok(userDto);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

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
            return CreatedAtAction(nameof(GetUserById), new { id = user.UserId }, new ResponseUserDto
            {
                Username = user.Username,
                Email = user.Email,
                CreatedBy = user.CreatedBy,
                CreatedAt = user.CreatedAt
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null) return NotFound("Usuario no encontrado");

            if (!string.IsNullOrWhiteSpace(updateUserDto.Password))
            {
                existingUser.PasswordHash = _passwordHasher.HashPassword(existingUser, updateUserDto.Password);
            }

            existingUser.Username = updateUserDto.Username;
            existingUser.Email = updateUserDto.Email;
            existingUser.ModifiedAt = DateTime.Now;
            existingUser.ModifiedBy = GetUserId();

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.IsActive = false;
            user.DeletedBy = GetUserId();
            user.DeletedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
