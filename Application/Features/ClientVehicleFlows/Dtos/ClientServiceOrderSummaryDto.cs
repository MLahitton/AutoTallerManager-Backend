namespace Application.Features.ClientVehicleFlows.Dtos;

public class ClientServiceOrderSummaryDto
{
    public int ServiceOrderId { get; set; }
    public int VehicleId { get; set; }
    public int OrderStatusId { get; set; }
    public DateTime EntryDate { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public string? GeneralDescription { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? CancellationDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
