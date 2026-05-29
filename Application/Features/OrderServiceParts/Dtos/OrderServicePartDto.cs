namespace Application.Features.OrderServiceParts.Dtos;

public class OrderServicePartDto
{
    public int OrderServicePartId { get; set; }
    public int OrderServiceId { get; set; }
    public int PartId { get; set; }
    public int Quantity { get; set; }
    public decimal AppliedUnitPrice { get; set; }
    public decimal Subtotal { get; set; }
    public bool? CustomerApproved { get; set; }
    public DateTime? ApprovalDate { get; set; }
}
