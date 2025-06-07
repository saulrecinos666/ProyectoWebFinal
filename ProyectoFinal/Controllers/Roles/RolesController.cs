using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoFinal.Models.Roles.Dto;
using ProyectoFinal.Services;
using System.Collections.Generic; // Para List<int>
using System.Threading.Tasks;

namespace ProyectoFinal.Controllers.Api
{
    [ApiController]
    [Route("api")] // <-- CAMBIO 1: Hacemos la ruta base más flexible
    [Authorize(Policy = "CanManageRoles")]
    public class RolesController : ControllerBase
    {
        private readonly RoleService _roleService;

        public RolesController(RoleService roleService)
        {
            _roleService = roleService;
        }

        // --- Endpoints de Roles existentes ---

        [HttpGet("roles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(roles);
        }

        [HttpGet("roles/{id}")]
        public async Task<IActionResult> GetRoleById(int id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null) return NotFound();
            return Ok(role);
        }

        [HttpPost("roles")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var newRoleDto = await _roleService.CreateRoleAsync(dto);
                return CreatedAtAction(nameof(GetRoleById), new { id = newRoleDto.RoleId }, newRoleDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Ocurrió un error inesperado al crear el rol." });
            }
        }

        [HttpPut("roles/{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!await _roleService.UpdateRoleAsync(id, dto)) return NotFound();
            return NoContent();
        }

        [HttpDelete("roles/{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            if (!await _roleService.DeleteRoleAsync(id)) return NotFound();
            return NoContent();
        }

        [HttpPost("roles/{roleId}/permissions")]
        public async Task<IActionResult> UpdateRolePermissions(int roleId, [FromBody] AssignPermissionsToRoleDto dto)
        {
            if (roleId != dto.RoleId) return BadRequest("IDs no coinciden.");
            if (!await _roleService.UpdateRolePermissionsAsync(roleId, dto.PermissionIds)) return NotFound();
            return NoContent();
        }

        [HttpGet("roles/permissions")]
        public async Task<IActionResult> GetAllPermissions()
        {
            var permissions = await _roleService.GetAllActivePermissionsAsync();
            return Ok(permissions);
        }

        // --- ¡NUEVOS ENDPOINTS PARA EL MODAL DE ASIGNACIÓN! ---

        // GET: /api/users
        // Devuelve todos los usuarios para llenar el modal.
        [HttpGet("users")]
        public async Task<IActionResult> GetUsersForAssign()
        {
            // Este método debe estar en tu RoleService (o un UserService)
            var users = await _roleService.GetAllUsersForRoleManagementAsync();
            return Ok(users);
        }

        // GET: /api/roles/{roleId}/users
        // Devuelve los IDs de los usuarios que ya tienen asignado un rol.
        [HttpGet("roles/{roleId}/users")]
        public async Task<IActionResult> GetAssignedUsersForRole(int roleId)
        {
            var userIds = await _roleService.GetAssignedUserIdsForRoleAsync(roleId);
            return Ok(userIds);
        }

        // POST: /api/roles/{roleId}/assign-users
        // Recibe una lista de IDs de usuario y los asigna a un rol, reemplazando los anteriores.
        [HttpPost("roles/{roleId}/assign-users")]
        public async Task<IActionResult> AssignUsersToRole(int roleId, [FromBody] List<int> userIds)
        {
            var result = await _roleService.AssignUsersToRoleAsync(roleId, userIds);
            if (!result)
            {
                return BadRequest("No se pudieron actualizar las asignaciones. El rol podría no existir.");
            }
            return Ok(new { message = "Asignaciones actualizadas correctamente." });
        }
    }
}
