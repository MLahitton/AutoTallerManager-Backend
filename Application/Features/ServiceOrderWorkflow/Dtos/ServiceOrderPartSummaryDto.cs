namespace Application.Features.ServiceOrderWorkflow.Dtos;

public class ServiceOrderPartSummaryDto
{
    public int OrderServicePartId { get; set; }
    public int PartId { get; set; }
    public int Quantity { get; set; }
    public decimal AppliedUnitPrice { get; set; }
    public decimal Subtotal { get; set; }
    public bool? CustomerApproved { get; set; }
    public DateTime? ApprovalDate { get; set; }
}
