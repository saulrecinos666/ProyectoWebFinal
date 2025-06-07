using ProyectoFinal.Models.Users; 

namespace ProyectoFinal.Models.Roles
{
    public class UserRole
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public DateTime? AssignedAt { get; set; }
        public string? AssignedBy { get; set; }
        public virtual User User { get; set; } = null!;
        public virtual Role Role { get; set; } = null!;
    }
}