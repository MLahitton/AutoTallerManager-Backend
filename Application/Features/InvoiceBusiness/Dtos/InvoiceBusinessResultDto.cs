namespace Application.Features.InvoiceBusiness.Dtos;

public class InvoiceBusinessResultDto
{
    public int InvoiceId { get; set; }
    public string Action { get; set; } = string.Empty;
    public bool Success { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
}
