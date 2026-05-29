namespace Application.Features.Payments.Requests;

public class CreatePaymentRequest
{
    public int InvoiceId { get; set; }
    public int PaymentMethodId { get; set; }
    public int PaymentStatusId { get; set; }
    public DateTime? PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string? Reference { get; set; }
}
