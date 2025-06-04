using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal.Models.Users.Dto
{
    public class CreatedUserDto
    {
        [Required(ErrorMessage = "El Nombre de Usuario es requerido.")]
        [StringLength(50, ErrorMessage = "El Nombre de Usuario no puede exceder los 50 caracteres.")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "El Correo es requerido.")]
        [StringLength(100, ErrorMessage = "El Correo no puede exceder los 100 caracteres.")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = null!;
    }
}
