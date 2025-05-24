namespace ProyectoFinal.Models.Users;

public partial class UserLoginHistory
{
    public int LoginId { get; set; }

    public int? UserId { get; set; }

    public DateTime? LoginDate { get; set; }

    public string? IpAddress { get; set; }

    public virtual User? User { get; set; }
}
