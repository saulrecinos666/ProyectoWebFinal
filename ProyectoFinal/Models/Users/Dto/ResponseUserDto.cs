namespace ProyectoFinal.Models.Users.Dto
{
    public class ResponseUserDto
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? ModifiedBy { get; set; } 
        public DateTime? ModifiedAt { get; set; }
    }
}
