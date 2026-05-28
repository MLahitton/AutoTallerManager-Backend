namespace Domain.Entities;

public class OrderServicePart
{
    public int OrderServicePartId { get; set; }
    public int OrderServiceId { get; set; }
    public int PartId { get; set; }
    public int Quantity { get; set; }
    public decimal AppliedUnitPrice { get; set; }
    public bool? CustomerApproved { get; set; }
    public DateTime? ApprovalDate { get; set; }

    public OrderService OrderService { get; set; } = null!;
    public Part Part { get; set; } = null!;
}
