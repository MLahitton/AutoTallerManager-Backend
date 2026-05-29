using Application.Common.Results;

namespace Application.Features.Payments.Errors;

public static class PaymentErrors
{
    public static readonly Error NotFound = new("Payments.NotFound", "Payment was not found.");
    public static readonly Error InvoiceIdInvalid = new("Payments.InvoiceIdInvalid", "InvoiceId must be greater than 0.");
    public static readonly Error InvoiceNotFound = new("Payments.InvoiceNotFound", "Invoice was not found.");
    public static readonly Error InvoiceTotalInvalid = new("Payments.InvoiceTotalInvalid", "Invoice total must be greater than 0.");
    public static readonly Error PaymentMethodIdInvalid = new("Payments.PaymentMethodIdInvalid", "PaymentMethodId must be greater than 0.");
    public static readonly Error PaymentMethodNotFound = new("Payments.PaymentMethodNotFound", "Payment method was not found.");
    public static readonly Error PaymentStatusIdInvalid = new("Payments.PaymentStatusIdInvalid", "PaymentStatusId must be greater than 0.");
    public static readonly Error PaymentStatusNotFound = new("Payments.PaymentStatusNotFound", "Payment status was not found.");
    public static readonly Error PaymentDateInvalid = new("Payments.PaymentDateInvalid", "PaymentDate is invalid.");
    public static readonly Error AmountInvalid = new("Payments.AmountInvalid", "Amount must be greater than 0.");
    public static readonly Error ReferenceTooLong = new("Payments.ReferenceTooLong", "Reference cannot exceed 100 characters.");
    public static readonly Error PaymentMethodCannotChangeBecauseCardExistsConflict = new("Payments.PaymentMethodCannotChangeBecauseCardExistsConflict", "Payment method cannot be changed because payment has card details.");
    public static readonly Error CompletedPaymentsExceedInvoiceTotalConflict = new("Payments.CompletedPaymentsExceedInvoiceTotalConflict", "Completed payments exceed invoice total.");
    public static readonly Error InUse = new("Payments.InUse", "Payment is assigned to one or more records.");
}
