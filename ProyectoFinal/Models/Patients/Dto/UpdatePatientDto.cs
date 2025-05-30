namespace ProyectoFinal.Models.Patients.Dto
{
    public class UpdatePatientDto
    {
        public string FirstName { get; set; } = null!;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = null!;
        public string? SecondLastName { get; set; }
        public string Dui { get; set; } = null!;
        public DateOnly DateOfBirth { get; set; }
        public string Gender { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string? Phone { get; set; }
        public string Email { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
