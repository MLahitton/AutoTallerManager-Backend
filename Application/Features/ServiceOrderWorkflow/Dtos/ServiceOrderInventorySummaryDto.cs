namespace Application.Features.ServiceOrderWorkflow.Dtos;

public class ServiceOrderInventorySummaryDto
{
    public int EntryInventoryId { get; set; }
    public bool HasScratches { get; set; }
    public string? ScratchesDescription { get; set; }
    public bool HasToolbox { get; set; }
    public string? ToolboxDescription { get; set; }
    public bool OwnershipCardDelivered { get; set; }
    public string? Observations { get; set; }
    public DateTime RegisteredAt { get; set; }
}
