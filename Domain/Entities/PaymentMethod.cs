namespace Domain.Entities;

public class PaymentMethod
{
    public int PaymentMethodId { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
