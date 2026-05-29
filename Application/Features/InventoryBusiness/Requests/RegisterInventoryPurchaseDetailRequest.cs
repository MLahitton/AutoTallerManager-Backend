namespace Application.Features.InventoryBusiness.Requests;

public class RegisterInventoryPurchaseDetailRequest
{
    public int PartId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
