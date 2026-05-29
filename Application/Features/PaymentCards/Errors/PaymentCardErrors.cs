using Application.Common.Results;

namespace Application.Features.PaymentCards.Errors;

public static class PaymentCardErrors
{
    public static readonly Error NotFound = new("PaymentCards.NotFound", "Payment card was not found.");
    public static readonly Error PaymentIdInvalid = new("PaymentCards.PaymentIdInvalid", "PaymentId must be greater than 0.");
    public static readonly Error PaymentNotFound = new("PaymentCards.PaymentNotFound", "Payment was not found.");
    public static readonly Error PaymentMethodIsNotCardConflict = new("PaymentCards.PaymentMethodIsNotCardConflict", "Payment method is not card.");
    public static readonly Error PaymentAlreadyHasCardConflict = new("PaymentCards.PaymentAlreadyHasCardConflict", "Payment already has a card assigned.");
    public static readonly Error CardTypeIdInvalid = new("PaymentCards.CardTypeIdInvalid", "CardTypeId must be greater than 0.");
    public static readonly Error CardTypeNotFound = new("PaymentCards.CardTypeNotFound", "Card type was not found.");
    public static readonly Error LastFourDigitsRequired = new("PaymentCards.LastFourDigitsRequired", "LastFourDigits is required.");
    public static readonly Error LastFourDigitsInvalid = new("PaymentCards.LastFourDigitsInvalid", "LastFourDigits must contain exactly 4 digits.");
    public static readonly Error CardHolderRequired = new("PaymentCards.CardHolderRequired", "CardHolder is required.");
    public static readonly Error CardHolderTooLong = new("PaymentCards.CardHolderTooLong", "CardHolder cannot exceed 100 characters.");
    public static readonly Error AuthorizationCodeTooLong = new("PaymentCards.AuthorizationCodeTooLong", "AuthorizationCode cannot exceed 100 characters.");
}
