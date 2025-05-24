using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace ProyectoFinal.Models.Users.Dto
{
    public class UpdateUserDto
    {
        [StringLength(50, ErrorMessage = "El Nombre de Usuario no puede exceder los 50 caracteres.")]
        public string? Username { get; set; } = null!;

        public string? Password { get; set; } = null!;

        [StringLength(100, ErrorMessage = "El Correo no puede exceder los 100 caracteres.")]
        public string? Email { get; set; } = null!;
    }
}
