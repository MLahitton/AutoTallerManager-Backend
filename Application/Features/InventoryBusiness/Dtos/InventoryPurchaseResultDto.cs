namespace Application.Features.InventoryBusiness.Dtos;

public class InventoryPurchaseResultDto
{
    public int PartPurchaseId { get; set; }
    public int SupplierId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal Total { get; set; }
    public IReadOnlyList<InventoryPurchaseDetailResultDto> Details { get; set; } = Array.Empty<InventoryPurchaseDetailResultDto>();
}
