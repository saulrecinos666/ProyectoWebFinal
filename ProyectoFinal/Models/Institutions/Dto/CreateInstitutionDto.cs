namespace ProyectoFinal.Models.Institutions.Dto
{
    public class CreateInstitutionDto
    {
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string DistrictCode { get; set; } = null!;
        public string Phone { get; set; }
        public string Email { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
