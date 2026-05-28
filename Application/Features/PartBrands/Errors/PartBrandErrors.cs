using Application.Common.Results;

namespace Application.Features.PartBrands.Errors;

public static class PartBrandErrors
{
    public static readonly Error NotFound = new("PartBrands.NotFound", "Part brand was not found.");
    public static readonly Error NameRequired = new("PartBrands.NameRequired", "Part brand name is required.");
    public static readonly Error NameTooLong = new("PartBrands.NameTooLong", "Part brand name cannot exceed 80 characters.");
    public static readonly Error NameAlreadyExists = new("PartBrands.NameAlreadyExists", "Part brand name already exists.");
    public static readonly Error InUse = new("PartBrands.InUse", "Part brand is assigned to one or more parts.");
}
