namespace Application.Features.Search.Dtos;

public class InvoiceSearchResultDto
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int ServiceOrderId { get; set; }
    public int InvoiceStatusId { get; set; }
    public decimal Total { get; set; }
    public DateTime InvoiceDate { get; set; }
}
