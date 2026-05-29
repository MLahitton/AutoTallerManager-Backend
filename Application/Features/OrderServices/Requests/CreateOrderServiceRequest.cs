namespace Application.Features.OrderServices.Requests;

public class CreateOrderServiceRequest
{
    public int ServiceOrderId { get; set; }
    public int ServiceTypeId { get; set; }
    public string? Description { get; set; }
    public string? WorkPerformed { get; set; }
    public decimal LaborCost { get; set; }
    public bool? CustomerApproved { get; set; }
    public DateTime? ApprovalDate { get; set; }
}
