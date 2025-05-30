namespace ProyectoFinal.Models.Institutions.Dto
{
    public class UpdateInstitutionDto
    {
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string DistrictCode { get; set; } = null!;
        public string Phone { get; set; }
        public string Email { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
