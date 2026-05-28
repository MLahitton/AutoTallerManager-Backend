namespace Domain.Entities;

public class PaymentStatus
{
    public int PaymentStatusId { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
