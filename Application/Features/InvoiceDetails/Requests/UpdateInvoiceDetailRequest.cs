namespace Application.Features.InvoiceDetails.Requests;

public class UpdateInvoiceDetailRequest
{
    public int InvoiceId { get; set; }
    public int? SourcePartId { get; set; }
    public string? Concept { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? LineType { get; set; }
}
