namespace Application.Features.PartPurchases.Requests;

public class CreatePartPurchaseRequest
{
    public int SupplierId { get; set; }
    public DateTime? PurchaseDate { get; set; }
}
