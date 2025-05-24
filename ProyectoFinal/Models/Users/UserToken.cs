namespace ProyectoFinal.Models.Users;

public partial class UserToken
{
    public int TokenId { get; set; }

    public string Token { get; set; } = null!;

    public DateTime Expiration { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? UserId { get; set; }

    public virtual User? User { get; set; }
}
