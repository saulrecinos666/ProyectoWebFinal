using ProyectoFinal.Models.Base;
using ProyectoFinal.Models.Patients;
using ProyectoFinal.Models.Roles;
using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal.Models.Users;

public partial class User : BaseEntity
{
    public int UserId { get; set; }

    [Required(ErrorMessage = "El Nombre de Usuario es requerido.")]
    [StringLength(50, ErrorMessage = "El Nombre de Usuario no puede exceder los 50 caracteres.")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "La Contraseña es requerida.")]
    public string PasswordHash { get; set; } = null!;

    [Required(ErrorMessage = "El Correo es requerido.")]
    [StringLength(100, ErrorMessage = "El Correo no puede exceder los 100 caracteres.")]
    public string Email { get; set; } = null!;

    public virtual ICollection<Patient> Patients { get; set; } = new List<Patient>();

    public virtual ICollection<UserLoginHistory> UserLoginHistories { get; set; } = new List<UserLoginHistory>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}