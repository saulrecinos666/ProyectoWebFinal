using ProyectoFinal.Models.Municipalities;
using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal.Models.Departments;

public partial class Department
{
    [Required(ErrorMessage = "El Codigo del Departamento es requerido.")]
    [StringLength(2, ErrorMessage = "El Codigo del Departamento no puede exceder los 2 caracteres.")]
    public string DepartmentCode { get; set; } = null!;

    [Required(ErrorMessage = "El Nombre del Departamento es requerido.")]
    [StringLength(50, ErrorMessage = "El Nombre del Departamento no puede exceder los 50 caracteres.")]
    public string DepartmentName { get; set; } = null!;

    public virtual ICollection<Municipality> Municipalities { get; set; } = new List<Municipality>();
}
