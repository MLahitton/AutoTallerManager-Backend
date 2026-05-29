using Application.Common.Results;

namespace Application.Features.Invoices.Errors;

public static class InvoiceErrors
{
    public static readonly Error NotFound = new("Invoices.NotFound", "Invoice was not found.");
    public static readonly Error InvoiceNumberTooLong = new("Invoices.InvoiceNumberTooLong", "InvoiceNumber cannot exceed 50 characters.");
    public static readonly Error InvoiceNumberAlreadyExists = new("Invoices.InvoiceNumberAlreadyExists", "InvoiceNumber already exists.");
    public static readonly Error ServiceOrderIdInvalid = new("Invoices.ServiceOrderIdInvalid", "ServiceOrderId must be greater than 0.");
    public static readonly Error ServiceOrderNotFound = new("Invoices.ServiceOrderNotFound", "Service order was not found.");
    public static readonly Error ServiceOrderAlreadyHasInvoiceConflict = new("Invoices.ServiceOrderAlreadyHasInvoiceConflict", "Service order already has an invoice.");
    public static readonly Error ServiceOrderCannotBeInvoicedConflict = new("Invoices.ServiceOrderCannotBeInvoicedConflict", "Service order cannot be invoiced because it is cancelled or voided.");
    public static readonly Error InvoiceStatusIdInvalid = new("Invoices.InvoiceStatusIdInvalid", "InvoiceStatusId must be greater than 0.");
    public static readonly Error InvoiceStatusNotFound = new("Invoices.InvoiceStatusNotFound", "Invoice status was not found.");
    public static readonly Error InvoiceDateInvalid = new("Invoices.InvoiceDateInvalid", "InvoiceDate is invalid.");
    public static readonly Error TaxInvalid = new("Invoices.TaxInvalid", "Tax must be greater than or equal to 0.");
    public static readonly Error InUse = new("Invoices.InUse", "Invoice is assigned to one or more records.");
}
