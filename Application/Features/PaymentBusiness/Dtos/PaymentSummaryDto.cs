namespace Application.Features.PaymentBusiness.Dtos;

public class PaymentSummaryDto
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal InvoiceTotal { get; set; }
    public decimal CompletedPaidAmount { get; set; }
    public decimal RefundedAmount { get; set; }
    public decimal PendingAmount { get; set; }
    public IReadOnlyList<PaymentSummaryItemDto> Payments { get; set; } = Array.Empty<PaymentSummaryItemDto>();
}
