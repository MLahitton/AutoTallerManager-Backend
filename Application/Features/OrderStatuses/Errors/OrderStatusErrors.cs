using Application.Common.Results;

namespace Application.Features.OrderStatuses.Errors;

public static class OrderStatusErrors
{
    public static readonly Error NotFound = new("OrderStatuses.NotFound", "Order status was not found.");
    public static readonly Error NameRequired = new("OrderStatuses.NameRequired", "Order status name is required.");
    public static readonly Error NameTooLong = new("OrderStatuses.NameTooLong", "Order status name cannot exceed 50 characters.");
    public static readonly Error NameAlreadyExists = new("OrderStatuses.NameAlreadyExists", "Order status name already exists.");
    public static readonly Error InUse = new("OrderStatuses.InUse", "Order status is assigned to one or more records.");
}
