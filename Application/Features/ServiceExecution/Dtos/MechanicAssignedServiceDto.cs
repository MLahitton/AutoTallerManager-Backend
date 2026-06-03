namespace Application.Features.ServiceExecution.Dtos;

public class MechanicAssignedServiceDto
{
    public int MechanicAssignmentId { get; set; }
    public int OrderServiceId { get; set; }
    public int ServiceOrderId { get; set; }
    public int VehicleId { get; set; }
    public int ServiceTypeId { get; set; }
    public int OrderStatusId { get; set; }
    public string? OrderStatusName { get; set; }
    public string? VehiclePlate { get; set; }
    public string? VehicleVin { get; set; }
    public int? VehicleYear { get; set; }
    public string? VehicleColor { get; set; }
    public string? ServiceTypeName { get; set; }
    public string? Description { get; set; }
    public string? WorkPerformed { get; set; }
    public decimal LaborCost { get; set; }
    public bool? CustomerApproved { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public int SpecialtyId { get; set; }
    public string? SpecialtyName { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerDocumentNumber { get; set; }
}
