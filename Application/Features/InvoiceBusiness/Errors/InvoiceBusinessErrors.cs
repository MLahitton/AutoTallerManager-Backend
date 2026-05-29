using Application.Common.Results;

namespace Application.Features.InvoiceBusiness.Errors;

public static class InvoiceBusinessErrors
{
    public static readonly Error InvoiceIdInvalid = new("InvoiceBusiness.InvoiceIdInvalid", "InvoiceId must be greater than 0.");
    public static readonly Error InvoiceNotFound = new("InvoiceBusiness.InvoiceNotFound", "Invoice was not found.");
    public static readonly Error ServiceOrderIdInvalid = new("InvoiceBusiness.ServiceOrderIdInvalid", "ServiceOrderId must be greater than 0.");
    public static readonly Error ServiceOrderNotFound = new("InvoiceBusiness.ServiceOrderNotFound", "Service order was not found.");
    public static readonly Error ServiceOrderCannotBeInvoicedConflict = new("InvoiceBusiness.ServiceOrderCannotBeInvoicedConflict", "Service order cannot be invoiced.");
    public static readonly Error ServiceOrderAlreadyHasInvoiceConflict = new("InvoiceBusiness.ServiceOrderAlreadyHasInvoiceConflict", "Service order already has an invoice.");
    public static readonly Error InvoiceNumberTooLong = new("InvoiceBusiness.InvoiceNumberTooLong", "Invoice number cannot exceed 50 characters.");
    public static readonly Error InvoiceNumberAlreadyExists = new("InvoiceBusiness.InvoiceNumberAlreadyExists", "Invoice number already exists.");
    public static readonly Error InvoiceStatusIdInvalid = new("InvoiceBusiness.InvoiceStatusIdInvalid", "InvoiceStatusId must be greater than 0.");
    public static readonly Error InvoiceStatusNotFound = new("InvoiceBusiness.InvoiceStatusNotFound", "Invoice status was not found.");
    public static readonly Error DraftStatusNotFound = new("InvoiceBusiness.DraftStatusNotFound", "Draft invoice status was not found.");
    public static readonly Error IssuedStatusNotFound = new("InvoiceBusiness.IssuedStatusNotFound", "Issued invoice status was not found.");
    public static readonly Error CancelledStatusNotFound = new("InvoiceBusiness.CancelledStatusNotFound", "Cancelled invoice status was not found.");
    public static readonly Error TaxInvalid = new("InvoiceBusiness.TaxInvalid", "Tax must be greater than or equal to 0.");
    public static readonly Error NoBillableItemsConflict = new("InvoiceBusiness.NoBillableItemsConflict", "Service order does not have approved billable items.");
    public static readonly Error InvoiceTotalInvalid = new("InvoiceBusiness.InvoiceTotalInvalid", "Invoice total must be greater than 0.");
    public static readonly Error CompletedPaymentsExistConflict = new("InvoiceBusiness.CompletedPaymentsExistConflict", "Invoice has completed payments and cannot be cancelled.");
    public static readonly Error CancelReasonRequired = new("InvoiceBusiness.CancelReasonRequired", "Cancel reason is required.");
}
