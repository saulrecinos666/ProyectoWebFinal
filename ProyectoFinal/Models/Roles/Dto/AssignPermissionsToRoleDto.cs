using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal.Models.Roles.Dto
{
    public class AssignPermissionsToRoleDto
    {
        [Required(ErrorMessage = "El ID del rol es requerido.")]
        public int RoleId { get; set; }

        public List<int> PermissionIds { get; set; } = new List<int>();
    }
}