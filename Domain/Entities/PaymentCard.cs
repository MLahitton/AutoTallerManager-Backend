namespace Domain.Entities;

public class PaymentCard
{
    public int PaymentCardId { get; set; }
    public int PaymentId { get; set; }
    public int CardTypeId { get; set; }
    public string LastFourDigits { get; set; } = string.Empty;
    public string CardHolder { get; set; } = string.Empty;
    public string? AuthorizationCode { get; set; }

    public Payment Payment { get; set; } = null!;
    public CardType CardType { get; set; } = null!;
}
