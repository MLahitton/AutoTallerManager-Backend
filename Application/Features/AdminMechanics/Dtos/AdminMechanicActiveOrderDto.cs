namespace Application.Features.AdminMechanics.Dtos;

public class AdminMechanicActiveOrderDto
{
    public int ServiceOrderId { get; set; }
    public int VehicleId { get; set; }
    public string? VehiclePlate { get; set; }
    public string OrderStatusName { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public int AssignedServicesCount { get; set; }
    public int PendingWorkReportsCount { get; set; }
}
