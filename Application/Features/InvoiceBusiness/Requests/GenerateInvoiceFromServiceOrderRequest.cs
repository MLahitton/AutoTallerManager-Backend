namespace Application.Features.InvoiceBusiness.Requests;

public class GenerateInvoiceFromServiceOrderRequest
{
    public string? InvoiceNumber { get; set; }
    public int? InvoiceStatusId { get; set; }
    public decimal Tax { get; set; }
    public string? Observations { get; set; }
}
