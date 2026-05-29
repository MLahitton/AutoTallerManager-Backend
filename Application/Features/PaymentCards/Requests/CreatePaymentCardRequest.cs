namespace Application.Features.PaymentCards.Requests;

public class CreatePaymentCardRequest
{
    public int PaymentId { get; set; }
    public int CardTypeId { get; set; }
    public string? LastFourDigits { get; set; }
    public string? CardHolder { get; set; }
    public string? AuthorizationCode { get; set; }
}
