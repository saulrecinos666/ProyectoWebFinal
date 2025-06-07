using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal.Models.Roles.Dto
{
    public class AssignRolesToUserDto
    {
        [Required(ErrorMessage = "El ID del usuario es requerido.")]
        public int UserId { get; set; }

        // Lista de IDs de roles que deben estar asociados a este usuario
        public List<int> RoleIds { get; set; } = new List<int>();
    }
}