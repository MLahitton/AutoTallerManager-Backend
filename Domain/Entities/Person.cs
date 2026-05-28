namespace Domain.Entities;

public class Person
{
    public int PersonId { get; set; }
    public int DocumentTypeId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string? SecondLastName { get; set; }
    public DateTime? BirthDate { get; set; }
    public int? GenderId { get; set; }
    public int? AddressId { get; set; }
    public DateTime CreatedAt { get; set; }

    public DocumentType DocumentType { get; set; } = null!;
    public Gender? Gender { get; set; }
    public Address? Address { get; set; }
    public User? User { get; set; }
    public ICollection<PersonEmail> PersonEmails { get; set; } = new List<PersonEmail>();
    public ICollection<PersonPhone> PersonPhones { get; set; } = new List<PersonPhone>();
    public ICollection<PersonRole> PersonRoles { get; set; } = new List<PersonRole>();
    public ICollection<MechanicSpecialtyAssignment> MechanicSpecialtyAssignments { get; set; } = new List<MechanicSpecialtyAssignment>();
    public ICollection<VehicleOwnerHistory> VehicleOwnerHistories { get; set; } = new List<VehicleOwnerHistory>();
    public ICollection<MechanicAssignment> MechanicAssignments { get; set; } = new List<MechanicAssignment>();
}
