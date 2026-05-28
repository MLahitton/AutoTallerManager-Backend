namespace Domain.Entities;

public class Invoice
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

    public ServiceOrder ServiceOrder { get; set; } = null!;
    public InvoiceStatus InvoiceStatus { get; set; } = null!;
    public ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
