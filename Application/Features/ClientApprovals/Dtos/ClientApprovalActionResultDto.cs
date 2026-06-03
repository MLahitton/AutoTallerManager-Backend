namespace Application.Features.ClientApprovals.Dtos;

public class ClientApprovalActionResultDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public bool CustomerApproved { get; set; }
    public DateTime ApprovalDate { get; set; }
    public int ServiceOrderId { get; set; }
}
