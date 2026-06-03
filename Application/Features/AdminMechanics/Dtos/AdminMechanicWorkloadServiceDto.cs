namespace Application.Features.AdminMechanics.Dtos;

public class AdminMechanicWorkloadServiceDto
{
    public int MechanicAssignmentId { get; set; }
    public int OrderServiceId { get; set; }
    public int ServiceOrderId { get; set; }
    public string? ServiceTypeName { get; set; }
    public string? VehiclePlate { get; set; }
    public string? OrderStatusName { get; set; }
    public string? CustomerName { get; set; }
    public bool CustomerApproved { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public bool WorkReported { get; set; }
}
