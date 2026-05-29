namespace Application.Features.MechanicAssignments.Requests;

public class CreateMechanicAssignmentRequest
{
    public int OrderServiceId { get; set; }
    public int MechanicPersonId { get; set; }
    public int SpecialtyId { get; set; }
}
