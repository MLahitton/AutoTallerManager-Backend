using Application.Common.Results;

namespace Application.Features.PaymentBusiness.Errors;

public static class PaymentBusinessErrors
{
    public static readonly Error InvoiceIdInvalid = new("PaymentBusiness.InvoiceIdInvalid", "InvoiceId must be greater than 0.");
    public static readonly Error InvoiceNotFound = new("PaymentBusiness.InvoiceNotFound", "Invoice was not found.");
    public static readonly Error InvoiceTotalInvalid = new("PaymentBusiness.InvoiceTotalInvalid", "Invoice total must be greater than 0.");
    public static readonly Error PaymentMethodIdInvalid = new("PaymentBusiness.PaymentMethodIdInvalid", "PaymentMethodId must be greater than 0.");
    public static readonly Error PaymentMethodNotFound = new("PaymentBusiness.PaymentMethodNotFound", "Payment method was not found.");
    public static readonly Error PaymentStatusIdInvalid = new("PaymentBusiness.PaymentStatusIdInvalid", "PaymentStatusId must be greater than 0.");
    public static readonly Error PaymentStatusNotFound = new("PaymentBusiness.PaymentStatusNotFound", "Payment status was not found.");
    public static readonly Error CompletedStatusNotFound = new("PaymentBusiness.CompletedStatusNotFound", "Completed payment status was not found.");
    public static readonly Error RefundedStatusNotFound = new("PaymentBusiness.RefundedStatusNotFound", "Refunded payment status was not found.");
    public static readonly Error PaymentDateInvalid = new("PaymentBusiness.PaymentDateInvalid", "PaymentDate is invalid.");
    public static readonly Error AmountInvalid = new("PaymentBusiness.AmountInvalid", "Amount must be greater than 0.");
    public static readonly Error ReferenceTooLong = new("PaymentBusiness.ReferenceTooLong", "Reference cannot exceed 100 characters.");
    public static readonly Error CompletedPaymentsExceedInvoiceTotalConflict = new("PaymentBusiness.CompletedPaymentsExceedInvoiceTotalConflict", "Completed payments exceed invoice total.");
    public static readonly Error CardDetailsRequired = new("PaymentBusiness.CardDetailsRequired", "Card details are required for card payments.");
    public static readonly Error CardDetailsNotAllowedInvalid = new("PaymentBusiness.CardDetailsNotAllowedInvalid", "Card details are only allowed for card payments.");
    public static readonly Error CardTypeIdInvalid = new("PaymentBusiness.CardTypeIdInvalid", "CardTypeId must be greater than 0.");
    public static readonly Error CardTypeNotFound = new("PaymentBusiness.CardTypeNotFound", "Card type was not found.");
    public static readonly Error LastFourDigitsRequired = new("PaymentBusiness.LastFourDigitsRequired", "LastFourDigits is required.");
    public static readonly Error LastFourDigitsInvalid = new("PaymentBusiness.LastFourDigitsInvalid", "LastFourDigits must contain exactly 4 digits.");
    public static readonly Error CardHolderRequired = new("PaymentBusiness.CardHolderRequired", "CardHolder is required.");
    public static readonly Error CardHolderTooLong = new("PaymentBusiness.CardHolderTooLong", "CardHolder cannot exceed 100 characters.");
    public static readonly Error AuthorizationCodeTooLong = new("PaymentBusiness.AuthorizationCodeTooLong", "AuthorizationCode cannot exceed 100 characters.");
    public static readonly Error PaymentIdInvalid = new("PaymentBusiness.PaymentIdInvalid", "PaymentId must be greater than 0.");
    public static readonly Error PaymentNotFound = new("PaymentBusiness.PaymentNotFound", "Payment was not found.");
    public static readonly Error ClientCannotAccessInvoiceConflict = new("PaymentBusiness.ClientCannotAccessInvoiceConflict", "Client cannot access this invoice.");
}
