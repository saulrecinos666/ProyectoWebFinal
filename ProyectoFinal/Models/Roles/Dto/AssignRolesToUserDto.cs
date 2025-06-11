using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal.Models.Roles.Dto
{
    public class AssignRolesToUserDto
    {
        [Required(ErrorMessage = "El ID del usuario es requerido.")]
        public int UserId { get; set; }

        public List<int> RoleIds { get; set; } = new List<int>();
    }
}