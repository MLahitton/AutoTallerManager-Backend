namespace Application.Features.ServiceExecution.Dtos;

public class MechanicAssignedServiceDto
{
    public int OrderServiceId { get; set; }
    public int ServiceOrderId { get; set; }
    public int VehicleId { get; set; }
    public int ServiceTypeId { get; set; }
    public string? Description { get; set; }
    public string? WorkPerformed { get; set; }
    public decimal LaborCost { get; set; }
    public bool? CustomerApproved { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public int SpecialtyId { get; set; }
}
