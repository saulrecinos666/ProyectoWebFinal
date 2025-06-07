using System.ComponentModel.DataAnnotations;
using ProyectoFinal.Models.Permissions.Dto; // Para ResponsePermissionDto
using ProyectoFinal.Models.Users.Dto; // Para ResponseUserDto

namespace ProyectoFinal.Models.Roles.Dto
{
    public class ResponseRoleDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public int? ModifiedBy { get; set; }

        // Propiedades adicionales para la UI o API
        public int NumberOfUsers { get; set; } // Cuántos usuarios tienen este rol
        public int NumberOfPermissions { get; set; } // Cuántos permisos tiene este rol
        public List<ResponsePermissionDto> Permissions { get; set; } = new List<ResponsePermissionDto>(); // Permisos específicos del rol
    }
}