using Application.Common.Results;

namespace Application.Features.Parts.Errors;

public static class PartErrors
{
    public static readonly Error NotFound = new("Parts.NotFound", "Part was not found.");
    public static readonly Error PartCategoryIdInvalid = new("Parts.PartCategoryIdInvalid", "PartCategoryId must be greater than 0.");
    public static readonly Error PartCategoryNotFound = new("Parts.PartCategoryNotFound", "Part category was not found.");
    public static readonly Error PartBrandIdInvalid = new("Parts.PartBrandIdInvalid", "PartBrandId must be greater than 0 when provided.");
    public static readonly Error PartBrandNotFound = new("Parts.PartBrandNotFound", "Part brand was not found.");
    public static readonly Error CodeRequired = new("Parts.CodeRequired", "Code is required.");
    public static readonly Error CodeTooLong = new("Parts.CodeTooLong", "Code cannot exceed 50 characters.");
    public static readonly Error CodeAlreadyExists = new("Parts.CodeAlreadyExists", "Code already exists.");
    public static readonly Error DescriptionRequired = new("Parts.DescriptionRequired", "Description is required.");
    public static readonly Error DescriptionTooLong = new("Parts.DescriptionTooLong", "Description cannot exceed 255 characters.");
    public static readonly Error StockInvalid = new("Parts.StockInvalid", "Stock must be greater than or equal to 0.");
    public static readonly Error MinimumStockInvalid = new("Parts.MinimumStockInvalid", "MinimumStock must be greater than or equal to 0.");
    public static readonly Error UnitPriceInvalid = new("Parts.UnitPriceInvalid", "UnitPrice must be greater than or equal to 0.");
    public static readonly Error InUse = new("Parts.InUse", "Part is assigned to one or more records.");
}
