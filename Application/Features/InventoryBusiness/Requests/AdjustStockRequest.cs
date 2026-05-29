namespace Application.Features.InventoryBusiness.Requests;

public class AdjustStockRequest
{
    public int PartId { get; set; }
    public int AdjustmentQuantity { get; set; }
    public string? Reason { get; set; }
}
