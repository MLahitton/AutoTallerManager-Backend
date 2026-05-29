namespace Application.Features.PaymentCards.Requests;

public class UpdatePaymentCardRequest
{
    public int PaymentId { get; set; }
    public int CardTypeId { get; set; }
    public string? LastFourDigits { get; set; }
    public string? CardHolder { get; set; }
    public string? AuthorizationCode { get; set; }
}
