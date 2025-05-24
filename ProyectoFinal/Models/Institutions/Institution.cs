using ProyectoFinal.Models.Appointments;
using ProyectoFinal.Models.Base;
using ProyectoFinal.Models.Districts;
using ProyectoFinal.Models.Doctors;
using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal.Models.Institutions;

public partial class Institution : BaseEntity
{
    public int InstitutionId { get; set; }

    [Required(ErrorMessage = "El Nombre es requerido.")]
    [StringLength(100, ErrorMessage = "El Nombre no puede exceder los 100 caracteres.")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "La Direccion es requerida.")]
    [StringLength(200, ErrorMessage = "La Direccion no puede exceder los 200 caracteres.")]
    public string Address { get; set; } = null!;

    [Required(ErrorMessage = "El Codigo de Distrito es requerida.")]
    [StringLength(4, ErrorMessage = "La Codigo de Distrito no puede exceder los 4 caracteres.")]
    public string DistrictCode { get; set; } = null!;

    public string Phone { get; set; }

    public string Email { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual District DistrictCodeNavigation { get; set; } = null!;

    public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
}
