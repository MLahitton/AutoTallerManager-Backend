namespace Application.Features.ClientApprovals.Dtos;

public class ClientPendingApprovalDto
{
    public int ServiceOrderId { get; set; }
    public int VehicleId { get; set; }
    public string VehiclePlate { get; set; } = string.Empty;
    public int OrderStatusId { get; set; }
    public DateTime EntryDate { get; set; }
    public string? GeneralDescription { get; set; }
    public IReadOnlyList<ClientPendingServiceApprovalDto> PendingServices { get; set; } = Array.Empty<ClientPendingServiceApprovalDto>();
    public IReadOnlyList<ClientPendingPartApprovalDto> PendingParts { get; set; } = Array.Empty<ClientPendingPartApprovalDto>();
}
