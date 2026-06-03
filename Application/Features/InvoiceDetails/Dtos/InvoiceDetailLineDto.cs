namespace Application.Features.InvoiceDetails.Dtos;

public class InvoiceDetailLineDto
{
    public int InvoiceDetailId { get; set; }
    public int? SourcePartId { get; set; }
    public string Concept { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
    public string LineType { get; set; } = string.Empty;
}
