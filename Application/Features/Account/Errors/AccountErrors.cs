using Application.Common.Results;

namespace Application.Features.Account.Errors;

public static class AccountErrors
{
    public static readonly Error UserNotFound = new("Account.UserNotFound", "User was not found.");
    public static readonly Error PersonNotFound = new("Account.PersonNotFound", "Person was not found.");
    public static readonly Error UserInactiveInvalid = new("Account.UserInactiveInvalid", "User is inactive.");
    public static readonly Error FirstNameRequired = new("Account.FirstNameRequired", "First name is required.");
    public static readonly Error FirstNameTooLong = new("Account.FirstNameTooLong", "First name cannot exceed 50 characters.");
    public static readonly Error MiddleNameTooLong = new("Account.MiddleNameTooLong", "Middle name cannot exceed 50 characters.");
    public static readonly Error LastNameRequired = new("Account.LastNameRequired", "Last name is required.");
    public static readonly Error LastNameTooLong = new("Account.LastNameTooLong", "Last name cannot exceed 50 characters.");
    public static readonly Error SecondLastNameTooLong = new("Account.SecondLastNameTooLong", "Second last name cannot exceed 50 characters.");
    public static readonly Error BirthDateInvalid = new("Account.BirthDateInvalid", "Birth date cannot be in the future.");
    public static readonly Error GenderIdInvalid = new("Account.GenderIdInvalid", "GenderId must be greater than 0 when provided.");
    public static readonly Error GenderNotFound = new("Account.GenderNotFound", "Gender was not found.");
    public static readonly Error AddressIdInvalid = new("Account.AddressIdInvalid", "AddressId must be greater than 0 when provided.");
    public static readonly Error AddressNotFound = new("Account.AddressNotFound", "Address was not found.");
    public static readonly Error EmailInvalid = new("Account.EmailInvalid", "Email format is invalid.");
    public static readonly Error EmailAlreadyExists = new("Account.EmailAlreadyExists", "Email already exists.");
    public static readonly Error PhoneCountryIdRequired = new("Account.PhoneCountryIdRequired", "PhoneCountryId is required when phone number is provided.");
    public static readonly Error PhoneCountryNotFound = new("Account.PhoneCountryNotFound", "Phone country was not found.");
    public static readonly Error PhoneNumberInvalid = new("Account.PhoneNumberInvalid", "Phone number format is invalid.");
    public static readonly Error PhoneNumberTooLong = new("Account.PhoneNumberTooLong", "Phone number cannot exceed 20 characters.");
    public static readonly Error PhoneNumberAlreadyExists = new("Account.PhoneNumberAlreadyExists", "Phone number already exists.");
    public static readonly Error CurrentPasswordRequired = new("Account.CurrentPasswordRequired", "Current password is required.");
    public static readonly Error NewPasswordRequired = new("Account.NewPasswordRequired", "New password is required.");
    public static readonly Error NewPasswordTooShort = new("Account.NewPasswordTooShort", "New password must be at least 8 characters long.");
    public static readonly Error NewPasswordTooLong = new("Account.NewPasswordTooLong", "New password cannot exceed 100 characters.");
    public static readonly Error CurrentPasswordInvalid = new("Account.CurrentPasswordInvalid", "Current password is invalid.");
}
