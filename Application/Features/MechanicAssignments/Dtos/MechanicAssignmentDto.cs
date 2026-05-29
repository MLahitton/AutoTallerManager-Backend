namespace Application.Features.MechanicAssignments.Dtos;

public class MechanicAssignmentDto
{
    public int MechanicAssignmentId { get; set; }
    public int OrderServiceId { get; set; }
    public int MechanicPersonId { get; set; }
    public int SpecialtyId { get; set; }
}
