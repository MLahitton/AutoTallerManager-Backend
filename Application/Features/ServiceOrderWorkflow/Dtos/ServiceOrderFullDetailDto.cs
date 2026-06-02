namespace Application.Features.ServiceOrderWorkflow.Dtos;

public class ServiceOrderFullDetailDto
{
    public int ServiceOrderId { get; set; }
    public int VehicleId { get; set; }
    public string VehiclePlate { get; set; } = string.Empty;
    public int OrderStatusId { get; set; }
    public DateTime EntryDate { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public string? GeneralDescription { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? CancellationDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public ServiceOrderInventorySummaryDto? Inventory { get; set; }
    public IReadOnlyList<ServiceOrderServiceSummaryDto> Services { get; set; } = Array.Empty<ServiceOrderServiceSummaryDto>();
    public ServiceOrderInvoiceSummaryDto? Invoice { get; set; }
}
