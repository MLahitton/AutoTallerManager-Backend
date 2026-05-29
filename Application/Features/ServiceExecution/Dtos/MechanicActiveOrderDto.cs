namespace Application.Features.ServiceExecution.Dtos;

public class MechanicActiveOrderDto
{
    public int ServiceOrderId { get; set; }
    public int VehicleId { get; set; }
    public int OrderStatusId { get; set; }
    public DateTime EntryDate { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public string? GeneralDescription { get; set; }
}
