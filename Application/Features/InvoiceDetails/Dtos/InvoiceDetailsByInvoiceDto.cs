namespace Application.Features.InvoiceDetails.Dtos;

public class InvoiceDetailsByInvoiceDto
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int InvoiceStatusId { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public IReadOnlyList<InvoiceDetailLineDto> Details { get; set; } = Array.Empty<InvoiceDetailLineDto>();
}
