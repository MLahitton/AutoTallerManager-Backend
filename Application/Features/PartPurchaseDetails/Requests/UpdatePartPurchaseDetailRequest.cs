namespace Application.Features.PartPurchaseDetails.Requests;

public class UpdatePartPurchaseDetailRequest
{
    public int PartPurchaseId { get; set; }
    public int PartId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
