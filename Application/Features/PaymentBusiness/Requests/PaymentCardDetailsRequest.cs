namespace Application.Features.PaymentBusiness.Requests;

public class PaymentCardDetailsRequest
{
    public int CardTypeId { get; set; }
    public string? LastFourDigits { get; set; }
    public string? CardHolder { get; set; }
    public string? AuthorizationCode { get; set; }
}
