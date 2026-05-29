namespace Application.Features.WorkshopIntake.Dtos;

public class WorkshopIntakeDto
{
    public int ServiceOrderId { get; set; }
    public int VehicleId { get; set; }
    public int OrderStatusId { get; set; }
    public int EntryInventoryId { get; set; }
    public int OrderStatusHistoryId { get; set; }
    public DateTime EntryDate { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public string? GeneralDescription { get; set; }
    public IReadOnlyList<WorkshopIntakeOrderServiceDto> Services { get; set; } = Array.Empty<WorkshopIntakeOrderServiceDto>();
}
