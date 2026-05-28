namespace Domain.Entities;

public class MechanicSpecialtyAssignment
{
    public int AssignmentId { get; set; }
    public int PersonId { get; set; }
    public int SpecialtyId { get; set; }

    public Person Person { get; set; } = null!;
    public MechanicSpecialty Specialty { get; set; } = null!;
}
