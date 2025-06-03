using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal.Models.Institutions.Dto
{
    public class ResponseInstitutionDto
    {
        public int InstitutionId { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string DistrictName { get; set; } = null!;
        public string Phone { get; set; }
        public string Email { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}