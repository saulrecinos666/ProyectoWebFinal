using ProyectoFinal.Models.Permissions.Dto; 

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
        public int NumberOfUsers { get; set; } 
        public int NumberOfPermissions { get; set; }
        public List<ResponsePermissionDto> Permissions { get; set; } = new List<ResponsePermissionDto>();
    }
}