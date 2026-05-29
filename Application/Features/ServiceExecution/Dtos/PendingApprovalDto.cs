namespace Application.Features.ServiceExecution.Dtos;

public class PendingApprovalDto
{
    public IReadOnlyList<PendingOrderServiceApprovalDto> OrderServices { get; set; } = Array.Empty<PendingOrderServiceApprovalDto>();
    public IReadOnlyList<PendingOrderServicePartApprovalDto> OrderServiceParts { get; set; } = Array.Empty<PendingOrderServicePartApprovalDto>();
}
