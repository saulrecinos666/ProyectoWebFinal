namespace ProyectoFinal.Models.Doctors.Dto
{
    public class CreateDoctorDto
    {
        public string FirstName { get; set; } = null!;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = null!;
        public string? SecondLastName { get; set; }
        public string Dui { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public int SpecialtyId { get; set; }
        public int InstitutionId { get; set; } 
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
