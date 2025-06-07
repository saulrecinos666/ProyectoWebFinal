using ProyectoFinal.Models.Appointments;
using ProyectoFinal.Models.Base;
using ProyectoFinal.Models.Patients;
using ProyectoFinal.Models.Roles; // ¡NUEVO! Agrega este using para UserRole
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic; // Asegúrate de que este using esté presente

namespace ProyectoFinal.Models.Users;

public partial class User : BaseEntity // ¡Confirmado que hereda de BaseEntity, perfecto!
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

    public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();

    public virtual ICollection<UserToken> UserTokens { get; set; } = new List<UserToken>();

    // ¡NUEVO! Propiedad de navegación para la relación con Roles
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}