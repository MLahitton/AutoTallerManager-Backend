using Application.Common.Results;

namespace Application.Features.ServiceOrders.Errors;

public static class ServiceOrderErrors
{
    public static readonly Error NotFound = new("ServiceOrders.NotFound", "Service order was not found.");
    public static readonly Error VehicleIdInvalid = new("ServiceOrders.VehicleIdInvalid", "VehicleId must be greater than 0.");
    public static readonly Error VehicleNotFound = new("ServiceOrders.VehicleNotFound", "Vehicle was not found.");
    public static readonly Error VehicleInactive = new("ServiceOrders.VehicleInactive", "Vehicle is inactive.");
    public static readonly Error OrderStatusIdInvalid = new("ServiceOrders.OrderStatusIdInvalid", "OrderStatusId must be greater than 0.");
    public static readonly Error OrderStatusNotFound = new("ServiceOrders.OrderStatusNotFound", "Order status was not found.");
    public static readonly Error EntryDateInvalid = new("ServiceOrders.EntryDateInvalid", "Entry date is invalid.");
    public static readonly Error EstimatedDeliveryDateInvalid = new("ServiceOrders.EstimatedDeliveryDateInvalid", "Estimated delivery date must be greater than or equal to entry date.");
    public static readonly Error ActiveOrderAlreadyExists = new("ServiceOrders.ActiveOrderAlreadyExists", "Vehicle already has an active service order.");
    public static readonly Error CancellationReasonRequired = new("ServiceOrders.CancellationReasonRequired", "Cancellation reason is required for cancelled or voided service orders.");
    public static readonly Error InUse = new("ServiceOrders.InUse", "Service order is assigned to one or more records.");
}
