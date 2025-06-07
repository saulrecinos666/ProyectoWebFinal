using ProyectoFinal.Models.Permissions; 

namespace ProyectoFinal.Models.Roles
{
    public class RolePermission
    {
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
        public DateTime? GrantedAt { get; set; }
        public string? GrantedBy { get; set; }
        public virtual Role Role { get; set; } = null!;
        public virtual Permission Permission { get; set; } = null!;
    }
}