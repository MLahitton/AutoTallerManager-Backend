using Application.Common.Results;

namespace Application.Features.parte_nueva.Errors;

public static class parte_nuevaErrors
{
    public static readonly Error IdInvalid = new("parte_nueva.IdInvalid", "parte_nueva id must be greater than 0.");
    public static readonly Error NotFound = new("parte_nueva.NotFound", "parte_nueva was not found.");
    public static readonly Error NameRequired = new("parte_nueva.NameRequired", "parte_nueva name is required.");
    public static readonly Error NameTooLong = new("parte_nueva.NameTooLong", "parte_nueva name cannot exceed 100 characters.");
    public static readonly Error DescriptionTooLong = new("parte_nueva.DescriptionTooLong", "parte_nueva description cannot exceed 500 characters.");
    public static readonly Error NameAlreadyExists = new("parte_nueva.NameAlreadyExists", "parte_nueva name already exists.");
}
