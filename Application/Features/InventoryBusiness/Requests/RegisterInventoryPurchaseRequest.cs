namespace Application.Features.InventoryBusiness.Requests;

public class RegisterInventoryPurchaseRequest
{
    public int SupplierId { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public IReadOnlyList<RegisterInventoryPurchaseDetailRequest>? Details { get; set; }
}
