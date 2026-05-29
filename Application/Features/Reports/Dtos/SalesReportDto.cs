namespace Application.Features.Reports.Dtos;

public class SalesReportDto
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int InvoiceCount { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public int IssuedInvoices { get; set; }
    public int CancelledInvoices { get; set; }
}
