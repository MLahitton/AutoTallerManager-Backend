using Application.Common.Results;

namespace Application.Features.PaymentMethods.Errors;

public static class PaymentMethodErrors
{
    public static readonly Error NotFound = new("PaymentMethods.NotFound", "Payment method was not found.");
    public static readonly Error NameRequired = new("PaymentMethods.NameRequired", "Payment method name is required.");
    public static readonly Error NameTooLong = new("PaymentMethods.NameTooLong", "Payment method name cannot exceed 50 characters.");
    public static readonly Error NameAlreadyExists = new("PaymentMethods.NameAlreadyExists", "Payment method name already exists.");
    public static readonly Error InUse = new("PaymentMethods.InUse", "Payment method is assigned to one or more payments.");
}
