using ProyectoFinal.Models.Permissions;

namespace ProyectoFinal.Models.Users;

public partial class UserPermission
{
    public int UserId { get; set; }

    public int PermissionId { get; set; }

    public DateTime? GrantedAt { get; set; }

    public string? GrantedBy { get; set; }

    public virtual Permission Permission { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
