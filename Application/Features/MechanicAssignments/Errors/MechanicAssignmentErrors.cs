using Application.Common.Results;

namespace Application.Features.MechanicAssignments.Errors;

public static class MechanicAssignmentErrors
{
    public static readonly Error NotFound = new("MechanicAssignments.NotFound", "Mechanic assignment was not found.");
    public static readonly Error OrderServiceIdInvalid = new("MechanicAssignments.OrderServiceIdInvalid", "OrderServiceId must be greater than 0.");
    public static readonly Error OrderServiceNotFound = new("MechanicAssignments.OrderServiceNotFound", "Order service was not found.");
    public static readonly Error MechanicPersonIdInvalid = new("MechanicAssignments.MechanicPersonIdInvalid", "MechanicPersonId must be greater than 0.");
    public static readonly Error PersonNotFound = new("MechanicAssignments.PersonNotFound", "Person was not found.");
    public static readonly Error PersonIsNotMechanicInvalid = new("MechanicAssignments.PersonIsNotMechanicInvalid", "Person does not have an active mechanic role.");
    public static readonly Error SpecialtyIdInvalid = new("MechanicAssignments.SpecialtyIdInvalid", "SpecialtyId must be greater than 0.");
    public static readonly Error SpecialtyNotFound = new("MechanicAssignments.SpecialtyNotFound", "Mechanic specialty was not found.");
    public static readonly Error MechanicDoesNotHaveSpecialtyInvalid = new("MechanicAssignments.MechanicDoesNotHaveSpecialtyInvalid", "Mechanic does not have the required specialty.");
    public static readonly Error DuplicateMechanicForOrderServiceConflict = new("MechanicAssignments.DuplicateMechanicForOrderServiceConflict", "Mechanic is already assigned to this order service.");
    public static readonly Error OrderServiceCannotBeModifiedConflict = new("MechanicAssignments.OrderServiceCannotBeModifiedConflict", "Order service cannot be modified because its service order is cancelled or voided.");
}
