namespace Application.Features.Invoices.Requests;

public class UpdateInvoiceRequest
{
    public string? InvoiceNumber { get; set; }
    public int ServiceOrderId { get; set; }
    public int InvoiceStatusId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public decimal Tax { get; set; }
    public string? Observations { get; set; }
}
