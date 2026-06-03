using Application.Common.Results;

namespace Application.Features.PartPurchases.Errors;

public static class PartPurchaseErrors
{
    public static readonly Error NotFound = new("PartPurchases.NotFound", "Part purchase was not found.");
    public static readonly Error SupplierIdInvalid = new("PartPurchases.SupplierIdInvalid", "SupplierId must be greater than 0.");
    public static readonly Error SupplierNotFound = new("PartPurchases.SupplierNotFound", "Supplier was not found.");
    public static readonly Error SupplierInactive = new("PartPurchases.SupplierInactive", "Supplier is inactive.");
    public static readonly Error PurchaseDateInvalid = new("PartPurchases.PurchaseDateInvalid", "PurchaseDate is invalid.");
    public static readonly Error InUse = new("PartPurchases.InUse", "Part purchase is assigned to one or more records.");
    public static readonly Error CannotModifyCancelledPurchaseConflict = new("PartPurchases.CannotModifyCancelledPurchaseConflict", "Cancelled purchases cannot be modified.");
    public static readonly Error CannotDeleteCancelledPurchaseConflict = new("PartPurchases.CannotDeleteCancelledPurchaseConflict", "Cancelled purchases cannot be deleted.");
}
