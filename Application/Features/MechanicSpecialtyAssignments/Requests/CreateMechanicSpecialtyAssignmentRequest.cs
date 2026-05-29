namespace Application.Features.MechanicSpecialtyAssignments.Requests;

public class CreateMechanicSpecialtyAssignmentRequest
{
    public int PersonId { get; set; }
    public int SpecialtyId { get; set; }
}
