namespace Application.Features.ServiceOrderWorkflow.Dtos;

public class ServiceOrderInvoiceSummaryDto
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int InvoiceStatusId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public IReadOnlyList<ServiceOrderPaymentSummaryDto> Payments { get; set; } = Array.Empty<ServiceOrderPaymentSummaryDto>();
}
