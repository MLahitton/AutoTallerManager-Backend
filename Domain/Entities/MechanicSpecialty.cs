namespace Domain.Entities;

public class MechanicSpecialty
{
    public int SpecialtyId { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<MechanicSpecialtyAssignment> MechanicSpecialtyAssignments { get; set; } = new List<MechanicSpecialtyAssignment>();
    public ICollection<MechanicAssignment> MechanicAssignments { get; set; } = new List<MechanicAssignment>();
}
