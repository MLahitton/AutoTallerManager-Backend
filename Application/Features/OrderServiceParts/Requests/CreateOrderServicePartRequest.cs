namespace Application.Features.OrderServiceParts.Requests;

public class CreateOrderServicePartRequest
{
    public int OrderServiceId { get; set; }
    public int PartId { get; set; }
    public int Quantity { get; set; }
    public decimal AppliedUnitPrice { get; set; }
    public bool? CustomerApproved { get; set; }
    public DateTime? ApprovalDate { get; set; }
}
