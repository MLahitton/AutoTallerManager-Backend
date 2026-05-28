namespace Domain.Entities;

public class User
{
    public int UserId { get; set; }
    public int PersonId { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiration { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public Person Person { get; set; } = null!;
    public ICollection<OrderStatusHistory> OrderStatusHistories { get; set; } = new List<OrderStatusHistory>();
    public ICollection<Audit> Audits { get; set; } = new List<Audit>();
}
