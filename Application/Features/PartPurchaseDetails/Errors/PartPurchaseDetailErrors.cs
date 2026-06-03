using Application.Common.Results;

namespace Application.Features.PartPurchaseDetails.Errors;

public static class PartPurchaseDetailErrors
{
    public static readonly Error NotFound = new("PartPurchaseDetails.NotFound", "Part purchase detail was not found.");
    public static readonly Error PartPurchaseIdInvalid = new("PartPurchaseDetails.PartPurchaseIdInvalid", "PartPurchaseId must be greater than 0.");
    public static readonly Error PartPurchaseNotFound = new("PartPurchaseDetails.PartPurchaseNotFound", "Part purchase was not found.");
    public static readonly Error PartIdInvalid = new("PartPurchaseDetails.PartIdInvalid", "PartId must be greater than 0.");
    public static readonly Error PartNotFound = new("PartPurchaseDetails.PartNotFound", "Part was not found.");
    public static readonly Error PartInactive = new("PartPurchaseDetails.PartInactive", "Part is inactive.");
    public static readonly Error QuantityInvalid = new("PartPurchaseDetails.QuantityInvalid", "Quantity must be greater than 0.");
    public static readonly Error UnitPriceInvalid = new("PartPurchaseDetails.UnitPriceInvalid", "UnitPrice must be greater than or equal to 0.");
    public static readonly Error DuplicatePartForPurchaseConflict = new("PartPurchaseDetails.DuplicatePartForPurchaseConflict", "Part already exists for this purchase.");
    public static readonly Error StockWouldBeNegativeInvalid = new("PartPurchaseDetails.StockWouldBeNegativeInvalid", "Stock cannot become negative.");
    public static readonly Error CannotAddDetailToCancelledPurchaseConflict = new("PartPurchaseDetails.CannotAddDetailToCancelledPurchaseConflict", "Details cannot be added to a cancelled purchase.");
    public static readonly Error CannotModifyDetailFromCancelledPurchaseConflict = new("PartPurchaseDetails.CannotModifyDetailFromCancelledPurchaseConflict", "Details from a cancelled purchase cannot be modified.");
    public static readonly Error CannotMoveDetailToCancelledPurchaseConflict = new("PartPurchaseDetails.CannotMoveDetailToCancelledPurchaseConflict", "Details cannot be moved to a cancelled purchase.");
    public static readonly Error CannotDeleteDetailFromCancelledPurchaseConflict = new("PartPurchaseDetails.CannotDeleteDetailFromCancelledPurchaseConflict", "Details from a cancelled purchase cannot be deleted.");
}
