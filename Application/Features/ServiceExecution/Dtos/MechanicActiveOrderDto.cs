namespace Application.Features.ServiceExecution.Dtos;

public class MechanicActiveOrderDto
{
    public int ServiceOrderId { get; set; }
    public int VehicleId { get; set; }
    public int OrderStatusId { get; set; }
    public string? OrderStatusName { get; set; }
    public string? VehiclePlate { get; set; }
    public string? VehicleVin { get; set; }
    public int? VehicleYear { get; set; }
    public string? VehicleColor { get; set; }
    public int AssignedServicesCount { get; set; }
    public int PendingWorkReportsCount { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerDocumentNumber { get; set; }
    public DateTime EntryDate { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public string? GeneralDescription { get; set; }
}
