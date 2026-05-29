using Application.Common.Results;

namespace Application.Features.MechanicSpecialtyAssignments.Errors;

public static class MechanicSpecialtyAssignmentErrors
{
    public static readonly Error NotFound = new("MechanicSpecialtyAssignments.NotFound", "Mechanic specialty assignment was not found.");
    public static readonly Error PersonIdInvalid = new("MechanicSpecialtyAssignments.PersonIdInvalid", "PersonId must be greater than 0.");
    public static readonly Error PersonNotFound = new("MechanicSpecialtyAssignments.PersonNotFound", "Person was not found.");
    public static readonly Error PersonIsNotMechanicInvalid = new("MechanicSpecialtyAssignments.PersonIsNotMechanicInvalid", "Person does not have an active mechanic role.");
    public static readonly Error SpecialtyIdInvalid = new("MechanicSpecialtyAssignments.SpecialtyIdInvalid", "SpecialtyId must be greater than 0.");
    public static readonly Error SpecialtyNotFound = new("MechanicSpecialtyAssignments.SpecialtyNotFound", "Mechanic specialty was not found.");
    public static readonly Error DuplicateAssignmentConflict = new("MechanicSpecialtyAssignments.DuplicateAssignmentConflict", "Person already has this mechanic specialty assignment.");
    public static readonly Error InUse = new("MechanicSpecialtyAssignments.InUse", "Mechanic specialty assignment is used by one or more mechanic assignments.");
}
