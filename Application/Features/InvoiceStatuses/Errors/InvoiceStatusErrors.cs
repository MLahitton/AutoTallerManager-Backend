using Application.Common.Results;

namespace Application.Features.InvoiceStatuses.Errors;

public static class InvoiceStatusErrors
{
    public static readonly Error NotFound = new("InvoiceStatuses.NotFound", "Invoice status was not found.");
    public static readonly Error NameRequired = new("InvoiceStatuses.NameRequired", "Invoice status name is required.");
    public static readonly Error NameTooLong = new("InvoiceStatuses.NameTooLong", "Invoice status name cannot exceed 50 characters.");
    public static readonly Error NameAlreadyExists = new("InvoiceStatuses.NameAlreadyExists", "Invoice status name already exists.");
    public static readonly Error InUse = new("InvoiceStatuses.InUse", "Invoice status is assigned to one or more invoices.");
}
