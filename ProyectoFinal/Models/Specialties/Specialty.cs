using ProyectoFinal.Models.Base;
using ProyectoFinal.Models.Doctors;

namespace ProyectoFinal.Models.Specialties;

public partial class Specialty : BaseEntity
{
    public int SpecialtyId { get; set; }

    public string SpecialtyName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
}
