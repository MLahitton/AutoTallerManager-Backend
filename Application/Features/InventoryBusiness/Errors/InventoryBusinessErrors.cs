using Application.Common.Results;

namespace Application.Features.InventoryBusiness.Errors;

public static class InventoryBusinessErrors
{
    public static readonly Error SupplierIdInvalid = new("InventoryBusiness.SupplierIdInvalid", "SupplierId must be greater than 0.");
    public static readonly Error SupplierNotFound = new("InventoryBusiness.SupplierNotFound", "Supplier was not found.");
    public static readonly Error SupplierInactiveInvalid = new("InventoryBusiness.SupplierInactiveInvalid", "Supplier is inactive.");
    public static readonly Error PurchaseDateInvalid = new("InventoryBusiness.PurchaseDateInvalid", "PurchaseDate is invalid.");
    public static readonly Error PurchaseDetailsRequired = new("InventoryBusiness.PurchaseDetailsRequired", "At least one purchase detail is required.");
    public static readonly Error DuplicatePartInPurchaseConflict = new("InventoryBusiness.DuplicatePartInPurchaseConflict", "Duplicate parts are not allowed in the same purchase request.");
    public static readonly Error PartIdInvalid = new("InventoryBusiness.PartIdInvalid", "PartId must be greater than 0.");
    public static readonly Error PartNotFound = new("InventoryBusiness.PartNotFound", "Part was not found.");
    public static readonly Error PartInactiveInvalid = new("InventoryBusiness.PartInactiveInvalid", "Part is inactive.");
    public static readonly Error QuantityInvalid = new("InventoryBusiness.QuantityInvalid", "Quantity must be greater than 0.");
    public static readonly Error UnitPriceInvalid = new("InventoryBusiness.UnitPriceInvalid", "UnitPrice must be greater than or equal to 0.");
    public static readonly Error AdjustmentQuantityInvalid = new("InventoryBusiness.AdjustmentQuantityInvalid", "AdjustmentQuantity cannot be 0.");
    public static readonly Error StockWouldBeNegativeInvalid = new("InventoryBusiness.StockWouldBeNegativeInvalid", "Stock cannot become negative.");
    public static readonly Error ChangedByUserIdInvalid = new("InventoryBusiness.ChangedByUserIdInvalid", "ChangedByUserId must be greater than 0.");
    public static readonly Error ChangedByUserNotFound = new("InventoryBusiness.ChangedByUserNotFound", "Changed-by user was not found.");
}
