namespace Application.Features.ClientApprovals.Dtos;

public class ClientPendingServiceApprovalDto
{
    public int OrderServiceId { get; set; }
    public int ServiceTypeId { get; set; }
    public string? ServiceTypeName { get; set; }
    public string? Description { get; set; }
    public decimal LaborCost { get; set; }
    public string? WorkPerformed { get; set; }
    public bool? CustomerApproved { get; set; }
    public DateTime? ApprovalDate { get; set; }
}
