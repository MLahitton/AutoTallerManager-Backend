namespace Domain.Entities;

public class PartPurchase
{
    public int PartPurchaseId { get; set; }
    public int SupplierId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal Total { get; set; }

    public Supplier Supplier { get; set; } = null!;
    public ICollection<PartPurchaseDetail> PartPurchaseDetails { get; set; } = new List<PartPurchaseDetail>();
}
