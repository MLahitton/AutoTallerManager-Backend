namespace Application.Features.ServiceExecution.Dtos;

public class PendingOrderServicePartApprovalDto
{
    public int OrderServicePartId { get; set; }
    public int OrderServiceId { get; set; }
    public int ServiceOrderId { get; set; }
    public int VehicleId { get; set; }
    public int PartId { get; set; }
    public int Quantity { get; set; }
    public decimal AppliedUnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}
