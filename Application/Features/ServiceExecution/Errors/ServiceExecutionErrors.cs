using Application.Common.Results;

namespace Application.Features.ServiceExecution.Errors;

public static class ServiceExecutionErrors
{
    public static readonly Error CurrentPersonIdInvalid = new("ServiceExecution.CurrentPersonIdInvalid", "Current person id must be greater than 0.");
    public static readonly Error OrderServiceIdInvalid = new("ServiceExecution.OrderServiceIdInvalid", "OrderServiceId must be greater than 0.");
    public static readonly Error OrderServiceNotFound = new("ServiceExecution.OrderServiceNotFound", "Order service was not found.");
    public static readonly Error ServiceOrderNotFound = new("ServiceExecution.ServiceOrderNotFound", "Service order was not found.");
    public static readonly Error ServiceOrderCannotBeModifiedConflict = new("ServiceExecution.ServiceOrderCannotBeModifiedConflict", "Service order cannot be modified.");
    public static readonly Error MechanicNotAssignedConflict = new("ServiceExecution.MechanicNotAssignedConflict", "Mechanic is not assigned to this order service.");
    public static readonly Error MechanicPersonIdInvalid = new("ServiceExecution.MechanicPersonIdInvalid", "MechanicPersonId must be greater than 0.");
    public static readonly Error MechanicPersonNotFound = new("ServiceExecution.MechanicPersonNotFound", "Mechanic person was not found.");
    public static readonly Error PersonIsNotMechanicInvalid = new("ServiceExecution.PersonIsNotMechanicInvalid", "Person does not have an active mechanic role.");
    public static readonly Error SpecialtyIdInvalid = new("ServiceExecution.SpecialtyIdInvalid", "SpecialtyId must be greater than 0.");
    public static readonly Error SpecialtyNotFound = new("ServiceExecution.SpecialtyNotFound", "Specialty was not found.");
    public static readonly Error MechanicDoesNotHaveSpecialtyInvalid = new("ServiceExecution.MechanicDoesNotHaveSpecialtyInvalid", "Mechanic does not have the requested specialty.");
    public static readonly Error DuplicateMechanicAssignmentConflict = new("ServiceExecution.DuplicateMechanicAssignmentConflict", "Mechanic is already assigned to this order service.");
    public static readonly Error MechanicAssignmentIdInvalid = new("ServiceExecution.MechanicAssignmentIdInvalid", "MechanicAssignmentId must be greater than 0.");
    public static readonly Error MechanicAssignmentNotFound = new("ServiceExecution.MechanicAssignmentNotFound", "Mechanic assignment was not found.");
    public static readonly Error MechanicAssignmentDoesNotBelongToOrderServiceConflict = new("ServiceExecution.MechanicAssignmentDoesNotBelongToOrderServiceConflict", "Mechanic assignment does not belong to this order service.");
    public static readonly Error WorkPerformedRequired = new("ServiceExecution.WorkPerformedRequired", "WorkPerformed is required.");
    public static readonly Error LaborCostInvalid = new("ServiceExecution.LaborCostInvalid", "LaborCost must be greater than or equal to 0.");
    public static readonly Error PartIdInvalid = new("ServiceExecution.PartIdInvalid", "PartId must be greater than 0.");
    public static readonly Error PartNotFound = new("ServiceExecution.PartNotFound", "Part was not found.");
    public static readonly Error PartInactive = new("ServiceExecution.PartInactive", "Part is inactive.");
    public static readonly Error QuantityInvalid = new("ServiceExecution.QuantityInvalid", "Quantity must be greater than 0.");
    public static readonly Error AppliedUnitPriceInvalid = new("ServiceExecution.AppliedUnitPriceInvalid", "AppliedUnitPrice must be greater than or equal to 0.");
    public static readonly Error DuplicatePartForOrderServiceConflict = new("ServiceExecution.DuplicatePartForOrderServiceConflict", "Part already requested for this order service.");
    public static readonly Error OrderServicePartIdInvalid = new("ServiceExecution.OrderServicePartIdInvalid", "OrderServicePartId must be greater than 0.");
    public static readonly Error OrderServicePartNotFound = new("ServiceExecution.OrderServicePartNotFound", "Order service part was not found.");
    public static readonly Error ClientCannotAccessServiceOrderConflict = new("ServiceExecution.ClientCannotAccessServiceOrderConflict", "Client cannot access this service order.");
    public static readonly Error InsufficientStockConflict = new("ServiceExecution.InsufficientStockConflict", "Part does not have enough stock.");
    public static readonly Error StockWouldBeNegativeInvalid = new("ServiceExecution.StockWouldBeNegativeInvalid", "Stock cannot become negative.");
}
