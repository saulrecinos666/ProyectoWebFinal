namespace ProyectoFinal.Models.Specialties.Dto
{
    public class UpdateSpecialtyDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
