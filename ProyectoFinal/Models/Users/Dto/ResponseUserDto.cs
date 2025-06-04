namespace ProyectoFinal.Models.Users.Dto
{
    public class ResponseUserDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? ModifiedBy { get; set; } 
        public DateTime? ModifiedAt { get; set; }
    }
}
