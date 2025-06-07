using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Models.Base;
using ProyectoFinal.Models.Permissions.Dto;
using ProyectoFinal.Models.Roles;
using ProyectoFinal.Models.Roles.Dto;
using ProyectoFinal.Models.Users.Dto;
using System.Security.Claims;

namespace ProyectoFinal.Services
{
    public class RoleService
    {
        private readonly DbCitasMedicasContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RoleService(DbCitasMedicasContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        // --- Operaciones CRUD para Roles ---
        public async Task<List<ResponseRoleDto>> GetAllRolesAsync()
        {
            return await _context.Roles
                .Where(r => r.IsActive)
                .Select(r => new ResponseRoleDto
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName,
                    Description = r.Description,
                    IsActive = r.IsActive,
                    NumberOfUsers = r.UserRoles.Count(ur => ur.User.IsActive),
                    NumberOfPermissions = r.RolePermissions.Count()
                })
                .ToListAsync();
        }

        public async Task<ResponseRoleDto?> GetRoleByIdAsync(int roleId)
        {
            return await _context.Roles
               .Where(r => r.RoleId == roleId && r.IsActive)
               .Select(r => new ResponseRoleDto
               {
                   RoleId = r.RoleId,
                   RoleName = r.RoleName,
                   Description = r.Description,
                   IsActive = r.IsActive,
                   Permissions = r.RolePermissions
                                   .Where(rp => rp.Permission.IsActive == true)
                                   .Select(rp => new ResponsePermissionDto
                                   {
                                       PermissionId = rp.Permission.PermissionId,
                                       PermissionName = rp.Permission.PermissionName,
                                       Description = rp.Permission.Description
                                   }).ToList()
               })
               .FirstOrDefaultAsync();
        }

        public async Task<ResponseRoleDto> CreateRoleAsync(CreateRoleDto dto)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
            {
                throw new InvalidOperationException("No se pudo identificar al usuario autenticado para realizar esta acción.");
            }

            if (await _context.Roles.AnyAsync(r => r.RoleName == dto.RoleName))
            {
                throw new InvalidOperationException($"Ya existe un rol con el nombre '{dto.RoleName}'.");
            }

            var newRole = new Role
            {
                RoleName = dto.RoleName,
                Description = dto.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId
            };

            _context.Roles.Add(newRole);
            await _context.SaveChangesAsync(); // Guardamos aquí para obtener el ID

            if (dto.PermissionIds != null && dto.PermissionIds.Any())
            {
                await UpdateRolePermissionsAsync(newRole.RoleId, dto.PermissionIds);
            }

            // Devolvemos un DTO limpio en lugar de la entidad completa
            return new ResponseRoleDto
            {
                RoleId = newRole.RoleId,
                RoleName = newRole.RoleName,
                Description = newRole.Description,
                IsActive = newRole.IsActive,
                NumberOfPermissions = dto.PermissionIds?.Count ?? 0,
                NumberOfUsers = 0 // Un rol nuevo siempre tiene 0 usuarios
            };
        }

        public async Task<bool> UpdateRoleAsync(int roleId, UpdateRoleDto dto)
        {
            var role = await _context.Roles.FindAsync(roleId);
            if (role == null) return false;
            role.RoleName = dto.RoleName;
            role.Description = dto.Description;
            role.IsActive = dto.IsActive;
            role.ModifiedAt = DateTime.UtcNow;
            role.ModifiedBy = GetCurrentUserId();
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteRoleAsync(int roleId)
        {
            var role = await _context.Roles.FindAsync(roleId);
            if (role == null) return false;
            role.IsActive = false;
            role.DeletedAt = DateTime.UtcNow;
            role.DeletedBy = GetCurrentUserId();
            await _context.SaveChangesAsync();
            return true;
        }

        // --- Operaciones de Asignación ---

        // --- ¡MÉTODO CORREGIDO CON VALIDACIÓN! ---
        public async Task<bool> UpdateRolePermissionsAsync(int roleId, List<int> newPermissionIds)
        {
            var role = await _context.Roles
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.RoleId == roleId);

            if (role == null) return false;

            var currentUserIdStr = GetCurrentUserId().ToString();

            // --- NUEVA VALIDACIÓN (se mantiene) ---
            // 1. Obtenemos los IDs de los permisos que SÍ existen y están activos.
            var allValidPermissionIds = await _context.Permissions
                .Where(p => p.IsActive == true)
                .Select(p => p.PermissionId)
                .ToListAsync();

            // 2. Usamos solo los IDs que nos llegaron Y que son válidos.
            var validNewPermissionIds = newPermissionIds?.Intersect(allValidPermissionIds).ToList() ?? new List<int>();

            var currentPermissionIds = role.RolePermissions.Select(rp => rp.PermissionId).ToList();

            // 3. Permisos a AÑADIR (los que están en la nueva lista pero no en la actual)
            var permissionsToAddIds = validNewPermissionIds.Except(currentPermissionIds).ToList();
            foreach (var permId in permissionsToAddIds)
            {
                _context.RolePermissions.Add(new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permId,
                    GrantedAt = DateTime.UtcNow,
                    GrantedBy = currentUserIdStr
                });
            }

            // 4. Permisos a REMOVER (los que están en la lista actual pero no en la nueva)
            var permissionsToRemoveIds = currentPermissionIds.Except(validNewPermissionIds).ToList();
            if (permissionsToRemoveIds.Any())
            {
                var permissionsToRemove = role.RolePermissions
                    .Where(rp => permissionsToRemoveIds.Contains(rp.PermissionId))
                    .ToList();
                _context.RolePermissions.RemoveRange(permissionsToRemove);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<int>> GetAssignedUserIdsForRoleAsync(int roleId)
        {
            return await _context.UserRoles
                .Where(ur => ur.RoleId == roleId)
                .Select(ur => ur.UserId)
                .ToListAsync();
        }

        public async Task<bool> AssignUsersToRoleAsync(int roleId, List<int> userIds)
        {
            var role = await _context.Roles.FindAsync(roleId);
            if (role == null) return false;
            var currentUserIdStr = GetCurrentUserId().ToString();
            var existingAssignments = await _context.UserRoles.Where(ur => ur.RoleId == roleId).ToListAsync();
            _context.UserRoles.RemoveRange(existingAssignments);
            if (userIds != null && userIds.Any())
            {
                var newAssignments = userIds.Select(userId => new UserRole
                {
                    RoleId = roleId,
                    UserId = userId,
                    AssignedAt = DateTime.UtcNow,
                    AssignedBy = currentUserIdStr
                });
                await _context.UserRoles.AddRangeAsync(newAssignments);
            }
            await _context.SaveChangesAsync();
            return true;
        }


        // --- Ayudantes ---
        public async Task<List<ResponsePermissionDto>> GetAllActivePermissionsAsync()
        {
            return await _context.Permissions
                .Where(p => p.IsActive == true)
                .Select(p => new ResponsePermissionDto
                {
                    PermissionId = p.PermissionId,
                    PermissionName = p.PermissionName,
                    Description = p.Description
                })
                .ToListAsync();
        }

        public async Task<List<ResponseUserDto>> GetAllUsersForRoleManagementAsync()
        {
            return await _context.Users
               .Where(u => u.IsActive)
               .Select(u => new ResponseUserDto
               {
                   UserId = u.UserId,
                   Username = u.Username,
                   Email = u.Email
               })
               .OrderBy(u => u.Username)
               .ToListAsync();
        }
    }
}
