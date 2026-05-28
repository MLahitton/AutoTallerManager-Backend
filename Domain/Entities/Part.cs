namespace Domain.Entities;

public class Part
{
    public int PartId { get; set; }
    public int PartCategoryId { get; set; }
    public int? PartBrandId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Stock { get; set; }
    public int MinimumStock { get; set; }
    public decimal UnitPrice { get; set; }
    public bool IsActive { get; set; }

    public PartCategory PartCategory { get; set; } = null!;
    public PartBrand? PartBrand { get; set; }
    public ICollection<OrderServicePart> OrderServiceParts { get; set; } = new List<OrderServicePart>();
    public ICollection<PartPurchaseDetail> PartPurchaseDetails { get; set; } = new List<PartPurchaseDetail>();
    public ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();
}
