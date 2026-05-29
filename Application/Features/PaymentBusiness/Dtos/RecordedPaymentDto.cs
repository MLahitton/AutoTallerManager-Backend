namespace Application.Features.PaymentBusiness.Dtos;

public class RecordedPaymentDto
{
    public int PaymentId { get; set; }
    public int InvoiceId { get; set; }
    public int PaymentMethodId { get; set; }
    public int PaymentStatusId { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string? Reference { get; set; }
    public int? PaymentCardId { get; set; }
}
