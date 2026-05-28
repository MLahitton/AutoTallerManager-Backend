namespace Domain.Entities;

public class InvoiceStatus
{
    public int InvoiceStatusId { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
