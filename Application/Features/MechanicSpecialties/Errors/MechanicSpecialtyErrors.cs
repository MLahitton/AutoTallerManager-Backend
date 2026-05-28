using Application.Common.Results;

namespace Application.Features.MechanicSpecialties.Errors;

public static class MechanicSpecialtyErrors
{
    public static readonly Error NotFound = new("MechanicSpecialties.NotFound", "Mechanic specialty was not found.");
    public static readonly Error NameRequired = new("MechanicSpecialties.NameRequired", "Mechanic specialty name is required.");
    public static readonly Error NameTooLong = new("MechanicSpecialties.NameTooLong", "Mechanic specialty name cannot exceed 100 characters.");
    public static readonly Error NameAlreadyExists = new("MechanicSpecialties.NameAlreadyExists", "Mechanic specialty name already exists.");
    public static readonly Error InUse = new("MechanicSpecialties.InUse", "Mechanic specialty is assigned to one or more records.");
}
