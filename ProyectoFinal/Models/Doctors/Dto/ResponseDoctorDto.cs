namespace ProyectoFinal.Models.Doctors.Dto
{
    public class ResponseDoctorDto
    {
        public int DoctorId { get; set; }
        public string FirstName { get; set; } = null!;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = null!;
        public string? SecondLastName { get; set; }
        public string Dui { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string SpecialtyName { get; set; } = null!;
        public string InstitutionName { get; set; } = null!;
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
