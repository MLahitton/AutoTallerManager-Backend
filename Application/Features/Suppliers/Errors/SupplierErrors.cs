using Application.Common.Results;

namespace Application.Features.Suppliers.Errors;

public static class SupplierErrors
{
    public static readonly Error NotFound = new("Suppliers.NotFound", "Supplier was not found.");
    public static readonly Error NameRequired = new("Suppliers.NameRequired", "Name is required.");
    public static readonly Error NameTooLong = new("Suppliers.NameTooLong", "Name cannot exceed 120 characters.");
    public static readonly Error TaxIdTooLong = new("Suppliers.TaxIdTooLong", "TaxId cannot exceed 30 characters.");
    public static readonly Error TaxIdAlreadyExists = new("Suppliers.TaxIdAlreadyExists", "TaxId already exists.");
    public static readonly Error PhoneTooLong = new("Suppliers.PhoneTooLong", "Phone cannot exceed 30 characters.");
    public static readonly Error EmailTooLong = new("Suppliers.EmailTooLong", "Email cannot exceed 120 characters.");
    public static readonly Error EmailInvalid = new("Suppliers.EmailInvalid", "Email format is invalid.");
    public static readonly Error InUse = new("Suppliers.InUse", "Supplier is assigned to one or more records.");
}
