namespace Application.Features.WorkshopIntake.Requests;

public class CreateWorkshopIntakeRequest
{
    public int VehicleId { get; set; }
    public int? InitialOrderStatusId { get; set; }
    public DateTime? EntryDate { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public string? GeneralDescription { get; set; }
    public bool HasScratches { get; set; }
    public string? ScratchesDescription { get; set; }
    public bool HasToolbox { get; set; }
    public string? ToolboxDescription { get; set; }
    public bool OwnershipCardDelivered { get; set; }
    public string? InventoryObservations { get; set; }
    public IReadOnlyList<CreateWorkshopIntakeOrderServiceRequest>? Services { get; set; }
}
