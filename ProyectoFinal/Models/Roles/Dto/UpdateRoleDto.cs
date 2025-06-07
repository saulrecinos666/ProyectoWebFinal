using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal.Models.Roles.Dto
{
    public class UpdateRoleDto
    {
        [Required(ErrorMessage = "El nombre del rol es requerido.")]
        [StringLength(50, ErrorMessage = "El nombre del rol no puede exceder los 50 caracteres.")]
        public string RoleName { get; set; } = null!;

        [StringLength(200, ErrorMessage = "La descripción no puede exceder los 200 caracteres.")]
        public string? Description { get; set; }

        public bool IsActive { get; set; }
    }
}