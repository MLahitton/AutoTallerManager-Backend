namespace Domain.Entities;

public class Supplier
{
    public int SupplierId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }

    public ICollection<PartPurchase> PartPurchases { get; set; } = new List<PartPurchase>();
}
