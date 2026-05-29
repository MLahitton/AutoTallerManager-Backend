namespace Application.Features.PartPurchases.Requests;

public class UpdatePartPurchaseRequest
{
    public int SupplierId { get; set; }
    public DateTime PurchaseDate { get; set; }
}
