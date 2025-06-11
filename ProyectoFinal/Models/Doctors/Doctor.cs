using ProyectoFinal.Models.Appointments;
using ProyectoFinal.Models.Base;
using ProyectoFinal.Models.Institutions;
using ProyectoFinal.Models.Specialties;
using ProyectoFinal.Models.Users;
using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal.Models.Doctors;

public partial class Doctor : BaseEntity
{
    public int DoctorId { get; set; }

    [Required(ErrorMessage = "El Primer Nombre es requerido.")]
    [StringLength(50, ErrorMessage = "El Primer Nombre no puede exceder los 50 caracteres.")]
    public string FirstName { get; set; } = null!;

    [StringLength(50, ErrorMessage = "El Segundo Nombre no puede exceder los 50 caracteres.")]
    public string? MiddleName { get; set; }

    [Required(ErrorMessage = "El Primer Apellido es requerido.")]
    [StringLength(50, ErrorMessage = "El Primer Apellido no puede exceder los 50 caracteres.")]
    public string LastName { get; set; } = null!;

    [StringLength(50, ErrorMessage = "El Segundo Apellido no puede exceder los 50 caracteres.")]
    public string? SecondLastName { get; set; }

    [Required(ErrorMessage = "El DUI es requerido.")]
    [StringLength(9, ErrorMessage = "El DUI no puede exceder los 9 digitos.")]
    public string Dui { get; set; } = null!;

    [Required(ErrorMessage = "El Id de la Especialidad es requerido.")]
    public int SpecialtyId { get; set; }

    public int InstitutionId { get; set; }

    [Required(ErrorMessage = "El Correo es requerido.")]
    [StringLength(100, ErrorMessage = "El Correo no puede exceder los 100 caracteres.")]
    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public int UserId { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual Institution Institution { get; set; }

    public virtual Specialty Specialty { get; set; } = null!;
}
