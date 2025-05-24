using ProyectoFinal.Models.Appointments;
using ProyectoFinal.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal.Models.Patients;

public partial class Patient : BaseEntity
{
    public int PatientId { get; set; }

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

    [Required(ErrorMessage = "La Fecha de Nacimeinto es requerida.")]
    public DateOnly DateOfBirth { get; set; }

    [Required(ErrorMessage = "El Genero es requerido.")]
    public string Gender { get; set; } = null!;

    [Required(ErrorMessage = "La Direccion es requerida.")]
    [StringLength(200, ErrorMessage = "El Correo no puede exceder los 200 caracteres.")]
    public string Address { get; set; } = null!;

    public string? Phone { get; set; }

    [Required(ErrorMessage = "El Correo es requerido.")]
    [StringLength(100, ErrorMessage = "El Correo no puede exceder los 100 caracteres.")]
    public string Email { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
