namespace Application.Features.PaymentCards.Dtos;

public class PaymentCardDto
{
    public int PaymentCardId { get; set; }
    public int PaymentId { get; set; }
    public int CardTypeId { get; set; }
    public string LastFourDigits { get; set; } = string.Empty;
    public string CardHolder { get; set; } = string.Empty;
    public string? AuthorizationCode { get; set; }
}
