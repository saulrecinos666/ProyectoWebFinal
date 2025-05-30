using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace ProyectoFinal.Models.Users.Dto
{
    public class UpdateUserDto
    {
        [StringLength(50, ErrorMessage = "El Nombre de Usuario no puede exceder los 50 caracteres.")]
        public string? Username { get; set; } = null!;

        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$",
        ErrorMessage = "La contraseña debe tener al menos una mayúscula, una minúscula, un número y un carácter especial")]
        public string? Password { get; set; } = null!;

        [StringLength(100, ErrorMessage = "El Correo no puede exceder los 100 caracteres.")]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; } = null!;
    }
}
