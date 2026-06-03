namespace Application.Features.ClientApprovals.Dtos;

public class ClientPendingPartApprovalDto
{
    public int OrderServicePartId { get; set; }
    public int OrderServiceId { get; set; }
    public int PartId { get; set; }
    public string? PartName { get; set; }
    public int Quantity { get; set; }
    public decimal AppliedUnitPrice { get; set; }
    public decimal Subtotal { get; set; }
    public bool? CustomerApproved { get; set; }
    public DateTime? ApprovalDate { get; set; }
}
