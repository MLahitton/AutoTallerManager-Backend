namespace Application.Features.PartPurchases.Dtos;

public class PartPurchaseDto
{
    public int PartPurchaseId { get; set; }
    public int SupplierId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal Total { get; set; }
}
