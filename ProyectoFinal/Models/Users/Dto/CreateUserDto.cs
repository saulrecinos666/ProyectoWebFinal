using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal.Models.Users.Dto
{
    public class CreateUserDto
    {
        [Required(ErrorMessage = "El Nombre de Usuario es requerido.")]
        [StringLength(50, ErrorMessage = "El Nombre de Usuario no puede exceder los 50 caracteres.")]
        public string Username { get; set; } = null!;
        
        [Required(ErrorMessage = "La Contraseña es requerida.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$",
        ErrorMessage = "La contraseña debe tener al menos una mayúscula, una minúscula, un número y un carácter especial")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "El Correo es requerido.")]
        [StringLength(100, ErrorMessage = "El Correo no puede exceder los 100 caracteres.")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = null!;
    }
}
