using ProyectoFinal.Models.Base;
using ProyectoFinal.Models.Doctors;
using ProyectoFinal.Models.Institutions;
using ProyectoFinal.Models.Patients;
using ProyectoFinal.Models.Users;
using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal.Models.Appointments;

public partial class Appointment : BaseEntity
{
    public int AppointmentId { get; set; }

    [Required(ErrorMessage = "El Usuario es requerido.")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "El Doctor es requerido.")]
    public int DoctorId { get; set; }

    [Required(ErrorMessage = "El Paciente es requerido.")]
    public int PatientId { get; set; }

    [Required(ErrorMessage = "El Instituto es requerido.")]
    public int InstitutionId { get; set; }

    [Required(ErrorMessage = "El Fecha de la cita es requerida.")]
    public DateTime AppointmentDate { get; set; }

    [Required(ErrorMessage = "El Estado de la cita es requerido.")]
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

    public string? Notes { get; set; }

    public virtual Doctor Doctor { get; set; }

    public virtual Institution Institution { get; set; }

    public virtual Patient Patient { get; set; }
}
