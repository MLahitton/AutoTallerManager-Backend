using Application.Common.Results;

namespace Application.Features.OrderStatusHistories.Errors;

public static class OrderStatusHistoryErrors
{
    public static readonly Error NotFound = new("OrderStatusHistories.NotFound", "Order status history was not found.");
    public static readonly Error ServiceOrderIdInvalid = new("OrderStatusHistories.ServiceOrderIdInvalid", "ServiceOrderId must be greater than 0.");
    public static readonly Error ServiceOrderNotFound = new("OrderStatusHistories.ServiceOrderNotFound", "Service order was not found.");
    public static readonly Error PreviousOrderStatusIdInvalid = new("OrderStatusHistories.PreviousOrderStatusIdInvalid", "PreviousOrderStatusId must be greater than 0 when provided.");
    public static readonly Error PreviousOrderStatusNotFound = new("OrderStatusHistories.PreviousOrderStatusNotFound", "Previous order status was not found.");
    public static readonly Error PreviousOrderStatusDoesNotMatchCurrentConflict = new("OrderStatusHistories.PreviousOrderStatusDoesNotMatchCurrentConflict", "Previous order status does not match the current service order status.");
    public static readonly Error NewOrderStatusIdInvalid = new("OrderStatusHistories.NewOrderStatusIdInvalid", "NewOrderStatusId must be greater than 0.");
    public static readonly Error NewOrderStatusNotFound = new("OrderStatusHistories.NewOrderStatusNotFound", "New order status was not found.");
    public static readonly Error ChangedByUserIdInvalid = new("OrderStatusHistories.ChangedByUserIdInvalid", "ChangedByUserId must be greater than 0.");
    public static readonly Error ChangedByUserNotFound = new("OrderStatusHistories.ChangedByUserNotFound", "Changed-by user was not found.");
}
