using ProyectoFinal.Models.Users;
using ProyectoFinal.Models.Roles; // ¡NUEVO! Agrega este using para RolePermission

namespace ProyectoFinal.Models.Permissions;

public partial class Permission
{
    public int PermissionId { get; set; }

    public string PermissionName { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}