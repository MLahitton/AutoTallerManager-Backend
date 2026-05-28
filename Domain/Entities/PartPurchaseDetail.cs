namespace Domain.Entities;

public class PartPurchaseDetail
{
    public int PartPurchaseDetailId { get; set; }
    public int PartPurchaseId { get; set; }
    public int PartId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public PartPurchase PartPurchase { get; set; } = null!;
    public Part Part { get; set; } = null!;
}
