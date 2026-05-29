using Application.Common.Results;

namespace Application.Features.OrderServiceParts.Errors;

public static class OrderServicePartErrors
{
    public static readonly Error NotFound = new("OrderServiceParts.NotFound", "Order service part was not found.");
    public static readonly Error OrderServiceIdInvalid = new("OrderServiceParts.OrderServiceIdInvalid", "OrderServiceId must be greater than 0.");
    public static readonly Error OrderServiceNotFound = new("OrderServiceParts.OrderServiceNotFound", "Order service was not found.");
    public static readonly Error OrderServiceCannotBeModifiedConflict = new("OrderServiceParts.OrderServiceCannotBeModifiedConflict", "Order service cannot be modified because its service order is cancelled or voided.");
    public static readonly Error PartIdInvalid = new("OrderServiceParts.PartIdInvalid", "PartId must be greater than 0.");
    public static readonly Error PartNotFound = new("OrderServiceParts.PartNotFound", "Part was not found.");
    public static readonly Error PartInactive = new("OrderServiceParts.PartInactive", "Part is inactive.");
    public static readonly Error QuantityInvalid = new("OrderServiceParts.QuantityInvalid", "Quantity must be greater than 0.");
    public static readonly Error AppliedUnitPriceInvalid = new("OrderServiceParts.AppliedUnitPriceInvalid", "AppliedUnitPrice must be greater than or equal to 0.");
    public static readonly Error DuplicatePartForOrderServiceConflict = new("OrderServiceParts.DuplicatePartForOrderServiceConflict", "Part already exists for this order service.");
    public static readonly Error InsufficientStockConflict = new("OrderServiceParts.InsufficientStockConflict", "Part does not have enough stock.");
    public static readonly Error StockWouldBeNegativeInvalid = new("OrderServiceParts.StockWouldBeNegativeInvalid", "Stock cannot become negative.");
}
