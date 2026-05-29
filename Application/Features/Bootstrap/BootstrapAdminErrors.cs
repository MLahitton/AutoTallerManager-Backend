using Application.Common.Results;

namespace Application.Features.Bootstrap;

public static class BootstrapAdminErrors
{
    public static readonly Error Disabled = new("BootstrapAdmin.Disabled", "Bootstrap admin is disabled.");
    public static readonly Error AdminRoleNotFound = new("BootstrapAdmin.AdminRoleNotFound", "Admin role was not found.");
    public static readonly Error EmailRequired = new("BootstrapAdmin.EmailRequired", "Email is required.");
    public static readonly Error EmailInvalid = new("BootstrapAdmin.EmailInvalid", "Email format is invalid.");
    public static readonly Error PasswordRequired = new("BootstrapAdmin.PasswordRequired", "Password is required.");
    public static readonly Error PasswordTooShort = new("BootstrapAdmin.PasswordTooShort", "Password must be at least 8 characters long.");
    public static readonly Error DocumentTypeIdInvalid = new("BootstrapAdmin.DocumentTypeIdInvalid", "DocumentTypeId must be greater than 0.");
    public static readonly Error DocumentTypeNotFound = new("BootstrapAdmin.DocumentTypeNotFound", "Document type was not found.");
    public static readonly Error DocumentNumberRequired = new("BootstrapAdmin.DocumentNumberRequired", "Document number is required.");
    public static readonly Error DocumentNumberAlreadyExists = new("BootstrapAdmin.DocumentNumberAlreadyExists", "Document number already exists.");
    public static readonly Error FirstNameRequired = new("BootstrapAdmin.FirstNameRequired", "First name is required.");
    public static readonly Error LastNameRequired = new("BootstrapAdmin.LastNameRequired", "Last name is required.");
    public static readonly Error GenderNotFound = new("BootstrapAdmin.GenderNotFound", "Gender was not found.");
    public static readonly Error AddressNotFound = new("BootstrapAdmin.AddressNotFound", "Address was not found.");
    public static readonly Error EmailAlreadyExists = new("BootstrapAdmin.EmailAlreadyExists", "Email already exists.");
}
