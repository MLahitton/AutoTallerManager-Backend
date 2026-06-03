namespace Application.Features.Dashboards.Dtos;

public class MechanicDashboardActiveOrderDto
{
    public int ServiceOrderId { get; set; }
    public int VehicleId { get; set; }
    public string? VehiclePlate { get; set; }
    public string? OrderStatusName { get; set; }
    public DateTime EntryDate { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public int AssignedServicesCount { get; set; }
    public int PendingWorkReportsCount { get; set; }
}
