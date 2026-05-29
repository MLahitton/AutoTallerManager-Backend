using Application.Common.Results;

namespace Application.Features.OrderServices.Errors;

public static class OrderServiceErrors
{
    public static readonly Error NotFound = new("OrderServices.NotFound", "Order service was not found.");
    public static readonly Error ServiceOrderIdInvalid = new("OrderServices.ServiceOrderIdInvalid", "ServiceOrderId must be greater than 0.");
    public static readonly Error ServiceOrderNotFound = new("OrderServices.ServiceOrderNotFound", "Service order was not found.");
    public static readonly Error ServiceOrderCannotBeModifiedConflict = new("OrderServices.ServiceOrderCannotBeModifiedConflict", "Order service cannot be modified because its service order is cancelled or voided.");
    public static readonly Error ServiceTypeIdInvalid = new("OrderServices.ServiceTypeIdInvalid", "ServiceTypeId must be greater than 0.");
    public static readonly Error ServiceTypeNotFound = new("OrderServices.ServiceTypeNotFound", "Service type was not found.");
    public static readonly Error LaborCostInvalid = new("OrderServices.LaborCostInvalid", "LaborCost must be greater than or equal to 0.");
    public static readonly Error ApprovalDateInvalid = new("OrderServices.ApprovalDateInvalid", "ApprovalDate is invalid.");
    public static readonly Error InUse = new("OrderServices.InUse", "Order service is assigned to one or more records.");
}
