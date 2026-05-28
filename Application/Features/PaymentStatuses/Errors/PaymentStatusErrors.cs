using Application.Common.Results;

namespace Application.Features.PaymentStatuses.Errors;

public static class PaymentStatusErrors
{
    public static readonly Error NotFound = new("PaymentStatuses.NotFound", "Payment status was not found.");
    public static readonly Error NameRequired = new("PaymentStatuses.NameRequired", "Payment status name is required.");
    public static readonly Error NameTooLong = new("PaymentStatuses.NameTooLong", "Payment status name cannot exceed 50 characters.");
    public static readonly Error NameAlreadyExists = new("PaymentStatuses.NameAlreadyExists", "Payment status name already exists.");
    public static readonly Error InUse = new("PaymentStatuses.InUse", "Payment status is assigned to one or more payments.");
}
