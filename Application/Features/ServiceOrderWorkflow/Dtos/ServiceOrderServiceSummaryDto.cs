namespace Application.Features.ServiceOrderWorkflow.Dtos;

public class ServiceOrderServiceSummaryDto
{
    public int OrderServiceId { get; set; }
    public int ServiceTypeId { get; set; }
    public string? Description { get; set; }
    public string? WorkPerformed { get; set; }
    public decimal LaborCost { get; set; }
    public bool? CustomerApproved { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public IReadOnlyList<ServiceOrderMechanicSummaryDto> Mechanics { get; set; } = Array.Empty<ServiceOrderMechanicSummaryDto>();
    public IReadOnlyList<ServiceOrderPartSummaryDto> Parts { get; set; } = Array.Empty<ServiceOrderPartSummaryDto>();
}
