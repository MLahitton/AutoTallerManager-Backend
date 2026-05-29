namespace Application.Features.ClientVehicleFlows.Dtos;

public class ClientInvoiceSummaryDto
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int ServiceOrderId { get; set; }
    public int InvoiceStatusId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public string? Observations { get; set; }
}
