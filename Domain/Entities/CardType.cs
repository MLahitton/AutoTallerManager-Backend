namespace Domain.Entities;

public class CardType
{
    public int CardTypeId { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<PaymentCard> PaymentCards { get; set; } = new List<PaymentCard>();
}
