using ProyectoFinal.Models.Departments;
using ProyectoFinal.Models.Districts;
using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal.Models.Municipalities;

public partial class Municipality
{
    [Required(ErrorMessage = "El Codigo del Municipio es requerido.")]
    [StringLength(4, ErrorMessage = "El Codigo del Municipio no puede exceder los 4 caracteres.")]
    public string MunicipalityCode { get; set; } = null!;

    [Required(ErrorMessage = "El Nombre del Municipio es requerido.")]
    [StringLength(50, ErrorMessage = "El Nombre del Municipio no puede exceder los 50 caracteres.")]
    public string MunicipalityName { get; set; } = null!;

    [Required(ErrorMessage = "El Codigo del Departamento es requerido.")]
    [StringLength(4, ErrorMessage = "El Codigo del Departamento no puede exceder los 4 caracteres.")]
    public string DepartmentCode { get; set; } = null!;

    public virtual Department DepartmentCodeNavigation { get; set; } = null!;

    public virtual ICollection<District> Districts { get; set; } = new List<District>();
}
