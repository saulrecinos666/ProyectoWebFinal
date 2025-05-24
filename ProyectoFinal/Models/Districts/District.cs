using ProyectoFinal.Models.Institutions;
using ProyectoFinal.Models.Municipalities;
using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal.Models.Districts;

public partial class District
{
    [Required(ErrorMessage = "El Codigo del Distrito es requerido.")]
    [StringLength(4, ErrorMessage = "El Codigo del Distrito no puede exceder los 4 caracteres.")]
    public string DistrictCode { get; set; } = null!;

    [Required(ErrorMessage = "El Nombre del Distrito es requerido.")]
    [StringLength(50, ErrorMessage = "El Nombre del Distrito no puede exceder los 50 caracteres.")]
    public string DistrictName { get; set; } = null!;

    [Required(ErrorMessage = "El Codigo del Municipio es requerido.")]
    [StringLength(4, ErrorMessage = "El Codigo del Municipio no puede exceder los 4 caracteres.")]
    public string MunicipalityCode { get; set; } = null!;

    public virtual ICollection<Institution> Institutions { get; set; } = new List<Institution>();

    public virtual Municipality MunicipalityCodeNavigation { get; set; } = null!;
}
