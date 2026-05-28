using Application.Common.Results;

namespace Application.Features.PartCategories.Errors;

public static class PartCategoryErrors
{
    public static readonly Error NotFound = new("PartCategories.NotFound", "Part category was not found.");
    public static readonly Error NameRequired = new("PartCategories.NameRequired", "Part category name is required.");
    public static readonly Error NameTooLong = new("PartCategories.NameTooLong", "Part category name cannot exceed 100 characters.");
    public static readonly Error NameAlreadyExists = new("PartCategories.NameAlreadyExists", "Part category name already exists.");
    public static readonly Error InUse = new("PartCategories.InUse", "Part category is assigned to one or more parts.");
}
