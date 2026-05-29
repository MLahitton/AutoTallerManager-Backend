namespace Application.Features.InventoryBusiness.Dtos;

public class InventoryPurchaseDetailResultDto
{
    public int PartPurchaseDetailId { get; set; }
    public int PartId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}
