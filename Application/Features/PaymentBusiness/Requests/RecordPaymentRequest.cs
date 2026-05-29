namespace Application.Features.PaymentBusiness.Requests;

public class RecordPaymentRequest
{
    public int PaymentMethodId { get; set; }
    public int? PaymentStatusId { get; set; }
    public DateTime? PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string? Reference { get; set; }
    public PaymentCardDetailsRequest? Card { get; set; }
}
