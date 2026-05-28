namespace Domain.Entities;

public class OrderService
{
    public int OrderServiceId { get; set; }
    public int ServiceOrderId { get; set; }
    public int ServiceTypeId { get; set; }
    public string? Description { get; set; }
    public string? WorkPerformed { get; set; }
    public decimal LaborCost { get; set; }
    public bool? CustomerApproved { get; set; }
    public DateTime? ApprovalDate { get; set; }

    public ServiceOrder ServiceOrder { get; set; } = null!;
    public ServiceType ServiceType { get; set; } = null!;
    public ICollection<OrderServicePart> OrderServiceParts { get; set; } = new List<OrderServicePart>();
    public ICollection<MechanicAssignment> MechanicAssignments { get; set; } = new List<MechanicAssignment>();
}
